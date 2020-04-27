using System;
using System.Collections.Generic;

namespace Mobius.Data
{
	/// <summary>
	/// Interface to factory for creating dictionaries
	/// </summary>

	public interface IDictionaryFactory
	{

		/// <summary>
		/// Load metadata describing dictionaries
		/// </summary>
		/// <param name="dictFileName"></param>
		/// <returns></returns>

		Dictionary<string, DictionaryMx> LoadDictionaries(); 

/// <summary>
/// Get words and definitions for dictionary, usually from an Oracle database
/// </summary>
/// <param name="dict"></param>
/// <returns></returns>

		void GetDefinitions ( DictionaryMx dict);
	}
}
