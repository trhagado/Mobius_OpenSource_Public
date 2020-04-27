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

	public class SerializeMsx
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
