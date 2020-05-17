using Mobius.ComOps;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// This file contains the base Interface and Enums to support specific Molecule Libraries (e.g. CDK)
/// </summary>

namespace Mobius.Data
{
	/// <summary>
	/// Interface for a MoleculeMx object that is the parent of an ICdkMol object
	/// </summary>

	public interface IMoleculeMx
	{
		MoleculeFormat PrimaryFormat { get; }
		string PrimaryValue { get; }
	}

	/// <summary>
	/// Class containing injected public static instance of CdkMolFactory used to create CdkMol instances
	/// 
	/// </summary>

	public class CdkMolFactory
	{
		/// <summary>
		/// Create basic CdkMol instance
		/// </summary>
		/// <returns></returns>
		/// 
		public static ICdkMol NewCdkMol()
		{
			return I.NewCdkMol();
		}

		/// <summary>
		/// Create CdkMol instance from MoleculeMx
		/// </summary>
		/// <param name="molMx"></param>
		/// <returns></returns>

		public static ICdkMol NewCdkMol(MoleculeMx molMx)
		{
			return I.NewCdkMol(molMx);
		}

		/// <summary>
		/// Construct from mol format and string
		/// </summary>
		/// <param name="molFormat"></param>
		/// <param name="molString"></param>
		/// <returns></returns>

		public static ICdkMol NewCdkMol(
			MoleculeFormat molFormat,
			string molString)
		{
			return I.NewCdkMol(molFormat, molString);
		}


		/// <summary>
		/// Injected instance of CdkMolFactory
		/// </summary>

		public static ICdkMolFactory I
		{
			get
			{
				if (i != null) return i;
				throw new NullReferenceException("CdkMolFactory instance not defined");
			}

			set => i = value;

		}
		static ICdkMolFactory i = null;
	}

	/// <summary>
	/// Interface for factory that creates CdkMol instances
	/// </summary>

	public interface ICdkMolFactory
	{
		/// <summary>
		/// Create basic CdkMol instance
		/// </summary>
		/// <returns></returns>

		ICdkMol NewCdkMol();

		/// <summary>
		/// Create CdkMol instance from MoleculeMx
		/// </summary>
		/// <param name="molMx"></param>
		/// <returns></returns>

		ICdkMol NewCdkMol(MoleculeMx molMx);

		/// <summary>
		/// Construct from mol format and string
		/// </summary>
		/// <param name="molFormat"></param>
		/// <param name="molString"></param>
		/// <returns></returns>

		ICdkMol NewCdkMol(
			MoleculeFormat molFormat,
			string molString);

	}

	/// <summary>
	/// Class to provide access to a static CdkMol class
	/// </summary>

	public class StaticCdkMol
	{

		public static ICdkMol I => GetCdkMolInstance();

		static ICdkMol GetCdkMolInstance()
		{
			if (i == null) // get instance if not done yet
				i = CdkMolFactory.NewCdkMol();

			return i;
		}

		static ICdkMol i = null;
	}

	/// <summary>
	/// Interface to MoleculeLibrary (e.g. Cdk) functionality for MoleculeMx Class
	/// </summary>

	public interface ICdkMol
	{

		void UpdateNativeMolecule(); // update native molecule to 

		bool IsValidMolfile(string molfile);

		/// <summary>
		/// Get molecule
		/// </summary>
		/// <returns></returns>

		CompactMolecule GetMolecule();

		/// <summary>
		/// Get molecule
		/// </summary>
		/// <param name="molFormat"></param>
		/// <param name="molString"></param>

		void GetMolecule(
			out MoleculeFormat molFormat,
			out string molString);

		/// <summary>
		/// Set molecule value including the associated native CDK IAtomContainer
		/// </summary>
		/// <param name="molFormat"></param>
		/// <param name="molString"></param>

		void SetMolecule(
			MoleculeFormat molFormat,
			string molString);

		/// <summary>
		/// Get the molecule value in a molfile format
		/// </summary>
		/// <returns></returns>

		string GetMolfile();

		/// <summary>
		/// Get the molecule value as a smiles string
		/// </summary>
		/// <returns></returns>

		string GetSmiles();

		int AtomCount { get; } // Get atom count for molecule

		int HeavyAtomCount { get; } // Get heavy atom count

		bool ContainsQueryFeature { get; } // Return true if mol contains a query feature

		double MolWeight { get; } // Get mol weight for a molecule 

		string MolFormula { get; } // Get mol formula

		/// <summary>
		///  Get the sum of the heavy atom and bond counts for a smiles string
		/// </summary>
		/// <param name="smiles"></param>
		/// <param name="haCnt"></param>
		/// <param name="hbCnt"></param>

		void GetHeavyAtomBondCounts(
			string smiles,
			out int haCnt,
			out int hbCnt);

		/// <summary>
		/// Convert an Inchi string to a molfile
		/// </summary>
		/// <param name="inchiString"></param>
		/// <returns></returns>

		string InChIStringToMolfileString(string inchiString);

		/// <summary>
		/// Convert a molfile string to a Smiles string
		/// </summary>
		/// <param name="molfile"></param>
		/// <returns></returns>

		string MolfileStringToSmilesString(string molfile);

		/// <summary>
		/// Convert a Smiles string to a Molfile string
		/// </summary>
		/// <param name="smiles"></param>
		/// <returns></returns>

		string SmilesStringToMolfileString(string smiles);

		object BuildBitSetFingerprint(
			string molfile,
			FingerprintType fpType,
			int fpSubtype,
			int fpLen);

		/// <summary>
		/// Get the largest molecule fragment from supplied molfile
		/// </summary>
		/// <param name="molfile"></param>
		/// <returns>Molfile of largest fragment</returns>

		string GetLargestMolfileMoleculeFragment(
			string molfile);
		/// <summary>
		/// Get the largest molecule fragment from supplied smiles
		/// </summary>
		/// <param name="smiles"></param>
		/// <returns>Smiles of largest fragment</returns>

		string GetLargestSmilesMoleculeFragment(
			string smiles);

		/// <summary>
		/// Calculate similarity of two Bitsetfingerprints
		/// </summary>
		/// <param name="queryFingerprint"></param>
		/// <param name="targetFingerprint"></param>
		/// <returns></returns>

		double CalculateBitSetFingerprintSimilarity(object queryFingerprint, object targetFingerprint);

		/// <summary>
		/// Get array of set bits
		/// </summary>
		/// <param name="fingerprint"></param>
		/// <returns></returns>

		IEnumerable<int> GetBitSet(object fingerprint);

		void FitStructureIntoRectangle(
			ref Rectangle destRect,
			int desiredBondLength,
			int translateType,
			int fixedHeight,
			bool markBoundaries,
			int pageHeight,
			out Rectangle boundingRect);

		/// <summary>
		/// Render molecule into bitmap of specified size.
		/// </summary>
		/// <param name="bitmapWidth"></param>
		/// <param name="bitmapHeight"></param>
		/// <param name="dp"></param>
		/// <returns></returns>

		Bitmap GetMoleculeBitmap(
		 int bitmapWidth,
		 int bitmapHeight,
		 DisplayPreferences dp = null);

		/// <summary>
		/// Get structure bitmap with optional caption
		/// </summary>
		/// <param name="pixWidth"></param>
		/// <param name="pixHeight"></param>
		/// <param name="desiredBondLength">Desired length in milliinches</param>
		/// <param name="cellStyle"></param>
		/// <param name="caption"></param>
		/// <returns></returns>

		Bitmap GetFixedHeightMoleculeBitmap(
			int pixWidth,
			int pixHeight,
			DisplayPreferences dp,
			CellStyleMx cellStyle,
			string caption);

		/// <summary>
		/// Render molecule into svg of specified size.
		/// </summary>
		/// <param name="bitmapWidth"></param>
		/// <param name="bitmapHeight"></param>
		/// <param name="dp"></param>
		/// <returns></returns>

		string GetMoleculeSvg(
			int bitmapWidth,
			int bitmapHeight,
			string units,
			DisplayPreferences dp = null);

		/// <summary>
		/// Get metafile of structure
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <returns></returns>
		Metafile GetMetafile(
 int width,
 int height);

		/// <summary>
		/// Set the caption for a molecule if molfile format
		/// </summary>
		/// <param name="caption">Text of caption</param>

		void CreateStructureCaption(
			string caption);

		/// <summary>
		/// Remove any existing structure captions
		/// </summary>
		/// <param name="m"></param>
		/// <returns></returns>

		bool RemoveStructureCaption();

		/// <summary>
		/// Adjust bond length to valid range
		/// </summary>
		/// <param name="bondLen"></param>
		/// <returns></returns>

		int AdjustBondLengthToValidRange(int bondLen);

		/// <summary>
		/// Transform molecule according to flag settings
		/// </summary>
		/// <param name="flags"></param>
		/// <param name="name">Name to go in first line</param>
		/// <returns></returns>

		void TransformMolecule(
			MoleculeTransformationFlags flags,
			string name);

		/// <summary>
		/// Integrate MMP difference and common fragments
		/// </summary>
		/// <param name="smilesFrags"></param>
		/// <returns>Molfile of integrated mol with difference fragment hilighted via special atom isotope values </returns>

		string IntegrateAndHilightMmpStructure(
			string smilesFrags);

		///////////// From Pipeline Pilot //////////////

		/// <summary>
		/// Check if substructure query matches target molecule
		/// </summary>
		/// <param name="queryMol"></param>
		/// <param name="targetMol"></param>
		/// <returns></returns>

		bool IsSSSMatch(
			ICdkMol queryMol,
			ICdkMol targetMol);

		/// <summary>
		/// Prepare for SSS matching of supplied query molecule
		/// </summary>
		/// <param name="queryMol"></param>

		void SetSSSQueryMolecule(
			ICdkMol queryMol);

		/// <summary>
		/// Map current query against supplied target molecule
		/// </summary>
		/// <param name="targetMol"></param>
		/// <returns></returns>

		bool IsSSSMatch(
			ICdkMol targetMol);

		/// <summary>
		/// Get mapping of current query against supplied target
		/// </summary>
		/// <param name="targetMol"></param>
		/// <param name="queryIndex"></param>
		/// <param name="mappedAtoms"></param>
		/// <param name="mappedBonds"></param>
		/// <returns></returns>

		bool GetSSSMapping(
			ICdkMol targetMol,
			out int queryIndex,
			out int[] mappedAtoms,
			out int[] mappedBonds);

		/// <summary>
		/// GetNextSGMapping
		/// </summary>
		/// <param name="queryIndex"></param>
		/// <param name="mappedAtoms"></param>
		/// <param name="mappedBonds"></param>
		/// <returns></returns>

		bool GetNextSSSMapping(
			out int queryIndex,
			out int[] mappedAtoms,
			out int[] mappedBonds);

		/// <summary>
		/// Map and hilight a substructure match
		/// </summary>
		/// <param name="molfile"></param>
		/// <returns></returns>

		string HilightSSSMatch(string molfile);

		/// <summary>
		/// Hilight a target molecule using mapped atoms and bonds
		/// </summary>
		/// <param name="targetMol"></param>
		/// <param name="mappedAtoms"></param>
		/// <param name="mappedBonds"></param>
		/// <returns></returns>

		ICdkMol HilightSSSMatchGMap(
			ICdkMol targetMol,
			int[] mappedAtoms,
			int[] mappedBonds);

		/// <summary>
		/// Map current query against structure & return oriented match
		/// </summary>
		/// <param name="target molfile"></param>
		/// <returns></returns>

		string OrientToMatchingSubstructure(
			string targetMolfile);

		/// <summary>
		/// Set substructure search option
		/// </summary>
		/// <param name="option"></param>

		void SetSSSOption(
			//SGMap.SearchOption option, 
			bool value);

		/// <summary>
		/// Perform a full structure search
		/// </summary>
		/// <param name="query"></param>
		/// <param name="target"></param>
		/// <param name="switches"></param>
		/// <returns></returns>

		bool FullStructureMatch(
			ICdkMol query,
			ICdkMol target,
			string FullStructureSearchType = null);

		/// <summary>
		/// Prepare for FSS matching of supplied query molecule
		/// </summary>
		/// <param name="queryMol"></param>

		void SetFSSQueryMolecule(
			ICdkMol queryMol,
			string FullStructureSearchType = null);

		/// <summary>
		/// Map current query against supplied target molecule
		/// </summary>
		/// <param name="targetMol"></param>
		/// <returns></returns>

		bool IsFSSMatch(
			ICdkMol targetMol);

	}

	/// <summary>
	/// Interface for molecule renderer and editor
	/// </summary>

	public interface INativeMolControl
	{

		void SetMoleculeAndRender(MoleculeFormat format, string value); 

		void RenderMolecule();

		void GetMolecule(out MoleculeFormat format, out string value);

		void SetTag(object tag);

		object GetTag();

		MolEditorReturnedHandler EditorReturnedHandler { get; set; }
	}

	public delegate void MolEditorReturnedHandler(object sender, MolEditorReturnedEventArgs e);

	public class MolEditorReturnedEventArgs : EventArgs
	{
		public bool Validated;
	}



	/// <summary>
	/// Structure search types
	/// Use powers of 2 to allow multiple types to be combined
	/// </summary>

	public enum StructureSearchType
	{
		Unknown = 0,
		Substructure = 1,
		MolSim = 2,
		FullStructure = 4,
		MatchedPairs = 8,
		SmallWorld = 16,
		Related = 32
	}

	/// <summary>
	/// StructureSearchType Utilities
	/// </summary>

	public class SST : StructureSearchTypeUtil { } // alias

	public class StructureSearchTypeUtil
	{
		public static string StructureSearchTypeToExternalName(StructureSearchType sst)
		{
			if (sst == StructureSearchType.Unknown) return "";

			string txt = sst.ToString(); // default name

			if (sst == StructureSearchType.Substructure)
				txt = "Substructure";

			else if (sst == StructureSearchType.MolSim)
				txt = "Similar";

			else if (sst == StructureSearchType.MatchedPairs)
				txt = "Matched Pair";

			return txt;
		}

		public static bool IsFull(StructureSearchType searchTypes) => (searchTypes & StructureSearchType.FullStructure) != 0;
		public static bool IsMmp(StructureSearchType searchTypes) => (searchTypes & StructureSearchType.MatchedPairs) != 0;
		public static bool IsSw(StructureSearchType searchTypes) => (searchTypes & StructureSearchType.SmallWorld) != 0;
		public static bool IsSim(StructureSearchType searchTypes) => (searchTypes & StructureSearchType.MolSim) != 0;
		public static bool IsSSS(StructureSearchType searchTypes) => (searchTypes & StructureSearchType.Substructure) != 0;

	}

	/// <summary>
	/// Full structure search types
	/// </summary>

	public class FullStructureSearchType
	{
		public const string Exact = "All"; // complete match
		public const string Fragment = "STE/BON"; // single fragment ignoring other fragments/counterions
		public const string Isomer = "BON"; // single fragment, no stereo, ignoring other fragments/counterions
		public const string Tautomer = "TAU"; // tautomers ignoring charge, stereo, fragments etc
		public const string Parent = "FRA/BON/MET/MAS/RAD/VAL/STE/POL/TYP/MIX";
	}

	/// <summary>
	/// Similarity search types
	/// </summary>

	public enum SimilaritySearchType
	{
		Unknown = 0,
		Normal = 1, // Normal
		Sub = 2, // Supersimilarity
		Super = 3, // Subsimilarity
		ECFP4 = 4 // Extended Connectectivity FingerPrint a max diameter of 4 for each circular neighborhood considered
	}

	public enum AtomNumberDisplayMode
	{
		None = 0,
		All = 1,
		AAMapped = 2
	}

	/// <summary>
	/// Types of fingerprints currently available (based on CDK types)
	/// </summary>

	public enum FingerprintType
	{
		UndefinedMinusOne = -1,
		Undefined = 0,
		Basic = 1,
		Circular = 2, // ECFPx / FCFPx, must specify subtype
		Extended = 3,
		EState = 4,
		MACCS = 5,
		PubChem = 6,
		ShortestPath = 7, // fails with atom type issue for many structures (e.g. 123456)
		Signature = 8, // can't convert array fingerprint to bitsetfingerprint
		Substructure = 9,
	}

	public class CircularFingerprintType
	{
		public const int ECFP0 = 1;
		public const int ECFP2 = 2;
		public const int ECFP4 = 3;
		public const int ECFP6 = 4;
		public const int FCFP0 = 5;
		public const int FCFP2 = 6;
		public const int FCFP4 = 7;
		public const int FCFP6 = 8;


		public const int DefaultCircularClass = ECFP4;
		public const int DefaultCircularLength = 1024;
	}

	public enum MoleculeTransformationFlags
	{
		None = 0,
		LargestFragmentOnly = 1, // output only the largest fragment
		NoSuperAtomInfo = 2, // remove superatom information
		StructuresOnly = 4, // output only structures (no other data)
		SkipNoStructs = 8, // remove no structs
		RemoveHydrogens = 16, // remove explicit non-stereo h atoms
		RemoveStructureCaption = 32, // remove any caption (usually stereo comment)
		RemoveStereochemistry = 64 // remove any stereocenter information
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

	/// <summary>
	/// Molecule DisplayPreferences
	/// </summary>

	public class DisplayPreferences
	{
		public double StandardBondLength = -1;
		public HydrogenDisplayMode HydrogenDisplayMode = HydrogenDisplayMode.HeteroOrTerminal;
		public Color BackColor = Color.Transparent;

		/// <summary>
		/// Set standard display preferences for a Renderer/Reditor control
		/// </summary>
		/// <returns></returns>

		//public static DisplayPreferences SetStandardDisplayPreferences(MoleculeControl molCtl)
		//{
		//	throw new NotImplementedException();
		//}

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
		/// Set standard display preferences for a renderer
		/// </summary>
		/// <param name="dp"></param>

		public static void SetStandardDisplayPreferences(
			DisplayPreferences dp)
		{
			return; // todo - throw new NotImplementedException();
		}
	}

	public class Hilighting
	{
		public string HighlightChildren = "";
		public Color HighlightColor = Color.Blue;
	}

}
