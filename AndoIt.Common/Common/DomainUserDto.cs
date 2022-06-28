using System;
using System.Collections.Generic;
using System.Security;

namespace AndoIt.Common
{
	public class DomainUserDto
	{
		public string Domain { get; set; }
		public string Name { get; set; }
		public string Password { get; set; }

		public SecureString SecurePassword
		{
			get
			{
				return ConvertToSecureString(this.Password);
			}
		}

		public SecureString ConvertToSecureString(string password)
		{
			if (password == null)
				throw new ArgumentNullException("password");

			//unsafe
			//{
			//	fixed (char* passwordChars = password)
			//	{
			var securePassword = new SecureString();
			new List<char>(password).ForEach(a => securePassword.AppendChar(a));
			//foreach (char a in password)
			//{
				
			//	//var securePassword = new SecureString(passwordChars, password.Length);
			//}
			securePassword.MakeReadOnly();
			return securePassword;
			//	}
			//}
		}
	}
}