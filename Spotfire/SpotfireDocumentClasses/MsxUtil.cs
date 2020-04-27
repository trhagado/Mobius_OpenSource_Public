using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Mobius.SpotfireDocument
{
	/// <summary>
	/// Utility class for Msx objects  
	/// Serializing/deserializing et al
	/// </summary>

	public class MsxUtil
	{
		/// <summary>
		/// Serialize the Mobius version of a Spotfire Document Model class (e.g. DocumentMsx, DataTableMsx, PageMsx, VisualMsx)
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>

		public static string Serialize(object obj)
		{
			if (obj == null) return "";

			string xmlStr = String.Empty;
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Indent = true;
			settings.OmitXmlDeclaration = true;
			settings.NewLineChars = "\r\n";
			settings.NewLineHandling = NewLineHandling.Entitize;

			using (StringWriter stringWriter = new StringWriter())
			{
				using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter, settings))
				{
					XmlSerializer serializer = new XmlSerializer(obj.GetType());
					serializer.Serialize(xmlWriter, obj);
					xmlStr = stringWriter.ToString();
					xmlWriter.Close();
				}
			}
			return xmlStr.ToString();
		}

		/// <summary>
		/// Deserialize a visual returning the appropriate visual object that is derived from 
		/// the base VisualMsx object.
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>

		public static VisualMsx DeserializeVisual(string data)
		{
			Type visType = null;

			if (data.IndexOf("<TablePlotMsx", StringComparison.OrdinalIgnoreCase) == 0)
				visType = typeof(TablePlotMsx);

			else if (data.IndexOf("<BarChartMsx", StringComparison.OrdinalIgnoreCase) == 0)
				visType = typeof(BarChartMsx);

			else if (data.IndexOf("<ScatterPlotMsx", StringComparison.OrdinalIgnoreCase) == 0)
				visType = typeof(ScatterPlotMsx);

			else if (data.IndexOf("<TreemapMsx", StringComparison.OrdinalIgnoreCase) == 0)
				visType = typeof(TreemapMsx);

			else if (data.IndexOf("<HeatMapMsx", StringComparison.OrdinalIgnoreCase) == 0)
				visType = typeof(HeatMapMsx);

			else if (data.IndexOf("<TrellisCardvisualMsx", StringComparison.OrdinalIgnoreCase) == 0)
				visType = typeof(TrellisCardVisualMsx);

			else visType = typeof(VisualMsx); // shouldn't happen

			XmlSerializer xmlSer = new XmlSerializer(visType);
			StringReader reader = new StringReader(data);

			VisualMsx visMsx = (VisualMsx)(xmlSer.Deserialize(reader));
			return visMsx;
		}

		/// <summary>
		/// General deserialization of classes serialized with XmlSerializer
		/// </summary>
		/// <param name="data"></param>
		/// <param name="objType"></param>
		/// <returns></returns>

		public static object Deserialize(string data, Type objType)
		{
			XmlSerializer xmlSer = null;
			StringReader reader = null;
			object obj = null;

			try
			{
				xmlSer = new XmlSerializer(objType);
				reader = new StringReader(data);

				obj = new object();
				obj = (object)(xmlSer.Deserialize(reader));
				return obj;
			}

			catch (Exception ex)
			{
				string msg =
					"Exception deserializing Type: " + objType + "\r\n" +
					ex.Message + "\r\n" +
					data;

				throw new Exception(msg, ex);
			}
		}

	}
}
