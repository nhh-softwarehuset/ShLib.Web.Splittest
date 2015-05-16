using System.IO;
using SHLib.Web.SplitTest;
using SHLib.Web.SplitTest.FileProvider;

namespace Splittest.Tests
{
    public class TestPathProvider : IPathProvider
    {
        private string basePath = string.Empty;
        public void MapPath(string path)
        {
            basePath = Path.Combine(@"C:\", path);
        }

        public string LoadContent()
        {
            if (File.Exists(basePath))
            {
                using (StreamReader streamReader = new StreamReader(basePath))
                {
                    return streamReader.ReadToEnd();
                }
            }
            return null;
        }

        public void SaveContent(string xml)
        {
            SerializationHelper.SerializeToFile(xml, basePath);
        }
    }
}