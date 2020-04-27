using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Drawing;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Mobius.SpotfireDocument
{
	/// <summary>
	/// SortInfoCollectionMsx
	/// </summary>

	public class SortInfoCollectionMsx : NodeMsx
	{
		public List<SortInfoMsx> SortList = new List<SortInfoMsx>();

		public SortInfoMsx GetSortItem(int idx)
		{
			if (SortList != null && SortList.Count > idx)
				return SortList[idx];

			else return null;
		}

		/// <summary>
		/// Adjust secondary references for each Mobius.SpotfireDocument.xxxMsx class 
		/// including DataTableMsx, DataColumnMsx, VisualMsx, PageMsx ...
		/// </summary>

		public override void UpdatePreSerializationSecondaryReferences()
		{
			foreach (SortInfoMsx si in SortList)
				UpdatePreSerializationSecondaryReferences(si);

			return;
		}

		/// <summary>
		/// Adjust secondary references for each Mobius.SpotfireDocument.xxxMsx class 
		/// including DataTableMsx, DataColumnMsx, VisualMsx, PageMsx ...
		/// </summary>

		public override void UpdatePostDeserializationSecondaryReferences()
		{
			ValidateNode();

			foreach (SortInfoMsx si in SortList)
				UpdatePostDeserializationSecondaryReferences(si);

			return;
		}

	}

	/// <summary>
	/// Column reference and sort order for the column
	/// </summary>

	public class SortInfoMsx : NodeMsx
	{
		[XmlIgnore]
		public DataColumnMsx DataColumnReference = null;
		public string DataColumnReferenceSerializedId = "";

		public SortOrderMsx SortOrder = SortOrderMsx.Ascending;

		/// <summary>
		/// Adjust secondary references for each Mobius.SpotfireDocument.xxxMsx class 
		/// including DataTableMsx, DataColumnMsx, VisualMsx, PageMsx ...
		/// </summary>

		public override void UpdatePreSerializationSecondaryReferences()
		{
			DataColumnReferenceSerializedId = DataColumnMsx.GetReferenceId(DataColumnReference);
			return;
		}

		/// <summary>
		/// Adjust secondary references for each Mobius.SpotfireDocument.xxxMsx class 
		/// including DataTableMsx, DataColumnMsx, VisualMsx, PageMsx ...
		/// </summary>

		public override void UpdatePostDeserializationSecondaryReferences()
		{
			ValidateNode();

			DataColumnReference = DataColumnMsx.GetInstanceFromReferenceId(App_Document, DataColumnReferenceSerializedId);

			return;
		}

	}

	/// <summary>
	/// SortOrderMsx
	/// </summary>

	public enum SortOrderMsx
	{
		Ascending = 0,
		Descending = 1
	}

}
