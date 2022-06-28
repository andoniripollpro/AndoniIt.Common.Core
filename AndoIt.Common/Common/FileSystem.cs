using System;
using System.IO;
using System.Linq;

namespace AndoIt.Common
{
    public class FileSystem 
    {
        public DateTime LastModifiedFileDate(string folderBaseAddress)
        { 
            DateTime lastDatetimeModified = DateTime.MinValue;
            
            var directoryInfo = new DirectoryInfo(folderBaseAddress);
            var files = directoryInfo.GetFiles().Where(f => !f.Attributes.HasFlag(FileAttributes.Hidden)).ToList();
            files.ForEach(f => {
                DateTime lastDatetimeModifiedInLoop = new FileInfo(f.FullName).LastWriteTime;
                if (lastDatetimeModifiedInLoop > lastDatetimeModified)
                    lastDatetimeModified = lastDatetimeModifiedInLoop;
            });

            var folders = new DirectoryInfo(folderBaseAddress).GetDirectories().Where(x => (x.Attributes & FileAttributes.Hidden) == 0).ToList();
            foreach (var f in folders)
            {
                // Recursive
                DateTime lastDatetimeModifiedInLoop = LastModifiedFileDate(f.FullName); 
                if (lastDatetimeModifiedInLoop > lastDatetimeModified)
                    lastDatetimeModified = lastDatetimeModifiedInLoop;
            }

            return lastDatetimeModified;
        }

        public static void WriteAllTextSafe(string fileCompletePath, string fileContent)
        {
            if (!Directory.Exists(Path.GetDirectoryName(fileCompletePath))) 
                Directory.CreateDirectory(Path.GetDirectoryName(fileCompletePath));
            File.WriteAllText(fileCompletePath, fileContent);
        }
    }    
}
