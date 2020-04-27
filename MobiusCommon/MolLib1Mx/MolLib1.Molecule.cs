using Mobius.ComOps;

using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;

namespace Mobius.MolLib1
{

		/// <summary>
		/// Wrapper for MolLib1 native Molecule class
		/// </summary>

	public class Molecule
	{
		public NativeMolecule NativeMolecule = null; // native format library molecule

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

		/// <summary>
		/// Construct from Molfile or ChimeString
		/// </summary>
		/// <param name="molfile"></param>

		public Molecule(
			StructureType structureType,
			string molString)
		{
			if (Lex.IsUndefined(molString)) return; // create new structure

			else if (structureType == StructureType.MolFile)
				MolfileString = molString;

			else if (structureType == StructureType.Chime)
				ChimeString = molString;

			else throw new ArgumentException("Unsupported StructureFormat: " + structureType);

			return;
		}

		/// <summary>
		/// MolfileString form of molecule
		/// </summary>

		public string MolfileString
		{
			get
			{
				if (NativeMolecule != null)
					throw new NotImplementedException();

				else return "";
			}

			set
			{
				if (Lex.IsDefined(value))
				{
					throw new NotImplementedException();
				}

				else NativeMolecule = new NativeMolecule();
			}
		}

		/// <summary>
		/// ChimeString form of molecule
		/// </summary>

		public string ChimeString
		{
			get
			{
				if (NativeMolecule != null)
				{
					string molFile = StructureConverter.NativeMoleculeToMolfileString(NativeMolecule);
					return StructureConverter.MolfileStringToChimeString(molFile);
				}

				else return "";
			}

			set
			{
				if (Lex.IsDefined(value))
				{
					string molFile = StructureConverter.ChimeStringToMolfileString(value);
					NativeMolecule = StructureConverter.MolfileStringToNativeMolecule(molFile);
				}

				else NativeMolecule = new NativeMolecule();
			}
		}

		/// <summary>
		/// Get atom count for structure
		/// </summary>
		/// <returns></returns>

		public int AtomCount
		{
			get { throw new NotImplementedException(); }
		}

		/// <summary>
		/// Get heavy atom count
		/// </summary>

		public int HeavyAtomCount
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Return true if mol contains a query feature
		/// </summary>

		public bool ContainsQueryFeature
		{
			get { throw new NotImplementedException(); }
		}

		/// <summary>
		/// Compare "molweight" of two structures
		/// </summary>
		/// <param name="o"></param>
		/// <returns></returns>

		public static int CompareMolWeight(string molfile1, string molfile2)
		{
			throw new NotImplementedException();
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

		public void FitStructure(
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
			throw new NotImplementedException();
		}

		/// <summary>
		/// Get bounding box for molecule
		/// </summary>

		public RectangleF GetBoundingBox()
		{
			NativeMolecule mol = NativeMolecule;

			float left = 0, right = 0, top = 0, bottom = 0;
			throw new NotImplementedException();
		}

		/// <summary>
		/// Render molecule into bitmap of specified size.
		/// </summary>
		/// <param name="bitmapWidth"></param>
		/// <param name="bitmapHeight"></param>
		/// <param name="dp"></param>
		/// <returns></returns>

		public Bitmap GetMoleculeBitmap(
			int bitmapWidth,
			int bitmapHeight,
			DisplayPreferences dp)
		{
			byte[] ba;
			FileStream fs;
			StructureConverter sc;
			float top, bottom, left, right, height, width, strBottom, hCenter, drop, stdBndLen, scale, fontSize, bondThickness;
			int txtLen;
			Bitmap bm;

			throw new NotImplementedException();
		}

		/// <summary>
		/// Adjust bond length to valid range
		/// </summary>
		/// <param name="bondLen"></param>
		/// <returns></returns>

		public static int AdjustBondLengthToValidRange(int bondLen)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Position implicit hydrogens on certain single atoms in the preferred left position
		/// </summary>
		/// <param name="m"></param>
		/// <returns></returns>

		public int FixSingleAtomHydrogens()
		{
			NativeMolecule m = NativeMolecule;
			throw new NotImplementedException();
		}

		/// <summary>
		/// Contract all abbreviations in a molecule
		/// </summary>
		/// <param name="skid"></param>

		static void ContractAbbreviations()
		{
			throw new Exception("Not implemented");

			//int oldMode = SetSketchMode(skid, SKMODE_CHEM); // must be in chem mode
			//Ol.CollectAllSkObj(skid, 1); // put all objects in collection 1
			//Ol.ContractMapAbbrevs(skid, false, 0, 0, 0, 1); // contract abbrvs
			//SetSketchMode(skid, oldMode);
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
			StructureConverter sc;
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

			NativeMolecule m = NativeMolecule; // operate on active molecule
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

			NativeMolecule m = NativeMolecule;
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
		/// Convert structure according to flag settings
		/// </summary>
		/// <param name="molfile"></param>
		/// <param name="flags"></param>
		/// <param name="name">Name to go in first line</param>
		/// <returns></returns>

		public Molecule Convert(
			int intFlags,
			string name)
		{
			int i1, i2, optionsProcessed = 0;

			MoleculeTransformationFlags flags = (MoleculeTransformationFlags)intFlags;

			NativeMolecule m = NativeMolecule;

			throw new NotImplementedException();
		}

		/// <summary>
		/// GetLargestFragmentOnly
		/// </summary>
		/// <returns></returns>

		bool GetLargestFragmentOnly()
		{
			NativeMolecule m = NativeMolecule;

			throw new NotImplementedException();
		}

		/// <summary>
		/// Remove Hydrogens
		/// </summary>
		/// <returns></returns>

		bool RemoveHydrogens()
		{
			NativeMolecule m = NativeMolecule;

			throw new NotImplementedException();
		}

		/// <summary>
		/// Get list of RNumbers and a count of instances for each
		/// </summary>
		/// <param name="rgCounts"></param>
		/// <param name="totalRgCount"></param>

		public void GetCoreRGroupInfo(
			out SortedDictionary<int, int> rgCounts,
			out int totalRgCount)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Remove RGroup attachment point atoms (code is incomplete)
		/// </summary>
		/// <returns></returns>

		public NativeMolecule RemoveRGroupAttachmentPointAtoms()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Convert to Metafile 
		/// </summary>
		/// <returns></returns>

		public Metafile GetMetaFile(
			int width,
			int height)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Get Standard display preferences
		/// </summary>
		/// <returns></returns>

		public static DisplayPreferences GetStandardDisplayPreferences()
		{
			DisplayPreferences dp = new DisplayPreferences();
			SetStandardDisplayPreferences(dp);
			return dp;
		}

		/// <summary>
		/// Set standard display preferences for a Renderer/Reditor control
		/// </summary>
		/// <returns></returns>

		public static DisplayPreferences SetStandardDisplayPreferences(MoleculeControl molCtl)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Set standard display preferences for a renderer
		/// </summary>
		/// <param name="dp"></param>

		public static void SetStandardDisplayPreferences(
			DisplayPreferences dp)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// One-time initialization of display preferences
		/// </summary>

		static void InitializeDisplayPreferences()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Use new (2013+) stereo display preferences
		/// </summary>
		/// <param name="dp"></param>

		public static void SetNewStereoDisplayPreferences(
			DisplayPreferences dp)
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

	/// <summary>
	/// Native library molecule format
	/// Replace this class with references to the native molecule associated with this wrapped molecule class
	/// </summary>

	public class NativeMolecule
	{
		public string HighlightChildren = "";
		public Color HighlightColor = Color.Blue;
	}

	/// <summary>
	/// Native library DisplayPreferences
	/// </summary>

	public class DisplayPreferences
	{
		public double StandardBondLength = -1;
		public HydrogenDisplayMode HydrogenDisplayMode = HydrogenDisplayMode.HeteroOrTerminal;
		public Color BackColor = Color.Transparent;
	}

	public delegate void EditorReturnedHandler(object sender, EditorReturnedEventArgs e);

	/// <summary>
	/// Native molecule control class
	/// </summary>

	public class MoleculeControl : System.Windows.Forms.ContainerControl
	{
		public DisplayPreferences Preferences = null;

		public event EventHandler StructureChanged;

		public event EditorReturnedHandler EditorReturned;

		/// <summary>
		/// Set control molecule
		/// </summary>
		/// <param name="mol"></param>

		public void SetMolecule(Molecule mol)
		{
			throw new NotImplementedException();
		}

/// <summary>
/// Set control molecule from molfile string
/// </summary>

		public string MolfileString
		{
			get	
			{
				return ""; // throw new NotImplementedException();
			}

			set 
			{
				return; // throw new NotImplementedException();
			}
		}

		public DisplayPreferences DisplayPreferences;

		public bool CanPaste => throw new NotImplementedException();

		public bool CanCopy => throw new NotImplementedException();

		public void PasteFromClipboard()
		{
			throw new NotImplementedException();
		}

		public void CopyToClipboard()
		{
			throw new NotImplementedException();
		}

		public Bitmap PaintMolecule(object d, StructureType structureType, int width, int height)
		{
			throw new NotImplementedException();
		}


		/// <summary>
		/// Get current version
		/// </summary>
		/// <returns></returns>
		public static string GetVersion()
		{
			throw new NotImplementedException();
			//Renderer hr = new Renderer(); // causes exception if not installed
			//Assembly a = hr.GetType().Assembly;
			//string codeBase = a.CodeBase; // needed in html references
			//string version = a.GetName().Version.ToString();
			//return version;
		}

		/// <summary>
		/// Edit the structure in the specified Renditor
		/// </summary>
		/// <param name="renditor"></param>

		public static void EditStructure(
			MoleculeControl renditor)
		{
			AssertMx.IsNotNull(renditor, "renditor");

			try
			{
				throw new NotImplementedException();
			}

			catch (Exception ex)
			{
				string msg =
					"An error has occurred starting the molecule editor";

				DialogResult dr = MessageBox.Show(msg, "Error starting molecule editor", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		public static void SetStandardDisplayPreferences(MoleculeControl molCtl)
		{
			return; // throw new NotImplementedException();
		}

	}

	public class EditorReturnedEventArgs : EventArgs
	{
		public bool Validated;
	}

	/// <summary>
	/// StructureFormat
	/// </summary>

	public enum StructureType
	{
		Unknown = 0,
		MolFile = 1,
		Smiles = 2,
		Chime = 3
	}

	public enum HPositionEnum
	{
		OFF = -1,
		AUTO = 0,
		RIGHT = 1,
		LEFT = 2,
		TOP = 3,
		BOTTOM = 4,
		DOT = 5,
		CIRCLE = 6
	}
	public enum HDisplayEnum
	{
		OFF = 0,
		HETERO = 1,
		HETEROTERMINAL = 2,
		ALL = 3,
		NONE = 4
	}
	public enum HydrogenDisplayMode
	{
		Off = 0,
		Hetero = 1,
		Terminal = 2,
		HeteroOrTerminal = 3,
		All = 4
	}
	public enum StereoParityEnum
	{
		UNKNOWN = 0,
		ODD = 1,
		EVEN = 2,
		EITHER = 3
	}

}
