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
	/// BarChart Visualization
	/// </summary>

	public class BarChartMsx : VisualMsx
	{
		public ColoringCollectionMsx Colorings = new ColoringCollectionMsx();

		public LabelColumnMsx Labels = new LabelColumnMsx();

		public SortInfoCollectionMsx SortInfo = new SortInfoCollectionMsx();

		public LegendMsx Legend = new LegendMsx();

		public TrellisMsx Trellis = new TrellisMsx();

		/// <summary>
		/// Adjust secondary references for each Mobius.SpotfireDocument.xxxMsx class 
		/// including DataTableMsx, DataColumnMsx, VisualMsx, PageMsx ...
		/// </summary>

		public override void UpdatePreSerializationSecondaryReferences()
		{
			base.UpdatePreSerializationSecondaryReferences(); // Update refs for base VisualMsx members

			UpdatePreSerializationSecondaryReferences(Colorings);

			UpdatePreSerializationSecondaryReferences(Labels);

			UpdatePreSerializationSecondaryReferences(SortInfo);

			UpdatePreSerializationSecondaryReferences(Legend);

			UpdatePreSerializationSecondaryReferences(Trellis);

			return;
		}

		/// <summary>
		/// Adjust secondary references for each Mobius.SpotfireDocument.xxxMsx class 
		/// including DataTableMsx, DataColumnMsx, VisualMsx, PageMsx ...
		/// </summary>

		public override void UpdatePostDeserializationSecondaryReferences()
		{
			ValidateNode();

			base.UpdatePostDeserializationSecondaryReferences(); // Update refs for base VisualMsx members

			UpdatePostDeserializationSecondaryReferences(Colorings);

			UpdatePostDeserializationSecondaryReferences(Labels);

			UpdatePostDeserializationSecondaryReferences(SortInfo);

			UpdatePostDeserializationSecondaryReferences(Legend);

			UpdatePostDeserializationSecondaryReferences(Trellis);

			return;
		}
	}
}
