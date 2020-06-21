using System;
using System.IO;
using System.Linq;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

using Serilog;

using dackup.Configuration;

namespace dackup
{
    public static class PerformConfigHelper
    {
        public static PerformConfig LoadFrom(string fileName)
        {
            FileStream fs = null;
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(PerformConfig));
                              fs         = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                return (PerformConfig)serializer.Deserialize(fs);
            }
            catch (Exception exception)
            {
                Log.Error(Utils.FlattenException(exception));
                return null;
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                }
            }
        }

        public static void GenerateMockupConfig(string fileName)
        {
            WriteResourceToFile("dackup.perform-config-mockup.config", fileName);
        }
        private static void WriteResourceToFile(string resourceName, string fileName)
        {
            Console.WriteLine($"====> Write mockup file to {fileName}");
            using (var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                using (var file = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    resource.CopyTo(file);
                }
            }
        }
    }
}
