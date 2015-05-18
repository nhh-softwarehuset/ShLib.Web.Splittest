using System;
using System.IO;
using System.Web;
using System.Xml.Serialization;

namespace SHLib.Web.SplitTest.FileProvider
{
    public class ServerPathProvider : IPathProvider
    {
        
        private string AbsolutePath { get; set; }

        public void MapPath(string path)
        {
            AbsolutePath = HttpContext.Current.Server.MapPath(path ?? "~/tests.ab");

        }

        public string LoadContent()
        {
            if (AbsolutePath == null)
                MapPath(null);
            var content = string.Empty;
            try
            {


                using (FileStream fs = new FileStream(AbsolutePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (StreamReader reader = new StreamReader(fs))
                    {
                        content = reader.ReadToEnd();
                    }

                }
                return content;
            }
            catch (FileNotFoundException)
            {
                throw new FileNotFoundException(string.Format("could not find file: {0}", AbsolutePath));
            }
            catch (Exception e)
            {
            }
            return content;
        }

        public void SaveContent(string xml)
        {
            if (AbsolutePath == null)
                MapPath(null);
            try
            {
                using (TextWriter writer = new StreamWriter(AbsolutePath, false))
                {
                    writer.Write(xml);
                    writer.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("could not save content:\n" + xml + "\n\n" + ex);
            }
        }
    }
}