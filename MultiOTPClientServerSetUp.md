# Introduction #

multiOTP.exe (which mOTP-CP depends on) is fully client-server configurable. You may set-up OTP-servers in your network and use multiOTP.exe to synchronize with them.


# Install multiOTP.exe on the server side #

To install multiOTP.exe on the server, extract the multiOTP archive and do the following:
  1. Install the multiOTP web service on the server side. If you are using the unmodified included installer to install it under Windows, the URL for the multiOTP web service will be http://ip.address.of.server:8112
  1. Set the shared secret key you will use to encode the data between the server and the client: multiotp -config server-secret=MySharedSecret
  1. If you want to allow the client to cache the data on its side, set the options accordingly (enable the cache and define the lifetime of the cache): multiotp -config server-cache-level=1 server-cache-lifetime=15552000

# Install multiOTP.exe on the client side #

To install multiOTP.exe on the client, extract the multiOTP archive and do the following:
  1. Set the shared secret key you will use to encode the data between the client and the server: multiotp -config server-secret=MySharedSecret
  1. If you want to have cache support if allowed by the multiOTP web service, set the option accordingly: multiotp -config server-cache-level=1
  1. Define the timeout after which you will switch to the next server(s) and finally locally: multiotp -config server-timeout=3
  1. Last but not least, define the server(s) you want to connect with: multiotp -config server-url=http://ip.address.of.server:8112;http://url2;http://url3
  1. Done :)

# Further instructions #

For further instructions see the multiOTP.exe README, that is included with the multiOTP.exe download archive.