using Mobius.ComOps;

using java.io;

using cdk = org.openscience.cdk;
using org.openscience.cdk;
using org.openscience.cdk.inchi;
using org.openscience.cdk.interfaces;
using org.openscience.cdk.fingerprint;
using org.openscience.cdk.smiles;
using org.openscience.cdk.tools.manipulator;
using org.openscience.cdk.graph;
using org.openscience.cdk.qsar.result;
using org.openscience.cdk.io;
using org.openscience.cdk.io.iterator;
using org.openscience.cdk.tools;
using org.openscience.cdk.layout;
using org.openscience.cdk.config;
using org.openscience.cdk.config.isotopes;

using net.sf.jniinchi; // low level IUPAC interface, needed for access to some enumerations

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Mobius.CdkMx
{

	public partial class CdkMol // : IMolLibMx
	{

		static int hilightIsotopeValue = -100;

		static int IntegrateAndHilightMmpStructureCount = 0;
		static double IntegrateAndHilightMmpStructureTotalTime = 0;
		static double IntegrateAndHilightMmpStructureAvgTime = -1;

		/// <summary>
		/// Remove explicit and implicit hydrogens bonded to positive nitrogens
		/// </summary>
		/// <param name="org"></param>
		/// <returns></returns>
		public static int RemoveHydrogensBondedToPositiveNitrogens(IAtomContainer org)
		{
			int chg, impHydCnt;
			int implicitHydRemoved = 0;

			HashSet<IAtom> atomsToRemove = new HashSet<IAtom>();
			HashSet<IBond> bondsToRemove = new HashSet<IBond>();
			int nOrgAtoms = org.getAtomCount();
			int nOrgBonds = org.getBondCount();

			// Get H atoms and their bonds to pos-charged Nitrogen, adjusting charge

			for (int bi = 0; bi < org.getBondCount(); bi++)
			{
				IBond bond = org.getBond(bi);
				if (bond.getAtomCount() != 2) continue;

				IAtom a1 = bond.getAtom(0);
				IAtom a2 = bond.getAtom(1);
				if (a1.getSymbol() == "H" && a2.getSymbol() == "N" && GetFormalCharge(a2) > 0)
				{
					chg = GetFormalCharge(a2) - 1;
					SetFormalCharge(a2, chg);
					atomsToRemove.Add(a1);
					bondsToRemove.Add(bond);
				}

				else if (a2.getSymbol() == "H" && a1.getSymbol() == "N" && GetFormalCharge(a1) > 0)
				{
					chg = GetFormalCharge(a1) - 1;
					SetFormalCharge(a1, chg);
					atomsToRemove.Add(a2);
					bondsToRemove.Add(bond);
				}
			}

			// Check for implicit H attached to pos-charged N

			for (int ai = 0; ai < nOrgAtoms; ai++)
			{
				IAtom a = org.getAtom(ai);
				if (a.getSymbol() == "N" && GetFormalCharge(a) > 0 && GetImplicitHydrogenCount(a) > 0)
				{
					chg = GetFormalCharge(a) - 1;
					SetFormalCharge(a, chg);

					impHydCnt = GetImplicitHydrogenCount(a) - 1;
					SetImplicitHydrogenCount(a, impHydCnt);
					implicitHydRemoved++;
				}
			}

			if (implicitHydRemoved > 0) implicitHydRemoved = implicitHydRemoved; // debug

			if (atomsToRemove.Count == 0) return implicitHydRemoved; // just return if no explicit H to remove

			// Get list of atoms to keep

			IAtom[] cpyAtoms = new IAtom[nOrgAtoms - atomsToRemove.Count];
			int nCpyAtoms = 0;

			for (int ai = 0; ai < nOrgAtoms; ai++)
			{
				IAtom atom = org.getAtom(ai);
				if (!atomsToRemove.Contains(atom))
					cpyAtoms[nCpyAtoms++] = atom;
			}

			org.setAtoms(cpyAtoms);

			// Get list of bonds to keep

			IBond[] cpyBonds = new IBond[nOrgBonds - bondsToRemove.Count];
			int nCpyBonds = 0;

			for (int bi = 0; bi < org.getBondCount(); bi++)
			{
				IBond bond = org.getBond(bi);
				if (!bondsToRemove.Contains(bond))
					cpyBonds[nCpyBonds++] = bond;
			}

			org.setBonds(cpyBonds);

			return atomsToRemove.Count + implicitHydRemoved;
		}

		/// <summary>
		/// Remove the following features from a molecule
		///  - Atom non-standard mass
		///  - Stereo chemistry
		///  - explicit hydrogens
		/// </summary>
		/// <param name="src"></param>
		/// <returns>Modified mol</returns>

		public static IAtomContainer RemoveIsotopesStereoExplicitHydrogens(
			IAtomContainer src)
		{
			IAtom[] atoms = new IAtom[src.getAtomCount()];
			IBond[] bonds = new IBond[src.getBondCount()];

			IChemObjectBuilder builder = src.getBuilder();

			for (int i = 0; i < atoms.Length; i++)
			{
				IAtom atom = src.getAtom(i);
				IAtom atom2 = (IAtom)builder.newInstance(typeof(IAtom), atom.getSymbol());
				SetImplicitHydrogenCount(atom2, GetImplicitHydrogenCount(atom));
				atom2.setPoint2d(atom.getPoint2d());
				atoms[i] = atom2;
			}

			for (int i = 0; i < bonds.Length; i++)
			{
				IBond bond = src.getBond(i);

				int u = src.getAtomNumber(bond.getAtom(0));
				int v = src.getAtomNumber(bond.getAtom(1));
				IBond bond2 = (IBond)builder.newInstance(typeof(IBond), atoms[u], atoms[v]);

				bond2.setIsAromatic(bond.isAromatic());
				bond2.setIsInRing(bond.isInRing());
				bond2.setOrder(bond.getOrder());

				bond2.setFlag(CDKConstants.ISAROMATIC, bond.getFlag(CDKConstants.ISAROMATIC));
				bond2.setFlag(CDKConstants.SINGLE_OR_DOUBLE, bond.getFlag(CDKConstants.SINGLE_OR_DOUBLE));

				bonds[i] = bond2;
			}

			IAtomContainer dest = (IAtomContainer)builder.newInstance(typeof(IAtomContainer));
			dest.setAtoms(atoms);
			dest.setBonds(bonds);

			AtomContainerManipulator.percieveAtomTypesAndConfigureAtoms(dest);
			dest = AtomContainerManipulator.suppressHydrogens(dest);
			GetHydrogenAdder().addImplicitHydrogens(dest);

			return dest;
		}

		/// <summary>
		/// RemovePseudoAtoms
		/// </summary>
		/// <param name="mol"></param>
		/// <returns>Pseudo atom count</returns>

		public static int RemovePseudoAtoms(IAtomContainer mol)
		{
			List<IAtom> pseudoAtoms = new List<IAtom>();
			for (int ai = 0; ai < mol.getAtomCount(); ai++)
			{
				IAtom atom = mol.getAtom(ai);
				if (atom is IPseudoAtom)
					pseudoAtoms.Add(atom);
			}

			if (pseudoAtoms.Count == 0) return 0;

			foreach (IAtom atom in pseudoAtoms)
			{
				mol.removeAtomAndConnectedElectronContainers(atom);
			}

			AtomContainerManipulator.percieveAtomTypesAndConfigureAtoms(mol);
			mol = AtomContainerManipulator.suppressHydrogens(mol);
			GetHydrogenAdder().addImplicitHydrogens(mol);

			return pseudoAtoms.Count;
		}

		/// <summary>
		/// Integrate MMP difference and common fragments
		/// </summary>
		/// <param name="smilesFrags"></param>
		/// <returns>Molfile of integrated mol with difference fragment hilighted via special atom isotope values </returns>

		public static string IntegrateAndHilightMmpStructure(
			string smilesFrags)
		{
			IAtomContainer mol;
			IAtom atom;
			string frag, context1, context2, molfile, chime = "", molfile2 = "";
			int ai, atomCount;

			//IsotopeConversionTest(""); // debug
			DateTime t0 = DateTime.Now;

			try
			{
				Lex.Split(smilesFrags, ":", out frag, out context1, out context2);
				if (Lex.Eq(context2, "NULL")) context2 = ""; // fixup for undefined Redshift fragment smiles fragment represented as "NULL" rather than a blank string

				mol = SmilesToAtomContainer(frag);

				atomCount = mol.getAtomCount();
				for (ai = 0; ai < atomCount; ai++) // set isotope labels to mark difference frag
				{
					atom = mol.getAtom(ai);
					int mn = GetMassNumber(atom);
					if (mn == 0)
						SetMassNumber(atom, hilightIsotopeValue); // mark with special value
					else if (mn > 0)
						SetMassNumber(atom, -mn); // negate original value to indicate part of difference frag
				}

				if (Lex.IsDefined(context1))
				{
					JoinMmpFragments(mol, context1, 1);
					atomCount = mol.getAtomCount();
				}

				if (Lex.IsDefined(context2))
				{
					JoinMmpFragments(mol, context2, 2);
					atomCount = mol.getAtomCount();
				}

				mol = AtomContainerManipulator.suppressHydrogens(mol); // fix potential valency issues
				atomCount = mol.getAtomCount();

				for (ai = 0; ai < atomCount; ai++) // clean up structure for good display
				{
					atom = mol.getAtom(ai);

					if (atom.getValency() != null && atom.getValency().intValue() != 0)
						atom.setValency(new java.lang.Integer(0));

					if (GetImplicitHydrogenCount(atom) != 0)
						SetImplicitHydrogenCount(atom, 0);
				}

				AtomContainerManipulator.percieveAtomTypesAndConfigureAtoms(mol);
				mol = AtomContainerManipulator.suppressHydrogens(mol);
				GetHydrogenAdder().addImplicitHydrogens(mol);
				mol = GenerateCoordinates(mol);
				atomCount = mol.getAtomCount();

				molfile = ConvertIsotopeValuesToHilighting(mol); // about 3.0 ms per IntegrateAndHilightMmpStructure call

				double msTime = TimeOfDay.Delta(t0);

				IntegrateAndHilightMmpStructureCount++;
				IntegrateAndHilightMmpStructureTotalTime += msTime;
				IntegrateAndHilightMmpStructureAvgTime = IntegrateAndHilightMmpStructureTotalTime / IntegrateAndHilightMmpStructureCount;

				return molfile; 
			}

			catch (Exception ex)
			{
				string msg = "Error integrating Smiles Fragments: " + smilesFrags + DebugLog.FormatExceptionMessage(ex, true);
				DebugLog.Message(msg);
				return "";
			}
		}

		/// <summary>
		/// Get integer MassNumber (isotope value) for an atom
		/// </summary>
		/// <param name="atom"></param>
		/// <returns></returns>

		public static int GetMassNumber(IAtom atom)
		{
			if (atom.getMassNumber() == null) return 0;
			else return atom.getMassNumber().intValue();
		}

		/// <summary>
		/// Get the major isotope mass number for an element
		/// </summary>
		/// <param name="atomicNumber"></param>
		/// <returns></returns>

		public static int GetMajorIsotopeMassNumber(int atomicNumber)
		{
			Isotopes factory = Isotopes.getInstance();
			IIsotope major = factory.getMajorIsotope(atomicNumber);
			int mass = major.getMassNumber().intValue();
			return mass;
		}

		/// <summary>
		/// Set integer MassNumber (isotope value) for an atom
		/// </summary>
		/// <param name="atom"></param>
		/// <param name="mass"></param>

		public static void SetMassNumber(
			IAtom atom,
			int mass)
		{
			atom.setMassNumber(new java.lang.Integer(mass));
			return;
		}

		/// <summary>
		/// Get integer FormalCharge for an atom
		/// </summary>
		/// <param name="atom"></param>
		/// <returns></returns>

		public static int GetFormalCharge(IAtom atom)
		{
			if (atom.getFormalCharge() == null) return 0;
			else return atom.getFormalCharge().intValue();
		}

		/// <summary>
		/// Set integer FormalCharge for an atom
		/// </summary>
		/// <param name="atom"></param>
		/// <param name="charge"></param>

		public static void SetFormalCharge(
			IAtom atom,
			int charge)
		{
			atom.setFormalCharge(new java.lang.Integer(charge));
			return;
		}

		/// <summary>
		/// Get integer ImplicitHydrogenCount for an atom
		/// </summary>
		/// <param name="atom"></param>
		/// <returns></returns>

		public static int GetImplicitHydrogenCount(IAtom atom)
		{
			if (atom.getImplicitHydrogenCount() == null) return 0;
			else return atom.getImplicitHydrogenCount().intValue();
		}

		/// <summary>
		/// Set integer ImplicitHydrogenCount (isotope value) for an atom
		/// </summary>
		/// <param name="atom"></param>
		/// <param name="hcnt"></param>

		public static void SetImplicitHydrogenCount(
			IAtom atom,
			int hcnt)
		{
			atom.setImplicitHydrogenCount(new java.lang.Integer(hcnt));
			return;
		}

		/// <summary>
		/// Convert isotope labels to hilighting.
		/// </summary>
		/// <param name="mol"></param>
		/// <returns></returns>

		public static string ConvertIsotopeValuesToHilighting(
			IAtomContainer mol)
		{
			bool hilight;
			string molfile2 = "";
			bool clearAttachmentPointLabels = true;
			bool hilightAttachmentBond = true;

			List<int> atomSet = new List<int>();
			List<int> bondSet = new List<int>();

			for (int bi = 0; bi < mol.getBondCount(); bi++)
			{
				IBond b = mol.getBond(bi);
				if (b.getAtomCount() != 2) continue;

				IAtom a1 = b.getAtom(0);
				IAtom a2 = b.getAtom(1);

				if (hilightAttachmentBond)
					hilight = (GetMassNumber(a1) != 0 && GetMassNumber(a2) != 0);
				else hilight = (GetMassNumber(a1) < 0 && GetMassNumber(a2) < 0);
				if (hilight)
				{
					bondSet.Add(bi);
				}
			}

			//String posMap = ""; // debug (note that hydrogens can get repositioned)
			for (int ai = 0; ai < mol.getAtomCount(); ai++) // scan atoms for hilighting and reset isotope values
			{
				IAtom a = mol.getAtom(ai);

				hilight = GetMassNumber(a) < 0;
				if (hilight)
				{
					atomSet.Add(ai);
				}

				//posMap += ai.ToString() + ", " + a.getSymbol() + ", " + hilight + "\r\n"; // debug 

				int massNo = GetMassNumber(a);
				if (massNo == hilightIsotopeValue)
					a.setMassNumber(null); // set back to original value
				else if (massNo < 0)
				{
					if (clearAttachmentPointLabels)
						a.setMassNumber(null);

					else SetMassNumber(a, -massNo); // set back to positive
				}
			}

			//mol = GenerateCoordinates(mol); // (should already be done)

			try { molfile2 = AtomContainerToMolFileV3000(mol); }
			catch (Exception ex) { ex = ex; }

			if (Lex.IsUndefined(molfile2)) // couldn't convert to v3000, just return unhilighted v2000 file
			{
				molfile2 = AtomContainerToMolFile(mol);
				return molfile2;
			}

			string txt = "MDLV30/HILITE";
			if (atomSet.Count > 0)
			{
				txt += " " + BuildV3000KeywordList("ATOMS", atomSet);
			}

			if (bondSet.Count > 0)
				txt += " " + BuildV3000KeywordList("BONDS", bondSet);

			txt = BuildV3000Lines(txt);

			bool hasCollection = Lex.Contains(molfile2, "M  V30 END COLLECTION");
			if (hasCollection) // already have collection begin and end
			{
				txt = txt +
					"M  V30 END COLLECTION";
				molfile2 = Lex.Replace(molfile2, "M  V30 END COLLECTION", txt);
			}

			else // add collection begin & end
			{
				txt =
					"M  V30 BEGIN COLLECTION\n" +
					txt +
					"M  V30 END COLLECTION\n" +
					"M  V30 END CTAB";
				molfile2 = Lex.Replace(molfile2, "M  V30 END CTAB", txt);
			}

			return molfile2;
		}

		/// <summary>
		/// Build string containing keyWord and list of values
		/// </summary>
		/// <param name="keyWord"></param>
		/// <param name="set"></param>
		/// <returns></returns>

		public static string BuildV3000KeywordList(
			string keyWord,
			List<int> list)
		{
			list.Sort();

			string txt = keyWord + "=(" + list.Count; // list name and item count

			for (int li = 0; li < list.Count; li++)
			{
				txt += " " + (list[li] + 1); // add atom or bond number incrementing by 1
			}
			txt += ")";

			return txt;
		}

		/// <summary>
		/// Break up the text of an entry with proper V3000 line prefix and line length
		/// </summary>
		/// <param name="txt"></param>
		/// <returns></returns>

		public static string BuildV3000Lines(
			string txt)
		{
			int ci;

			string txt2 = "";
			while (txt.Length > 0)
			{
				for (ci = 0; ci < txt.Length; ci++)
				{
					if (txt[ci] == ' ' && ci > 60) break; // conservatively break line at length 60 (must be less than 80)
				}

				if (ci >= txt.Length) ci--; // at end

				txt2 += "M  V30 " + txt.Substring(0, ci + 1);
				txt = txt.Substring(ci + 1);
				if (txt.Length > 0) txt2 += "-\n"; // more to come
				else txt2 += "\n"; // all done
			}

			return txt2;
		}

		/// <summary>
		/// Generate coordinates for a molecule
		/// </summary>
		/// <param name="mol"></param>
		/// <returns></returns>
		public static IAtomContainer GenerateCoordinates(
		IAtomContainer mol)
		{
			StructureDiagramGenerator sdg = new StructureDiagramGenerator();
			sdg.setMolecule(mol);
			sdg.generateCoordinates();
			IAtomContainer mol2 = sdg.getMolecule();
			return mol2;
		}

		static bool JoinMmpFragments(
			IAtomContainer mol,
			string commonFragSmiles,
			int attachmentNo)
		{
			IAtomContainer cfMol = SmilesToAtomContainer(commonFragSmiles);
			//AtomContainerManipulator.removeHydrogens(cfMol);

			mol.add(cfMol);

			List<IAtom> atoms = GetAttachmentAtoms(mol, attachmentNo);
			if (atoms.Count != 2) return false;

			Bond b = new Bond(atoms[0], atoms[1], IBond.Order.SINGLE);
			mol.addBond(b);

			return true;
		}

		static List<IAtom> GetAttachmentAtoms(
			IAtomContainer mol,
			int attachmentNo)
		{
			List<IAtom> atoms = new List<IAtom>();
			for (int ai = 0; ai < mol.getAtomCount(); ai++)
			{
				IAtom atom = mol.getAtom(ai);
				int massNo = Math.Abs(GetMassNumber(atom));
				if (massNo == attachmentNo || massNo == 12 || massNo == 21) // 12 & 21 values occur at atoms with two attachments
				{
					//SetMassNumber(atom, 0); // clear attachment number (isotope) (can't may need later, e.g.12 value for two attachements to same atom)
					atoms.Add(atom);
				}
			}

			return atoms;
		}


		/// <summary>
		/// (Converted from Java to C# for possible later modification)
		/// Suppress any explicit hydrogens in the provided container. Only hydrogens
		/// that can be represented as a hydrogen count value on the atom are
		/// suppressed. The container is updated and no elements are copied, please
		/// use either <seealso cref="#copyAndSuppressedHydrogens"/> if you would to preserve
		/// the old instance.
		/// </summary>
		/// <param name="org"> the container from which to remove hydrogens </param>
		/// <returns> the input for convenience </returns>
		/// <seealso cref= #copyAndSuppressedHydrogens </seealso>
		public static IAtomContainer suppressHydrogens(IAtomContainer org)
		{
			bool anyHydrogenPresent = false;
			for (int ai = 0; ai < org.getAtomCount(); ai++)
			{
				IAtom atom = org.getAtom(ai);
				if ("H".Equals(atom.getSymbol()))
				{
					anyHydrogenPresent = true;
					break;
				}
			}

			if (!anyHydrogenPresent)
			{
				return org;
			}

			// we need fast adjacency checks (to check for suppression and
			// update hydrogen counts)
			int[][] graph = GraphUtil.toAdjList(org);

			int nOrgAtoms = org.getAtomCount();
			int nOrgBonds = org.getBondCount();

			int nCpyAtoms = 0;
			int nCpyBonds = 0;

			ISet<IAtom> hydrogens = new HashSet<IAtom>();
			IAtom[] cpyAtoms = new IAtom[nOrgAtoms];

			// filter the original container atoms for those that can/can't
			// be suppressed
			for (int v = 0; v < nOrgAtoms; v++)
			{
				IAtom atom = org.getAtom(v);
				if (suppressibleHydrogen(org, graph, v))
				{
					hydrogens.Add(atom);
					incrementImplHydrogenCount(org.getAtom(graph[v][0]));
				}
				else
				{
					cpyAtoms[nCpyAtoms++] = atom;
				}
			}

			// none of the hydrogens could be suppressed - no changes need to be made
			if (hydrogens.Count == 0)
			{
				return org;
			}

			org.setAtoms(cpyAtoms);

			// we now update the bonds - we have auxiliary variable remaining that
			// bypasses the set membership checks if all suppressed bonds are found
			IBond[] cpyBonds = new IBond[nOrgBonds - hydrogens.Count];
			int remaining = hydrogens.Count;

			for (int bi = 0; bi < org.getBondCount(); bi++)
			{
				IBond bond = org.getBond(bi);
				if (remaining > 0 && (hydrogens.Contains(bond.getAtom(0)) || hydrogens.Contains(bond.getAtom(1))))
				{
					remaining--;
					continue;
				}
				cpyBonds[nCpyBonds++] = bond;
			}

			// we know how many hydrogens we removed and we should have removed the
			// same number of bonds otherwise the container is strange
			if (nCpyBonds != cpyBonds.Length)
			{
				throw new System.ArgumentException("number of removed bonds was less than the number of removed hydrogens");
			}

			org.setBonds(cpyBonds);

			return org;
		}

		/// <summary>
		/// Is the {@code atom} a suppressible hydrogen and can be represented as
		/// implicit. A hydrogen is suppressible if it is not an ion, not the major
		/// isotope (i.e. it is a deuterium or tritium atom) and is not molecular
		/// hydrogen.
		/// </summary>
		/// <param name="container"> the structure </param>
		/// <param name="atom">      an atom in the structure </param>
		/// <returns> the atom is a hydrogen and it can be suppressed (implicit) </returns>
		private static bool suppressibleHydrogen(IAtomContainer container, IAtom atom)
		{
			// is the atom a hydrogen
			if (!"H".Equals(atom.getSymbol()))
			{
				return false;
			}
			// is the hydrogen an ion?
			if (GetFormalCharge(atom) != 0)
			{
				return false;
			}
			// is the hydrogen deuterium / tritium?
			if (GetMassNumber(atom) != 1)
			{
				return false;
			}
			// molecule hydrogen with implicit H?
			if (atom.getImplicitHydrogenCount() != null && atom.getImplicitHydrogenCount().intValue() != 0)
			{
				return false;
			}
			// molecule hydrogen
			List<IAtom> neighbors = container.getConnectedAtomsList(atom) as List<IAtom>;
			if (neighbors.Count == 1 && neighbors[0].getSymbol().Equals("H"))
			{
				return false;
			}
			// what about bridging hydrogens?
			// hydrogens with atom-atom mapping?
			return true;
		}

		/// <summary>
		/// Increment the implicit hydrogen count of the provided atom. If the atom
		/// was a non-pseudo atom and had an unset hydrogen count an exception is
		/// thrown.
		/// </summary>
		/// <param name="atom"> an atom to increment the hydrogen count of </param>
		private static void incrementImplHydrogenCount(IAtom atom)
		{
			int? hCount = atom.getImplicitHydrogenCount().intValue();

			if (hCount == null)
			{
				if (!(atom is IPseudoAtom))
				{
					throw new System.ArgumentException("a non-pseudo atom had an unset hydrogen count");
				}
				hCount = 0;
			}

			atom.setImplicitHydrogenCount(new java.lang.Integer((int)hCount + 1));
		}

		/// <summary>
		/// Is the {@code atom} a suppressible hydrogen and can be represented as
		/// implicit. A hydrogen is suppressible if it is not an ion, not the major
		/// isotope (i.e. it is a deuterium or tritium atom) and is not molecular
		/// hydrogen.
		/// </summary>
		/// <param name="container"> the structure </param>
		/// <param name="graph">     adjacent list representation </param>
		/// <param name="v">         vertex (atom index) </param>
		/// <returns> the atom is a hydrogen and it can be suppressed (implicit) </returns>
		private static bool suppressibleHydrogen(IAtomContainer container, int[][] graph, int v)
		{

			IAtom atom = container.getAtom(v);

			// is the atom a hydrogen
			if (!"H".Equals(atom.getSymbol()))
			{
				return false;
			}
			// is the hydrogen an ion?
			if (GetFormalCharge(atom) != 0)
			{
				return false;
			}
			// is the hydrogen deuterium / tritium?
			if (GetMassNumber(atom) != 1)
			{
				return false;
			}
			// hydrogen is either not attached to 0 or 2 neighbors
			if (graph[v].Length != 1)
			{
				return false;
			}

			// okay the hydrogen has one neighbor, if that neighbor is not a
			// hydrogen (i.e. molecular hydrogen) then we can suppress it
			return !"H".Equals(container.getAtom(graph[v][0]).getSymbol());
		}

		/// <summary>
		/// Finds an neighbor connected to 'atom' which is not 'exclude1'
		/// or 'exclude2'. If no neighbor exists - null is returned.
		/// </summary>
		/// <param name="container"> structure </param>
		/// <param name="atom">      atom to find a neighbor of </param>
		/// <param name="exclude1">  the neighbor should not be this atom </param>
		/// <param name="exclude2">  the neighbor should also not be this atom </param>
		/// <returns> a neighbor of 'atom', null if not found </returns>
		private static IAtom findOther(IAtomContainer container, IAtom atom, IAtom exclude1, IAtom exclude2)
		{
			foreach (IAtom neighbor in container.getConnectedAtomsList(atom) as List<IAtom>)
			{
				if (neighbor != exclude1 && neighbor != exclude2)
				{
					return neighbor;
				}
			}
			return null;
		}

		/// <summary>
		/// Convert java.lang.Integer to C# int
		/// </summary>
		/// <param name="ji"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public static int ji2i(
			java.lang.Integer ji,
			int defaultValue = 0)
		{
			if (ji == null) return defaultValue;
			else return ji.intValue();
		}


		/// <summary>
		/// Convert java.lang.Integer to C# int
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public static java.lang.Integer i2ji(
			int i)
		{
			return new java.lang.Integer(i);
		}

		/////////////////////////////////////////
		// public interface ICdkUtil methods
		/////////////////////////////////////////
		/// <summary>
		/// Convert an Inchi string to a molfile
		/// </summary>
		/// <param name="inchiString"></param>
		/// <returns></returns>

		public string InChIStringToMolfileString(string inchiString)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Build a CDK BitSetFingerprint from a molfile
		/// </summary>
		/// <param name="molfile"></param>
		/// <returns></returns>

		public object BuildBitSetFingerprint(
			string molfile,
			int fpTypeInt,
			int fpSubtype = -1,
			int fpLen = -1)
		{
			FingerprintType fpType = FingerprintType.Undefined;

			IAtomContainer mol = MolfileToAtomContainer(molfile);

			if (fpTypeInt > 0 && Enum.IsDefined(typeof(FingerprintType), fpTypeInt))
				fpType = (FingerprintType)fpTypeInt;

			else throw new Exception("Invalid FingerprintType: " + fpTypeInt);

			BitSetFingerprint bfp = BuildBitSetFingerprintForLargestFragment(
				mol,
				fpType,
				fpSubtype,
				fpLen);

			return bfp;
		}

		/// <summary>
		/// Build CDK BitSetFingerprints from an AtomContainer 
		/// including the overall fingerprint and a fingerprint for each fragment
		/// </summary>
		/// <param name="mol"></param>
		/// <param name="fpTypeInt"></param>
		/// <param name="fpSubtype"></param>
		/// <param name="fpLen"></param>
		/// <returns></returns>

		public static List<BitSetFingerprint> BuildBitSetFingerprints(
			IAtomContainer mol,
			bool includeOverallFingerprint,
			FingerprintType fpType,
			int fpSubtype = -1,
			int fpLen = -1)
		{
			IAtomContainer mol2;
			BitSetFingerprint fp;
			string smiles;
			int fi, fi2;

			List<BitSetFingerprint> fpList = new List<BitSetFingerprint>();

			int pseudoCount = RemovePseudoAtoms(mol);

			//mol = AtomContainerToSmilesAndBack(mol, out smiles); // need this for kekulization?

			mol = RemoveIsotopesStereoExplicitHydrogens(mol); // additional normalization for fingerprint

			List<IAtomContainer> frags = FragmentMolecule(mol, true); // get fragments filtering out small and common frags

			if (includeOverallFingerprint && frags.Count > 1) // include overall fingerprint for multiple fragments
			{
				fp = // generate a fingerprint
					CdkFingerprint.BuildBitSetFingerprint(mol, fpType, fpSubtype, fpLen);

				fpList.Add(fp);
			}

			for (fi = 0; fi < frags.Count; fi++)
			{
				mol2 = frags[fi];

				fp = CdkFingerprint.BuildBitSetFingerprint(
					mol2,
					fpType,
					fpSubtype,
					fpLen);

				for (fi2 = 0; fi2 < fpList.Count; fi2++) // see if a dup fp
				{
					if (fp.equals(fpList[fi2])) break;
				}
				if (fi2 < fpList.Count) continue; // skip if dup

				fpList.Add(fp);
			}

			return fpList;
		}

		/// <summary>
		/// Build a CDK BitSetFingerprint for the largest fragment in an AtomContainer
		/// </summary>
		/// <param name="mol"></param>
		/// <param name="fpTypeInt"></param>
		/// <param name="fpSubtype"></param>
		/// <param name="fpLen"></param>
		/// <returns></returns>

		public static BitSetFingerprint BuildBitSetFingerprintForLargestFragment(
			IAtomContainer mol,
			FingerprintType fpType,
			int fpSubtype = -1,
			int fpLen = -1)
		{
			string smiles;

			mol = GetLargestMoleculeFragment(mol);

			int pseudoCount = RemovePseudoAtoms(mol);

			//mol = AtomContainerToSmilesAndBack(mol, out smiles); // need this for kekulization

			mol = RemoveIsotopesStereoExplicitHydrogens(mol); // additional normalization for fingerprint

			BitSetFingerprint bfp = CdkFingerprint.BuildBitSetFingerprint(
				mol,
				fpType,
				fpSubtype,
				fpLen);

			return bfp;
		}

		/// <summary>
		/// Calculate similarity of two Bitsetfingerprints
		/// </summary>
		/// <param name="queryFingerprint"></param>
		/// <param name="targetFingerprint"></param>
		/// <returns></returns>

		public double CalculateBitSetFingerprintSimilarity(object queryFingerprint, object targetFingerprint)
		{
			BitSetFingerprint qfp = queryFingerprint as BitSetFingerprint;
			if (qfp == null) throw new Exception("Query Fingerprint is not a defined CdkFingerprint");

			BitSetFingerprint tfp = targetFingerprint as BitSetFingerprint;
			if (tfp == null) throw new Exception("Target Fingerprint is not a defined CdkFingerprint");

			int qCard = qfp.cardinality();
			int tCard = tfp.cardinality();

			BitSetFingerprint tfp2 = new BitSetFingerprint(tfp); // must make copy of target that can be modified
			tfp2.and(qfp); // and target copy and query into target copy
			int commonCnt = tfp2.cardinality();

			float simScore = commonCnt / (float)(tCard + qCard - commonCnt);

			if (qfp.cardinality() != qCard || tfp.cardinality() != tCard) // debug
				throw new Exception("Cardinality changed");

			return simScore;
		}

		/// <summary>
		/// Get array of set bits
		/// </summary>
		/// <param name="fingerprint"></param>
		/// <returns></returns>

		public int[] GetBitSet(object fingerprint)
		{
			BitSetFingerprint fp = fingerprint as BitSetFingerprint;
			if (fp == null) throw new Exception("Fingerprint is not a defined CdkFingerprint");

			int [] setBits = fp.getSetbits();

			return setBits;
		}

		/// <summary>
		/// Convert special atom isotope values to MDL V3000-type hilighting
		/// Add hilight collection to V3000 mol file resulting in a molfile 
		/// with a form like the following:
		///
		///M  V30 END BOND
		///M  V30 BEGIN COLLECTION
		///M  V30 MDLV30 / STEABS ATOMS = (1 4)
		///M  V30 MDLV30 / HILITE ATOMS = (13 10 11 12 34 35 36 37 38 39 40 41 42 43) -
		///M  V30 BONDS = (13 11 12 37 38 39 40 41 42 43 44 45 46 47)
		///M  V30 END COLLECTION
		///M  V30 END CTAB
		///M  END
		/// </summary>
		/// <param name="molfile"></param>
		/// <param name="hilightIsotopeValue"></param>
		/// <returns></returns>
		/// 

		// Note that isotope values are handled in different ways based on the form (molfile vs internal mol object)
		// and the software doing the conversion. 
		// For CDK, the molfile value and the internal object value are the same value, i.e. the total mass (number of protons & neutrons)
		// For MolLib1 the internal value is the normal current mass minus the normal mass. e.g. a carbon with mass = 1 will have an atom.Isotope value of -15;

		public static string ConvertIsotopeValuesToHilighting(
			string molfile)
		{
			bool hilight;
			int hilightIsotopeValue = 100;
			string molfile2 = "";
			bool clearAttachmentPointLabels = true;
			bool hilightAttachmentBond = true;

			throw new NotImplementedException();
		}

		/// <summary>
		/// GetNormalizedIsotope
		/// The isotope value is difference from the normal mass, e.g. Carbon 14 has a value of 2.
		/// This method (which is a bit of a hack) returns the the mass unless it is normal in which case it returns 0.
		/// </summary>
		/// <param name="a"></param>
		/// <returns></returns>
		static int GetNormalizedIsotope(
			object a)
		{
			throw new NotImplementedException();
		}


		/// <summary>
		/// Cdk Isotope Conversion Test
		/// </summary>
		/// <param name="smiles"></param>
		/// <returns></returns>

		public static string CdkIsotopeConversionTest(
			string smiles)
		{
			// Initial mol object creation from Smiles

			string msg =
				"CDK Conversion Test, Smiles: " + smiles + "\r\n\r\n";

			IAtomContainer mol1 = SmilesToAtomContainer(smiles);
			mol1.getAtom(0).setMassNumber(new java.lang.Integer(-1)); // try neg value
			int isotope1 = mol1.getAtom(0).getMassNumber().intValue();
			msg += "Smiles -> Obj isotope: " + isotope1 + "\r\n\r\n";

			// Mol object to V2000 & back

			string v2000 = AtomContainerToMolFile(mol1); // obj to molfile
			msg += "Obj -> CDK V2000\r\n==================================\r\n " + v2000 + "\r\n\r\n";

			IAtomContainer molV2000 = MolfileToAtomContainer(v2000); // molfile to obj
			int isotopeV2 = molV2000.getAtom(0).getMassNumber().intValue();
			msg += "V2000 -> Obj isotope: " + isotopeV2 + "\r\n\r\n";
			string v2000b = AtomContainerToMolFile(molV2000); // obj back to molfile

			// Mol object to V3000 & back

			string v3000 = AtomContainerToMolFileV3000(mol1);  // obj to molfile
			msg += "Obj -> CDK V3000\r\n==================================\r\n " + v3000 + "\r\n\r\n";

			IAtomContainer molV3000 = MolfileToAtomContainer(v3000); // molfile to obj
			int isotopeV3 = mol1.getAtom(0).getMassNumber().intValue();
			msg += "V3000 -> Obj isotope: " + isotopeV3 + "\r\n\r\n";
			string v3000b = AtomContainerToMolFileV3000(molV3000); // obj back to molfile

			return msg;
		}

		/// <summary>
		/// Returns standard mass for specified element no, as integer
		/// </summary>
		/// <param name="atomicNumber"></param>
		/// <returns></returns>

		public static int GetMajorIsotopeMassNumberFromTable(int atomicNumber)
		{
			if (atomicNumber < 1 || atomicNumber >= MajorIsotopeMassNumber.Length)
				throw new Exception("Invalid atomic number: " + atomicNumber);

			int mass = MajorIsotopeMassNumber[atomicNumber];
			return mass;
		}

		/// <summary>
		/// Table of major isotope mass numbers indexed by atomic number
		/// </summary>

		public static int[] MajorIsotopeMassNumber = {
			/* 0 */ 0,
			/* 1 */ 1,
			/* 2 */ 4,
			/* 3 */ 7,
			/* 4 */ 9,
			/* 5 */ 11,
			/* 6 */ 12,
			/* 7 */ 14,
			/* 8 */ 16,
			/* 9 */ 19,
			/* 10 */ 20,
			/* 11 */ 23,
			/* 12 */ 24,
			/* 13 */ 27,
			/* 14 */ 28,
			/* 15 */ 31,
			/* 16 */ 32,
			/* 17 */ 35,
			/* 18 */ 40,
			/* 19 */ 39,
			/* 20 */ 40,
			/* 21 */ 45,
			/* 22 */ 48,
			/* 23 */ 51,
			/* 24 */ 52,
			/* 25 */ 55,
			/* 26 */ 56,
			/* 27 */ 59,
			/* 28 */ 58,
			/* 29 */ 63,
			/* 30 */ 64,
			/* 31 */ 69,
			/* 32 */ 74,
			/* 33 */ 75,
			/* 34 */ 80,
			/* 35 */ 79,
			/* 36 */ 84,
			/* 37 */ 85,
			/* 38 */ 88,
			/* 39 */ 89,
			/* 40 */ 90,
			/* 41 */ 93,
			/* 42 */ 98,
			/* 43 */ 85,
			/* 44 */ 102,
			/* 45 */ 103,
			/* 46 */ 106,
			/* 47 */ 107,
			/* 48 */ 114,
			/* 49 */ 115,
			/* 50 */ 120,
			/* 51 */ 121,
			/* 52 */ 130,
			/* 53 */ 127,
			/* 54 */ 132,
			/* 55 */ 133,
			/* 56 */ 138,
			/* 57 */ 139,
			/* 58 */ 140,
			/* 59 */ 141,
			/* 60 */ 142,
			/* 61 */ 126,
			/* 62 */ 152,
			/* 63 */ 153,
			/* 64 */ 158,
			/* 65 */ 159,
			/* 66 */ 164,
			/* 67 */ 165,
			/* 68 */ 166,
			/* 69 */ 169,
			/* 70 */ 174,
			/* 71 */ 175,
			/* 72 */ 180,
			/* 73 */ 181,
			/* 74 */ 184,
			/* 75 */ 187,
			/* 76 */ 192,
			/* 77 */ 193,
			/* 78 */ 195,
			/* 79 */ 197,
			/* 80 */ 202,
			/* 81 */ 205,
			/* 82 */ 208,
			/* 83 */ 209,
			/* 84 */ 188,
			/* 85 */ 193,
			/* 86 */ 195,
			/* 87 */ 199,
			/* 88 */ 202,
			/* 89 */ 206,
			/* 90 */ 232,
			/* 91 */ 231,
			/* 92 */ 238,
			/* 93 */ 225,
			/* 94 */ 228,
			/* 95 */ 231,
			/* 96 */ 233,
			/* 97 */ 235,
			/* 98 */ 237,
			/* 99 */ 240,
			/* 100 */ 242,
			/* 101 */ 245,
			/* 102 */ 248,
			/* 103 */ 251 };



	} // CdkUtil


}
