using System;
using System.Collections.Generic;
using System.Text;

namespace Mobius.ComOps
{

// General use simple delegates

	public delegate void SimpleDelegate(); // simple delegate with no parameters and no return value

	public delegate void ObjectParmDelegate(object arg); // simple delegate with a single object parameter

	public delegate object ObjectObjectDelegate(object arg); // simple delegate with a single object parameter and object return value


	/// <summary>
	/// Allow calls into SessionManager
	/// </summary>

	public interface ISessionManager
	{
		void DisplayStartupMessage(string msg);
	}

	/// <summary>
	/// Allow calls to ServerFile methods from both the client & services
	/// </summary>

	public interface IServerFile
	{
		string ReadAll(string fileName);
	}

	/// <summary>
	/// Interface to MoleculeLibrary (e.g. Cdk) functionality for MoleculeMx Class
	/// </summary>

	public interface IMolLibMx
	{
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
			string MolfileString,
			int fpType,
			int fpSubtype,
			int fpLen);


		/// <summary>
		/// Get the largest molecule fragment from supplied molfile
		/// </summary>
		/// <param name="molfile"></param>
		/// <returns>Molfile of largest fragment</returns>

		string GetLargestMoleculeFragmentAsMolfile(
			string molfileString);

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

		int[] GetBitSet(object fingerprint);

	}


}
