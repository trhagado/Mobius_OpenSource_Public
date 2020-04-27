using Mobius.SpotfireDocument;
using Mobius.ComOps;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Mobius.SpotfireDocument
{
	/// <summary>
	/// HeapMap Visualization
	/// </summary>

	public class HeatMapMsx : VisualMsx
	{
		public ColoringCollectionMsx Colorings = new ColoringCollectionMsx();

		public LabelColumnMsx Labels = new LabelColumnMsx();

		public SortInfoCollectionMsx SortInfo = new SortInfoCollectionMsx();

		public LegendMsx Legend = new LegendMsx();

		public TrellisMsx Trellis = new TrellisMsx();

		/// <summary>
		/// Update secondary object xxxMsx class object references to the
		/// associated Guid Ids prior to serialization.
		/// This method is normally overridden by each xxxMsx class to update 
		/// the references for that class.
		/// </summary>

		public override void UpdatePreSerializationSecondaryReferences()
		{
			// todo

			return;
		}

		/// <summary>
		/// Adjust secondary references for each Mobius.SpotfireDocument.xxxMsx class 
		/// including DataTableMsx, DataColumnMsx, VisualMsx, PageMsx ...
		/// </summary>

		public override void UpdatePostDeserializationSecondaryReferences()
		{
			ValidateNode();

			// todo

			return;
		}

	}
}
