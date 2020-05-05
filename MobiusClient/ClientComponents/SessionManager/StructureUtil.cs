using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;

using System;
using System.IO;
using System.Drawing;
using System.Text;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	/// <summary>
	/// Summary description for Structure.
	/// </summary>
	public class StructureUtil
	{
		public const int STD_BOND_LENGTH  = 200; // standard bond length milliinches
		public const int STD_FONT_SIZE    =  90; // standard font size in decipoints

		public static string [] HydrogenDisplayTypes = 
		{	"off", "hetero", "terminal", "all"};

		static string lastControlName;
		static int lastWidth = 0;
		static int lastHeight = 0;

/// <summary>
/// Read a structure file on the client
/// </summary>
/// <param name="cn"></param>
/// <returns></returns>

		public static MoleculeMx Read (
			string fileName)
		{
			FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
			Byte[] ba = new byte[fs.Length];

			fs.Read(ba, 0, ba.Length);
			fs.Close();

			throw new NotImplementedException();

			//string molFile = sc.MolfileString;
			//MoleculeMx cs = new MoleculeMx(MoleculeFormat.Molfile, molFile);
			//return cs;
		}

/// <summary>
/// Copy the structure associated with the specified compound id to the clipboard
/// </summary>
/// <param name="cn"></param>
/// <param name="mt"></param>
/// <returns></returns>

		public static bool CopyCompoundIdStructureToClipBoard ( 
			string cn,
			MetaTable mt)
		{
			MoleculeMx mol = MoleculeUtil.SelectMoleculeForCid(cn, mt);
			if (mol != null)
			{
				MoleculeControl.CopyMoleculeToClipboard(mol);
				return true;
			}
			else 
			{
				MessageBoxMx.Show("Unable to retrieve structure");
				return false;
			}
		}

		/// <summary>
		/// Fix HCl etc groups for proper left side H display
		/// </summary>
		/// <param name="skid"></param>
		/// <returns></returns>

		public static int FixSingleAtomHydrogens(
			MoleculeMx cs)
		{
			int atmcnt, ai, oldmode, fixcnt, atype, i1, i2, i3;
			string fontname;
			int size, emphasis;
			double x = 0, y = 0, z = 0;
			int aid, na, nb, sgrp;
			long t1, t2;
			string lp;

			return 0;
		}
	}
}
