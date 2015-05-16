using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.SessionState;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SHLib.Web.SplitTest;
using SHLib.Web.SplitTest.FileProvider;

namespace Splittest.Tests
{
    [TestClass]
    public class CreateSplitTest
    {

        private TestPathProvider pathProvider;
        private string dataPath;

        [TestInitialize]
        public void InitMethod()
        {
            pathProvider = new TestPathProvider();
            dataPath  = "test.ab";
            //RemoveExistingData();

            var httpRequest = new HttpRequest("", "http://mySomething/", "");
            var stringWriter = new StringWriter();
            var httpResponce = new HttpResponse(stringWriter);
            var httpContext = new HttpContext(httpRequest, httpResponce);

            var sessionContainer = new HttpSessionStateContainer("id", new SessionStateItemCollection(),
                                                                 new HttpStaticObjectsCollection(), 10, true,
                                                                 HttpCookieMode.AutoDetect,
                                                                 SessionStateMode.InProc, false);

            httpContext.Items["AspSession"] = typeof(HttpSessionState).GetConstructor(
                                                     BindingFlags.NonPublic | BindingFlags.Instance,
                                                     null, CallingConventions.Standard,
                                                     new[] { typeof(HttpSessionStateContainer) },
                                                     null)
                                                .Invoke(new object[] { sessionContainer });

            HttpContext.Current = httpContext;
            
        }



        //private void RemoveExistingData() {
        //    if (File.Exists(pathProvider.MapPath(dataPath)))
        //    {
        //        File.Delete(pathProvider.MapPath(dataPath));
        //    }
        //}

        [TestMethod]
        public void TestMethod1()
        {            
            var goalName = "convertion";
            var testName = "Test";
            var a1 = "V1";
            var a2 = "V2";
            var sTester = new SplitTester(pathProvider, dataPath);
            var variation = sTester.Test(testName, a1, a2);



            sTester.Score(testName, goalName);
            sTester.ForceSave();

            var alternatives = sTester.Tests[testName].Alternatives;
            var alternative = alternatives.FirstOrDefault(x => x.Content == variation);
            Assert.AreEqual(alternative.GetConversions(goalName), 1);
            var falseAlternative = alternatives.FirstOrDefault(x => x.Content != variation);
            Assert.AreNotEqual(falseAlternative.GetConversions(goalName), 1);
        }
    }
}
