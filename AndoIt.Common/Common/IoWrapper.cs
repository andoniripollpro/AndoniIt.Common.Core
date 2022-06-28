using AndoIt.Common.Infrastructure;
using System.IO;

namespace AndoIt.Common.Common
{
    public class IoWrapper : IIoWrapper
    {
        DirectoryInfo IIoWrapper.DirectoryCreate(string path)
        {
            if (!Directory.Exists(path))
                try { Directory.CreateDirectory(path); } catch { }
            return new DirectoryInfo(path);
        }

        void IIoWrapper.DirectoryDelete(string path)
        {
            Directory.Delete(path);
        }

        void IIoWrapper.DirectoryMove(string origin, string destination)
        {
            Directory.Move(origin, destination);
        }

        void IIoWrapper.FileCopy(string source, string destination)
        {
            File.Copy(source, destination);
        }

        void IIoWrapper.FileDelete(string path)
        {
            File.Delete(path);
        }

        void IIoWrapper.FileMove(string source, string destination)
        {
            File.Move(source, destination);
        }
    }
}
