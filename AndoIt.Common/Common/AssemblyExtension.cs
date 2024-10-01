using System;
using System.IO;
using System.Reflection;
using AndoIt.Common.Common;

namespace AndoIt.Common
{
	public static class AssemblyExtension
	{
		public static DateTime GetLinkerTime(this Assembly assembly, TimeZoneInfo target = null)
		{
			var filePath = assembly.Location;
			const int c_PeHeaderOffset = 60;
			const int c_LinkerTimestampOffset = 8;

			var buffer = new byte[2048];

			using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
				stream.Read(buffer, 0, 2048);

			var offset = BitConverter.ToInt32(buffer, c_PeHeaderOffset);
			var secondsSince1970 = BitConverter.ToInt32(buffer, offset + c_LinkerTimestampOffset);
			var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

			var linkTimeUtc = epoch.AddSeconds(secondsSince1970);

			var tz = target ?? TimeZoneInfo.Local;
			var localTime = TimeZoneInfo.ConvertTimeFromUtc(linkTimeUtc, tz);

			return localTime;
		}

        public static DateTime GetBuildDate(this Assembly assembly)
        {
            const int PeHeaderOffset = 60;
            const int LinkerTimestampOffset = 8;

            string filePath = assembly.Location;

            if (!File.Exists(filePath))
                throw new FileNotFoundException("No se encontró el archivo del assembly.", filePath);

            byte[] buffer = new byte[2048];

            using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                stream.Read(buffer, 0, 2048);
            }

            int headerOffset = BitConverter.ToInt32(buffer, PeHeaderOffset);
            int timestampOffset = headerOffset + LinkerTimestampOffset;
            int secondsSince1970 = BitConverter.ToInt32(buffer, timestampOffset);

            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime buildDate = epoch.AddSeconds(secondsSince1970);

            return buildDate.ToLocalTime();
        }

        public static DateTime GetAssemblyCompilationTime(this Assembly assembly, TimeZoneInfo target = null)
		{
			//string codeBase = assembly.GetName().CodeBase;
			//string fullPath = codeBase.Replace("file:///", string.Empty);
			string fullPath = assembly.Location;
            DateTime fecha1 = File.GetCreationTime(fullPath).ToLocalTime();
			DateTime fecha2 = File.GetLastWriteTime(fullPath).ToLocalTime();
			DateTime fecha3 = GetBuildDate(assembly).ToLocalTime();
			DateTime fecha4 = GetLinkerTime(assembly).ToLocalTime();
            DateTime result = DateTimeExtension.ObtenerFechaMayor(new DateTime[] { fecha1, fecha2, fecha3, fecha4});
			return result;
		}

		public static string AssemblyDirectory(this Assembly assembly)
		{
			UriBuilder uri = new UriBuilder(assembly.CodeBase);
			string path = Uri.UnescapeDataString(uri.Path);
			return Path.GetDirectoryName(path);
		}

	}
}
