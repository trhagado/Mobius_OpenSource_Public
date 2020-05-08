using Mobius.ComOps;

using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Mobius.MolLib1
{
	public class MolLib1Util
	{
		/// <summary>
		/// Convert special atom isotope values to MDL V3000-type hilighting
		/// Add hilight collection to V3000 mol file resulting in a molfile 
		/// with a form like the following:
		///
		///M  V30 END BOND
		///M  V30 BEGIN COLLECTION
		///M  V30 MDLV30 / STEABS ATOMS = (1 4)
		///M  V30 MDLV30 / HILITE ATOMS = (13 10 11 12 34 35 36 37 38 39 40 41 42 43) -
		///M  V30 BONDS = (13 11 12 37 38 39 40 41 42 43 44 45 46 47)
		///M  V30 END COLLECTION
		///M  V30 END CTAB
		///M  END
		/// </summary>
		/// <param name="molfile"></param>
		/// <param name="hilightIsotopeValue"></param>
		/// <returns></returns>
		/// 

		// Note that isotope values are handled in different ways based on the form (molfile vs internal mol object)
		// and the software doing the conversion. 
		// For CDK, the molfile value and the internal object value are the same value, i.e. the total mass (number of protons & neutrons)
		// For MolLib1 the internal value is the normal current mass minus the normal mass. e.g. a carbon with mass = 1 will have an atom.Isotope value of -15;

		public static string ConvertIsotopeValuesToHilighting(
			string molfile)
		{
			bool hilight;
			int hilightIsotopeValue = 100;
			string molfile2 = "";
			bool clearAttachmentPointLabels = true;
			bool hilightAttachmentBond = true;

			throw new NotImplementedException();
		}

		/// <summary>
		/// GetNormalizedIsotope
		/// The isotope value is difference from the normal mass, e.g. Carbon 14 has a value of 2.
		/// This method (which is a bit of a hack) returns the the mass unless it is normal in which case it returns 0.
		/// </summary>
		/// <param name="a"></param>
		/// <returns></returns>
		static int GetNormalizedIsotope(
			object a)
		{
			throw new NotImplementedException();
		}


		/// <summary>
		/// Returns standard mass for specified element no, as integer
		/// </summary>
		/// <param name="atomicNumber"></param>
		/// <returns></returns>

		public static int GetMajorIsotopeMassNumber(int atomicNumber)
		{
			if (atomicNumber < 1 || atomicNumber >= MajorIsotopeMassNumber.Length)
				throw new Exception("Invalid atomic number: " + atomicNumber);

			int mass = MajorIsotopeMassNumber[atomicNumber];
			return mass;
		}

		/// <summary>
		/// Isotope Conversion Test
		/// </summary>
		/// <param name="smiles"></param>
		/// <returns></returns>

		public static string IsotopeConversionTest(
			string smiles)
		{
			// Initial mol object creation from Smiles

			string msg =
				"MolLib1 Conversion Test, Smiles: " + smiles + "\r\n\r\n";

			throw new NotImplementedException();

			//string molfile1 = MolLib1StructureConverter.SmilesStringToMolfileString(smiles);
			//msg += "Smiles -> MoleLib1 V2000\r\n==================================\r\n " + molfile1 + "\r\n\r\n";

			//NativeMolecule mol1 = MolLib1StructureConverter.MolfileStringToMolecule(molfile1);

			////mol1.GetAtom(0).Isotope = -1; // try neg value
			//int isotope1 = mol1.GetAtom(0).Isotope;
			//msg += "Smiles -> Obj isotope: " + isotope1 + "\r\n\r\n";

			//// Mol object to V2000 & back

			//string v2000 = MolLib1StructureConverter.MoleculeToMolfileString(mol1); // obj to molfile
			//msg += "Obj -> MoleLib1 V2000\r\n==================================\r\n " + v2000 + "\r\n\r\n";

			//NativeMolecule molV2000 = MolLib1StructureConverter.MolfileStringToMolecule(v2000); // molfile to obj
			//int isotopeV2 = molV2000.GetAtom(0).Isotope;
			//msg += "V2000 -> Obj isotope: " + isotopeV2 + "\r\n\r\n";
			//string v2000b = MolLib1StructureConverter.MoleculeToMolfileString(molV2000); // obj back to molfile

			//// Mol object to V3000 & back

			//mol1.HighlightChildren = "1;"; // force V3000
			//string v3000 = MolLib1StructureConverter.MoleculeToMolfileString(mol1); // obj to molfile
			//msg += "Obj -> MoleLib1 V3000\r\n==================================\r\n " + v3000 + "\r\n\r\n";

			//NativeMolecule molV3000 = MolLib1StructureConverter.MolfileStringToMolecule(v3000); // molfile to obj
			//int isotopeV3 = molV3000.GetAtom(0).Isotope;
			//msg += "V3000 -> Obj isotope: " + isotopeV3 + "\r\n\r\n";
			//string v3000b = MolLib1StructureConverter.MoleculeToMolfileString(molV3000); // obj back to molfile

			//return msg;
		}

		/// <summary>
		/// Table of major isotope mass numbers indexed by atomic number
		/// </summary>

		public static int[] MajorIsotopeMassNumber = {
			/* 0 */ 0,
			/* 1 */ 1,
			/* 2 */ 4,
			/* 3 */ 7,
			/* 4 */ 9,
			/* 5 */ 11,
			/* 6 */ 12,
			/* 7 */ 14,
			/* 8 */ 16,
			/* 9 */ 19,
			/* 10 */ 20,
			/* 11 */ 23,
			/* 12 */ 24,
			/* 13 */ 27,
			/* 14 */ 28,
			/* 15 */ 31,
			/* 16 */ 32,
			/* 17 */ 35,
			/* 18 */ 40,
			/* 19 */ 39,
			/* 20 */ 40,
			/* 21 */ 45,
			/* 22 */ 48,
			/* 23 */ 51,
			/* 24 */ 52,
			/* 25 */ 55,
			/* 26 */ 56,
			/* 27 */ 59,
			/* 28 */ 58,
			/* 29 */ 63,
			/* 30 */ 64,
			/* 31 */ 69,
			/* 32 */ 74,
			/* 33 */ 75,
			/* 34 */ 80,
			/* 35 */ 79,
			/* 36 */ 84,
			/* 37 */ 85,
			/* 38 */ 88,
			/* 39 */ 89,
			/* 40 */ 90,
			/* 41 */ 93,
			/* 42 */ 98,
			/* 43 */ 85,
			/* 44 */ 102,
			/* 45 */ 103,
			/* 46 */ 106,
			/* 47 */ 107,
			/* 48 */ 114,
			/* 49 */ 115,
			/* 50 */ 120,
			/* 51 */ 121,
			/* 52 */ 130,
			/* 53 */ 127,
			/* 54 */ 132,
			/* 55 */ 133,
			/* 56 */ 138,
			/* 57 */ 139,
			/* 58 */ 140,
			/* 59 */ 141,
			/* 60 */ 142,
			/* 61 */ 126,
			/* 62 */ 152,
			/* 63 */ 153,
			/* 64 */ 158,
			/* 65 */ 159,
			/* 66 */ 164,
			/* 67 */ 165,
			/* 68 */ 166,
			/* 69 */ 169,
			/* 70 */ 174,
			/* 71 */ 175,
			/* 72 */ 180,
			/* 73 */ 181,
			/* 74 */ 184,
			/* 75 */ 187,
			/* 76 */ 192,
			/* 77 */ 193,
			/* 78 */ 195,
			/* 79 */ 197,
			/* 80 */ 202,
			/* 81 */ 205,
			/* 82 */ 208,
			/* 83 */ 209,
			/* 84 */ 188,
			/* 85 */ 193,
			/* 86 */ 195,
			/* 87 */ 199,
			/* 88 */ 202,
			/* 89 */ 206,
			/* 90 */ 232,
			/* 91 */ 231,
			/* 92 */ 238,
			/* 93 */ 225,
			/* 94 */ 228,
			/* 95 */ 231,
			/* 96 */ 233,
			/* 97 */ 235,
			/* 98 */ 237,
			/* 99 */ 240,
			/* 100 */ 242,
			/* 101 */ 245,
			/* 102 */ 248,
			/* 103 */ 251 };

	}
}
