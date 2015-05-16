using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
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
    public class CreateSplitTest_Azure
    {

        private IPathProvider pathProvider;
        private string dataPath;

        [TestInitialize]
        public void InitMethod()
        {
            var blobStorage = @"DefaultEndpointsProtocol=https;AccountName=xxxxxxxxxxxxx;AccountKey=xxxxxxxxxxx+xxxxxxxxxxx==";
            pathProvider = new AzureBlobProvider("SplitTest", "test.ab", blobStorage);
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
        public void AzureWebsiteRegisterGoal()
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

        [TestMethod]
        public void InitSpeedtest()
        {
            var list = new List<int>();
            var goalName = "convertion";
            var testName = "Test";
            var a1 = "V1";
            var a2 = "V2";
  
            for (int i = 0; i < 50; i++)
            {
                var stopWatch = new Stopwatch();
                stopWatch.Start();
                var sTester = new SplitTester(pathProvider, dataPath);
                var variation = sTester.Test(testName, a1, a2);
                sTester.ForceSave();
                stopWatch.Stop();
                sTester.Score(testName, goalName);
                stopWatch.ElapsedMilliseconds.ToString();
                list.Add(Convert.ToInt32(stopWatch.ElapsedMilliseconds));
            }
            var average = list.Sum(x => x)/50;
            Assert.IsTrue(average < 500);
        }
    }
}
