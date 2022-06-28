using System.IO;

namespace AndoIt.Common.Infrastructure
{
    public interface IIoWrapper
    {
        void FileCopy(string origin, string destination);
        void FileMove(string origin, string destination);
        void FileDelete(string toDelete);

        DirectoryInfo DirectoryCreate(string toCreate);
        void DirectoryMove(string origin, string destination);
        void DirectoryDelete(string toDelete);
    }
}
