using Mobius.ComOps;

using java.io;

using cdk = org.openscience.cdk;
using org.openscience.cdk;
using org.openscience.cdk.inchi;
using org.openscience.cdk.interfaces;
using org.openscience.cdk.fingerprint;
using org.openscience.cdk.smiles;
using org.openscience.cdk.tools.manipulator;
using org.openscience.cdk.aromaticity;
using org.openscience.cdk.graph;
using org.openscience.cdk.qsar.result;
using org.openscience.cdk.io;
using org.openscience.cdk.io.iterator;
using org.openscience.cdk.tools;

using net.sf.jniinchi; // low level IUPAC interface, needed for access to some enumerations

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Mobius.CdkMx
{

	/// <summary>
	/// CdkUtil
	/// 
	/// /// Note: 
	///  This code uses the CDK and InChI Java .jar libraries converted for C#.Net use via IKVM.
	///  Start a Cmd window and CD to the directory containing the .jar files.
	///  
	/* CdkIkvmBuildForMobius.txt
	  
	; Combine CDK/Ambit jars into a single DLL. Note that the final dll must be named CDK.dll to link properly with 
	; the INCHI JNI library (jni-inchi-0.7-jar-with-dependencies.dll)

	cd \CDK\IkvmBuildForMobius

	C:\ikvm-7.4.5046\bin\ikvmc -out:CDK.dll cdk-bundle-1.5.12.jar ambit2-base-2.6.0-SNAPSHOT.jar ambit2-core-2.6.0-SNAPSHOT.jar ambit2-smarts-2.6.0-SNAPSHOT.jar ambit2-tautomers-2.6.0-SNAPSHOT.jar jchempaint-hotfix-3.4.jar jmol-10.jar 

	*/

	///  The current production version is 7.2.4650 (2/26/16).
	///  However, the following bug-fix version (7.2.4650) is used because it takes care of a null reference exception:
	///    http://www.frijters.net/ikvmbin-7.4.5046.zip
	/// </summary>

	public partial class CdkMol : IMolLib

	{
		public static IChemObjectBuilder DefaultChemObjectBuilder
		{
			get
			{
				if (_defaultChemObjectBuilder == null)
					_defaultChemObjectBuilder = org.openscience.cdk.DefaultChemObjectBuilder.getInstance();

				return _defaultChemObjectBuilder;
			}
		}
		static IChemObjectBuilder _defaultChemObjectBuilder = null;

		public static int ParseSmilesErrorCount = 0;
		public static string LastParseSmilesError = "";

		/// <summary>
		/// Get the version of Java that was used to build the CDK .jar file
		/// </summary>
		/// <returns></returns>

		public static string GetJavaVersion()
		{
			string version = java.lang.Package.getPackage("java.lang").getImplementationVersion();
			return version;
		}

		/// <summary>
		/// GetHydrogenAdder
		/// </summary>
		/// <returns></returns>

		static CDKHydrogenAdder GetHydrogenAdder()
		{
			if (_hydrogenAdder == null)
				_hydrogenAdder = CDKHydrogenAdder.getInstance(DefaultChemObjectBuilder);
			return _hydrogenAdder;
		}
		static CDKHydrogenAdder _hydrogenAdder = null;

		/// <summary>
		/// CanonicalizeSmiles
		/// </summary>
		/// <param name="smiles"></param>
		/// <returns></returns>

		public static string CanonicalizeSmiles(string smiles)
		{
			SmilesGeneratorType smiGenFlags = SmilesGeneratorType.Unique | SmilesGeneratorType.Aromatic;
			return CanonicalizeSmiles(smiles, smiGenFlags);
		}

		public static string CanonicalizeSmiles(
			string smiles,
			SmilesGeneratorType smiGenFlags)
		{
			IAtomContainer mol = SmilesToAtomContainer(smiles);
			if (mol.getAtomCount() == 0) return "";
			else
			{
				string smiles2 = AtomContainerToSmiles(mol, smiGenFlags);
				return smiles2;
			}
		}

		/// <summary>
		/// Get the largest molecule fragment from supplied molfile
		/// </summary>
		/// <param name="molfile"></param>
		/// <returns>Molfile of largest fragment</returns>

		public string GetLargestMoleculeFragmentAsMolfile(
			string molfile)
		{
			IAtomContainer mol, mol2;

			mol = MolfileToAtomContainer(molfile);
			mol2 = GetLargestMoleculeFragment(mol);
			if (mol2 != null)
			{
				string largestFrag = AtomContainerToMolFile(mol2);
				return largestFrag;
			}

			else return "";
		}

		/// <summary>
		/// GetLargestFragmentAsSmiles
		/// </summary>
		/// <param name="smiles"></param>
		/// <returns></returns>

		public static string GetLargestMoleculeFragmentAsSmiles(
			string smiles)
		{
			IAtomContainer mol, mol2;

			mol = SmilesToAtomContainer(smiles);
			mol2 = GetLargestMoleculeFragment(mol);
			if (mol2 != null)
			{
				string largestFrag = AtomContainerToSmiles(mol2);
				return largestFrag;
			}

			else return "";
		}

		/// <summary>
		/// Fragment a smiles string
		/// </summary>
		/// <param name="smiles"></param>
		/// <param name="filterOutCommonCounterIons"></param>
		/// <returns></returns>

		public static List<KeyValuePair<string, IAtomContainer>> FragmentAndCanonicalizeSmiles(
			string smiles,
			bool filterOutCommonCounterIons)
		{
			IAtomContainer mol = SmilesToAtomContainer(smiles);
			List<KeyValuePair<string, IAtomContainer>> frags = FragmentMoleculeAndCanonicalizeSmiles(mol, filterOutCommonCounterIons);
			return frags;
		}

		/// <summary>
		/// Fragment a molecule
		/// </summary>
		/// <param name="mol"></param>
		/// <param name="filterOutCommonCounterIons"></param>
		/// <returns></returns>
		public static List<IAtomContainer> FragmentMolecule(
			IAtomContainer mol,
			bool filterOutCommonCounterIons = true)
		{
			List<KeyValuePair<string, IAtomContainer>> fragSmiList = FragmentMoleculeAndCanonicalizeSmiles(mol, filterOutCommonCounterIons);

			List<IAtomContainer> fragList = new List<IAtomContainer>();
			foreach (KeyValuePair<string, IAtomContainer> kvp in fragSmiList)
				fragList.Add(kvp.Value);

			return fragList;
		}

		/// <summary>
		/// Fragment a molecule
		/// </summary>
		/// <param name="mol"></param>
		/// <returns></returns>

		public static List<KeyValuePair<string, IAtomContainer>> FragmentMoleculeAndCanonicalizeSmiles(
			IAtomContainer mol,
			bool filterOutCommonCounterIons)
		{
			int aci, fi, i1;

			KeyValuePair<string, IAtomContainer> kvp;
			List<KeyValuePair<string, IAtomContainer>> frags = new List<KeyValuePair<string, IAtomContainer>>();

			AtomContainerSet acs = (AtomContainerSet)ConnectivityChecker.partitionIntoMolecules(mol);

			int acc = acs.getAtomContainerCount();
			for (aci = 0; aci < acc; aci++)
			{
				IAtomContainer fragMol = acs.getAtomContainer(aci);
				string fragSmiles = AtomContainerToSmiles(fragMol);
				if (filterOutCommonCounterIons)
				{
					if (CommonSmallFragments.Contains(fragSmiles) ||
					 GetHeavyAtomCount(fragMol) <= 6)
						continue;
				}

				kvp = new KeyValuePair<string, IAtomContainer>(fragSmiles, fragMol);

				int ac = fragMol.getAtomCount();

				for (fi = frags.Count - 1; fi >= 0; fi--) // insert into list so that fragments are ordered largest to smallest
				{
					if (frags[fi].Value.getAtomCount() >= ac) break;
				}
				frags.Insert(fi + 1, kvp);
			}

			return frags;
		}

		/// <summary>
		/// Convert a Molfile into a CDK AtomContainer (e.g. Molecule)
		/// </summary>
		/// <param name="molfile"></param>
		/// <returns></returns>

		public static IAtomContainer MolfileToAtomContainer(string molfile)
		{

			// Do basic read step

			if (Lex.Contains(molfile, "V2000"))
			{
				cdk.io.DefaultChemObjectReader cor;

				java.io.StringReader sr = new java.io.StringReader(molfile);
				cor = new MDLV2000Reader(sr);
				cor.setReaderMode(IChemObjectReader.Mode.RELAXED);

				IAtomContainer mol = (IAtomContainer)cor.read(new AtomContainer());

				ConfigureAtomContainer(mol);
				return mol;
			}


			else if (Lex.Contains(molfile, "V3000"))
				return MolfileV3000ToAtomContainer(molfile);

			else throw new Exception("Unrecognized molfile format");

		}

		/// <summary>
		/// MolfileV3000ToAtomContainer
		/// </summary>
		/// <param name="molfile"></param>
		/// <returns></returns>
		public static IAtomContainer MolfileV3000ToAtomContainer(string molfile)
		{
			// Extract any mass info before conversion to avoid losing our custom info in conversion
			Dictionary<int, int> map = new Dictionary<int, int>();
			if (molfile.Contains(" MASS="))
			{
				map = ExtractMassAttributes(ref molfile);
			}

			cdk.io.DefaultChemObjectReader cor;
			java.io.StringReader sr = new java.io.StringReader(molfile);
			cor = new MDLV3000Reader(sr);
			cor.setReaderMode(IChemObjectReader.Mode.RELAXED);

			IAtomContainer mol = (IAtomContainer)cor.read(new AtomContainer());

			for (int ai = 0; ai < mol.getAtomCount(); ai++)
			{
				IAtom a = mol.getAtom(ai);
				if (map.ContainsKey(ai + 1))
					a.setMassNumber(new java.lang.Integer(map[ai + 1]));
				else a.setMassNumber(null);
			}

			ConfigureAtomContainer(mol);
			return mol;
		}

		/// <summary>
		/// Hack to extract MASS= attributes from a V3000 file because CDK has an issue reading these
		/// </summary>
		/// <param name="molfile"></param>
		/// <returns></returns>
		static Dictionary<int, int> ExtractMassAttributes(ref string molfile)
		{
			Dictionary<int, int> map = new Dictionary<int, int>();

			string molfile2 = molfile.Replace("\r", "");
			molfile2 = molfile2.Replace("\n", " ");
			string[] sa = molfile2.Split(' ');
			for (int si = 0; si < sa.Length; si++)
			{
				if (Lex.Contains(sa[si], "MASS="))
				{
					int mass = int.Parse(sa[si].Substring(5));
					molfile = molfile.Replace(" " + sa[si], "");
					for (int si2 = si - 1; si2 >= 0; si2--)
					{
						if (Lex.Eq(sa[si2], "V30"))
						{
							int ai = int.Parse(sa[si2 + 1]);
							map[ai] = mass;
							break;
						}
					}
				}
			}

			return map;
		}

		public static void ConfigureAtomContainer(IAtomContainer mol)
		{
			AtomContainerManipulator.percieveAtomTypesAndConfigureAtoms(mol); // Perceive Configure atoms

			GetHydrogenAdder().addImplicitHydrogens(mol); // Be sure implicit hydrogens have been added

			ApplyAromaticity(mol);

			//// If bond order 4 was present, deduce bond orders

			//      DeduceBondSystemTool dbst = new DeduceBondSystemTool();
			//      mol = dbst.fixAromaticBondOrders(mol);

			return;
		}

		/// <summary>
		/// Apply an aromaticity model to a molecule and set the aromatic flags
		/// </summary>
		/// <param name="mol"></param>
		/// <returns></returns>

		public static bool ApplyAromaticity(
			IAtomContainer mol)
		{
			bool isAromatic = ApplyAromaticity(mol, ElectronDonation.cdk(), Cycles.cdkAromaticSet());
			return isAromatic;
		}

		/// <summary>
		/// ApplyAromaticity
		/// 
		/// Mimics the CDKHuckelAromaticityDetector
		///  Aromaticity aromaticity = new Aromaticity(ElectronDonation.cdk(),	Cycles.cdkAromaticSet());
		///
		/// Mimics the DoubleBondAcceptingAromaticityDetector
		///  Aromaticity aromaticity = new Aromaticity(ElectronDonation.cdkAllowingExocyclic(), Cycles.cdkAromaticSet());
		///
		/// A good model for writing SMILES
		///  Aromaticity aromaticity = new Aromaticity(ElectronDonation.daylight(), Cycles.all());
		///
		/// A good model for writing MDL/Mol2
		///  Aromaticity aromaticity = new Aromaticity(ElectronDonation.piBonds(), Cycles.all());
		///
		/// </summary>
		/// <param name="mol"></param>
		/// <param name="electronDonation"></param>
		/// <param name="cycleFinder"></param>
		/// <returns></returns>

		public static bool ApplyAromaticity(
			IAtomContainer mol,
			ElectronDonation electronDonation,
			CycleFinder cycleFinder)
		{
			Aromaticity aromaticity = new Aromaticity(electronDonation, cycleFinder);

			try
			{
				bool isAromatic = aromaticity.apply(mol);
				return isAromatic;
			}
			catch (Exception e)
			{
				string msg = e.Message; // cycle computation was intractable
				return false;
			}
		}

		/// <summary>
		/// Convert mol to V2000 Molfile
		/// </summary>
		/// <param name="mol"></param>
		/// <returns></returns>

		public static string AtomContainerToMolFile(IAtomContainer mol)
		{
			java.io.StringWriter sw = new java.io.StringWriter();

			MDLV2000Writer writer = new MDLV2000Writer(sw);
			writer.write(mol);
			writer.close();
			sw.close();

			string molFile = sw.toString();
			return molFile;
		}

		/// <summary>
		/// Convert mol to V3000 Molfile
		/// </summary>
		/// <param name="mol"></param>
		/// <returns></returns>

		public static string AtomContainerToMolFileV3000(IAtomContainer mol)
		{
			java.io.StringWriter sw = new java.io.StringWriter();

			MDLV3000Writer writer = new MDLV3000Writer(sw);
			writer.write(mol);
			writer.close();
			sw.close();

			string molFile = sw.toString();
			return molFile;
		}


		/// <summary>
		/// Convert molecule to Smiles and then back to a molecule for normalization purposes
		/// </summary>
		/// <param name="mol"></param>
		/// <returns></returns>

		public static IAtomContainer AtomContainerToSmilesAndBack(
			IAtomContainer mol)
		{
			string smiles;

			IAtomContainer mol2 = AtomContainerToSmilesAndBack(mol, out smiles);
			return mol2;
		}

		/// <summary>
		/// Convert molecule to Smiles and then back to a molecule for normalization purposes
		/// </summary>
		/// <param name="mol"></param>
		/// <param name="flags"></param>
		/// <param name="smiles"></param>
		/// <returns></returns>

		public static IAtomContainer AtomContainerToSmilesAndBack(
			IAtomContainer mol,
			out string smiles)
		{
			smiles = // convert to smiles with stereo and isotopes
				AtomContainerToSmiles(mol, SmilesGeneratorType.CanonicalWithoutIsotopesAndStereo | SmilesGeneratorType.Aromatic);
			IAtomContainer mol2 = SmilesToAtomContainer(smiles);
			return mol2;
		}

		/// <summary>
		/// Generate Smiles - Canonical, stereo/isomers, aromatic bonds
		/// </summary>
		/// <param name="mol"></param>
		/// <returns></returns>

		public static String AtomContainerToSmiles(IAtomContainer mol)
		{
			string smiles = AtomContainerToSmiles(mol, SmilesGeneratorType.CanonicalWithIsotopesAndStereo | SmilesGeneratorType.Aromatic);

			return smiles;
		}

		/// <summary>
		/// Generate Smiles
		/// </summary>
		/// <param name="mol"></param>
		/// <param name="flags"></param>
		/// <returns></returns>
		public static String AtomContainerToSmiles(
			IAtomContainer mol,
			SmilesGeneratorType flags)
		{
			ApplyAromaticity(mol);

			SmilesGenerator sg = null;

			if ((flags & SmilesGeneratorType.Generic) != 0)
				sg = SmilesGenerator.generic();

			else if ((flags & SmilesGeneratorType.Isomeric) != 0)
				sg = SmilesGenerator.isomeric();

			else if ((flags & SmilesGeneratorType.Unique) != 0)
				sg = SmilesGenerator.unique();

			else if ((flags & SmilesGeneratorType.Absolute) != 0)
				sg = SmilesGenerator.unique();

			else throw new Exception("Canonical/Stereo/Isotop types not defined");

			if ((flags & SmilesGeneratorType.NotAromatic) != 0) { } // not aromatic
			else sg = sg.aromatic(); // aromatic by default even if not specified

			if ((flags & SmilesGeneratorType.WithAtomClasses) != 0)
				sg = sg.withAtomClasses();

			string smiles = sg.create(mol);
			return smiles;
		}

		/// <summary>
		/// Get atom count for structure
		/// </summary>
		/// <returns></returns>

		public int AtomCount => NativeMol.getAtomCount();

		/// <summary>
		/// Get heavy atom count
		/// </summary>

		public int HeavyAtomCount => GetHeavyAtomCount(NativeMol);

		/// <summary>
		/// Return true if mol contains a query feature
		/// </summary>

		public bool ContainsQueryFeature
		{
			get { throw new NotImplementedException(); }
		}

		public double MolWeight // get mol weight
		{
			get
			{
				IMolecularFormula mfm = MolecularFormulaManipulator.getMolecularFormula(NativeMol);
				return MolecularFormulaManipulator.getMass(mfm, MolecularFormulaManipulator.MolWeight);
			}
		}

		public string MolFormula => GetMolecularFormula(NativeMol);// get mol formula


		/// <summary>
		/// GetMolecularFormula
		/// </summary>
		/// <param name="mol"></param>
		/// <returns></returns>

		public static string GetMolecularFormula(IAtomContainer mol)
		{
			IMolecularFormula moleculeFormula = MolecularFormulaManipulator.getMolecularFormula(mol);
			String formula = MolecularFormulaManipulator.getString(moleculeFormula);
			return formula;
		}

		/// <summary>
		///  Get the sum of the heavy atom and bond counts for a smiles string
		/// </summary>
		/// <param name="smiles"></param>
		/// <param name="haCnt"></param>
		/// <param name="hbCnt"></param>

		public static void GetHeavyAtomBondCounts(
			string smiles,
			out int haCnt,
			out int hbCnt)
		{
			IAtomContainer mol = SmilesToAtomContainer(smiles);
			haCnt = GetHeavyAtomCount(mol);
			hbCnt = GetHeavyBondCount(mol);
			return;
		}

		/// <summary>
		/// GetHeavyAtomCount
		/// </summary>
		/// <param name="mol"></param>
		/// <returns></returns>

		public static int GetHeavyAtomCount(IAtomContainer mol)
		{
			int haCnt = 0;

			for (int ai = 0; ai < mol.getAtomCount(); ai++)
			{
				IAtom atom = mol.getAtom(ai);
				if (atom.getAtomicNumber().intValue() == 1 ||  // do not count hydrogens
						atom.getSymbol().Equals("H"))
				{
					continue;
				}
				else
				{
					haCnt++;
				}
			}

			return haCnt;
		}

		/// <summary>
		/// Count bonds connecting two heavy atoms
		/// </summary>
		/// <param name="mol"></param>
		/// <returns></returns>

		public static int GetHeavyBondCount(IAtomContainer mol)
		{
			int hbCnt = 0;

			for (int bi = 0; bi < mol.getBondCount(); bi++)
			{
				IBond b = mol.getBond(bi);
				if (b.getAtomCount() != 2) continue;

				IAtom a = b.getAtom(0); // first atom
				if (a.getAtomicNumber().intValue() == 1 ||  // do not count hydrogens
					a.getSymbol().Equals("H"))

					a = b.getAtom(1); // second atom
				if (a.getAtomicNumber().intValue() == 1 ||  // do not count hydrogens
					a.getSymbol().Equals("H"))
					continue;

				hbCnt++;
			}

			return hbCnt;
		}



		/// <summary>
		/// Get largest fragment of a molecule
		/// </summary>
		/// <param name="mol"></param>
		/// <returns></returns>

		public static IAtomContainer GetLargestMoleculeFragment(
			IAtomContainer mol)
		{
			int acc;
			IAtomContainer largestFrag = GetLargestMoleculeFragment(mol, out acc);
			return largestFrag;
		}

		public static IAtomContainer GetLargestMoleculeFragment(
			IAtomContainer mol,
			out int acc)
		{
			AtomContainerSet acs;
			IAtomContainer mol2, molMain = null;
			acs = null;

			if (ConnectivityChecker.isConnected(mol))
			{
				acc = 1;
				return mol;
			}

			acs = (AtomContainerSet)ConnectivityChecker.partitionIntoMolecules(mol);
			int largestAc = -1;

			acc = acs.getAtomContainerCount();
			for (int aci = 0; aci < acc; aci++)
			{
				mol2 = acs.getAtomContainer(aci);
				int ac2 = mol2.getAtomCount();
				if (ac2 > largestAc)
				{
					largestAc = mol2.getAtomCount();
					molMain = mol2;
				}
			}

			return molMain;
		}

		/// <summary>
		/// Convert SmilesToMolfile
		/// </summary>
		/// <param name="smiles"></param>
		/// <returns></returns>

		public string SmilesStringToMolfileString(string smiles)
		{
			if (Lex.IsUndefined(smiles)) return "";

			IAtomContainer mol = SmilesToAtomContainer(smiles);
			if (mol.getAtomCount() == 0) return "";

			string molfile = AtomContainerToMolFile(mol);
			return molfile;
		}

		/// <summary>
		/// Convert molfile to Smiles
		/// </summary>
		/// <param name="molfile"></param>
		/// <returns></returns>

		public string MolfileStringToSmilesString(string molfile)
		{
			if (Lex.IsUndefined(molfile)) return "";

			IAtomContainer mol = MolfileToAtomContainer(molfile);
			if (mol.getAtomCount() == 0) return "";

			string smiles = AtomContainerToSmiles(mol);
			return smiles;
		}


		/// <summary>
		/// SmilesToAtomContainer
		/// </summary>
		/// <param name="smiles"></param>
		/// <returns></returns>

		public static IAtomContainer SmilesToAtomContainer(string smiles)
		{
			try
			{
				SmilesParser sp = new SmilesParser(DefaultChemObjectBuilder);

				IAtomContainer mol = sp.parseSmiles(smiles); // may get "could not parse error" for some CorpIds, e.g.: 3401013, 3418008, 3428937

				ConfigureAtomContainer(mol);

				return mol;
			}

			catch (Exception ex)
			{
				ParseSmilesErrorCount++;
				LastParseSmilesError = ex.Message;
				throw new Exception(ex.Message, ex);
			}
		}


		/// <summary>
		/// Convert an InChI string to an IAtomContainer
		/// </summary>
		/// <param name="inchiString"></param>
		/// <returns></returns>

		public static IAtomContainer InChIToAtomContainer(string inchiString)
		{
			string warningMsg, errorMsg;

			InChIGeneratorFactory factory = InChIGeneratorFactory.getInstance();
			InChIToStructure intostruct = factory.getInChIToStructure(inchiString, DefaultChemObjectBuilder);

			INCHI_RET ret = intostruct.getReturnStatus();
			if (ret == INCHI_RET.WARNING) // Structure generated, but with warning message
			{
				warningMsg = "InChI warning: " + intostruct.getMessage();
			}

			else if (ret != INCHI_RET.OKAY)  // Structure generation failed
			{
				errorMsg = "Structure generation failed failed: " + ret.toString() + " [" + intostruct.getMessage() + "]";
				throw new Exception(errorMsg);
			}

			IAtomContainer mol = intostruct.getAtomContainer();


			return mol;
		}

		/// <summary>
		/// Common small fragments, Canonical Smiles, first block of Inchi Key, formula
		/// </summary>

		public static HashSet<string> CommonSmallFragments = new HashSet<string>
		{
		"[B-](F)(F)(F)F", // "ODGCEQLVLXJUCC", // BF4
		"C(=O)(C(=O)[O-])O", //"MUBZPKHOEPUJKR", // C2H2O4
		"CC(=O)O", // "QTBSBXVTEAMEQO", // C2H4O2
		"C(=O)(C(F)(F)F)O", // "DTQVDTLACAAQTR", // C2HF3O2

		"C(CC(=O)O)C(=O)O", // "VZCYOOQTPOCHFL", // C4H4O4
		"C(=CC(=O)[O-])C(=O)[O-]", // "VZCYOOQTPOCHFL", // C4H4O4

		"C(CC(=O)O)C(=O)O", // "KDYFGRWQOYBRFD", // C4H6O4
		"C(CC(=O)[O-])C(=O)[O-]", // "KDYFGRWQOYBRFD", // C4H6O4
		
		"C(C(C(=O)O)O)(C(=O)O)O", // "FEWJPZIEWOKRBE", // C4H6O6
		"C(C(C(=O)[O-])O)(C(=O)[O-])O", // "FEWJPZIEWOKRBE", // C4H6O6

		"C(=O)O", // "BDAGIHXWWSANSR", // CH2O2
		"C(=O)[O-]", // "BDAGIHXWWSANSR", // CH2O2

		"C(=O)(O)[O-]", // "BVKZGUZCCUSVTD", // CH2O3
		"C(=O)([O-])[O-]", // "BVKZGUZCCUSVTD", // CH2O3

		"OCl(=O)(=O)=O", // "VLTRZXGMWDSKGL", // ClHO4
		"[O-]Cl(=O)(=O)=O", // "VLTRZXGMWDSKGL", // ClHO4

		"OS(=O)(=O)O", // "[O-]S(=O)(=O)[O-]", // H2O4S
		"[O-]S(=O)(=O)[O-]", // "[O-]S(=O)(=O)[O-]", // H2O4S

		"ON(=O)=O", // "GRYLNZFGIOXLOG", // HNO3
		"[N+](=O)(O)[O-]", // "GRYLNZFGIOXLOG", // HNO3

		"O", // "XLYOFNOQVPJJNP", // H2O	Water
		"N", // "QGZKDVFQNNGYKY", // H3N
		"Br", // "CPELXLSAUQHCOX", // HBr	Hydrogen Bromide
		"Cl", // VEXZGXHMUGYJMC", // HCl	Hydrogen Chloride
		"I", // "XMBWDFGMSWQBCA", // HI
		"K", // "NPYPAHLBTDXSSS", // K	Potassium
		"Mg", // "XYAGEBWLYIDCRX", // Mg	Magnesium
		"Na", // "FKNQFGJONOIPTF"  // Na	Sodium
		};

	}

	/// <summary>
	/// SmilesGeneratorType
	/// </summary>
	public enum SmilesGeneratorType
	{
		NonCanonicalWithoutIsotopesAndStereo = 1,
		NonCanonicalWithIsotopesAndStereo = 2,
		CanonicalWithoutIsotopesAndStereo = 4,
		CanonicalWithIsotopesAndStereo = 8,

		Generic = 1, // non-canonical without isotopes / stereo 
		Isomeric = 2, // non-canonical with isotopes / stereo 
		Unique = 4, // canonical without isotopes / stereo 
		Absolute = 8, // canonical with isotopes / stereo 

		Aromatic = 16, // aromatic (lower-case) SMILES (default, unless NotAromatic specified)
		NotAromatic = 32, // aromatic (lower-case) SMILES.
		WithAtomClasses = 62 // include atom classes
	}

	//		                   non-canonical canonical
	//                       ------------- ---------			 
	// no isotopes / stereo     generic    unique
	// with isotopes / stereo   isomeric   absolute

}
