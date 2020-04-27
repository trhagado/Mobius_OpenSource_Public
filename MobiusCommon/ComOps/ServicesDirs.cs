

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mobius.ComOps
{

	/// <summary>
	/// Common services directory locations
	/// </summary>

	public class ServicesDirs
	{
		public static string MetaDataDir; // folder for metadata file
		public static string DocDir; // folder for documents
		public static string LogDir; // folder for writing logs
		public static string CacheDir; // folder for caching files used to increase performance
		public static string TempDir; // folder for temp files
		public static string TargetMapsDir; // folder for target maps

		public static string QueryResultsDataSetDir; // folder for query results datasets
		public static string BackgroundExportDir; // folder for background exports
		public static string BinaryDataDir; // folder containing binary data used to speed query execution

		public static string ModelQueriesDir; // folder for queries
		public static string MiscConfigDir => CommonConfigInfo.MiscConfigDir;
		public static string ImageDir;
		public static string ImageDirUNC;

		public static string ExecutablePath; // path to executable
		public static string ServerStartupDir; // directory containing executable
	}
}
