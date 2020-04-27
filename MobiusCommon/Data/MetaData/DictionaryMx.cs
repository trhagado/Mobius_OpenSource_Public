using Mobius.ComOps;

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Mobius.Data
{
	/// <summary>
	/// Mobius dictionaries; controlled vocabulary lookup for MetaColumns etc.
	/// </summary>

	public class DictionaryMx
	{
		public static Dictionary<string, DictionaryMx> Dictionaries; // collection of all dictionaries
		public static IDictionaryFactory DictionaryFactory;
		public string Name = ""; // dictionary name
		public List<string> Words = null; // ordered list of words in this dictionary
		public Dictionary<string, string> WordLookup = null; // definitions in this dictionary keyed by lowercase word
		public Dictionary<string, string> DefinitionLookup = null; // words in this dictionary keyed by lowercase definition
		public string Sql = ""; // sql used to generate dictionary
		public bool Cache = false; // if true read this dictionary from cache file and update daily from source (sql)

		public DictionaryMx()
		{
		}

/// <summary>
/// Load the basic dictionary definitions
/// </summary>
/// <param name="factory"></param>

		public static Dictionary<string, DictionaryMx> LoadDictionaries (
			IDictionaryFactory factory)
		{
			DictionaryFactory = factory;
			Dictionaries = factory.LoadDictionaries();
			return Dictionaries;
		}

		/// <summary>
		/// Lookup the definition for a word
		/// </summary>
		/// <param name="dictName"></param>
		/// <param name="word"></param>
		/// <returns></returns>

		public static string LookupDefinition (
			string dictName,
			string word)
		{
			DictionaryMx d = DictionaryMx.Get(dictName);
			if (d==null) return null;
			return d.LookupDefinition(word);
		}

		/// <summary>
		/// Return true if dictionary contains specified word
		/// </summary>
		/// <param name="word"></param>
		/// <returns></returns>

		public bool ContainsWord(
			string word)
		{
			if (LookupDefinition(word) != null)
				return true;
			else return false;
		}

		/// <summary>
		/// Lookup the definition for a word
		/// </summary>
		/// <param name="word"></param>
		/// <returns></returns>

		public string LookupDefinition (
			string word)
		{
			if (Lex.IsNullOrEmpty(word)) return null;
			if (WordLookup == null) return null;
			if (!WordLookup.ContainsKey(word.ToLower())) return null;

			string s = (string)this.WordLookup[word.ToLower()];
			return s;
		}

		/// <summary>
		/// Lookup a word by definition
		/// </summary>
		/// <param name="dictName"></param>
		/// <param name="definition"></param>
		/// <returns></returns>
		public static string LookupWordByDefinition (
			string dictName,
			string definition)
		{
			DictionaryMx d = DictionaryMx.Get(dictName);
			if (d==null) return null;
			return d.LookupWordByDefinition(definition);
		}

/// <summary>
/// Lookup a word by definition
/// </summary>
/// <param name="definition"></param>
/// <returns></returns>
		public string LookupWordByDefinition (
			string definition)
		{
			if (DefinitionLookup == null) return null;
			if (!DefinitionLookup.ContainsKey(definition.ToLower())) return null;

			string s = (string)this.DefinitionLookup[definition.ToLower()];
			return s;
		}

/// <summary>
/// Get a list of all of the words in a dictionary
/// </summary>
/// <param name="dictName"></param>
/// <returns></returns>

		public static List<string> GetWords (
			string dictName,
			bool removeDuplicates)
		{
			DictionaryMx dict = DictionaryMx.Get(dictName);
			if (dict==null) return null;
			if (!removeDuplicates) return dict.Words;

			List<string> words = new List<string>();
			HashSet<string> set = new HashSet<string>();

			foreach (string word in dict.Words)
			{
				if (set.Contains(word.ToLower())) continue;

				words.Add(word);
				set.Add(word.ToLower());
			}

			return words;
		}

		/// <summary>
		/// Get a dictionary
		/// </summary>
		/// <param name="dictName"></param>
		/// <returns></returns>
		/// 
		public static DictionaryMx Get( // lookup or read dictionary and return
			string dictName)
		{
			try
			{
				DictionaryMx d = DictionaryMx.GetWithException(dictName);
				return d;
			}
			catch (Exception ex)
			{
				return null;
			}

		}

/// <summary>
/// Get a dictionary
/// </summary>
/// <param name="dictName"></param>
/// <returns></returns>

		public static DictionaryMx GetWithException( // lookup or read dictionary and return
			string dictName)
		{
			string sql,tok;

			//if (Lex.Eq(dictName, "DIT_PERSON")) dictName = dictName; // debug

			if (dictName == null || dictName == "") throw new Exception("Dictionary name not defined");

			if (RootTable.IsDatabaseListDictionaryName(dictName))
			{ // dict of root tables move to factory after Mobius 2.4 is deactivated
				return RootTable.GetDictionary(dictName);
			}

			dictName = dictName.ToLower();
			DictionaryMx dict = Dictionaries[dictName];
			if (dict == null) throw new Exception("Dictionary not defined: " + dictName);

			if (dict.Words == null)
			{
				if (DictionaryFactory == null) throw new Exception("DictionaryFactory not defined");
				int t0 = TimeOfDay.Milliseconds();
				DictionaryFactory.GetDefinitions(dict);
				t0 = TimeOfDay.Milliseconds() - t0;
				//DebugLog.Message("DictionaryMx.Get " + dictName + ", time: " + t0);
			}

			return dict;
		}

/// <summary>
/// Clear dictionary
/// </summary>

		public void Clear()
		{
			Words = null;
			WordLookup = null; 
			DefinitionLookup = null; 
			return;
		}

/// <summary>
/// Add a word and its definition to a dictionary
/// </summary>
/// <param name="word"></param>
/// <param name="definition"></param>

		public void Add(
			string word,
			string definition)
		{
			if (Words == null) // allocate area to store words if not done
				Initialize();

			bool dupWord = false;

			if (WordLookup.ContainsKey(word.ToLower())) // check if duplicate word, comparing case
			{
				foreach (string word0 in Words)
				{
					if (word0 == word)
					{
						dupWord = true;
						break;
					}
				}
			}

			if (dupWord) // already have the word
			{
				if (String.Compare((string)WordLookup[word.ToLower()],definition) == 0) return;
				WordLookup[word.ToLower()] = definition; // new def for same word
			}

			else // add new word
			{
				this.Words.Add(word);
				this.WordLookup[word.ToLower()] = definition;
				if (definition != null) // allow lookup by definition if defined
					this.DefinitionLookup[definition.ToLower()] = word;
			}
		}

		public void Initialize()
		{
			Words = new List<string>();
			WordLookup = new Dictionary<string, string>();
			DefinitionLookup = new Dictionary<string, string>();
			return;
		}

/// <summary>
/// Remove a word from its dictionary
/// </summary>
/// <param name="word"></param>
/// <param name="definition"></param>
		public void Remove(
			string word)
		{
			if (WordLookup == null) return;

			if (!WordLookup.ContainsKey(word.ToLower())) return;

			if (WordLookup[word.ToLower()] != null)
				WordLookup.Remove(word.ToLower());

			WordLookup.Remove(word);
			for (int i1=0; i1<Words.Count; i1++)
			{
				if ((string)Words[i1]==word)
				{
					Words.RemoveAt(i1);
					break;
				}
			}
		}

/// <summary>
/// Serialize dictionary
/// </summary>
/// <returns></returns>

		public string Serialize()
		{
			if (Words == null)
				Words = new List<string>(); // throw new Exception("Words is null for dictionary: " + Name);

			StringBuilder sb = new StringBuilder();
			if (Name.Contains("\t")) Name = Name.Replace("\t", " ");
			sb.Append(Name);
			sb.Append('\t');
			if (Sql.Contains("\t")) Sql = Sql.Replace("\t", " ");
			sb.Append(Sql);
			sb.Append('\t');

			for (int wi = 0; wi < Words.Count; wi++)
			{
				string word = Words[wi];
				if (word.Contains("\t")) word = word.Replace("\t", " ");
				sb.Append(word);
				sb.Append('\t');
				string def = LookupDefinition(word);
				if (def != null && def.Contains("\t")) def = def.Replace("\t", " ");
				sb.Append(def);
				if (wi < Words.Count - 1) sb.Append('\t');
			}

			return sb.ToString();
		}

/// <summary>
/// Deserialize dictionary
/// </summary>
/// <param name="serializedVersion"></param>
/// <param name="dex"></param>

		public static void Deserialize(string serializedVersion, DictionaryMx dex)
		{
			dex.Initialize();
			DictionaryMx dex2 = Deserialize(serializedVersion);
			foreach (string word in dex2.Words)
				dex.Add(word, dex2.LookupDefinition(word));

			return;
		}

/// <summary>
/// Deserialize dictionary
/// </summary>
/// <param name="serializedVersion"></param>
/// <returns></returns>

		public static DictionaryMx Deserialize(string serializedVersion)
		{
			DictionaryMx dex = new DictionaryMx();
			dex.Initialize();
			string[] sa = serializedVersion.Split('\t');
			dex.Name = sa[0];
			dex.Sql = sa[1];

			for (int i = 2; i + 1 < sa.Length; i += 2)
				dex.Add(sa[i], sa[i+1]);

			return dex;
		}
	}

	/// <summary>
	/// Simple compact string dictionary
	/// </summary>

	public class StringDictionary
	{
		public Dictionary<string, int> StringDict = null; // definitions in this dictionary looked up by word
		public Dictionary<int, string> IdDict = null; // words in this dictionary looked up by definition

		public StringDictionary()
		{
		}

/// <summary>
/// Lookup the definition for a word
/// </summary>
/// <param name="word"></param>
/// <returns></returns>

		public int Lookup (
			string word)
		{
			if (Lex.IsNullOrEmpty(word)) return -1;
			if (StringDict == null) return -1;
			if (!StringDict.ContainsKey(word)) return -1;

			int idx = StringDict[word];
			return idx;
		}

/// <summary>
/// Lookup by id
/// </summary>
/// <param name="id"></param>
/// <returns></returns>

		public string LookupById(
			int id)
		{
			if (IdDict == null) return null;
			if (!IdDict.ContainsKey(id)) return null;

			string s = (string)this.IdDict[id];
			return s;
		}


/// <summary>
/// Clear dictionary
/// </summary>

		public void Clear()
		{
			StringDict = null;
			IdDict = null;
			return;
		}

/// <summary>
/// Add a word to a dictionary and assign an integer id to the word
/// </summary>
/// <param name="word"></param>
/// <param name="definition"></param>

		public int Add(
			string word)
		{
			int idx;

			if (StringDict == null) // allocate area to store words if not done
			{
				StringDict = new Dictionary<string, int>();
				IdDict = new Dictionary<int, string>();
			}

			if (StringDict.ContainsKey(word)) // already have the word
			{
				idx = StringDict[word];
				return idx;
			}

			else // add new word
			{
				idx = StringDict.Count;
				StringDict[word] = idx;
				IdDict[idx] = word;
				return idx;
			}
		}

/// <summary>
/// Serialize to file
/// </summary>
/// <param name="fileName"></param>

		public void SerializeToFile(string fileName)
		{
			string s = Serialize();
			StreamWriter sr = new StreamWriter(fileName);
			sr.Write(s);
			sr.Close();
		}

/// <summary>
/// Serialize dictionary
/// </summary>
/// <returns></returns>

		public string Serialize()
		{
			if (StringDict == null) return "";
			StringBuilder sb = new StringBuilder();

			for (int wi = 0; wi < StringDict.Count; wi++)
			{
				string word = IdDict[wi];
				sb.Append(word);
				if (wi < StringDict.Count - 1) sb.Append('\t');
			}

			return sb.ToString();
		}

/// <summary>
/// Deserialize from file
/// </summary>
/// <param name="fileName"></param>
/// <returns></returns>

		public static StringDictionary DeserializeFromFile(string fileName)
		{
			StreamReader sr = new StreamReader(fileName);
			string s = sr.ReadToEnd();
			return Deserialize(s);
		}

/// <summary>
/// Deserialize dictionary
/// </summary>
/// <param name="serializedVersion"></param>
/// <param name="dex"></param>

		public static void Deserialize(string serializedVersion, StringDictionary dex)
		{
			dex.Clear();
			StringDictionary dex2 = Deserialize(serializedVersion);
			foreach (string word in dex2.StringDict.Keys)
				dex.Add(word);

			return;
		}

/// <summary>
/// Deserialize dictionary
/// </summary>
/// <param name="serializedVersion"></param>
/// <returns></returns>

		public static StringDictionary Deserialize(string serializedVersion)
		{
			StringDictionary dex = new StringDictionary();
			string[] sa = serializedVersion.Split('\t');
			for (int i = 0; i < sa.Length; i++)
				dex.Add(sa[i]);

			return dex;
		}
	}

}
