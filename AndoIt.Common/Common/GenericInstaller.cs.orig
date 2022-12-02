using System.ComponentModel;
using System.Configuration;
using System.Reflection;
using System.ServiceProcess;

namespace AndoIt.Common
{
	/// <summary>	
	/// Necesita que en la App.config exista en appSettings con las variables ServiceName y ServiceDescription
	/// Se necesita heredar de ella en el proyecto raíz que se quiera utilizar
	/// </summary>
	[RunInstaller(true)]
    public class GenericInstaller : System.Configuration.Install.Installer
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private IContainer components = null;

		private ServiceProcessInstaller serviceProcessInstaller;
		private ServiceInstaller serviceInstaller;

		public GenericInstaller()
		{
			InitializeComponent();
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			components = new Container();
			this.serviceProcessInstaller = new ServiceProcessInstaller()
			{
				Account = ServiceAccount.LocalSystem,
				Password = null,
				Username = null
			};

			this.serviceInstaller = new ServiceInstaller()
			{
				StartType = ServiceStartMode.Manual,
				Description = GetServiceNameAppConfig("ServiceDescription"),
				ServiceName = GetServiceNameAppConfig("ServiceName"),
				DisplayName = GetServiceNameAppConfig("ServiceName")
			};
			
			this.Installers.AddRange(new System.Configuration.Install.Installer[] {
				this.serviceProcessInstaller,
				this.serviceInstaller
			});
		}

		private string GetServiceNameAppConfig(string serviceName)
		{
			var config = ConfigurationManager.OpenExeConfiguration(Assembly.GetAssembly(this.GetType()).Location);
			return config.AppSettings.Settings[serviceName].Value;
		}
	}
}