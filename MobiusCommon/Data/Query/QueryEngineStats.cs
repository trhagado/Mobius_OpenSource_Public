using Mobius.ComOps;

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Xml.Serialization;

namespace Mobius.Data
{
	/// <summary>
	/// MetaBrokerStats
	/// </summary>

	[DataContract]
	public class MetaBrokerStats
	{
		[DataMember]
		public string Label = ""; // label for broker, usually blank, defined for search steps

		[DataMember]
		public int MetatableRowCount = 0; // Number of QueryTable/Metatable rows for selected columns returned by broker

		[DataMember]
		public int OracleRowCount = 0; // DbCommand Read operations (partial or full Metatable row)

		[DataMember]
		public int Time = 0; // time in ms

		[DataMember]
		public int MultiPivot = 0; // 0 - not multipivot, 1 - key multipivot, 2 - secondary multipivot


// Serialization/deserialization

		public static string SerializeList(List<MetaBrokerStats> mbStats) 
		{
			StringBuilder sb = new StringBuilder();
			foreach (MetaBrokerStats s in mbStats)
			{
				if (sb.Length > 0) sb.Append(";");
				sb.Append(s.Serialize());
			}

			return sb.ToString();
		}

		public string Serialize()
		{
			bool includeLabel = ClientState.MobiusClientVersionIsAtLeast(4, 1);
			if (includeLabel)
				return Label + "\t" + MetatableRowCount.ToString() + "\t" + OracleRowCount + "\t" + Time + "\t" + MultiPivot; 

			else return MetatableRowCount.ToString() + "," + OracleRowCount + "," + Time + "," + MultiPivot;
		}

		public static List<MetaBrokerStats> DeserializeList(string txt)
		{
			List<MetaBrokerStats> mbStats = new List<MetaBrokerStats>();
			string[] sa = txt.Split(';');
			foreach (string s in sa)
			{
				mbStats.Add(Deserialize(s));
			}
			return mbStats;
		}

		public static MetaBrokerStats Deserialize(string txt)
		{
			MetaBrokerStats mbs = new MetaBrokerStats();
			string[] sa = txt.Split('\t');
			mbs.Label = sa[0];
			mbs.MetatableRowCount = int.Parse(sa[1]);
			mbs.OracleRowCount = int.Parse(sa[2]);
			mbs.Time = int.Parse(sa[3]);
			mbs.MultiPivot = int.Parse(sa[4]);
			return mbs;
		}

	}
	
}
