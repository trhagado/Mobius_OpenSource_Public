using Mobius.ComOps;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mobius.MolLib2
{

	/// <summary>
	/// MolLib2 Utilities
	/// </summary>

	public class Util 
	{

		/// <summary>
		/// Get version
		/// </summary>
		/// <returns></returns>

		public static string GetVersion()
		{
			int[] va = null;

			try
			{
				//va = todo...
				string v = "";
				foreach (int i in va)
				{
					if (v != "") v += ".";
					v += i;
				}
				return v;
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message, ex);
			}
		}

		/// <summary>
		/// Check if substructure query matches target molecule
		/// </summary>
		/// <param name="queryMol"></param>
		/// <param name="targetMol"></param>
		/// <returns></returns>

		public static bool IsSSSMatch(
			Molecule queryMol,
			Molecule targetMol)
		{
			SetSSSQueryMolecule(queryMol);
			return IsSSSMatch(targetMol);
		}

		/// <summary>
		/// Prepare for SSS matching of supplied query molecule
		/// </summary>
		/// <param name="queryMol"></param>

		public static void SetSSSQueryMolecule(
		Molecule queryMol)
		{
			try
			{
				throw new NotImplementedException();
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message, ex);
			}
		}

		/// <summary>
		/// Map current query against supplied target molecule
		/// </summary>
		/// <param name="targetMol"></param>
		/// <returns></returns>

		public static bool IsSSSMatch(
			Molecule targetMol)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Get mapping of current query against supplied target
		/// </summary>
		/// <param name="targetMol"></param>
		/// <param name="queryIndex"></param>
		/// <param name="mappedAtoms"></param>
		/// <param name="mappedBonds"></param>
		/// <returns></returns>

		public static bool GetSSSMapping(
			Molecule targetMol,
			out int queryIndex,
			out int[] mappedAtoms,
			out int[] mappedBonds)
		{
			queryIndex = -1;
			mappedAtoms = mappedBonds = null;

			try
			{
				throw new NotImplementedException();
			}
			catch (Exception ex)
			{
				return false; // just say false if exception encountered
			}
		}

		/// <summary>
		/// GetNextSGMapping
		/// </summary>
		/// <param name="queryIndex"></param>
		/// <param name="mappedAtoms"></param>
		/// <param name="mappedBonds"></param>
		/// <returns></returns>

		public static bool GetNextSSSMapping(
			out int queryIndex, 
			out int[] mappedAtoms, 
			out int[] mappedBonds)
		{
			queryIndex = -1;
			mappedAtoms = mappedBonds = null;

			try
			{
				throw new NotImplementedException();
			}
			catch (Exception ex)
			{
				return false; // just say false if exception encountered
			}
		}

		/// <summary>
		/// Map and hilight a substructure match
		/// </summary>
		/// <param name="molfile"></param>
		/// <returns></returns>

		public static string HilightSSSMatch(string molfile)
		{
			// This seems to be slow for some reason when running in the VisualStudio debugger

			int queryIndex;
			int[] mappedAtoms, mappedBonds;

			try
			{

				Molecule m = MolfileStringToMolecule(molfile);
				if (GetSSSMapping(m, out queryIndex, out mappedAtoms, out mappedBonds))
				{
					throw new NotImplementedException();
				}

				else return molfile;
			}

			catch (Exception ex)
			{
				return molfile; // just return the input if exception encountered
			}
		}

		/// <summary>
		/// Hilight a target molecule using mapped atoms and bonds
		/// </summary>
		/// <param name="targetMol"></param>
		/// <param name="mappedAtoms"></param>
		/// <param name="mappedBonds"></param>
		/// <returns></returns>

		public static Molecule HilightSSSMatchGMap(
			Molecule targetMol,
			int[] mappedAtoms,
			int[] mappedBonds)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Map current query against structure & return oriented match
		/// </summary>
		/// <param name="target molfile"></param>
		/// <returns></returns>

		public static string OrientToMatchingSubstructure(
			string targetMolfile)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Set substructure search option
		/// </summary>
		/// <param name="option"></param>

		public static void SetSSSOption(
			//SGMap.SearchOption option, 
			bool value)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Perform a full structure search
		/// </summary>
		/// <param name="query"></param>
		/// <param name="target"></param>
		/// <param name="switches"></param>
		/// <returns></returns>

		public static bool FullStructureMatch(
			Molecule query,
			Molecule target,
			string FullStructureSearchType = null)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Prepare for FSS matching of supplied query molecule
		/// </summary>
		/// <param name="queryMol"></param>

		public static void SetFSSQueryMolecule(
			Molecule queryMol,
			string FullStructureSearchType = null)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Map current query against supplied target molecule
		/// </summary>
		/// <param name="targetMol"></param>
		/// <returns></returns>

		public static bool IsFSSMatch(
			Molecule targetMol)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// MoleculetoMolfileString
		/// </summary>
		/// <param name="mol"></param>
		/// <returns></returns>

		public static string MoleculeTofMolfileString(Molecule mol)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// MoleculetoChimeString
		/// </summary>
		/// <param name="mol"></param>
		/// <returns></returns>

		public static string MoleculeToChimeString(Molecule mol)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// MoleculetoChimeString
		/// </summary>
		/// <param name="mol"></param>
		/// <returns></returns>

		public static string MoleculeToSmilesString(Molecule mol)
		{
			throw new NotImplementedException();
		}


		/// <summary>
		/// CreateMoleculeFromMolfile
		/// </summary>
		/// <param name="chimeString"></param>
		/// <returns></returns>

		public static Molecule MolfileStringToMolecule(
			string molfile)
		{
			Molecule mol = null;
			try
			{
				throw new NotImplementedException();
			}

			catch (Exception ex)
			{
				throw new Exception(ex.Message, ex);
			}
		}

		/// <summary>
		///  ChimeStringToMolecule
		/// </summary>
		/// <param name="chimeString"></param>
		/// <returns></returns>

		public static Molecule ChimeStringToMolecule(
			string chimeString)
		{
			Molecule mol = null;
			try
			{
				throw new NotImplementedException();
			}

			catch (Exception ex)
			{
				throw new Exception(ex.Message, ex);
			}
		}

		/// <summary>
		/// SmilesStringToMolecule
		/// </summary>
		/// <param name="smilesString"></param>
		/// <returns></returns>

		public static Molecule SmilesStringToMolecule(
			string smilesString)
		{
			Molecule mol = null;
			try
			{
				throw new NotImplementedException();
			}

			catch (Exception ex)
			{
				throw new Exception(ex.Message, ex);
			}
		}

		/// <summary>
		/// GetMolFormulaDotDisconnect
		/// </summary>
		/// <param name="molfile"></param>
		/// <returns></returns>

		public static string GetMolFormulaDotDisconnect(
			string molfile)
		{
			Molecule mol = MolfileStringToMolecule(molfile);
			string mf = GetMolFormulaDotDisconnect(mol);
			return mf;
		}

		/// <summary>
		/// GetMolFormulaDotDisconnect
		/// </summary>
		/// <param name="mol"></param>
		/// <returns></returns>

		public static String GetMolFormulaDotDisconnect(
			Molecule mol)
		{
			string mf = GetMolFormula(mol, includeSpaces: false, separateFragments: true);
			return mf;
		}

		/// <summary>
		/// GetFolFormula
		/// </summary>
		/// <param name="mol"></param>
		/// <param name="includeSpaces"></param>
		/// <param name="separateFragments"></param>
		/// <param name="use2Hand3HforHydrogenIsotopes"></param>
		/// <param name="ignoreIsotopes"></param>
		/// <returns></returns>

		public static String GetMolFormula(
			Molecule mol,
			bool includeSpaces = false,
			bool separateFragments = false,
			bool use2Hand3HforHydrogenIsotopes = false,
			bool ignoreIsotopes = false)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Get mol weight for a molfile
		/// </summary>
		/// <param name="molfile"></param>
		/// <returns></returns>

		public static double GetMolWeight(
			string molfile)
		{
			Molecule mol = MolfileStringToMolecule(molfile);
			double mw = GetMolWeight(mol);
			return mw;
		}

		/// <summary>
		/// GetMolWeight
		/// </summary>
		/// <param name="mol"></param>
		/// <returns></returns>

		public static double GetMolWeight(
			Molecule mol)
		{
			throw new NotImplementedException();
		}


		/// <summary>
		/// Get list of the Rgroups and the instance count and first buffer position for each one
		/// </summary>
		/// <param name="mol"></param>
		/// <param name="rgCounts"></param>
		/// <param name="rgBufferStartingPos"></param>

		public static void GetCoreRGroupInfo(
			Molecule mol,
			out SortedDictionary<int, int> rgCounts,
			out SortedDictionary<int, int> rgBufferStartingPos)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Scan the atoms in a fragment to identify the rgroup that the fragment has been mapped to
		/// </summary>
		/// <param name="fragment"></param>
		/// <returns></returns>

		public static int GetFragmentRGroupAssignment(
			Molecule fragment)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Remove RGroup attachment point atoms
		/// </summary>
		/// <param name="molfile"></param>
		/// <returns></returns>

		public static string RemoveRGroupAttachmentPointAtoms(string molfile)
		{
			throw new NotImplementedException();

		}


	}
}
