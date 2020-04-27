using Mobius.ComOps;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Mobius.SpotfireDocument
{

	/// <summary>
	/// PageCollection class
	/// </summary>

	public class PageCollectionMsx : NodeMsx
	{
		public List<PageMsx> PageList = new List<PageMsx>();

		//public PageNavigationMode NavigationMode { get; set; }
		//public VisualizationAreaSize VisualizationAreaSize { get; }

		public bool TryGetPage(
			string id,
			out PageMsx page)
		{
			foreach (PageMsx current in PageList)
			{
				if (current.Id == id)
				{
					page = current;
					return true;
				}
			}
			page = null;
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
			foreach (PageMsx p in PageList)
			{
				UpdatePreSerializationSecondaryReferences(p);
			}

			return;
		}

		/// <summary>
		/// Adjust secondary references for each Mobius.SpotfireDocument.xxxMsx class 
		/// including DataTableMsx, DataColumnMsx, VisualMsx, PageMsx ...
		/// </summary>

		public override void UpdatePostDeserializationSecondaryReferences()
		{
			ValidateNode();

			foreach (PageMsx p in PageList)
			{
				UpdatePostDeserializationSecondaryReferences(p);
			}

			return;
		}

	}


	/// <summary>
	/// Page class
	/// </summary>

	public class PageMsx : NodeMsx
	{
		public string Id; // Guid

		public string Title = "";

		public VisualCollectionMsx Visuals = new VisualCollectionMsx();

		[XmlIgnore]
		public DataTableMsx ActiveDataTableReference = null;
		public string ActiveDataTableReferenceSerializedId;

		[XmlIgnore]
		public VisualMsx ActiveVisualReference = null;
		public string ActiveVisualReferenceSerializedId;

		/// <summary>
		/// Adjust secondary references for each Mobius.SpotfireDocument.xxxMsx class 
		/// including DataTableMsx, DataColumnMsx, VisualMsx, PageMsx ...
		/// </summary>

		public override void UpdatePreSerializationSecondaryReferences()
		{
			ActiveDataTableReferenceSerializedId = ActiveDataTableReference?.Id;

			ActiveVisualReferenceSerializedId = ActiveVisualReference?.Id;

			foreach (VisualMsx visual in Visuals.VisualList)
			{
				UpdatePreSerializationSecondaryReferences(visual);
			}

			return;
		}

		/// <summary>
		/// Adjust secondary references for each Mobius.SpotfireDocument.xxxMsx class 
		/// including DataTableMsx, DataColumnMsx, VisualMsx, PageMsx ...
		/// </summary>

		public override void UpdatePostDeserializationSecondaryReferences()
		{
			ValidateNode();

			Doc_Tables.TryGetTableById(ActiveDataTableReferenceSerializedId, out ActiveDataTableReference);

			App_Document.TryGetVisual(ActiveVisualReferenceSerializedId, out ActiveVisualReference);

			foreach (VisualMsx visual in Visuals.VisualList)
			{
				UpdatePostDeserializationSecondaryReferences(visual);
			}

			return;
		}

	}


}
