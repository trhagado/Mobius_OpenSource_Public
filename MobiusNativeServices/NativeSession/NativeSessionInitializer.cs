using Mobius.ComOps;
using Mobius.MolLib1;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mobius.Services.Native
{

	/// <summary>
	/// Initialize native service session
	/// </summary>

	public class NativeSessionInitializer : MarshalByRefObject
	{
		private static object _lockObject = new object();

		public static NativeSessionInitializer Instance
		{
			get
			{
				if (_instance == null) 	_instance = new NativeSessionInitializer();
				return _instance;
			}
		}
		private static NativeSessionInitializer _instance;

		/// <summary>
		/// Constructor
		/// </summary>

		public NativeSessionInitializer()
		{
			return;
		}

		/// <summary>
		/// Get the location of the IniFile
		/// </summary>
		/// <returns></returns>

		public static string GetMobiusServicesIniFileLocation()
		{
			string configFileLocation = System.Web.Configuration.WebConfigurationManager.AppSettings.Get("IniFilePath");
			if (configFileLocation == null)
			{
				System.Configuration.AppSettingsReader settingsReader = new System.Configuration.AppSettingsReader();
				configFileLocation = (string)settingsReader.GetValue("IniFilePath", typeof(System.String));
			}
			return configFileLocation;
		}

		/// <summary>
		/// Initialize the UAL and load the MetaData
		/// </summary>

		//public void Initialize()
		//{
		//  InitializeUAL();
		//  LoadMetaData();
		//  return;
		//}

		/// <summary>
		/// Initialize the UAL so basic Oracle operations can be performed
		/// </summary>

		public void InitializeUAL()
		{
			lock (_lockObject)
			{

				// Set the ini file

				if (ServicesIniFile.IniFile == null)
				{
					string iniFilePath = GetMobiusServicesIniFileLocation();
					Mobius.UAL.UalUtil.Initialize(iniFilePath);
					if (!System.IO.File.Exists(iniFilePath))
						throw new Exception("IniFile doesn't exist: " + iniFilePath);
				}

				if (ServicesDirs.MetaDataDir == "")
					ServicesDirs.MetaDataDir = ServicesIniFile.Read("MetaData");

				string logFile = ServicesDirs.LogDir + @"\" + Mobius.ComOps.CommonConfigInfo.ServicesLogFileName; // log file name
				Mobius.Services.Util.ServicesLog.Initialize(logFile); // initialize for logging 

				// Get list of valid privileges

				Mobius.ComOps.PrivilegesMx.SetValidPrivilegeListFromInifile(ServicesIniFile.IniFile);

				// Load data source meta info

				if (Mobius.UAL.DataSourceMx.DataSources == null ||
						Mobius.UAL.DataSourceMx.DataSources.Count == 0 ||
						Mobius.UAL.DataSourceMx.Schemas == null ||
						Mobius.UAL.DataSourceMx.Schemas.Count == 0)
				{
					Mobius.UAL.DataSourceMx.LoadMetadata();
				}

				// Foundation dependency injections

				// Dependency injections for client components
				//Mobius.Data.Query.IDataTableManager = new DataTableManager();
				//Mobius.Data.ChemicalStructure.IChemicalStructureUtil = new Mobius.ClientComponents.ChemicalStructureUtil();
				//Mobius.Data.RootTable.IServerFile = new ServerFile();
				//Mobius.ServServiceFacade.ServiceFacade.IDebugLog = new DebugLogMediator();

				Mobius.Data.ResultsViewProps.ResultsViewFactory = new Mobius.ClientComponents.ViewFactory();
				Mobius.Data.Query.IDataTableManager = new Mobius.ClientComponents.DataTableManager();
				Mobius.Data.CidList.ICidListDao = new Mobius.UAL.CidListDao();
				Mobius.Data.MoleculeMx.IMoleculeMxUtil = new Mobius.QueryEngineLibrary.MoleculeUtil();
				Mobius.Data.RootTable.IServerFile = new Mobius.UAL.ServerFile();

				Mobius.Data.InterfaceRefs.IUserObjectDao = new Mobius.UAL.IUserObjectDaoMethods();
				Mobius.Data.InterfaceRefs.IUserObjectIUD = new Mobius.ClientComponents.IUserObjectIUDMethods();
				Mobius.Data.InterfaceRefs.IUserObjectTree = new Mobius.ClientComponents.IUserObjectTreeMethods();

				Mobius.MolLib1.StructureConverter.ICdkUtil = new Mobius.CdkMx.CdkUtil(); 
			}
		}

		/// <summary>
		/// Load dictionary, metatable and metatree metadata
		/// Note that this must be performed under the proper Mobius user since the 
		///  initial Oracle connections to VPN databases (e.g. Star) must include the proper username
		/// </summary> 

		public void LoadMetaData()
		{
			lock (_lockObject)
			{

				//  Load dictionary information

				if (Mobius.Data.DictionaryMx.Dictionaries == null ||
						Mobius.Data.DictionaryMx.Dictionaries.Count == 0)
				{
					Mobius.Data.DictionaryMx.LoadDictionaries(new Mobius.MetaFactoryNamespace.DictionaryFactory()); //adds ref and calls load
				}

				// Setup the metatable factory collection

				if (Mobius.MetaFactoryNamespace.MetaTableFactory.MetaFactories.Count == 0)
					Mobius.MetaFactoryNamespace.MetaTableFactory.Initialize();

				if (Mobius.Data.MetaTableCollection.MetaTableFactory == null)
					Mobius.Data.MetaTableCollection.MetaTableFactory = new Mobius.MetaFactoryNamespace.MetaTableFactory(); // factory to call to get metatables built

				Mobius.Data.MetaTable.KeyMetaTable = Mobius.Data.MetaTableCollection.GetDefaultRootMetaTable();

				// Load Metatree

				if (Mobius.Data.MetaTree.MetaTreeFactory == null)
				{
					Mobius.Data.MetaTree.MetaTreeFactory = new Mobius.MetaFactoryNamespace.MetaTreeFactory(); // factory to call to build metatree
					Mobius.Data.MetaTree.Nodes = Mobius.Data.MetaTree.MetaTreeFactory.GetMetaTree(); // make sure that it's built & referenced in MetaTree
				}

				//Mobius.ClientComponents.UserObjectTree.Initialize(); // initialize the UserObjectTree class

				// Initialize the QueryEngine

				if (Mobius.QueryEngineLibrary.MetaBrokerUtil.GlobalBrokers == null)
				{
					Mobius.QueryEngineLibrary.QueryEngine.InitializeForSession();
				}

			}
		}

		/// <summary>
		/// Initialize client-side components that may be called
		/// </summary>

		public static void InitializeClientComponents()
		{
			Mobius.ClientComponents.SessionState css = Mobius.ClientComponents.SS.I; // client session state

			//if (css.ServicesIniFile != null) return; // already initialized
			//css.ServicesIniFile = Mobius.UAL.UalUtil.ServicesIniFile; // copy already-defined services inifile from UAL

			css.UserInfo = Mobius.UAL.Security.UserInfo; // copy user info
			css.UserName = css.UserInfo.UserName;
			css.UserDomainName = css.UserInfo.UserDomainName;

			return;
		}

/// <summary>
/// ReloadContentsTree
/// </summary>
/// <returns></returns>

		public bool ReloadContentsTree()
		{
			bool succeeded = false;
			lock (_lockObject)
			{
				try
				{
					Mobius.MetaFactoryNamespace.MetaTreeFactory.Build(true); // rebuild from the factory (which may choose to reload from cache)

					//wait for the (re-)build to be complete...
					Mobius.MetaFactoryNamespace.MetaTreeFactory.WaitForInitializeCompletion();

					succeeded = true;
				}
				catch (Exception)
				{
					//probably an I/O error -- log?
				}
			}
			return succeeded;
		}

/// <summary>
/// IsContentsTreeStale
/// </summary>
/// <param name="cacheDate"></param>
/// <returns></returns>

		public bool IsContentsTreeStale(out DateTime cacheDate)
		{
			bool result = false;
			lock (_lockObject)
			{
				result = Mobius.MetaFactoryNamespace.MetaTreeFactory.IsCachedTreeStale(out cacheDate);
			}
			return result;
		}
	}
}
