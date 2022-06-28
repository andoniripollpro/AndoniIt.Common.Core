using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AndoIt.Common.Test.Common.Unit
{
	[TestClass]
	public class GenericExtensionsTest
	{
		[TestMethod]
		public void BetterToString_NameList_OneStringCommaSeparated()
		{
			//	Arrange
			List<string> toTest = new List<string>(new string[] { "uno", "dos", "tres" });
			string expected = "('uno','dos','tres')";

			//	Act
			var actual = toTest.BetterToString("'");			

			//	Assert
			Assert.IsTrue(actual.Contains(expected));
			Assert.AreEqual(expected.Length, actual.Length);
		}
		[TestMethod]
		public void BetterToString_NameListWithDoubleComma_OneStringCommaSeparatedDoubleComma()
		{
			//	Arrange
			List<string> toTest = new List<string>(new string[] { "uno", "dos", "tres" });
			string expected = "(\"uno\",\"dos\",\"tres\")";

			//	Act
			var actual = toTest.BetterToString("\"");

			//	Assert
			Assert.IsTrue(actual.Contains(expected));
			Assert.AreEqual(expected.Length, actual.Length);
		}
		[TestMethod]
		public void BetterToString_NoQuotation_OneStringCommaSeparatedWithoutQuotation()
		{
			//	Arrange
			List<string> toTest = new List<string>(new string[] { "uno", "dos", "tres" });
			string expected = "(uno,dos,tres)";

			//	Act
			var actual = toTest.BetterToString();

			//	Assert
			Assert.IsTrue(actual.Contains(expected));
			Assert.AreEqual(expected.Length, actual.Length);
		}
		[TestMethod]
		public void BetterToString_EmptyList_OpenClosePanrenthesis()
		{
			//	Arrange
			List<string> toTest = new List<string>();
			string expected = "()";

			//	Act
			var actual = toTest.BetterToString();

			//	Assert
			Assert.IsTrue(actual.Contains(expected));
			Assert.AreEqual(expected.Length, actual.Length);
		}
		[TestMethod]
		public void BetterToString_NameListWithOneNull_NullIsLikeEmptyString()
		{
			//	Arrange
			List<string> toTest = new List<string>(new string[] { "uno", null, "tres" });
			string expected = "('uno','','tres')";

			//	Act
			var actual = toTest.BetterToString("'");

			//	Assert
			Assert.IsTrue(actual.Contains(expected));
			Assert.AreEqual(expected.Length, actual.Length);
		}
		[TestMethod]
		public void BetterToString_NameListWithOneNullAndNoQuotation_NullIsLikeEmptyString()
		{
			//	Arrange
			List<string> toTest = new List<string>(new string[] { "uno", null, "tres" });
			string expected = "(uno,,tres)";

			//	Act
			var actual = toTest.BetterToString();

			//	Assert
			Assert.IsTrue(actual.Contains(expected));
			Assert.AreEqual(expected.Length, actual.Length);
		}
	}
}
