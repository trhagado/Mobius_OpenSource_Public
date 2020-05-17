using Mobius.ComOps;

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
	/// Mobius molecule class
	/// </summary>

	//[Serializable]
	public partial class MoleculeMx : MobiusDataType, IMoleculeMx
	{

		/// <summary>
		/// Compound identifier
		/// </summary>

		//[DataMember]
		public string Id = "";

		/// <summary>
		/// Caption to be displayed above structure
		/// </summary>

		//[DataMember]
		public string Caption = "";

		/// <summary>
		/// Format of molecule representation string
		/// </summary>

		//[DataMember]
		public MoleculeFormat PrimaryFormat
		{
			set => _primaryFormat = value;
			get => _primaryFormat;
		}
		MoleculeFormat _primaryFormat = MoleculeFormat.Unknown;


		/// <summary>
		/// Create/Get associated lower level molecule MolLib instance
		/// </summary>

		public ICdkMol CdkMol => GetCdkMolInstance(); // link to low level object implementation

		/// <summary>
		/// Get or create associated IMolLib instance
		/// </summary>
		/// <returns></returns>

    public ICdkMol GetCdkMolInstance()
    {
      if (_cdkMol == null)
				_cdkMol = CdkMolFactory.NewCdkMol(this);

      return _cdkMol;
    }
    private ICdkMol _cdkMol;

		static ICdkMol CdkMolUtil => StaticCdkMol.I; // static molecule shortcut for utility methods

		///////////////////////////////////////////////////////
		// String format molecule definitions
		// The MoleculeFormat Type determins the primary type
		// for the molecule. Values in other formats
		// indicate the results of runtime conversions
		// or multiple type values from a data source
		///////////////////////////////////////////////////////

		public string MolfileString = null;

		public string ChimeString = null;

		public string SmilesString = null;

		public string InchiString = null;

		public string HelmString = null;

		public string SequenceString = null; // RNA or Peptide natural analog sequence 

		public string FastaString = null; // FASTA nucleotide or amino acid sequences

		public string SvgString // SGV (Scalable Vector Graphics depiction), not convertible to other types
		{ get => GetSvgString(); set => SetSvgString(value); }

		/// <summary>
		/// Get string value of molecule based for the primary format
		/// </summary>

		[DataMember]
		public string PrimaryValue
		{
			get
			{
				switch (PrimaryFormat)
				{
					case MoleculeFormat.Molfile: return MolfileString;
					case MoleculeFormat.Chime: return ChimeString;
					case MoleculeFormat.Smiles: return SmilesString;
					case MoleculeFormat.InChI: return InchiString;
					case MoleculeFormat.Helm: return HelmString;
					case MoleculeFormat.Sequence: return SequenceString; // RNA or Peptide natural analog sequence
					case MoleculeFormat.Fasta: return FastaString;

					default: return "";
				}
			}
		}

		/// <summary>
		/// Return the primary display format based on the primary storage format
		/// </summary>

		public MoleculeRendererType PrimaryDisplayFormat
		{
			get
			{
				if (IsChemStructureFormat)
					return MoleculeRendererType.Chemistry;

				else if (IsBiopolymerFormat)
					return MoleculeRendererType.Helm;

				return MoleculeRendererType.Chemistry; // default
			}
		}

		/// <summary>
		/// Return true if the primary molecule format is an atom/bond structure
		/// </summary>

		public bool IsChemStructureFormat
		{
			get
			{
				switch (PrimaryFormat)
				{
					case MoleculeFormat.Molfile:
					case MoleculeFormat.Chime:
					case MoleculeFormat.Smiles:
					case MoleculeFormat.InChI:
						return true;

					default:
						return false;
				}
			}
		}

		/// <summary>
		/// Return true if the primary molecule format is that of a biopolymer
		/// </summary>

		public bool IsBiopolymerFormat
		{
			get
			{
				switch (PrimaryFormat)
				{
					case MoleculeFormat.Helm:
					case MoleculeFormat.Sequence:
					case MoleculeFormat.Fasta:
						return true;

					default:
						return false;
				}
			}
		}

		/// <summary>
		/// Get any SVG string
		/// </summary>
		/// <returns></returns>

		string GetSvgString() // Stored in AltForms as a GZipped Base64String
		{
			string txt = GetAltForm("SVG");
			string svg = "";
			if (Lex.IsUndefined(txt)) return "";

			if (Lex.Contains(txt, "<svg"))
				svg = txt;

			else
			{
				if (SvgUtil.TryGetSvgXml(txt, out svg))
					return svg;

				else return "";
			}

			return svg;
		}

		/// <summary>
		/// Set a SVG string
		/// </summary>
		/// <param name="svg"></param>

		void SetSvgString(string svg) // Retrieved from AltForms (stored as a GZipped, Base64String)
		{
			if (Lex.IsUndefined(svg))
			{
				RemoveAltForm("SVG");
			}

			else
			{
				string txt = svg;
				if (Lex.Contains(svg, "<svg")) // compress if necessary before storage
					txt = SvgUtil.CompressSvgString(svg);

				SetAltForm("SVG", txt);
			}

			return;
		}

		/// <summary>
		/// Alternate forms (e.g. SVG depictions for SmallWorld)
		/// </summary>

		public Dictionary<string, string> AltForms = null;

		public bool AltFormDefined(string typeName)
		{
			if (AltForms != null && AltForms.ContainsKey(typeName))
				return true;

			else return false;
		}

		public bool TryGetAltForm(
			string typeName,
			out string value)
		{
			value = null;

			if (AltForms != null && AltForms.ContainsKey(typeName))
			{
				value = AltForms[typeName];
				return true;
			}

			else return false;
		}


		/// <summary>
		/// Get an alternate form
		/// </summary>
		/// <param name="typeName"></param>
		/// <returns></returns>

		private string GetAltForm(string typeName)
		{
			if (AltForms != null && AltForms.ContainsKey(typeName))
				return AltForms[typeName];

			else return "";
		}

		/// <summary>
		/// Set an alternate form
		/// </summary>
		/// <param name="typeName"></param>
		/// <param name="value"></param>

		private void SetAltForm(
			string typeName,
			string value)
		{
			if (AltForms == null)
				AltForms = new Dictionary<string, string>();

			AltForms[typeName] = value;
			return;
		}

		private void RemoveAltForm(string typeName)
		{
			if (AltForms != null && AltForms.ContainsKey(typeName))
				AltForms.Remove(typeName);

			return;
		}

		/// <summary>
		/// Debug setting of molecule values
		/// </summary>
		/// <param name="value"></param>

		//void DebugSet(String value)
		//{
		//  string smiles, molfile, chime;

		//  if (Lex.IsUndefined(value)) DebugLog.Message("SetValue: null/blank");
		//  else
		//  {
		//    if (value.Contains("\n"))
		//      molfile = value;
		//    else molfile = StructureConverter.ChimeStringToMolfileString(value); // expect chime here

		//    StructureConverter sc = new StructureConverter();
		//    sc.MolfileString = molfile;

		//    DebugLog.Message("SetValue - Length: " + value.Length + ", value: " + value + "\r\nStack: " + new StackTrace(true).ToString());
		//  }

		//  _value = value;
		//}

		public static MoleculeFormat PreferredMoleculeFormat = MoleculeFormat.Molfile; // preferred format for new molecules

		public static IMoleculeMxUtil IMoleculeMxUtil; // instance for accessing utility methods

		public static bool? HelmEnabled = true; // HELM form of biopolymer structure representation supported

		/// <summary>
		/// Basic constructor
		/// </summary>

		public MoleculeMx()
		{
			return;
		}

		/// <summary>
		/// Just type supplied
		/// </summary>
		/// <param name="molFormatType"></param>

		public MoleculeMx(MoleculeFormat molFormatType)
		{
			SetPrimaryTypeAndValue(molFormatType, "");
			return;
		}

		/// <summary>
		/// Constructor with type and molecule string
		/// </summary>
		/// <param name="molFormatType"></param>
		/// <param name="structure"></param>

		public MoleculeMx(
			MoleculeFormat molFormatType,
			string molString)
		{
			string typedMolString = (molString != null ? molString.Trim() : ""); // Note that this can remove the 1st blank line in a normal molfile)

			if (Lex.StartsWith(typedMolString, "Molfile=") || // really need these checks?
				Lex.StartsWith(typedMolString, "Chime=") ||
				Lex.StartsWith(typedMolString, "Smiles=") ||
				Lex.StartsWith(typedMolString, "Helm=") ||
				Lex.StartsWith(typedMolString, "InChI=") ||
				Lex.StartsWith(typedMolString, "Helm=") ||
				Lex.StartsWith(typedMolString, "Sequence=") ||
				Lex.StartsWith(typedMolString, "Fasta=")
			 )
			{ // type specified in string, call other form of constructor
				MoleculeMx cs2 = new MoleculeMx(molString);
				cs2.MemberwiseCopy(this);
				return;
			}

			else SetPrimaryTypeAndValue(molFormatType, molString);
			return;
		}

		/// <summary>
		/// Constructor with initial molecule string supplied
		/// </summary>

		public MoleculeMx(
			string molString)
		{
			if (String.IsNullOrEmpty(molString)) 
				return;

			string typedMolString = molString.Trim(); // Note that this can remove the 1st blank line in a normal molfile)

			if (String.IsNullOrEmpty(typedMolString))
				return;

			else if (Lex.StartsWith(typedMolString, "MolFile="))
			{
				this.PrimaryFormat = MoleculeFormat.Molfile;
				this.MolfileString = molString.Substring(8);
			}

			else if (molString.IndexOf(" V2000\n") > 0 || molString.IndexOf(" V2000\r") > 0 || // simple molfile without type name
				molString.IndexOf(" V3000\n") > 0 || molString.IndexOf(" V3000\r") > 0)
			{
				this.PrimaryFormat = MoleculeFormat.Molfile;
				this.MolfileString = molString;
			}

			else if (Lex.StartsWith(typedMolString, "Chime="))
			{
				this.PrimaryFormat = MoleculeFormat.Chime;
				this.ChimeString = typedMolString.Substring(6);
			}

			else if (Lex.StartsWith(typedMolString, "Smiles="))
			{
				this.PrimaryFormat = MoleculeFormat.Smiles;
				this.SmilesString = typedMolString.Substring(7);
			}

			else if (Lex.StartsWith(typedMolString, "Helm="))
			{
				this.PrimaryFormat = MoleculeFormat.Helm;
				this.HelmString = typedMolString.Substring(5);
			}

			else if (Lex.StartsWith(typedMolString, "Sequence="))
			{
				this.PrimaryFormat = MoleculeFormat.Sequence;
				this.SequenceString = typedMolString.Substring(9);
			}

			else if (Lex.StartsWith(typedMolString, "Fasta="))
			{
				this.PrimaryFormat = MoleculeFormat.Fasta;
				this.FastaString = typedMolString.Substring(6);
			}

			else if (HelmConverter.IsHelmString(typedMolString)) // see if Helm string without a qualifying prefix
			{
				this.PrimaryFormat = MoleculeFormat.Helm;
				this.HelmString = typedMolString;
			}

			else if (!String.IsNullOrEmpty(ChimeStringToMolfileString(molString)))
			{ // see if a chime string without a qualifying prefix
				this.PrimaryFormat = MoleculeFormat.Chime;
				this.ChimeString = molString;
			}

			else if (!String.IsNullOrEmpty(CdkMol.SmilesStringToMolfileString(molString)))
			{ // see if a Smiles string without a qualifying prefix
				this.PrimaryFormat = MoleculeFormat.Smiles;
				this.SmilesString = molString;
			}

			else if (Lex.StartsWith(typedMolString, "CompoundId="))
			{
				if (IMoleculeMxUtil == null)
					throw new Exception("IMoleculeMxUtil interface instance not defined");

				string cid = typedMolString.Substring("CompoundId=".Length);
				MoleculeMx cs = IMoleculeMxUtil.SelectMoleculeFromCid(cid);
				if (cs != null)
				{
					this.PrimaryFormat = MoleculeFormat.Chime; // store in Chime format
					this.ChimeString = cs.GetChimeString();
				}
			}

			else
			{ // Just continue, optionally logging (some valid smiles strings seem to fail sometimes)
				//DebugLog.Message("Warning: Unrecognized ChemicalStructure format, continuing: " + molString + "\n" + new StackTrace(true));
				//throw new Exception("Unrecognized ChemicalStructure format");
				//return;
			}

			//if (Lex.IsDefined(Value)) // debug
			//  DebugLog.Message("Constructor - Length: " + Value.Length + ", value: " + Value + "\r\nStack: " + new StackTrace(true).ToString());

			return;
		}

		/// <summary>
		/// Set the primary type and value of a molecule from another molecule
		/// The values for other formats are cleared
		/// </summary>
		/// <param name="mol2"></param>

		public void SetPrimaryTypeAndValue(MoleculeMx mol2)
		{
			SetPrimaryTypeAndValue(mol2.PrimaryFormat, mol2.PrimaryValue);
		}

		/// <summary>
		/// Set the primary type and value of a molecule
		/// The values for other formats are cleared
		/// </summary>
		/// <param name="molFormatType"></param>
		/// <param name="molString"></param>

		public void SetPrimaryTypeAndValue(
			MoleculeFormat molFormatType,
			string molString)
		{
			PrimaryFormat = molFormatType;
			ClearMolStrings();

			switch (PrimaryFormat)
			{
				case MoleculeFormat.Molfile:
					string molfile = Lex.AdjustEndOfLineCharacters(molString, "\r\n");
					MolfileString = molfile;
					return;

				case MoleculeFormat.Chime:
					ChimeString = molString;
					return;

				case MoleculeFormat.Smiles:
					SmilesString = molString;
					return;

				case MoleculeFormat.InChI:
					InchiString = molString;
					return;

				case MoleculeFormat.Helm:
					HelmString = molString;
					return;

				case MoleculeFormat.Sequence:
					SequenceString = molString;
					return;

				case MoleculeFormat.Fasta:
					FastaString = molString;
					return;

				default:
					throw new Exception("Invalid MoleculeFormat: " + molFormatType);
			}
		}

		/// <summary>
		/// Get simple PrimaryFormat=PrimaryValue string
		/// </summary>
		/// <returns></returns>
		public string GetPrimaryTypeAndValueString()
		{
			if (PrimaryFormat == MoleculeFormat.Unknown) return "";

			return PrimaryFormat.ToString() + "=" + PrimaryValue;
		}

		/// <summary>
		/// Clear all of the molecule representation string values
		/// </summary>

		void ClearMolStrings()
		{
			MolfileString = null;
			ChimeString = null;
			SmilesString = null;
			InchiString = null;
			HelmString = null;
			SequenceString = null;
			SequenceString = null;
			FastaString = null;

			return;
		}

		/// <summary>
		/// Convert an objectinto a MoleculeMx object
		/// </summary>
		/// <param name="fieldValue"></param>
		/// <returns></returns>

		public static MoleculeMx ConvertObjectToChemicalStructure(
			object fieldValue)
		{
			MoleculeMx cs = null;
			MoleculeFormat molFormatType;
			string txt;

			if (fieldValue == null) return null;

			else if (fieldValue is MoleculeMx)
				cs = (MoleculeMx)fieldValue;

			else if (fieldValue is string || fieldValue is QualifiedNumber)
			{
				if (fieldValue is string)
					txt = (string)fieldValue;

				else txt = ((QualifiedNumber)fieldValue).TextValue;

				molFormatType = MoleculeMx.GetMoleculeFormatType(txt);
				if (molFormatType != MoleculeFormat.Unknown) // just convert is known format
					cs = MoleculeMx.MolStringToMoleculeMx(txt);
			}

			if (cs != null) return cs;
			else throw new Exception("Object is not a recognized molecule format");
		}

		/// <summary>
		/// Return true if molecule structure is defined
		/// </summary>
		/// <param name="mol"></param>
		/// <returns></returns>

		public static bool IsDefined(MoleculeMx mol)
		{
			if (mol != null && Lex.IsDefined(mol.PrimaryValue) && mol.PrimaryFormat != MoleculeFormat.Unknown)
				return true;

			else return false;
		}

		/// <summary>
		/// Return true if molecule structure is undefined
		/// </summary>
		/// <param name="mol"></param>
		/// <returns></returns>

		public static bool IsUndefined(MoleculeMx mol)
		{
			return !IsDefined(mol);
		}

		public int AtomCount => CdkMol.AtomCount; // Get atom count for molecule

		public int HeavyAtomCount => CdkMol.HeavyAtomCount; // Get heavy atom count

		public bool ContainsQueryFeature => CdkMol.ContainsQueryFeature; // Return true if mol contains a query feature

		public double MolWeight => CdkMol.MolWeight; // Get mol weight for a molecule 

		public string MolFormula => CdkMol.MolFormula; //Get mol formula

		/// <summary>
		/// Get molecule name from molFile
		/// </summary>
		/// <returns></returns>

		public string GetMolName()
		{
			string[] header = GetMolHeader();
			return header[0];
		}

		/// <summary>
		/// Set molecule name
		/// </summary>
		/// <returns></returns>

		public void SetMolName(
			string molName)
		{
			if (!IsChemStructureFormat) return;

			string[] header = GetMolHeader();
			header[0] = molName;
			SetMolHeader(header);
			return;
		}

		/// <summary>
		/// Get molecule comments from molFile
		/// </summary>
		/// <param name="molFile"></param>
		/// <returns></returns>

		public string GetMolComments()
		{
			if (!IsChemStructureFormat) return "";

			string[] header = GetMolHeader();
			return header[2];
		}

		/// <summary>
		/// Set molecule comments
		/// </summary>
		/// <param name="molComments"></param>

		public void SetMolComments(
			string molComments)
		{
			if (!IsChemStructureFormat) return;

			string[] header = GetMolHeader();
			header[2] = molComments;
			SetMolHeader(header);
			return;
		}

		/// <summary>
		/// Get three mol header lines
		/// </summary>
		/// <param name="molFile"></param>
		/// <returns></returns>

		public string[] GetMolHeader()
		{
			if (!IsChemStructureFormat) return new string[] { "", "", "" };

			string molFile = GetMolfileString();
			string[] header = new string[3];
			string[] sa = molFile.Split('\n');
			if (sa.Length >= 3)
			{
				for (int i1 = 0; i1 < 3; i1++)
					header[i1] = sa[i1].Replace("\r", "");
			}

			else header[0] = header[1] = header[2] = "";

			return header;
		}

		/// <summary>
		/// Set three header lines for molfile
		/// </summary>
		/// <param name="header"></param>

		public void SetMolHeader(
			string[] header)
		{
			if (!IsChemStructureFormat) return;

			MoleculeFormat initialFormat = PrimaryFormat;
			string molFile = GetMolfileString();
			if (!Lex.Contains(molFile, "V2000")) return; // only works for V2000 format

			int nlCnt = 0;
			int i1;
			bool seenCr = false;
			for (i1 = 0; i1 < molFile.Length; i1++)
			{
				if (molFile[i1] == '\n')
				{
					nlCnt++;
					if (nlCnt == 3) break;
				}
				if (molFile[i1] == '\r') seenCr = true;

			}

			string lineEnd = "\n";
			if (seenCr) lineEnd = "\r\n";

			string molFile2 =
				header[0] + lineEnd +
				header[1] + lineEnd +
				header[2] + lineEnd;

			if (i1 + 1 < molFile.Length)
				molFile2 += molFile.Substring(i1 + 1);

			SetPrimaryTypeAndValue(MoleculeFormat.Molfile, molFile2);

			if (initialFormat == MoleculeFormat.Chime) // convert to back to chime if came in this way
				ConvertToNewType(MoleculeFormat.Chime);

			return;
		}

		/// <summary>
		/// Store key column name and value in molecule comments
		/// </summary>
		/// <param name="mc"></param>
		/// <param name="cid"></param>

		public void StoreKeyValueInMolComments(
			MetaColumn mc,
			string cid)
		{
			if (!IsChemStructureFormat) return;

			string keyName = mc.Name;

			bool canStore = // can store in comments only if molfile or some molfile variation (e.g. not smiles...)
				PrimaryFormat == MoleculeFormat.Molfile ||
				PrimaryFormat == MoleculeFormat.Chime;

			if (!canStore) return;

			if (Lex.StartsWith(mc.MetaTable.Name, MetaTable.PrimaryRootTable))
				keyName = "CorpId";

			SetMolComments(keyName + "=" + cid);
			return;
		}

		/// <summary>
		/// Do a shallow copy of a MoleculeMx to another MoleculeMx
		/// </summary>
		/// <param name="copyToNode"></param>

		public void MemberwiseCopy(MoleculeMx c)
		{
			c.PrimaryFormat = PrimaryFormat;

			c.MolfileString = MolfileString;
			c.ChimeString = ChimeString;
			c.SmilesString = SmilesString;
			c.InchiString = InchiString;
			c.HelmString = HelmString;
			c.SequenceString = SequenceString;
			c.SequenceString = SequenceString;

			return;
		}

		/// <summary>
		/// Check if valid HELM string
		/// </summary>
		/// <param name="helm"></param>

		public static bool IsValidHelmString(string helm)
		{
			return true; // todo: do real check
		}

		/// <summary>
		/// Convert a molecule string to a ChemicalStructure object
		/// </summary>
		/// <param name="molString"></param>

		public static MoleculeMx MolStringToMoleculeMx(
			string molString)
		{
			MoleculeMx cs = new MoleculeMx(molString);
			return cs;
		}

		/// <summary>
		/// Get the structure format of a string
		/// </summary>
		/// <param name="txt"></param>
		/// <returns></returns>

		public static MoleculeFormat GetMoleculeFormatType(
			object molValue)
		{
			if (molValue == null) return MoleculeFormat.Unknown;

			else if (molValue is string)
			{
				string molString = (string)molValue;
				if (molString == "") return MoleculeFormat.Unknown;

				else if (Lex.IsLong(molString)) return MoleculeFormat.Unknown;

				else if (Lex.StartsWith(molString, "SketchFile=")) return MoleculeFormat.SkcFileName;

				else if (Lex.StartsWith(molString, "MolFileFile=")) return MoleculeFormat.MolfileName;

				else if (Lex.StartsWith(molString, "MolFile=")) return MoleculeFormat.Molfile;

				else if (molString.IndexOf(" V2000\n") > 0 || molString.IndexOf(" V2000\r") > 0 ||
					molString.IndexOf(" V3000\n") > 0 || molString.IndexOf(" V3000\r") > 0)
					return MoleculeFormat.Molfile; // simple molfile without type name

				else if (Lex.StartsWith(molString, "Chime=")) return MoleculeFormat.Chime;

				else if (Lex.StartsWith(molString, "Smiles=")) return MoleculeFormat.Smiles;

				else if (Lex.StartsWith(molString, "InChI=")) return MoleculeFormat.InChI;

				else if (Lex.StartsWith(molString, "CompoundId=")) return MoleculeFormat.CompoundId;

				else if (Lex.StartsWith(molString, "Helm=")) return MoleculeFormat.Helm;

				else if (Lex.StartsWith(molString, "Sequence=")) return MoleculeFormat.Sequence;

				else if (Lex.StartsWith(molString, "Fasta=")) return MoleculeFormat.Fasta;

				else if (!String.IsNullOrEmpty(ChimeStringToMolfileString(molString))) return MoleculeFormat.Chime;

				else if (!String.IsNullOrEmpty(SmilesStringToMolfileString(molString))) return MoleculeFormat.Smiles;

				else return MoleculeFormat.Unknown;
			}

			else return MoleculeFormat.Unknown;
		}

		/// <summary>
		/// Verify that proper prefix is set for transform type
		/// </summary>
		/// <param name="strString"></param>
		/// <param name="transform"></param>
		/// <returns>True if a recognized transform</returns>

		public static bool TrySetStructureFormatPrefix(
			ref string strString,
			string transform)
		{

			if (Lex.IsUndefined(strString) || Lex.IsUndefined(transform)) return false;

			else if (Lex.Eq(transform, "FromMolFile"))
			{
				if (!Lex.StartsWith(strString, "MolFile="))
					strString = "MolFile=" + strString; // add prefix so identified as molFile

				return true;
			}

			else if (Lex.Eq(transform, "FromChime"))
			{
				if (!strString.StartsWith("Chime="))
					strString = "Chime=" + strString; // add prefix so identified as chime

				return true;
			}

			else if (Lex.Eq(transform, "FromSmiles") || Lex.Eq(transform, "IntegrateMmpStructure"))
			{
				if (!strString.StartsWith("Smiles="))
					strString = "Smiles=" + strString; // add prefix so identified as smiles

				return true;
			}

			else if (Lex.Eq(transform, "FromInChI"))
			{
				if (!strString.StartsWith("InChI="))
					strString = "InChI=" + strString; // add prefix so identified as InChI if needed

				return true;
			}

			else return false;
		}

		/// <summary>
		/// Convert molfile to Smiles string
		/// </summary>
		/// <param name="?"></param>
		/// <returns></returns>

		public static string MolFileToSmilesString(
			string molFile)
		{
			return MolfileStringToSmilesString(molFile);
		}

		/// <summary>
		/// Convert Smiles string to molfile
		/// </summary>
		/// <param name="Smilesstring"></param>
		/// <returns></returns>

		public static string SmilesStringToMolFile(
			string smilesString)
		{
			return SmilesStringToMolfileString(smilesString);
		}

		/// <summary>
		/// Convert Chime string to Smiles string
		/// </summary>
		/// <param name="?"></param>
		/// <returns></returns>

		public static string ChimeStringToSmilesString(
			string chimeString)
		{
			return ChimeStringToSmilesString(chimeString);
		}

		/// <summary>
		/// Convert Smiles string to Chime string
		/// </summary>
		/// <param name="Smilesstring"></param>
		/// <returns></returns>

		public static string SmilesStringToChimeString(
			string smilesString)
		{
			return SmilesStringToChimeString(smilesString);
		}

		/// <summary>
		/// Set the structure in a Render/Renditor control
		/// Also, set the display preferences based on any associated CorpId and 
		/// a temp name in the tag.
		/// </summary>
		/// <param name="r"></param>
		/// <param name="cs"></param>

		public static void SetRendererStructure(
			INativeMolControl r,
			MoleculeMx cs,
			bool updateDisplayPreferences = true,
			string name = "")
		{
			string molfile = "";
			if (cs != null)
			{
				molfile = cs.GetMolfileString();

				if (updateDisplayPreferences)
				{
					throw new NotImplementedException();

					//DisplayPreferences dp = cs.GetDisplayPreferences();
					//if (r.Preferences == null) r.Preferences = new DisplayPreferences();
					//dp.CopyTo(r.Preferences);
				}
			}

			r.SetMoleculeAndRender(cs.PrimaryFormat, cs.PrimaryValue); // set the structure

			string tag = "";
			if (Lex.IsDefined(name))
				tag = name + "\t" + cs?.PrimaryFormat + "=" + cs?.PrimaryValue; // used to hold associated name and to detect if molecule has beed edited since set here

			r.SetTag(tag);

			return;
		}

		/// <summary>
		/// Get Display Preferences including stereo preferences base on the CorpId associated with the struture
		/// </summary>
		/// <returns></returns>

		public DisplayPreferences GetDisplayPreferences()
		{
			DisplayPreferences dp = new DisplayPreferences();
			return dp;
		}

		/// <summary>
		/// Return  if stored in molecule comments
		/// </summary>

		public int GetCompoundId()
		{
			int corpId;

			try
			{
				string comments = GetMolComments();
				if (!Lex.StartsWith(comments, "cid=")) return -1;
				if (!int.TryParse(comments.Substring(4), out corpId)) return -1;
				else return corpId;
			}
			catch (Exception ex) { return -1; }
		}

		public static void SetMoleculeControlStructure(
			 INativeMolControl r,
			 MoleculeMx cs)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Convert the specified object to the corresponding MobiusDataType
		/// </summary>
		/// <param name="o"></param>
		/// <returns></returns>

		public static MoleculeMx ConvertTo(
			object o)
		{
			if (o is MoleculeMx) return (MoleculeMx)o;
			else if (NullValue.IsNull(o)) return new MoleculeMx();
			else if (o is string) return new MoleculeMx((string)o);

			throw new InvalidCastException(o.GetType().ToString());
		}

		/// <summary>
		/// Get hash code for this MobiusDataType
		/// </summary>
		/// <returns></returns>

		public override int GetHashCode()
		{
			if (SystemUtil.InDesignMode) return 123;

			int hash = unchecked((int)MolWeight); // use molweight as hash
			return hash;
		}

		/// <summary>
		/// Compare this MobiusDataType to another for equality 
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>

		public override bool Equals(object obj)
		{
			return (CompareTo(obj) == 0);
		}

		/// <summary>
		/// Compare two structures (IComparable.CompareTo)
		/// </summary>
		/// <param name="o"></param>
		/// <returns></returns>

		public override int CompareTo(
			object o)
		{
			MoleculeMx cs2 = (MoleculeMx)o;

			if (cs2 == null)
			{
				if (Lex.IsDefined(this.PrimaryValue))
					return 1; // say greater if this has a defined value

				else return 0; // say equal if both null?
			}

			double mw1 = CdkMolFactory.NewCdkMol(this).MolWeight;
			double mw2 = CdkMolFactory.NewCdkMol(cs2).MolWeight;
			return mw1.CompareTo(mw2);
		}

		/// <summary>
		/// Return true if null value
		/// </summary>

		[XmlIgnore]
		public override bool IsNull => IsUndefined(this);

		/// <summary>
		/// Return true if column is sortable
		/// </summary>

		[XmlIgnore]
		public override bool IsSortable
		{
			get { return true; }
		}

		/// <summary>
		/// Return true if column normally has a graphical representation
		/// </summary>

		[XmlIgnore]
		public override bool IsGraphical
		{
			get { return true; }
		}

		/// <summary>
		/// Convert the value to the nearest primitive type
		/// </summary>
		/// <returns></returns>

		public override object ToPrimitiveType()
		{
			return GetSmilesString();
			//			return ChimeString;
		}

		/// <summary>
		/// Select a ChemicalStructure object for a compound id inserting any stereo comments
		/// </summary>
		/// <param name="cid"></param>
		/// <returns></returns>

		public static MoleculeMx SelectMoleculeForCid(
			string cid,
			MetaTable mt = null)
		{
			if (IMoleculeMxUtil == null) return null;
			else return IMoleculeMxUtil.SelectMoleculeFromCid(cid, mt);
		}

		/// <summary>
		/// Custom compact serialization
		/// </summary>
		/// <returns></returns>

		public override StringBuilder Serialize()
		{
			if (IsNull) return new StringBuilder("<M>");
			StringBuilder sb = BeginSerialize("M");
			if (PrimaryFormat != MoleculeFormat.Molfile) // serialize in primary format
			{
				sb.Append(((int)PrimaryFormat).ToString());
				sb.Append(",");
				sb.Append(NormalizeForSerialize(PrimaryValue));
			}

			else // serialize molfile as chimestring
			{
				sb.Append(((int)MoleculeFormat.Chime).ToString());
				sb.Append(",");
				sb.Append(NormalizeForSerialize(GetChimeString()));
			}

			sb.Append(">");
			return sb;
		}

		/// <summary>
		/// Custom Compact deserialization
		/// </summary>
		/// <param name="sa"></param>
		/// <param name="sai"></param>
		/// <returns></returns>

		public static MoleculeMx Deserialize(string[] sa, int sai)
		{
			MoleculeFormat type = (MoleculeFormat)int.Parse(sa[sai]);
			string value = sa[sai + 1];
			MoleculeMx cs = new MoleculeMx(type, value);
			return cs;
		}

		/// <summary>
		/// Binary serialize
		/// </summary>
		/// <param name="bw"></param>

		public override void SerializeBinary(BinaryWriter bw)
		{
			bw.Write((byte)VoDataType.ChemicalStructure);
			base.SerializeBinary(bw);

			bw.Write(Lex.S(Id));
			bw.Write(Lex.S(Caption));

			if (PrimaryFormat != MoleculeFormat.Molfile) // serialize in primary format
			{
				bw.Write((int)PrimaryFormat); // serialize type
				bw.Write(Lex.S(PrimaryValue)); // and value
			}

			else // serialize molfile as chime
			{
				bw.Write((int)MoleculeFormat.Chime);
				bw.Write(Lex.S(GetChimeString()));
			}

			if (AltForms != null && AltForms.Count > 0 && ClientSupportsAltForms)
			{
				bw.Write((byte)VoDataType.ChemicalStructureAppendix); // indicate appendix exists
				bw.Write((Int32)AltForms.Count);
				foreach (KeyValuePair<string, string> kvp in AltForms)
				{
					bw.Write(Lex.S(kvp.Key));
					bw.Write(Lex.S(kvp.Value));
				}
			}

		}

		/// <summary>
		/// return true if alternate structure forms supported by client (e.g. SVG structure depictions for SmallWorld)
		/// </summary>

		public static bool ClientSupportsAltForms
		{
			get
			{
				return ClientState.MobiusClientVersionIsAtLeast(4, 1);
			}
		}

		/// <summary>
		/// Binary Deserialize
		/// </summary>
		/// <param name="br"></param>
		/// <returns></returns>

		public static MoleculeMx DeserializeBinary(BinaryReader br)
		{
			MoleculeMx cs = new MoleculeMx();
			MobiusDataType.DeserializeBinary(br, cs);
			cs.Id = br.ReadString();
			cs.Caption = br.ReadString();

			MoleculeFormat type = (MoleculeFormat)br.ReadInt32();
			string value = br.ReadString();
			cs.SetPrimaryTypeAndValue(type, value);

			int peek = br.PeekChar(); // see if structure appendix included
			if (peek == (int)VoDataType.ChemicalStructureAppendix)
			{
				peek = br.ReadByte(); // read the peeked at byte

				int altCnt = br.ReadInt32();
				if (altCnt > 0)
					cs.AltForms = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

				for (int ai = 0; ai < altCnt; ai++)
				{
					string key = br.ReadString();
					value = br.ReadString();
					cs.SetAltForm(key, value);
				}
			}

			return cs;
		}

		/// <summary>
		/// Clone 
		/// </summary>
		/// <returns></returns>

		public new MoleculeMx Clone()
		{
			MoleculeMx cs = (MoleculeMx)this.MemberwiseClone();
			if (cs.MdtExAttr != null) cs.MdtExAttr = MdtExAttr.Clone();
			return cs;
		}

		/// <summary>
		/// Return type & value when converting to string
		/// </summary>
		/// <returns></returns>

		public override string ToString()
		{
			return "Type=" + PrimaryFormat + ", Value=" + (PrimaryValue != null ? PrimaryValue : "");
		}
	}

	/// <summary>
	/// Interface for selecting chemical structures from database
	/// </summary>

	public interface IMoleculeMxUtil
	{

		/// <summary>
		/// Select structure for specified cid
		/// </summary>
		/// <param name="cid"></param>
		/// <param name="mt"></param>
		/// <returns></returns>

		MoleculeMx SelectMoleculeFromCid(
			string cid,
			MetaTable mt = null);


	} // IMoleculeMxUtil

	/// <summary>
	/// Enumeration of molecule representation format types
	/// </summary>

	public enum MoleculeFormat
	{
		Unknown = 0,
		Molfile = 1, // Molfile
		MolfileName = 2, // Name of file containing a molfile
		Sketch_Old = 3, // Internal sketch object
		SkcFileName = 4, // Name of sketch file
		Chime = 5, // Chime string
		Smiles = 6, // Smiles
		CompoundId = 7,  // Compound identifier
		InChI = 8, // InChI (not fully supported)

		Helm = 11, // Pistoia HELM Biopolymer format
		Sequence = 12, // "Natural RNA sequence" nucleotide or amino acid sequences
		Fasta = 13 // FASTA nucleotide or amino acid sequences
	}

	/// <summary>
	/// Molecule display (rendering) format
	/// </summary>

	public enum MoleculeRendererType
	{
		Unknown = 0,
		Chemistry = 1, // 2D full molfile atom/bond rendering
		Helm = 2 // Helm Biopolymer rendering (Scilligence)
	}

}
