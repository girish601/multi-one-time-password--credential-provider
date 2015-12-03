# Introduction #

mOTP-CP depends on multiOTP.exe from SysCo (http://www.multiotp.net). To install multiOTP.exe and create a user account that works with mOTP-CP read further.
Make sure multiOTP.exe is installed, configured and working **BEFORE** you install mOTP-CP.


# Install multiOTP.exe #

To install multiOTP.exe do the following:
  1. Download the current version of multiotp from http://www.multiotp.net
  1. Create a directory on your hard-drive
  1. Extract multiotp.exe from the downloaded archive to the directory created in step 2

# Add a user to multiOTP.exe #

To add a user account that works with mOTP-CP do the following:
  1. Open a command line (cmd) to the directory you installed multiOTP.exe to
  1. To add a user account that uses TOTP algorithm passwords:
```
multiotp.exe -debug -create <USERNAME> TOTP <KEY> <PIN> <LENGTH> <LIVETIME>
```
    * 

&lt;USERNAME&gt;

 Use your Windows account name
    * 

&lt;KEY&gt;

 should be a 160 bit HEX-key (recommended)
    * 

&lt;PIN&gt;

 A 4-digit pin value. This value does not matter unless you create an account with the option -prefix-pin.
    * 

&lt;LENGTH&gt;

 specifies the length of the generated OTPs
    * 

&lt;LIVETIME&gt;

 (in TOTP-words) specifies how long each generated TOTP password is valid
    * Example. Windows account name is john and we dont want -prefix-pin. We choose 6-digit passwords and a live time of 30s.
```
multiotp.exe -debug -create john TOTP 56821bac24fbd234339356821bac24fbd2343393 4455 6 30
```
    * I recommend to use the -debug option. If not provided you won't see any output from multiotp.exe
    * The example creates a user john, using TOTP as algorithm that produces 6-digit TOTP passwords which are valid for 30 seconds

# Configuring Google Authenticator #

In order to use Google Authenticator for token generation, do the following:

  1. Use [this online tool](http://www.darkfader.net/toolbox/convert/) to convert the `<KEY>` from the above steps to Base32:
    1. Select "Hexadecimal" from the first box of the first row
    1. Enter the `<KEY>` in the column "Value"
    1. Select "Base32" from the first box of the second row
    1. The Base32-representation appears in the value field of the second row
  1. Open Google Authenticator
  1. Press "Menu"
  1. Select "Create Account"
  1. Select "Enter Key"
  1. Enter a name for the account
  1. Enter the Base32-converted `<KEY>` from above and select time-based
  1. Select "Add"
  1. Done :)

# Configuring a token production device #

  * You need to use the values of `<KEY>`, `<LENGTH>` and `<LIVETIME>` to configure your token production device (e.g. Smartphone).
    * For Android devices: try Android Token.
  * Read the README of your device or software

# Testing your configuration #

  1. Open a command line (cmd) to the directory you installed multiOTP.exe to
  1. Let your token device produce a OTP for you
  1. Do the following:
```
multiotp.exe -debug <USERNAME> <GENERATED_TOKEN>
```
  1. The result should be something like:
```
0 OK: Token accepted
```
  1. Now you are ready to install mOTP-CP