using System;
using System.IO;
using System.Text;
using System.Diagnostics;

using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;

namespace dackup
{
    public static class Utils
    {
        public static TimeSpan ConvertRemoveThresholdToTimeSpan(string timeSpan)
        {
            if (timeSpan.Length < 2)
            {
                throw new InvalidOperationException($"Invalid value for option: remove_threshold '{timeSpan}'");
            }

            var l = timeSpan.Length - 1;
            var value = timeSpan.Substring(0, l);
            var type = timeSpan.Substring(l, 1);

            switch (type)
            {
                case "d": return TimeSpan.FromDays(double.Parse(value));
                case "h": return TimeSpan.FromHours(double.Parse(value));
                case "m": return TimeSpan.FromMinutes(double.Parse(value));
                case "s": return TimeSpan.FromSeconds(double.Parse(value));
                case "f": return TimeSpan.FromMilliseconds(double.Parse(value));
                case "z": return TimeSpan.FromTicks(long.Parse(value));
                default: throw new InvalidOperationException($"Invalid value for remove_threshold option: '{timeSpan}'");
            }
        }
        public static DateTime ConvertRemoveThresholdToDateTime(string timeSpan)
        {
            return DateTime.Now - ConvertRemoveThresholdToTimeSpan(timeSpan);
        }
        public static void DirectoryCopy(string sourceDirectory, string targetDirectory)
        {
            DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
            DirectoryInfo diTarget = new DirectoryInfo(targetDirectory);

            CopyAll(diSource, diTarget);
        }
        private static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);

            foreach (FileInfo fi in source.GetFiles())
            {
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }
        // https://github.com/icsharpcode/SharpZipLib/wiki/GZip-and-Tar-Samples#createTGZ
        public static void CreateTarGZ(string tgzFilename, string sourceDirectory)
        {
            Stream outStream = File.Create(tgzFilename);
            Stream gzoStream = new GZipOutputStream(outStream);
            TarArchive tarArchive = TarArchive.CreateOutputTarArchive(gzoStream);

            tarArchive.RootPath = sourceDirectory.Replace('\\', '/');
            if (tarArchive.RootPath.EndsWith("/"))
            {
                tarArchive.RootPath = tarArchive.RootPath.Remove(tarArchive.RootPath.Length - 1);
            }

            AddDirectoryFilesToTar(tarArchive, sourceDirectory, true);

            tarArchive.Close();
        }
        private static void AddDirectoryFilesToTar(TarArchive tarArchive, string sourceDirectory, bool recurse)
        {
            TarEntry tarEntry = TarEntry.CreateEntryFromFile(sourceDirectory);
            tarArchive.WriteEntry(tarEntry, false);

            string[] filenames = Directory.GetFiles(sourceDirectory);
            foreach (string filename in filenames)
            {
                tarEntry = TarEntry.CreateEntryFromFile(filename);
                tarArchive.WriteEntry(tarEntry, true);
            }

            if (recurse)
            {
                string[] directories = Directory.GetDirectories(sourceDirectory);
                foreach (string directory in directories)
                    AddDirectoryFilesToTar(tarArchive, directory, recurse);
            }
        }
        public static string FlattenException(Exception exception)
        {
            var stringBuilder = new StringBuilder();
            var footprints = GetAllFootprints(exception);

            while (exception != null)
            {
                stringBuilder.AppendLine(exception.Message);
                exception = exception.InnerException;
            }
            stringBuilder.AppendLine(footprints);

            return stringBuilder.ToString();
        }
        private static string GetAllFootprints(Exception exception)
        {
            if (exception == null)
            {
                return string.Empty;
            }
            var st = new StackTrace(exception, true);
            var frames = st.GetFrames();
            var traceString = new StringBuilder();

            foreach (var frame in frames)
            {
                if (frame.GetFileLineNumber() < 1)
                {
                    continue;
                }
                traceString.Append("File: " + frame.GetFileName());
                traceString.Append(", Method:" + frame.GetMethod().Name);
                traceString.Append(", LineNumber: " + frame.GetFileLineNumber());
                traceString.Append("  -->  ");
            }

            return traceString.ToString();
        }
    }
}