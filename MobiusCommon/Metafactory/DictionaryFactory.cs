using Mobius.Data;
using Mobius.ComOps;
using Mobius.UAL;

using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Data;
using System.Data.Common;

namespace Mobius.MetaFactoryNamespace
{
	/// <summary>
	/// Summary description for DictionaryFactory.
	/// </summary>
	public class DictionaryFactory : IDictionaryFactory
	{
		Dictionary<string, DictionaryMx> Dictionaries;

		public DictionaryFactory()
		{
			return;
		}

		/// <summary>
		/// Get words and definitions for dictionary, usually from an Oracle database
		/// </summary>
		/// <param name="dict"></param>
		/// <returns></returns>

		public DictionaryMx GetDefinitions(string dictName)
		{
			dictName = dictName.ToLower();

			if (Dictionaries == null) throw new Exception("Dictionary XML not loaded");
			if (!Dictionaries.ContainsKey(dictName)) throw new Exception("Unknown dictionary: " + dictName);

			DictionaryMx dict = Dictionaries[dictName.ToLower()];
			if (dict.Words != null) return dict; // just return if already have
			if (dict.Sql == null || dict.Sql == "") return dict; // empty dictionary

			ReadDictionary(dict);
			return dict;
		}

		/// <summary>
		/// Get words and definitions for dictionary, usually from an Oracle database
		/// </summary>
		/// <param name="dict"></param>
		/// <returns></returns>

		public void GetDefinitions(DictionaryMx dict)
		{
			if (Dictionaries == null) throw new Exception("Dictionary XML not loaded");

			string dictName = dict.Name.ToLower();

			if (dict.Words != null) return; // just return if already have

			dict.Initialize();

			if (dict.Sql == null || dict.Sql == "") return; // empty dictionary
			ReadDictionary(dict);
			return;
		}

		static DictionaryMx ReadDictionary(
			DictionaryMx dict)
		{
			if (!dict.Cache) return ReadDictionaryFromOracle(dict); // normal read from Oracle

			string fileName = ServicesDirs.CacheDir + "\\CachedDictionary." + dict.Name + ".txt";
			if (FileUtil.Exists(fileName))
			{
				DateTime updateTime = FileUtil.GetFileLastWriteTime(fileName);
				if (DateTime.Now.Subtract(updateTime).TotalDays < 1)
				{
					ReadDictionaryFromCacheFile(fileName, dict);
					if (dict.Words != null && dict.Words.Count > 0) return dict;
				}
			}

			ReadDictionaryFromOracle(dict);
			WriteDictionaryToCacheFile(fileName, dict);
			return dict;
		}

		/// <summary>
		/// Read dictionary from Oracle table
		/// </summary>
		/// <param name="dict"></param>
		/// <returns></returns>

		static DictionaryMx ReadDictionaryFromOracle(
			DictionaryMx dict)
		{
			DbCommandMx drd = null;
			int t0, t1, i1;

			try
			{
				//if (Lex.Eq(dict.Name, "DIT_PERSON")) dict = dict; // debug
				DbDataReader dr = null;

				drd = new DbCommandMx();
				drd.Prepare(dict.Sql);
				dr = drd.ExecuteReader();

				t0 = TimeOfDay.Milliseconds();
				while (drd.Read())
				{
					if (!dr.IsDBNull(0))
					{
						string word = dr.GetValue(0).ToString();
						if (Lex.IsNullOrEmpty(word)) continue;
						string definition = null;
						if (!dr.IsDBNull(1)) definition = dr.GetValue(1).ToString();
						dict.Add(word, definition);
						t1 = TimeOfDay.Milliseconds();
						//						if (t1-t0 > 2000) break; // limit time for development
					}
				}
				drd.CloseReader();
				drd.Dispose();

				t1 = TimeOfDay.Milliseconds() - t0;

				//				DebugLog.Message("ReadDictionaryFromOracle " + dict.Name + " Time: " + t1.ToString());
				return dict;
			}
			catch (Exception ex)
			{
				if (drd != null) drd.Dispose();
				return null;
			}
		}

		static bool ReadDictionaryFromCacheFile(
			string fileName,
			DictionaryMx dict)
		{
			StreamReader sr = null;
			string rec, word, definition;
			string[] sa;

			try
			{
				sr = new StreamReader(fileName);
				while (true)
				{
					rec = sr.ReadLine();
					if (rec == null) break;
					sa = rec.Split('\t');
					if (sa.Length != 2) continue;

					word = sa[0];
					if (Lex.IsNullOrEmpty(word)) continue;

					definition = sa[1];
					dict.Add(word, definition);
				}

				sr.Close();

				return true;
			}

			catch (Exception ex)
			{
				try { sr.Close(); } catch { }
				DebugLog.Message(ex.Message);
				return false;
			}
		}

		static bool WriteDictionaryToCacheFile(
			string fileName,
			DictionaryMx dict)
		{
			StreamWriter sw = null;
			string rec, definition;

			try
			{
				if (!Directory.Exists(ServicesDirs.CacheDir))
					Directory.CreateDirectory(ServicesDirs.CacheDir);

				sw = new StreamWriter(fileName);
				foreach (string word in dict.Words)
				{
					definition = dict.LookupDefinition(word);
					if (definition == null) definition = "";
					sw.WriteLine(word + "\t" + definition);
				}

				sw.Close();

				return true;
			}

			catch (Exception ex)
			{
				try { sw.Close(); } catch { }
				DebugLog.Message(ex.Message);
				return false;
			}
		}


		/// <summary>
		/// Load metadata describing dictionaries
		/// </summary>
		/// <param name="dictFileName"></param>
		/// <returns></returns>

		public Dictionary<string, DictionaryMx> LoadDictionaries()
		{
			XmlAttributeCollection atts;

			Dictionaries = new Dictionary<string, DictionaryMx>();

			//UalUtil is assumed to have been initialized
			// and UalUtil.MetaDataDir set
			string dictFileName = ServicesDirs.MetaDataDir + @"\" + "Dictionaries.xml";
			StreamReader sr = new StreamReader(dictFileName);
			XmlDocument doc = new XmlDocument();
			doc.Load(sr);
			XmlNode dictsNode = doc.FirstChild;

			while (dictsNode != null)
			{
				if (dictsNode.NodeType == XmlNodeType.Element) break;
				dictsNode = dictsNode.NextSibling;
				if (dictsNode == null)
					throw new Exception("No initial element found");
			}

			if (!Lex.Eq(dictsNode.Name, "Dictionaries"))
				throw new Exception("Expected Dictionaries node: " + dictsNode.Name);

			XmlNode dictNode = dictsNode.FirstChild;
			while (dictNode != null) // loop through dictionaries
			{
				if (dictNode.NodeType != XmlNodeType.Element) ; // ignore non-elements

				else if (Lex.Eq(dictNode.Name, "Dictionary")) // dictionary element
				{
					DictionaryMx dict = new DictionaryMx();

					atts = dictNode.Attributes;
					for (int i = 0; i < atts.Count; i++)
					{
						XmlNode att = atts.Item(i);

						if (Lex.Eq(att.Name, "Name"))
							dict.Name = att.Value.ToLower();

						else if (Lex.Eq(att.Name, "Sql"))
							dict.Sql = att.Value;

						else if (Lex.Eq(att.Name, "Cache"))
							bool.TryParse(att.Value, out dict.Cache);

						else
							DebugLog.Message("Unexpected Dictionary (" + dict.Name + ") attribute: " + att.Name);

					}

					if (dict.Name == "") throw new Exception("Connection is missing name");
					if (Lex.EndsWith(dict.Name, "_cache")) dict.Cache = true; // alternate way to indicate cached dictionary (avoid unexpected attribute exception in older clients)

					Dictionaries[dict.Name] = dict;

					XmlNode entryNode = dictNode.FirstChild;
					while (entryNode != null) // loop through dict entries
					{
						if (entryNode.NodeType != XmlNodeType.Element) ; // ignore non-elements

						else if (Lex.Eq(entryNode.Name, "Entry")) // word entry
						{
							string word = "";
							string def = "";
							atts = entryNode.Attributes;
							for (int i = 0; i < atts.Count; i++)
							{
								XmlNode att = atts.Item(i);

								if (Lex.Eq(att.Name, "Word"))
									word = att.Value.Trim();

								else if (Lex.Eq(att.Name, "Def") ||
									Lex.Eq(att.Name, "Definition"))
									def = att.Value.Trim();

								else
									DebugLog.Message("Unexpected Dictionary (" + dict.Name + ") entry attribute: " + att.Name);

							}

							if (word == "") throw new Exception("Dictionary entry is missing Word attribute");
							dict.Add(word, def);
						}

						else throw new Exception("Expected Entry element but saw " +
									 dictNode.Name);

						entryNode = entryNode.NextSibling;
					} // end of entry loop

				}

				else throw new Exception("Expected Dictionary element but saw " +
							 dictNode.Name);

				dictNode = dictNode.NextSibling;
			} // end of dictionary loop

			sr.Close();

			return Dictionaries;
		}

	}
}
