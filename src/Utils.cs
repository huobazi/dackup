using System;
using System.IO;
using System.Text;
using System.Net;
using System.Diagnostics;
using System.Collections.Generic;

using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;

namespace Dackup
{
    public static class Utils
    {
        public static TimeSpan ConvertRemoveThresholdToTimeSpan(string timeSpan)
        {
            var invalidTimeSpanExceptionMessage = $"Invalid value for remove_threshold option: '{timeSpan}'";
            if (timeSpan.Length < 2)
            {
                throw new InvalidOperationException(invalidTimeSpanExceptionMessage);
            }

            var l     = timeSpan.Length - 1;
            var value = timeSpan.Substring(0, l);
            var type  = timeSpan.Substring(l, 1);

            switch (type)
            {
                case "d"    : return TimeSpan.FromDays(double.Parse(value));
                case "h"    : return TimeSpan.FromHours(double.Parse(value));
                case "m"    : return TimeSpan.FromMinutes(double.Parse(value));
                case "s"    : return TimeSpan.FromSeconds(double.Parse(value));
                case "f"    : return TimeSpan.FromMilliseconds(double.Parse(value));
                case "z"    : return TimeSpan.FromTicks(long.Parse(value));
                     default: throw new InvalidOperationException(invalidTimeSpanExceptionMessage);
            }
        }
        public static DateTime ConvertRemoveThresholdToDateTime(string timeSpan)
        {
            return DateTime.Now - ConvertRemoveThresholdToTimeSpan(timeSpan);
        }
        public static void DirectoryCopy(string sourceDirectory, string targetDirectory, List<string> excludes = null)
        {
            DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
            DirectoryInfo diTarget = new DirectoryInfo(targetDirectory);

            CopyAll(diSource, diTarget, excludes);
        }
        private static void CopyAll(DirectoryInfo source, DirectoryInfo target, List<string> excludes = null)
        {
            if (!IsDirectoryExcluded(source.FullName, excludes))
            {
                Directory.CreateDirectory(target.FullName);

                foreach (FileInfo fi in source.GetFiles())
                {
                    string destFile = Path.Combine(target.FullName, fi.Name);
                    FileCopy(fi.FullName, destFile, excludes);
                }

                foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
                {
                    DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                    CopyAll(diSourceSubDir, nextTargetSubDir, excludes);
                }
            }

            bool IsDirectoryExcluded(string directoryPath, List<string> excludeList)
            {
                if (excludeList == null || excludeList.Count <= 0)
                {
                    return false;
                }
                else if (excludeList.Contains(directoryPath))
                {
                    return true;
                }

                return false;
            }
        }
        public static void FileCopy(string sourceFile, string destFile, List<string> excludes = null)
        {
            if (!IsFileExcluded(sourceFile, excludes))
            {
                File.Copy(sourceFile, destFile, true);
            }

            bool IsFileExcluded(string fileName, List<string> excludeList)
            {
                if (excludeList == null || excludeList.Count <= 0)
                {
                    return false;
                }
                else if (excludeList.Contains(fileName))
                {
                    return true;
                }

                var extension = Path.GetExtension(fileName);
                if (!string.IsNullOrEmpty(extension))
                {
                    return excludeList.Contains("*" + extension);
                }

                return false;
            }
        }
        // https://github.com/icsharpcode/SharpZipLib/wiki/GZip-and-Tar-Samples#createTGZ
        public static void CreateTarGZ(string tgzFilename, string sourceDirectory)
        {
            Stream     outStream  = File.Create(tgzFilename);
            Stream     gzoStream  = new GZipOutputStream(outStream);
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
                {
                    AddDirectoryFilesToTar(tarArchive, directory, recurse);
                }
            }
        }

        public static void CreateTarGZ(List<string> sourceFileList, string tgzFilename)
        {
            using (FileStream fs = new FileStream(tgzFilename, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (Stream gzipStream = new GZipOutputStream(fs))
                {
                    using (TarArchive tarArchive = TarArchive.CreateOutputTarArchive(gzipStream))
                    {
                        foreach (string filename in sourceFileList)
                        {
                            {
                                TarEntry tarEntry      = TarEntry.CreateEntryFromFile(filename);
                                         tarEntry.Name = Path.GetFileName(filename);
                                tarArchive.WriteEntry(tarEntry, false);
                            }
                        }
                    }
                }
            }
        }
        public static string FlattenException(Exception exception)
        {
            var stringBuilder = new StringBuilder();
            var footprints    = GetAllFootprints(exception);

            while (exception != null)
            {
                stringBuilder.AppendLine(exception.Message);
                exception = exception.InnerException;
            }
            stringBuilder.AppendLine(footprints);

            return stringBuilder.ToString();

            string GetAllFootprints(Exception ex)
            {
                if (ex == null)
                {
                    return string.Empty;
                }
                var st          = new StackTrace(ex, true);
                var frames      = st.GetFrames();
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
        public static bool IsLocalhost(string hostNameOrAddress)
        {
            if (string.IsNullOrEmpty(hostNameOrAddress))
            {
                return false;
            }
            try
            {
                IPAddress[] hostIPs  = Dns.GetHostAddresses(hostNameOrAddress);
                IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
                foreach (IPAddress hostIP in hostIPs)
                {
                    if (IPAddress.IsLoopback(hostIP)) return true;
                    foreach (IPAddress localIP in localIPs)
                    {
                        if (hostIP.Equals(localIP)) return true;
                    }
                }
            }
            catch
            {
                return false;
            }
            return false;
        }

    }
}