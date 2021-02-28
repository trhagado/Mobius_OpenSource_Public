//using DevExpress.CodeParser; // check these out
//using DevExpress.CodeParser.CSharp;

using System;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Mobius.ComOps
{
	/// <summary>
	/// Lexical analysis class
	/// </summary>

	public class Lex
	{

		// Current state data

		//		TextReader reader; // reader source of input
		int lineNumber, lastLineNumber, lastLineNumber2; // line numbers in source
		String s; // current line
		int p, lastp, lastp2; // cursor positions in input string
		int l; // length of current string 
		char c; // current char
		int sourceMode; // 1 for file, 2 for string
		bool endOfStream; // end of stream flag
		bool[] delimiter1 = new bool[256]; // entry is true if corresponding character is a delimiter
		bool[] delimiter2 = new bool[256]; // true for 1st char of 2-char delimiters
		List<string> longDelimiters; // array of 2 character delimiters 
		StringBuilder delimiter = new StringBuilder(8); // current delimiter
		bool ignoreComments; // if true ignore comments
		StringBuilder lastToken = new StringBuilder(MAX_TOKEN_SIZE); // last token selected
		int tokenType; // type of current token
		char c2; // temporary single char
		bool decimalPointSeen; // decimal point has been seen 

		// Constants

		public const string DefaultDelimiters = ", ( )";
		public const int MAX_TOKEN_SIZE = 4096; // maximum number of chars in a token

		// Token types

		const int LEX_TYPE_NULL = 0; // no more tokens
		const int LEX_TYPE_ID = 1; // identifier
		const int LEX_TYPE_INT = 2; // integer
		const int LEX_TYPE_FLT = 3; // floating point number
		const int LEX_TYPE_DELIMITER = 4; // delimiter
		const int LEX_TYPE_QUOTE = 5; // quoted string
		const int LEX_TYPE_OTHER = 6; // something else

		/// <summary>
		/// Default constructor
		/// </summary>

		public Lex()
		{
			SetDelimiters(DefaultDelimiters); // default delimiters
		}

		/// <summary>
		/// Construct specifying delimiters
		/// </summary>
		/// <param name="delimiters"></param>

		public Lex(
			string delimiters)
		{
			SetDelimiters(delimiters);
		}

		/// <summary>
		/// Peek ahead one token
		/// </summary>
		/// <returns></returns>

		public string Peek()
		{
			string tok = Get();
			if (tok != "") Backup();
			return tok;
		}

		/// <summary>
		/// Property get next token
		/// </summary>

		public string NextToken
		{
			get
			{
				return Get();
			}
		}

		/// <summary>
		/// Get the next token
		/// </summary>
		/// <returns>Token</returns>

		public String Get()
		{
			lexInternal();
			return lastToken.ToString();
		}

		/// <summary>
		/// Get token with position information
		/// </summary>
		/// <returns></returns>
		public PositionedToken GetPositionedToken()
		{
			lexInternal();
			if (lastToken.Length == 0) return null;

			PositionedToken tok = new PositionedToken();
			tok.Text = lastToken.ToString();
			tok.Position = lastp - 1; // return 0-based value
			tok.LineNumber = lastLineNumber - 1; // return 0-based value
			return tok;
		}

		/// <summary>
		/// Get the next token & be sure it matches passed token
		/// </summary>
		/// <returns>Token</returns>

		public String GetExpected(
			string expectedToken)
		{
			lexInternal();
			string tok = lastToken.ToString();
			if (Lex.Eq(tok, expectedToken))
				return tok;
			else throw new Exception("Expected " + Lex.Dq(expectedToken) +
						 " but found " + Lex.Dq(tok));
		}


		/// <summary>
		/// Convert a simple List of strings into a comma separated string list
		/// </summary>
		/// <param name="list"></param>
		/// <param name="quoteItems"></param>
		/// <returns></returns>

		public static string ListToCsvString(
			List<string> list,
			bool quoteItems = true)
		{
			return ArrayToCsvString(list.ToArray(), quoteItems);
		}

		/// <summary>
		/// Convert a string array into a comma separated string list
		/// </summary>
		/// <param name="stringArray"></param>
		/// <param name="quoteItems"></param>
		/// <returns></returns>

		public static string ArrayToCsvString(
			string[] stringArray,
			bool quoteItems = true)
		{
			if (stringArray == null || stringArray.Length == 0) return "";

			StringBuilder sb = new StringBuilder();
			for (int li = 0; li < stringArray.Length; li++)
			{
				string tok = stringArray[li];
				if (Lex.IsNullOrEmpty(tok)) continue;

				if (sb.Length > 0) sb.Append(",");
				if (quoteItems) tok = Lex.AddSingleQuotes(tok);
				sb.Append(tok);
			}

			return sb.ToString();
		}


		/// <summary>
		/// Convert a HastSet strings into a comma separated string list
		/// </summary>
		/// <param name="hashSet"></param>
		/// <param name="quoteItems"></param>
		/// <returns></returns>

		public static string HashSetToCsvString(
			HashSet<string> hashSet,
			bool quoteItems)
		{
			StringBuilder sb = new StringBuilder();
			foreach (string tok0 in hashSet)
			{
				string tok = tok0;
				if (Lex.IsNullOrEmpty(tok)) continue;

				if (sb.Length > 0) sb.Append(",");
				if (quoteItems) tok = Lex.AddSingleQuotes(tok);
				sb.Append(tok);
			}

			return sb.ToString();
		}

		/// <summary>
		/// Parse string for and return list of non-delimiter tokens
		/// </summary>
		/// <param name="sourceString"></param>
		/// <param name="delimiters"></param>
		/// <returns></returns>

		public static List<string> ParseAllExcludingDelimiters(
			string sourceString)
		{
			return ParseAllExcludingDelimiters(sourceString, DefaultDelimiters, true);
		}

		/// <summary>
		/// Parse string for and return list of non-delimiter tokens
		/// </summary>
		/// <param name="sourceString"></param>
		/// <param name="delimiters"></param>
		/// <returns></returns>

		public static List<string> ParseAllExcludingDelimiters(
			string sourceString,
			string delimiters,
			bool removeQuotes)
		{
			Lex lex = new Lex();
			lex.SetDelimiters(delimiters);
			lex.OpenString(sourceString);

			List<string> toks = new List<string>();
			while (true)
			{
				string tok = lex.ParseNonDelimiter();
				if (tok == "") break;
				if (removeQuotes) tok = RemoveAllQuotes(tok);
				toks.Add(tok);
			}

			return toks;
		}

		/// <summary>
		/// Get next non-delimiter token
		/// </summary>
		/// <param name="lex"></param>
		/// <returns></returns>

		public string ParseNonDelimiter()
		{
			while (true)
			{
				string tok = this.Get();

				if (tok.Length == 1)
				{
					if (!delimiter1[tok[0]]) return tok;
				}

				else if (tok.Length == 2)
				{
					if (!delimiter2[tok[0]] || longDelimiters == null || !longDelimiters.Contains(tok)) return tok;
				}

				else return tok;
			}
		}

		/// <summary>
		/// Internal routine to get the next token */
		/// </summary>
		/// 
		void lexInternal()
		{
			char quoteChar;
			int i1, i2;

			scanToken: // Start scanning next token

			while (true) // remove white space and comments
			{
				lastToken.Length = 0; // reset token
				tokenType = 0; // and its type

				if (endOfStream) return;

				while ((c == ' ' || c == '\t' || c == '\r' || c == '\n') &&
					!endOfStream) // pass white space
				{
					getNextCharacter();
				}
				if (endOfStream) return;

				// Ignore comments if specified

				i1 = p; // save position in case of error in comment
				i2 = lineNumber;

				if (!ignoreComments && c == '/' && p < l)
				{
					if (s[p] == '*')
					{
						getNextCharacter(); // get *
						getNextCharacter(); // and following char

						// Read characters until end of comment or endOfStream

						while (true)
						{
							if (c != '*' && !endOfStream)
							{
								getNextCharacter();
							}
							else
							{
								getNextCharacter(); // may be ending slash
								if (endOfStream)
								{
									return; // unended comment
								}
								if (c == '/')
								{ // ending slash?
									getNextCharacter();
									break;
								}
							}
						}

						goto scanToken; // start again
					}
				}
				break;
			}

			// Save current position to allow backup up to two steps

			lastp2 = lastp;
			lastp = p;
			lastLineNumber2 = lastLineNumber;
			lastLineNumber = lineNumber;

			// See if we are at a delimiter

			CheckForDelimiter();
			if (delimiter.Length > 0)
			{
				tokenType = LEX_TYPE_DELIMITER;
				lastToken.Append(delimiter);
				getNextCharacter();
				for (i1 = 1; i1 < delimiter.Length; i1++) // get rest of chars
					getNextCharacter();
				return;
			}

			// Check for quoted string

			if (c == '\'' || c == '\"')
			{
				quoteChar = c;
				tokenType = LEX_TYPE_QUOTE;
				lastToken.Append(c); // starting quote
				while (true)
				{
					getNextCharacter();
					if (endOfStream)
					{
						tokenType = LEX_TYPE_OTHER;
						return;
					}
					lastToken.Append(c);
					if (c == quoteChar)
					{
						getNextCharacter(); // double quote?
						if (endOfStream || c != quoteChar) return;
						else lastToken.Append(c);
					}
				}
			}

			// Other type of token

			while (!endOfStream && c != ' ' && c != '\t')
			{
				CheckForDelimiter();
				if (delimiter.Length > 0) break;
				lastToken.Append(c);
				getNextCharacter();
			}

			// Set token type

			tokenType = LEX_TYPE_OTHER;
			c2 = lastToken[0];

			// See if identifier

			if (Char.IsLetter(c2) || c2 == '@' || c2 == '#' || c2 == '$')
			{
				for (i1 = 2; i1 <= lastToken.Length; i1++)
				{
					c2 = lastToken[i1 - 1];
					if (Char.IsLetter(c2) || c2 == '_' || Char.IsDigit(c2)) { } // acceptable char
					else return;
				}
				tokenType = LEX_TYPE_ID;
				return;
			}

			// See if number

			else if (c2 == '-' || Char.IsDigit(c2))
			{
				decimalPointSeen = false;
				for (i1 = 2; i1 <= lastToken.Length; i1++)
				{
					c2 = lastToken[i1 - 1];
					if (c2 == '.')
					{
						if (decimalPointSeen) return; // more than one decimal 
						decimalPointSeen = true;
					}
					else if (Char.IsDigit(c2)) { }
					else return; // something else
				}
				if (!decimalPointSeen) tokenType = LEX_TYPE_INT;
				else tokenType = LEX_TYPE_FLT;
			}
		}

		/// <summary>
		/// Define the delimiter set to use when parsing
		/// </summary>
		/// <param name="ds">String of delimiters separated by spaces</param>
		public void SetDelimiters(String ds)
		{
			string tok;
			int i1;

			Array.Clear(delimiter1, 0, delimiter1.Length); // set all elements to false
			Array.Clear(delimiter2, 0, delimiter2.Length);

			delimiter1[' '] = true; // space is a delimiter
			delimiter1['\t'] = true; // so is tab
			ignoreComments = true; // comments allowed by default

			string[] delims = ds.Split(' ');
			for (i1 = 0; i1 < delims.Length; i1++)
			{
				tok = delims[i1];
				if (tok.Length == 0) { }
				else if (tok.Length == 1)
				{
					delimiter1[tok[0]] = true;
				}
				else
				{
					delimiter2[tok[0]] = true;
					if (longDelimiters == null) longDelimiters = new List<string>();
					longDelimiters.Add(tok);
				}
			}
			return;
		}

		/// <summary>
		/// Open file for lexical analysis
		/// </summary>
		/// <param name="pfid">File to open</param>
		/// <returns>True if success</returns>
		public bool OpenFile(
			String pfid)
		{

			String txt;

			Close();

			// Open file
			// Read first rec
			txt = "First Rec";
			s = txt;
			init();
			sourceMode = 1;

			return true;
		}

		/// <summary>
		/// Open scan on string
		/// </summary>
		/// <param name="sl">String to scan</param>
		public void OpenString(
			String sl)
		{
			Close();

			s = sl;
			init();
			sourceMode = 2;
			return;
		}

		/// <summary>
		/// Open scan on string & get first token
		/// </summary>
		/// <param name="sl"></param>
		/// <returns></returns>
		public string OpenStringGet(
			String sl)
		{
			Close();
			s = sl;
			init();
			sourceMode = 2;
			return Get();
		}

		/// <summary>
		/// Initialize scanner & get first char
		/// </summary>

		void init()
		{
			if (string.IsNullOrEmpty(s))
			{
				s = ""; // avoid any null execptions on s
				endOfStream = true;
			}

			else
			{
				lastLineNumber = lineNumber = lastLineNumber2 = 1; // record numbers
				p = lastp = lastp2 = 1; // pos for next char
				c = s[0]; // get char
				l = s.Length;
				endOfStream = false;
			}

			return;
		}

		/// <summary>
		/// Back up analyzer (max 2 tokens)
		/// </summary>
		public void Backup()
		{
			if (lastp <= 0) return; // already at beginning?

			p = lastp;
			lastp = lastp2;

			if (sourceMode == 1)
			{
				if (lineNumber != lastLineNumber)
				{ // reread record into s
				}
				lineNumber = lastLineNumber;
				lastLineNumber = lastLineNumber2;
			}
			
			c = s[p - 1];
			endOfStream = false;
		}

		/// <summary>
		/// Get current line
		/// </summary>
		public string Line
		{
			get { return s; }
		}

		/// <summary>
		/// Get current line number
		/// </summary>
		public int LineNumber
		{
			get { return lineNumber; }
		}

		/// <summary>
		/// Get position in current line
		/// </summary>
		public int Position
		{
			get { return p; }
		}

		/// <summary>
		/// Get type of current token 
		/// </summary>
		public int TokenType
		{
			get { return tokenType; }
		}

		/// <summary>
		/// Close Analyzer
		/// </summary>
		void Close()
		{
			if (sourceMode == 1) // (need to close?)
				sourceMode = 0;
			endOfStream = true;
		}

		/// <summary>
		/// Parse all tokens in input string into an ArrayList
		/// </summary>
		/// <returns>ArrayList of tokens</returns>

		public ArrayList ParseAll() // parse all tokens in input string into an array
		{
			ArrayList toks = new ArrayList();

			String tok;
			while (true)
			{
				tok = Get();
				if (tok.Length == 0)
					break;
				toks.Add(tok);
			}

			return toks;
		}

		/// <summary>
		/// Get the remainder of the current line
		/// </summary>
		/// <returns></returns>

		public string GetRestOfLine()
		{
			if (p >= s.Length) return "";

			lastp2 = lastp;
			lastp = p;
			lastLineNumber2 = lastLineNumber;
			lastLineNumber = lineNumber;

			string s2 = s.Substring(p).Trim();
			p = s.Length;

			if (s2.Contains("\n")) s2 = s2.Replace("\n", "");
			if (s2.Contains("\r")) s2 = s2.Replace("\r", "");
			if (s2.Contains("\t")) s2 = s2.Replace("\t", " ");
			return s2;
		}

		/// <summary>
		/// See that next token is what was expected
		/// </summary>
		/// <param name="ptok">token expected</param>
		/// <returns></returns>

		bool CheckNextToken(string ptok)
		{
			string tok = Get();
			if (tok.CompareTo(ptok) == 0) return true;
			else return false;
		}

		/// <summary>
		/// Get next token & convert to upper case
		/// </summary>
		/// <returns></returns>
		public string GetLower()
		{
			String tok;

			tok = Get();
			return tok.ToLower();
		}

		/// <summary>
		/// Get next token & convert to upper case
		/// </summary>
		/// <returns></returns>
		public string GetUpper()
		{
			String tok;

			tok = Get();
			return tok.ToUpper();
		}

		/// <summary>
		/// See if we are at a 1 or 2 char delimiter & return in delimiter var
		/// </summary>

		void CheckForDelimiter()
		{
			delimiter.Length = 0;

			if (c >= delimiter1.Length) return;

			if (delimiter2[c]) // 1st char match possible 2-char delimiter?
			{
				if (p < l)
				{
					delimiter.Append(c);
					delimiter.Append(s[p]);
					if (longDelimiters != null && longDelimiters.Contains(delimiter.ToString())) return;
					delimiter.Length = 0;
				}
			}

			if (delimiter1[c]) // 1 char delimiter 
				delimiter.Append(c);

			return;
		}

		/// <summary>
		/// Internal routine to get next char
		/// </summary>
		void getNextCharacter()
		{
			String txt;
			if (endOfStream)
				return;

			if (sourceMode == 1) // read from file
				if (p == l && tokenType != LEX_TYPE_QUOTE) // add space between lines
				{
					p = p + 1;
					c = ' ';
					return;
				}
				else if (p >= l)
				{
					readnext:
					while (true)
					{
						txt = "xxx"; // TODO: read rec
						if (txt == null)
						{
							endOfStream = true;
							return;
						}
						else
						{
							s = txt;
							lineNumber = lineNumber + 1;
							p = 0;
							l = s.Length;
							if (l == 0)
								goto readnext;
							else
								break;
						}
					}
				}

			// Get next char

			p = p + 1;
			if (p > l)
				endOfStream = true;
			else
				c = s[p - 1];
			if ((c == '\r' || c == '\n') && tokenType != LEX_TYPE_QUOTE)
				c = ' '; // treat end of line chars as spaces unless in quoted string
		}

		/// <summary>
		/// Add single quotes to a string
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>

		public static String ASQ(
			string s)
		{
			return AddSingleQuotes(s);
		}

		/// <summary>
		/// Add single quotes to a string
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>

		public static String AddSingleQuotes(
			string s)
		{
			return AddQuotes(s, '\'');
		}

		/// <summary>
		/// Add single quotes to a string if not already quoted
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>

		public static String AddSingleQuotesIfNeeded(
			string s)
		{
			if (IsQuoted(s, '\'')) return s;
			return AddQuotes(s, '\'');
		}

		public static String SqIfNeeded(
			string s)
		{
			if (IsQuoted(s, '\'')) return s;
			return AddQuotes(s, '\'');
		}

		/// <summary>
		/// Add double quotes to a string
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>

		public static String ADQ(
			string s)
		{
			return AddDoubleQuotes(s);
		}

		/// <summary>
		/// Add double quotes to a string
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>

		public static String AddDoubleQuotes(
			string s)
		{
			return AddQuotes(s, '\"');
		}

		public static String Dq(
		string s)
		{
			return AddQuotes(s, '\"');
		}

		/// <summary>
		/// Add double quotes to a string if not already quoted
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>

		public static String AddDoubleQuotesIfNeeded(
			string s)
		{
			if (IsQuoted(s, '\"')) return s;
			return AddQuotes(s, '\"');
		}

		public static String DqIfNeeded(
			string s)
		{
			if (IsQuoted(s, '\"')) return s;
			return AddQuotes(s, '\"');
		}

		/// <summary>
		/// Add quotes to a string
		/// </summary>
		/// <param name="s"></param>
		/// <param name="quoteChar">Single or double quote char</param>
		/// <returns></returns>

		public static string AddQuotes(
			string s,
			char quoteChar)
		{
			string quoteString = quoteChar.ToString();

			if (s == null) return quoteString + quoteString;

			//			if (s.StartsWith(quoteChar.ToString()) && s.EndsWith(quoteChar.ToString()))
			//				return s; // already quoted

			if (s.IndexOf(quoteChar) < 0)
				return quoteChar + s + quoteChar;

			s = s.Replace(quoteString, quoteString + quoteString);
			return quoteChar + s + quoteChar;

#if false
			os = new StringBuilder();
			for (int i1 = 0; i1 < s.Length; i1++) 
			{
				os.Append(s[i1]);
				if (s[i1] == quoteChar) 
					os.Append(quoteChar);
			}

			os.Append(quoteChar);

			return os.ToString();
#endif
		}

		/// <summary>
		/// Return true if string is already quoted with double quotes
		/// </summary>
		/// <param name="s"></param>
		/// <param name="quoteChar"></param>
		/// <returns></returns>

		public static bool IsQuoted(
			string s,
			char quoteChar)
		{
			if (!s.StartsWith(quoteChar.ToString()) ||
				!s.EndsWith(quoteChar.ToString())) return false;

			int ci = 1; // start after quote char 
			while (ci < s.Length - 1)
			{
				if (s[ci] != quoteChar) // if not a quote just move forward
				{
					ci++;
					continue;
				}

				if (ci == s.Length - 2) return false; // fail if 2nd to last char is quote
				if (s[ci + 1] != quoteChar) return false; // fail if next char is not a quote
				ci += 2;
			}

			return true;
		}

		/// <summary>
		/// Remove both single & double quotes from a string
		/// </summary>
		/// <param name="s">string to be unquoted</param>
		/// <returns></returns>

		public static string RemoveAllQuotes( // 
			string s)
		{
			string os = null;

			while (true)
			{ // loop until no more quotes are removed
				os = RemoveSingleQuotes(s);
				os = RemoveDoubleQuotes(os);
				if (os.Length == s.Length) break;
				s = os;
			}

			return os;
		}

		/// <summary>
		/// Remove single quotes from a string
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>

		public static string RemoveSingleQuotes(
			string s)
		{
			if (s == null || s.Length < 2) return s;

			if (s[0] == (char)8216) // check for "smart" curly left/right quotes
			{
				s = s.Replace((char)8216, '\'');
				s = s.Replace((char)8217, '\'');
			}

			string s2 = RemoveQuotes(s, '\'');
			return s2;
		}

		/// <summary>
		/// Remove single quotes from a string
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>

		public static string RemoveDoubleQuotes(
			string s)
		{
			if (s == null || s.Length < 2) return s;

			if (s[0] == (char)8220) // check for "smart" curly left/right quotes
			{
				s = s.Replace((char)8220, '\"');
				s = s.Replace((char)8221, '\"');
			}

			return RemoveQuotes(s, '\"');
		}


		/// <summary>
		/// Remove quotes from a string
		/// </summary>
		/// <param name="s">string to be unquoted</param>
		/// <param name="quoteChar">single or double quote character</param>
		/// <returns></returns>

		public static string RemoveQuotes(
			string s,
			char quoteChar)
		{
			if (s == null || s.Length < 2) return s;
			if (s[0] != quoteChar || s[s.Length - 1] != quoteChar) return s;

			string os = s.Substring(1, s.Length - 2);
			string twoQuotes = quoteChar + Char.ToString(quoteChar);
			os = os.Replace(twoQuotes, Char.ToString(quoteChar));
			return os;
		}

		/// <summary>
		/// Remove call-defined outer brackets from a string
		/// </summary>
		/// <param name="s"></param>
		/// <param name="leftBracket"></param>
		/// <param name="rightBracket"></param>
		/// <returns></returns>

		public static string RemoveOuterBrackets(
			string s,
			string leftBracket,
			string rightBracket)
		{
			s = s.Trim();
			if (Lex.StartsWith(s, leftBracket) && Lex.EndsWith(s, rightBracket))
			{
				int l1 = leftBracket.Length;
				int l2 = rightBracket.Length;
				s = s.Substring(l1, s.Length - (l1 + l2));
			}

			return s;
		}

		/// <summary>
		/// RemoveLineBreaksAndTabs
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>

		public static string RemoveLineBreaksAndTabs(string s)
		{
			s = s.Trim();
			
			if (s.Contains("\n")) // newlines
			{
				if (s.Contains("\n\r\t"))
					s = s.Replace("\n\r\t", " ");

				if (s.Contains("\n\r"))
					s = s.Replace("\n\r", " ");

				if (s.Contains("\r\n"))
					s = s.Replace("\r\n", " ");

				if (s.Contains("\n"))
					s = s.Replace("\n", " ");
			}

			if (s.Contains("\r")) // returns
				s = s.Replace("\r", " ");

			if (s.Contains("\t")) // tabs
			{
				if (s.Contains("\t\t"))
					s = s.Replace("\t\t", " ");

				if (s.Contains("\t"))
					s = s.Replace("\t", " ");
			}

			return s;
		}

		/// <summary>
		/// Adjust all of the end of line characters to specified chars
		/// </summary>
		/// <param name="txt"></param>
		/// <param name="lineEndChars"></param>
		/// <returns></returns>

		public static string AdjustEndOfLineCharacters(
			string txt,
			string lineEndChars)
		{
			if (txt == null) return txt;

			txt = txt.Replace("\r\n", "\n"); // change all line ends to "\n"
			txt = txt.Replace("\n\r", "\n");
			txt = txt.Replace("\r", "\n");

			if (lineEndChars != "\n")
				txt = txt.Replace("\n", "\r\n"); // complete by changing all line ends to desired form

			return txt;
		}

		/// <summary>
		/// Append an item to a list inserting comma separator if list not empty
		/// </summary>
		/// <param name="source"></param>
		/// <param name="item"></param>
		/// <returns></returns>

		public static string AppendToList(
			string source,
			string item)
		{
			string result = source;
			AppendToList(ref result, item);
			return result;
		}

		/// <summary>
		/// Append an item to a list inserting comma separator if list not empty
		/// </summary>
		/// <param name="dest">Source and dest</param>
		/// <param name="item"></param>

		public static void AppendToList(
			ref string dest,
			string item)
		{
			if (Lex.IsUndefined(item)) return;
			string separator = ", ";
			AppendSeparatorToStringIfNotBlank(ref dest, separator);
			dest += item;
			return;
		}

		/// <summary>
		/// Append an item to a list inserting separator if list not empty
		/// </summary>
		/// <param name="dest"></param>
		/// <param name="separator"></param>
		/// <param name="item"></param>

		public static void AppendToList(
				ref string dest,
				string separator,
				string item)
		{
			AppendSeparatorToStringIfNotBlank(ref dest, separator);
			dest += item;
			return;
		}

		/// <summary>
		/// Append the source string to the dest using the separator if not already contained in the dest
		/// </summary>
		/// <param name="dest"></param>
		/// <param name="source"></param>
		/// <param name="separator"></param>

		public static void AppendToListIfNew(
			ref string dest,
			string source,
			string separator)
		{
			if (Lex.Eq(dest, source) || Lex.StartsWith(dest, source + separator) ||
			 Lex.Contains(dest, separator + source + separator) || Lex.EndsWith(dest, separator + source))
				return;

			Lex.AppendSeparatorToStringIfNotBlank(ref dest, separator);
			dest += source;
			return;
		}

		/// <summary>
		/// Append s2 to s1 if s1 is not blank
		/// </summary>
		/// <param name="dest"></param>
		/// <param name="separator"></param>

		public static void AppendSeparatorToStringIfNotBlank(
			ref string dest,
			string separator)
		{
			if (Lex.IsNullOrEmpty(dest)) return;
			else dest += separator;
		}

		/// <summary>
		/// Capitalize first character of each word, set others to lower case
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>

		public static string CapitalizeFirstLetters(
			string s)
		{
			return CapitalizeFirstLetters(s, true);
		}

		/// <summary>
		/// Capitalize first character of each word
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>

		public static string CapitalizeFirstLetters(
			string s,
			bool setOtherLettersToLowerCase)
		{
			StringBuilder s2;
			int ci;

			if (s.Length == 0) return "";
			if (!setOtherLettersToLowerCase)
				s2 = new StringBuilder(s);
			else s2 = new StringBuilder(s.ToLower());

			for (ci = 0; ci < s.Length; ci++)
			{
				if (!Char.IsLetter(s2[ci])) continue;
				if (ci == 0) s2[ci] = Char.ToUpper(s2[ci]);
				else if (!Char.IsLetterOrDigit(s2[ci - 1])) s2[ci] = Char.ToUpper(s2[ci]);
			}
			return s2.ToString();
		}

		/// <summary>
		/// Count number of lines in a string
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>

		public static int CountLines(
			string s)
		{
			if (Lex.IsUndefined(s)) return 0;

			int lineCount = Lex.CountCharacter(s, '\n');
			if (lineCount == 0)
				lineCount = Lex.CountCharacter(s, '\r');

			char c = s[s.Length - 1];
			if (c != '\n' && c != 'r') lineCount++;

			return lineCount;
		}

		/// <summary>
		/// Count number of occurances of character in string
		/// </summary>
		/// <param name="s"></param>
		/// <param name="c"></param>
		/// <returns></returns>

		public static int CountCharacter(
			string s,
			char c)
		{
			if (s == null) return 0;

			int cnt = 0;
			foreach (char c2 in s)
			{
				if (c2 == c) cnt++;
			}

			return cnt;
		}

		/// <summary>
		/// Count number of occurances of substring in string
		/// </summary>
		/// <param name="s"></param>
		/// <param name="subString"></param>
		/// <returns></returns>

		public static int CountSubstring(
			string s,
			string subString)
		{
			if (s == null) return 0;

			string pattern = Regex.Escape(subString);
			MatchCollection mc = Regex.Matches(s, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);
			return mc.Count;
		}

		/// <summary>
		/// Extract integer value between two substrings
		/// </summary>
		/// <param name="s"></param>
		/// <param name="substringBefore"></param>
		/// <param name="substringAfter"></param>
		/// <returns></returns>

		public static int ExtractIntBetween(
			string s,
			string substringBefore,
			string substringAfter)
		{
			int i1;

			string s2 = SubstringBetween(s, substringBefore, substringAfter);
			if (s2 == null) return NumberEx.NullNumber;

			if (!int.TryParse(s2.Trim(), out i1)) return NumberEx.NullNumber;
			return i1;
		}

		/// <summary>
		/// Extract the substring between two other substrings
		/// </summary>
		/// <param name="s"></param>
		/// <param name="substringBefore"></param>
		/// <param name="substringAfter"></param>
		/// <returns></returns>

		public static string SubstringBetween(
			string s,
			string substringBefore,
			string substringAfter, 
			bool includeSubstrings = false)
		{
			string after = SubstringAfter(s, substringBefore);
			string match = SubstringBefore(after, substringAfter);
			if (match == null) return null;

			if (!includeSubstrings) return match.Trim();
			else return substringBefore + match + substringAfter;

			//string pattern = @".*" + Regex.Escape(substringBefore) + "(.*?)" + Regex.Escape(substringAfter) + ".*"; // (can match too much (without ?))
			//string match = GetRegexGroupMatch(s, pattern);
			//if (match != null) return match.Trim();
			//else return null;
		}

		/// <summary>
		/// Extract the list of matching substrings that begin and end with the specified strings
		/// </summary>
		/// <param name="s"></param>
		/// <param name="substringBefore"></param>
		/// <param name="substringAfter"></param>
		/// <returns></returns>

		public static List<string> GetSubstringsBetweenInclusive(
			string s,
			string substringBefore,
			string substringAfter)
		{
			return GetSubstringsBetween(s, substringBefore, substringAfter, true);
		}

		/// <summary>
		/// Extract the list of matching substrings that begin and end with the specified strings
		/// </summary>
		/// <param name="s"></param>
		/// <param name="substringBefore"></param>
		/// <param name="substringAfter"></param>
		/// <returns></returns>

		public static List<string> GetSubstringsBetween(
			string s,
			string substringBefore,
			string substringAfter,
			bool includeSubstrings = false)
		{
			int p = 0, p1, p2;
			int l1 = substringBefore.Length;
			int l2 = substringAfter.Length;
			string match;

			List<string> matches = new List<string>();

			while (p < s.Length)
			{
				p1 = s.IndexOf(substringBefore, p, StringComparison.OrdinalIgnoreCase);
				if (p1 < 0) return matches;

				p = p1 + l1;

				p2 = s.IndexOf(substringAfter, p, StringComparison.OrdinalIgnoreCase);
				if (p2 < 0) return matches;

				p = p2 + l2;

				if (includeSubstrings)
					match = s.Substring(p1, (p2 + l2) - p1);

				else match = s.Substring(p1 + l1, p2 - (p1 + l1));
				matches.Add(match);
			}
			return matches;
		}

		/// <summary>
		/// Get the substring preceeding the specified substring
		/// </summary>
		/// <param name="s"></param>
		/// <param name="subString"></param>
		/// <returns></returns>

		public static string SubstringBefore(
			string s,
			string subString)
		{
			int i1 = IndexOf(s, subString);
			if (i1 < 0) return null;

			return s.Substring(0, i1);
		}

		/// <summary>
		/// Get the remainder of a string following specified substring
		/// </summary>
		/// <param name="s"></param>
		/// <param name="subString"></param>
		/// <returns></returns>

		public static string SubstringAfter(
			string s,
			string subString)
		{
			int i1 = IndexOf(s, subString);
			if (i1 < 0) return null;

			return s.Substring(i1 + subString.Length);
		}

		/// <summary>
		/// Trim a string
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>

		public static string Trim(
			string s,
			char trimChar = ' ')
		{
			if (s != null) return s.Trim(trimChar);
			else return null;
		}

		/// <summary>
		/// Truncate a string to specified max length as needed
		/// </summary>
		/// <param name="s"></param>
		/// <param name="maxLength"></param>
		/// <returns></returns>

		public static string Truncate(
			string s, 
			int maxLength)
		{
			if (string.IsNullOrEmpty(s)) return s;
			return s.Length <= maxLength ? s : s.Substring(0, maxLength);
		}

		/// <summary>
		/// Increment any integer suffix at the end of a string by one
		/// If no integer suffix append a suffix of: " 2"
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>

		public static string IncrementIntegerSuffix(string s)
		{
			string s2;
			int num; 

			Match match = Regex.Match(s, @"^(.*)([0-9]+)$"); // matches anything at start of string and any integer at end of string

			if (match == null || match.Groups.Count != (1 + 2) ||
				!int.TryParse(match.Groups[2].Value, out num))
			{
				s2 = s + " 2";
			}

			else
			{
				s2 = match.Groups[1].Value + (num + 1);
			}

			return s2;
		}

		/// <summary>
		/// Scan string and attempt to extract a version number
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>

		public static Version ExtractVersion(string s)
		{
			try
			{
				string rex = @"\d+(\.\d+)+";
				Match match = Regex.Match(s, rex, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
				if (!match.Success) return new Version();

				Version v = new Version(match.Value);
				return v;
			}

			catch (Exception ex)
			{
				string msg = ex.Message;
				return new Version();
			}
		}

		/// <summary>
		/// Get an element of a string array returning "" if index is out of range
		/// </summary>
		/// <param name="array"></param>
		/// <param name="index"></param>
		/// <returns></returns>

		public static string GetArrayString(
			string[] array,
			int index)
		{

			if (array != null && index >= 0 && index < array.Length) return array[index];
			else return "";
		}


		/// <summary>
		/// Get an element of a string list returning "" if index is out of range
		/// </summary>
		/// <param name="list"></param>
		/// <param name="index"></param>
		/// <returns></returns>

		public static string GetStringListItem(
			List<string> list,
			int index)
		{
			if (list != null && index >= 0 && index < list.Count) return list[index];
			else return "";
		}

/// <summary>
/// Get string length, return 0 if null
/// </summary>
/// <param name="s"></param>
/// <returns></returns>

		public static int GetStringLength(
			string s)
		{
			return (s != null ? s.Length : 0);
		}

		/// <summary>
		/// Extract a group from a RegexMatch
		/// </summary>
		/// <param name="s"></param>
		/// <param name="pattern"></param>
		/// <param name="group"></param>
		/// <returns></returns>

		public static string GetRegexGroupMatch(
			string s,
			string pattern,
			int group = 0)
		{
			Match match = Regex.Match(s, pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
			if (!match.Success) return null;

			string groupVal = match.Groups[group].Value;
			return groupVal;
		}

		/**************************************************************************************************
		 **************************** C# Regular Expressions Cheat Sheet **********************************
		 **************************************************************************************************
		 * 

Char  Description
====  ==========================================================================
\     Marks the next character as either a special character or escapes a literal. For example, "n" matches the character "n". "\n" matches a newline character. 
       The sequence "\\" matches "\" and "\(" matches "(". Note: Double quotes may be escaped by doubling them: "<a href=""...>"
*     Matches the preceding character zero or more times. For example, "zo*" matches either "z" or "zoo".
+     Matches the preceding character one or more times. For example, "zo+" matches "zoo" but not "z".
?     Matches the preceding character zero or one time. For example, "a?ve?" matches the "ve" in "never".
.     Matches any single character except a newline character.
^     Depending on whether the MultiLine option is set, matches the position before the first character in a line, or the first character in the string.
$     Depending on whether the MultiLine option is set, matches the position after the last character in a line, or the last character in the string.
(pattern) - Matches pattern and remembers the match. The matched substring can be retrieved from the resulting Matches collection, using Item [0]...[n]. 
             To match parentheses characters ( ), use "\(" or "\)".
(?<name>pattern) - Matches pattern and gives the match a name.
(?:pattern) - A non-capturing group
(?=...)  A positive lookahead
(?!...)  A negative lookahead
(?<=...) A positive lookbehind.
(?<!...) A negative lookbehind.
x|y    Matches either x or y. For example, "z|wood" matches "z" or "wood". "(z|w)oo" matches "zoo" or "wood".
{n}    n is a non-negative integer. Matches exactly n times. For example, "o{2}" does not match the "o" in "Bob," but matches the first two o's in "foooood".
{n,}   n is a non-negative integer. Matches at least n times. For example, "o{2,}" does not match the "o" in "Bob" and matches all the o's in "foooood." "o{1,}" is equivalent to "o+". "o{0,}" is equivalent to "o*".
{n,m}  m and n are non-negative integers. Matches at least n and at most m times. For example, "o{1,3}" matches the first three o's in "fooooood." "o{0,1}" is equivalent to "o?".
[xyz]  A character set. Matches any one of the enclosed characters. For example, "[abc]" matches the "a" in "plain".
[^xyz] A negative character set. Matches any character not enclosed. For example, "[^abc]" matches the "p" in "plain".
[a-z]  A range of characters. Matches any character in the specified range. For example, "[a-z]" matches any lowercase alphabetic character in the range "a" through "z".
[^m-z] A negative range characters. Matches any character not in the specified range. For example, "[m-z]" matches any character not in the range "m" through "z".
\b     Matches a word boundary, that is, the position between a word and a space. For example, "er\b" matches the "er" in "never" but not the "er" in "verb".
\B     Matches a non-word boundary. "ea*r\B" matches the "ear" in "never early".
\d     Matches a digit character. Equivalent to [0-9].
\D     Matches a non-digit character. Equivalent to [^0-9].
\f     Matches a form-feed character.
\k     A back-reference to a named group.
\n     Matches a newline character.
\r     Matches a carriage return character.
\s     Matches any white space including space, tab, form-feed, etc. Equivalent to "[ \f\n\r\t\v]".
\S     Matches any nonwhite space character. Equivalent to "[^ \f\n\r\t\v]".
\t     Matches a tab character.
\v     Matches a vertical tab character.
\w     Matches any word character including underscore. Equivalent to "[A-Za-z0-9_]".
\W     Matches any non-word character. Equivalent to "[^A-Za-z0-9_]".
\num   Matches num, where num is a positive integer. A reference back to remembered matches. For example, "(.)\1" matches two consecutive identical characters.
\n     Matches n, where n is an octal escape value. Octal escape values must be 1, 2, or 3 digits long. For example, "\11" and "\011" both match a tab character. "\0011" is the equivalent of "\001" & "1". Octal escape values must not exceed 256. If they do, only the first two digits comprise the expression. Allows ASCII codes to be used in regular expressions.
\xn    Matches n, where n is a hexadecimal escape value. Hexadecimal escape values must be exactly two digits long. For example, "\x41" matches "A". "\x041" is equivalent to "\x04" & "1". Allows ASCII codes to be used in regular expressions.
\un    Matches a Unicode character expressed in hexadecimal notation with exactly four numeric digits. "\u0200" matches a space character.
\A     Matches the position before the first character in a string. Not affected by the MultiLine setting
\Z     Matches the position after the last character of a string. Not affected by the MultiLine setting.
\G     Specifies that the matches must be consecutive, without any intervening non-matching characters.  
       */

		/// <summary>
		/// Case-insensitive StartsWith
		/// </summary>
		/// <param name="s"></param>
		/// <param name="subString"></param>
		/// <returns></returns>

		public static bool StartsWith(
			string s,
			string subString)
		{
			if (s == null || subString == null) return false;
			return s.StartsWith(subString, StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>
		/// Case-insensitive EndsWith
		/// </summary>
		/// <param name="s"></param>
		/// <param name="subString"></param>
		/// <returns></returns>

		public static bool EndsWith(
			string s,
			string subString)
		{
			if (s == null || subString == null) return false;
			if (s == null) return false;
			return s.EndsWith(subString, StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>
		/// Case-insensitive Contains
		/// </summary>
		/// <param name="s"></param>
		/// <param name="subString"></param>
		/// <returns></returns>

		public static bool Contains(
			string s,
			string subString)
		{
			if (s == null || subString == null) return false;
			if (s.IndexOf(subString, StringComparison.OrdinalIgnoreCase) >= 0) return true;
			else return false;
		}

		/// <summary>
		/// Case-insensitive IndexOf
		/// </summary>
		/// <param name="s"></param>
		/// <param name="subString"></param>
		/// <returns></returns>

		public static int IndexOf(
			string s,
			string subString)
		{
			if (s == null) return -1;
			return s.IndexOf(subString, StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>
		/// Case-insensitive LastIndexOf
		/// </summary>
		/// <param name="s"></param>
		/// <param name="subString"></param>
		/// <returns></returns>

		public static int LastIndexOf(
			string s,
			string subString)
		{
			if (s == null) return -1;
			return s.LastIndexOf(subString, StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>
		/// Index of previous matching substring from specified position
		/// 
		/// </summary>
		/// <param name="s"></param>
		/// <param name="i1"></param>
		/// <param name="matchString"></param>
		/// <returns></returns>

		public static int IndexOfPrevious(
			string s,
			int i1,
			string matchString)
		{
			int i2 = LastIndexOf(s.Substring(0,i1), matchString);
			if (i2 >= 0) return i2;
			else return -1;
		}

		/// <summary>
		/// Index of next matching substring from specified position
		/// </summary>
		/// <param name="s"></param>
		/// <param name="i1"></param>
		/// <param name="matchString"></param>
		/// <returns></returns>

		public static int IndexOfNext(
			string s,
			int i1,
			string matchString)
		{
			int i2 = IndexOf(s.Substring(i1), matchString);
			if (i2 >= 0) return i1 + i2;
			else return -1;
		}

		/// <summary>
		/// Remove substring at specified position
		/// </summary>
		/// <param name="s"></param>
		/// <param name="pos"></param>
		/// <param name="len"></param>
		/// <returns></returns>

		public static string RemoveSubstring(
			string s,
			int pos,
			int len)
		{
			string s2 = s.Substring(0, pos) + s.Substring(pos + len);
			return s2;
		}

		/// <summary>
		/// Case-insensitive Replacement of first occurance of substring
		/// </summary>
		/// <param name="str"></param>
		/// <param name="oldValue"></param>
		/// <param name="newValue"></param>
		/// <returns></returns>

		public static string ReplaceFirst(
			string str,
			string oldValue,
			string newValue)
		{
			string s2 = Replace(str, oldValue, newValue, 1);
			return s2;
		}

		/// <summary>
		/// Case-insensitive Replace returning bool
		/// </summary>
		/// <param name="str"></param>
		/// <param name="oldValue"></param>
		/// <param name="newValue"></param>
		/// <param name="maxReplacements"></param>
		/// <returns></returns>

		static public bool TryReplace(
			ref string str,
			string oldValue,
			string newValue,
			int maxReplacements = -1)
		{
			if (!Lex.Contains(str, oldValue)) return false;

			str = Lex.Replace(str, oldValue, newValue, maxReplacements);
			return true;
		}

		/// <summary>
		/// Case-insensitive Replace
		/// </summary>
		/// <param name="str"></param>
		/// <param name="oldValue"></param>
		/// <param name="newValue"></param>
		/// <returns></returns>

		static public string Replace(
			string str,
			string oldValue,
			string newValue,
			int maxReplacements = -1)
		{
			if (str == null) return null;
			StringBuilder sb = new StringBuilder();

			int replaceCount = 0;
			int previousIndex = 0;
			int index = str.IndexOf(oldValue, StringComparison.OrdinalIgnoreCase);
			while (index != -1)
			{
				if (maxReplacements >= 0 && replaceCount >= maxReplacements) break;

				sb.Append(str.Substring(previousIndex, index - previousIndex));
				sb.Append(newValue);
				index += oldValue.Length;
				previousIndex = index;
				replaceCount++;

				index = str.IndexOf(oldValue, index, StringComparison.OrdinalIgnoreCase);
			}
			sb.Append(str.Substring(previousIndex));

			return sb.ToString();
		}

		/// <summary>
		/// Split a list into one or more sublists with the max specified 
		/// number of subitems per list
		/// </summary>
		/// <param name="list"></param>
		/// <param name="delim"></param>
		/// <param name="maxItemsPerSublist"></param>
		/// <returns></returns>

		public static string[] SplitListIntoSublists(
			string list,
			char delim,
			int maxItemsPerSublist)
		{
			if (Lex.IsUndefined(list))
				return new string[] { list };

			int totalItems = Lex.CountCharacter(list, delim) + 1;
			if (totalItems <= maxItemsPerSublist)
				return new string[] { list };

			int listCnt = ((totalItems - 1) / maxItemsPerSublist) + 1;
			string[] subLists = new string[listCnt];
			int li = 0; // index of current list being built

			int ci0 = 0; // starting char position for current list
			int delimCnt = 0; // delimiters seen for current list

			for (int ci = 0; ; ci++)
			{
				if (ci < list.Length && list[ci] == delim) delimCnt++;

				if (delimCnt == maxItemsPerSublist || ci >= list.Length)
				{
					subLists[li] = list.Substring(ci0, ci - ci0);
					li++; // next list index
					if (li >= listCnt) break;
					ci0 = ci + 1;
					delimCnt = 0;
				}
			}

			return subLists;
		}


		/// <summary>
		/// Split a string and return array of trimmed items with empty items removed
		/// </summary>
		/// <param name="str"></param>
		/// <param name="delim"></param>
		/// <returns></returns>

		public static string[] Split(
			string str,
			string delim)
		{
			string[] sa = str.Split(new string[] { delim }, StringSplitOptions.RemoveEmptyEntries);
			for (int si = 0; si < sa.Length; si++)
			{
				sa[si] = sa[si].Trim();
			}

			return sa;
		}

		public static string[] Split(
			string str,
			string delim,
			out string s1)
		{
			string[] sa = Split(str, delim);
			s1 = GetSI(sa, 0);
			return sa;
		}

		public static string[] Split(
			string str,
			string delim,
			out string s1,
			out string s2)
		{
			string[] sa = Split(str, delim);
			s1 = GetSI(sa, 0);
			s2 = GetSI(sa, 1);

			return sa;
		}
		public static string[] Split(
			string str,
			string delim,
			out string s1,
			out string s2,
			out string s3)
		{
			string[] sa = Split(str, delim);
			s1 = GetSI(sa, 0);
			s2 = GetSI(sa, 1);
			s3 = GetSI(sa, 2);

			return sa;
		}

		public static string[] Split(
			string str,
			string delim,
			out string s1,
			out string s2,
			out string s3,
			out string s4)
		{
			string[] sa = Split(str, delim);
			s1 = GetSI(sa, 0);
			s2 = GetSI(sa, 1);
			s3 = GetSI(sa, 2);
			s4 = GetSI(sa, 3);

			return sa;
		}

		public static string[] Split(
			string str,
			string delim,
			out string s1,
			out string s2,
			out string s3,
			out string s4,
			out string s5)
		{
			string[] sa = Split(str, delim);
			s1 = GetSI(sa, 0);
			s2 = GetSI(sa, 1);
			s3 = GetSI(sa, 2);
			s4 = GetSI(sa, 3);
			s5 = GetSI(sa, 4);

			return sa;
		}
		public static string[] Split(
			string str,
			string delim,
			out string s1,
			out string s2,
			out string s3,
			out string s4,
			out string s5,
			out string s6)
		{
			string[] sa = Split(str, delim);
			s1 = GetSI(sa, 0);
			s2 = GetSI(sa, 1);
			s3 = GetSI(sa, 2);
			s4 = GetSI(sa, 3);
			s5 = GetSI(sa, 4);
			s6 = GetSI(sa, 5);

			return sa;
		}

		static string GetSI(string[] sa, int i)
		{
			if (sa.Length - 1 < i) return "";
			else return sa[i];
		}

		/// <summary>
		/// Join a list of strings into a single string
		/// </summary>
		/// <param name="sList"></param>
		/// <param name="delim"></param>
		/// <returns></returns>

		public static string Join(
			IEnumerable<string> sList,
			string delim = ",")
		{
			string s2 = String.Join(delim, sList);
			return s2;
		}

/// <summary>
/// Join a list of integers into a single string
/// </summary>
/// <param name="sList"></param>
/// <param name="delim"></param>
/// <returns></returns>
/// 
		public static string Join(
			IEnumerable<int> sList,
			string delim = ",")
		{
			string s2 = String.Join(delim, sList);
			return s2;
		}

		/// <summary>
		/// Remove all non-digit characters from a string
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>

		public static string RemoveNonDigits(string s)
		{
			if (string.IsNullOrEmpty(s)) return s;
			StringBuilder sb = new StringBuilder(s.Length);
			foreach (char c in s)
			{
				if (char.IsNumber(c))
					sb.Append(c);
			}
			string digits = sb.ToString();
			return digits;
		}

		/// <summary>
		/// Split and analyze lines of text
		/// </summary>
		/// <param name="txt"></param>
		/// <returns></returns>

		public static LexLineMetrics AnalyzeLines(string txt)
		{
			LexLineMetrics llm = LexLineMetrics.Analyze(txt);
			return llm;
		}

		/// <summary>
		/// Alternate syntax for comparing two string for equality ignoring case
		/// </summary>
		/// <param name="s1"></param>
		/// <param name="s2"></param>
		/// <returns></returns>

		public static bool Eq(
			string s1,
			string s2)
		{
			return String.Equals(s1, s2, StringComparison.OrdinalIgnoreCase);
		}

		public static bool Ne(
			string s1,
			string s2)
		{
			return !String.Equals(s1, s2, StringComparison.OrdinalIgnoreCase);
		}

		public static bool Lt(
			string s1,
			string s2)
		{
			if (String.Compare(s1, s2, true) < 0) return true;
			else return false;
		}

		public static bool Le(
			string s1,
			string s2)
		{
			if (String.Compare(s1, s2, true) <= 0) return true;
			else return false;
		}

		public static bool Gt(
			string s1,
			string s2)
		{
			if (String.Compare(s1, s2, true) > 0) return true;
			else return false;
		}

		public static bool Ge(
			string s1,
			string s2)
		{
			if (String.Compare(s1, s2, true) >= 0) return true;
			else return false;
		}

		/// <summary>
		/// Alternate syntax for comparing two string for equality ignoring case
		/// </summary>
		/// <param name="s1"></param>
		/// <param name="s2"></param>
		/// <returns></returns>

		public static bool EqualsIgnoreCase(
			string s1,
			string s2)
		{
			if (String.Compare(s1, s2, true) == 0) return true;
			else return false;
		}

		/// <summary>
		/// Return true if token is an integer
		/// </summary>
		/// <param name="tok"></param>
		/// <returns></returns>

		public static bool IsInteger(
			string tok)
		{
			double d;

			if (Lex.IsNullOrEmpty(tok)) return false;
			if (tok == ".") return false;

			if (!double.TryParse(tok, out d)) return false; // say true for values like 1.0

			if (d > Int32.MaxValue || d < Int32.MinValue) return false;
			else if ((int)d != d) return false;
			else return (true);
		}

		/// <summary>
		/// Return true if token is an unsigned integer
		/// </summary>
		/// <param name="tok"></param>
		/// <returns></returns>

		public static bool IsUnsignedInteger(
			string tok)
		{
			uint i;

			if (Lex.IsNullOrEmpty(tok)) return false;
			if (tok == ".") return false;

			return uint.TryParse(tok, out i);
		}

		/// <summary>
		/// Return true if token is a 64 bit integer
		/// </summary>
		/// <param name="tok"></param>
		/// <returns></returns>

		public static bool IsLong(
			string tok)
		{
			long i;

			if (Lex.IsNullOrEmpty(tok)) return false;
			if (tok == ".") return false;

			return Int64.TryParse(tok, out i);
		}

		/// <summary>
		/// Return true if token is a number
		/// </summary>
		/// <param name="tok"></param>
		/// <returns></returns>

		public static bool IsDouble(
			string tok)
		{
			double d;

			return NumberEx.DoubleTryParseEx(tok, out d); // more accurate check for string that is a double
		}

		/// <summary>
		/// Return true if parameter is a Uri
		/// </summary>
		/// <param name="txt"></param>
		/// <returns></returns>

		public static bool IsUri(
				string txt)
		{
			if (txt == null) return false;

			Uri uri;
			if (Uri.TryCreate(txt, UriKind.Absolute, out uri) && uri.HostNameType != UriHostNameType.Unknown)
			{
				return true;
			}
			return (txt.ToLower().StartsWith("www.") ||
							txt.ToLower().StartsWith("http:") ||
							txt.ToLower().StartsWith("https:"));
			//if (txt.ToLower().StartsWith("www.")) return true; // special case
			//if (txt.ToLower().StartsWith("http:") || txt.ToLower().StartsWith("https:")) return true; // in case of invalid host name exception (link will probably fail)
			//return false;
		}

		/// <summary>
		/// Parse a bool arg
		/// </summary>
		/// <param name="arg"></param>
		/// <returns></returns>

		public static bool BoolParse(string arg)
		{
			bool b;

			if (BoolTryParse(arg, out b)) return b;
			else throw new InvalidExpressionException();
		}
		/// <summary>
		/// Try to get a boolean setting value
		/// </summary>
		/// <returns></returns>

		public static bool BoolTryParse(
			string arg,
			out bool value)
		{
			if (bool.TryParse(arg, out value)) return true;

			arg = arg.ToLower().Trim();
			if (arg == "on" || arg == "yes" || arg == "true")
			{
				value = true;
				return true;
			}
			else if (arg == "off" || arg == "no" || arg == "false")
			{
				value = false;
				return true;
			}

			else return false;
		}

		/// <summary>
		/// Try to parse off the first token in a string up through the delimiter
		/// </summary>
		/// <param name="txt"></param>
		/// <param name="value"></param>
		/// <param name="delim"></param>
		/// <returns></returns>

		public static bool TryParseOffFirstToken(
			ref string txt,
			string delim,
			out string value)
		{
			value = null;

			int i1 = txt.IndexOf(delim);
			if (i1 < 0) return false;

			value = txt.Substring(0, i1);
			txt = txt.Substring(i1 + delim.Length);
			return true;
		}

		/// <summary>
		/// Try to parse off the first integer token in a string
		/// </summary>
		/// <param name="txt"></param>
		/// <param name="value"></param>
		/// <param name="delim"></param>
		/// <returns></returns>

		public static bool TryParseOffFirstIntegerToken(
			ref string txt,
			char delim,
			out int value)
		{
			value = NumberEx.NullNumber;

			if (txt == null) return false;

			int i1 = txt.IndexOf(delim);
			if (i1 < 0) return false;

			if (!int.TryParse(txt.Substring(0, i1), out value)) return false;

			txt = txt.Substring(i1 + 1);
			return true;
		}

	/// <summary>
	/// Case-insensitive list sort
	/// </summary>
	/// <param name="l"></param>

	public static void SortList(
			List<string> l)
		{
			bool caseSensitive = false;
			l.Sort(delegate (string left, string right)
				{
					return String.Compare(left, right, caseSensitive);
				}
			);
		}

		/// <summary>
		/// Return true if string is not null or empty or all whitespace
		/// </summary>
		/// <param name="txt"></param>
		/// <returns></returns>

		public static bool IsDefined(string txt)
		{
			return !String.IsNullOrWhiteSpace(txt);
		}

		/// <summary>
		/// Return true if string is null or empty or all whitespace
		/// </summary>
		/// <param name="txt"></param>
		/// <returns></returns>

		public static bool IsUndefined(string txt)
		{
			return String.IsNullOrWhiteSpace(txt);
		}

		/// <summary>
		/// Return true if string is null or empty or all whitespace
		/// </summary>
		/// <param name="txt"></param>
		/// <returns></returns>

		public static bool IsNullOrEmpty(string txt)
		{
			return String.IsNullOrWhiteSpace(txt);
		}

		/// <summary>
		/// Convert an Id to a better looking label
		/// </summary>

		public static string InternalNameToLabel(
			string id)
		{
			char pc, cc;
			int i1;

			StringBuilder label = new StringBuilder(id);
			pc = ' '; // prev char
			for (i1 = 0; i1 < label.Length; i1++)
			{
				cc = label[i1];
				if (Char.IsLetter(cc))
				{
					if (pc == ' ') label[i1] = Char.ToUpper(cc); // cap first char
					else label[i1] = Char.ToLower(cc);
				}
				else if (cc == '_')
				{ // treat underscore like space
					label[i1] = ' ';
					cc = ' ';
				}
				pc = cc; // 
			}
			return label.ToString();
		}

		/// <summary>
		/// ComputeMD5Hash
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>

		public static string ComputeMD5Hash(string s)
		{
			MD5 md5 = MD5.Create();
			byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(s));

			StringBuilder sb = new StringBuilder();

			for (int i = 0; i < data.Length; i++)
			{
				sb.Append(data[i].ToString("x2"));
			}

			return sb.ToString();
		}

		/// <summary>
		/// Base64 decode
		/// </summary>
		/// <param name="b64line"></param>
		/// <returns></returns>

		public static string B64Decode(string b64line)
		{
			StringBuilder sb = new StringBuilder();
			try
			{
				byte[] decodedBytes = Convert.FromBase64String(b64line);
				for (int i = 0; i < decodedBytes.Length; i++)
				{
					sb.Append(Convert.ToChar(decodedBytes[i]));
				}
			}
			catch (Exception ex)
			{
				sb.Length = 0;
				sb.Append(ex.Message);
			}
			if (sb[sb.Length - 1] == ' ') sb.Remove(sb.Length - 1, 1); // remove any added trailing space
			return sb.ToString();
		}

		/// <summary>
		/// Base64 encode
		/// </summary>
		/// <param name="line"></param>
		/// <returns></returns>

		public static string B64Encode(string line)
		{
			string b64line;
			try
			{
				if (line.Length % 2 != 0) line += " ";
				char[] b64chars = new char[(int)Math.Ceiling((double)line.Length * 3 / 2)];
				int len = line.Length;
				byte[] originalBytes = new byte[len];
				for (int i = 0; i < len; i++)
				{
					originalBytes[i] = Convert.ToByte(line[i]);
				}
				len = Convert.ToBase64CharArray(originalBytes, 0, line.Length, b64chars, 0);
				b64line = new String(b64chars, 0, len);
			}
			catch (Exception ex)
			{
				b64line = ex.Message;
			}
			return b64line;
		}

		/// <summary>
		/// Expand a compressed capitalized name to a name with separate individual words
		/// E.g. ThisIsASentence -> This is a sentence
		/// </summary>
		/// <param name="txt"></param>
		/// <returns></returns>

		public static string ExpandCapitalizedName(
			string txt,
			bool keepCaps)
		{
			StringBuilder sb = new StringBuilder();
			for (int ci = 0; ci < txt.Length; ci++)
			{
				char c = txt[ci];
				if (ci > 0 && Char.IsUpper(c))
				{
					sb.Append(' ');
					if (!keepCaps) c = Char.ToLower(c);
				}
				sb.Append(c);
			}

			return sb.ToString();
		}

		/// <summary>
		/// Expand a compressed capitalized name to a name with separate individual words
		/// E.g. ThisIsASentence -> This is a sentence
		/// </summary>
		/// <param name="txt"></param>
		/// <returns></returns>

		public static string CompressCapitalizedName(
			string txt)
		{
			StringBuilder sb = new StringBuilder();
			bool capitalize = false;
			for (int ci = 0; ci < txt.Length; ci++)
			{
				char c = txt[ci];
				if (c == ' ') capitalize = true;
				else
				{
					if (capitalize) c = Char.ToUpper(c);
					sb.Append(c);
					capitalize = false;
				}
			}

			return sb.ToString();
		}

		/// <summary>
		/// Return string value or blank if null (alias)
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>

		public static string S(object o)
		{
			return ToString(o);
		}
		/// <summary>
		/// Return string value or blank if null
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>

		public static string ToString(object o)
		{
			if (o == null) return "";
			else if (o is string) return o as string;
			else return o.ToString();
		}

		/// <summary>
		/// Convert multiple contiguous white space characters within a string to a single space characters
		/// trimming any beginning and end white space
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>

		public static String CompactWhitespace(String s)
		{
			if (Lex.IsUndefined(s)) return "";

			StringBuilder sb = new StringBuilder(s);

			int start = -1;

			for (int ci = 0; ci < s.Length; ci++)
			{
				if (Char.IsWhiteSpace(s[ci]))
				{
					if (start < 0)
						start = ci;
				}

				else
				{
					if (start >= 0)
					{
						if (start > 0) sb.Append(' '); // append space before this chunk if not first chunk
						start = -1;
					}
				}
			}

			return sb.ToString();
		}

	} // end of Lex class

	public class LexLineMetrics
	{
		public string[] Lines = null;
		public int LongestLine = 0;

		public int LineCnt
		{
			get
			{
				if (Lines != null) return Lines.Length;
				else return 0;
			}
		}

		public static LexLineMetrics Analyze(string txt)
		{
			int lineCnt = 0, li;
			LexLineMetrics i = new LexLineMetrics();
			string[] sa = txt.Split('\n');

			for (li = sa.Length - 1; li >= 0; li--)
			{
				if (Lex.IsDefined(sa[li])) break;
			}

			lineCnt = li + 1;

			i.Lines = new string[lineCnt];

			for (li = 0; li < lineCnt; li++)
			{
				string line = sa[li];
				if (line.Contains("\r")) line = line.Replace("\r", "");
				i.Lines[li] = line;
				if (line.Length > i.LongestLine) i.LongestLine = line.Length;
			}

			return i;
		}
	}

	/// <summary>
	/// Token with position information
	/// </summary>

	public class PositionedToken
	{
		public string Text; // text of the token
		public int Position; // position of 1st char in token in input stream
		public int LineNumber; // line number in input stream
		public int Length
		{
			get
			{
				if (Lex.IsNullOrEmpty(Text)) return 0;
				else return Text.Length;
			}
		}

		public PositionedToken()
		{
			return;
		}

		public PositionedToken(
			string initialText)
		{
			Text = initialText;
		}

		public PositionedToken Clone ()
		{
			return (PositionedToken)this.MemberwiseClone();
		}

	}

} // end of namespace
