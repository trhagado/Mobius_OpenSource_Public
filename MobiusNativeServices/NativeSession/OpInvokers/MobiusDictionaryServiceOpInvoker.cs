using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mobius.Services.Util.TypeConversionUtil;
using Mobius.Services.Types;
using Mobius.Services.Native.ServiceOpCodes;

namespace Mobius.Services.Native.OpInvokers
{
	public class MobiusDictionaryServiceOpInvoker : IInvokeServiceOps
	{
		private static object _lockObject = new object();
		private static MetaFactoryNamespace.DictionaryFactory _dictionaryFactoryInstance = null;

		private static TypeConversionHelper _transHelper = new TypeConversionHelper(TypeConversionHelper.ContextRetentionMode.NoRetention);

		#region IInvokeServiceOps Members

		object IInvokeServiceOps.InvokeServiceOperation(int opCode, object[] args)
		{
			MobiusDictionaryService op = (MobiusDictionaryService)opCode;
			switch (op)
			{

				case MobiusDictionaryService.GetBaseDictionaries:
					{
						//if (Mobius.Data.DictionaryMx.DictionaryFactory == null;
						if (_dictionaryFactoryInstance == null)
						{
							lock (_lockObject)
							{
								_dictionaryFactoryInstance = new Mobius.MetaFactoryNamespace.DictionaryFactory();
								Mobius.Data.DictionaryMx.LoadDictionaries(_dictionaryFactoryInstance);

								Mobius.Data.DictionaryMx secondaryMessages = // see if secondary debug error messages defined
									Mobius.Data.DictionaryMx.Get("SecondaryErrorMessages");

								if (secondaryMessages != null)
									Mobius.ComOps.DebugLog.SecondaryErrorMessages = secondaryMessages.WordLookup;
							}
						}

						Dictionary<string, DictionaryMx> metaDataDictionary =
							_transHelper.Convert<Dictionary<string, Mobius.Data.DictionaryMx>,
								Dictionary<string, DictionaryMx>>(Mobius.Data.DictionaryMx.Dictionaries);

						return metaDataDictionary;
					}


				case MobiusDictionaryService.GetDictionaryByName:
					{ // serialize dictionary into a text string & return
						string dictName = (string)args[0];
						Data.DictionaryMx dict = _dictionaryFactoryInstance.GetDefinitions(dictName);
						string txt = dict.Serialize();
						return txt;
					}
			}
			return null;
		}

		#endregion
	}
}
