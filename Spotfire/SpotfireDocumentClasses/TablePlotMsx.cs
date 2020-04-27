using Mobius.SpotfireDocument;
using Mobius.ComOps;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Xml.Serialization;


namespace Mobius.SpotfireDocument
{

	/// <summary>
	/// TablePlot Visualization
	/// </summary>

	public class TablePlotMsx : VisualMsx
	{
		public TableColumnCollectionMsx TableColumns = new TableColumnCollectionMsx();

		public bool AutoAddNewColumns { get; set; }

		public ColoringCollectionMsx Colorings = new ColoringCollectionMsx();

		public SortInfoCollectionMsx SortInfo = new SortInfoCollectionMsx();

		public LegendMsx Legend = new LegendMsx();

		/// <summary>
		/// Update secondary object xxxMsx class object references to the
		/// associated Guid Ids prior to serialization.
		/// This method is normally overridden by each xxxMsx class to update 
		/// the references for that class.
		/// </summary>

		public override void UpdatePreSerializationSecondaryReferences()
		{
			base.UpdatePreSerializationSecondaryReferences();

			UpdatePreSerializationSecondaryReferences(TableColumns);

			UpdatePreSerializationSecondaryReferences(Colorings);

			UpdatePreSerializationSecondaryReferences(SortInfo);

			UpdatePreSerializationSecondaryReferences(Legend);

			return; // todo
		}

		/// <summary>
		/// Adjust secondary references for each Mobius.SpotfireDocument.xxxMsx class 
		/// including DataTableMsx, DataColumnMsx, VisualMsx, PageMsx ...
		/// </summary>

		public override void UpdatePostDeserializationSecondaryReferences()
		{
			ValidateNode();

			base.UpdatePostDeserializationSecondaryReferences();

			UpdatePostDeserializationSecondaryReferences(TableColumns);

			UpdatePostDeserializationSecondaryReferences(Colorings);

			UpdatePostDeserializationSecondaryReferences(SortInfo);

			UpdatePostDeserializationSecondaryReferences(Legend);

			return;
		}
	}

	/// <summary>
	/// TableColumnCollectionMsx class
	/// </summary>

	public class TableColumnCollectionMsx : NodeMsx
	{
		public List<TableColumnMsx> Columns = new List<TableColumnMsx>();

		public static int DefaultTableColumnWidth = 100;

		public void Add(DataColumnMsx dataColumn)
		{
			TableColumnMsx tc = new TableColumnMsx();
			tc.DataColumn = dataColumn;
			tc.Width = DefaultTableColumnWidth;
		}

		public bool Contains(DataColumnMsx dataColumn)
		{
			foreach (TableColumnMsx tc in Columns)
			{
				if (tc.DataColumn == dataColumn) return true;
			}

			return false;
		}

		public bool TryGetTableColumn(DataColumnMsx dataColumn, out TableColumnMsx tableColumn)
		{
			foreach (TableColumnMsx tc in Columns)
			{
				if (tc.DataColumn == dataColumn)
				{
					tableColumn = tc;
					return true;
				}
			}

			tableColumn = null;
			return false;
		}

		public List<DataColumnMsx> GetDataColumns()
		{
			List<DataColumnMsx> list = new List<DataColumnMsx>();

			foreach (TableColumnMsx tc in Columns)
			{
				list.Add(tc.DataColumn);
			}

			return list;
		}

		/// <summary>
		/// Update secondary object xxxMsx class object references to the
		/// associated Guid Ids prior to serialization.
		/// This method is normally overridden by each xxxMsx class to update 
		/// the references for that class.
		/// </summary>

		public override void UpdatePreSerializationSecondaryReferences()
		{
			foreach (TableColumnMsx tc in Columns)
			{
				UpdatePreSerializationSecondaryReferences(tc);
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

			foreach (TableColumnMsx tc in Columns)
			{
				UpdatePostDeserializationSecondaryReferences(tc);
			}

			return;
		}
	}

	/// <summary>
	/// TableColumnMsx class
	/// </summary>

	public class TableColumnMsx : NodeMsx
	{
		[XmlIgnore]
		public DataColumnMsx DataColumn = null;
		public string DataColumnSerializedId = "";

		public int Width { get; set; }

		/// <summary>
		/// Update secondary object xxxMsx class object references to the
		/// associated Guid Ids prior to serialization.
		/// This method is normally overridden by each xxxMsx class to update 
		/// the references for that class.
		/// </summary>

		public override void UpdatePreSerializationSecondaryReferences()
		{
			DataColumnSerializedId = DataColumnMsx.GetReferenceId(DataColumn);

			return;
		}

		/// <summary>
		/// Adjust secondary references for each Mobius.SpotfireDocument.xxxMsx class 
		/// including DataTableMsx, DataColumnMsx, VisualMsx, PageMsx ...
		/// </summary>

		public override void UpdatePostDeserializationSecondaryReferences()
		{
			ValidateNode();

			DataColumn = DataColumnMsx.GetInstanceFromReferenceId(App_Document, DataColumnSerializedId);

			return;
		}
	}

}
