
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SHLib.Web.SplitTest
{
    public class ABAlternative
    {
        //public int AlternativeID { get; set; }
        //public int ExperimentID { get; set; }
        public string Content { get; set; }
        //public string Lookup { get; set; }
        public int Participants { get; set; }
        public SerializableDictionary<string, int> Conversions { get; set; }

        public int GetConversions(string goalKey)
        {

            return Conversions.ContainsKey(goalKey) ? Conversions[goalKey] : 0;

        }


        [XmlIgnore]
        public int Index { get; set; }

        public double ConversionRate(string goalKey)
        {

            return (double)(Conversions.ContainsKey(goalKey) ? Conversions[goalKey] : 0) / Participants;

        }

        public string PrettyConversionRate(string goalKey)
        {
            return (ConversionRate(goalKey) * 100).ToString("0.##") + "%";
        }


        public ABAlternative()
        {
        }

        public ABAlternative(string content)
        {
            Content = content;
            Conversions = new SerializableDictionary<string, int>();
        }

        public void ScoreParticipation()
        {
            Participants++;
        }

        public void ScoreConversion(string goalKey)
        {
            if (Conversions.ContainsKey(goalKey))
                Conversions[goalKey]++;
            else
            {
                Conversions[goalKey] = 1;
            }
        }
    }
}
