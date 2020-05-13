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

//using net.sf.jniinchi; // low level IUPAC interface, needed for access to some enumerations

// JNI-InChI is a library intented for use by developers of other projects. Sam Adams - 31 October 2010
// It does not enable users to generate InChIs from molecule file formats such as .mol, .cml, .mol2, or SMILES strings.
//  https://sourceforge.net/projects/jni-inchi/?source=directory
// net.sf.jniinchi.JniInchiWrapper w = null; // IUPAC Java InChI wrapper (called from Assembly CDK.DotNet.dll)
//using net.sf.jniinchi; 
// Classes: JniInchiStructure for structure input
// 

// Wrapper for Inchi to run the full command-line program in Java. Matthew Clark - 2014-10-11
// This allows using MOL or SMILES input and generating the keys. A typical execution time is 20 milliseconds per compound
//  https://sourceforge.net/projects/inchijni/?source=directory 

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Mobius.CdkMx
{

/// <summary>
/// UniChem Utility Methods 
/// </summary>

public class UniChemUtil
	{
		public static double InChIToAtomContainerTime = 0;
		public static double BuildFinterprintTime1 = 0; // biggest
		public static double PartitionIntoMoleculesTime = 0;
		public static double GetAtomContainerTime = 0;
		public static double InChIGeneratorTime = 0;
		public static double BuildFinterprintTime2 = 0; // 2nd biggest

		public static int AtomContainerToSmilesAndBackErrorCount = 0;

		static ICdkMol CdkMolUtil => StaticCdkMol.I; // static molecule shortcut for utility methods

		/// <summary>
		/// Build UniChem Data 
		/// Note that this routine takes about 14 secs the first time it's called, faster thereafter.
		/// </summary>
		/// <param name="mol"></param>
		/// <returns></returns>

		public static UniChemData BuildUniChemData(
			IAtomContainer mol)
		{
			IAtomContainer mol2;
			InChIGenerator ig = null;
			string molSmiles, fragSmiles = "", molFile = "";
			int acc, fi, ci;

			//if (Lex.StartsWith(molString, "InChI="))
			//{
			//	sourceInchi = molString;
			//	mol = CdkMol.InChIToAtomContainer(sourceInchi);
			//}

			//else // assume molfile otherwise
			//{
			//	molfile = molString;
			//	mol = CdkMol.MolfileToAtomContainer(molfile);
			//}

			if (mol.Atoms.Count <= 1) throw new Exception("Atom count <= 1");

			int pseudoCount = CdkMol.RemovePseudoAtoms(mol);
			mol = CdkMol.AtomContainerToSmilesAndBack(mol, out molSmiles);

			InChIGeneratorFactory igf = InChIGeneratorFactory.Instance;
			try
			{
				//string options = "/KET /15T"; // options to include keto-enol and 1,5-tautomerism (not recognized by CDK)
				ig = igf.GetInChIGenerator(mol); //, options);
			}

			catch (Exception ex) // may fail for some complex mols (e.g. CorpId 12345, a MDL V3000 mol with a CF3 Sgroup/alias)
			{
				throw new Exception(ex.Message, ex);
			}

			//{
			//	try // try to simplify the molfile so that InChI can handle it
			//	{
			//		//if (Lex.IsUndefined(molfile)) throw ex;
			//		string molfile2 = SimplifyMolfileForInChIGeneration(molile);
			//		mol = CdkMol.MolfileToAtomContainer(molfile2);
			//		mol = CdkMol.AtomContainerToSmilesAndBack(mol, out molSmiles);
			//		ig = igf.getInChIGenerator(mol);
			//	}
			//	catch (Exception ex2)
			//	{
			//		throw new Exception(ex2.Message, ex);
			//	}
			//}

			if (!IsAcceptableInchiStatus(ig))
			{
				string errMsg = "InChI generation " + ig.ReturnStatus + ": " + ig.Message;
				molFile = CdkMol.AtomContainerToMolfile(mol); // debug
				throw new Exception(errMsg);
			}

			// Populate the UniChem object

			UniChemData icd = new UniChemData();
			//icd.Molfile = molfile;
			icd.AtomContainer = mol;
			icd.InChIString = ig.InChI;
			icd.InChIKey = ig.GetInChIKey();
			icd.CanonSmiles = molSmiles;

			// Build and store fingerprint

			mol = CdkMol.InChIToAtomContainer(icd.InChIString); 

			//int hydRemovedCnt = CdkMol.RemoveHydrogensBondedToPositiveNitrogens(mol);
			mol = CdkMol.RemoveIsotopesStereoExplicitHydrogens(mol); // additional normalization for fingerprint

			BitSetFingerprint fp = // generate a fingerprint
				CdkFingerprint.BuildBitSetFingerprint(mol, FingerprintType.MACCS, -1, -1);

			icd.Fingerprint = fp;

			if (ConnectivityChecker.IsConnected(mol)) return icd; // single fragment

			//string mf = CdkMol.GetMolecularFormula(mol);

			//AtomContainerSet acs = (AtomContainerSet)ConnectivityChecker.partitionIntoMolecules(mol);
			List<IAtomContainer> frags = CdkMol.FragmentMolecule(mol, true); // get fragments filtering out small and common frags

			for (fi = 0; fi < frags.Count; fi++)
			{
				mol2 = frags[fi];

				int atomCnt = mol2.Atoms.Count;
				if (atomCnt <= 1) continue;

				try
				{
					mol2 = CdkMol.AtomContainerToSmilesAndBack(mol2, out fragSmiles);
				}
				catch (Exception ex)
				{
					AtomContainerToSmilesAndBackErrorCount++; // just count error and ignore
				}

				ig = igf.GetInChIGenerator(mol2);
				if (!IsAcceptableInchiStatus(ig)) continue;

				string childInChIString = ig.InChI;
				string childInChIKey = ig.GetInChIKey();
				string childFIKHB = UniChemUtil.GetFIKHB(childInChIKey);

				mol2 = CdkMol.InChIToAtomContainer(childInChIString); // convert from inchi
				mol2 = CdkMol.RemoveIsotopesStereoExplicitHydrogens(mol2); // additional normalization for fingerprint

				fp = // generate a fingerprint for the fragment
					CdkFingerprint.BuildBitSetFingerprint(mol2, FingerprintType.MACCS, -1, -1);

				for (ci = 0; ci < icd.Children.Count; ci++) // see if a dup child
				{
					if (icd.Children[ci].ChildFIKHB == childFIKHB) break;
				}
				if (ci < icd.Children.Count) continue; // skip if dup

				UniChemFIKHBHierarchy icdChild = new UniChemFIKHBHierarchy();
				icdChild.ParentFIKHB = icd.GetFIKHB();
				icdChild.ChildFIKHB = childFIKHB;
				icdChild.InChIString = childInChIString;
				icdChild.CanonSmiles = fragSmiles;

				icdChild.Fingerprint = fp;

				icd.Children.Add(icdChild);
			}

			return icd;
		}

		/// <summary>
		/// BuildFingerprints
		/// </summary>
		public static void BuildFingerprints(
			UniChemData icd)
		{
			IAtomContainer mol, mol2;
			InChIGenerator ig = null;
			int acc, aci, ci;

			icd.Children.Clear();

			string parentFIKHB = icd.GetFIKHB();

			DateTime t0 = DateTime.Now;

			mol = CdkMol.InChIToAtomContainer(icd.InChIString);

			icd.CanonSmiles = CdkMol.AtomContainerToSmiles(mol);

			InChIToAtomContainerTime += TimeOfDay.Delta(ref t0);

			BitSetFingerprint fp = // generate a fingerprint
				CdkFingerprint.BuildBitSetFingerprint(mol, FingerprintType.MACCS, -1, -1);

			BuildFinterprintTime1 += TimeOfDay.Delta(ref t0);

			icd.Fingerprint = fp;

			if (ConnectivityChecker.IsConnected(mol)) return; // single fragment

			InChIGeneratorFactory igf = InChIGeneratorFactory.Instance;

			AtomContainerSet acs = (AtomContainerSet)ConnectivityChecker.PartitionIntoMolecules(mol);
			PartitionIntoMoleculesTime += TimeOfDay.Delta(ref t0);

			acc = acs.Count;

			for (aci = 0; aci < acc; aci++)
			{
				mol2 = acs[aci];
				GetAtomContainerTime += TimeOfDay.Delta(ref t0);

				ig = igf.GetInChIGenerator(mol2);
				if (!IsAcceptableInchiStatus(ig)) continue;

				string childKey = ig.GetInChIKey();
				string childFIKHB = UniChemUtil.GetFIKHB(childKey);
				InChIGeneratorTime += TimeOfDay.Delta(ref t0);

				fp = // generate a fingerprint for the fragment
					CdkFingerprint.BuildBitSetFingerprint(mol2, FingerprintType.MACCS, -1, -1);

				BuildFinterprintTime2 += TimeOfDay.Delta(ref t0);

				for (ci = 0; ci < icd.Children.Count; ci++) // see if a dup child
				{
					if (icd.Children[ci].ChildFIKHB == childFIKHB) break;
				}
				if (ci < icd.Children.Count) continue; // skip if dup

				UniChemFIKHBHierarchy fikhbHier = new UniChemFIKHBHierarchy();
				fikhbHier.ParentFIKHB = parentFIKHB;
				fikhbHier.ChildFIKHB = childFIKHB;
				fikhbHier.CanonSmiles = CdkMol.AtomContainerToSmiles(mol2);
				fikhbHier.Fingerprint = fp;

				icd.Children.Add(fikhbHier);
			}

			return;
		}


		/// <summary>
		/// SimplifyMolfileForInChIGeneration
		/// </summary>
		/// <param name="molfile"></param>
		/// <returns></returns>

		//static string SimplifyMolfileForInChIGeneration(string molfile)
		//{
		//	string smiles = CdkMol.StructureConverter.MolfileStringToSmilesString(molfile); // convert to smiles
		//	string molfile2 = CdkMol.StructureConverter.SmilesStringToMolfileString(smiles); // and back to V2000 molfile without Sgroups
		//	return molfile2;
		//}

		/// <summary>
		/// IsAcceptableInchiStatus
		/// </summary>
		/// <param name="ig"></param>
		/// <returns></returns>

		static bool IsAcceptableInchiStatus(InChIGenerator ig)
		{
			InChIReturnCode inchiStatus = ig.ReturnStatus;
			if (inchiStatus == InChIReturnCode.Ok) return true;

			string msg = ig.Message;
			string log = ig.Log;
			string auxInfo = ig.AuxInfo;
			if (inchiStatus == InChIReturnCode.Warning)
				return true;

			else return false;
		}

		public static string GetFIKHB(string inchiKey)
		{
			return Lex.IsDefined(inchiKey) ? inchiKey.Substring(0, 14) : "";
		}

		/// <summary>
		/// Get count of structure components (fragments) from the formula part of the inchi
		/// </summary>
		/// <param name="inchi"></param>
		/// <returns></returns>

		public static int GetComponentCount(string inchi)
		{
			string mf = GetMf(inchi);
			if (Lex.IsUndefined(mf)) return 0;
			int componentCount = Lex.CountCharacter(mf, '.') + 1;
			return componentCount;
		}

		/// <summary>
		/// Get formula part of the inchi
		/// </summary>
		/// <param name="inchi"></param>
		/// <returns></returns>

		public static string GetMf(string inchi)
		{
			int i1 = inchi.IndexOf("/");
			string mf = inchi.Substring(i1 + 1);
			i1 = mf.IndexOf("/");
			if (i1 >= 1) mf = mf.Substring(0, i1);
			return mf;
		}

		/// <summary>
		/// Convert an Inchi string to a molfile
		/// </summary>
		/// <param name="inchiString"></param>
		/// <returns></returns>

		string InChIStringToMolfileString(string inchiString)
		{
			throw new NotImplementedException();
		}

	}

	/// <summary>
	/// InChIData
	/// </summary>

	public class UniChemData
	{
		public UniChemId Id = new UniChemId(); // Id object referred to by fields below

		public Int64 UCI { get { return Id.UCI; } set { Id.UCI = value; } } // UniChem Compound Id
		public int SrcId { get { return Id.SrcId; } set { Id.SrcId = value; } } // database source
		public string SrcCid { get { return Id.SrcCid; } set { Id.SrcCid = value; } } // source compound id
		public int SrcCidInt { get { return Id.SrcCidInt; } set { Id.SrcCidInt = value; } }
		public int Assignment { get { return Id.Assignment; } set { Id.Assignment = value; } } // 1 if assigned in Unichem, 0 if not but still in DB
		public DateTime UpdateDate { get { return Id.UpdateDate; } set { Id.UpdateDate = value; } }

		public string Chime;
		public string Molfile;
		public IAtomContainer AtomContainer;
		public string Smiles;
		public string CanonSmiles;
		public string InChIString;
		public string InChIKey;
		public BitSetFingerprint Fingerprint; // CDK fingerprint
		public DateTime CreateDate;
		public string GetFIKHB() { return UniChemUtil.GetFIKHB(InChIKey); } // First InChI Key Hash Block (Connectivity, no stereo, Isomers)
		public int GetComponentCount() { return UniChemUtil.GetComponentCount(InChIString); }  // Number of components in the molecule
		public List<UniChemFIKHBHierarchy> Children = new List<UniChemFIKHBHierarchy>(); // list of FIKHBs for children for multi-component structures
	}

	/// <summary>
	/// UniChemId
	/// </summary>
	public class UniChemId
	{
		public Int64 UCI = -1; // UniChem Compound Id
		public int SrcId = -1; // database source
		public string SrcCid = ""; // source compound id
		public int SrcCidInt = -1;
		public int Assignment = -1; // 1 if assigned in Unichem, 0 if not but still in DB
		public DateTime UpdateDate = DateTime.MinValue;
	}

	/// <summary>
	/// UniChemHierarchy - Entries for multi-component structures
	/// </summary>

	public class UniChemFIKHBHierarchy
	{
		public string ParentFIKHB = ""; // Parent FIKHB that includes all components
		public string ChildFIKHB = ""; // FIKHB of this child component
		public string InChIString = ""; // full InChI of child
		public string CanonSmiles; // Canonical Smiles of child
		public BitSetFingerprint Fingerprint; // fingerprint for child
		public int Count1 = -1; // count entries from UC_FIKHB_HIERARCHY.txt
		public int Count2 = -1;
	}

}
