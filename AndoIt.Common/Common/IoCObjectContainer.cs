using AndoIt.Common.Interface;
using System.Configuration;

namespace AndoIt.Common.Common
{
	public static class IoCObjectContainer
	{
		private static IIoCObjectContainer singleton = null;
		
		public static IIoCObjectContainer Singleton {
			get {
				return IoCObjectContainer.singleton ?? throw new ConfigurationErrorsException("IoCObjectContainer.Singleton sin asignar. Es necesario inicializarlo IoCObjectContainer.Singleton = tuIoCContainer");
			}
			set {
				IoCObjectContainer.singleton = value;
			}
		}
	}
}
