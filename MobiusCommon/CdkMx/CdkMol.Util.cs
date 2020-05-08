using Mobius.ComOps;
using Mobius.Data;

using NCDK;
using NCDK.Aromaticities;
using NCDK.Default;
using NCDK.Depict;
using NCDK.Fingerprints;
using NCDK.Graphs;
using NCDK.Graphs.InChI;
using NCDK.Isomorphisms;
using NCDK.Isomorphisms.Matchers;
using NCDK.IO;
using NCDK.IO.Iterator;
//using NCDK.Silent;
using NCDK.Smiles;
using NCDK.Tools;
using NCDK.Tools.Manipulator;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Mobius.CdkMx
{

	public partial class CdkMol : ICdkMol
	{

		public static int ParseSmilesErrorCount = 0;
		public static string LastParseSmilesError = "";

		/// <summary>
		/// Get the version of Java that was used to build the CDK .jar file
		/// </summary>
		/// <returns></returns>

		/// <summary>
		/// GetHydrogenAdder
		/// </summary>
		/// <returns></returns>

		static CDKHydrogenAdder GetHydrogenAdder()
		{

			if (_hydrogenAdder == null)
				_hydrogenAdder = CDKHydrogenAdder.GetInstance();
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
			if (mol.Atoms.Count == 0) return "";
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

		public string GetLargestMolfileMoleculeFragment(
			string molfile)
		{
			IAtomContainer mol, mol2;

			mol = MolfileToAtomContainer(molfile);
			mol2 = GetLargestMoleculeFragment(mol);
			if (mol2 != null)
			{
				string largestFragMolfile = AtomContainerToMolfile(mol2);
				return largestFragMolfile;
			}

			else return "";
		}

		/// <summary>
		/// Get the largest molecule fragment from supplied smiles
		/// </summary>
		/// <param name="smiles"></param>
		/// <returns>Smiles of largest fragment</returns>

		public string GetLargestSmilesMoleculeFragment(
			string smiles)
		{
			IAtomContainer mol, mol2;

			mol = SmilesToAtomContainer(smiles);
			mol2 = GetLargestMoleculeFragment(mol);
			if (mol2 != null)
			{
				string largestFragSmiles = AtomContainerToSmiles(mol2);
				return largestFragSmiles;
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

			IReadOnlyList<IAtomContainer> acs = ConnectivityChecker.PartitionIntoMolecules(mol);

			int acc = acs.Count;
			for (aci = 0; aci < acc; aci++)
			{
				IAtomContainer fragMol = acs[aci];
				string fragSmiles = AtomContainerToSmiles(fragMol);
				if (filterOutCommonCounterIons)
				{
					if (CommonSmallFragments.Contains(fragSmiles) ||
					 GetHeavyAtomCount(fragMol) <= 6)
						continue;
				}

				kvp = new KeyValuePair<string, IAtomContainer>(fragSmiles, fragMol);

				int ac = fragMol.Atoms.Count;

				for (fi = frags.Count - 1; fi >= 0; fi--) // insert into list so that fragments are ordered largest to smallest
				{
					if (frags[fi].Value.Atoms.Count >= ac) break;
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
				DefaultChemObjectReader cor;

				StringReader sr = new StringReader(molfile);
				cor = new MDLV2000Reader(sr);
				cor.ReaderMode = ChemObjectReaderMode.Relaxed;

				IAtomContainer mol = (IAtomContainer)cor.Read(new AtomContainer());
				cor.Close();

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

			DefaultChemObjectReader cor;
			StringReader sr = new StringReader(molfile);
			cor = new MDLV3000Reader(sr);
			cor.ReaderMode = ChemObjectReaderMode.Relaxed;

			IAtomContainer mol = cor.Read(new AtomContainer());
			cor.Close();

			for (int ai = 0; ai < mol.Atoms.Count; ai++)
			{
				IAtom a = mol.Atoms[ai];
				if (map.ContainsKey(ai + 1))
					a.MassNumber = map[ai + 1];
				//a.setMassNumber(new java.lang.Integer(map[ai + 1]));
				else a.MassNumber = null;
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
			//SystemUtil.LoadAssembly(@"C:\Mobius_OpenSource\MobiusClient\Client\bin\Debug\IKVM.OpenJDK.XML.Parse.dll");
			//var s = new com.sun.org.apache.xerces.@internal.jaxp.SAXParserFactoryImpl();
			//var t = new com.sun.org.apache.xalan.@internal.xsltc.trax.TransformerFactoryImpl();

			try // Throws “Provider com.sun.org.apache.xalan.internal.xsltc.trax.TransformerFactoryImpl not found” 
			{
				AtomContainerManipulator.PercieveAtomTypesAndConfigureAtoms(mol); // Perceive Configure atoms
			}
			catch (Exception ex) { ex = ex; }

			GetHydrogenAdder().AddImplicitHydrogens(mol); // Be sure implicit hydrogens have been added

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
			bool isAromatic = ApplyAromaticity(mol, ElectronDonation.CDKModel, Cycles.CDKAromaticSetFinder);
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
			ICycleFinder cycleFinder)
		{
			Aromaticity aromaticity = new Aromaticity(electronDonation, cycleFinder);

			try
			{
				bool isAromatic = aromaticity.Apply(mol);
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

		public static string AtomContainerToMolfile(IAtomContainer mol)
		{
			StringWriter sw = new StringWriter();

			MDLV2000Writer writer = new MDLV2000Writer(sw);
			writer.Write(mol);
			writer.Close();
			sw.Close();

			string molFile = sw.ToString();
			return molFile;
		}

		/// <summary>
		/// Convert mol to V3000 Molfile
		/// </summary>
		/// <param name="mol"></param>
		/// <returns></returns>

		public static string AtomContainerToMolFileV3000(IAtomContainer mol)
		{
			StringWriter sw = new StringWriter();

			MDLV3000Writer writer = new MDLV3000Writer(sw);
			writer.Write(mol);
			writer.Close();
			sw.Close();

			string molFile = sw.ToString();
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
				sg = SmilesGenerator.Generic;

			else if ((flags & SmilesGeneratorType.Isomeric) != 0)
				sg = SmilesGenerator.Isomeric;

			else if ((flags & SmilesGeneratorType.Unique) != 0)
				sg = SmilesGenerator.Unique;

			else if ((flags & SmilesGeneratorType.Absolute) != 0)
				sg = SmilesGenerator.Unique;

			else throw new Exception("Canonical/Stereo/Isotop types not defined");

			if ((flags & SmilesGeneratorType.NotAromatic) != 0) { } // not aromatic
			else sg = new SmilesGenerator(SmiFlavors.UseAromaticSymbols); // aromatic by default even if not specified

			if ((flags & SmilesGeneratorType.WithAtomClasses) != 0)
				sg = new SmilesGenerator(SmiFlavors.AtomAtomMap);

			string smiles = sg.Create(mol);
			return smiles;
		}

		/// <summary>
		/// Get atom count for structure
		/// </summary>
		/// <returns></returns>

		public int AtomCount => NativeMol.Atoms.Count;

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

		/// <summary>
		/// Get mol weight for a molfile
		/// </summary>
		/// <param name="molfile"></param>
		/// <returns></returns>

		public double GetMolWeight(
			string molfile)
		{
			CdkMol mol = new CdkMol(MoleculeFormat.Molfile, molfile);
			double mw = mol.MolWeight;
			return mw;
		}


		public double MolWeight // get mol weight
		{
			get
			{
			IMolecularFormula mfm = MolecularFormulaManipulator.GetMolecularFormula(NativeMol);
				return MolecularFormulaManipulator.GetMass(mfm);
			}
		}

		public string MolFormula =>	GetMolecularFormula(NativeMol);// get mol formula

		/// <summary>
		/// GetMolecularFormula
		/// </summary>
		/// <param name="mol"></param>
		/// <returns></returns>

		public static string GetMolecularFormula(IAtomContainer mol)
		{
			IMolecularFormula moleculeFormula = MolecularFormulaManipulator.GetMolecularFormula(mol);
			String formula = MolecularFormulaManipulator.GetString(moleculeFormula);
			return formula;
		}

		/// <summary>
		/// GetMolFormulaDotDisconnect
		/// </summary>
		/// <param name="molfile"></param>
		/// <returns></returns>

		public string GetMolFormulaDotDisconnect(
			string molfile)
		{
			ICdkMol mol = new CdkMol(MoleculeFormat.Molfile, molfile);
			string mf = GetMolFormulaDotDisconnect(mol as CdkMol);
			return mf;
		}

		/// <summary>
		/// GetMolFormulaDotDisconnect
		/// </summary>
		/// <param name="mol"></param>
		/// <returns></returns>

		public String GetMolFormulaDotDisconnect(
			ICdkMol mol)
		{
			string mf = GetMolFormula(mol as CdkMol, includeSpaces: false, separateFragments: true);
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

		public String GetMolFormula(
			ICdkMol mol,
			bool includeSpaces = false,
			bool separateFragments = false,
			bool use2Hand3HforHydrogenIsotopes = false,
			bool ignoreIsotopes = false)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///  Get the sum of the heavy atom and bond counts for a smiles string
		/// </summary>
		/// <param name="smiles"></param>
		/// <param name="haCnt"></param>
		/// <param name="hbCnt"></param>

		public void GetHeavyAtomBondCounts(
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

			for (int ai = 0; ai < mol.Atoms.Count; ai++)
			{
				IAtom atom = mol.Atoms[ai];
				if (atom.AtomicNumber == 1 ||  // do not count hydrogens
						atom.Symbol.Equals("H"))
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

			for (int bi = 0; bi < mol.Bonds.Count; bi++)
			{
				IBond b = mol.Bonds[bi]; // was molmol.Bonds[bi];
				if (b.Atoms.Count != 2) continue;

				IAtom a = b.Atoms[0]; // first atom
				if (a.AtomicNumber == 1 ||  // do not count hydrogens
					a.Symbol.Equals("H"))

					a = b.Atoms[1]; // second atom
				if (a.AtomicNumber == 1 ||  // do not count hydrogens
					a.Symbol.Equals("H"))
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
			IAtomContainer mol2, molMain = null;

			if (ConnectivityChecker.IsConnected(mol))
			{
				acc = 1;
				return mol;
			}

			IReadOnlyList<IAtomContainer> acs = ConnectivityChecker.PartitionIntoMolecules(mol);
			int largestAc = -1;

			acc = acs.Count;
			for (int aci = 0; aci < acc; aci++)
			{
				mol2 = acs[aci];
				int ac2 = mol2.Atoms.Count;
				if (ac2 > largestAc)
				{
					largestAc = mol2.Atoms.Count;
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
			if (mol.Atoms.Count == 0) return "";

			string molfile = AtomContainerToMolfile(mol);
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
			if (mol.Atoms.Count == 0) return "";

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
				SmilesParser sp = new SmilesParser(ChemObjectBuilder.Instance);

				IAtomContainer mol = sp.ParseSmiles(smiles); // may get "could not parse error" for some CorpIds, e.g.: 111, 222, 333

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

			InChIGeneratorFactory factory = InChIGeneratorFactory.Instance;
			InChIToStructure intostruct = factory.GetInChIToStructure(inchiString, ChemObjectBuilder.Instance);

			InChIReturnCode ret = intostruct.ReturnStatus;
			if (ret == InChIReturnCode.Warning) // Structure generated, but with warning message
			{
				warningMsg = "InChI warning: " + intostruct.Message;
			}

			else if (ret != InChIReturnCode.Ok)  // Structure generation failed
			{
				errorMsg = "Structure generation failed failed: " + ret.ToString() + " [" + intostruct.Message + "]";
				throw new Exception(errorMsg);
			}

			IAtomContainer mol = intostruct.AtomContainer;


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
