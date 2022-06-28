using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AndoIt.Common.Test.Common.Unit
{
	[TestClass]
	public class StringAdapterTest
	{
		#region Start and Length
		[TestMethod]
		public void SubStringTruncated_JustLenth_AsInSubstring()
		{
			//	Arrange			
			string toTest = "Cosas que hay en un string y quiero cortar por aquí. BLA BLA BLA BLA BLE BLE BLE";
			//               0123456789012345678901234567890123456789012345678901
			//                         1         2         3         4         5
			int positionToTruncate = 51;

			//	Act
			string actual = toTest.SubStringTruncated(positionToTruncate);

			//	Assert
			Assert.AreEqual(". BLA BLA BLA BLA BLE BLE BLE", actual);
		}

		[TestMethod]
		public void SubStringTruncated_JustStart_AsInSubstring()
		{
			//	Arrange			
			string toTest = "Cosas que hay en un string y quiero cortar por aquí. BLA BLA BLA BLA BLE BLE BLE";
			//               0123456789012345678901234567890123456789012345678901
			//                         1         2         3         4         5
			int positionToTruncate = 51;

			//	Act
			string actual = toTest.SubStringTruncated(0, positionToTruncate);

			//	Assert
			Assert.AreEqual("Cosas que hay en un string y quiero cortar por aquí", actual);
		}

		[TestMethod]
		public void SubStringTruncated_StartAndLenght_AsInSubstring()
		{
			//	Arrange			
			string toTest = "Cosas que hay en un string y quiero cortar por aquí. BLA BLA BLA BLA BLE BLE BLE";
			//               0123456789012345678901234567890123456789012345678901
			//                         1         2         3         4         5
			int positionToTruncate = 51;

			//	Act
			string actual = toTest.SubStringTruncated(positionToTruncate +2, 3);

			//	Assert
			Assert.AreEqual("BLA", actual);
		}

		[TestMethod]
		public void SubStringTruncated_StartAndLenghtTooBig_AsInSubstring()
		{
			//	Arrange			
			string toTest = "Cosas que hay en un string y quiero cortar por aquí. BLA BLA BLA BLA BLE BLE BLE";
			//               0123456789012345678901234567890123456789012345678901
			//                         1         2         3         4         5
			int positionToTruncate = 51;

			//	Act
			string actual = toTest.SubStringTruncated(positionToTruncate, int.MaxValue);

			//	Assert
			Assert.AreEqual(". BLA BLA BLA BLA BLE BLE BLE", actual);
		}

		[TestMethod]
		public void SubStringTruncated_JustLenthTooBig_SameStringNoError()
		{
			//	Arrange			
			string toTest = "Cosas que hay en un string y quiero cortar por aquí. BLA BLA BLA BLA BLE BLE BLE";
			//               0123456789012345678901234567890123456789012345678901
			//                         1         2         3         4         5			

			//	Act
			string actual = toTest.SubStringTruncated(0, int.MaxValue);

			//	Assert
			Assert.AreEqual(toTest, actual);
		}

		[TestMethod]
		public void SubStringTruncated_JustStartTooBig_EmptyNoError()
		{
			//	Arrange			
			string toTest = "Cosas que hay en un string y quiero cortar por aquí. BLA BLA BLA BLA BLE BLE BLE";
			//               0123456789012345678901234567890123456789012345678901
			//                         1         2         3         4         5			

			//	Act
			string actual = toTest.SubStringTruncated(int.MaxValue);

			//	Assert
			Assert.AreEqual(string.Empty, actual);
		}
		#endregion

		#region Words
		[TestMethod]
		public void SubStringTruncated_CutsFromStartWord_RestOfStringOk()
		{
			//	Arrange			
			string toTest = "Cosas que hay en un string y quiero cortar por aquí. BLA BLA BLA BLA BLE BLE BLE";
			
			//	Act
			string actual = toTest.SubStringTruncated("aquí");

			//	Assert
			Assert.AreEqual(". BLA BLA BLA BLA BLE BLE BLE", actual);
		}
		[TestMethod]
		public void SubStringTruncated_CutToEndWord_RestOfStringOk()
		{
			//	Arrange			
			string toTest = "Cosas que hay en un string y quiero cortar por aquí. BLA BLA BLA BLA BLE BLE BLE";

			//	Act
			string actual = toTest.SubStringTruncated("no existe", "aquí");

			//	Assert
			Assert.AreEqual("Cosas que hay en un string y quiero cortar por ", actual);
		}
		[TestMethod]
		public void SubStringTruncated_CutFromToWords_RestOfStringOk()
		{
			//	Arrange			
			string toTest = "<Un XML con muchas cosas/><Identificador>El contenido que quiero quedarme</Identificador><Mas cosas que no me interesan/>";

			//	Act
			string actual = toTest.SubStringTruncated("<Identificador>", "</Identificador>");

			//	Assert
			Assert.AreEqual("El contenido que quiero quedarme", actual);
		}
		[TestMethod]
		public void SubStringTruncated_WordsDontExist_AllString()
		{
			//	Arrange			
			string toTest = "Cosas que hay en un string y quiero cortar por aquí. BLA BLA BLA BLA BLE BLE BLE";

			//	Act
			string actual = toTest.SubStringTruncated("no existe", "tampoco existe");

			//	Assert
			Assert.AreEqual(toTest, actual);
		}
		#endregion

		#region Variables and nomenclature
		[TestMethod]
		public void FromCamelCaseToPhrase_LowerCamelback_Phrase()
		{
			//	Arrange			
			string toTest = "variableEnLowerCamelback";

			//	Act
			string actual = toTest.FromCamelCaseToPhrase();

			//	Assert
			Assert.AreEqual("Variable en lower camelback", actual);
		}
		[TestMethod]
		public void FromCamelCaseToPhrase_UpperCamelback_Phrase()
		{
			//	Arrange			
			string toTest = "VariableEnUpperCamelback";

			//	Act
			string actual = toTest.FromCamelCaseToPhrase();

			//	Assert
			Assert.AreEqual("Variable en upper camelback", actual);
		}
		#endregion
	}
}
