using System.Web;

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
            throw new System.NotImplementedException();
        }

        public void SaveContent(string xml)
        {
            throw new System.NotImplementedException();
        }
    }
}