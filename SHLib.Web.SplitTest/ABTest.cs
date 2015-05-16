using System;
using System.Collections.Generic;
using System.Text;

namespace SHLib.Web.SplitTest
{
    public class ABTest
    {
        public string TestName { get; set; }
        public string Status { get; set; }
        public DateTime CreatedOn { get; set; }
        public List<ABAlternative> Alternatives { get; set; }

        /// <summary>
        /// A list of users participating in this test
        /// </summary>
        public int Participants
        {
            get
            {
                int count = 0;
                foreach (ABAlternative alt in Alternatives)
                {
                    count += alt.Participants;
                }

                return count;
            }
        }

        /// <summary>
        /// Count of total conversions for this test, regardless of outcome.
        /// </summary>
        public int Conversions(string key)
        {

            int count = 0;
            foreach (ABAlternative alt in Alternatives)
            {
                count += alt.Conversions[key];
            }

            return count;

        }

        /// <summary>
        /// Rate of total conversions for this test, regardless of outcome.
        /// </summary>
        public double ConversionRate(string key)
        {
            return (double)Conversions(key) / Participants;
        }

        /// <summary>
        /// Rate of total conversions for this test, regardless of outcome, formatted like "3.22%"
        /// </summary>
        public string PrettyConversionRate(string key)
        {
            return (ConversionRate(key) * 100).ToString("0.##") + "%";
        }

        public ABTest()
        {
            CreatedOn = DateTime.Now;
            Alternatives = new List<ABAlternative>();
        }

        public ABTest(string testName)
            : this()
        {
            TestName = testName;
        }

        public ABTest(string testName, params string[] alternatives)
            : this(testName)
        {
            foreach (string alt in alternatives)
            {
                Alternatives.Add(new ABAlternative(alt));
            }
        }

        /// <summary>
        /// Given a userID, return the appropriate alternative.
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public ABAlternative GetUserAlternative(int userID)
        {
            int index = userID % Alternatives.Count;
            ABAlternative choice = Alternatives[index];
            choice.Index = index;

            return choice;
        }

        /// <summary>
        /// Score a conversion for this test for the specified user
        /// </summary>
        /// <param name="user"></param>
        public void Score(ABUser user, string key)
        {
            GetUserAlternative(user.ID).ScoreConversion(key);
        }

        #region statistics
        /*
		 * NOTE: This is all lifted directly from the ABingo source:  http://www.bingocardcreator.com/abingo
		 * 
		 */

        private static readonly double[,] ZScores = { { 0.10, 1.29 }, { 0.05, 1.65 }, { 0.01, 2.33 }, { 0.001, 3.08 } };
        private static readonly Dictionary<double, string> Percentages = new Dictionary<double, string> { { 0.10, "90%" }, { 0.05, "95%" }, { 0.01, "99%" }, { 0.001, "99.9%" } };
        private static readonly Dictionary<double, string> Descriptions = new Dictionary<double, string> { { 0.10, "fairly confident" }, { 0.05, "confident" }, { 0.01, "very confident" }, { 0.001, "extremely confident" } };

        private double GetZScore(string key)
        {
            if (Alternatives.Count != 2)
            {
                throw new Exception("Sorry, can't currently automatically calculate statistics for A/B tests with > 2 alternatives.");
            }
            if (Alternatives[0].Participants == 0 || Alternatives[1].Participants == 0)
            {
                throw new Exception("Can't calculate the z score if either of the alternatives lacks participants.");
            }

            double cr1 = Alternatives[0].ConversionRate(key);
            double cr2 = Alternatives[1].ConversionRate(key);

            int n1 = Alternatives[0].Participants;
            int n2 = Alternatives[1].Participants;

            double numerator = cr1 - cr2;
            double frac1 = cr1 * (1 - cr1) / n1;
            double frac2 = cr2 * (1 - cr2) / n2;

            return numerator / Math.Pow((frac1 + frac2), 0.5);
        }

        public double GetPValue(string key)
        {
            double z = GetZScore(key);
            z = Math.Abs(z);

            for (int a = 0; a < ZScores.Length / 2; a++)
            {
                if (z > ZScores[a, 1])
                {
                    return ZScores[a, 0];
                }
            }

            return 1;
        }

        public bool IsStatisticallySignificant(string key)
        {
            return IsStatisticallySignificant(0.05, key);
        }

        public ABAlternative GetBestAlternative(string key)
        {
            ABAlternative best = null;
            foreach (ABAlternative alt in Alternatives)
            {
                if (best == null || alt.ConversionRate(key) > best.ConversionRate(key))
                {
                    best = alt;
                }
            }

            return best;
        }

        public ABAlternative GetWorstAlternative(string key)
        {
            ABAlternative best = null;
            foreach (ABAlternative alt in Alternatives)
            {
                if (best == null || alt.ConversionRate(key) <= best.ConversionRate(key))
                {
                    best = alt;
                }
            }

            return best;
        }

        public bool IsStatisticallySignificant(double p, string key)
        {
            return GetPValue(key) <= p;
        }

        public string GetResultDescription(string key)
        {
            double p;
            try
            {
                p = GetPValue(key);
            }
            catch (Exception e)
            {
                return e.Message;
            }

            StringBuilder builder = new StringBuilder();
            if (Alternatives[0].Participants < 10 || Alternatives[1].Participants < 10)
            {
                builder.Append("Take these results with a grain of salt since your samples are so small: ");
            }

            ABAlternative best = GetBestAlternative(key);
            ABAlternative worst = GetWorstAlternative(key);

            builder.Append(String.Format(@"
				The best alternative you have is: [{0}], which had 
				{1} conversions from {2} participants 
				({3}).  The other alternative was [{4}], 
				which had {5} conversions from {6} participants 
				({7}).  "
                , best.Content
                , best.Conversions[key]
                , best.Participants
                , best.PrettyConversionRate(key)
                , worst.Content
                , worst.Conversions[key]
                , worst.Participants
                , worst.PrettyConversionRate(key)
                ));


            if (p == 1)
            {
                builder.Append("However, this difference is not statistically significant.");
            }
            else
            {
                builder.Append(String.Format(@"
					This difference is <b>{0} likely to be statistically significant</b>, which means you can be 
					{1} that it is the result of your alternatives actually mattering, rather than 
					being due to random chance.  However, this statistical test can't measure how likely the currently 
					observed magnitude of the difference is to be accurate or not.  It only says ""better"", not ""better 
					by so much"".  "
                    , Percentages[p]
                    , Descriptions[p]
                    ));
            }


            return builder.ToString();

        }


        #endregion
    }
}
