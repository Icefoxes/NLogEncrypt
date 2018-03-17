# NLogEncrypt
A .NET Core Project implement encrypt functionality

### How to encrypt NLog 

Nlog does not have a built-in mechanism to perform encrypting its log, but it provides a simple wrapper, just like a pipeline to transform the original text into encrypted text, this is the base class
```CSharp
public abstract class WrapperLayoutRendererBase : LayoutRenderer
{
    protected WrapperLayoutRendererBase();

    [DefaultParameter]
    public Layout Inner { get; set; }

    protected override void Append(StringBuilder builder, LogEventInfo logEvent);
    protected virtual string RenderInner(LogEventInfo logEvent);
    protected abstract string Transform(string text);
}
```


#### 1. Create Wrapper
---------------------------------------
```CSharp
using System.ComponentModel;
using NLog.Config;
using NLog.LayoutRenderers;
using NLog.LayoutRenderers.Wrappers;

namespace NLogEncryt
{
    [LayoutRenderer("Encrypt")]
    [AmbientProperty("Encrypt")]
    [ThreadAgnostic]
    public sealed class EncryptLayoutRendererWrapper : WrapperLayoutRendererBase
    {
        public EncryptLayoutRendererWrapper()
        {
            Encrypt = true;
        }

        [DefaultValue(true)]
        public bool Encrypt { get; set; }

        protected override string Transform(string text)
        {
            return Encrypt ? LogEncryptor.EncryptData(text) : text;
        }
    }
}
```

#### 2. create nlog.config file
```XML
<?xml version="1.0" encoding="utf-8" ?>
<!-- XSD manual extracted from package NLog.Schema: https://www.nuget.org/packages/NLog.Schema-->
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd" xsi:schemaLocation="NLog NLog.xsd"
      xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
      autoReload="true"
      internalLogFile="c:\temp\console-example-internal.log"
      internalLogLevel="Info" >

  <!-- the targets to write to -->
  <targets>
    <!-- write logs to file -->
    <target xsi:type="File" name="target1" fileName="c:\temp\console-example.log"
            layout="${Encrypt:${date}|${level:uppercase=true}|${message} ${exception}|${logger}|${all-event-properties}}" />
    <target xsi:type="Console" name="target2"
            layout="${Encrypt:${date}|${level:uppercase=true}|${message} ${exception}|${logger}|${all-event-properties}}" />


  </targets>

  <!-- rules to map from logger name to target -->
  <rules>
    <logger name="*" minlevel="Trace" writeTo="target1,target2" />

  </rules>
</nlog>
```

#### 3. create encryptor
LogEncryptor implements AES algorithm
```CSharp
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace NLogEncryt
{
    // AES Log Encryptor
    public class LogEncryptor
    {
        // 64 bit key
        private static readonly byte[] Key = {
            0x4a, 0xfa, 0xbc, 0x2f, 0x85, 0xb9, 0x3d, 0xad,
            0x30, 0x5a, 0x4a, 0x96, 0x8e, 0x23, 0xd1, 0x2a,
            0xf0, 0xbc, 0x8b, 0x04, 0x69, 0xa4, 0xaa, 0x50,
            0xf5, 0x8d, 0x95, 0x10, 0x7a, 0x73, 0xee, 0x61 };

        // 32 bit IV
        private static readonly byte[] IV = {
            0xd1, 0xa1, 0xd3, 0x1f, 0xda, 0x89, 0xbf, 0x19,
            0x25, 0x76, 0x3a, 0x7a, 0x69, 0x6b, 0xaf, 0xcf };

        /// <summary>
        /// Padding Mode
        /// </summary>
        private static readonly PaddingMode Padding = PaddingMode.PKCS7;

        /// <summary>
        /// Encrypt Data
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        public static string EncryptData(string original)
        {
            return EncryptStringToBytes(original, Key, IV);
        }

        /// <summary>
        /// Decrypt Data
        /// </summary>
        /// <param name="encrypted"></param>
        /// <returns></returns>
        public static string DecryptData(string encrypted)
        {
            return DecryptStringFromBytes(encrypted, Key, IV);
        }

        /// <summary>
        /// Encrypt String To Bytes With AES
        /// </summary>
        /// <param name="plainText"></param>
        /// <param name="Key"></param>
        /// <param name="IV"></param>
        /// <returns></returns>
        private static string EncryptStringToBytes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentNullException("plainText");

            string encrypted = string.Empty;
            // Create an Aes object with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;
                aesAlg.Padding = Padding;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor();
                byte[] planbytes = Encoding.UTF8.GetBytes(plainText);
                // Create the streams used for encryption.
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        cs.Write(planbytes, 0, planbytes.Length);
                    encrypted = Convert.ToBase64String(ms.ToArray());
                }
            }
            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }

        /// <summary>
        /// Decrypt String From Bytes with AES
        /// </summary>
        /// <param name="cipherText"></param>
        /// <param name="Key"></param>
        /// <param name="IV"></param>
        /// <returns></returns>
        private static string DecryptStringFromBytes(string cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (string.IsNullOrEmpty(cipherText))
                throw new ArgumentNullException("cipherText");

            // Declare the string used to hold the decrypted text.
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            string plaintext = string.Empty;

            // Create an Aes object with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;
                aesAlg.Padding = Padding;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor();

                // Create the streams used for decryption.
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                    plaintext = Encoding.UTF8.GetString(ms.ToArray());
                }
            }
            return plaintext;
        }
    }
}
```

#### 4. register layout
For this demo, I used .NET Core Console, and nlog doesn't support auto register for .NET Core yet, if you are using .NET Framework, please refer to this [manual](https://github.com/NLog/NLog/wiki/Register-your-custom-component)
```CSharp
Layout.Register("Encrypt", typeof(EncryptLayoutRendererWrapper));
```

The whole demo is located at [NLogEncrypt](https://github.com/Icefoxes/NLogEncrypt)
