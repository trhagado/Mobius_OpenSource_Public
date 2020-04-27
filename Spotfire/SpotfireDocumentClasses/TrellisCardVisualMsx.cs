using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Threading.Tasks;

namespace Mobius.SpotfireDocument
{
	public class TrellisCardVisualMsx : VisualMsx
	{
		[XmlIgnore]
		public DataTableMsx DataTable = null;
		public string DataTableSerializedId = ""; // guid of DataTable

		public string IdColumnName = "";
		public string FocusColumnName = "";

		public List<ColorCodingItemMsx> FeatureColumns = new List<ColorCodingItemMsx>();
		public List<ColorCodingItemMsx> SelectedColumns = new List<ColorCodingItemMsx>();

		public int ColumnAmount = 3; // number of trellis columns
		public int RowAmount = 2; // number of trellis rows

		public int NavigationType = 0; // paging = 0, scrolling = 1
	
		public string SortingByColumn = "";
		public SortOrderMsx SortingByType = SortOrderMsx.Ascending;
		public string ThenBy1Column = "";
		public SortOrderMsx ThenBy1Type = SortOrderMsx.Ascending;
		public string ThenBy2Column = "";
		public SortOrderMsx ThenBy2Type = SortOrderMsx.Ascending;

		/// <summary>
		/// Update secondary object xxxMsx class object references to the
		/// associated Guid Ids prior to serialization.
		/// This method is normally overridden by each xxxMsx class to update 
		/// the references for that class.
		/// </summary>

		public override void UpdatePreSerializationSecondaryReferences()
		{
			DataTableSerializedId = DataTableMsx.GetReferenceId(DataTable);

			return;
		}

		/// <summary>
		/// Adjust secondary references for each Mobius.SpotfireDocument.xxxMsx class 
		/// </summary>

		public override void UpdatePostDeserializationSecondaryReferences()
		{
			ValidateNode();

			Doc_Tables.TryGetTableById(DataTableSerializedId, out DataTable);

			return;
		}
	}

	/// <summary>
	/// Define coloring and limits for both Feature and BarChart columns
	/// </summary>

	public class ColorCodingItemMsx
	{
		public ColorCodingItemMsx()
		{
			return;
		}

		public string DataTableName = ""; // Spotfire data table name
		public string ColumnName = ""; // Spotfire column name
		//public string ImageName = ""; // name of image to display if rule matches
		public List<ColorCodingSubRuleMsx> SubRules = new List<ColorCodingSubRuleMsx>();
		public XmlColor BackColor = System.Drawing.Color.Empty;
	}

	public class ColorCodingSubRuleMsx
	{

		public ColorCodingSubRuleMsx()
		{
			return;
		}

		public string Value = ""; // external string form of value
		public ValueCalcTypeEnumMsx CalcType = ValueCalcTypeEnumMsx.Value;
		public XmlColor Color = System.Drawing.Color.Empty;
	}

	public enum ValueCalcTypeEnumMsx
	{
		Value = 0,
		Min = 1,
		Max = 2,
		Average = 3
	}
}
