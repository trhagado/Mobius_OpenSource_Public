using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mobius.SpotfireDocument
{

	/// <summary>
	/// TrellisMsx
	/// </summary>

	public class TrellisMsx : NodeMsx
	{
		// public Font HeaderFont { get; set; }

		public bool IsTrellising { get; }

		public int ManualRowCount { get; set; }

		public int ManualColumnCount { get; set; }
		
		//public int PageCount { get; }

		//public LegendTrellisItem LegendItem { get; }

		//public int ActivePageIndexRuntime { get; }

		public AxisMsx RowAxis { get; set; }

		public AxisMsx ColumnAxis { get; set; }

		public AxisMsx PageAxis { get; set; }

		public AxisMsx PanelAxis { get; }

		public TrellisModeMsx TrellisMode { get; set; }

		public bool ManualLayout { get; set; }

		/// <summary>
		/// Update secondary object xxxMsx class object references to the
		/// associated Guid Ids prior to serialization.
		/// This method is normally overridden by each xxxMsx class to update 
		/// the references for that class.
		/// </summary>

		public override void UpdatePreSerializationSecondaryReferences()
		{
				UpdatePreSerializationSecondaryReferences(RowAxis);

				UpdatePreSerializationSecondaryReferences(ColumnAxis);

				UpdatePreSerializationSecondaryReferences(PageAxis);

				UpdatePreSerializationSecondaryReferences(PanelAxis);

			return;
		}
		
		/// <summary>
		/// Adjust secondary references for each Mobius.SpotfireDocument.xxxMsx class 
		/// including DataTableMsx, DataColumnMsx, VisualMsx, PageMsx ...
		/// </summary>

		public override void UpdatePostDeserializationSecondaryReferences()
		{
			ValidateNode();

			UpdatePostDeserializationSecondaryReferences(RowAxis);
			UpdatePostDeserializationSecondaryReferences(ColumnAxis);
			UpdatePostDeserializationSecondaryReferences(PageAxis);
			UpdatePostDeserializationSecondaryReferences(PanelAxis);

			return;
		}
	}

	public enum TrellisModeMsx
	{
		RowsColumns = 0,
		Panels = 1
	}

}
