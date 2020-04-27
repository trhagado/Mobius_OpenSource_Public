using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mobius.ComOps
{
	/// <summary>
	/// Codes for common Unicode characters
	/// UnicodeString return strings rather than characters for convenience
	/// To lookup a character see: https://en.wikipedia.org/wiki/List_of_Unicode_characters
	/// </summary>
	public class UnicodeString
	{
		public const string Space = "\u0200";
		public const string Bullet = "\u2022";
		public const string NotEqual = "\u2260"; // equal sign with slash
		public const string LessOrEqual = "\u2264";
		public const string GreaterOrEqual = "\u2265";
	}
}
