using AndoIt.Common.Service;
using System.ServiceProcess;

namespace AndoIt.Productos.Service
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main()
		{
			ServiceBase[] ServicesToRun;

			ServicesToRun = new ServiceBase[]
			{
				new WinService()
			};
			ServiceBase.Run(ServicesToRun);
		}
	}
}
