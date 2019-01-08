
using System;
using System.Linq;
using System.Collections.Generic;

namespace dackup
{
    public sealed class BackupContext
    {
        private static object _mutex = new object();
        private static BackupContext instance;

        private static List<string> generateFilesList = new List<string>();
        private BackupContext() { }
        private BackupContext(string logFile, string tmpPath)
        {
            this.LogFile = logFile;
            this.TmpPath = tmpPath;
        }
        public static BackupContext Create(string logFile, string tmpPath)
        {
            if (instance != null)
            {
                throw new InvalidOperationException("BackupContext already created - use BacupContext.Current to get");
            }
            else
            {
                lock (_mutex)
                {
                    if (instance == null)
                    {
                        instance = new BackupContext(logFile, tmpPath);
                    }
                }
            }
            return instance;
        }
        public static BackupContext Current
        {
            get
            {
                if (instance == null)
                {
                    throw new InvalidOperationException("BackupContext not created - use BacupContext.Create to create");
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
                return generateFilesList.Distinct().ToList();
            }
        }
        public void AddToGenerateFilesList(string fileName)
        {
            if(! generateFilesList.Contains(fileName))
            {
                generateFilesList.Add(fileName);
            }
        }
        public void AddToGenerateFilesList(List<string> filesList)
        {
            if( filesList == null)
            {
                return;
            }
            generateFilesList.AddRange(filesList);   
        }
    }
}