using Mobius.ComOps;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Mobius.MolLib1
{

	/// <summary>
	/// StructureConverter
	/// </summary>

	public class StructureConverter
	{

		public static ICdkUtil ICdkUtil; // instance for accessing CDK utility methods

		public byte[] SketchData = null;

		/// <summary>
		/// Constructor
		/// </summary>

		public StructureConverter()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// MolfileString
		/// </summary>

		public string MolfileString
		{
			get { throw new NotImplementedException(); }
			set 
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Smiles
		/// </summary>

		public string Smiles
		{
			get
			{
				throw new NotImplementedException();
			}

			set
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Check if a string is a valid Chime string
		/// </summary>
		/// <param name="chime"></param>
		/// <returns></returns>

		public static bool IsValidChimeString(string chime)
		{
			if (Lex.IsUndefined(chime)) return false;

			try
			{
				string molfile = ChimeStringToMolfileString(chime);
				return IsValidMolfile(molfile);
			}
			catch (Exception ex)
			{
				return false;
			}

		}

		/// <summary>
		/// Check if a string is a valid Molfile
		/// </summary>
		/// <param name="molfile"></param>
		/// <returns></returns>

		public static bool IsValidMolfile(string molfile)
		{
			if (Lex.IsUndefined(molfile)) return false;

			try
			{
				NativeMolecule m = MolfileStringToNativeMolecule(molfile);
				if (m != null) return true;
				else return false;
			}
			catch (Exception ex)
			{
				return false;
			}

		}

		/// <summary>
		/// Direct converion of molfile to Molecule object
		/// </summary>
		/// <param name="molfile"></param>
		/// <returns></returns>

		public static NativeMolecule MolfileStringToNativeMolecule(string molfile)
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
		/// Direct converion of Molecule object to molfile
		/// </summary>
		/// <param name="m"></param>
		/// <returns></returns>

		public static string NativeMoleculeToMolfileString(NativeMolecule m)
		{
			StructureConverter sc = new StructureConverter();
			throw new NotImplementedException();
		}

		/// <summary>
		/// Convert a molfile into a chime string
		/// </summary>
		/// <param name="molfile"></param>
		/// <returns></returns>

		public static string MolfileStringToChimeString(string molfile)
		{
			if (Lex.IsUndefined(molfile)) return "";

			string chimeString = StructureConverter.MolfileStringToChimeString(molfile);
			if (chimeString == null) chimeString = "";
			return chimeString;
		}

		/// <summary>
		/// MolfileStringToSmilesString
		/// </summary>
		/// <param name="molfileString"></param>
		/// <returns></returns>

		public static string MolfileStringToSmilesString(string molfile)
		{
			string smiles = "", msg;

			if (Lex.IsUndefined(molfile)) return "";

			try // try converting with native converter
			{
				smiles = (string)StructureConverter.MolfileStringToSmilesString(molfile);
				if (Lex.IsDefined(smiles))
				{
					//DebugLog.Message("Succeeded Convert1, Smiles: " + smiles);
					return smiles;
				}
			}
			catch (Exception ex)
			{
				msg = "Failed Convert1: " + ex.Message;
				//DebugLog.Message(msg);
			}

			try
			{
				if (ICdkUtil == null) throw new InvalidDataException("ICdkUtil is null");

				smiles = ICdkUtil.MolfileStringToSmilesString(molfile);

				if (Lex.IsDefined(smiles))
				{
					//DebugLog.Message("Succeeded Convert3, Smiles: " + smiles);
					return smiles;
				}

				else throw new Exception("Failed CdkUtil.MolfileStringToSmilesString");
			}

			catch (Exception ex)
			{
				msg = "Failed Convert3: " + ex.Message;
				//DebugLog.Message(msg);
				return "";
			}

		}

		/// <summary>
		/// ChimeStringToMolfileString
		/// </summary>
		/// <param name="chimeString"></param>
		/// <returns></returns>

		public static string ChimeStringToMolfileString(string chimeString)
		{
			if (Lex.IsUndefined(chimeString)) return "";

			string molfile = StructureConverter.ChimeStringToMolfileString(chimeString);
			if (molfile == null) molfile = "";
			return molfile;
		}

		/// <summary>
		/// ChimeStringToSmilesString
		/// </summary>
		/// <param name="chimeString"></param>
		/// <returns></returns>

		public static string ChimeStringToSmilesString(string chimeString)
		{
			string molfile = ChimeStringToMolfileString(chimeString); // first convert chime to molfile
			string smiles = MolfileStringToSmilesString(molfile); // then molfile to smiles
			return smiles;
		}

		/// <summary>
		/// SmilesStringToChimeString
		/// </summary>
		/// <param name="smilesString"></param>
		/// <returns></returns>

		public static string SmilesStringToChimeString(string smilesString)
		{
			string molfile = SmilesStringToMolfileString(smilesString); // first convert smiles to molfile
			string chimeString = MolfileStringToChimeString(molfile); // then molfile to chime
			return chimeString;
		}

		/// <summary>
		/// SmilesStringToMolfileString
		/// </summary>
		/// <param name="smilesString"></param>
		/// <returns></returns>

		public static string SmilesStringToMolfileString(string smilesString)
		{
			string molFile = "";

			if (Lex.IsUndefined(smilesString)) return "";

			try
			{
				molFile = StructureConverter.SmilesStringToMolfileString(smilesString);
				if (Lex.IsDefined(molFile)) return molFile;
			}

			catch (Exception ex) { ex = ex; }

			try
			{
				if (ICdkUtil == null) throw new InvalidDataException("ICdkUtil is null");

				molFile = ICdkUtil.SmilesStringToMolfileString(smilesString);

				if (Lex.IsDefined(molFile))
				{
					return molFile;
				}

				else throw new InvalidDataException("Failed CdkUtil.SmilesStringToMolfileString");
			}

			catch (Exception ex)
			{
				return "";
			}
		}

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
/// Placeholder
/// </summary>

	public class NativeStructureConverter
	{
	}

}
