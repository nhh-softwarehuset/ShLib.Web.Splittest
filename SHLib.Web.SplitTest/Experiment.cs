using System;
using System.Collections.Generic;

namespace SHLib.Web.SplitTest
{
	public class Experiment
	{
		public int ExperimentID { get; set; }
		public string TestName { get; set; }
		public string Status { get; set; }
		public DateTime CreatedOn { get; set; }
		public List<Alternative> Alternatives { get; set; }

		public Experiment()
		{
			CreatedOn = DateTime.Now;
			Alternatives = new List<Alternative>();
		}

		public Experiment(string testName, string conversionName, params string[] alternatives) : this()
		{
			TestName = testName;
			foreach (string alt in alternatives)
			{
				Alternatives.Add(new Alternative(alt));
			}
		}

		public Alternative GetUserAlternative(string userID)
		{
			int id = StringConvert.ToInt32(userID, 0);
			return Alternatives[id % Alternatives.Count];
		}
	}
}
