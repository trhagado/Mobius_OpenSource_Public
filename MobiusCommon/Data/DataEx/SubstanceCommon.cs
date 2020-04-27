using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mobius.Data.DataEx
{
	/// <summary>
	/// Substance Libraries (CORP_OWNER.SUBSTANCE_LIBRARY table)
	/// </summary>

	public class SubstanceLibrary
	{
		public int LibId = -1; // library id
		public string LibraryName = "";
		public string LibraryDescText = "";
		public int LibTypeId = -1;
		public int CrtPrsnId = -1; // system id of creator
		public DateTime CrtTimestamp = DateTime.MinValue;
	}
}
