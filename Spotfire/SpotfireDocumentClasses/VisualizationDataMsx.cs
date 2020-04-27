using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Mobius.SpotfireDocument
{
	/// <summary>
	/// The data table and other data-related members associated with a particular visualization
	/// </summary>

	public class VisualizationDataMsx : NodeMsx
	{
		[XmlIgnore]
		public DataTableMsx DataTableReference; // main data table

		public string DataTableReferenceSerializedId; // guid for serialization

		//public DataSelectionCombinationMethod MarkingCombinationMethod { get; set; }
		//public LegendMarkingItem MarkingLegendItem { get; }
		//public LegendDataTableItem DataTableLegendItem { get; }
		//public LimitingMarkingsEmptyBehavior LimitingMarkingsEmptyBehavior { get; set; }
		//public string LimitingMarkingsEmptyMessage { get; set; }
		//public Font LimitingMarkingsEmptyMessageFont { get; set; }
		//public bool UseActiveFiltering { get; set; }
		//public VisualizationRelations Relations { get; }
		//public string WhereClauseExpression { get; set; }
		//public VisualizationSubsetCollection Subsets { get; }
		//public VisualizationFilteringCollection Filterings { get; }
		//public DataMarkingSelection MarkingReference { get; set; }
		//public LegendFilteringsItem FilteringsLegendItem { get; }

		/// <summary>
		/// Update secondary object xxxMsx class object references to the
		/// associated Guid Ids prior to serialization.
		/// This method is normally overridden by each xxxMsx class to update 
		/// the references for that class.
		/// </summary>

		public override void UpdatePreSerializationSecondaryReferences()
		{
			DataTableReferenceSerializedId = DataTableReference?.Id;
			return;
		}

		/// <summary>
		/// Adjust secondary references for each Mobius.SpotfireDocument.xxxMsx class 
		/// including DataTableMsx, DataColumnMsx, VisualMsx, PageMsx ...
		/// </summary>

		public override void UpdatePostDeserializationSecondaryReferences()
		{
			ValidateNode();

			Doc_Tables.TryGetTableById(DataTableReferenceSerializedId, out DataTableReference);

			return;
		}

	}
}
