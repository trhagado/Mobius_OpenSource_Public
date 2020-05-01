using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;

namespace Mobius.ComOps
{
  public class MoleculeBase
  {
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
