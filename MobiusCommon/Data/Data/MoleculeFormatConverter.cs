using Mobius.ComOps;
using Mobius.MolLib1;
using Mobius.MolLib2;

using System;
using System.IO;
using System.Data;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.ServiceModel;
using System.Diagnostics;

namespace Mobius.Data
{

	/// <summary>
	/// MoleculeMx molecule format converersion methods
	/// </summary>

	public partial class MoleculeMx : MobiusDataType
	{

		/// <summary>
		/// Try to get molfile form of molecule doing a conversion from another format as necessary
		/// </summary>
		/// <param name="molFormat"></param>
		/// <param name="molString"></param>
		/// <returns></returns>

		public bool TryGetMoleculeString(
			MoleculeFormat molFormat,
			out string molString)
		{
			try
			{
				molString = GetMoleculeString(molFormat);
				return true;
			}

			catch (Exception ex)
			{
				molString = null;
				return false;
			}
		}

		/// <summary>
		/// Get the molecule string of the specified type
		/// Do a conversion from an existing type if specified type not yet detined for the molecule
		/// </summary>
		/// <param name="newType"></param>
		/// <returns></returns>

		public string GetMoleculeString(
			MoleculeFormat newType)
		{

			try
			{
				switch (newType)
				{
					case MoleculeFormat.Molfile:
						return GetMolfileString();

					case MoleculeFormat.Chime:
						return GetChimeString();

					case MoleculeFormat.Smiles:
						return GetSmilesString();

					case MoleculeFormat.Helm:
						return GetHelmString();

					case MoleculeFormat.Sequence:
						return GetSequenceString();

					case MoleculeFormat.Fasta:
						return GetFastaString();

					default: throw new Exception("Can't convert type: " + this.PrimaryFormat);

				}
			}

			catch (Exception ex) // handle structures that don't convert - issue #216
			{
				throw new Exception(ex.Message, ex);
			}
		}


		/// <summary>
		/// Try to get molfile form of molecule doing a conversion from another format as necessary
		/// </summary>
		/// <param name="molfile"></param>
		/// <returns></returns>

		public bool TryGetMolfileString(out string molfile)
		{
			try
			{
				molfile = GetMolfileString();
				return true;
			}

			catch (Exception ex)
			{
				molfile = null;
				return false;
			}
		}

			/// <summary>
			/// Get molfile form of molecule doing a conversion from another format as necessary
			/// </summary>
			/// <returns></returns>

			public string GetMolfileString()
		{
			string molfile = "", helm = "";

			if (Lex.IsDefined(MolfileString))
				return MolfileString;

			else if (Lex.IsDefined(ChimeString))
				molfile = MolLib1.StructureConverter.ChimeStringToMolfileString(ChimeString);

			else if (Lex.IsDefined(SmilesString))
				molfile = MolLib1.StructureConverter.SmilesStringToMolfileString(SmilesString);

			else if (Lex.IsDefined(InchiString))
				molfile = MolLib1.StructureConverter.ICdkUtil.InChIStringToMolfileString(PrimaryValue.ToString());

			else if (Lex.IsDefined(HelmString))
			{
				molfile = HelmConverter.HelmToMolfile(this, HelmString);
			}

			else if (Lex.IsDefined(SequenceString)) 
			{
				helm = HelmConverter.SequenceToHelm(SequenceString);
				HelmString = helm;
				if (Lex.IsDefined(helm))
					molfile = HelmConverter.HelmToMolfile(this, HelmString);
			}

			else if (Lex.IsDefined(FastaString))
			{
				helm = HelmConverter.FastaToHelm(FastaString);
				HelmString = helm;
				if (Lex.IsDefined(helm))
					molfile = HelmConverter.HelmToMolfile(this, HelmString);
			}

			else molfile = "";

			MolfileString = molfile; // save for later possible reuse
			return MolfileString;
		}

		/// <summary>
		/// Get chime form of molecule doing conversion if necessary
		/// </summary>
		/// <returns></returns>

		public string GetChimeString()
		{
			string chime = "", molfile = "";

			if (Lex.IsDefined(ChimeString))
				return ChimeString;

			molfile = GetMolfileString();
			chime = MolLib1.StructureConverter.MolfileStringToChimeString(molfile);

			ChimeString = chime;
			return chime;
		}

		/// <summary>
		/// Get smiles form of molecule doing conversion if necessary
		/// </summary>
		/// <returns></returns>

		public string GetSmilesString()
		{
			string smiles = "", molfile = "";

			if (Lex.IsDefined(SmilesString))
				return SmilesString;

			molfile = GetMolfileString();
			smiles = MolLib1.StructureConverter.MolfileStringToSmilesString(molfile);

			SmilesString = smiles;
			return smiles;
		}

		/// <summary>
		/// Get HELM form of molecule doing conversion if necessary
		/// </summary>

		public string GetHelmString()
		{
			string helm = "", molfile = "";

			if (Lex.IsDefined(HelmString))
				return HelmString;

			if (Lex.IsDefined(SequenceString))
			{
				helm = HelmConverter.SequenceToHelm(SequenceString);
			}

			else if (Lex.IsDefined(FastaString))
			{
				helm = HelmConverter.FastaToHelm(FastaString);
			}

			else 
			{
				molfile = GetMolfileString();
				if (Lex.IsDefined(molfile))
				{
					MolfileString = molfile;
					helm = HelmConverter.MolfileToHelm(molfile);
				}
			}

			HelmString = helm; // save in case needed later
			return HelmString;
		}

		/// <summary>
		/// Get Peptide/RNA sequence form of molecule doing conversion if necessary
		/// </summary>
		/// <returns></returns>

		public string GetSequenceString()
		{
			string seq = "", helm = "";

			if (Lex.IsDefined(SequenceString))
				return SequenceString;

			if (Lex.IsDefined(HelmString))
			{
				seq = HelmConverter.HelmToAnalogSequence(HelmString);
			}

			else if (Lex.IsDefined(FastaString))
			{
				helm = HelmConverter.FastaToHelm(FastaString);
				if (Lex.IsDefined(helm))
				{
					HelmString = helm;
					seq = HelmConverter.HelmToAnalogSequence(HelmString);
				}
			}

			SequenceString = seq;
			return SequenceString;
		}

		/// <summary>
		/// Get Peptide/RNA FASTA form of molecule doing conversion if necessary
		/// </summary>
		/// <returns></returns>

		public string GetFastaString()
		{
			string fasta = "", helm = "";

			if (Lex.IsDefined(FastaString))
				return FastaString;

			else if (Lex.IsDefined(HelmString))
			{
				fasta = HelmConverter.HelmToFastaString(HelmString);
			}

			else if (Lex.IsDefined(SequenceString))
			{
				helm = HelmConverter.SequenceToHelm(SequenceString);
				if (Lex.IsDefined(helm))
				{
					HelmString = helm;
					fasta = HelmConverter.HelmToFastaString(helm);
				}
			}

			FastaString = fasta;
			return FastaString;
		}

		public static void SetMoleculeToTestHelmString(
			string cid, 
			MoleculeMx mol)
		{
			if (!Lex.IsInteger(cid)) return;
			int i1 = int.Parse(cid);
			if (i1 > 0 && i1 < 10)
			{
				string helm = GetTestHelmString(i1);
				mol.SetPrimaryTypeAndValue(MoleculeFormat.Helm, helm);
			}
		}

		/// <summary>
		/// Get a random HELM string for use in dev work
		/// </summary>
		/// <returns></returns>

		static string GetTestHelmString(int cid)
		{
			if (DebugMx.False) return helms[0];

			int idx = cid % 10;
			return helms[idx];

			//helmIdx++;
			//if (helmIdx >= 10) helmIdx = 0;

			//return helms[helmIdx];
		}

		static int helmIdx = -1;
		static string[] helms = new string[] {
 "PEPTIDE1{A}$$$$", // minimal size
 "RNA1{R(C)P.RP.R(A)P.RP.R(A)P.R(U)P}$RNA1,RNA1,4:R3-9:R3$$$",	// branch cyclic RNA
 "RNA1{R(C)P.RP.R(A)P.RP.R(A)P.R(U)P}$RNA1,RNA1,1:R1-16:R2$$$", // backbone cyclic RNA
 "RNA1{R(A)P.R(G)P.R(A)P.R(A)P.R(A)P.R(A)P.R(A)P.R(A)P.R(A)P.R(A)P.R(A)P.R(A)P}$$$$", // simple RNA
 "RNA1{R(A)P.R(A)P}|CHEM1{[*]OCCOCCOCCO[*] |$_R1;;;;;;;;;;;_R3$|}$RNA1,CHEM1,6:R2-1:R1$$$", // conjugate with adhoc chem modifier
 "RNA1{R(U)P.R(T)P.R(G)P.R(C)P.R(A)}$$$$",
 "RNA1{R(G)P.R(G)P.R(C)P.R(A)P.R(C)P.R(U)P.R(U)P.R(C)P.R(G)P.R(G)P.R(U)P.R(G)P.R(C)P.R(C)}$$$$",
 "RNA1{R(A)P.R(C)P.R(T)}|RNA2{R(A)P.R(G)P.R(T)}$RNA1,RNA2,2:pair-8:pair$$$V2.0",
 "RNA1{R(T)P.RP.R(T)P}$$$$V2.0",
 "RNA1{R(A)P.R(G)P.R(C)P.R(U)P.R(C)P.R(C)P.R(C)}|RNA2{R(U)P.R(G)P.R(G)P.R(G)P.R(G)P.R(A)P.R(G)}$RNA1,RNA2,20:pair-8:pair|RNA1,RNA2,17:pair-11:pair|RNA1,RNA2,8:pair-20:pair|RNA1,RNA2,14:pair-14:pair$$$V2.0",
 //"RNA3{R(A)P.R(U)P.R(U)P.R(C)P.R(G)P.R(C)P}|RNA2{R(A)P.R(U)P.R(U)P.R(C)P.R(C)P}|RNA1{R(A)P.R(U)P.R(U)P.R(C)P.R(C)P}$$RNA1,RNA1,2:pair-5:pair|RNA2,RNA2,2:pair-8:pair|RNA3,RNA3,11:pair-14:pair$$", // siRNA with base pair
 //"RNA1{R(A)P.R(A)P}|CHEM1{PEG2}$RNA1,CHEM1,6:R2-1:R1$$$", // conjugate
 //"RNA1{[MOE](C)[sP].[MOE](G)[sP].[MOE](A)[sP].[MOE](U)[sP].[MOE](C)[sP].[MOE](G)[sP].[MOE](A)[sP].[MOE](U)[sP].[MOE](C)[sP].[MOE](G)[sP].[MOE](A)[sP].[MOE](U)[sP].[MOE](C)[sP].[MOE](G)[sP].[MOE](A)[sP].[MOE](U)[sP]}$$$$V2.0",

 "PEPTIDE1{A.L.C}$$$$",
 "PEPTIDE1{A.[meA].C}$$$$",
 "PEPTIDE1{A.C.T.G.C.T.W.G.T.W.E.C.W.C.Q.W}|PEPTIDE2{A.C.T.G.C.T.W.G.T.W.E.Q}$PEPTIDE1,PEPTIDE1,5:R3-14:R3|PEPTIDE2,PEPTIDE1,2:R3-12:R3$$$$",
 "PEPTIDE1{A.A.G.K}$PEPTIDE1,PEPTIDE1,1:R1-4:R2$$$", // backbone cyclic peptide
 "PEPTIDE1{Y.[dA].G.F}$$$$V2.0",
 "PEPTIDE1{A.C.D.E.F}|PEPTIDE2{P.Q.R.S.T}$$$$V2.0",
 "PEPTIDE1{A.C.D.D.E.F.G}\"HC\"|PEPTIDE2{G.C.S.S.S.P.K.K.V.K}\"LC\"$$$$V2.0",
 "PEPTIDE1{C.Y.I.Q.N.C.P.L.G.[am]}$PEPTIDE1,PEPTIDE1,1:R3-6:R3$$$V2.0",
 "PEPTIDE1{(C,[dC]).(Y,[dY]).(F,[dF]).(Q,[dQ]).(N,[dN]).(C,[dC]).(P,[dP]).(K,[dK]).G.[am]}$PEPTIDE1,PEPTIDE1,1:R3-6:R3$$$V2.0",

 "CHEM1{[sDBl]}$$$$",
 "CHEM1{[SMCC]}|CHEM2{[EG]}$CHEM1,CHEM2,1:R1-1:R1$$$$",
 "CHEM1{[Az].[MCC]}$$$$",
 "CHEM1{BMA}|CHEM2{MAN}|CHEM3{MAN}|CHEM4{NAG}|CHEM5{FUL}|CHEM6{NAG}|CHEM7{NAG}|CHEM8{NAG}|CHEM9{FUL}|CHEM10{BMA}|CHEM11{BMA}|CHEM12{NAG}|CHEM13{NAG}|CHEM14{FUL}|CHEM15{NAG}|CHEM16{FUL}|CHEM17{NAG}|CHEM18{MAN}|CHEM19{BMA}|CHEM20{MAN}|CHEM21{MAN}|CHEM22{NAG}|CHEM23{NAG}|CHEM24{FUL}|CHEM25{MAN}|CHEM26{BMA}|CHEM27{NAG}|CHEM28{NAG}|PEPTIDE1{H.M.E.L.A.L.[Ngly].V.T.E.S.F.D.A.W.E.N.T.V.T.E.Q.A.I.E.D.V.W.Q.L.F.E.T.S.I.K.P.C.V.K.L.S.P.L.C.I.G.A.G.H.C.[Ngly].T.S.I.I.Q.E.S.C.D.K.H.Y.W.D.T.I.R.F.R.Y.C.A.P.P.G.Y.A.L.L.R.C.[Ngly].D.T.[Ngly].Y.S.G.F.M.P.K.C.S.K.V.V.V.S.S.C.T.R.M.M.E.T.Q.T.S.T.W.F.G.F.[Ngly].G.T.R.A.E.[Ngly].R.T.Y.I.Y.W.H.G.R.D.[Ngly].R.T.I.I.S.L.N.K.Y.Y.[Ngly].L.T.M.K.C.R.G.A.G.W.C.W.F.G.G.N.W.K.D.A.I.K.E.M.K.Q.T.I.V.K.H.P.R.Y.T.G.T.[Ngly].N.T.D.K.I.[Ngly].L.T.A.P.R.G.G.D.P.E.V.T.F.M.W.T.N.C.R.G.E.F.L.Y.C.K.M.N.W.F.L.N.W.V.E.D.R.D.V.T.N.Q.R.P.K.E.R.H.R.R.N.Y.V.P.C.H.I.R.Q.I.I.N.T.W.H.K.V.G.K.N.V.Y.L.P.P.R.E.G.D.L.T.C.[Ngly].S.T.V.T.S.L.I.A.N.I.D.W.T.D.G.[Ngly].Q.T.[Ngly].I.T.M.S.A.E.V.A.E.L.Y.R.L.E.L.G.D.Y.K.L.V.E.I.T}|CHEM29{NAG}|CHEM30{NAG}|CHEM31{BMA}|CHEM32{MAN}|CHEM33{MAN}|CHEM34{MAN}|CHEM35{NAG}|CHEM36{NAG}|CHEM37{BMA}|CHEM38{MAN}|CHEM39{BMA}|CHEM40{BMA}|CHEM41{MAN}|CHEM42{NAG}|CHEM43{NAG}|CHEM44{BMA}|CHEM45{NAG}|CHEM46{NAG}|CHEM47{FUL}|CHEM48{NDG}|CHEM49{NAG}|CHEM50{MAN}|CHEM51{BMA}|CHEM52{BMA}|CHEM53{NAG}|CHEM54{NDG}|CHEM55{FUL}$CHEM1,CHEM2,1:R2-1:R1|CHEM5,CHEM6,1:R1-1:R3|CHEM16,CHEM15,1:R1-1:R3|PEPTIDE1,CHEM30,52:R3-1:R1|PEPTIDE1,CHEM8,87:R3-1:R3|CHEM19,CHEM21,1:R3-1:R1|CHEM8,CHEM7,1:R1-1:R2|CHEM22,CHEM23,1:R1-1:R2|PEPTIDE1,CHEM28,292:R3-1:R1|PEPTIDE1,CHEM43,273:R3-1:R1|PEPTIDE1,CHEM46,146:R3-1:R1|PEPTIDE1,CHEM15,135:R3-1:R1|CHEM10,CHEM8,1:R1-1:R2|PEPTIDE1,CHEM36,289:R3-1:R1|CHEM51,CHEM50,1:R2-1:R1|CHEM39,CHEM41,1:R3-1:R1|PEPTIDE1,CHEM12,124:R3-1:R1|PEPTIDE1,CHEM29,7:R3-1:R1|CHEM39,CHEM38,1:R1-1:R2|CHEM20,CHEM19,1:R1-1:R2|CHEM17,CHEM15,1:R1-1:R2|CHEM12,CHEM13,1:R2-1:R1|CHEM46,CHEM47,1:R3-1:R1|CHEM32,CHEM31,1:R1-1:R3|CHEM52,CHEM51,1:R3-1:R1|PEPTIDE1,CHEM23,84:R3-1:R1|CHEM28,CHEM27,1:R2-1:R1|CHEM4,CHEM1,1:R2-1:R1|CHEM31,CHEM33,1:R2-1:R1|PEPTIDE1,CHEM54,190:R3-1:R1|CHEM34,CHEM33,1:R1-1:R2|CHEM18,CHEM17,1:R1-1:R2|CHEM31,CHEM35,1:R1-1:R2|CHEM9,CHEM7,1:R1-1:R3|CHEM26,CHEM27,1:R1-1:R2|CHEM54,CHEM53,1:R2-1:R1|CHEM6,CHEM4,1:R2-1:R1|CHEM1,CHEM3,1:R3-1:R1|CHEM25,CHEM26,1:R1-1:R2|CHEM54,CHEM55,1:R3-1:R1|CHEM43,CHEM42,1:R2-1:R1|CHEM40,CHEM37,1:R2-1:R1|CHEM46,CHEM45,1:R2-1:R1|CHEM12,CHEM14,1:R3-1:R1|CHEM42,CHEM40,1:R2-1:R1|PEPTIDE1,CHEM6,118:R3-1:R1|CHEM24,CHEM23,1:R1-1:R3|CHEM22,CHEM19,1:R2-1:R1|CHEM53,CHEM52,1:R2-1:R1|CHEM48,CHEM49,1:R2-1:R1|PEPTIDE1,CHEM48,184:R3-1:R1|CHEM45,CHEM44,1:R2-1:R1|CHEM37,CHEM38,1:R2-1:R1|CHEM11,CHEM10,1:R1-1:R2|CHEM36,CHEM35,1:R2-1:R1$$$",

 "BLOB1{Gold Particle}|PEPTIDE1{C.C.C.C.C.C}$$$$V2.0" };

#if false
		/// <summary>
		///  Scalable Vector Graphics (SVG)
		/// </summary>

		public string Svg
		{
			get
			{
				try
				{
					if (AltForms != null && AltForms.ContainsKey("Svg"))
					{
						string svg = AltForms["Svg"];
						return svg;
					}

					else return "";
				}

				catch { return ""; }; 
			}

			set
			{
				if (AltForms != null)
				{
					AltForms["Svg"] = value;
					return;
				}

				else return;
			}
		}
#endif

		/// <summary>
		/// Convert this molecule to a new type
		/// </summary>
		/// <param name="newType"></param>

		public void ConvertToNewType(
			MoleculeFormat newType)
		{
			if (PrimaryFormat == newType) return; // no change in type

			else if (PrimaryFormat == MoleculeFormat.Molfile && newType == MoleculeFormat.Chime)
			{
				if (!Lex.IsNullOrEmpty(MolfileString))
					ChimeString = MolLib1.StructureConverter.MolfileStringToChimeString(MolfileString);
				PrimaryFormat = MoleculeFormat.Chime;
			}

			else if (PrimaryFormat == MoleculeFormat.Chime && newType == MoleculeFormat.Molfile)
			{
				if (!Lex.IsNullOrEmpty(ChimeString))
					MolfileString = MolLib1.StructureConverter.ChimeStringToMolfileString(ChimeString);
				PrimaryFormat = MoleculeFormat.Molfile;
			}

			else
			{
				MoleculeMx cs2 = ConvertTo(newType);
				cs2.MemberwiseCopy(this);
			}

			return;
		}

		/// <summary>
		/// Convert from one format to another
		/// </summary>
		/// <param name="newFormat"></param>
		/// <returns></returns>

		public MoleculeMx ConvertTo(
				MoleculeFormat molFormatType)
		{
			string molString, msg;
			bool conversionFailed = false;

			MoleculeMx mol = new MoleculeMx(molFormatType);

			try
			{
				// If no change in type, just copy value

				if (this.PrimaryFormat == molFormatType) // any change in type?
					molString = this.PrimaryValue;

				else molString = GetMoleculeString(molFormatType); // convert from old type if possible

				mol.SetPrimaryTypeAndValue(molFormatType, molString);
				return mol;

			}

			catch (Exception ex) // handle structures that don't convert - issue #216
			{
				//DebugLog.Message("ChemicalStructure.Convert exception for structure: \r\n" + Value + "\r\n" +
				//  DebugLog.FormatExceptionMessage(ex));
				//throw new Exception(ex.Message, ex);
				return new MoleculeMx(PrimaryFormat, ""); // return blank structure
			}
		}

		/// <summary>
		/// Convert chime to molfile with fixups
		/// </summary>
		/// <param name="chime"></param>
		/// <returns></returns>

		internal string ChimeStringToMolfileString(string chime)
		{
			if (chime.Contains(" ")) chime = chime.Replace(" ", "");
			if (chime.Contains("\r")) chime = chime.Replace("\r", "");
			if (chime.Contains("\n")) chime = chime.Replace("\n", "");

			string molFile = MolLib1.StructureConverter.ChimeStringToMolfileString(chime);
			return molFile;
		}

	}
}
