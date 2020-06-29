using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using dackup;

using Xunit;

namespace dackup.Tests
{
    public class DackupContext_Tests
    {
        [Fact]
        public void Instance_Duplicate_Creation_Test()
        {
            DackupContext.resetForTesting();
            DackupContext.Create("/log", "/tmp");
            Exception ex = Assert.Throws<InvalidOperationException>(() => DackupContext.Create("/log2", "/tmp2"));
            Assert.Equal("DackupContext already created - use BacupContext.Current to get", ex.Message);
        }
        
        [Fact]
        public void Property_Return_Test()
        {
            DackupContext.resetForTesting();
            DackupContext.Create("/log", "/tmp");
            
            Assert.Equal("/log",DackupContext.Current.LogFile);
            Assert.Equal(Path.Combine("/tmp", $"dackup-tmp-{DateTime.UtcNow:s}"),DackupContext.Current.TmpPath);
        }

        [Fact]
        public void GenerateFilesList_Test()
        {
            DackupContext.resetForTesting();
            DackupContext.Create("/log", "/tmp");

            DackupContext.Current.AddToGenerateFilesList("/tmp0");
            DackupContext.Current.AddToGenerateFilesList("/tmp1");
            DackupContext.Current.AddToGenerateFilesList("/tmp0");
            DackupContext.Current.AddToGenerateFilesList("/tmp1");
            DackupContext.Current.AddToGenerateFilesList(new[]{"/tmp3","/tmp2"});

            Assert.Equal(new List<string>{"/tmp0","/tmp1","/tmp2","/tmp3"}, DackupContext.Current.GenerateFilesList);
        }
    }
}
