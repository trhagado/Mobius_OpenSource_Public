using Mobius.ComOps;

using System;
using System.Collections;

namespace Mobius.Data
{
	public class CidListElement // implements Comparable 
	{
		public string Cid; // compound number
		public double Tag; // numeric tag info
		public string StringTag; // text tag info

		public CidListElement() 
		{
			Tag=-1; // null value for tag
		}

		public CidListElement(String cn) 
		{
			Tag=-1; // null value for tag
			this.Cid = cn;
		}

		public override bool Equals(Object e2) 
		{ // compare like string
			return this.Cid.Equals(e2.ToString());
		}

		public override int GetHashCode() 
		{ // hash like string
			return this.Cid.GetHashCode();
		}

		public int CompareTo(Object e2) 
		{
			return Cid.CompareTo(e2.ToString());
		}

		public override string ToString() 
		{
			return Cid;
		}


	} // end of CnListElement
}