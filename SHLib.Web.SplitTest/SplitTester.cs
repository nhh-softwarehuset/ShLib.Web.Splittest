using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using SHLib.Web.SplitTest.FileProvider;

namespace SHLib.Web.SplitTest
{
	public class SplitTester
	{

		private static string _absoluteDataPath;
		private DateTime _lastSave = DateTime.Now;
		private SerializableDictionary<string, ABTest> _tests;
	    private IPathProvider PathProvider { get; set; }


	    /// <summary>
	    /// This is the file path where we'll be storing our serialized data 
	    /// </summary>
	    public string DataPath { get; set; }

	    /// <summary>
	    /// These are bots that we want to ignore.  If any of these strings shows up in a useragent, we'll show the default option and not score the request.
	    /// </summary>
	    public List<string> Bots { get; set; }

	    /// <summary>
		/// All tests, in progress or otherwise
		/// </summary>
		public SerializableDictionary<string, ABTest> Tests
		{
			get 
			{
				if (_tests == null)
				{
					Load();
				}
				return _tests; 
			}
			set { _tests = value; }
		}

        public SplitTester(IPathProvider pathProvider, string dataPath = null, List<string> bots = null)
        {
            PathProvider = pathProvider;
            setBots(bots);
        }
        private void setBots(List<string> bots)
        {
            Bots = bots ?? new List<string> { "Googlebot", "Slurp", "msnbot", "nagios", "Baiduspider", "Sogou", "SiteUptime.com", "Python", "DotBot", "Feedfetcher", "Jeeves", };;
        }

	    #region public interface (for end users)

		/// <summary>
		/// This is the meat of the whole library, and likely the only method you'll need to call.
		/// Given a set of alternatives, pick one to always show for this user and show it.
		/// Remember this test by name.
		/// </summary>
		/// <param name="testName"></param>
		/// <param name="alternatives"></param>
		/// <returns></returns>
		public string Test(string testName, params string[] alternatives)
		{
			// TODO - short circuit

			ABTest test = GetOrCreateTest(testName, alternatives);
			ABAlternative choice = GetUserAlternative(test);

			return choice.Content;
		}

		/// <summary>
		/// Special case for when you just want to switch between two alternates in an "if" block.
		/// </summary>
		/// <param name="testName"></param>
		/// <returns></returns>
		public bool Test(string testName)
		{
			ABTest test = GetOrCreateTest(testName, "true", "false");
			ABAlternative choice = GetUserAlternative(test);
			return bool.Parse(choice.Content);
//			ABUser user = IdentifyUser();
//			return (user.ID % 2 == 0);
		}

	    /// <summary>
	    /// Mark this user as having converted for the specified tests.
	    /// </summary>
	    /// <param name="testNames"></param>
	    /// <param name="goalKey"></param>
	    public void Score(string testName, string goalKey)
		{

				ABUser user = IdentifyUser();

                if ( user.Conversions.ContainsKey(testName))
				{
					// not part of the test or already scored.
					return;
				}

                if (Tests.ContainsKey(testName))
				{
                    Tests[testName].Score(user, goalKey);

                    user.Conversions.Add(testName, goalKey);
					user.SaveToCookie();
				}

		}
		#endregion

		#region public helpers (for library code)


		/// <summary>
		/// Attempt to populate ourself from a saved file.  Will leave us with in a clean, empty state if the file is missing or corrupt.
		/// </summary>
		private void Load()
		{
		    
			_tests = new SerializableDictionary<string, ABTest>();
			if (File.Exists(_absoluteDataPath))
			{
				try
				{
					_tests = (SerializableDictionary<string, ABTest>)
						SerializationHelper.DeSerialize(PathProvider.LoadContent(), typeof (SerializableDictionary<string, ABTest>));
					return;
				}
				catch
				{
					// Don't sweat it if we can't parse the file.  It's not worth crashing the page load.
				}
			}

			// No saved data yet (or bad xml)
			Save();
		}

		/// <summary>
		/// Write serialized test data to a file on the server.  
		/// This can be called frequently, since it batches up saveable data and only writes it to disk once per minute.
		/// </summary>
		private void Save()
		{
			// NOTE: in high traffic situations, this will probably fall down.
			// If it becomes an issue, migrate to a Singleton instance and lock that while doing this check:
			if ((DateTime.Now - _lastSave).TotalMinutes > 1)
			{
				_lastSave = DateTime.Now;
				try
				{
					ForceSave();
				}
				catch
				{
					// TODO - add onException handler to SHLib.Web.SplitTest
					// We're only saving AB data.  Not worth complaining about if it fails.
				}
			}
		}


		/// <summary>
		/// Write serialized test data to a file on the server.  Right now.
		/// </summary>
		public void ForceSave()
		{
            PathProvider.SaveContent(SerializationHelper.Serialize(Tests));
		}

		/// <summary>
		/// Create a new test, or load an existing one.
		/// </summary>
		/// <param name="testName"></param>
		/// <param name="alternatives"></param>
		/// <returns></returns>
		public ABTest GetOrCreateTest(string testName, params string[] alternatives)
		{
			ABTest test;
			if (Tests.ContainsKey(testName))
			{
				test = Tests[testName];
			}
			else
			{
				test = new ABTest(testName, alternatives);
				Tests.Add(testName, test);
			}

			return test;
		}

        ///// <summary>
        ///// Create a new test, or load an existing one.
        ///// </summary>
        ///// <param name="testName"></param>
        ///// <param name="alternatives"></param>
        ///// <returns></returns>
        //public static ABTest GetOrCreateTest(string testName, ControlCollection alternatives)
        //{
        //    ABTest test;
        //    if (Tests.ContainsKey(testName))
        //    {
        //        test = Tests[testName];
        //    }
        //    else
        //    {
        //        string[] altNames = new string[alternatives.Count];
        //        for (int a = 0; a < alternatives.Count; a++)
        //        {
        //            Alternative alt = (Alternative)alternatives[a];
        //            if (!String.IsNullOrEmpty(alt.Name))
        //            {
        //                altNames[a] = alt.Name;
        //            }
        //            else
        //            {
        //                altNames[a] = "Alternative " + (a + 1);
        //            }
        //        }

        //        test = new ABTest(testName, altNames);
        //        Tests.Add(testName, test);
        //    }

        //    return test;
        //}

		/// <summary>
		/// Create a new test, or load an existing one.
		/// </summary>
		/// <param name="testName"></param>
		/// <param name="altCount"></param>
		/// <returns></returns>
		public ABTest GetOrCreateTest(string testName, int altCount)
		{
			ABTest test;
			if (Tests.ContainsKey(testName))
			{
				test = Tests[testName];
			}
			else
			{
				string[] alternatives = new string[altCount];
				for (int a = 0; a < altCount; a++)
				{
					alternatives[a] = "Alternative " + (a + 1);
				}
				test = new ABTest(testName, alternatives);
				Tests.Add(testName, test);
			}

			return test;
		}

		/// <summary>
		/// For the specified test, pick an alternative to always show this user, and return that alternative.
		/// </summary>
		/// <param name="test"></param>
		/// <returns></returns>
		public ABAlternative GetUserAlternative(ABTest test)
		{
			ABUser user = IdentifyUser();
			ABAlternative choice = test.GetUserAlternative(user.ID);

			if (!user.Tests.Contains(test.TestName) && !IsBotRequest())
			{
				choice.ScoreParticipation();

				// NOTE: If this runs into concurrency issues in high traffic, we'll probably want to move it out to a timer of some form.
				// For now though, it's probably safe here for most sites, since it's balling up changes and saving infrequently.
				Save();

				user.Tests.Add(test.TestName);
				user.SaveToCookie();
			}


			return choice;
		}
		#endregion

		#region private helpers
		/// <summary>
		/// Check the current request against our list of known Bot useragent signatures
		/// </summary>
		/// <returns></returns>
		private bool IsBotRequest()
		{
			if (HttpContext.Current == null
				|| HttpContext.Current.Request == null
				|| String.IsNullOrEmpty(HttpContext.Current.Request.UserAgent))
			{
				return true;
			}

			string userAgent = HttpContext.Current.Request.UserAgent;
			foreach (string botIdentifier in Bots)
			{
				if (userAgent.Contains(botIdentifier))
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// If we've seen this user before, pull his info from the cookie.  If not, start tracking him.
		/// </summary>
		/// <returns></returns>
		private static ABUser IdentifyUser()
		{
			ABUser user = ABUser.LoadFromCookie();
			return user;
		}
		#endregion
	}
}
