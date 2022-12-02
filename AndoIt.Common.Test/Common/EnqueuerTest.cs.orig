using AndoIt.Common.Infrastructure;
using AndoIt.Common.Interface;
using AndoIt.Common.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading;

namespace AndoIt.Publicador.Common.Test.Unit
{
	[TestClass]
	public class EnqueuerTest
    {
		[TestMethod]
		public void InsertTask_DueOnPast_Haldeled()
        {
			//  Arrange
			var helper = new TestsHelper();			
			IEnqueuer toTest = new Enqueuer(helper.MockLog.Object, helper.MockEnquerClient.Object);
			var mockEnqueable = helper.MockEnqueable;
			mockEnqueable.Setup(x => x.WhenToHandleNext).Returns(DateTime.Now.AddSeconds(-1));

			//  Act
			toTest.InsertTask(this, mockEnqueable.Object);
		

			//	Assert
			mockEnqueable.Verify(x => x.Handle(), Times.Once);
			Assert.AreEqual(1, toTest.Queue.Count); // Still in the queue until next cycle when State != Pending		
		}

		[TestMethod]
		public void InsertTask_DueOnFuture_NotHaldeled()
		{
			//  Arrange
			var helper = new TestsHelper();
			IEnqueuer toTest = new Enqueuer(helper.MockLog.Object, helper.MockEnquerClient.Object);
			var mockEnqueable = helper.MockEnqueable;
			mockEnqueable.Setup(x => x.WhenToHandleNext).Returns(DateTime.Now.AddSeconds(2));

			//  Act
			toTest.InsertTask(this, mockEnqueable.Object);
			
			//	Assert
			mockEnqueable.Verify(x => x.Handle(), Times.Never);
			Assert.AreEqual(1, toTest.Queue.Count); 
		}

		[TestMethod]
		public void InsertTask_Twice_OnlyOneInQueue()
		{
			//  Arrange
			var helper = new TestsHelper();
			IEnqueuer toTest = new Enqueuer(helper.MockLog.Object, helper.MockEnquerClient.Object);
			var mockEnqueable = helper.MockEnqueable;
			mockEnqueable.Setup(x => x.WhenToHandleNext).Returns(DateTime.Now.AddSeconds(2));

			//  Act
			toTest.InsertTask(this, mockEnqueable.Object);
			toTest.InsertTask(this, mockEnqueable.Object);

			//	Assert
			mockEnqueable.Verify(x => x.Handle(), Times.Never);
			Assert.AreEqual(1, toTest.Queue.Count); 
			//Assert.AreEqual(IEnqueable.EnqueableState.HandledOk, toTest.Queue[0].State); 
		}
	}
}
