
---


Hi, this is Dominik from Last Squirrel IT. Here I'm providing my Credential Provider (CP) for Microsoft Windows-based systems.

<font color='darkred'><b>We are moving our repositories to GitHub. See this project here: <a href='https://github.com/LastSquirrelIT/MultiOneTimePassword-CredentialProvider'>https://github.com/LastSquirrelIT/MultiOneTimePassword-CredentialProvider</a></b>

<b>!!! Downloads moved to GitHub !!!</b>

<b>Please use the issue tracker on GitHub now.</b>
</font>

# MultiOneTimePassword Credential Provider  #

The _MultiOneTimePassword Credential Provider_ (mOTP-CP) aims to improve the overall security of the Windows logon process by adding an authenticator. The additional authenticator is "Something That You Have" and consists of the validation of an one-time password (OTP).

The CP wraps the built-in _Password Credential Provider_, so that Windows' default authentication ("Something That You Know", username and password) and improved authentication ("Something That You Have", one-time password, token, etc.) are fully indepent from each other, but both may be required to authenticate a user.

You can use **software or hardware tokens** like **mobileOTP** and **Google Authenticator** for Windows logon, but can also **receive one-time passwords over the air by SMS** or have a **scratch passwords list** for offline authentication.

## > Screenshots: ##

The [screenshots](http://tinyurl.com/mOTP-Screenshots) are hosted on Google Drive.

## > Support this Software and Donate: ##
mOTP-CP will stay free for private use. This project takes a measurable amount of my spare time - including support and help. You like this piece of software? I appreciate if you support the development with buying me a beer - or two. Further a donation will make you feel better - for sure :)

Donate via **PayPal.**

&lt;wiki:gadget url="http://www.lastsquirrel.com/gadget/google-paypal2.xml" height="40" border="0" /&gt;

Or send a micro donation via **Flattr.**

&lt;wiki:gadget url="http://www.lastsquirrel.com/gadget/google-flattr.xml" height="30" border="0" /&gt;

**Thank you! You are awesome! :)**


---


## >> MultiOneTimePassword CP ##

_mOTP-CP_ requires _multiOTP.exe_ to be available on the target machine. You can download _multiOTP.exe_ at http://www.multiotp.net .

You have to add user accounts to _multiOTP.exe_, before installing _mOTP-CP_. See [MultiOTP](MultiOTP.md) in the project Wiki-pages for a descriptive How-To on installing, configuring and testing _multiOTP.exe_.

**_multiOTP.exe_ features:**
  * Full client/server support
  * Emergency scratch passwords list
  * SMS code sending
  * Automatic caching of token definitions used on the machine
  * One-click installation in about 10 seconds
  * Many many more...

**Requirements:**
  * mobileOTP, Google Authenticator or the like (OATH/HOTP, OATH/TOTP or mobileOTP compatible token generator)
  * Windows Vista/7/2008/8/2012 both 32 and 64 bits
  * Administrator privileges
  * _multiOTP.exe_ (http://www.multiotp.net) (read our Wiki for [MultiOTP](MultiOTP.md))
  * Configured user account(s) in _multiOTP.exe_ (use your Windows account name!)


---


## >> Client-Server Operation ##

This functionality including a cache feature (for laptops) is available in cooperation with SysCo and included into _multiOTP.exe_. [The new release](http://www.multiotp.net/website/index.php?language=en) also includes SMS-authentication, QRcode generation and scratch passwords.

Just download the latest version of mOTP-CP and configure _multiOTP.exe_ for network access and synchronization. See [this wiki-page](MultiOTPClientServerSetUp.md) for client-server installation instructions.


---


## >> General information: ##
### Testing your installation: ###
To verify that _mOTP-CP_ works for you, you should deselect to install the software as default provider during installation. If it works, just re-install _mOTP-CP_ with the option checked. If it fails, feel free to contact me or file an issue. I'm not perfect and thus I can't find every bug :)

### Account locking / OTPs out of sync: ###

_mOTP-CP_ will add a virtual user account to your system called _"Resync OTP"_.
In case that you can not logon to your account using OTPs, they may be out of sync. You may resynchronize them before logging in again.

By default multiotp.exe from SysCo locks accounts after six failed authentication requests. You may resychronize your OTPs and logon again.

### Error: You need to be Administrator! ###
Whenever you encounter this error, you are possibly on a Windows Server or Enterprise system. It depends on the way how Windows elevates privileges during an MSI install. It seems that this behaviour is more strict on Server and Enterprise systems.

For the time being just **run the setup and (re-)configuration from an elevated command prompt**.

### Windows Safe Mode ###

By default, Windows _does not load_ custom credential providers (like _mOTP-CP_) in safe mode.
If you really want to know how to enable _mOTP_-CP in safe mode, reading our UseInSafeMode wiki page may help you.

### Unattended/Mass deployment of mOTP-CP ###

To mass deploy mOTP-CP [see this wiki-page](UnattendedInstallation.md) for the setup-file parameters.


---


## >> Project information: ##
### Issues and Bugs: ###
  * If there are bugs or issues, please mind [filing a bug report](http://code.google.com/p/multi-one-time-password--credential-provider/issues/list) using our issue tracker

### Contribute: ###
  * You may use this repository as a base for your development or contribute your knowledge and ideas to this project. I'm looking forward to your response ;)
  * If you have problems while checking out the source, see [Issue #4](http://code.google.com/p/multi-one-time-password--credential-provider/issues/detail?id=4&can=1)


---


## >> Other projects ##
See my other Credential Provider: http://code.google.com/p/open-one-time-password--credential-provider/.

It uses RCDevs OpenOTP Authentication Server. The CP is able to use tokens generated by a token device (e.g. _YubiKey_) or software token (e.g. _Google Authenticator_, _Mobile-OTP/mOTP_), but tokens could also be send to you by eMail, SMS or using a pre-generated token list. The OpenOTP Authentication Server is able to manage a fully featured LDAP tree, thus including multiple domains/organizations/organizational units.