using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using AndoIt.Common.Common;

namespace AndoIt.Common.Test.Common.Unit
{
	[TestClass]
	public class InsisterTest
	{
		[TestMethod]
		public void Insist_NoErrorReturnVoid_OneCall()
		{
			//	Arrange
			var objetoStub = new Mock<IHaceCosas>();
			objetoStub.Setup(x => x.SinReturn());

			//	Act
			new Insister().Insist(() => objetoStub.Object.SinReturn(), 5);

			//	Assert
			objetoStub.Verify(x => x.SinReturn(), Times.Once);
		}
		public interface IHaceCosas
		{
			void SinReturn();
			int ConReturn();
		}
		
		[TestMethod]
		public void Insist_OneErrorReturnVoid_TwoCalls()
		{
			//	Arrange
			var objetoStub = new Mock<IHaceCosas>();
			bool firstTimeExecuteCalled = true;
			objetoStub.Setup(x => x.SinReturn())
				.Callback(() =>
			{
				if (firstTimeExecuteCalled)
				{
					firstTimeExecuteCalled = false;
					throw new Exception();
				}
			});

			//	Act
			new Insister().Insist(() => objetoStub.Object.SinReturn(), 5);

			//	Assert
			objetoStub.Verify(x => x.SinReturn(), Times.Exactly(2));
		}

		[TestMethod]
		public void Insist_MoreErrorThanSetReturnVoid_Exception()
		{
			//	Arrange
			var objetoStub = new Mock<IHaceCosas>();
			objetoStub.Setup(x => x.SinReturn()).Throws<Exception>();

			//	Act
			try
			{
				new Insister().Insist(() => objetoStub.Object.SinReturn(), 5);
				Assert.Fail("Debería fallar pues todas devuelve exception");
			}
			catch
			{
				//	Por quí pasa siempre
			}

			//	Assert
			objetoStub.Verify(x => x.SinReturn(), Times.Exactly(5));
		}

		[TestMethod]
		public void Insist_NoErrorReturnInt_OneCall()
		{
			//	Arrange
			var objetoStub = new Mock<IHaceCosas>();
			objetoStub.Setup(x => x.ConReturn()).Returns(33);

			//	Act
			int actual = new Insister().Insist(() => objetoStub.Object.ConReturn(), 5);

			//	Assert
			objetoStub.Verify(x => x.ConReturn(), Times.Once);
			Assert.AreEqual(33, actual);
		}

		[TestMethod]
		public void Insist_OneErrorReturnInt_TwoCalls()
		{
			//	Arrange
			var objetoStub = new Mock<IHaceCosas>();
			int calls = 0;
			objetoStub.Setup(x => x.ConReturn())
				.Returns(() => calls)
				.Callback(() =>
				{
					if (calls == 0)
					{
						calls++;
						throw new Exception();
					}
				});

			//	Act
			int actual = new Insister().Insist(() => objetoStub.Object.ConReturn(), 5);

			//	Assert
			objetoStub.Verify(x => x.ConReturn(), Times.Exactly(2));
			Assert.AreEqual(1, actual);
		}

		[TestMethod]
		public void Insist_MoreErrorThanSetReturnInt_Exception()
		{
			//	Arrange
			var objetoStub = new Mock<IHaceCosas>();
			objetoStub.Setup(x => x.ConReturn()).Throws<Exception>();

			//	Act
			try
			{
				new Insister().Insist(() => objetoStub.Object.ConReturn(), 5);
				Assert.Fail("Debería fallar pues todas devuelve exception");
			}
			catch
			{
				//	Por quí pasa siempre
			}

			//	Assert
			objetoStub.Verify(x => x.ConReturn(), Times.Exactly(5));
		}
	}
}
