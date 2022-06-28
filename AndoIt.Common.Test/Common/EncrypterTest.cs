using AndoIt.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AndoIt.Publicador.Common.Test.Unit
{
	[TestClass]
	public class EncrypterTest
    {
		[TestMethod]
		public void Encrypt_ThenDecrypt_Equal()
        {
			//  A
			string expected = "cualquie Ko$A $|@¬";
			Encrypter toTest = new Encrypter();

			//  AA
			Assert.AreEqual(expected, toTest.Decrypt(toTest.Encrypt(expected)));
		}

		[TestMethod]
		public void Encrypt_Normal_HardToRecognize()
		{
			//  A
			string expected = "cualquie Ko$A $|@¬";
			Encrypter toTest = new Encrypter();

			//  AA
			Assert.AreNotEqual(expected, toTest.Encrypt(expected));
		}
	}
}
