using System.Diagnostics;
using System.IO;
using System.Text;

namespace AndoIt.Common
{
	public class CmdCommand
    {
		private readonly string cmdCommandLocation = null;
        private readonly string workingDirectory = null;
        private readonly bool independentProcess = false;
		private readonly DomainUserDto user = null;

		public CmdCommand(string cmdCommandLocation, string workingDirectory = null, bool independentProcess = false, DomainUserDto user = null)
        {
			this.cmdCommandLocation = cmdCommandLocation;
            this.independentProcess = independentProcess;
            if (!string.IsNullOrWhiteSpace(workingDirectory))
            {
                var dir = Path.Combine(Directory.GetCurrentDirectory(), workingDirectory);
                //var res = Directory.GetDirectories(dir);
                this.workingDirectory = dir;
            }
			if (user != null)
				this.user = user;
        }

		public string Start(string command, string arguments)
        {
			Process process = new Process();
			ProcessStartInfo processStart = GetProcessStartInfo();            
			if (this.user != null)
			{
				processStart.Domain = this.user.Domain;
				processStart.UserName = this.user.Name;
				processStart.Password = this.user.SecurePassword;
			}
			processStart.WindowStyle = ProcessWindowStyle.Hidden;
			processStart.FileName = command; // $"{this.cmdCommandLocation}\\CmdCommand.bat";
			processStart.Arguments = arguments;// $"\"{this.workingDirectory}\" {command} {arguments}";
			processStart.StandardOutputEncoding = Encoding.UTF8;
			processStart.StandardErrorEncoding = Encoding.UTF8;
			processStart.WorkingDirectory = this.cmdCommandLocation;
			process.StartInfo = processStart;
                        
            var result = process.Start();

            if (this.independentProcess)
                return string.Empty;
            else
            {
				string output = process.StandardOutput.ReadToEnd();
				string error = process.StandardError.ReadToEnd();

				process.WaitForExit();

				var resultStringBuilder = output;
				if (!string.IsNullOrWhiteSpace(error))
					resultStringBuilder += $"ERROR: {error}";
				return resultStringBuilder;
			}
        }

		public string StartPsCommand(string commandAndArguments)
		{
			var process = new Process();
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.RedirectStandardError = true;
			process.StartInfo.FileName = @"C:\windows\system32\windowspowershell\v1.0\powershell.exe";
			process.StartInfo.Arguments = commandAndArguments;

			process.Start();
				
			string output = process.StandardOutput.ReadToEnd();
			string error = process.StandardError.ReadToEnd();
								
			process.WaitForExit();

			var resultStringBuilder = output;
			if (!string.IsNullOrWhiteSpace(error))
				resultStringBuilder += $"ERROR: {error}";
			return resultStringBuilder;
		}

        private ProcessStartInfo GetProcessStartInfo()
        {
            ProcessStartInfo silentProcess = new ProcessStartInfo();
            silentProcess.UseShellExecute = false;
            silentProcess.CreateNoWindow = true;
            silentProcess.LoadUserProfile = true;
            silentProcess.RedirectStandardError = true;
            silentProcess.RedirectStandardOutput = true;
            return silentProcess;
        }
		
		private bool IsAsp
		{
			get
			{
				if (Process.GetCurrentProcess().ProcessName == "w3wp"
					|| Process.GetCurrentProcess().ProcessName == "aspnet_wp"
					|| Process.GetCurrentProcess().ProcessName == "iisexpress")
					return true;
				else
					return false;
			}
		}
	}
}
