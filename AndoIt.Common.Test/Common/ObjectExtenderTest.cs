using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using AndoIt.Common.Core.Common;

namespace AndoIt.Common.Test.Common
{
    [TestClass]
    public class ObjectExtenderTest
    {
        [TestMethod]
        public void ObjectToJsonString_NormalCase_SerializesObjectCorrectly()
        {
            var obj = new { Nombre = "Juan", Edad = 30 };
            string json = obj.ObjectToJsonString();

            Assert.IsNotNull(json);
            Assert.AreEqual("{\"Nombre\":\"Juan\",\"Edad\":30}", json);
        }

        [TestMethod]
        public void GetKeyValue_MemberNNestedMemberNListMember_ThreElements()
        {
            //	Arrange
            object toTest = new
            {
                FirstMember = "FirstMemberData",
                NestingMember = new { NestedMember = "NestedData" },
                ThingsList = new List<string> { "FirstDataInList" }
            };
            Dictionary<string, object> expected = new Dictionary<string, object> {
                { "FirstMember", "FirstMemberData" },
                { "NestingMember.NestedMember", "NestedData" },
                { "ThingsList[0]", "FirstDataInList" }
            };

            //	Act
            var actual = toTest.GetKeyValue();

            //	Assert
            Assert.AreEqual(expected["FirstMember"], actual["FirstMember"]);
            Assert.AreEqual(expected["NestingMember.NestedMember"], actual["NestingMember.NestedMember"]);
            Assert.AreEqual(expected["ThingsList[0]"], actual["ThingsList[0]"]);
        }

        [TestMethod]
        public void GetKeyValue_SimpleObject_ReturnsCorrectDictionary()
        {
            var obj = new { Nombre = "Juan", Edad = 30 };
            var result = obj.GetKeyValue();

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("Juan", result["Nombre"]);
            Assert.AreEqual(30, result["Edad"]);
        }

        [TestMethod]
        public void GetKeyValue_NestedObject_ReturnsFlattenedKeys()
        {
            var obj = new
            {
                User = new
                {
                    Nombre = "Ana",
                    Edad = 25
                }
            };

            var result = obj.GetKeyValue();

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("Ana", result["User.Nombre"]);
            Assert.AreEqual(25, result["User.Edad"]);
        }

        [TestMethod]
        public void GetKeyValue_ListOfPrimitives_ReturnsIndexedKeys()
        {
            var obj = new { Numbers = new List<int> { 10, 20, 30 } };
            var result = obj.GetKeyValue();

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(10, result["Numbers[0]"]);
            Assert.AreEqual(20, result["Numbers[1]"]);
            Assert.AreEqual(30, result["Numbers[2]"]);
        }

        [TestMethod]
        public void GetKeyValue_ListOfObjects_ReturnsFlattenedKeys()
        {
            var obj = new
            {
                Users = new List<object>
            {
                new { Nombre = "Pedro", Edad = 40 },
                new { Nombre = "María", Edad = 35 }
            }
            };

            var result = obj.GetKeyValue();

            Assert.AreEqual(4, result.Count);
            Assert.AreEqual("Pedro", result["Users[0].Nombre"]);
            Assert.AreEqual(40, result["Users[0].Edad"]);
            Assert.AreEqual("María", result["Users[1].Nombre"]);
            Assert.AreEqual(35, result["Users[1].Edad"]);
        }

        [TestMethod]
        public void GetKeyValue_EmptyObject_ReturnsEmptyDictionary()
        {
            var obj = new { };
            var result = obj.GetKeyValue();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetKeyValue_NullObject_ReturnsEmptyDictionary()
        {
            object obj = null;
            var result = obj.GetKeyValue();

            Assert.AreEqual(0, result.Count);
        }
    }
}
