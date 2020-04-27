using Mobius.ComOps;

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Mobius.Data
{

	/// <summary>
	/// Operations to read and write SDfiles 
	/// </summary>

	public class SdFileDao
	{

		/// <summary>
		/// Read the next record from an sdfile
		/// </summary>
		/// <param name="sr">Stream to read from</param>
		/// <returns></returns>

		public static List<SdFileField> Read (
				StreamReader sr)
		{
			string sdfRec;
			return Read(sr, out sdfRec);
		}

		/// <summary>
		/// Read the next record from an sdfile
		/// </summary>
		/// <param name="sr">Stream to read from</param>
		/// <param name="sdfRec">Return full original record here</param>
		/// <returns></returns>

		public static	List<SdFileField> Read(
				StreamReader sr,
				out string sdfRec)
		{
			sdfRec = null;
			if (sr.EndOfStream) return null;

			List<SdFileField> fList = new List<SdFileField>();
			string rec = null; // current record

			SdFileField nf = null;

			StringBuilder sdFileRec = new StringBuilder(); // full rec built here
			while (true)
			{
				if (!sr.EndOfStream)
				{
					rec = sr.ReadLine();
					sdFileRec.Append(rec);
					sdFileRec.Append("\n");
				}

				if (rec.Trim() == "$$$$" || sr.EndOfStream) break;

				//if (Lex.Contains(rec, "cpd id")) rec = rec; // debug

				bool newfield = false;
				if (fList.Count == 0) newfield = true;
				if (rec.Length > 0 && rec[0] == '>') newfield = true;
				if (newfield)
				{ /* got a new Data field */
					nf = new SdFileField();
					fList.Add(nf);

					if (rec.Length > 0 && rec[0] == '>')
					{
						nf.Header = rec;
						continue;
					}
				}

				if (nf.Data.Length > 0) nf.Data += "\n";
				if (rec.Length == 0) rec = " "; // make record at least 1 space
				nf.Data += rec;
			}

			if (fList.Count == 0) return null;

			if (String.IsNullOrEmpty(fList[0].Header) && fList[0].Data.Contains("V2000"))
			{ // be sure right number of lines in molfile header
				string molFile = fList[0].Data;
				int i1 = molFile.IndexOf("V2000");
				int newlineCount = 0;
				int lastNewline = -1;
				for (int i2 = 0; i2 < molFile.Length; i2++)
				{
					if (molFile[i2] != '\n') continue;

					newlineCount++;
					if (i2 < i1)
					{
						lastNewline = i2;
						continue;
					}

					else
					{
						if (newlineCount != 4)
						{
							molFile = molFile.Substring(lastNewline + 1); // just ignore existing header
							if (molFile.Contains("\r")) molFile = "\r\n\r\n\r\n" + molFile;
							else molFile = "\n\n\n" + molFile;
							fList[0].Data = molFile;
						}

						break;
					}
				}

			}

			return fList;
		}

		/// <summary>
		/// Get a value for a Data field
		/// </summary>
		/// <param name="fList">List of fields</param>
		/// <param name="field">Field to get</param>
        /// <returns></returns>

		public static string GetDataField(
				List<SdFileField> fList,
				string fieldName)
		{
			foreach (SdFileField field in fList)
			{
                string name = fieldName.Trim(new Char[] { ' ' });
                string header = field.Header.Trim(new Char[] { ' ', '<', '>' });
                if (Lex.Eq(name, header))
				{
					string data = field.Data.Trim();
					if (data.EndsWith("\n")) return data.Substring(0, data.Length - 1);
                    else return data; // field.Data;
				}
			}

			return "";
		}

		/// <summary>
		/// Set value for a Data field
		/// </summary>
		/// <param name="fList">Pointer to field list</param>
		/// <param name="field">Name of field to set</param>
		/// <param name="value">Field value</param>

		public static void SetSdFileField( /*  */
				List<SdFileField> fList,
				string fieldName,
				string value)
		{
			// See if field exists 

			foreach (SdFileField field2 in fList)
			{
				if (Lex.Eq(fieldName, field2.Header))
				{
					field2.Data = value;
					if (!field2.Data.EndsWith("\n")) field2.Data += "\n";
					return;
				}
			}

			// Allocate new field

			SdFileField field = new SdFileField();
			field.Header = fieldName;
			field.Data = value;
			if (!field.Data.EndsWith("\n")) field.Data += "\n";

			return;
		}

		/// <summary>
		/// Write a record into an sdfile
		/// </summary>
		/// <param name="sw">File to write to</param>
		/// <param name="fList">Field List</param>

		void Write (
				StreamWriter sw,
				List<SdFileField> fList)
		{
			for (int i1 = 0; i1 < fList.Count; i1++)
			{
				if (i1 > 0) sw.WriteLine(fList[i1].Header); // write header unless 1st entry which is molfile
				sw.WriteLine(fList[i1].Data);
			}
			sw.WriteLine("$$$$");
		}
	}

/// <summary>
/// Header & data for a single SdFileField
/// </summary>

	public class SdFileField
	{
		public string Header = ""; // header for Data or blank for molfile item
		public string Data = ""; // Data entry
	}
}