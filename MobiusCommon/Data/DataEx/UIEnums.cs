using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mobius.Data
{
	/// <summary>
	/// Flags for column selection 
	/// </summary>

	public enum SelectColumnFlags
	{
		SearchableOnly = 1, // searchable columns only
		NongraphicOnly = 2, // nongraphic columns only
		FirstTableKeyOnly = 4, // if key is selected return key for 1st table
		ExcludeKeys = 8, // keys not allowed
		ExcludeImages = 16, // images not allowed
		IncludeAllSelectableCols = 32, // include all selectable cols not just the selected cols
		SelectFromQueryOnly = 64, // select from query only, not database
		IncludeNoneItem = 128  // allow "none" column to be selected
	}
}
