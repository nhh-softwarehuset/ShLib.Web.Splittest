using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHLib.Web.SplitTest.FileProvider
{
    public interface IPathProvider
    {

        string LoadContent();
        void SaveContent(string xml);
    }
}
