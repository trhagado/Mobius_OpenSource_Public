using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mobius.MolLib2
{
	/// <summary>
	/// Wrapper for MolLib1 native Molecule class
	/// </summary>

	public class Molecule
	{
		public NativeMolecule NativeMolecule = null; // native format library molecule
		public string MolfileString = "";

		/// <summary>
		/// Basic constructor
		/// </summary>

		public Molecule()
		{
			return;
		}


		/// <summary>
		/// Construct from Molfile
		/// </summary>
		/// <param name="molfile"></param>

		public Molecule(string molfile)
		{
			MolfileString = molfile;
		}
	}

	/// <summary>
	/// Native library molecule format
	/// Replace this class with references to the native molecule associated with this wrapped molecule class
	/// </summary>

	public class NativeMolecule
	{
		// noop 
	}


}
