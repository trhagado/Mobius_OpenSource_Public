using Mobius.ComOps;
using Mobius.SpotfireDocument;

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Windows.Forms;
using System.IO;

namespace Mobius.Data
{

/// <summary>
/// Spotfire View Properties for mapping to a Spotfire Analysis/Visualizations
/// </summary>

	public class SpotfireViewProps
	{
		public ResultsViewProps ResultsViewProps; // parent ResultsViewProps that this instance belongs to

		public Query Query => ResultsViewProps?.BaseQuery; // Mobius query that supplies data to the Spotfire view

		public string WebplayerUrl = ""; // server the analysis is on
		public string AnalysisPath = ""; // path to the analysis in the library
		public string Description = "";

		public DataTableMapsMsx DataTableMaps = null;  // List of mappings from Mobius query to Spotfire DataTables and DataColumns that this view references

		/*** Spotfire Document the view is going against ***/

		public AnalysisApplicationMsx AnalysisApp = null; // current Analysis App instance

		public DocumentMsx Doc // associated Spotfire document
		{
			get { return AnalysisApp?.Document; }
			set { if (AnalysisApp != null) AnalysisApp.Document = value; else throw new Exception("Spotfire AnalysisApp is null"); }
		}

		public VisualMsx ActiveVisual {  // currently active visual
			get { return Doc?.ActiveVisualReference; } 
			set { if (Doc != null)  Doc.ActiveVisualReference = value; else throw new Exception("Spotfire Document is null"); }
		}

		public string ParameterString = ""; // parameters specific to the configuration of this specific view instance

		//[Obsolete("Deprecated")]
		public UserObject UserObject = new UserObject(UserObjectType.SpotfireLink); // associated user object for SpotfireLinks

		//[Obsolete("Deprecated")]
		public Dictionary<string, SpotfireLinkParameter> SpotfireLinkParameters = new Dictionary<string, SpotfireLinkParameter>(StringComparer.OrdinalIgnoreCase); // dictionary of parameters specific to the configuration of this specific view instance


		/****************************************************************************************/
		/*************** Static WebPlayer and Spotfire data file UNC and URL paths **************/
		/****************************************************************************************/

		public static string SpotfireDataFolder = @"\\[server]\Mobius\MobiusQueryExecutorApp\SpotfireData"; // folder that Spotfire data files should be written to
		public static string SpotfireDataFolderUrl = "http://[server]/MobiusQueryExecutorApp/SpotfireData"; // url to folder containing files

		public static string DefaultWebplayerUrl => GetDefaultWebPlayerUrl();

		/**************************************************************************/
		/******************************* Methods **********************************/
		/**************************************************************************/

		/// <summary>
		/// Basic constructor
		/// </summary>
		public SpotfireViewProps(ResultsViewProps rvp)
		{
			ResultsViewProps = rvp; // link to parent ResultsViewProps

			if (rvp != null) rvp.SpotfireViewProps = this; // link rvp to us

			DataTableMaps = new DataTableMapsMsx(this); // allocate maps and point back up to us
			return;
		}

		/// <summary>
		/// GetDefaultWebPlayerUrl
		/// </summary>
		/// <returns></returns>

		static string GetDefaultWebPlayerUrl()
		{
			if (_defaultWebplayerUrl == null)
				_defaultWebplayerUrl = ServicesIniFile.Read("SpotfireWebplayer", "https://[server]/spotfire/wp");

			return _defaultWebplayerUrl;
		}
		static string _defaultWebplayerUrl = null;

		/// <summary>
		/// Get base file names for exports of query results to Spotfire
		/// </summary>
		/// <returns></returns>

		public static string GetBaseExportFileName(Query query)
		{
			if (Lex.IsUndefined(query.SpotfireDataFileName))
			{
				query.SpotfireDataFileName =
					ClientState.UserInfo.UserName + "_" +
					query.UserObject.Id + "_" +
					Guid.NewGuid();
			}

			return query.SpotfireDataFileName;
		}

		/// <summary>
		/// GetExportFileUrlFromBaseFileName
		/// </summary>
		/// <param name="baseFileName"></param>
		/// <returns></returns>

		public static string GetFullExportFileUrlFromBaseFileName (
			string baseFileName)
		{
			baseFileName = GetExportBaseFileNameFromPath(baseFileName);
			if (Lex.IsUndefined(baseFileName)) return "";

			string url = SpotfireDataFolderUrl + "/" + baseFileName + ".stdf"; // full url assuming stdf output
			return url;
		}

		/// <summary>
		/// GetFullExportFilePathFromBaseFileName
		/// </summary>
		/// <param name="baseFileName"></param>
		/// <returns></returns>

		public static string GetFullExportFilePathFromBaseFileName (
			string baseFileName)
		{
			baseFileName = GetExportBaseFileNameFromPath(baseFileName);
			if (Lex.IsUndefined(baseFileName)) return "";

			string path = SpotfireDataFolder + @"\" + baseFileName + ".stdf"; // full path assuming stdf output
			return path;
		}

		/// <summary>
		/// GetExportBaseFileNameFromPath
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>

		public static string GetExportBaseFileNameFromPath (
			string path)
		{
			string fileName = Path.GetFileName(path);
			return fileName;
		}

		/// <summary>
		/// Validate mapping against the associated Spotfire analysis document
		/// </summary>
		/// <param name="svp"></param>

		public void ValidateMapsAgainstAnalysisDocument()
		{
			if (DataTableMaps == null || DataTableMaps.Count == 0)
			{
				DataTableMaps = new DataTableMapsMsx(this);
				DataTableMaps.AssignInitialMapping();
				return;
			}

			DataTableMaps.ValidateMapsAgainstAnalysisDocument();
			return;
		}

		/// <summary>
		/// Get bool Spotfire Parameter
		/// </summary>
		/// <param name="parmName"></param>
		/// <returns></returns>

		public bool IsParameterValueTrue(string parmName)
		{

			if (SpotfireLinkParameters.ContainsKey(parmName.ToUpper()))
			{
				string val = SpotfireLinkParameters[parmName.ToUpper()].Value;
				bool b = false;

				bool.TryParse(val, out b);
				return b;
			}

			else return false;
		}

/// <summary>
/// Set bool Spotfire Parameter
/// </summary>
/// <param name="parmName"></param>
/// <param name="value"></param>

		public void SetParameter(
			string parmName,
			bool value)
		{
			SpotfireLinkParameter p;

			if (SpotfireLinkParameters.ContainsKey(parmName.ToUpper()))
			{
				p = SpotfireLinkParameters[parmName.ToUpper()];
				p.Value = value.ToString();
			}

			else 
				p = new SpotfireLinkParameter(parmName, value.ToString());

			return;
		}

/// <summary>
/// Get the Webplayer URL to the analysis
/// Example: https://[server]/spotfire/wp/ViewAnalysis.aspx?file=/Mobius/Visualizations/CMN_ASSY_ATRBTS
/// </summary>
/// <returns></returns>

		public string GetWebplayerUrlOfAnalysis()
		{
			string url = "";
			if (Lex.IsDefined(WebplayerUrl)) url = WebplayerUrl;
			else url = DefaultWebplayerUrl;
			if (!url.EndsWith("/")) url += "/";
			url += "ViewAnalysis.aspx?file=" + AnalysisPath;
			return url;
		}

		/// <summary>
		/// Serialize Spotfire link
		/// </summary>
		/// <returns></returns>
		 
		public string Serialize()
		{
			XmlMemoryStreamTextWriter mstw = new XmlMemoryStreamTextWriter();
			mstw.Writer.Formatting = Formatting.Indented;
			Serialize(mstw.Writer);
			string xml = mstw.GetXmlAndClose();
			return xml;
		}

		public void Serialize(
			XmlTextWriter tw)
		{
			tw.WriteStartElement("SpotfireViewProps");
			XmlUtil.WriteAttributeIfDefined(tw, "WebplayerUrl", WebplayerUrl);
			XmlUtil.WriteAttributeIfDefined(tw, "AnalysisPath", AnalysisPath);
			XmlUtil.WriteAttributeIfDefined(tw, "Description", Description);
			XmlUtil.WriteAttributeIfDefined(tw, "ParameterString", ParameterString);

			foreach (SpotfireLinkParameter p in SpotfireLinkParameters.Values)
			{
				tw.WriteStartElement("SpotfireLinkParameter");
				tw.WriteAttributeString("Name", p.Name);
				tw.WriteAttributeString("Value", p.Value);
				tw.WriteEndElement(); 
			}

			if (DataTableMaps != null)
				DataTableMaps.Serialize(tw);

			tw.WriteEndElement(); 
			return;
		}

		/// <summary>
		/// Deserialize SpotfireViewProp
		/// </summary>
		/// <param name="q"></param>
		/// <param name="tr"></param>
		/// <param name="view"></param>
		/// <returns></returns>

		public static SpotfireViewProps DeserializeSpotfireProperties(
			Query q,
			XmlTextReader tr,
			ResultsViewProps view)
		{
			Enum iEnum = null;

			SpotfireViewProps svp = new SpotfireViewProps(view);

			XmlUtil.GetStringAttribute(tr, "WebPlayerUrl", ref svp.WebplayerUrl);
			XmlUtil.GetStringAttribute(tr, "AnalysisPath", ref svp.AnalysisPath);
			XmlUtil.GetStringAttribute(tr, "Description", ref svp.Description);
			XmlUtil.GetStringAttribute(tr, "ParameterString", ref svp.ParameterString);

			while (true)
			{
				if (!XmlUtil.MoreSubElements(tr, "SpotfireViewProps")) break;

				else if (Lex.Eq(tr.Name, "DataTableMaps"))
					DataTableMapsMsx.Deserialize(view.BaseQuery, tr, svp);

				else if (Lex.Eq(tr.Name, "SpotfireLinkParameter"))
					DeserializeSpotfireLinkParameter(tr, svp);


				else throw new Exception("Unexpected element: " + tr.Name);
			} 

			return svp;
		}

		/// <summary>
		/// Deserialize a ColumnMapMsx
		/// </summary>
		/// <param name="tr"></param>
		/// <returns></returns>

		internal static void DeserializeSpotfireLinkParameter(
			XmlTextReader tr,
			SpotfireViewProps svp)
		{
			XmlUtil.VerifyElementName(tr, "SpotfireLinkParameter");

			SpotfireLinkParameter p = new SpotfireLinkParameter();
			if (XmlUtil.GetStringAttribute(tr, "Name", ref p.Name) &&
				XmlUtil.GetStringAttribute(tr, "Value", ref p.Value))
					svp.SpotfireLinkParameters[p.Name] = p;

			XmlUtil.VerifyAtEndOfElement(tr, "SpotfireLinkParameter");

			return;
		}

		/// <summary>
		/// Deserialize from MetaTable TableMap
		/// </summary>
		/// <param name="mt"></param>
		/// <returns></returns>

		public static SpotfireViewProps DeserializeSqlProcedureVisualization(
			MetaTable mt)
		{
			SpotfireViewProps sl = Deserialize(mt.Code); // deserialize link definition which is stored in the metatable code field
			return sl;
		}

		/// <summary>
		/// Deserialize Spotfire link
		/// </summary>
		/// <param name="serializedForm"></param>
		/// <returns></returns>

		public static SpotfireViewProps Deserialize(
			string serializedForm)
		{
			XmlMemoryStreamTextReader mstr = new XmlMemoryStreamTextReader(serializedForm);

			XmlTextReader tr = mstr.Reader;
			tr.Read(); // get CalcField element
			tr.MoveToContent();
			if (!Lex.Eq(tr.Name, "SpotfireLink"))
				throw new Exception("\"SpotfireLink\" element not found");

			SpotfireViewProps sl = Deserialize(mstr.Reader);
			mstr.Close();
			return sl;
		}

		public static SpotfireViewProps Deserialize(
			XmlTextReader tr)
		{
			string txt;

			SpotfireViewProps sl = new SpotfireViewProps(null);

			XmlUtil.GetStringAttribute(tr, "Server", ref sl.WebplayerUrl);
			XmlUtil.GetStringAttribute(tr, "LibraryPath", ref sl.AnalysisPath);
			XmlUtil.GetStringAttribute(tr, "Description", ref sl.Description);

			//txt = tr.GetAttribute("Name");
			//if (txt != null) sl.UserObject.Name = txt;

			if (!tr.IsEmptyElement)
			{
				while (true) // loop on Parameters
				{
					tr.Read(); // get Parameter element or end element
					tr.MoveToContent();

					if (tr.NodeType == XmlNodeType.Element && Lex.Eq(tr.Name, "Parameter"))
					{
						SpotfireLinkParameter sp = new SpotfireLinkParameter();
						XmlUtil.GetStringAttribute(tr, "Name", ref sp.Name);
						XmlUtil.GetStringAttribute(tr, "Value", ref sp.Value);
						sl.SpotfireLinkParameters[sp.Name] = sp;
					}

					else if (tr.NodeType == XmlNodeType.EndElement && // end element?
					 Lex.Eq(tr.Name, "SpotfireLink"))
						break;

					else throw new Exception("Expected SpotfireLink end element");
				}
			}

			return sl;
		}

	} // SpotfireLink

/// <summary>
/// SpotfireParameter
/// </summary>

	public class SpotfireLinkParameter
	{
		public SpotfireLinkParameter ()
		{
			return;
		}

		public SpotfireLinkParameter (string name, string value)
		{
			Name = name;
			Value = value;
		}

		public string Name = ""; // name of parameter used in InfoLink(s)
		public string Value = ""; // value or source of value(s) for parameter
	}

/// <summary>
/// SpotfireParmDataType
/// </summary>

	public enum SpotfireParmDataType
	{
		String    = 1,
		Integer   = 2,
		Real      = 3,
		DateTime  = 4,
		Date      = 5,
		Time      = 6,
		Undefined = 7
	}

}
