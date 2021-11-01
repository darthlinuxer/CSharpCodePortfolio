using App.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests;

[TestClass]
public class ExtentionMethodTests
{
    [TestMethod]
    [DataRow("This is a Test String")]
    [DataRow("This is another Test String")]
    public void EncodeAndDecodeFromBase64_MustReturnTrue(string text)
    {
        var encodedTest = text.EncodeTo64();
        var decodedText = encodedTest.DecodeFrom64();
        Assert.IsTrue(text == decodedText);
    }

    [TestMethod]
    [DataRow("This is a Test String","secretpassword","saltwithmorethan8bytesofData")]
    [DataRow("This is another Test String","secretpassword","saltwithmorethan8bytesofData")]
    public void EncryptAndDecryptAES256_MustReturnTrue(string text, string password, string salt)
    {
        var encryptedText = text.EncryptAES256(password, salt);
        var decryptedText = encryptedText.DecryptAES256(password, salt);

        Assert.IsTrue(text == decryptedText);

    }

}