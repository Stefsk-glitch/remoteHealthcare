using System;
using System.Security.Cryptography;
using System.Text;

public class RSAEncryption
{
    private RSACryptoServiceProvider rsa;

    public RSAEncryption() { 
        rsa = new RSACryptoServiceProvider(2048);
    }

    public string GetPublicKey()
    {
        return rsa.ToXmlString(false);
    }

    public byte[] EncryptMessage(string message, string publicKey)
    {

        RSACryptoServiceProvider rsaEncrypt = new RSACryptoServiceProvider();
        rsaEncrypt.FromXmlString(publicKey);

        byte[] encryptedBytes = rsaEncrypt.Encrypt(Encoding.UTF8.GetBytes(message), false);
        return encryptedBytes;
    }

    public string DecryptMessage(byte[] encryptedMessage)
    {
        byte[] decryptedBytes = rsa.Decrypt(encryptedMessage, false);
        return Encoding.UTF8.GetString(decryptedBytes);
    }
}
