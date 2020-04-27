using Mobius.ComOps;
using Mobius.Data;
using Mobius.ClientComponents;
using Mobius.ServiceFacade;

using System;
using System.Data;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Xml;
using System.Reflection;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
/// <summary>
/// Class for individual plugin
/// </summary>

	public class Plugin
	{
		public string Id = ""; 
		public string Name = ""; // plugin name, also command name for command plugins
		public string ProviderName = "";
		public string Class = ""; // class that contains plugin functionality
		public string Version = "";
		public bool Enabled = true;
		public bool Visible = true;
		public List<string> RunTimeLibraries = new List<string>(); // list of client side assembly dlls
		public List<ExtensionPoint> ExtensionPoints = new List<ExtensionPoint>(); // list of extension points
		public string Directory = ""; // directory containing the plugin
		public object Instance = null; // pointer to loaded instance of plugin class
	}

/// <summary>
/// Extension point within a plugin
/// </summary>

	public class ExtensionPoint
	{
		public string Id = "";
		public string Type = ""; // Mobius.Action, Mobius.Dialog or Mobius.Method
		public string MenuBarPath = ""; // path to menu to add to
		public string MethodName = ""; // name of associated method
		public string CommandName = ""; // command line command name
		public string Label = ""; // label for menu item
		public string Class = ""; // class that contains extension point functionality
		public Plugin Plugin = null; // plugin that this extension point belongs to
		public object Instance = null; // pointer to loaded instance of extension point class
		public Image Image = null;  // image to use with command
	}

/// <summary>
/// Code to manage Mobius plugins
/// </summary>

	public class Plugins
	{
		static List<Plugin> PluginList = null;
		static Dictionary<string,object> ClientAssembliesLoaded = null; 

/// <summary>
/// Load plugin definitions
/// </summary>
/// <param name="pluginRootDir"></param>
/// <returns></returns>

		public static int LoadDefinitions(string pluginRootDir)
		{
			if (PluginList != null) return PluginList.Count;
			PluginList = new List<Plugin>();

			string[] dirs = Directory.GetDirectories(pluginRootDir);
			foreach (string dir in dirs)
			{
				if (Lex.EndsWith(dir, @"\TargetResultsViewerNetwork") || // obsolete
					Lex.EndsWith(dir, @"\RgroupMatrix")) // disabled
						continue; // ignore

				string fileName = dir + @"\plugin.xml";
//				ClientLog.Message("Loading Plugin: " + fileName); // debug
				if (!File.Exists(fileName)) continue;
				try
				{
					XmlDocument doc = new XmlDocument();
					doc.Load(fileName);
					XmlNodeList nodes = doc.GetElementsByTagName("plugin"); // get all plugins in file
					if (nodes == null || nodes.Count == 0) return 0;

					foreach (XmlNode node in nodes)
					{
						Plugin p = new Plugin();
						p.Directory = dir;


						foreach (XmlAttribute attr in node.Attributes)
						{
							if (Lex.Eq(attr.Name, "Id"))
								p.Id = attr.Value;

							else if (Lex.Eq(attr.Name, "Name"))
								p.Name = attr.Value;

							else if (Lex.Eq(attr.Name, "Class"))
								p.Class = attr.Value;

							else if (Lex.Eq(attr.Name, "Provider-Name"))
								p.ProviderName = attr.Value;

							else if (Lex.Eq(attr.Name, "Version"))
							{
								p.Version = attr.Value;

								if (Lex.Eq(attr.Value, "Hidden")) p.Visible = false; // temp workaround to hide a plugin
							}

							else if (Lex.Eq(attr.Name, "Enabled"))
								bool.TryParse(attr.Value, out p.Enabled);

							else if (Lex.Eq(attr.Name, "Visible"))
								bool.TryParse(attr.Value, out p.Visible);

							else throw new Exception("Unrecognized attribute: " + attr.Name);
						}

						if (p.Id == "")
							throw new Exception("Id missing");

						if (p.Class == "")
							throw new Exception("Class name missing");

						XmlNodeList libraryNodes = node.SelectNodes(".//library"); // get library nodes (case sensitive)
						if (libraryNodes.Count == 0) libraryNodes = node.SelectNodes(".//Library");
						if (libraryNodes.Count == 0) libraryNodes = node.SelectNodes(".//LIBRARY");

						foreach (XmlNode node2 in libraryNodes)
						{
							if (node2.Attributes == null || node2.Attributes.Count < 1 ||
								!Lex.Eq(node2.Attributes[0].Name, "Name"))
								throw new Exception("Runtime Library Name missing");
							p.RunTimeLibraries.Add(node2.Attributes[0].Value);
						}

						if (p.RunTimeLibraries.Count == 0)
							throw new Exception("Runtime library missing");

						XmlNodeList eNodes = node.SelectNodes(".//extension"); // get extension point nodes
						if (eNodes == null || eNodes.Count == 0)
							throw new Exception("Extension points missing");

						foreach (XmlNode eNode in eNodes)
						{
							ExtensionPoint ep = new ExtensionPoint();
							ep.Plugin = p;

							foreach (XmlAttribute attr in eNode.Attributes)
							{
								if (Lex.Eq(attr.Name, "Point"))
								{
									if (Lex.Ne(attr.Value, "Mobius.Action") && Lex.Ne(attr.Value, "Mobius.ClientDialog") &&
										Lex.Ne(attr.Value, "Mobius.Method"))
										throw new Exception("Invalid extension point type " + attr.Value);
									ep.Type = attr.Value;
								}

								else if (Lex.Eq(attr.Name, "Id"))
									ep.Id = attr.Value;

								else if (Lex.Eq(attr.Name, "Class"))
									ep.Class = attr.Value;

								else if (Lex.Eq(attr.Name, "MenuBarPath"))
								{
									if (Lex.Ne(attr.Value, "Tools"))
										throw new Exception("Unsupported MenubarPath " + attr.Value);
									ep.MenuBarPath = attr.Value;
								}

								else if (Lex.Eq(attr.Name, "CommandLineCommand"))
									ep.CommandName = attr.Value;

								else if (Lex.Eq(attr.Name, "MethodName"))
									ep.MethodName = attr.Value;

								else if (Lex.Eq(attr.Name, "Label"))
									ep.Label = attr.Value;

								else throw new Exception("Unrecognized extension point attribute: " + attr.Name);
							}

							if (ep.Id == "")
								throw new Exception("Extension point Id missing");
							if (ep.Type == "")
								throw new Exception("Extension point type missing");
							if (ep.Class == "")
							{
								if (p.Class != "") ep.Class = p.Class; // use plugin class if defined
								else throw new Exception("Extension point class name missing");
							}

							fileName = dir + @"\" + ep.Id + ".bmp";
							if (File.Exists(fileName))
							{
								try { ep.Image = Bitmap.FromFile(fileName); }
								catch (Exception ex) { ex = ex; }
							}

							p.ExtensionPoints.Add(ep); // add extension point to plugin
						}

						PluginList.Add(p); // add plugin to list
					}
				}
				catch (Exception ex)
				{
					ServicesLog.Message(DebugLog.FormatExceptionMessage(ex));
					string msg = "Error in file \"" + fileName + "\"\r\n" + ex.Message;
					throw new Exception(msg, ex);
				}
			}

			PluginCaller.CallRef = // alloc PluginDao to call up to us
				new PluginCaller.CallMethodDelegate(CallStringExtensionPointMethod);

			return PluginList.Count;
		}

/// <summary>
/// Update user interface with plugin additions
/// </summary>

		public static void UpdateUI()
		{
			if (PluginList == null) return;

			foreach (Plugin p in PluginList)
			{
				if (!p.Visible) continue;

				foreach (ExtensionPoint e in p.ExtensionPoints)
				{
					if (Lex.Ne(e.Type, "Mobius.Action")) continue;

					if (e.MenuBarPath != "") // add menu item
					{
						SessionManager.Instance.MainMenuControl.AddToolsMenuPluginItem(e.Label, "PluginCommand " + e.Id, e.Image, p.Visible, p.Enabled);
					}

					else if (e.CommandName != "" && p.Visible && p.Enabled) // add command line command
					{
						CommandLine.AddCommand(e.CommandName, e, 0, false, true);
					}
				}
			}
		}

/// <summary>
/// Update & Load client assembly containing class as necessary
/// </summary>
/// <param name="className"></param>

		public static void LoadClientAssembly
			(string className)
		{
			if (ClientAssembliesLoaded == null)
			 ClientAssembliesLoaded = new Dictionary<string,object>();

			ExtensionPoint ep = GetExtensionPointByClassName(className);
			if (ep == null) return;
			Plugin p = ep.Plugin;

			foreach (string assemblyName in p.RunTimeLibraries)
			{
				if (ClientAssembliesLoaded.ContainsKey(assemblyName)) continue; // just continue if already loaded

				string clientAssemblyPath = // path to assembly on client
					ClientDirs.StartupDir + @"\" + Path.GetFileName(assemblyName);
				string clientVersion = SystemUtil.GetAssemblyVersion(clientAssemblyPath);

				//string serverAssemblyPath = assemblyName; // path to assembly on server
				//if (!Path.IsPathRooted(serverAssemblyPath)) // if not rooted look in plugin dir for assembly
				//  serverAssemblyPath = p.Directory + @"\" + assemblyName;

				//bool serverAssemblyExists = File.Exists(serverAssemblyPath);
				//if (!serverAssemblyExists) // if not there try in server startup path
				//{
				//  serverAssemblyPath = ProcessState.ServerStartupPath + @"\" + assemblyName;
				//  serverAssemblyExists = File.Exists(serverAssemblyPath);
				//}

				//if (serverAssemblyExists)
				//{ // if found on server see if same version on client otherwise assume standard part of client assembly sets
				//  AssemblyName an = AssemblyName.GetAssemblyName(serverAssemblyPath);
				//  string serverVersion = an.Version.ToString();

				//  if (clientVersion != serverVersion) // download newer assembly if needed
				//    Client.PutFile(serverAssemblyPath, clientAssemblyPath);
				//}

				SystemUtil.LoadAssembly(clientAssemblyPath);

				ClientAssembliesLoaded[assemblyName] = null; // add to dict of loaded assemblies
			}
			return;
		}

/// <summary>
/// Lookup plugin by name
/// </summary>
/// <param name="className"></param>
/// <returns></returns>

		public static Plugin GetPluginByName(
			string name)
		{
			if (PluginList == null) return null;

			foreach (Plugin p in PluginList)
			{
				if (Lex.Eq(p.Name, name)) return p;
			}
			return null;
		}

/// <summary>
/// Return any extension point associated with the supplied class name
/// </summary>
/// <param name="className"></param>
/// <returns></returns>

		public static ExtensionPoint GetExtensionPointByClassName(
			string className)
		{
			if (PluginList == null) return null;

			foreach (Plugin p in PluginList)
			{
				foreach (ExtensionPoint ep in p.ExtensionPoints)
				{
					if (Lex.Eq(ep.Class, className)) return ep;
				}
			}
			return null;
		}

/// <summary>
/// Call a plugin extension point run method
/// </summary>
/// <param name="extensionId"></param>
/// <param name="args"></param>
/// <returns>Method return value converted to string</returns>

		public static string CallExtensionPointRunMethod (
			string extensionId,
			string args)
		{
			List<object> argList = new List<object>();
			argList.Add(args);
			return CallStringExtensionPointMethod(extensionId + ".Run", argList);
		}

/// <summary>
/// Return true if specified method reference (ExtensionPointId.MethodName) is part
/// of a valid extension point
/// </summary>
/// <param name="methodRef"></param>
/// <returns></returns>
 
		public static bool IsMethodExtensionPoint(
			string methodRef)
		{
			ExtensionPoint ep = GetMethodExtensionPoint (methodRef);
			if (ep != null) return true;
			else return false;
		}

/// <summary>
/// Lookup method extension point for specified method reference (ExtensionPointId.MethodName)
/// </summary>
/// <param name="methodRef"></param>
/// <returns></returns>

		public static ExtensionPoint GetMethodExtensionPoint (
			string methodRef)
		{
			int i1 = methodRef.LastIndexOf(".");
			string extensionId = methodRef.Substring(0, i1);
			string methodName = methodRef.Substring(i1 + 1);

			if (PluginList == null) return null;

			foreach (Plugin p in PluginList)
			{
				foreach (ExtensionPoint ep in p.ExtensionPoints)
				{
					if (Lex.Ne(ep.Type, "Mobius.Action") &&
						Lex.Ne(ep.Type, "Mobius.Method")) continue;

					if (Lex.Ne(ep.Id, extensionId)) continue;

					if (Lex.Eq(ep.MethodName, methodName)) return ep;
				}
			}

			return null;
		}

		/// <summary>
		/// Call an extension point method returning a string value
		/// </summary>
		/// <param name="methodRef">extensionPointId.method or nameSpace.class.method</param>
		/// <param name="args"></param>
		/// <returns></returns>

		public static string CallStringExtensionPointMethod(
			string methodRef,
			List<object> args)
		{
			object returnValue = CallExtensionPointMethod(methodRef, args);
			if (returnValue == null) return null;
			else return returnValue.ToString();
		}

		public static DialogResult CallDialogResultExtensionPointMethod(
			string methodRef,
			List<object> args)
		{
			object returnValue = CallExtensionPointMethod(methodRef, args);
			if (returnValue == null) return DialogResult.Cancel;
			else return (DialogResult)returnValue;
		}

		/// <summary>
		/// Call an extension point method returning an object value
		/// </summary>
		/// <param name="methodRef">extensionPointId.method or nameSpace.class.method</param>
		/// <param name="args"></param>
		/// <returns></returns>

		public static object CallExtensionPointMethod(
			string methodRef,
			List<object> args)
		{
			object[] invokeArgs;

			if (PluginList == null) throw new Exception("PluginList not defined");

			int i1 = methodRef.LastIndexOf(".");
			string extensionId = methodRef.Substring(0, i1);
			string methodName = methodRef.Substring(i1 + 1);

			foreach (Plugin p in PluginList)
			{
				foreach (ExtensionPoint ep in p.ExtensionPoints)
				{
					if (Lex.Ne(ep.Type, "Mobius.Action") &&
						Lex.Ne(ep.Type, "Mobius.Method")) continue;

					if (Lex.Ne(ep.Id, extensionId)) continue;

					if (ep.Instance == null) ep.Instance = GetExtensionPointClassInstance(ep);

					Type type = ep.Instance.GetType();
					object target = ep.Instance;
					if (args == null) invokeArgs = null;
					else invokeArgs = args.ToArray(); // convert list to array

					BindingFlags bindFlags =
						BindingFlags.InvokeMethod |
						BindingFlags.IgnoreCase |
						BindingFlags.Instance |
						BindingFlags.Static |
						BindingFlags.Public |
						//					BindingFlags.NonPublic |
						BindingFlags.FlattenHierarchy;

					object returnValue = type.InvokeMember(
						methodName, // method to call
						bindFlags, // binding flags
						null, // use default binder
						target, // object to call
						invokeArgs); // args in array must match method args in number & type

					return returnValue;
				}
			}

			throw new Exception("Can't find plugin for extensionId: " + extensionId);
		}

		/// <summary>
		/// Create an instance of the specified class & return it
		/// </summary>
		/// <param name="plugin"></param>
		/// <param name="extensionPoint"></param>
		/// <returns></returns>

		public static object GetExtensionPointClassInstance(
			ExtensionPoint extensionPoint)
		{
			Plugin plugin = extensionPoint.Plugin;
			foreach (string assemblyName in plugin.RunTimeLibraries)
			{ // look for class in each assembly associated with plugin
				string assemblyPath = assemblyName;

				Assembly assembly = null;
				if (!Path.IsPathRooted(assemblyPath)) // if not rooted look in plugin dir for assembly
				{
					AssemblyName an = new AssemblyName();
					an.CodeBase = plugin.Directory + @"\" + assemblyPath;
					try { assembly = Assembly.Load(an); }
					catch (Exception ex) { string msg = ex.Message; } // fall through on exception

					if (assembly == null) // if not found try startup binary path
					{
						an.CodeBase = ClientDirs.StartupDir + @"\" + assemblyPath;
						try { assembly = Assembly.Load(an); }
						catch (Exception ex) { string msg = ex.Message; } // fall through on exception
					}
				}

				if (assembly == null) // if not found in plugin dir try assemblyPath as is
				{
					AssemblyName an = new AssemblyName();
					an.CodeBase = assemblyPath;
					assembly = Assembly.Load(an);
				}

				Type type = assembly.GetType(extensionPoint.Class, false, true); // get the type not thowing exception if fails & ignoring case
				if (type == null) // if failed try getting with fully qualified name throwing exception if fails
				{
					foreach (Type t in assembly.GetTypes())
					{
						if (String.Compare(t.Name, extensionPoint.Class, true) == 0)
						{
							type = t;
							break;
						}
					}
				}
				if (type != null)
				{
					object instance = assembly.CreateInstance(type.FullName); // create instance object
					return instance;
				}
			}

			throw new Exception("Can't find assembly for class: " + extensionPoint.Class);
		}
	}

	/// <summary>
	/// Call external tools
	/// </summary>

	public class PluginCaller
	{
		public delegate object CallMethodDelegate(
			string methodRef,
			List<object> args);

		public static CallMethodDelegate CallRef;

		/// <summary>
		/// Call a tool method
		/// </summary>
		/// <param name="methodRef">reference to method</param>
		/// <param name="args">Argument array</param>
		/// <returns>Result object</returns>
		/// 
		public static object CallPluginMethod(
			string methodRef,
			List<object> args)
		{
			if (CallRef == null)
				throw new Exception("CallMethodDelegateRef not defined");

			object result = CallRef(methodRef, args);
			return result;
		}
	}

/// <summary>
/// Interface for plugins that execute actions in response to user input
/// </summary>

	public interface IActionDelegate
	{

/// <summary>
/// Main entry point for tool
/// </summary>
/// <param name="args"></param>
/// <returns></returns>

		string Run (
			string args);
	}

}
