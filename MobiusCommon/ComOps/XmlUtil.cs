using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Drawing;
using System.Globalization;

namespace Mobius.ComOps
{
	public class XmlUtil
	{
		static CultureInfo EnUsFormatProvider = new CultureInfo("en-US"); // format provider for English, United States type formats

		public static bool GetBoolAttribute(
			XmlTextReader tr,
			string name,
			ref bool value)
		{
			string txt = GetAttribute(tr, name);
			if (txt == null) return false;
			return bool.TryParse(txt, out value);
		}

		public static bool GetIntAttribute(
			XmlTextReader tr,
			string name,
			ref int value)
		{
			string txt = GetAttribute(tr, name);
			if (txt == null) return false;
			return int.TryParse(txt, out value);
		}

		public static bool GetFloatAttribute(
			XmlTextReader tr,
			string name,
			ref float value)
		{
			string txt = GetAttribute(tr, name);
			if (txt == null) return false;
			return float.TryParse(txt, out value);
		}

		public static bool GetDoubleAttribute(
			XmlTextReader tr,
			string name,
			ref double value)
		{
			string txt = GetAttribute(tr, name);
			if (txt == null) return false;
			return double.TryParse(txt, out value);
		}

		public static bool GetDecimalAttribute(
			XmlTextReader tr,
			string name,
			ref Decimal value)
		{
			string txt = GetAttribute(tr, name);
			if (txt == null) return false;
			return Decimal.TryParse(txt, out value);
		}

		public static bool GetStringAttribute(
			XmlTextReader tr,
			string name,
			ref string value)
		{
			string txt = GetAttribute(tr, name);
			if (txt == null) return false;
			value = txt;
			return true;
		}

		public static bool GetDateAttribute(
			XmlTextReader tr,
			string name,
			ref DateTime value)
		{
			string txt = GetAttribute(tr, name);
			if (txt == null) return false;
			bool rv = DateTime.TryParse(txt, EnUsFormatProvider, DateTimeStyles.None, out value);
			return rv;
		}

		public static bool GetColorAttribute(
			XmlTextReader tr,
			string name,
			ref Color value)
		{
			string txt = GetAttribute(tr, name);
			if (txt == null) return false;
			value = Color.FromArgb(int.Parse(txt));
			return true;
		}

		public static bool GetRectAttribute(
			XmlTextReader tr,
			string name,
			ref Rectangle value)
		{
			int x, y, width, height;

			string txt = GetAttribute(tr, name);
			if (txt == null) return false;
			string[] sa = txt.Split(',');
			if (sa.Length != 4) return false;
			if (!int.TryParse(sa[0], out x)) return false;
			if (!int.TryParse(sa[1], out y)) return false;
			if (!int.TryParse(sa[2], out width)) return false;
			if (!int.TryParse(sa[3], out height)) return false;

			value = new Rectangle(x, y, width, height);
			return true;
		}

		/// <summary>
		/// Parse an enum variable of the specified type
		/// </summary>
		/// <param name="tr"></param>
		/// <param name="name"></param>
		/// <param name="type"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		/// 
		public static bool GetEnumAttribute(XmlTextReader tr, string name, Type type, ref Enum value)
		{
			string errMsg;

			return GetEnumAttribute(tr, name, type, ref value, out errMsg);
		}

		/// <summary>
		/// Parse an enum variable of the specified type
		/// </summary>
		/// <param name="tr"></param>
		/// <param name="name"></param>
		/// <param name="type"></param>
		/// <param name="value"></param>
		/// <param name="errMsg"></param>
		/// <returns></returns>

		public static bool GetEnumAttribute(XmlTextReader tr, string name, Type type, ref Enum value, out string errMsg)
		{
			errMsg = null;

			string txt = GetAttribute(tr, name);
			if (Lex.IsNullOrEmpty(txt))
			{
				errMsg = "Missing XML attribute: " + name;
				return false;
			}

			if (!Enum.IsDefined(type, txt))
			{
				errMsg = "Invalid value for XML attribute " + name + ": " + txt;
				return false;
			}

			value = (Enum)Enum.Parse(type, txt);
			return true;
		}

/// <summary>
///  Case-insensitive GetAttribute
/// </summary>
/// <param name="tr"></param>
/// <param name="name"></param>
/// <returns></returns>

		public static string GetAttribute(
			XmlTextReader tr, 
			string name)
		{
			string result = null;

			if (!tr.MoveToFirstAttribute())
				return null;

			do
			{
				if (tr.Prefix == "" && tr.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
				{
					result = tr.Value.Trim(' '); // trim all values. Trim spaces only, Trim() will remove whitespace including the Tab delimiter (\t).
					break;
				}
			}
			while (tr.MoveToNextAttribute());

			tr.MoveToElement(); // Go back to the containing element.   

			return result;
		}

		/// <summary>
		/// Convert a MemoryStream to a string
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>

		public static string MemoryStreamToString(
			MemoryStream stream)
		{
			byte[] buffer = new byte[stream.Length];
			stream.Seek(0, 0);
			stream.Read(buffer, 0, (int)stream.Length);
			string serializedForm = System.Text.Encoding.ASCII.GetString(buffer, 0, (int)stream.Length);
			return serializedForm;
		}

		/// <summary>
		/// Convert a string to a MemoryStream
		/// </summary>
		/// <param name="content"></param>
		/// <returns></returns>

		public static MemoryStream StringToMemoryStream(
			string content)
		{
			ASCIIEncoding encoder = new ASCIIEncoding();
			byte[] buffer = encoder.GetBytes(content);
			MemoryStream stream = new MemoryStream(buffer);
			return stream;
		}

/// <summary>
/// Write attribute if value is defined
/// </summary>
/// <param name="tw"></param>
/// <param name="attrName"></param>
/// <param name="value"></param>

		public static bool WriteAttributeIfDefined(
			XmlTextWriter tw,
			string attrName,
			string value)
		{
			if (Lex.IsNullOrEmpty(value)) return false;

			tw.WriteAttributeString(attrName, value);
			return true;
		}

		/// <summary>
		/// Return true if more subelements under the current element
		/// </summary>
		/// <param name="tr"></param>
		/// <param name="currentElementName"></param>
		/// <returns></returns>

		public static bool MoreSubElements(
			XmlTextReader tr,
			string currentElementName)
		{
			if (tr.NodeType == XmlNodeType.Element && // empty element?
				Lex.Eq(tr.Name, currentElementName) && tr.IsEmptyElement) return false;

			tr.Read(); // move to next main level element
			tr.MoveToContent();

			if (tr.NodeType == XmlNodeType.EndElement && // done with elements of the specified type?
				Lex.Eq(tr.Name, currentElementName)) return false;

			else return true;
		}

		/// <summary>
		/// Verify that we are at an element with the specified element name
		/// </summary>
		/// <param name="tr"></param>
		/// <param name="elementName"></param>

		public static void VerifyElementName(
			XmlTextReader tr,
			string elementName)
		{
			if (Lex.Eq(tr.Name, elementName)) return;

			else	throw new Exception("Expected " + elementName + " end element");
		}

		/// <summary>
		/// Verify that the reader is currently at the end of the specified element
		/// </summary>
		/// <param name="tr"></param>
		/// <param name="elementName"></param>

		public static void VerifyAtEndOfElement(
			XmlTextReader tr,
			string elementName)
		{
			if (tr.IsEmptyElement && Lex.Eq(tr.Name, elementName)) return; // empty element with matching name?

			else if (tr.NodeType == XmlNodeType.EndElement && Lex.Eq(tr.Name, elementName)) return; // end element with matching name?

			else // read next element and check if verify proper name
			{
				string currentName = tr.Name;
				tr.Read();
				tr.MoveToContent();

				if (Lex.Eq(tr.Name, elementName)) return;

				else throw new Exception("Expected " + elementName + " end element but found: " + currentName + " and then " + tr.Name);
			}
		}


		/// <summary>
		/// Serialize a class instance using the XmlSerializer
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


		/// <summary>
		/// Example of how to deserialize a subclass and return as base type
		/// </summary>

		public class ExampleSubClass : Object
		{
			public int ExampleAttribute = 0;

			/// <summary>
			/// Deserialize a derived type
			/// the base VisualMsx object.
			/// </summary>
			/// <param name="data"></param>
			/// <returns></returns>

			public static object DeserializeDerivedClass(string data)
			{
				Type subType = null;

				if (data.IndexOf("<(ExampleSubClass", StringComparison.OrdinalIgnoreCase) == 0)
					subType = typeof(ExampleSubClass);

				else throw new InvalidOperationException("Unexpected derived class tkype");

				XmlSerializer xmlSer = new XmlSerializer(subType);
				StringReader reader = new StringReader(data);

				Object baseType = (Object)(xmlSer.Deserialize(reader));
				return baseType;
			}

		}


	} // XmlUtil 

	public class XmlMemoryStreamTextWriter
	{
		public MemoryStream Stream;
		public XmlTextWriter Writer;

/// <summary>
/// Open XmlTextWriter that writes to a memory stream
/// </summary>

		public XmlMemoryStreamTextWriter()
		{
			Stream = new MemoryStream();
			Writer = new XmlTextWriter(Stream, null);
		}

/// <summary>
/// Get the Xml in the memory stream & close the writer
/// </summary>
/// <returns></returns>

		public string GetXmlAndClose ()
		{
			Writer.Flush();
			byte[] buffer = new byte[Stream.Length];
			Stream.Seek(0, 0);
			Stream.Read(buffer, 0, (int)Stream.Length);
			string serializedForm = System.Text.Encoding.ASCII.GetString(buffer, 0, (int)Stream.Length);
			Writer.Close();

			return serializedForm;
		}
	}

	public class XmlMemoryStreamTextReader
	{
		public MemoryStream Stream;
		public XmlTextReader Reader;

		/// <summary>
		/// Open XmlTextWriter that reades from a memory stream
		/// </summary>

		public XmlMemoryStreamTextReader(
			string content)
		{
			ASCIIEncoding encoder = new ASCIIEncoding();
			byte[] buffer = encoder.GetBytes(content);
			Stream = new MemoryStream(buffer);
			Reader = new XmlTextReader(Stream);

			//			Reader.MoveToContent();
			return;
		}

		/// <summary>
		/// Close the reader
		/// </summary>
		/// <returns></returns>

		public void Close()
		{
			Reader.Close();
			return;
		}
	}

}
