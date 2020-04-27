using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Threading.Tasks;

namespace Mobius.SpotfireDocument
{

	public class AnalysisApplicationMsx : Mobius.SpotfireDocument.NodeMsx
	{
		public DocumentMsx Document = null; // the document

		[XmlIgnore]
		public object Container = null; // object that contains the Analysis in the current usage context (e.g. SpotfireViewManager)

		[XmlIgnore]
		public string Path = ""; // full path to analysis in library or file
		[XmlIgnore]
		public string Name { get { return GetNameFromPath(); } }


		/// <summary>
		/// Extract the name of the analysis from the path
		/// </summary>
		/// <returns></returns>

		string GetNameFromPath()
		{
			if (string.IsNullOrWhiteSpace(Path)) return "";

			string name = System.IO.Path.GetFileNameWithoutExtension(Path);
			return name;
		}

		/// <summary>
		/// Adjust secondary references for each Mobius.SpotfireDocument.xxxMsx class 
		/// including DataTableMsx, DataColumnMsx, VisualMsx, PageMsx ...
		/// </summary>

		public override void UpdatePreSerializationSecondaryReferences()
		{
			UpdatePreSerializationSecondaryReferences(Document);

			return;
		}

		/// <summary>
		/// Adjust secondary references for each Mobius.SpotfireDocument.xxxMsx class 
		/// including DataTableMsx, DataColumnMsx, VisualMsx, PageMsx ...
		/// </summary>

		public override void UpdatePostDeserializationSecondaryReferences()
		{
			ValidateNode();

			UpdatePostDeserializationSecondaryReferences(Document);

			return;
		}

	}

}
