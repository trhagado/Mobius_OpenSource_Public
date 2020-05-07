using Mobius.ComOps;
using Mobius.Helm;

using DevExpress.Utils;

using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace Mobius.Data
{
	public partial class MoleculeMx : MobiusDataType
	{
		public const int StandardBondLength = 200; // (milliinches)
		public const int MinBondLength = 10; // (milliinches)
		public const int MaxBondLength = 9800; // (milliinches)

		public const int StandardFontSize = 10; // (points)
		public const int StandardBoxWidth = 2500; // (milliinches)

		public const double DefaultMinSimScore = .75; // lower sim limit
		public const int DefaultMaxSimHits = 100; // max number of sim hits

		public const int DefaultStructurePopupWidth = 768; // 256, 512...

				/// <summary>
		/// Build a molecule tooltip containing the structure and optional label
		/// </summary>
		/// <param name="mol"></param>
		/// <param name="structureLabel"></param>
		/// <param name="width"></param>
		/// <returns></returns>

		public SuperToolTip BuildStructureTooltip(
			int width = DefaultStructurePopupWidth,
			string structureLabel = null)
		{
			ToolTipItem i;

			//SystemUtil.Beep(); // debug

			MoleculeMx mol = this;

			if (MoleculeMx.IsUndefined(mol)) return null;

			if (mol.IsChemStructureFormat)
				width = (int)(width * .8); // scale down a bit

			else if (mol.IsBiopolymerFormat)
			{
				width = (int)(width * 1.75); // scale up a bit
				string svg = mol.SvgString;
				if (Lex.IsUndefined(svg))
					svg = HelmControl.GetSvg(mol.HelmString);

				RectangleF b = SvgUtil.GetSvgDocumentBounds(svg);
				if (b.Width > 0) // adjust width so that w + w/3 (e.g. h)
				{
					float l1 = width + width / 10; // default total length in pixels 
					float svgToPix = width / b.Width;

					float l2 = (b.Width + b.Height) * svgToPix; // total pix length if this width kept

					if (l2 > l1) // scale down width proportionally if box taller than expected
						width = (int)(width * (l1 / l2));
				}
			}

			SuperToolTip stt = new SuperToolTip();
			stt.MaxWidth = width;

			if (Lex.IsDefined(structureLabel))
			{
				i = new ToolTipItem();
				i.AllowHtmlText = DefaultBoolean.True;
				i.Appearance.TextOptions.WordWrap = WordWrap.Wrap;
				i.Text = structureLabel + "<br>";
				stt.Items.Add(i);
			}

			Bitmap bm = mol.GetBitmap(width);
			i = new ToolTipItem();
			i.Image = bm;
			stt.Items.Add(i);

			return stt;
		}

		/// <summary>
		/// Get bitmap of specified width and variable height for the current molecule (Chem or HELM)
		/// </summary>
		/// <param name="bitmapWidth"></param>
		/// <returns></returns>

		public Bitmap GetBitmap(
			int bitmapWidth)
		{
			Bitmap bm = null;

			MoleculeMx mol = this;

			if (mol == null) return null;

			if (mol.IsChemStructureFormat) // molfile type molecule
			{
				int translateType = 0, fixedHeight = 0, pageHeight = 11000;
				bool markBoundaries = false;
				Rectangle boundingRect;

				int miWidth = MoleculeMx.PixelsToMilliinches(bitmapWidth);
				Rectangle destRect = new Rectangle(0, 0, miWidth, miWidth * 4 / 5); // default dest rect

				int stdBitmapBoxWidth = MoleculeMx.MilliinchesToPixels(MoleculeMx.StandardBoxWidth);
				float scale = (float)bitmapWidth / stdBitmapBoxWidth;
				int desiredBondLength = (int)(MoleculeMx.StandardBondLength * scale); // in milliinches
				desiredBondLength = CdkMol.AdjustBondLengthToValidRange(desiredBondLength); // be sure bond len within allowed range 

				DisplayPreferences dp = mol.GetDisplayPreferences();
				dp.StandardBondLength = MoleculeMx.MilliinchesToDecipoints(desiredBondLength);

				mol.CdkMol.FitStructureIntoRectangle // scale and translate structure into supplied rectangle.
					(ref destRect, desiredBondLength, translateType, fixedHeight, markBoundaries, pageHeight, out boundingRect);

				int pixWidth = MoleculeMx.MilliinchesToPixels(destRect.Width);
				int pixHeigth = MoleculeMx.MilliinchesToPixels(destRect.Height);

				Font font = new Font("Tahoma", 8.25f);
				CellStyleMx cellStyle = new CellStyleMx(font, Color.Black, Color.Empty);

				bm = mol.CdkMol.GetFixedHeightMoleculeBitmap(pixWidth, pixHeigth, dp, cellStyle, (string)mol.Caption);
			}

			else if (mol.IsBiopolymerFormat) // Output HELM image for biopolymer
			{
				bm = HelmConverter.HelmToBitmap(mol, bitmapWidth);
				int height = bm.Height;
			}

			else bm = null;

			return bm;
		}

		/// <summary>
		/// Set the caption for a structure
		/// </summary>
		/// <param name="caption">Text of caption</param>

		public void CreateStructureCaption(
			string caption)
		{
			if (!IsChemStructureFormat) return;

			CdkMol.CreateStructureCaption(caption);
			return;
		}

		/// <summary>
		/// Remove any existing structure captions
		/// </summary>

		public void RemoveStructureCaption()
		{
			if (!IsChemStructureFormat) return;

			try
			{
				CdkMol.RemoveStructureCaption();

				return;
			}

			catch (Exception ex)
			{
				return; // ignore any errors
			}
		}

		/// <summary>
		/// Adjust bond length to valid range
		/// </summary>
		/// <param name="bondLen"></param>
		/// <returns></returns>

		public int AdjustBondLengthToValidRange(int bondLen)
		{
			return CdkMol.AdjustBondLengthToValidRange(bondLen);
		}

		// Conversions

		public static int MilliinchesToDecipoints(
			int mi)
		{
			return (int)(mi * (720.0 / 1000.0));
		}

		public static int DecipointsToMilliinches(
			int dp)
		{
			return (int)(dp * (1000.0 / 720.0));
		}

		public static int PixelsToMilliinches(
			int pixelCount)
		{
			return (int)(pixelCount / 96.0 * 1000.0); // assume 96 pixels/inch
		}

		public static int MilliinchesToPixels(
			int milliinches)
		{
			return (int)(milliinches / 1000.0 * 96.0); // assume 96 pixels/inch (wrong for high DPI displays)
		}

		/// <summary>
		/// Get molecule name from molFile
		/// </summary>
		/// <returns></returns>

		public static string GetMolfileMolName(
			string molFile)
		{
			string header = GetMolfileHeader(molFile);
			string[] hl = header.Split('\n');
			if (hl.Length < 1) return "";
			return hl[0].Trim();
		}

		/// <summary>
		/// Get molecule comments from molFile
		/// </summary>
		/// <param name="molFile"></param>
		/// <returns></returns>

		public static string GetMolfileComments(
			string molFile)
		{
			string header = GetMolfileHeader(molFile);
			string[] hl = header.Split('\n');
			if (hl.Length < 3) return "";
			return hl[2].Trim();
		}

		/// <summary>
		/// Get three mol header lines
		/// </summary>
		/// <param name="molFile"></param>
		/// <returns></returns>

		public static string GetMolfileHeader(
			string molFile)
		{
			int nlCnt = 0;
			int i1;
			for (i1 = 0; i1 < molFile.Length; i1++)
			{
				if (molFile[i1] == '\n')
				{
					nlCnt++;
					if (nlCnt == 3) break;
				}
			}

			string header = molFile.Substring(0, i1).Replace("\r", "");
			return header;
		}

		/// <summary>
		/// Convert structure according to flag settings
		/// </summary>
		/// <param name="cs"></param>
		/// <param name="flags"></param>
		/// <param name="name">Name to go in first line</param>
		/// <returns></returns>

		public MoleculeMx Convert(
			MoleculeTransformationFlags flags,
			string name)
		{
			MoleculeMx cs2 = this.Clone();

			cs2.CdkMol.TransformMolecule(flags, name);

			return cs2;
		}

		/// <summary>
		/// Read in a sketch file
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>

		public static MoleculeMx ReadSketchFile(
			string fileName)
		{
			FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
			Byte[] ba = new byte[fs.Length];

			fs.Read(ba, 0, ba.Length);
			fs.Close();

			throw new NotImplementedException();

			//CdkMol.StructureConverter sc = new CdkMol.StructureConverter();
			//sc.SketchData = ba;
			//string molFile = sc.MolfileString;
			//MoleculeMx cs = new MoleculeMx(MoleculeFormat.Molfile, molFile);
			//return cs;
		}

		/// <summary>
		/// Read in a Molfile 
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>

		public static MoleculeMx ReadMolfile(
			string fileName)
		{
			StreamReader sr = new StreamReader(fileName);
			string molFile = sr.ReadToEnd();
			sr.Close();

			MoleculeMx cs = new MoleculeMx(MoleculeFormat.Molfile, molFile);
			return cs;
		}

		/// <summary>
		/// Read the set of structures and IDs in an SDFile
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>

		public static List<MoleculeMx> ReadSDFileStructures(string fileName)
		{
			List<MoleculeMx> csList = new List<MoleculeMx>();

			StreamReader sr = new StreamReader(fileName);
			while (true)
			{
				List<SdFileField> flds = SdFileDao.Read(sr);
				if (flds == null) break;
				if (flds.Count == 0) continue;
				string molFile = flds[0].Data;
				if (Lex.IsNullOrEmpty(molFile)) continue;
				MoleculeMx cs = new MoleculeMx(MoleculeFormat.Molfile, molFile);
				if (cs == null || cs.AtomCount == 0) continue;

				cs.Id = MoleculeMx.GetMolfileMolName(molFile); // get any molecule name from molfile header
				csList.Add(cs);
			}
			sr.Close();

			return csList;
		}

		/// <summary>
		/// Read the set of structures and IDs in a Smiles file
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>

		public static List<MoleculeMx> ReadSmilesFileStructures(string fileName)
		{
			List<MoleculeMx> csList = new List<MoleculeMx>();

			StreamReader sr = new StreamReader(fileName);
			while (true)
			{
				string smiles = sr.ReadLine();
				if (smiles == null) break;

				smiles = smiles.Trim();
				if (smiles.Length == 0) continue;

				string cid = ""; // compoundId, name

				int i1 = smiles.IndexOf("\t"); // tab delimiter?
				if (i1 >= 0)
				{
					cid = smiles.Substring(0, i1).Trim();
					smiles = smiles.Substring(i1 + 1).Trim();
				}

				else // see if space delimiter
				{
					i1 = smiles.IndexOf(" ");
					if (i1 >= 0)
					{
						cid = smiles.Substring(i1 + 1).Trim();
						smiles = smiles.Substring(0, i1).Trim();
					}
				}

				MoleculeMx cs = new MoleculeMx(MoleculeFormat.Smiles, smiles);
				if (cs == null || cs.AtomCount == 0) continue;
				cs.Id = cid;

				csList.Add(cs);
			}
			sr.Close();

			return csList;
		}

		/// <summary>
		/// Read in a Smiles file 
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>

		public static MoleculeMx ReadSmilesFile(
			string fileName)
		{
			return ReadSingleLineMolStringFile(fileName, MoleculeFormat.Smiles);
		}

		/// <summary>
		/// Read in a Helm file 
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>

		public static MoleculeMx ReadHelmFile(
			string fileName)
		{
			return ReadSingleLineMolStringFile(fileName, MoleculeFormat.Helm);
		}

		/// <summary>
		/// Read in a Sequence file 
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>

		public static MoleculeMx ReadBiopolymerSequenceFile(
			string fileName)
		{
			return ReadSingleLineMolStringFile(fileName, MoleculeFormat.Sequence);
		}

		/// <summary>
		/// Read in a FASTAs file 
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>

		public static MoleculeMx ReadFastaFile(
			string fileName)
		{
			return ReadMultilineMolStringFile(fileName, MoleculeFormat.Fasta);
		}

		/// <summary>
		/// ReadSingleLineMolStringFile
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="format"></param>
		/// <returns></returns>

		public static MoleculeMx ReadSingleLineMolStringFile(
			string fileName,
			MoleculeFormat format)
		{
			StreamReader sr = new StreamReader(fileName);
			string molString = sr.ReadLine();
			sr.Close();

			MoleculeMx cs = new MoleculeMx(format, molString);
			return cs;
		}

		/// <summary>
		/// ReadMultilineMolStringFile
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="format"></param>
		/// <returns></returns>

		public static MoleculeMx ReadMultilineMolStringFile(
			string fileName,
			MoleculeFormat format)
		{
			string molString = FileUtil.ReadFile(fileName);
			MoleculeMx cs = new MoleculeMx(format, molString);
			return cs;
		}

		/// <summary>
		/// Modify Sql syntax so structures aren't retrieved from cartridge
		/// </summary>
		/// <param name="sqlParm"></param>
		/// <returns></returns>

		public static string RemoveSqlStructureRetrievalMethods(string sqlParm)
		{
			string intermediateSql, finalSql;
			string initialSql = intermediateSql = finalSql = Lex.CompactWhitespace(sqlParm);

			// First check references to helm and sequence strings and replace with "to_clob(null)" expressions 

			if (Lex.Contains(intermediateSql, "s.helm_txt"))
				intermediateSql = Lex.Replace(intermediateSql, "s.helm_txt", "to_clob(null)");

			if (Lex.Contains(intermediateSql, "s.sequence_txt"))
				intermediateSql = Lex.Replace(intermediateSql, "s.sequence_txt", "to_clob(null)");

			// Convert chime(x.ctab) references to molecule saying no access available

			if (Lex.Contains(intermediateSql, "chime"))
			{
				string chimeReplacement = Lex.AddSingleQuotes(StructureAccessNotAuthorizedChimeString);
				intermediateSql = Lex.Replace(intermediateSql, "chime(ctab)", "to_clob(" + chimeReplacement + ")");
				intermediateSql = Lex.Replace(intermediateSql, "chime(m.ctab)", "to_clob(" + chimeReplacement + ")");
			}

			// Now check for more chime, molfile and smiles

			if (!Lex.Contains(intermediateSql, "Chime") && !Lex.Contains(intermediateSql, "Molfile") && !Lex.Contains(intermediateSql, "Smiles"))
				return intermediateSql;

			Lex lex = new Lex();
			lex.SetDelimiters(" , ; ( ) < = > <= >= <> != !> !<");
			lex.OpenString(intermediateSql);
			finalSql = intermediateSql; // modify from intermediate

			// Find references to functions that return structures and replace them with TO_CLOB(NULL) expressions

			while (true)
			{
				var pt = lex.GetPositionedToken();
				if (pt == null) break;

				if (!Lex.Eq(pt.Text, "Chime") && !Lex.Eq(pt.Text, "Molfile") && !Lex.Eq(pt.Text, "Smiles"))
					continue;

				var tok = lex.Get();

				// some sql comes in with alias values such as: select SMILES MOLSTRUCTURE from....
				// We need to replace the SMILES with the appropriate "Unauthorized" CLOB
				bool isColumnAlias = Regex.IsMatch(tok, @"^[a-zA-Z]+$") && !Lex.EqualsIgnoreCase(tok, "from");
				string newSqlFragemnt, oldSqlFragment;
				if (isColumnAlias)
				{
					if (Lex.Eq(pt.Text, "Chime")) newSqlFragemnt = Lex.AddSingleQuotes(StructureAccessNotAuthorizedChimeString);
					else if (Lex.Eq(pt.Text, "Molfile") || Lex.Eq(tok, "MOLSTRUCTURE")) newSqlFragemnt = Lex.AddSingleQuotes(StructureAccessNotAuthorizedMolfile);
					else newSqlFragemnt = "null";
					newSqlFragemnt = "to_clob(" + newSqlFragemnt + ") "; // convert to clob
					newSqlFragemnt += " " + tok;

					oldSqlFragment = pt.Text.Trim() + " " + tok.Trim();

					finalSql = Lex.Replace(finalSql, oldSqlFragment, newSqlFragemnt);

					continue;
				}

				if (tok != "(") continue;
				int depth = 1;

				while (true) // scan to closing paren
				{
					var pt2 = lex.GetPositionedToken();
					if (pt2 == null) break;
					if (pt2.Text == "(") depth++;
					else if (pt2.Text == ")") depth--;
					if (depth == 0)
					{
						int i1 = pt.Position;
						oldSqlFragment = sqlParm.Substring(i1, pt2.Position - i1 + 1);

						if (Lex.Eq(pt.Text, "Chime")) newSqlFragemnt = Lex.AddSingleQuotes(StructureAccessNotAuthorizedChimeString);
						else if (Lex.Eq(pt.Text, "Molfile")) newSqlFragemnt = Lex.AddSingleQuotes(StructureAccessNotAuthorizedMolfile);
						else newSqlFragemnt = "null";

						newSqlFragemnt = "to_clob(" + newSqlFragemnt + ")"; // convert to clob

						finalSql = Lex.Replace(finalSql, oldSqlFragment, newSqlFragemnt);
						break;
					}
				}

			}

			finalSql = RemoveSelectMolSmilesSql(finalSql); // disable specialized smiles retrieval code

			return finalSql;
		}

		/// <summary>
		/// Modify Sql syntax so sequence columns are replaced with Access Denied message.
		/// </summary>
		/// <param name="sqlParm"></param>
		/// <returns></returns>
		public static string RemoveSqlSequenceRetrievalMethods(string sqlParm)
		{
			string startingSql = Lex.CompactWhitespace(sqlParm);

			//if (!Lex.Contains(startingSql, "HEAVYCHAIN") && 
			//    !Lex.Contains(startingSql, "LIGHTCHAIN") && 
			//    !Lex.Contains(startingSql, "CHAIN") && 
			//    !Lex.Contains(startingSql, "ALL_SEQUENCES") && 
			//    !Lex.Contains(startingSql, "SEQUENCEVALUE"))
			//    return startingSql;

			if (!Lex.Contains(startingSql, "SEQUENCEVALUE")) return startingSql;

			RegexOptions options = RegexOptions.IgnoreCase;
			var regexFrom = new Regex(Regex.Escape(" FROM "), options);
			var result = regexFrom.Split(startingSql, 2);

			if (result.Length < 1) return startingSql;

			string outerSelect = result[0];
			string remainingSql = result[1];

			//if (!Lex.Contains(outerSelect, "HEAVYCHAIN") && 
			//    !Lex.Contains(outerSelect, "LIGHTCHAIN") && 
			//    !Lex.Contains(outerSelect, "CHAIN") && 
			//    !Lex.Contains(outerSelect, "ALL_SEQUENCES") &&
			//    !Lex.Contains(outerSelect, "SEQUENCEVALUE"))
			//    return startingSql;

			if (!Lex.Contains(outerSelect, "SEQUENCEVALUE")) return startingSql;

			//var regex = new Regex(Regex.Escape("HEAVYCHAIN"), options);
			//outerSelect = regex.Replace(outerSelect, "'You are not authorized to view sequence data.'", 1);

			//regex = new Regex(Regex.Escape("LIGHTCHAIN"), options);
			//outerSelect = regex.Replace(outerSelect, "'You are not authorized to view sequence data.'", 1);

			//regex = new Regex(Regex.Escape("CHAIN"), options);
			//outerSelect = regex.Replace(outerSelect, "'You are not authorized to view sequence data.'", 1);

			//regex = new Regex(Regex.Escape("ALL_SEQUENCES"), options);
			//outerSelect = regex.Replace(outerSelect, "'You are not authorized to view sequence data.'", 1);

			var regex = new Regex(Regex.Escape("SEQUENCEVALUE"), options);
			outerSelect = regex.Replace(outerSelect, "'You are not authorized to view sequence data.'", 1);

			var endingSql = outerSelect + " FROM " + remainingSql;

			return endingSql;
		}


		/// <summary>
		/// Edit sql to remove MolSmiles elements for security and faster performance
		/// </summary>
		/// <param name="sql"></param>
		/// <returns></returns>

		public static string RemoveSelectMolSmilesSql(string sql)
		{
			string colExpr = "m2.molsmiles";
			string tableExpr = ", mbs_owner.corp_moltable_mx m2"; // raw form
			string tableExpr2 = ", mbs_owner.corp_moltable_mx m2"; // real SQL form
			string criteriaExpr = "and m2.corp_nbr (+) = m.corp_nbr";

			if (!Lex.Contains(sql, colExpr) ||
				(!Lex.Contains(sql, tableExpr) && !Lex.Contains(sql, tableExpr2)) ||
				!Lex.Contains(sql, criteriaExpr)) return sql; // just return if substitution vars not present

			sql = Lex.Replace(sql, colExpr, "null molsmiles");
			sql = Lex.Replace(sql, tableExpr, " ");
			sql = Lex.Replace(sql, tableExpr2, " ");
			sql = Lex.Replace(sql, criteriaExpr, " ");

			return sql;
		}

		/// <summary>
		/// StructureAccessNotAuthorizedChimestring
		/// </summary>

		public static string StructureAccessNotAuthorizedChimeString
		{
			get { return MolfileStringToChimeString(StructureAccessNotAuthorizedMolfile); }
		}

		/// <summary>
		/// StructureAccessNotAuthorizedMolfile
		/// </summary>

		public static string StructureAccessNotAuthorizedMolfile
		{ get { return GetTextMessageMolFile("Structure Access Not Authorized"); } }

		/// <summary>
		///  Create a minimal molfile that contains a text message
		/// </summary>

		public static string GetTextMessageMolFile(string textMessage)
		{
			string molfileTemplate =
@"
  SMMXDraw04191313262D

  1  0  0  0  0  0  0  0  0  0999 V2000
    2.3125   -1.8125    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
G    1  0
<text>
M  STY  1   1 SUP
M  SLB  1   1   1
M  SAL   1  1   1
M  SMT   1 <text>
M  END
";

			string text = molfileTemplate.Replace("<text>", textMessage);
			return text;
		}

		/// <summary>
		/// No Structure molfile
		/// </summary>

		public static string NoStructureMolfile = "\n  -OEChem-11191013402D\n\n  0  0  0     0  0  0  0  0  0999 V2000\nM  END\n$$$$\n";

	}

	/// <summary>
	/// Memory-cached set of structures for current session keyed on
	/// normalized compound id and stored as Chime/Helm strings
	/// User-compound database cids should not be cached because of their structures may change during a session
	/// </summary>

	public class MoleculeCache
	{
		public static bool Enabled = true; // cache enabled flag

		/// <summary>
		/// Add or replace
		/// </summary>
		/// <param name="cid"></param>
		/// <param name="cs"></param>

		public static void AddMolecule(
			string cid,
			MoleculeMx cs)
		{
			if (!Enabled) return;

			MoleculeFormat format = cs.PrimaryFormat;
			string value = cs.PrimaryValue;

			if (format == MoleculeFormat.Molfile) // compress molfiles to chime
			{
				format = MoleculeFormat.Chime;
				value = cs.ChimeString;
			}

			CompactMolecule cm = new CompactMolecule(format, value);
			string key = "CidToMolecule(" + cid + ")";

			CacheMx<CompactMolecule>.Add(key, cm);
			return;
		}

		/// <summary>
		/// Check for cid in cache
		/// </summary>
		/// <param name="cid"></param>
		/// <returns></returns>

		public static bool Contains(string cid)
		{
			return CacheMx<CompactMolecule>.Contains(GetKey(cid));
		}

		/// <summary>
		/// Get structure from cache if it exists
		/// </summary>
		/// <param name="cid"></param>
		/// <returns></returns>

		public static MoleculeMx Get(string cid)
		{
			CompactMolecule cm;

			if (!Enabled) return null;

			if (CacheMx<CompactMolecule>.TryGetValue(GetKey(cid), out cm))
			{
				MoleculeMx mol = new MoleculeMx(cm.MolFormat, cm.MolString);
				return mol;
			}

			else return null;
		}

		/// <summary>
		/// Get key for molecule cache
		/// </summary>

		public static string GetKey(string cid)
		{
			string key = "CidToMolecule(" + cid + ")";
			return key;
		}

	} // MoleculeCache

	/// <summary>
	/// Compact molecule that contains just a Format and Value string
	/// </summary>

	public class CompactMolecule
	{
		public MoleculeFormat MolFormat = MoleculeFormat.Unknown;
		public string MolString = null;

		public CompactMolecule(MoleculeFormat format, string value)
		{
			MolFormat = format;
			MolString = value;
			return;
		}

		public CompactMolecule(MoleculeMx mol)
		{
			AssertMx.IsNotNull(mol, "MoleculeMx");
			MolFormat = mol.PrimaryFormat;
			MolString = mol.PrimaryValue;
			return;
		}

		public string Serialize()
		{
			string s = MolFormat.ToString() + "=" + MolString;
			return s;
		}

		public static CompactMolecule Deserialize(string serializedForm)
		{
			MoleculeFormat format;

			int i1 = serializedForm.IndexOf("=");
			if (i1 <= 0 || i1 + 1 >= serializedForm.Length) return null;
			string s = serializedForm.Substring(0, i1);
			string molString = serializedForm.Substring(i1 + 1);

			if (!Enum.TryParse<MoleculeFormat>(s, true, out format))
				return null;

			CompactMolecule cm = new CompactMolecule(format, molString);
			return cm;
		}
	}

	/// <summary>
	/// Parsed Structure Criteria
	/// </summary>
	/// 
	/// Examples:
	///	SSS(structCol,'chime') = 1 /* Substructure search */
	/// MolSim(structCol,'chime','normal | sub | super) >= sim-score /* Similarity search */
	///

	public class ParsedStructureCriteria
	{
		public QueryColumn QueryColumn; // query column being searched
		public StructureSearchType SearchType;
		public StructureSearchType SearchTypeUnion; // union of search types if multiple
		public MoleculeMx Molecule;
		public SimilaritySearchType SimilarityType; // type of similarity if sim search
		public double MinimumSimilarity = MoleculeMx.DefaultMinSimScore; // lower sim limit
		public string Cid = ""; // compound id associated with search if any
		public int MaxSimHits = MoleculeMx.DefaultMaxSimHits; // max number of sim hits
		public string Options = ""; // search options 
		public SmallWorldPredefinedParameters SmallWorldParameters = null;
		public bool Highlight = true; // highlight structure match (SS)
		public bool Align = false; // align matching structures (SS)

		public bool IsChemistrySearch
		{
			get
			{
				if (SearchType == StructureSearchType.FullStructure ||
				 SearchType == StructureSearchType.Substructure ||
				 (SearchType == StructureSearchType.MolSim && SimilarityType != SimilaritySearchType.ECFP4))
					return true;

				else return false;
			}
		}

		public bool Exact
		{
			get
			{
				if (SearchType == StructureSearchType.FullStructure &&
				 Lex.Eq(Options, FullStructureSearchType.Exact))
					return true;
				else return false;
			}

			set
			{
				SearchType = StructureSearchType.FullStructure;
				Options = FullStructureSearchType.Exact;
			}
		}

		public bool Tautomer
		{
			get
			{
				if (SearchType == StructureSearchType.FullStructure &&
				 Lex.Eq(Options, FullStructureSearchType.Tautomer))
					return true;
				else return false;
			}

			set
			{
				SearchType = StructureSearchType.FullStructure;
				Options = FullStructureSearchType.Tautomer;
			}
		}

		public bool Isomer
		{
			get
			{
				if (SearchType == StructureSearchType.FullStructure &&
				 Lex.Eq(Options, FullStructureSearchType.Isomer))
					return true;
				else return false;
			}

			set
			{
				SearchType = StructureSearchType.FullStructure;
				Options = FullStructureSearchType.Isomer;
			}
		}

		public bool Parent
		{
			get
			{
				if (SearchType == StructureSearchType.FullStructure &&
				 Lex.Eq(Options, FullStructureSearchType.Fragment))
					return true;
				else return false;
			}

			set
			{
				SearchType = StructureSearchType.FullStructure;
				Options = FullStructureSearchType.Fragment;
			}
		}

		/// <summary>
		/// Try to parse criteria string from QueryColumn
		/// </summary>
		/// <param name="qc"></param>
		/// <param name="psc"></param>
		/// <returns></returns>

		public static bool TryParse(QueryColumn qc, out ParsedStructureCriteria psc)
		{
			psc = null;

			try
			{
				psc = Parse(qc);
				return (psc != null);
			}

			catch (Exception ex)
			{
				return false;
			}
		}


		/// <summary>
		/// Parse criteria string from QueryColumn
		/// </summary>
		/// <param name="criteria"></param>
		/// <returns></returns>

		public static ParsedStructureCriteria Parse(QueryColumn qc)
		{
			ParsedSingleCriteria pc = MqlUtil.ParseQueryColumnCriteria(qc);
			if (pc == null)
			{
				pc = new ParsedSingleCriteria();
				pc.QueryColumn = qc;
			}
			ParsedStructureCriteria psc = ConvertFromPscToPssc(pc);
			return psc;
		}

		/// <summary>
		/// Parse criteria string
		/// </summary>
		/// <param name="criteria"></param>
		/// <returns></returns>

		public static ParsedStructureCriteria Parse(string criteria)
		{
			ParsedSingleCriteria pc = MqlUtil.ParseSingleCriteria(criteria);
			ParsedStructureCriteria psc = ConvertFromPscToPssc(pc);
			return psc;
		}

		/// <summary>
		/// Convert a ParsedSingleCriteria to a ParsedStructureCriteria
		/// </summary>
		/// <param name="psc"></param>
		/// <returns></returns>

		public static ParsedStructureCriteria ConvertFromPscToPssc(
			ParsedSingleCriteria psc)
		{
			string tok;

			if (psc == null || psc.QueryColumn == null || psc.QueryColumn.MetaColumn.DataType != MetaColumnType.Structure)
				return null;

			ParsedStructureCriteria pssc = new ParsedStructureCriteria();

			pssc.QueryColumn = psc.QueryColumn;

			if (String.IsNullOrEmpty(psc.Value)) { } // structure supplied?

			pssc.Molecule = new MoleculeMx(psc.Value); // just use value as is, assume Chime

			//if (pssc.Structure.Type == StructureFormat.Unknown) pssc = pssc; // debug

			string options = Lex.RemoveSingleQuotes(psc.Value2).Trim();

			if (Lex.Eq(psc.Op, "SSS"))
			{
				pssc.SearchType = StructureSearchType.Substructure;
				pssc.Highlight = !Lex.Contains(options, "Highlight=false"); // default to true
				pssc.Align = Lex.Contains(options, "Align=true"); // default to false
			}

			else if (Lex.Eq(psc.Op, "MSimilar") || Lex.Eq(psc.Op, "Molsim"))
			{
				pssc.SearchType = StructureSearchType.MolSim;
				pssc.SimilarityType = SimilaritySearchType.Normal; // default
				pssc.MinimumSimilarity = .75;
				pssc.MaxSimHits = -1;

				try
				{
					string[] sa;
					if (options.Contains(",")) // options may be separated by a comma or space
						sa = options.Split(',');
					else sa = options.Split(' ');

					EnumUtil.TryParse(sa[0], out pssc.SimilarityType);
					if (sa.Length >= 2)
					{
						double.TryParse(sa[1], out pssc.MinimumSimilarity);

						if (pssc.MinimumSimilarity > 10) pssc.MinimumSimilarity = pssc.MinimumSimilarity / 100; // convert old to new value
					}
					if (sa.Length >= 3) int.TryParse(sa[2], out pssc.MaxSimHits);
				}
				catch (Exception ex) { pssc = pssc; }
			}

			else if (Lex.Eq(psc.Op, "FullStructure") || Lex.Eq(psc.Op, "FSS"))
			{
				pssc.SearchType = StructureSearchType.FullStructure;
				pssc.Options = options;
			}

			else if (psc.Op == "=") // exact match
			{
				pssc.SearchType = StructureSearchType.FullStructure;
				pssc.Options = FullStructureSearchType.Exact; // all parameters turned on
			}

			else if (Lex.Eq(psc.Op, "Isomer"))
			{
				pssc.SearchType = StructureSearchType.FullStructure;
				pssc.Options = FullStructureSearchType.Isomer;
			}

			else if (Lex.Eq(psc.Op, "Tautomer"))
			{
				pssc.SearchType = StructureSearchType.FullStructure;
				pssc.Options = FullStructureSearchType.Tautomer;
			}

			else if (Lex.Eq(psc.Op, "Parent"))
			{
				pssc.SearchType = StructureSearchType.FullStructure;
				pssc.Options = FullStructureSearchType.Fragment;
			}

			else if (psc.Op == ">=")
			{ // special "general search" exact & fragment search  
				pssc.SearchType = StructureSearchType.FullStructure;
				pssc.Options = FullStructureSearchType.Tautomer;
			}

			else if (Lex.Eq(psc.Op, "SmallWorld"))
			{
				pssc.SearchType = StructureSearchType.SmallWorld;
				SmallWorldPredefinedParameters swp;
				if (!SmallWorldPredefinedParameters.TryParse(options, out swp))
					swp = new SmallWorldPredefinedParameters(); // assign default in case of parse fail

				pssc.SmallWorldParameters = swp;
			}

			else if (Lex.Eq(psc.Op, "RelatedSS"))
			{
				pssc.SearchType = StructureSearchType.Related;
				ParseRelatedSSParms(options, pssc);
			}

			return pssc;
		}

		/// <summary>
		/// ParseRelatedSSParms
		/// </summary>
		/// <param name="parmString"></param>
		/// <param name="psc"></param>

		static void ParseRelatedSSParms(
			string parmString,
			ParsedStructureCriteria psc)
		{
			bool b;
			string cid;
			LexParmParser p = new LexParmParser(parmString, ",");

			psc.SearchTypeUnion = 0;

			if (p.GetParm("Cid", out cid)) psc.Cid = cid;

			if (p.GetBoolParm("AltForms", out b) && b) psc.SearchTypeUnion |= StructureSearchType.FullStructure;
			if (p.GetBoolParm("MatchedPairs", out b) && b) psc.SearchTypeUnion |= StructureSearchType.MatchedPairs;
			if (p.GetBoolParm("SmallWorld", out b) && b) psc.SearchTypeUnion |= StructureSearchType.SmallWorld;
			if (p.GetBoolParm("Similar", out b) && b) psc.SearchTypeUnion |= StructureSearchType.MolSim;
			if (p.GetBoolParm("Substructure", out b) && b) psc.SearchTypeUnion |= StructureSearchType.Substructure;

			if (!p.GetDoubleParm("MinSim", out psc.MinimumSimilarity)) psc.MinimumSimilarity = .75;
			if (!p.GetIntParm("MaxHits", out psc.MaxSimHits)) psc.MaxSimHits = 100;

			if (!p.GetBoolParm("Hilight", out psc.Highlight)) psc.Highlight = true;
			if (!p.GetBoolParm("Align", out psc.Highlight)) psc.Align = true;

			return;
		}

		public string FormatRelatedSSParms()
		{
			string options = "";

			if (Lex.IsDefined(Cid)) options += "Cid=" + Cid + ",";

			if (SST.IsFull(SearchTypeUnion)) options += "AltForms,";
			if (SST.IsMmp(SearchTypeUnion)) options += "MatchedPairs,";
			if (SST.IsSw(SearchTypeUnion)) options += "SmallWorld,";
			if (SST.IsSim(SearchTypeUnion)) options += "Similar,";
			if (SST.IsSSS(SearchTypeUnion)) options += "Substructure,";

			//			options += "minsim=.80,maxhits=20,hilight,align";
			options += "MinSim=" + MinimumSimilarity + ",";
			options += "MaxHits=" + MaxSimHits + ",";

			options += "Hilight=" + Highlight + ",";
			options += "Align=" + Align + ",";

			return options;
		}

		/// <summary>
		/// Format criteria for associated query column
		/// </summary>

		public void ConvertToQueryColumnCriteria(
			QueryColumn qc)
		{
			//
			// The following qc.Criteria values are produced:
			//  1. "MOLSTRUCTURE SSS SQUERY" - Substructure match
			//  2. "MOLSTRUCTURE = SQUERY" - Exact match
			//  3. "MOLSTRUCTURE ISOMER SQUERY" - Isomer search
			//  4. "MOLSTRUCTURE TAUTOMER SQUERY" - Tautomer search
			//  5. "MOLSTRUCTURE PARENT SQUERY" - Parent, remove salt(s) & match
			//  7. "MOLSTRUCTURE MSIMILAR SQUERY 'SUB 95'" - Similarity search
			//  8. "MOLSTRUCTURE SMALLWORLD SQUERY 'preset=SmallWorld;hilight=True;align=True;db=Corp;dist=0-4;maxhits=20;matrix=base;tdn=0-4;tup=0-4;rdn=0-4;rup=0-4;ldn=0-4;lup=0-4;maj=0-4;min=0-4;hyb=0-4;sub=0-4'" - SmallWorld search
			//  9. "MOLSTRUCTURE RELATEDSS SQUERY 'AltForms,MatchedPairs,SmallWorld,Similar,minsim=.80,maxhits=20,hilight,align'" - Related Structure Search Types
			//

			string parms = "", op, txt, tok;
			string criteria = "";
			string criteriaDisplay = "";

			MoleculeMx cs = Molecule;
			if (cs == null) cs = new MoleculeMx();

			if (SearchType == StructureSearchType.Unknown) { } // nothing defined

			else if (SearchType == StructureSearchType.Substructure)
			{
				criteriaDisplay = "Substructure search (SSS)";
				criteria = "MOLSTRUCTURE SSS SQUERY";
				if (!Highlight || Align) // include if non-defaults
				{
					parms = Lex.AddSingleQuotes("Highlight=" + Highlight + ";Align=" + Align);
					criteria += " " + parms;
				}
			}

			else if (SearchType == StructureSearchType.FullStructure)
			{
				op = "=";

				if (Isomer) op = "Isomer";
				else if (Tautomer) op = "Tautomer";
				else if (Parent) op = "Parent";

				tok = op;
				if (tok == "=") tok = "Exact"; // substitute for display
				criteriaDisplay = "Full structure search (" + tok;
				if (parms != "") criteriaDisplay += " " + parms;
				criteriaDisplay += ")";

				criteria = "MOLSTRUCTURE " + op + " SQUERY " + parms;
			}

			else if (SearchType == StructureSearchType.MolSim)
			{
				parms = "";
				op = "MSIMILAR";
				parms = SimilarityType.ToString() + " ";
				parms += MinimumSimilarity; // * 100; // minsim is 2nd parm (convert 0-1 range to 0-100)

				if (MaxSimHits > 0) // max hits is 3rd parm if exists
					parms += " " + MaxSimHits;

				parms = Lex.AddSingleQuotes(parms);

				criteriaDisplay = "Similarity search (" + parms + ")";
				criteria = "MOLSTRUCTURE " + op + " SQUERY " + parms;
			}

			else if (SearchType == StructureSearchType.SmallWorld)
			{
				SmallWorldPredefinedParameters swp = SmallWorldParameters;
				if (swp == null) swp = new SmallWorldPredefinedParameters();
				op = "SMALLWORLD";
				parms = swp.Serialize();
				parms = Lex.AddSingleQuotes(parms);

				txt = "SmallWorld, Distance";
				if (swp.Distance.Low <= 0) txt += " <= ";
				else txt += " = " + swp.Distance.Low.ToString() + "-";
				txt += swp.Distance.High.ToString();
				criteriaDisplay = txt;

				criteria = "MOLSTRUCTURE " + op + " SQUERY " + parms;
			}

			else if (SearchType == StructureSearchType.Related)
			{
				op = "RELATEDSS";
				parms = FormatRelatedSSParms();
				parms = Lex.AddSingleQuotes(parms);
				criteria = "MOLSTRUCTURE " + op + " SQUERY " + parms;

				criteriaDisplay = "Related, Dist. <= " + String.Format("{0:0.00}", MinimumSimilarity);
			}

			QueryColumn = qc;
			qc.CriteriaDisplay = criteriaDisplay;
			qc.Criteria = criteria;
			qc.MolString = cs.GetPrimaryTypeAndValueString();

			return;
		}

		/// <summary>
		/// Close
		/// </summary>
		/// <returns></returns>

		public ParsedStructureCriteria Clone()
		{
			return (ParsedStructureCriteria)this.MemberwiseClone();
		}

	} // ParsedStructureCriteria


	/// <summary>
	/// SmallWorldParameters
	/// </summary>

	public class SmallWorldPredefinedParameters
	{
		public string PresetName = "SmallWorld"; // Name of preset used to initialize parameters
		public bool Highlight = true; // highlight differences via coloring of atoms/bonds (color by default)
		public bool Align = true; // align orientation of matching structures (align by default)
															//		public bool DelayHilight = false; // if true just return unhilighted smiles initially

		public string Smiles = "";
		public RootTable RootTable; // associated root table if any
		public string Database = ""; // list of one or more database names to search
		public string Route = "normal"; // Route SmallWorld network: normal, full, exhaustive (default:normal)

		public RangeParm Distance = new RangeParm(0, 4); // Overall distance. Sum of topological and "color" edits 
		public int MaxHits = DefaultMaxHits; // max number of hits

		public const int DefaultMaxHits = 100; // default max number of hits

		// Topological Edits

		public RangeParm TerminalUp = new RangeParm(-1, -1); // tup
		public RangeParm TerminalDown = new RangeParm(-1, -1); // tdn
		public RangeParm RingUp = new RangeParm(-1, -1); // rup
		public RangeParm RingDown = new RangeParm(-1, -1); // rdn
		public RangeParm LinkerUp = new RangeParm(-1, -1); // lup
		public RangeParm LinkerDown = new RangeParm(-1, -1); // ldn

		// Atom type scoring ("Color" Edits)

		public RangeParm MutationMajor = new RangeParm(-1, -1); // Major Transmutation (maj)
		public RangeParm MutationMinor = new RangeParm(-1, -1); // Minor Transmutation (min)
		public RangeParm SubstitutionRange = new RangeParm(-1, -1); // Hydrogen Substitution (sub)
		public RangeParm HybridisationChange = new RangeParm(-1, -1); // Hybridisation Change (hyb)

		public string TransmutationMatrix = "base"; // base - includes atom type scoring, none - no atom type scoring

		public bool MatchAtomTypes // if true apply atom type scoring
		{
			get
			{
				return Lex.Eq(TransmutationMatrix, "base");
			}
			set
			{
				if (value == true) TransmutationMatrix = "base";
				else TransmutationMatrix = "none";
			}
		}

		public static string DefaultDatabase = "Corp";

		/// <summary>
		/// Compare parm values
		/// </summary>
		/// <param name="swp1"></param>
		/// <param name="swp2"></param>
		/// <returns></returns>

		public static bool Equals(
			SmallWorldPredefinedParameters swp1,
			SmallWorldPredefinedParameters swp2)
		{
			if (swp1 == null || swp2 == null) return false;
			return swp1.Equals(swp2);
		}

		/// <summary>
		/// Compare parm values (for light use only, not efficient)
		/// </summary>
		/// <param name="swp2"></param>
		/// <returns></returns>
		/// 

		public bool Equals(
			SmallWorldPredefinedParameters swp2)
		{
			if (swp2 == null) return false;

			SmallWorldPredefinedParameters swp1b = this.Clone();
			SmallWorldPredefinedParameters swp2b = swp2.Clone();

			swp1b.Smiles = swp1b.Database = swp1b.PresetName = "";
			string swp1s = swp1b.Serialize();

			swp2b.Smiles = swp2b.Database = swp2b.PresetName = "";
			string swp2s = swp2b.Serialize();

			return Lex.Eq(swp1s, swp2s);
		}

		/// <summary>
		/// Deserialize
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>

		public static SmallWorldPredefinedParameters Deserialize(string s)
		{
			SmallWorldPredefinedParameters swp;

			if (TryParse(s, out swp)) return swp;
			else return null;
		}

		/// <summary>
		/// Try parse
		/// </summary>
		/// <param name="s"></param>
		/// <param name="p"></param>
		/// <returns></returns>

		public static bool TryParse(string s, out SmallWorldPredefinedParameters p)
		{
			string pn, v1, v2;
			int i0, i1, i2;

			p = new SmallWorldPredefinedParameters();

			string[] sa = s.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string s2 in sa)
			{
				i0 = s2.IndexOf("=");
				if (i0 < 0) throw new Exception("Missing '=' in: " + s);
				pn = s2.Substring(0, i0).Trim();
				//if (Lex.Eq(pn, "sub")) pn = pn; // debug
				v1 = s2.Substring(i0 + 1).Trim();
				v2 = "";
				i1 = i2 = -1;
				if (v1.Contains("-")) // if range value, break it down
				{
					string[] sa3 = v1.Split('-');
					if (Lex.IsInteger(sa3[0]) && Lex.IsInteger(sa3[1])) // integer range?
					{
						v1 = sa3[0].Trim();
						v2 = sa3[1].Trim();
					}
				}

				if (!int.TryParse(v1, out i1))
					i1 = -1;

				if (v2 == "") i2 = i1; // if 2nd value not defined then set same as first
				else if (!int.TryParse(v2, out i2))
					i2 = -1;

				if (Lex.Eq(pn, "preset"))
					p.PresetName = v1;

				else if (Lex.Eq(pn, "hilight"))
					bool.TryParse(v1, out p.Highlight);

				else if (Lex.Eq(pn, "align"))
					bool.TryParse(v1, out p.Align);

				//else if (Lex.Eq(pn, "delayHilight"))
				//	bool.TryParse(v1, out p.DelayHilight);

				else if (Lex.Eq(pn, "smi"))
					p.Smiles = v1;

				else if (Lex.Eq(pn, "db"))
					p.Database = v1;

				else if (Lex.Eq(pn, "maxHits"))
					p.MaxHits = i1;

				else if (Lex.Eq(pn, "matrix"))
					p.TransmutationMatrix = v1;

				else if (IsParm(pn, "dist", p.Distance, i1, i2)) continue;
				else if (IsParm(pn, "tup", p.TerminalUp, i1, i2)) continue;
				else if (IsParm(pn, "tdn", p.TerminalDown, i1, i2)) continue;
				else if (IsParm(pn, "rup", p.RingUp, i1, i2)) continue;
				else if (IsParm(pn, "rdn", p.RingDown, i1, i2)) continue;
				else if (IsParm(pn, "lup", p.LinkerUp, i1, i2)) continue;
				else if (IsParm(pn, "ldn", p.LinkerDown, i1, i2)) continue;

				else if (IsParm(pn, "maj", p.MutationMajor, i1, i2)) continue;
				else if (IsParm(pn, "min", p.MutationMinor, i1, i2)) continue;
				else if (IsParm(pn, "sub", p.SubstitutionRange, i1, i2)) continue;
				else if (IsParm(pn, "hyb", p.HybridisationChange, i1, i2)) continue;
				else throw new Exception("Unrecognized parameter: " + pn);
			}

			return true;
		}

		static bool IsParm(
			string parmNameArg,
			string parmName,
			RangeParm r,
			int low,
			int high)
		{
			if (!Lex.Eq(parmNameArg, parmName)) return false;

			r.Low = low;
			r.High = high;
			return true;
		}


		/// <summary>
		/// Serialize
		/// </summary>
		/// <returns></returns>

		public string Serialize()
		{
			string s = "";
			string delim = "";

			if (Lex.IsDefined(PresetName))
				s += ";preset=" + PresetName;

			//if (Highlight)
			s += ";hilight=" + Highlight;

			//if (Align)
			s += ";align=" + Align;

			//if (DelayHilight)
			//	s += ";delayHilight=" + DelayHilight;

			if (Lex.IsDefined(Smiles))
				s += ";smi=" + Smiles;

			if (Lex.IsDefined(Database))
				s += ";db=" + Database;

			s +=
				Format("dist", Distance) +
				";maxhits=" + MaxHits +
				";matrix=" + TransmutationMatrix +

				Format("tdn", TerminalDown) +
				Format("tup", TerminalUp) +
				Format("rdn", RingDown) +
				Format("rup", RingUp) +
				Format("ldn", LinkerDown) +
				Format("lup", LinkerUp) +
				Format("maj", MutationMajor) +
				Format("min", MutationMinor) +
				Format("hyb", HybridisationChange) +
				Format("sub", SubstitutionRange);

			if (s.StartsWith(";")) s = s.Substring(1);

			return s;
		}

		string Format(
			string parmName,
			RangeParm range)
		{
			if (range == null || range.Low < 0 || range.High < 0) return "";
			else return ";" + parmName + "=" + range.Format();
		}

		/// <summary>
		/// Clone1
		/// </summary>
		/// <returns></returns>

		public SmallWorldPredefinedParameters Clone()
		{
			string s = Serialize();
			SmallWorldPredefinedParameters swp = Deserialize(s);
			return swp;
		}

	}

	/// <summary>
	/// Misc SmallWorld Data
	/// </summary>

	public class SmallWorldData
	{

		/// <summary>
		/// Get index into SVG array for depict setting
		/// Color Align Value
		/// ----- ----- -----
		/// false false     0
		/// false true      1
		/// true  false     2
		/// true  true      3
		/// 
		/// </summary>
		/// <param name="color"></param>
		/// <param name="align"></param>
		/// <returns></returns>

		public static int GetSvgOptionsIndex(
			bool color,
			bool align)
		{
			return (color ? 1 : 0) * 2 + (align ? 1 : 0);
		}

	}

	/// <summary>
	/// SmallWorldSearchType enum
	/// </summary>
	public enum SmallWorldSearchType
	{
		Unknown = 0,
		SmallWorld = 1,
		MCS = 2,
		Substructure = 3,
		Superstructure = 4,
		Bemis_Murcko_Framework = 5,
		Element_Graph = 6
	}

	/// <summary>
	/// Simple integer Up/Down value pair range parameter class 
	/// </summary>
	public class RangeParm
	{
		public int Low = -1;
		public int High = -1;
		public bool Active = true; // is active for the current "atom matching" setting
		public bool Enabled = true; // enabled for the current search type setting

		public RangeParm()
		{
			return;
		}

		public RangeParm(int low, int high)
		{
			Low = low;
			High = high;
			return;
		}

		public string Format()
		{
			if (Low < 0) return ""; // range not defined

			string s = Low.ToString();
			if (High >= 0)
				s += "-" + High.ToString();

			return s;
		}
	}

	/// <summary>
	/// Info on a structure that is a match in a structure search
	/// </summary>

	public class StructSearchMatch
	{
		public StructureSearchType SearchType; // type of structure search
		public int SearchSubtype = -1;
		public int SrcDbId = -1; // database source Id
		public string SrcCid; // source compound id
		public string MolString; // matching structure string
		public MoleculeFormat MolStringFormat; // matching structure type
		public float MatchScore; // similarity or other match score
		public string GraphicsString; // optional additional structure rendering information (e.g. SmallWorld SVG)

		public static int CorpDbId = 0;
		public static int ChemblDbId = 1;

		public string SrcName // Source Name
		{
			get { return SrcIdToName(SrcDbId); }
		}

		public string SearchTypeName
		{
			get
			{
				string name = StructureSearchTypeUtil.StructureSearchTypeToExternalName(SearchType);

				if (SearchType == StructureSearchType.FullStructure)
				{
					if (SearchSubtype == 0) name = "Query Structure"; // Query structure
					else if (SearchSubtype == 1) name = "Exact";
					else if (SearchSubtype == 2) name = "Fragment";
					else if (SearchSubtype == 3) name = "Isomer";
					else if (SearchSubtype == 4) name = "Tautomer";
				}

				return name;
			}
		}

		/// <summary>
		/// Code used to ordering of results by type "priority"
		/// </summary>
		public int SearchTypeOrderCode
		{
			get
			{
				int code = 999;

				if (SearchType == StructureSearchType.FullStructure)
					code = 100 + SearchSubtype;

				else if (SearchType == StructureSearchType.Substructure)
					code = 200;

				else if (SearchType == StructureSearchType.MatchedPairs)
					code = 300;

				else if (SearchType == StructureSearchType.SmallWorld)
					code = 400;

				else if (SearchType == StructureSearchType.MolSim)
					code = 500;

				return code;
			}
		}

		/// <summary>
		/// Convert source name to source id  (todo: make complete via Unichem)
		/// </summary>
		/// <param name="srcId"></param>
		/// <returns></returns>

		public static string SrcIdToName(
			int srcId)
		{
			if (srcId == CorpDbId) return "Corp";
			else if (srcId == ChemblDbId) return "ChEMBL";
			else return srcId.ToString();
		}

		/// <summary>
		/// Convert source id to source name  (todo: make complete via Unichem)
		/// </summary>
		/// <param name="srcName"></param>
		/// <returns></returns>

		public static int SrcNameToId(
			string srcName)
		{
			if (Lex.Contains(srcName, "Corp")) return CorpDbId;
			else if (Lex.Contains(srcName, "ChEMBL")) return ChemblDbId;
			else return -1;
		}

		/// <summary>
		/// Serialize
		/// </summary>
		/// <returns></returns>

		public string Serialize()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(SearchType.ToString());
			sb.Append('\t');
			sb.Append(SearchSubtype.ToString());
			sb.Append('\t');
			sb.Append(SrcDbId.ToString());
			sb.Append('\t');
			sb.Append(SrcCid);
			sb.Append('\t');
			sb.Append(MolString);
			sb.Append('\t');
			sb.Append(MolStringFormat.ToString());
			sb.Append('\t');
			sb.Append(MatchScore.ToString());

			if (Lex.IsDefined(GraphicsString))
			{
				sb.Append('\t');
				string svg = GraphicsString;
				if (svg.Contains("\t")) svg = svg.Replace("\t", " "); // remove any existing tabs and new lines
				if (svg.Contains("\n")) svg = svg.Replace("\n", " ");
				sb.Append(svg);
			}

			return sb.ToString();
		}

		/// <summary>
		/// Deserialize
		/// </summary>
		/// <param name="content"></param>
		/// <returns></returns>

		static public StructSearchMatch Deserialize
			(string content)
		{
			StructSearchMatch ssm = new StructSearchMatch();
			string[] sa = content.Split('\t');
			if (sa.Length < 7) return ssm;

			EnumUtil.TryParse(sa[0], out ssm.SearchType);
			int.TryParse(sa[1], out ssm.SearchSubtype);
			int.TryParse(sa[2], out ssm.SrcDbId);
			ssm.SrcCid = sa[3];
			ssm.MolString = sa[4];
			EnumUtil.TryParse(sa[5], out ssm.MolStringFormat);
			float.TryParse(sa[6], out ssm.MatchScore);

			if (sa.Length >= 8)
				ssm.GraphicsString = sa[7];

			return ssm;
		}

		/// <summary>
		/// Compare to match entries for sorting by match type, score and database name precedence
		/// </summary>
		/// <param name="o1"></param>
		/// <param name="o2"></param>
		/// <returns></returns>

		public static int CompareByMatchQuality(
			object o1,
			object o2)
		{
			double score1, score2;
			int cv;

			if (o1 is StructSearchMatch && o2 is StructSearchMatch)
			{
				StructSearchMatch ssm1 = o1 as StructSearchMatch;
				StructSearchMatch ssm2 = o2 as StructSearchMatch;

				// Compare first on search type (asc)

				cv = Math.Sign(ssm1.SearchTypeOrderCode - ssm2.SearchTypeOrderCode);
				if (cv != 0) return cv;

				// Then on score (desc)

				cv = ssm2.MatchScore.CompareTo(ssm1.MatchScore);
				if (cv != 0) return cv;

				// Then on database (asc)

				cv = ssm1.SrcDbId.CompareTo(ssm2.SrcDbId);
				return cv;
			}

			else
			{
				bool c1 = QualifiedNumber.TryConvertToDouble(o1, out score1);
				bool c2 = QualifiedNumber.TryConvertToDouble(o2, out score2);

				if (c1 && c2)
				{
					cv = -(score1).CompareTo(score2);
					return cv;
				}

				else if (c1 && !c2) return 1; // just o1 has value
				else if (!c1 && c2) return -1; // just o2 has value
				else return 0; // both null
			}
		}

		/// <summary>
		/// Clone
		/// </summary>
		/// <returns></returns>
		public StructSearchMatch Clone()
		{
			StructSearchMatch ssm = (StructSearchMatch)MemberwiseClone();
			return ssm;
		}

	}

}