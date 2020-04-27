using Mobius.Data;
using Mobius.ComOps;

using java.io;

using cdk = org.openscience.cdk;
using org.openscience.cdk;
using org.openscience.cdk.inchi; 
using org.openscience.cdk.interfaces;
using org.openscience.cdk.tools.manipulator;
using org.openscience.cdk.graph;
using org.openscience.cdk.qsar.result;
using org.openscience.cdk.io;
using org.openscience.cdk.fingerprint;
using org.openscience.cdk.io.iterator;
using org.openscience.cdk.tools;

using net.sf.jniinchi; // low level IUPAC interface, needed for access to some enumerations

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
			//	mol = CdkUtil.InChIToAtomContainer(sourceInchi);
			//}

			//else // assume molfile otherwise
			//{
			//	molfile = molString;
			//	mol = CdkUtil.MolfileToAtomContainer(molfile);
			//}

			if (mol.getAtomCount() <= 1) throw new Exception("Atom count <= 1");

			int pseudoCount = CdkUtil.RemovePseudoAtoms(mol);
			mol = CdkUtil.AtomContainerToSmilesAndBack(mol, out molSmiles);

			InChIGeneratorFactory igf = InChIGeneratorFactory.getInstance();
			try
			{
				//string options = "/KET /15T"; // options to include keto-enol and 1,5-tautomerism (not recognized by CDK)
				ig = igf.getInChIGenerator(mol); //, options);
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
			//		mol = CdkUtil.MolfileToAtomContainer(molfile2);
			//		mol = CdkUtil.AtomContainerToSmilesAndBack(mol, out molSmiles);
			//		ig = igf.getInChIGenerator(mol);
			//	}
			//	catch (Exception ex2)
			//	{
			//		throw new Exception(ex2.Message, ex);
			//	}
			//}

			if (!IsAcceptableInchiStatus(ig))
			{
				string errMsg = "InChI generation " + ig.getReturnStatus() + ": " + ig.getMessage();
				molFile = CdkUtil.AtomContainerToMolFile(mol); // debug
				throw new Exception(errMsg);
			}

			// Populate the UniChem object

			UniChemData icd = new UniChemData();
			//icd.Molfile = molfile;
			icd.AtomContainer = mol;
			icd.InChIString = ig.getInchi();
			icd.InChIKey = ig.getInchiKey();
			icd.CanonSmiles = molSmiles;

			// Build and store fingerprint

			mol = CdkUtil.InChIToAtomContainer(icd.InChIString); 

			//int hydRemovedCnt = CdkUtil.RemoveHydrogensBondedToPositiveNitrogens(mol);
			mol = CdkUtil.RemoveIsotopesStereoExplicitHydrogens(mol); // additional normalization for fingerprint

			BitSetFingerprint fp = // generate a fingerprint
				CdkFingerprint.BuildBitSetFingerprint(mol, FingerprintType.MACCS, -1, -1);

			icd.Fingerprint = fp;

			if (ConnectivityChecker.isConnected(mol)) return icd; // single fragment

			//string mf = CdkUtil.GetMolecularFormula(mol);

			//AtomContainerSet acs = (AtomContainerSet)ConnectivityChecker.partitionIntoMolecules(mol);
			List<IAtomContainer> frags = CdkUtil.FragmentMolecule(mol, true); // get fragments filtering out small and common frags

			for (fi = 0; fi < frags.Count; fi++)
			{
				mol2 = frags[fi];

				int atomCnt = mol2.getAtomCount();
				if (atomCnt <= 1) continue;

				try
				{
					mol2 = CdkUtil.AtomContainerToSmilesAndBack(mol2, out fragSmiles);
				}
				catch (Exception ex)
				{
					AtomContainerToSmilesAndBackErrorCount++; // just count error and ignore
				}

				ig = igf.getInChIGenerator(mol2);
				if (!IsAcceptableInchiStatus(ig)) continue;

				string childInChIString = ig.getInchi();
				string childInChIKey = ig.getInchiKey();
				string childFIKHB = UniChemUtil.GetFIKHB(childInChIKey);

				mol2 = CdkUtil.InChIToAtomContainer(childInChIString); // convert from inchi
				mol2 = CdkUtil.RemoveIsotopesStereoExplicitHydrogens(mol2); // additional normalization for fingerprint

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

			mol = CdkUtil.InChIToAtomContainer(icd.InChIString);

			icd.CanonSmiles = CdkUtil.AtomContainerToSmiles(mol);

			InChIToAtomContainerTime += TimeOfDay.Delta(ref t0);

			BitSetFingerprint fp = // generate a fingerprint
				CdkFingerprint.BuildBitSetFingerprint(mol, FingerprintType.MACCS, -1, -1);

			BuildFinterprintTime1 += TimeOfDay.Delta(ref t0);

			icd.Fingerprint = fp;

			if (ConnectivityChecker.isConnected(mol)) return; // single fragment

			InChIGeneratorFactory igf = InChIGeneratorFactory.getInstance();

			AtomContainerSet acs = (AtomContainerSet)ConnectivityChecker.partitionIntoMolecules(mol);
			PartitionIntoMoleculesTime += TimeOfDay.Delta(ref t0);

			acc = acs.getAtomContainerCount();

			for (aci = 0; aci < acc; aci++)
			{
				mol2 = acs.getAtomContainer(aci);
				GetAtomContainerTime += TimeOfDay.Delta(ref t0);

				ig = igf.getInChIGenerator(mol2);
				if (!IsAcceptableInchiStatus(ig)) continue;

				string childKey = ig.getInchiKey();
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
				fikhbHier.CanonSmiles = CdkUtil.AtomContainerToSmiles(mol2);
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

		static string SimplifyMolfileForInChIGeneration(string molfile)
		{
			string smiles = MolLib1.StructureConverter.MolfileStringToSmilesString(molfile); // convert to smiles
			string molfile2 = MolLib1.StructureConverter.SmilesStringToMolfileString(smiles); // and back to V2000 molfile without Sgroups
			return molfile2;
		}

		/// <summary>
		/// IsAcceptableInchiStatus
		/// </summary>
		/// <param name="ig"></param>
		/// <returns></returns>

		static bool IsAcceptableInchiStatus(InChIGenerator ig)
		{
			INCHI_RET inchiStatus = ig.getReturnStatus();
			if (inchiStatus == INCHI_RET.OKAY) return true;

			string msg = ig.getMessage();
			string log = ig.getLog();
			string auxInfo = ig.getAuxInfo();
			if (inchiStatus == INCHI_RET.WARNING)
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
