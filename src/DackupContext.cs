using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace Dackup
{
    public sealed class DackupContext
    {
        private static object _mutex = new object();
        private static DackupContext instance;
        private static List<string> generateFilesList = new List<string>();
        private DackupContext() { }
        private DackupContext(string logFile, string tmpPath)
        {
            this.LogFile = logFile;
            this.TmpPath = Path.Combine(tmpPath, $"dackup-tmp-{DateTime.UtcNow:s}");
        }
        public static DackupContext Create(string logFile, string tmpPath)
        {
            if (instance != null)
            {
                throw new InvalidOperationException("DackupContext already created - use BacupContext.Current to get");
            }
            else
            {
                lock (_mutex)
                {
                    if (instance == null)
                    {
                        instance = new DackupContext(logFile, tmpPath);
                    }
                }
            }
            return instance;
        }
        public static DackupContext Current
        {
            get
            {
                if (instance == null)
                {
                    throw new InvalidOperationException("DackupContext not created - use BacupContext.Create to create");
                }
                return instance;
            }
        }
        public string LogFile { get; private set; }
        public string TmpPath { get; private set; }
        public List<string> GenerateFilesList 
        {
            get
            {
                return generateFilesList.Distinct().OrderBy(c=>c).ToList();
            }
        }
        public void AddToGenerateFilesList(string fileName)
        {
            if(! generateFilesList.Contains(fileName))
            {
                generateFilesList.Add(fileName);
            }
        }
        public void AddToGenerateFilesList(IEnumerable<string> filesList)
        {
            if( filesList == null)
            {
                return;
            }
            generateFilesList.AddRange(filesList);   
        }
        public static void resetForTesting()
        {
        #if (DEBUG )
            instance = null;
        #else
            throw new InvalidOperationException("This method can only called on testting ");
        #endif
        }
    }
}