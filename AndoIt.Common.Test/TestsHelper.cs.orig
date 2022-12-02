using Moq;
using AndoIt.Common.Common;
using AndoIt.Common.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AndoIt.Common.Infrastructure;

namespace AndoIt.Common.Test
{
	public class TestsHelper
	{
		private Mock<ILog> mockLog;
		private Mock<IEnquerClient> mockEnquerClient;
		private Mock<IEnqueable> mockEnqueable;

		public TestsHelper()
		{
			this.mockLog = new Mock<ILog>();
			this.mockEnquerClient = new Mock<IEnquerClient>();
			this.mockEnqueable = new Mock<IEnqueable>();
		}

		public Mock<ILog> MockLog => this.mockLog;
		public Mock<IEnquerClient> MockEnquerClient => this.mockEnquerClient;
		public Mock<IEnqueable> MockEnqueable => this.mockEnqueable;

		public Mock<IIoCObjectContainer> InizializeIoCObjectContainer()
		{			
			var objectContainerMock = new Mock<IIoCObjectContainer>();
			objectContainerMock.Setup(x => x.Get<ILog>(null)).Returns(new Mock<ILog>().Object);
			IoCObjectContainer.Singleton = objectContainerMock.Object;
			return objectContainerMock;
		}
	}
}
