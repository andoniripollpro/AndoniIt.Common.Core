using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AndoIt.Common.Test.Common
{
    [TestClass]
    public class GoodConfigOnResapiTest
    {
        [TestMethod]
        public void Crto_HttpClientAdapterNormal_NormalLoad()
        {
            //	Arrange
            var helper = new TestsHelper();            
            string expected = "Válido";

            //	Act
            var toTest = new GoodConfigOnResapi(helper.InizializeIoCObjectContainer().Object, "http://urlvalida.test", 1, helper.MockHttpClientAdapter.Object);
            string actual = toTest.GetAsString("UnJSon");

            //	Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        /// En realidad testeamos GetRootJString
        public void GetAsString_HttpClientAdapterNormalException_NormalLoadOldValue()
        {
            //	Arrange
            var helper = new TestsHelper();
            string expected = "Válido";

            //	Act
            var toTest = new GoodConfigOnResapi(helper.InizializeIoCObjectContainer().Object, "http://urlvalida.test", 1, helper.MockHttpClientAdapter.Object);
            helper.MockHttpClientAdapter.Setup(x => x.AllCookedUpGet(It.IsAny<string>(), null)).Throws(new Exception("Por alguna razón no puede acceder"));
            Thread.Sleep(1001);
            string actual = toTest.GetAsString("UnJSon");

            //	Assert
            Assert.AreEqual(expected, actual);
            helper.MockHttpClientAdapter.Verify(x => x.AllCookedUpGet(It.IsAny<string>(), null), Times.Exactly(3)); // La 1 al construir el objeto y la excepción lo intenta 2 veces porque así está hardcodeado
        }

        [TestMethod]
        public void GetAsString_HttpClientAdapterSlow_LoadOldValue()
        {
            //	Arrange
            var helper = new TestsHelper();
            string expected = "Válido";

            //	Act
            var toTest = new GoodConfigOnResapi(helper.InizializeIoCObjectContainer().Object, "http://urlvalida.test", 1, helper.MockHttpClientAdapter.Object);
            helper.MockHttpClientAdapter.Setup(x => x.AllCookedUpGet(It.IsAny<string>(), null))
                .Returns(helper.WaitsMillsecondsThenReturnsString(100, "{\"log\":{\"forbiddenWords\":[]}, \"UnJSon\":\"Nuevo valor\"}"));
            Thread.Sleep(1001);
            string actual = toTest.GetAsString("UnJSon");

            //	Assert
            Assert.AreEqual(expected, actual);
            helper.MockHttpClientAdapter.Verify(x => x.AllCookedUpGet(It.IsAny<string>(), null), Times.Exactly(2)); // La 1 al construir el objeto y la segunda lectura lenta aunque no llegue a pillarlo
        }

        [TestMethod]
        public void GetAsStringTwice_HttpClientAdapterSlow_NormalLoadNewValue()
        {
            //	Arrange
            var helper = new TestsHelper();
            string expected = "Nuevo valor";

            //	Act
            var toTest = new GoodConfigOnResapi(helper.InizializeIoCObjectContainer().Object, "http://urlvalida.test", 1, helper.MockHttpClientAdapter.Object);
            helper.MockHttpClientAdapter.Setup(x => x.AllCookedUpGet(It.IsAny<string>(), null))
                .Returns(helper.WaitsMillsecondsThenReturnsString(100, "{\"log\":{\"forbiddenWords\":[]}, \"UnJSon\":\"Nuevo valor\"}"));
            Thread.Sleep(1001);
            string actual = toTest.GetAsString("UnJSon");
            Thread.Sleep(101);
            actual = toTest.GetAsString("UnJSon");

            //	Assert
            Assert.AreEqual(expected, actual);            
        }

        [TestMethod]
        public void GetAsStringMultithread_HttpClientAdapterSlow_NormalLoadNewValue()
        {
            //	Arrange
            var helper = new TestsHelper();
            string expected = "Nuevo valor";

            //	Act
            var toTest = new GoodConfigOnResapi(helper.InizializeIoCObjectContainer().Object, "http://urlvalida.test", 1, helper.MockHttpClientAdapter.Object);
            helper.MockHttpClientAdapter.Setup(x => x.AllCookedUpGet(It.IsAny<string>(), null))
                .Returns(helper.WaitsMillsecondsThenReturnsString(100, "{\"log\":{\"forbiddenWords\":[]}, \"UnJSon\":\"Nuevo valor\"}"));
            Thread.Sleep(1001);
            var taskList = new List<Task>();
            for (int i = 0; i < 100; i++)
                taskList.Add(new Task(new Action(() => toTest.GetAsString("UnJSon"))));
            taskList.ForEach(x => x.Start());
            Thread.Sleep(101);
            string actual = toTest.GetAsString("UnJSon");

            //	Assert
            Assert.AreEqual(expected, actual);
        }
    }
}
