# Introduction #

To deploy the mOTP-CP to multiple systems using e.g. the Windows software deployment features it is neccessary to install mOTP-CP in an unattended way.


# Install mOTP-CP unattended #

The MSI installer package provides switches to predefine the values to be used during installation:
  * PATH\_TO\_MULTIOTP defines the path multiOTP.exe is installed to
  * TIMEOUT defines a maximum timout that is used while communicating with multiOTP.exe

You can deploy the software along side with, e.g. a Batch file including the following:
```
   msiexec /qb /i MultiOTPSetup.msi PATH_TO_MULTIOTP=C:\path\to\multiotp TIMEOUT=10 ADDLOCAL=[MainInstall | InstallAsDefault | VCRedist]
```

This will install mOTP-CP, configure the multiOTP.exe's path to C:\path\to\multiotp and the timout to be 10 seconds. There will almost no user interface be shown (/qb).

The ADDLOCAL-switch tells the installer wich components to include. Adding "InstallAsDefault" configures the CP to be installed as default provider.

# Further information #

For more information concerning the command-line switches of msiexec see [this MS TechNet article](http://technet.microsoft.com/es-es/library/bb490936(en-us).aspx).