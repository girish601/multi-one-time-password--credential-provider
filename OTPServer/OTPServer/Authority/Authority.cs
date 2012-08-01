﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OTPServer.Communication.Local;
using OTPHelpers.XML.OTPPacket;
using OTPServer.Communication.Local.Observer;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;

namespace OTPServer.Authority
{
    class Authority : Observer
    {
        private const int MAX_PROCESS_AGE = 60; // seconds

        private static volatile Storage.ProcessDataStorage __ProcessDataStorage = null;
        private static volatile RequestQueue<OTPPacket, AuthorityResponseObject> __Requests = null;

        private static X509Certificate2 __ServerCertificate = null;
        private static volatile AutoResetEvent __Waiting = new AutoResetEvent(false);

        private static Thread __ProcessRequestsThread = null;
        private static Thread __MaintainProcessDataStorageThread = null;

        private static Authority __Instance = null;
        public static Authority Instance
        {
            get
            {
                if (__Instance == null)
                    __Instance = new Authority();
                return __Instance;
            }
        }

        private Authority()
        {
            if (__Requests == null)
                __Requests = new RequestQueue<OTPPacket, AuthorityResponseObject>();

            if (__ProcessDataStorage == null)
                __ProcessDataStorage = new Storage.ProcessDataStorage();

            __Requests.Attach(this);
        }

        ~Authority()
        {
            __Active = false;

            __Requests.Detach(this);
            __Requests = null;

            __MaintainProcessDataStorageThread = null;
            __ProcessRequestsThread = null;
            __ProcessDataStorage = null;
            __ServerCertificate = null;
        }

        private static bool __Active;
        public static bool Active
        {
            get { return __Active; }
        }

        public bool Start()
        {
            __Active = true;

            if (__ProcessRequestsThread == null)
            {
                __ProcessRequestsThread = new Thread(ProcessRequests);
                __ProcessRequestsThread.Start();
            }

            if (__MaintainProcessDataStorageThread == null)
            {
                __MaintainProcessDataStorageThread = new Thread(MaintainProcessDataStorage);
                __MaintainProcessDataStorageThread.Start();
            }

            return true;
        }

        public bool Stop()
        {
            __Active = false;

            if (__ProcessRequestsThread != null)
            {
                __ProcessRequestsThread.Abort();
                __ProcessRequestsThread.Join();
            }

            if (__MaintainProcessDataStorageThread != null)
            {
                __MaintainProcessDataStorageThread.Abort();
                __MaintainProcessDataStorageThread.Join();
            }

            return true;
        }

        private void MaintainProcessDataStorage()
        {
            while (Active)
            {
                try
                {
                    Thread.Sleep(5000);

                    Storage.ProcessDataStorage.ProcessAge oldestProcess = __ProcessDataStorage.GetOldestProcess();
                    while (oldestProcess != null && (Storage.ProcessDataStorage.ProcessAge.Now() - oldestProcess.ProcessStartedAt) > MAX_PROCESS_AGE)
                    {
                        Storage.ProcessDataStorage.ProcessData process = __ProcessDataStorage.GetProcess(oldestProcess.ProcessIdentifier);
                        __ProcessDataStorage.RemoveProcess(process);

                        oldestProcess = __ProcessDataStorage.GetOldestProcess();
                    }
                }
                catch (ThreadAbortException)
                {
                    break;
                }
                catch (ThreadInterruptedException)
                {
                    break;
                }
                catch (Exception)
                {
                    // TODO: Log it
                }
            }
        }

        private void ProcessRequests()
        {
            while (Active)
            {
                try
                {
                    if (__Requests.Empty())
                    {
                        __Waiting.WaitOne();
                    }

                    RequestObject<OTPPacket, AuthorityResponseObject> reqObj = __Requests.Dequeue();

                    // TODO: Check ProcessAge when existing (for life time check).
                    if (reqObj.Request.Message.Type == Message.TYPE.HELLO)
                    {
                        reqObj.Request.ProcessIdentifier.ID = ProcessIdentifier.NONE; // Clients can not choose a PID
                        int pid = __ProcessDataStorage.CreateProcess(reqObj.Request);
                        AnswerSuccess(ref reqObj, pid, Message.STATUS.S_OK);
                    }
                    else if (reqObj.Request.Message.Type == Message.TYPE.ERROR || reqObj.Request.Message.Type == Message.TYPE.SUCCESS)
                    {
                        // Server should filter out ERROR and SUCCESS messages from client to avoid filling up the RequestQueue.
                        // But just in case anyone reaches so far: DO NOTHING
                    }
                    else
                    {
                        // These requests need to be authorized, except an ADD containing KeyData only.
                        bool reqAuthorized = false;
                        Storage.ProcessDataStorage.ProcessData process = __ProcessDataStorage.GetProcess(reqObj.Request);

                        if (process != null
                         && process.KeyData.Type != KeyData.TYPE.NONE)
                            reqAuthorized = CheckMessageAuthenticationCode(process.KeyData, reqObj.Request);
                        else if (process != null
                              && reqObj.Request.Message.Type == Message.TYPE.ADD
                              && reqObj.Request.KeyData.Type != KeyData.TYPE.NONE
                              && reqObj.Request.DataItems.Count == 0)
                            reqAuthorized = true;

                        if (!reqAuthorized)
                        {
                            if (process == null)
                            {
                                AnswerNotFound(ref reqObj);
                                goto NotifyAndContinue;
                            }

                            if (process.KeyData.Type == KeyData.TYPE.NONE)
                                AnswerIncomplete(ref reqObj);
                            else
                                AnswerNotAuthorized(ref reqObj);

                            goto NotifyAndContinue;
                        }

                        if (reqObj.Request.Message.Type == Message.TYPE.ADD)
                        {
                            if (__ProcessDataStorage.AddDataFromPacketToProcess(reqObj.Request))
                            {
                                AnswerSuccess(ref reqObj, reqObj.Request.ProcessIdentifier.ID, Message.STATUS.S_OK);
                            }
                            else
                            {
                                AnswerFailure(ref reqObj, reqObj.Request.ProcessIdentifier.ID, Message.STATUS.E_LOCKED, "Could not add data. Maybe duplicate data was sent.");
                            }
                        }
                        else if (reqObj.Request.Message.Type == Message.TYPE.VERIFY)
                        {
                            // TODO: add (optional) data to process and verify (through Agent). send verify result.
                        }
                        else if (reqObj.Request.Message.Type == Message.TYPE.RESYNC)
                        {
                            // TODO: add (optional) data to process and resync (through Agent). send resync result.
                        }
                        else
                        {
                            // This shouldn't happen.
                            AnswerFailure(ref reqObj, reqObj.Request.ProcessIdentifier.ID, Message.STATUS.E_MALFORMED, "Malformed message. Dont know what to do.");
                        }
                    }

                NotifyAndContinue:
                    reqObj.Notify();
                }
                catch (ThreadAbortException)
                {
                    break;
                }
                catch (ThreadInterruptedException)
                {
                    break;
                }
                catch (Exception)
                {
                    // TODO: Log it
                }
            }
        }

        private void AnswerNotFound(ref RequestObject<OTPPacket, AuthorityResponseObject> reqObj)
        {
            AnswerFailure(ref reqObj, reqObj.Request.ProcessIdentifier.ID, Message.STATUS.E_NOT_FOUND, "PID or user not found.");
        }

        private void AnswerIncomplete(ref RequestObject<OTPPacket, AuthorityResponseObject> reqObj)
        {
            AnswerFailure(ref reqObj, reqObj.Request.ProcessIdentifier.ID, Message.STATUS.E_INCOMPLETE, "Request incomplete. Maybe no public key was provided prior to this request.");
        }

        private void AnswerNotAuthorized(ref RequestObject<OTPPacket, AuthorityResponseObject> reqObj)
        {
            AnswerFailure(ref reqObj, reqObj.Request.ProcessIdentifier.ID, Message.STATUS.E_NOT_AUTHED, "Request not authorized. MAC is wrong.");
        }

        private bool CheckMessageAuthenticationCode(KeyData keyData, OTPPacket otpPacket)
        {
            Storage.ProcessDataStorage.ProcessData process = __ProcessDataStorage.GetProcess(otpPacket);

            if (process == null)
                return false;

            using (RSACryptoServiceProvider pubKey = new RSACryptoServiceProvider())
            {
                try
                {
                    pubKey.FromXmlString(keyData.GetXmlForRSACryptoServiveProvider());

                    bool verifiedSignature =
                        pubKey.VerifyData(
                            Encoding.UTF8.GetBytes(otpPacket.ProcessIdentifier.ID.ToString() + otpPacket.Message.TimeStamp.ToString()),
                            new MD5CryptoServiceProvider(),
                            otpPacket.Message.MAC
                        );

                    if (!verifiedSignature)
                        return false;

                    if (otpPacket.Message.TimeStamp <= process.LastAuthedTimestamp || otpPacket.Message.TimeStamp > Storage.ProcessDataStorage.ProcessAge.NowMilli())
                        return false;

                    process.LastAuthedTimestamp = otpPacket.Message.TimeStamp;
                    return true;
                }
                catch (Exception)
                {
                    // TODO: Log it
                    return false;
                }
            }
        }

        private void AnswerSuccess(ref RequestObject<OTPPacket, AuthorityResponseObject> reqObj, int pid, Message.STATUS statusCode)
        {
            Answer(ref reqObj, true, pid, statusCode, String.Empty);
        }

        private void AnswerFailure(ref RequestObject<OTPPacket, AuthorityResponseObject> reqObj, int pid, Message.STATUS statusCode, string textMessage)
        {
            Answer(ref reqObj, false, pid, statusCode, textMessage);
        }

        private void Answer(ref RequestObject<OTPPacket, AuthorityResponseObject> reqObj, bool simpleResponse, int pid, Message.STATUS statusCode, string textMessage)
        {
            reqObj.Response.SimpleResponse = simpleResponse;
            reqObj.Response.ComplexResponse.ProcessIdentifier = pid;
            reqObj.Response.ComplexResponse.StatusCode = statusCode;
            reqObj.Response.ComplexResponse.TextMessage = textMessage;
        }

        public static RequestObject<OTPPacket, AuthorityResponseObject> Request(Observer observer, OTPPacket otpPacket)
        {
            AuthorityResponseObject repObj = new AuthorityResponseObject();
            RequestObject<OTPPacket, AuthorityResponseObject> reqObj = null;
            lock (__Requests)
                reqObj = __Requests.EnqueueAndObserve(observer, otpPacket, repObj);

            return reqObj;
        }

        public static X509Certificate GetServerCertificate()
        {
            if (__ServerCertificate == null) // TODO: Get certificate data from config
            {
                string serialNumber = Configuration.Instance.GetStringValue("serverCertificate");
                if (serialNumber != String.Empty)
                {
                    try
                    {
                        X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                        store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
                        X509Certificate2Collection collection = store.Certificates.Find(X509FindType.FindByTimeValid, DateTime.Now, false);
                        collection = collection.Find(X509FindType.FindBySerialNumber, serialNumber, false);
                        if (collection.Count == 1)
                        {
                            __ServerCertificate = collection[0];
                        }
                        collection.Clear();
                        store.Close();
                    }
                    catch (Exception)
                    { }
                }
            }
            return __ServerCertificate;
        }

        public override void Update()
        {
            __Waiting.Set();
        }
    }
}
