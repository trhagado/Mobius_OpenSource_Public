using System;

namespace Mobius.Data
{
	/// <summary>
	/// Summary description for Hyperlink.
	/// </summary>

	public class Hyperlink : IComparable
	{
		public string Text = ""; // text to be displayed
		public string URI = ""; // Universal Resourse Identifier

		public Hyperlink()
		{
		}

		/// <summary>
		/// Compare two link text values (IComparable.CompareTo)
		/// </summary>
		/// <param name="o"></param>
		/// <returns></returns>
		public int CompareTo (
			object o)
		{
			if (o == null) return 1; // object is null so this is greater
			else return this.Text.CompareTo(((Hyperlink)o).Text);
		}
	}
}
