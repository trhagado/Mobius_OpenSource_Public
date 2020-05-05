
using Mobius.ComOps;
using Mobius.Helm;

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
	/// Call HELM services to convert between HELM and other molecule formats
	/// </summary>

	public class HelmConverter
	{

		static HelmWebServiceMx HelmService//
		{
			get
			{
				if (_helmWebService == null)
					_helmWebService = new HelmWebServiceMx();
				return _helmWebService;
			}
		}
		static HelmWebServiceMx _helmWebService = null;

		static HelmWinFormsBrowser Browser
		{
			get
			{
				if (_browser == null)
				{
					_browser = new HelmWinFormsBrowser(HelmControlMode.BrowserViewOnly, null);
					_browser.LoadInitialPage();
				}

				try
				{
					if (_browser.Browser.Document == null)
						throw new Exception("Null Document");
				}
				catch (Exception ex)
				{
					_browser = new HelmWinFormsBrowser(HelmControlMode.BrowserViewOnly, null);
					_browser.LoadInitialPage();
				}

				return _browser;
			}
		}
		static HelmWinFormsBrowser _browser = null;

		public static bool UseWebEditor = true; // Use Scilligence web editor to do conversions
		public static bool UseServices => !UseWebEditor; // Call services to do conversions
		public static bool Debug => JavaScriptManager.Debug;

		/// <summary>
		/// Convert Helm to Molfile format
		/// </summary>
		/// <param name="helm"></param>
		/// <returns></returns>

		public static string HelmToMolfile(
			MoleculeMx mol,
			string helm)
		{
			string molfile = null;

			if (UseWebEditor)
			{
				lock (Browser) // lock to avoid reentrancy
				{
					Browser.SetHelm(helm);
					molfile = Browser.GetMolfile();
				}
			}

			else
			{
				return "Not Implemented";
			}

			return molfile;
		}

		/// <summary>
		/// SMILES generation for  the whole HELM molecule
		/// </summary>
		/// <param name="helm"></param>
		/// <returns></returns>

		public static string HelmToSmiles(
			MoleculeMx mol,
			string helm)
		{
			string smiles = null;

			if (UseWebEditor && DebugMx.False) // don't use this, only gets molfile at monomer level, not atoms/bonds
			{
				Browser.SetHelm(helm);
				smiles = Browser.GetSmiles();
			}

			else
			{
				smiles = HelmService.GenerateSMILESForHELM(helm);
			}

			return smiles;
		}

		/// <summary>
		/// Reads HELMNotation and converts it into peptide/RNA analogue (natural) sequence
		/// </summary>
		/// <param name="helm"></param>
		/// <returns></returns>

		public static string HelmToAnalogSequence(string helm)
		{
			string seq = null;

			if (UseWebEditor)
			{
				Browser.SetHelm(helm);
				seq = Browser.GetSequence();
			}

			else
			{
				if (IsPeptideString(helm))
					seq = HelmService.ConvertIntoPeptideAnalogSequence(helm);

				else if (IsRnaString(helm))
					seq = HelmService.ConvertIntoRNAAnalogSequence(helm);
			}

			return seq;
		}

		/// <summary>
		/// Reads RNA / peptide sequence and generates HELMNotation
		/// </summary>
		/// <param name="sequenceString"></param>
		/// <returns></returns>

		public static string SequenceToHelm(string seqString)
		{
			string helm = null;

			if (UseWebEditor)
			{
				throw new InvalidCastException("Can't convert sequence to Helm"); // can't do yet
			}

			else
			{
				if (IsPeptideString(seqString))
					helm = HelmService.GenerateHELMFromPeptideSequence(seqString);

				else helm = HelmService.GenerateHELMFromRnaSequence(seqString);
			}

			return helm;
		}

		/// <summary>
		/// Reads rna, peptide Fasta-Sequence(s) and generates HELMNotation
		/// </summary>
		/// <param name="fastaString"></param>
		/// <returns></returns>

		public static string FastaToHelm(string fastaString)
		{
			string helm = null;
			string errorMsg = "Can't convert FASTA to Helm";

			if (UseWebEditor)
			{
				throw new InvalidCastException(errorMsg); // can't do yet
			}

			else
			{
				string helm1 = HelmService.GenerateHELMFromFastaPeptide(fastaString);
				if (helm1 == null) throw new InvalidCastException(errorMsg);

				string helm2 = HelmService.GenerateHELMFromFastaRNA(fastaString);
				if (helm2 == null) throw new InvalidCastException(errorMsg);

				if (helm1.Length >= helm2.Length) return helm1;
				else return helm2;
			}
		}

		public static string FastaPeptideToHelm(string fastaString)
		{
			string helm = HelmService.GenerateHELMFromFastaPeptide(fastaString);

			return helm;
		}

		public static string FastaRnaToHelm(string fastaString)
		{
			string helm = null;

			if (UseWebEditor)
			{
				throw new InvalidCastException("Can't convert FASTA to Helm"); // can't do yet
			}

			else
			{
				helm = HelmService.GenerateHELMFromFastaRNA(fastaString);
			}

			return helm;
		}

		/// <summary>
		/// Converts HELM Input into Fasta (RNA or Peptide)
		/// <param name="helm"></param>
		/// <returns></returns>

		public static string HelmToFastaString(string helm)
		{
			string fasta = null;

			if (UseWebEditor)
			{
				return "Can't convert Helm to FASTA";
			}

			else
			{
				fasta = HelmService.GenerateFastaFromHelm(helm);
			}

			return fasta;
		}

		public static string HelmToSvg(
			MoleculeMx mol)
		{
			string svg = mol.SvgString;
			if (mol.PrimaryFormat == MoleculeFormat.Helm && Lex.IsDefined(mol.SvgString))
			{
				return svg;
			}

			string helm = mol.HelmString;
			svg = HelmControl.GetSvg(helm);
			return svg;
		}

		public static Bitmap HelmToBitmap(
					MoleculeMx mol,
					int pixWidth)
		{
			Bitmap bm = null;

			string svg = mol?.SvgString;
			string msg = String.Format("mol.Id: {0}, mol.Helm: {1}", mol?.Id, mol?.HelmString);

			try
			{
				if (mol.PrimaryFormat == MoleculeFormat.Helm && Lex.IsDefined(svg))
				{ // if SVG available use it
					bm = SvgUtil.GetBitmapFromSvgXml(svg, pixWidth);

					if (Debug) DebugLog.Message(msg + " used existing SVG");
					return bm;
				}

				else
				{
					string helm = mol.HelmString;
					bm = HelmControl.GetBitmap(helm, pixWidth);
					if (Debug) DebugLog.Message(msg + " generated SVG");
					return bm;
				}
			}

			catch (Exception ex)
			{
				msg += "\r\n" + DebugLog.FormatExceptionMessage(ex);
				DebugLog.Message(msg); // log it 
				return null;
			}

		}
		/// <summary>
		/// Attempt to convert a molfile to Helm
		/// </summary>
		/// <param name="molfile"></param>
		/// <returns></returns>

		public static string MolfileToHelm(string molfile)
		{
			return ""; // todo...
			//throw new Exception("Can't convert chemical structure to Helm"); // can't do yet
		}

		/// <summary>
		/// Return true is string looks like it's probably HELM 
		/// </summary>
		/// <param name="txt"></param>
		/// <returns></returns>

		public static bool IsHelmString(string txt)
		{
			txt = txt.Trim();

			bool isHelm =
				(Lex.StartsWith(txt, "Peptide1{") ||
				Lex.StartsWith(txt, "RNA1{") ||
				Lex.StartsWith(txt, "Chem1{") ||
				Lex.StartsWith(txt, "BLOB1{"));

			if (isHelm) return true;
			else return false;
		}

		/// <summary>
		/// IsPeptideString
		/// </summary>
		/// <param name="seq"></param>
		/// <returns></returns>

		public static bool IsPeptideString(string seq)
		{
			if (Lex.StartsWith(seq, "Peptide1") && Lex.EndsWith(seq, "$")) // Helm form?
				return true;

			else return false; // todo: check for sequence & fasta form
		}

		/// <summary>
		/// IsRnaString
		/// </summary>
		/// <param name="seq"></param>
		/// <returns></returns>

		public static bool IsRnaString(string seq)
		{
			if (Lex.StartsWith(seq, "RNA1") && Lex.EndsWith(seq, "$")) // Helm form?
				return true;

			else return false; // todo: check for sequence & fasta form
		}

	}
}
