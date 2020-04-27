using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Xml;

namespace Mobius.ComOps
{
	public class SpotfireTemplateParms
	{
		public string TemplatePath = ""; // path to template file
		public string SourceCsvFileName = ""; // path to .csv data file
		public string LibraryFolder = ""; // Library name to save to
		public string SaveName = ""; // File name to save to
		public string ImagePath = ""; // path to image file
		public Rectangle ImageBounds; // image boundaries for first visualization

		public string Serialize()
		{
			XmlMemoryStreamTextWriter mstw = new XmlMemoryStreamTextWriter();
			XmlTextWriter tw = mstw.Writer;
			tw.Formatting = Formatting.Indented;

			tw.WriteStartElement("SpotfireTemplateParms");

			tw.WriteAttributeString("TemplatePath", TemplatePath);
			tw.WriteAttributeString("SourceCsvFileName", SourceCsvFileName);
			tw.WriteAttributeString("LibraryFolder", LibraryFolder);
			tw.WriteAttributeString("SaveName", SaveName);

			tw.WriteAttributeString("ImagePath", ImagePath);
			tw.WriteAttributeString("ImageBounds.X", ImageBounds.X.ToString());
			tw.WriteAttributeString("ImageBounds.Y", ImageBounds.Y.ToString());
			tw.WriteAttributeString("ImageBounds.Width", ImageBounds.Width.ToString());
			tw.WriteAttributeString("ImageBounds.Height", ImageBounds.Height.ToString());

			tw.WriteEndElement();
			string xml = mstw.GetXmlAndClose();
			return xml;
		}

		/// <summary>
		/// Deserialize
		/// </summary>
		/// <param name="serializedForm"></param>
		/// <returns></returns>

		public static SpotfireTemplateParms Deserialize(
			string serializedForm)
		{
			int x = 0, y = 0, width = 0, height = 0;

			XmlMemoryStreamTextReader mstr = new XmlMemoryStreamTextReader(serializedForm);
			XmlTextReader tr = mstr.Reader;

			tr.Read();
			tr.MoveToContent();
			if (!Lex.Eq(tr.Name, "SpotfireTemplateParms"))
				throw new Exception("\"SpotfireTemplateParms\" element not found");

			SpotfireTemplateParms stp = new SpotfireTemplateParms();

			XmlUtil.GetStringAttribute(tr, "TemplatePath", ref stp.TemplatePath);
			XmlUtil.GetStringAttribute(tr, "SourceCsvFileName", ref stp.SourceCsvFileName);
			XmlUtil.GetStringAttribute(tr, "LibraryFolder", ref stp.LibraryFolder);
			XmlUtil.GetStringAttribute(tr, "SaveName", ref stp.SaveName);

			XmlUtil.GetStringAttribute(tr, "ImagePath", ref stp.ImagePath);
			XmlUtil.GetIntAttribute(tr, "ImageBounds.X", ref x);
			XmlUtil.GetIntAttribute(tr, "ImageBounds.Y", ref y);
			XmlUtil.GetIntAttribute(tr, "ImageBounds.Width", ref width);
			XmlUtil.GetIntAttribute(tr, "ImageBounds.Height", ref height);
			stp.ImageBounds = new Rectangle(x, y, width, height);

			if (!tr.IsEmptyElement)
			{
				tr.Read(); // move to Export options or end of Alert element
				tr.MoveToContent();

				if (!Lex.Eq(tr.Name, "SpotfireTemplateParms") || tr.NodeType != XmlNodeType.EndElement)
					throw new Exception("Expected SpotfireTemplateParms end element");
			}

			mstr.Close();
			return stp;
		}

	}
}
