using Mobius.ComOps;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Xml;
using System.Xml.Serialization;

namespace Mobius.SpotfireDocument
{

	// Serialize Mobius version of Spotfire Document for persistence or network transmission
	//  Possible C# attributes: [XmlIgnore] [XmlArray] [XmlArrayItem]

	public class DocumentMsx : NodeMsx
	{
		public DataManagerMsx DataManager = new DataManagerMsx();

		public PageCollectionMsx Pages = new PageCollectionMsx();

		public string Description = "";

		[XmlIgnore]
		public PageMsx ActivePageReference = null;
		public string ActivePageReferenceSerializedId = "";

		[XmlIgnore]
		public DataTableMsx ActiveDataTableReference = null;
		public string ActiveDataTableReferenceSerializedId = "";

		[XmlIgnore]
		public VisualMsx ActiveVisualReference = null;
		public String ActiveVisualReferenceSerializedId = ""; // Id of active Visual

		/// <summary>
		/// Serialize a DocumentMsx
		/// </summary>
		/// <returns></returns>

		public string Serialize()
		{
			UpdatePreSerializationSecondaryReferences();

			string serializedText = SerializeMsx.Serialize(this); // serialize
			return serializedText;
		}

/// <summary>
/// Deserialize a DocumentMsx
/// </summary>
/// <param name="serializedDoc"></param>
/// <returns></returns>

		public static DocumentMsx Deserialize(
			string serializedDoc,
			AnalysisApplicationMsx app)
		{
			DocumentMsx doc = (DocumentMsx)SerializeMsx.Deserialize(serializedDoc, typeof(DocumentMsx));

			app.Document = doc;

			doc.SetApp(app);

			doc.UpdatePostDeserializationSecondaryReferences();

			return doc;
		}

			/// <summary>
			/// Try to get Visual by Guid Id across all pages
			/// 
			/// </summary>
			/// <param name="id"></param>
			/// <param name="visual"></param>
			/// <returns></returns>

			public bool TryGetVisual(
			string id,
			out VisualMsx visual)
		{
			foreach (PageMsx p in Pages.PageList)
			{
				if (p.Visuals.TryGetVisual(id, out visual))
					return true;
			}

			visual = null;
			return false;
		}

		/// <summary>
		/// Update secondary object xxxMsx class object references to the
		/// associated Guid Ids prior to serialization.
		/// This method is normally overridden by each xxxMsx class to update 
		/// the references for that class.
		/// </summary>

		public override void UpdatePreSerializationSecondaryReferences()
		{
			ActivePageReferenceSerializedId = ActivePageReference?.Id;

			ActiveDataTableReferenceSerializedId = ActiveDataTableReference?.Id;

			ActiveVisualReferenceSerializedId = ActiveVisualReference?.Id;

			UpdatePreSerializationSecondaryReferences(DataManager);

			UpdatePreSerializationSecondaryReferences(Pages);

			return;
		}

		/// <summary>
		/// Update secondary references for each Mobius.SpotfireDocument.xxxMsx class 
		/// These secondary references are not serialized and need to be updated from 
		/// other Ids (usually Guids) after deserializing a Document. They include references
		/// to DataTableMsx, DataColumnMsx, VisualMsx, PageMsx ...
		/// </summary>

		public override void UpdatePostDeserializationSecondaryReferences()
		{
			ValidateNode();

			Pages.TryGetPage(ActivePageReferenceSerializedId, out ActivePageReference);

			DataManager.TableCollection.TryGetTableById(ActiveDataTableReferenceSerializedId, out ActiveDataTableReference);

			TryGetVisual(ActiveVisualReferenceSerializedId, out ActiveVisualReference);

			UpdatePostDeserializationSecondaryReferences(DataManager);

			UpdatePostDeserializationSecondaryReferences(Pages);

			return;
		}

	}

}
