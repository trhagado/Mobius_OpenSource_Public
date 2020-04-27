using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 

namespace Mobius.ComOps 
{

	/// <summary>
	/// Mobius Assertertion class
	/// Based on Microsoft.VisualStudio.TestTools.UnitTesting.Assert class
	/// </summary>

	public class AssertMx
	{

		/// <summary>
		/// Tests whether the specified condition is true and throws an exception if the condition is false.
		/// </summary>
		/// <param name="condition"></param>
		/// <param name="message"></param>

		public static void IsTrue(
			bool condition,
			string message = null)
		{
			if (condition == true) return;

			if (message == null) message = "Expected condition to be true";

			throw new InvalidOperationException(message);
		}

		/// <summary>
		/// Tests whether the specified condition is false and throws an exception if the condition is true.
		/// </summary>
		/// <param name="condition"></param>
		/// <param name="message"></param>

		public static void IsFalse(
			bool condition,
			string message = null)
		{
			if (condition == false) return;

			if (message == null) message = "Expected condition to be false";

			throw new InvalidOperationException(message);
		}

		/// <summary>
		/// Tests whether the specified object is non-null and throws an exception if it is null.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="message"></param>

		public static void IsNotNull(
			object value, 
			string message = null)
		{
			if (value != null) return;

			if (message == null) message = "Unexpected null value";
			else if (!Lex.Contains(message, "null"))
				message += " is null";

			throw new NullReferenceException(message);
		}

		/// <summary>
		/// Tests whether the specified object is null and throws an exception if it is not.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="message"></param>

		public static void IsNull(
			object value,
			string message = null)
		{
			if (value == null) return;

			if (message == null) message = "Expected null value";
			else if (!Lex.Contains(message, "null"))
				message += " is not null";

			throw new NullReferenceException(message);
		}

		/// <summary>
		/// Tests whether the specified string is defined
		/// </summary>
		/// <param name="value"></param>
		/// <param name="message"></param>

		public static void IsDefined(
			string value,
			string message = null)
		{
			if (!String.IsNullOrWhiteSpace(value)) return;

			if (message == null) message = "Unexpected undefined string value";
			else if (!Lex.Contains(message, "defined"))
				message += " is undefined";

			throw new NullReferenceException(message);
		}

		/// <summary>
		/// Tests whether the specified string is undefined
		/// </summary>
		/// <param name="value"></param>
		/// <param name="message"></param>

		public static void IsUndefined(
			string value,
			string message = null)
		{
			if (String.IsNullOrWhiteSpace(value)) return;

			if (message == null) message = "Expected undefined string value";
			else if (!Lex.Contains(message, "defined"))
				message += " is not undefined";

			throw new NullReferenceException(message);
		}




	}
}
