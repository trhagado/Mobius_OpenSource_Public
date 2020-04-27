using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Mobius.Data
{
	/// <summary>
	/// Execution stats for a QueryEngine instance
	/// </summary>

	public class QueryEngineStats
	{
		public int ExecutionCount = 0; // number of times engine executed
		public int QueryId = 0; // id of the query if stored in database
		public int Tables = 0; // number of tables in query
		public int Columns = 0; // total number of columns retrieved
		public int Criteria = 0; // total number of criteria in query
		public int PrepareTime = 0; // time to prepare query
		public int ExecuteTime = 0; // time for execute command and retrieval of keys
		public int KeyCount = 0; // number of keys
		public int KeyFetchTime = 0; // time to fetch keys
		public int FirstRowFetchTime = 0; // time to fetch first row
		public int TotalRowFetchTime = 0; // total time to fetch all rows for all keys
		public int TotalRowCount = 0; // total number of rows fetched

		public bool Cancelled = false; // search or retrieval was cancelled
		public string Message = ""; // any associated messages

		[XmlIgnore]
		public Dictionary<MetaBrokerType, int> BrokerTypeDict = new Dictionary<MetaBrokerType, int>();

		public string Serialize()
		{
			string s =
				ExecutionCount + "\t" +
				QueryId + "\t" +
				Tables + "\t" +
				Columns + "\t" +
				Criteria + "\t" +
				PrepareTime + "\t" +
				ExecuteTime + "\t" +
				KeyFetchTime + "\t" +
				KeyCount + "\t" +
				FirstRowFetchTime + "\t" +
				TotalRowFetchTime + "\t" +
				TotalRowCount + "\t" +
				Cancelled + "\t" +
				Message;

			return s;
		}

		public static QueryEngineStats Deserialize(string serializedString)
		{
			QueryEngineStats s = new QueryEngineStats();
			string[] sa = serializedString.Split('\t');
			int.TryParse(sa[0], out s.ExecutionCount);
			int.TryParse(sa[1], out s.QueryId);
			int.TryParse(sa[2], out s.Tables);
			int.TryParse(sa[3], out s.Columns);
			int.TryParse(sa[4], out s.Criteria);
			int.TryParse(sa[5], out s.PrepareTime);
			int.TryParse(sa[6], out s.ExecuteTime);
			int.TryParse(sa[7], out s.KeyFetchTime);
			int.TryParse(sa[8], out s.KeyCount);
			int.TryParse(sa[9], out s.FirstRowFetchTime);
			int.TryParse(sa[10], out s.TotalRowFetchTime);
			int.TryParse(sa[11], out s.TotalRowCount);
			bool.TryParse(sa[12], out s.Cancelled);
			s.Message = sa[13];

			return s;
		}
	}
}
