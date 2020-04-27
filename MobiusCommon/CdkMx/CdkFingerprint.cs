using Mobius.Data;
using Mobius.ComOps;

using java.io;
using java.util;

using cdk = org.openscience.cdk;
using org.openscience.cdk;
using org.openscience.cdk.inchi;
using org.openscience.cdk.interfaces;
using org.openscience.cdk.fingerprint;
using org.openscience.cdk.tools.manipulator;
using org.openscience.cdk.graph;
using org.openscience.cdk.qsar.result;
using org.openscience.cdk.io;
using org.openscience.cdk.io.iterator;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Mobius.CdkMx
{

	/// <summary>
	/// Generate CDK Fingerprints 
	/// </summary>

	public class CdkFingerprint
	{
		CdkUtil CdkUtil = new CdkUtil();

		static MACCSFingerprinter MACCSFp = null;

		/// <summary>
		/// BuildByteArrayFingerprint
		/// </summary>
		/// <param name="mol"></param>
		/// <param name="fpType"></param>
		/// <param name="fpSubtype"></param>
		/// <param name="fpLen">Length in bytes</param>
		/// <param name="zeroPad">Zero-pad the byte array on the right to full length</param>
		/// <returns></returns>
		public static byte[] BuildByteArrayFingerprint(
			IAtomContainer mol,
			FingerprintType fpType,
			int fpSubtype,
			int fpLen,
			bool zeroPad)
		{
			BitSetFingerprint bsfp = BuildBitSetFingerprint(mol, fpType, fpSubtype, fpLen);
			long size = bsfp.size();
			int card = bsfp.cardinality();

			BitSet bs = bsfp.asBitSet();
			byte[] ba = bs.toByteArray(); // no trailing zero bytes

			if (zeroPad)
			{
				byte[] ba2 = new byte[size / 8]; // alloc full size
				Array.Copy(ba, ba2, ba.Length); // and copy truncated to full size
				ba = ba2;
			}

			return ba;
		}

		/// <summary>
		/// Build fingerprint
		/// </summary>
		/// <param name="mol"></param>

		public static BitSetFingerprint BuildBitSetFingerprint(
			IAtomContainer mol,
			FingerprintType fpType,
			int fpSubtype = -1,
			int fpLen = -1)
		{

			// Data for Tanimoto similarity using various fingerprint types for CorpId 123456 query.
			// Cart - Standard MDL Oracle Cartridge scores
			//
			//                         Similarity Score        
			//         ------------------------------------------------      
			// Size ->        192    896   1024  1024   128  1024   320
			// CorpId     Cart  MACCS  PbChm  ECFP4 EXT  EState Basic Sbstr
			// ------  ----  ----   ----   ----  ----  ----	 ----  ----
			// 123456  0.99  1.00   1.00   1.00  1.00  1.00  1.00  1.00   
			// 123456  0.99  0.98   0.96 	 0.77  0.95  1.00  0.95  1.00  
			// 123456  0.99  0.98   0.96 	 0.77  0.95  1.00  0.94  1.00  
			// 123456  0.99  1.00   1.00 	 1.00  1.00  1.00  1.00  1.00  
			// 123456  0.99  1.00   1.00 	 1.00  1.00  1.00  1.00  1.00  
			// 123456  0.99  0.91   1.00 	 0.81  1.00  1.00  1.00  1.00  
			// 123456  0.98  0.95   1.00 	 0.74  0.92  1.00  0.93  0.94  
			// 123456  0.98  1.00   1.00 	 1.00  1.00  1.00  1.00  1.00  
			// 123456  0.98  1.00   1.00 	 1.00  1.00  1.00  1.00  1.00  
			// 123456  0.98  1.00   0.83   0.76  0.77  0.90  0.76  0.94  	 


			// LSH Bin Count - The number of LSH bins (of 25) that match the query bin values
			//--------------
			// CorpId     MAC  PbC ECFP EX
			// ------  ---  ---  --- ---
			// 123456   25   25   25  25
			// 123456	  25   20    7  16
			// 123456	  25   20    9  19
			// 123456	  25   25   25  25
			// 123456	  25   25   25  25
			// 123456	  20   25    9  25
			// 123456	  21   25   11  17
			// 123456	  25   25   25  25
			// 123456	  25   25   25  25
			// 123456	  25    9    6  11

			// Data for Tanimoto similarity using various Circular fingerprint types.
			// Using 2 molecules where the 2nd just has an added methyl group.
			//
			//  Measure      Score
			//  --------     -----
			//  ECFP0        1.00
			//  ECFP2         .88
			//  ECFP4         .75
			//  ECFP6         .64
			//  FCFP0        1.00
			//  FCFP2         .92
			//  FCFP4         .84
			//  FCFP6         .74

			IFingerprinter ifptr = null;
			IBitFingerprint ibfp = null;
			BitSetFingerprint bfp = null;
			BitSet bs;
			IAtomContainer mol2;
			string s = "";

			DateTime t0 = DateTime.Now;
			double getFptrTime = 0, buildFpTime = 0;

			if (fpType == FingerprintType.Basic) // size = 1024
			{
				ifptr = new Fingerprinter();
			}

			else if (fpType == FingerprintType.Circular) // size variable
			{

				int cfpClass = fpSubtype;
				if (cfpClass < CircularFingerprinter.CLASS_ECFP0 || cfpClass > CircularFingerprinter.CLASS_ECFP6)
					cfpClass = CircularFingerprintType.DefaultCircularClass; // default class

				if (fpLen < 0) fpLen = CircularFingerprintType.DefaultCircularLength; // default length

				ifptr = new CircularFingerprinter(cfpClass, fpLen);

				//CircularFingerprinter cfp = (CircularFingerprinter)ifptr;
				//ICountFingerprint cntFp = cfp.getCountFingerprint(mol); // debug
				//s = CircularFpToString(cfp); // debug
			}

			else if (fpType == FingerprintType.Extended) // size = 1024
			{
				ifptr = new ExtendedFingerprinter(); // use DEFAULT_SIZE and DEFAULT_SEARCH_DEPTH
			}

			else if (fpType == FingerprintType.EState) // size = 128
			{
				ifptr = new EStateFingerprinter(); // use DEFAULT_SIZE and DEFAULT_SEARCH_DEPTH
			}

			else if (fpType == FingerprintType.MACCS) // size = 192
			{
				if (MACCSFp == null)
					MACCSFp = new MACCSFingerprinter();

				ifptr = MACCSFp;
			}

			else if (fpType == FingerprintType.PubChem) // size = 896
			{
				IChemObjectBuilder builder = DefaultChemObjectBuilder.getInstance();
				ifptr = new PubchemFingerprinter(builder);
			}

			else if (fpType == FingerprintType.ShortestPath) // size =
			{
				ifptr = new ShortestPathFingerprinter(); // fails with atom type issue for many structures (e.g. 123456)
			}

			else if (fpType == FingerprintType.Signature) // size =
			{
				ifptr = new SignatureFingerprinter(); // can't convert array fingerprint to bitsetfingerprint
			}

			else if (fpType == FingerprintType.Substructure) // size = 320
			{
				ifptr = new SubstructureFingerprinter();
			}

			else throw new Exception("Invalid CdkFingerprintType: " + fpType);

			getFptrTime = TimeOfDay.Delta(ref t0);

			ibfp = ifptr.getBitFingerprint(mol);
			bfp = (BitSetFingerprint)ibfp;

			buildFpTime = TimeOfDay.Delta(ref t0);

			//long size = bfp.size();
			//int card = bfp.cardinality();
			return bfp;
		}

		public static string CircularFpToString(CircularFingerprinter cfp)
		{
			CircularFingerprinter.FP fp = null;

			string s = "fp\thashCode\titeration\tatoms\r\n";

			int fpCount = cfp.getFPCount();
			for (int fpi = 0; fpi < fpCount; fpi++)
			{
				fp = cfp.getFP(fpi);
				s += fpi.ToString() + "\t" + fp.hashCode + "\t" + fp.iteration + "\t(" + string.Join(", ", fp.atoms) + ")\r\n";
			}

			return s;
		}

		/// <summary>
		/// BuildTestFp
		/// </summary>
		/// <returns></returns>
		public static bool[] BuildTestFp(int corpId)
		{
			bool[] ba = null;

			string fileName = @"C:\Download\CorpId-12345.mol";
			if (corpId > 0) fileName = fileName.Replace("12345", corpId.ToString());
			string molfile = FileUtil.ReadFile(fileName);
			//string molfile = FileUtil.ReadFile();
			IAtomContainer mol = CdkUtil.MolfileToAtomContainer(molfile);

			//int fpClass = CircularFingerprinter.CLASS_ECFP6; // FP diameter
			//int fpLen = 2048; // folded binary fp length
			//ba = CdkFingerprint.BuildBoolArray(mol, fpClass, fpLen, true);

			return ba;
		}

		/// <summary>
		/// BuildTest
		/// </summary>
		public static void BuildTest()
		{
			CircularFingerprinter cfp = null;
			int FpClass = CircularFingerprinter.CLASS_ECFP6; // FP diameter
			int FpLen = 2048; // folded binary fp length

			DefaultChemObjectReader cor;
			IAtomContainer mol, mol2;

			//string molfile = FileUtil.ReadFile(@"C:\Download\CorpId-12345.mol");
			//java.io.StringReader sr = new java.io.StringReader(molfile);
			//if (Lex.Contains(molfile, "v2000"))
			//  cor = new MDLV2000Reader(sr);
			//else
			//  cor = new MDLV3000Reader(sr);

			//cor.setReaderMode(IChemObjectReader.Mode.RELAXED);

			//ac = (IAtomContainer)cor.read(new AtomContainer()); 

			FpClass = CircularFingerprinter.CLASS_ECFP4; // debug

			cfp = new CircularFingerprinter(FpClass, FpLen);

			FileReader FileReader = new FileReader(@"C:\Download\CorpId-12345.mol");
			//FileReader FileReader = new FileReader(@"C:\Download\V3000 Mols.sdf");

			IteratingSDFReader rdr = new IteratingSDFReader(FileReader, DefaultChemObjectBuilder.getInstance());
			rdr.setReaderMode(IChemObjectReader.Mode.RELAXED);

			while (rdr.hasNext())
			{
				mol = (IAtomContainer)rdr.next();

				mol = CdkUtil.GetLargestMoleculeFragment(mol);

				ICountFingerprint cfp1 = cfp.getCountFingerprint(mol); // get hash values and counts for each

				cfp.calculate(mol);
				int fpCount = cfp.getFPCount();
				for (int fpi = 0; fpi < fpCount; fpi++) // gets 
				{
					CircularFingerprinter.FP cfp2 = cfp.getFP(fpi); // gets hash, iteration and lists of atoms (dups appear multiple times)
				}

				IBitFingerprint bfp = cfp.getBitFingerprint(mol);
				java.util.BitSet bs = bfp.asBitSet();
				int bsCard = bfp.cardinality();
				long bsSize = bfp.size();
				continue;
			}

			FileReader.close();

			return;


			//java.io.StringReader sr = new java.io.StringReader(molfile);
			//AtomContainer mol = new AtomContainer();

			//mol.addAtom(new Atom("C"));
			//mol.addAtom(new Atom("H"));
			//mol.addAtom(new Atom("H"));
			//mol.addAtom(new Atom("H"));
			//mol.addAtom(new Atom("H"));
			//mol.addBond(new Bond(mol.getAtom(0), mol.getAtom(1)));
			//mol.addBond(new Bond(mol.getAtom(0), mol.getAtom(2)));
			//mol.addBond(new Bond(mol.getAtom(0), mol.getAtom(3)));
			//mol.addBond(new Bond(mol.getAtom(0), mol.getAtom(4)));

			//FileReader FileReader = new FileReader(@"C:\Download\CorpId-12345.mol");
			//MolReader mr = new MolReader(FileReader, DefaultChemObjectBuilder.getInstance());
			//java.io.StringReader sr = new java.io.StringReader(molfile);
			//IMol m = (IMol)mr.next();
			//FileReader.close(); 
		}
	}
}
