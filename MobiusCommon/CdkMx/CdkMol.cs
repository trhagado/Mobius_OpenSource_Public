using Mobius.ComOps;
using Mobius.Data;

using NCDK;
using NCDK.Aromaticities;
using NCDK.Config;
using NCDK.Default;
using NCDK.Depict;
using NCDK.Fingerprints;
using NCDK.Graphs;
using NCDK.Graphs.InChI;
using NCDK.Isomorphisms;
using NCDK.Isomorphisms.Matchers;
using NCDK.IO;
using NCDK.IO.Iterator;
using NCDK.Layout;
//using NCDK.Silent;
using NCDK.Smiles;
using NCDK.Tools;
using NCDK.Tools.Manipulator;

using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;

namespace Mobius.CdkMx
{

	/// <summary>
	/// Wrapper for native CDK Molecule class
	/// </summary>

	public partial class CdkMol : ICdkMol
	{
		public MoleculeMx MoleculeMx; // Parent MoleculeMx that we are supporting (if any)
		public IAtomContainer NativeMol = null; // native format library molecule that matches the current mol

		internal int Id = InstanceCount++; // id of this instance
		internal static int InstanceCount = 0; // instance count


		/// <summary>
		/// Get the molecule value in a molfile format
		/// </summary>
		/// <returns></returns>

		public string GetMolfile()
		{
			if (NativeMol == null) return null;

			string molfile = AtomContainerToMolfile(NativeMol);
			return molfile;
		}

        /// <summary>
        /// Get the molecule value as a smiles string
        /// </summary>
        /// <returns></returns>

        public string GetSmiles()
        {
            if (NativeMol == null) return null;

            string molfile = AtomContainerToSmiles(NativeMol);
            return molfile;
        }

        /// <summary>
        /// Get molecule as a CompactMolecule
        /// </summary>
        /// <returns></returns>

        public CompactMolecule GetMolecule()
		{
			return new CompactMolecule(_molFormat, _molString);
		}

/// <summary>
/// Get molecule
/// </summary>
/// <param name="molFormat"></param>
/// <param name="molString"></param>

		public void GetMolecule(
			out MoleculeFormat molFormat,
			out string molString)
		{
			molFormat = _molFormat;
			molString = _molString;
		}

		/// <summary>
		/// Set molecule value including the associated native CDK IAtomContainer
		/// </summary>
		/// <param name="molFormat"></param>
		/// <param name="molString"></param>

		public void SetMolecule(
			MoleculeFormat molFormat,
			string molString)
		{
			if (molFormat == _molFormat && Lex.Eq(molString, _molString))
				return; // no change

			if (molFormat == MoleculeFormat.Molfile)
			{
				NativeMol = MolfileToAtomContainer(molString);
			}

			else if (molFormat == MoleculeFormat.Chime)
			{
				string molfile = MoleculeMx.ChimeStringToMolfileString(molString);
				NativeMol = MolfileToAtomContainer(molfile);
			}

			else if (molFormat == MoleculeFormat.Smiles)
			{
				NativeMol = SmilesToAtomContainer(molString);
			}

			else if (molFormat == MoleculeFormat.Unknown)
			{
				NativeMol = null;
			}

			_molFormat = molFormat;
			_molString = molString;
		}

		/// <summary>
		/// Internal molecule, should be reflected in native CDK IAtomContainer
		/// </summary>

		private MoleculeFormat _molFormat = MoleculeFormat.Unknown; // internal mol format
		private string _molString = null; // internal mol string

		/// <summary>
		/// Basic constructor
		/// </summary>

		public CdkMol()
		{
			return;
		}

/// <summary>
/// Construct and assign parent IMoleculeMx 
/// </summary>
/// <param name="parent"></param>

		public CdkMol(MoleculeMx parent)
		{
			MoleculeMx = parent;
			return;
		}

		/// <summary>
		/// Construct from mol format and string
		/// </summary>

		public CdkMol(
			MoleculeFormat molFormat,
			string molString)
		{
			if (Lex.IsUndefined(molString)) return; // create new structure

			SetMolecule(molFormat, molString);

			return;
		}

		/// <summary>
		/// Update native molecule to match parent MoleculeMx
		/// </summary>

		public void UpdateNativeMolecule()
		{
			if (MoleculeMx == null) return;

			SetMolecule(MoleculeMx.PrimaryFormat, MoleculeMx.PrimaryValue);
			return;
		}

		/// <summary>
		/// Check if a string is a valid Molfile
		/// </summary>
		/// <param name="molfile"></param>
		/// <returns></returns>

		public bool IsValidMolfile(string molfile)
		{
			if (Lex.IsUndefined(molfile)) return false;

			try
			{
				IAtomContainer m = MolfileToAtomContainer(molfile);
				if (m != null) return true;
				else return false;
			}
			catch (Exception ex)
			{
				return false;
			}

		}

		/// <summary>
		/// Scale and translate structure into supplied rectangle.
		/// This method involves scaling,
		/// translating, and adjustments to font sizes and line widths.
		/// </summary>
		/// <param name="destRect">Destination rect to fit into, bottom may be adjusted</param>
		/// <param name="desiredBondLength">0=no scale, 1=bond length for "standard" size box, >1 = desired bond length in milliinches</param>
		/// <param name="translateType">0=no xlate, 1=xlate, 2= xlate to upper left</param>
		/// <param name="fixedHeight">0=no, 1=yes, 2=yes unless too tall to fit</param>
		/// <param name="markBoundaries">True to mark edges of structure</param>
		/// <returns>New bounding rectangle for structure in milliinches</returns>

		public void FitStructureIntoRectangle(
			ref Rectangle destRect,
			int desiredBondLength,
			int translateType,
			int fixedHeight,
			bool markBoundaries,
			int pageHeight,
			out Rectangle boundingRect)
		{
			bool adjustHeight;
			double sleft = 0, stop = 0, sright = 0, sbottom = 0, swidth, sheight, sscale, yscale, x, y, dx = 0, dy = 0, bondlength;
			double cleft, ctop, cright, cbottom;
			double dleft, dright, dtop, dbottom;
			double dwidth, dwidth2, dheight, avgBondLen, scale1, boxscale, d1;
			int objid, objcnt, objtype, oi;
			int width, height, linewidth, fontsize, oldmode, pass, rc, i1, i2, i3;
			long t1, t2;
			String sdata;

			boundingRect = new Rectangle();
			return; // todo -	throw new NotImplementedException();
		}

		/// <summary>
		/// Get bounding box for molecule
		/// </summary>

		public RectangleF GetBoundingBox()
		{
			IAtomContainer mol = NativeMol;

			float left = 0, right = 0, top = 0, bottom = 0;
			throw new NotImplementedException();
		}

		/// <summary>
		/// Adjust bond length to valid range
		/// </summary>
		/// <param name="bondLen"></param>
		/// <returns></returns>

		public int AdjustBondLengthToValidRange(int bondLen)
		{
			//throw new NotImplementedException();
			// todo
			return bondLen; 
		}

		/// <summary>
		/// Position implicit hydrogens on certain single atoms in the preferred left position
		/// </summary>
		/// <param name="m"></param>
		/// <returns></returns>

		public int FixSingleAtomHydrogens()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Set the caption for a structure
		/// </summary>
		/// <param name="caption">Text of caption</param>

		public void CreateStructureCaption(
			string caption)
		{
			byte[] ba;
			FileStream fs;
			float top, bottom, left, right, height, width, strBottom, hCenter, drop;
			int txtLen;
			string fieldName = "StructureCaption=";
			string molComments = "";

			throw new NotImplementedException();
		}

		/// <summary>
		/// Remove stereochemistry
		/// </summary>
		/// <param name="m"></param>
		/// <returns></returns>

		public bool RemoveStereochemistry()
		{
			int stereoAtoms = 0, stereoBonds = 0;

			throw new NotImplementedException();
		}

		/// <summary>
		/// Remove any existing structure captions
		/// </summary>
		/// <param name="m"></param>
		/// <returns></returns>

		public bool RemoveStructureCaption()
		{
			string fieldName = "StructureCaption=";
			int removedCnt = 0;
			int sgi = 0;


			//if (PrimaryFormat == MoleculeFormat.Chime)
			//	ChimeString = mol.ChimeString;

			//else
			//{
			//	PrimaryFormat = MoleculeFormat.Molfile;
			//	MolfileString = mol.MolfileString;
			//}

			throw new NotImplementedException();
		}

		/// <summary>
		/// Remove SuperAtom Info
		/// </summary>
		/// <returns></returns>

		public bool RemoveSuperAtomInfo()
		{
			int i1, i2;

			throw new NotImplementedException();
		}

		/// <summary>
		/// Transform molecule according to flag settings
		/// </summary>
		/// <param name="flags"></param>
		/// <param name="name">Name to go in first line</param>
		/// <returns></returns>

		public void TransformMolecule(
			MoleculeTransformationFlags flags,
			string name)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// GetLargestFragmentOnly
		/// </summary>
		/// <returns></returns>

		bool GetLargestFragmentOnly()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Remove Hydrogens
		/// </summary>
		/// <returns></returns>

		bool RemoveHydrogens()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Check Molecule/Molfile conversion
		/// </summary>
		/// <param name="molfile1"></param>
		/// <param name="molfile2"></param>
		/// <returns></returns>

		public static bool TestInternalMoleculeConversion(string molfile1, out string molfile2)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Find midpoint between two double values
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		/// <returns></returns>

		public static double Midpoint(double v1, double v2)
		{
			return v1 + (v2 - v1 + 1) / 2;
		}

		// Unit Converters 

		public static int MilliinchesToDecipoints(
		int mi)
		{
			return (int)(mi * (720.0 / 1000.0));
		}

		public static int DecipointsToMilliinches(
			int dp)
		{
			return (int)(dp * (1000.0 / 720.0));
		}

		public static int PixelsToMilliinches(
			int pixelCount)
		{
			return (int)(pixelCount / 96.0 * 1000.0); // assume 96 pixels/inch
		}

		public static int MilliinchesToPixels(
			int milliinches)
		{
			return (int)(milliinches / 1000.0 * 96.0); // assume 96 pixels/inch
		}

	}

}
