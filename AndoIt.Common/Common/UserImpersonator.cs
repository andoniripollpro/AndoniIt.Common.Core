using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;

namespace AndoIt.Common.Common
{
    public class UserImpersonator : IDisposable
    {
        private readonly WindowsImpersonationContext context;

        // constants from winbase.h
        private const int LOGON32_LOGON_INTERACTIVE = 2;
        private const int LOGON32_LOGON_NETWORK = 3;
        private const int LOGON32_LOGON_BATCH = 4;
        private const int LOGON32_LOGON_SERVICE = 5;
        private const int LOGON32_LOGON_UNLOCK = 7;
        private const int LOGON32_LOGON_NETWORK_CLEARTEXT = 8;
        private const int LOGON32_LOGON_NEW_CREDENTIALS = 9;
        
        private const int LOGON32_PROVIDER_DEFAULT = 0;
        private const int LOGON32_PROVIDER_WINNT35 = 1;
        private const int LOGON32_PROVIDER_WINNT40 = 2;
        private const int LOGON32_PROVIDER_WINNT50 = 3;

        public string UserName { get; }

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern int LogonUserA(String lpszUserName,
            String lpszDomain,
            String lpszPassword,
            int dwLogonType,
            int dwLogonProvider,
            ref IntPtr phToken);
        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int DuplicateToken(IntPtr hToken,
            int impersonationLevel,
            ref IntPtr hNewToken);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool RevertToSelf();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern bool CloseHandle(IntPtr handle);
                
        public UserImpersonator(string userName, string password, string domain = "")
        {
            this.UserName = userName;
            if (string.IsNullOrWhiteSpace(this.UserName))
                return;

            if (this.UserName.Contains("\\"))
            {
                var userNameSplitted = this.UserName.Split('\\');
                this.UserName = userNameSplitted[1];
                domain = userNameSplitted[0];
            }

            WindowsIdentity tempWindowsIdentity;
            IntPtr token = IntPtr.Zero;
            IntPtr tokenDuplicate = IntPtr.Zero;

            if (RevertToSelf())
            {
                if (LogonUserA(this.UserName, domain, password, LOGON32_LOGON_NEW_CREDENTIALS,
                    LOGON32_PROVIDER_DEFAULT, ref token) != 0)
                {
                    if (DuplicateToken(token, 2, ref tokenDuplicate) != 0)
                    {
                        tempWindowsIdentity = new WindowsIdentity(tokenDuplicate);
                        this.context = tempWindowsIdentity.Impersonate();
                        if (this.context != null)
                        {
                            CloseHandle(token);
                            CloseHandle(tokenDuplicate);
                            return;     //  Everything OK
                        }
                    }
                }
                else
                {
                    var win32 = new Win32Exception(Marshal.GetLastWin32Error());
                    //throw new Exception(string.Format("{0}, Domain:{1}, User:{2}, Password:{3}",
                    //    win32.Message, domain, this.UserName, password));
                    throw new Exception(win32.Message);
                }
            }
            if (token != IntPtr.Zero)
                CloseHandle(token);
            if (tokenDuplicate != IntPtr.Zero)
                CloseHandle(tokenDuplicate);

            throw new SecurityException("Some error happened impersonating");
        }

        public void Dispose()
        {            
            if (this.context != null) {
                this.context.Undo();             
            } else {
                //  Not impersonation made
            }
        }
    }
}
