using Mobius.ComOps;
using Mobius.Data;
using Mobius.MetaFactoryNamespace;

using Qel = Mobius.QueryEngineLibrary;
using System;
using System.Collections.Generic;
using System.Text; 

using ServiceTypes = Mobius.Services.Types;
using Native = Mobius.Services.Native;


namespace Mobius.ServiceFacade
{

/// <summary>
/// Facade class for Mobius dictionaries
/// </summary>

	public class DictionaryFactory : IDictionaryFactory
	{
		IDictionaryFactory DictionaryFactoryInstance; // real new or old service factory

		public DictionaryFactory()
		{
			if (ServiceFacade.UseRemoteServices)
			{
				//Mobius.Data.DictionaryMx expects to be used to maintain a local cache and
				// the individual "gets" require that DictionaryMx.Dictionaries isn't null
				// but the caller of "Dictionary<string, DictionaryMx> GetDictionaryMetaData()"
				// is expected to take care of this
			}
			else DictionaryFactoryInstance = new MetaFactoryNamespace.DictionaryFactory();
		}

		/// <summary>
		/// Get the set of dictionaries and those definitions contained in the base XML
		/// </summary>
		/// <returns></returns>

		public Dictionary<string, DictionaryMx> LoadDictionaries()
		{
			Dictionary<string, DictionaryMx> dicts = null;

			if (ServiceFacade.UseRemoteServices)
			{
				Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject = ServiceFacade.InvokeNativeMethod(nativeClient,
						(int)Native.ServiceCodes.MobiusDictionaryService,
						(int)Native.ServiceOpCodes.MobiusDictionaryService.GetBaseDictionaries,
						new Services.Native.NativeMethodTransportObject(new object[] { false }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				if (resultObject == null) return null;
				Dictionary<string, ServiceTypes.DictionaryMx> serviceDictionaryMxs =
					(Dictionary<string, ServiceTypes.DictionaryMx>)resultObject.Value;
				dicts =	ServiceFacade.TypeConversionHelper.Convert<
					Dictionary<string, ServiceTypes.DictionaryMx>,
					Dictionary<string, DictionaryMx>>(serviceDictionaryMxs);
				return dicts;
			}

			else
			{
				dicts = DictionaryFactoryInstance.LoadDictionaries();
				DictionaryMx.Dictionaries = dicts;

				DictionaryMx secondaryMessages = // see if secondary debug error messages defined
					DictionaryMx.Get("SecondaryErrorMessages");

				if (secondaryMessages != null)
					DebugLog.SecondaryErrorMessages = secondaryMessages.WordLookup;

				return dicts;
			}
		}

		/// <summary>
		/// Get words and definitions for dictionary, usually from an Oracle database
		/// </summary>
		/// <param name="dict"></param>
		/// <returns></returns>

		public void GetDefinitions(DictionaryMx dict)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				//Mobius.Data.DictionaryMx expects to be used to maintain the local cache
				// -- and I expect the static member that holds that cache is not null
				if (DictionaryMx.Dictionaries == null) throw new Exception("Dictionary XML not loaded");

				if (dict != null && !String.IsNullOrEmpty(dict.Name))
				{
						Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
						Services.Native.NativeMethodTransportObject resultObject = ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Native.ServiceCodes.MobiusDictionaryService,
								(int)Native.ServiceOpCodes.MobiusDictionaryService.GetDictionaryByName,
								new Services.Native.NativeMethodTransportObject(new object[] { dict.Name }));
						((System.ServiceModel.IClientChannel)nativeClient).Close();

						if (resultObject == null || resultObject.Value == null) return;

						string txt = resultObject.Value.ToString();
						DictionaryMx.Deserialize(txt, dict);

						return;
				}
			}

			else
			{
				DictionaryFactoryInstance.GetDefinitions(dict);
				string txt = dict.Serialize();
				DictionaryMx.Deserialize(txt, dict);
			}
		}

	}

}
