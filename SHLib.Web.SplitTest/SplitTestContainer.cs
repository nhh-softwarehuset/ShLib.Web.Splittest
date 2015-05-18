using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SHLib.Web.SplitTest.FileProvider;

namespace SHLib.Web.SplitTest
{
    public static class SplitTestContainer
    {
        private static SplitTester splitTester { get; set; }

        public static SplitTester Instance
        {
            get
            {
                if (splitTester == null)
                {
                    splitTester = new SplitTester(new ServerPathProvider(),"tests.ab");
                }
                return splitTester;
            }
        }
    }
}
