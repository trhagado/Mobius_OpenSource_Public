using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mobius.SpotfireDocument
{
	public class LegendMsx : NodeMsx
	{
		public bool Visible { get; set; }
		public int Width { get; set; }
		public List<LegendItemMsx> Items = new List<LegendItemMsx>();

		//public Font Font { get; set; }
		//public LegendDock Dock { get; set; }

		/// <summary>
		/// Update secondary object xxxMsx class object references to the
		/// associated Guid Ids prior to serialization.
		/// This method is normally overridden by each xxxMsx class to update 
		/// the references for that class.
		/// </summary>

		public override void UpdatePreSerializationSecondaryReferences()
		{
			foreach (LegendItemMsx item in Items)
			{
				UpdatePreSerializationSecondaryReferences(item);
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

			foreach (LegendItemMsx item in Items)
			{
				UpdatePostDeserializationSecondaryReferences(item);
			}

			return;
		}


	}

	/// <summary>
	/// LegendItemMsx - Spotfire LegendItem is virtual
	/// </summary>

	public abstract class LegendItemMsx : NodeMsx
	{
		public bool Visible { get; set; }
		public bool ShowTitle { get; set; }
		public string Title { get; set; }  // only a get for Spotfire LegendItem class
	}

	/// <summary>
	/// LegendItemMsx - Spotfire LegendItem is virtual
	/// </summary>

	public abstract class LegendAxisItemMsx : LegendItemMsx
	{
		public bool ShowAxisSelector { get; set; }
	}

	/// <summary>
	/// LegendGroupByItemMsx - Spotfire LegendGroupByItem is a real class, but with no added members
	/// </summary>
	public class LegendGroupByItemMsx : LegendAxisItemMsx 
	{	
		// No added members
	} 



}
