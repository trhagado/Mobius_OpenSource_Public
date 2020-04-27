using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Net;
using System.Net.Mail;
using System.Management;
using System.Runtime.InteropServices;
using System.Collections.Specialized;

namespace Mobius.ComOps
{
	/// <summary>
	/// Mobius Reflection Utilities
	/// </summary>

	public class ReflectionMx
	{
		static List<Assembly> MobiusAssemblies = null;
		static int MobiusTypeCount = 0;
		static HashSet<string> ServiceAssemblyNames = null; // names of assemblies that are used primarily within the services

	/// <summary>
	/// Call an internal method whose module should already be loaded
	/// </summary>
	/// <param name="methodRef"></param>
	/// <param name="args"></param>
	/// <returns></returns>

	public static object CallMethod(
			string methodRef,
			object args)
		{
			string methodName;
			Type type;

			object target = GetObjectInstance(methodRef, out type, out methodName);

			object[] invokeArgs = new object[1];
			if (args != null) invokeArgs[0] = args.ToString(); // assume single string arg for now

			object returnValue = type.InvokeMember(
				methodName, // method to call
				BindingFlags.InvokeMethod | BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Static,
				null, // use default binder
				target, // object to call
				invokeArgs);

			return returnValue;
		}

/// <summary>
/// Get string value of member
/// </summary>
/// <param name="memberRef"></param>
/// <returns></returns>
		public static string GetMemberStringValue(
			string memberRef)
		{
			object o = GetMemberValue(memberRef);
			if (o == null) return null;
			else
			{
				string val = o.ToString();
				return val;
			}
		}

/// <summary>
/// Get value of member as an object
/// </summary>
/// <param name="memberRef"></param>
/// <returns></returns>
		public static object GetMemberValue(
			string memberRef)
		{
			string memberName;
			Type type;
			object oVal;

			object target = GetObjectInstance(memberRef, out type, out memberName);

			BindingFlags flags = BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Static;

			FieldInfo fi = type.GetField(memberName, flags);

			if (fi != null)
			{
				oVal = fi.GetValue(target);
				return oVal;
			}

			PropertyInfo pi = type.GetProperty("Name", flags);
			if (pi != null)
			{
				oVal = pi.GetValue(target, null);
				return oVal;
			}

			throw new Exception("Property/Field not found: " + memberRef);
		}


		/// <summary>
		/// Set value of static class member 
		/// </summary>
		/// <param name="memberRef"></param>
		/// <param name="value"></param>

		public static void SetMemberValue(
			string memberRef,
			object value)
		{
			string memberName;
			Type type;

			object target = GetObjectInstance(memberRef, out type, out memberName);

			BindingFlags flags = BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Static;

			FieldInfo fi = type.GetField(memberName, flags);

			if (fi != null)
			{
				value = ConvertToCompatibleValue(fi.FieldType, value);
				fi.SetValue(target, value);
				return;
			}

			PropertyInfo pi = type.GetProperty("Name", flags);
			if (pi != null)
			{
				value = ConvertToCompatibleValue(fi.FieldType, value);
				pi.SetValue(target, value, null);
				return;
			}

			throw new Exception("Property/Field not found: " + memberRef);
		}

		/// <summary>
		/// Convert a value to the specified type
		/// </summary>
		/// <param name="type"></param>
		/// <param name="value"></param>
		/// <returns></returns>

		public static object ConvertToCompatibleValue(
			Type type,
			object value)
		{

			if (value == null) return value;

			string valType = value.GetType().Name;

			if (Lex.Eq(type.Name, "String"))
				return Convert.ToString(value);

			else if (Lex.Eq(type.Name, "Boolean"))
				return Convert.ToBoolean(value);

				else if (Lex.Eq(type.Name, "Int16"))
				return Convert.ToInt16(value);

			else if (Lex.Eq(type.Name, "Int32"))
				return Convert.ToInt32(value);

			else if (Lex.Eq(type.Name, "Int64"))
				return Convert.ToInt64(value);

			else if (Lex.Eq(type.Name, "Single"))
				return Convert.ToSingle(value);

			else if (Lex.Eq(type.Name, "Double"))
				return Convert.ToDouble(value);

			else if (Lex.Eq(type.Name, "Decimal"))
				return Convert.ToDecimal(value);

			else return value;
		}


		/// <summary>
		/// Create instance of specified object type
		/// </summary>
		/// <param name="methodRef"></param>
		/// <returns></returns>
		public static object GetObjectInstance(
			string methodRef,
			out Type type,
			out string memberName)
		{
			int i1 = methodRef.LastIndexOf(".");
			string className = methodRef.Substring(0, i1);
			memberName = methodRef.Substring(i1 + 1);
			type = GetClassType(className);
			if (type == null) throw new Exception("Class not found in loaded assemblies: " + className);



			object target = type.Assembly.CreateInstance(type.FullName); // create instance object
			return target;
		}

		/// <summary>
		/// GetClassType via search of loaded assemblies
		/// </summary>
		/// <param name="className"></param>
		/// <returns></returns>

		public static Type GetClassType(
			string className)
		{
			List<Assembly> assemblies;
			bool classNameIncludesNamespace = className.Contains(".");
			int ti=0;

			if (MobiusAssemblies == null) GetMobiusAssemblies();
			
			foreach (Assembly a in MobiusAssemblies)
			{
				//if (Lex.Contains(a.FullName, "QueryEngine")) ti = ti; // debug
				Type[] types = a.GetTypes();
				string assyName = a.FullName;
				for (ti = 0; ti < types.Length; ti++)
				{
					Type t = types[ti];
					if ((classNameIncludesNamespace && Lex.Eq(t.FullName, className)) || // match on full namespace.className
						(!classNameIncludesNamespace && (Lex.Eq(t.Name, className)))) // just match on className
					{
						return t; // found it
					}
				}
			}

			return null;
		}


		/// <summary>
		/// If class referenced in the memberRef is used primarily through the servicefacade then values & methods should apply only to 
		/// the native services process associated with a session and not with the client.
		/// </summary>
		/// <param name="memberRef"></param>
		/// <returns></returns>

		public static bool IsServiceClassMemberRef(
			string memberRef)
		{
			int i1 = memberRef.LastIndexOf(".");
			string className = memberRef.Substring(0, i1);
			return IsServiceClass(className);
		}

		/// <summary>
		/// If class is used primarily through the servicefacade then values & methods should apply only to 
		/// the native services process associated with a session and not with the client.
		/// </summary>
		/// <param name="className"></param>
		/// <returns></returns>

		public static bool IsServiceClass(
			string className)
		{
			Type t = GetClassType(className);
			if (t == null) return false;

			if (ServiceAssemblyNames.Contains(t.Assembly.FullName)) return true;
			else return false;
		}

		/// <summary>
		/// Scan all loaded assemblies and get list of those that contain Mobius types
		/// </summary>

		static void GetMobiusAssemblies()
		{
			HashSet<Assembly> aDict = new HashSet<Assembly>();
			HashSet<string> aNamesTriedDict = new HashSet<string>();
			Assembly serviceFacadeAssembly = null; // assembly that contains the ServiceFacade classes
			HashSet<string> serviceFacadeClassNames = new HashSet<string>(); // names of classes in ServiceFacade
			Type[] types;

			Assembly[] initialAssemblies = AppDomain.CurrentDomain.GetAssemblies();

			foreach (Assembly a in initialAssemblies) // Get all Mobius assemblies from initial set
			{
				aNamesTriedDict.Add(a.FullName);
				if (!IsMobiusAssembly(a)) continue;
				aDict.Add(a);
			}

			MobiusAssemblies = new List<Assembly>(aDict); // initial set of Mobius assemblies
			foreach (Assembly a in MobiusAssemblies) // Check referenced assemblies not initially loaded
			{
				CheckReferencedAssemblies(a, aDict, aNamesTriedDict);
			}

			MobiusAssemblies = new List<Assembly>(aDict); // get full set of Mobius assemblies

			foreach (Assembly a in MobiusAssemblies) // look for ServiceFacade assembly
			{
				if (Lex.Contains(a.FullName, "ServiceFacade"))
				{
					serviceFacadeAssembly = a;
					continue;
				}
			}

			ServiceAssemblyNames = new HashSet<string>();

			if (serviceFacadeAssembly == null) return; // just return if no serviceFacadeAssembly

			MobiusAssemblies.Remove(serviceFacadeAssembly);

// Get list of assemblies that are primarily service assemblies

			serviceFacadeClassNames = new HashSet<string>(); // build hash set of service facade class names
			types = serviceFacadeAssembly.GetTypes();
			for (int ti = 0; ti < types.Length; ti++)
			{
				Type t = types[ti];
				serviceFacadeClassNames.Add(t.Name);
			}

			foreach (Assembly a in MobiusAssemblies) // see if any class in assemble matches a class name in the service facade
			{
				types = a.GetTypes();
				for (int ti = 0; ti < types.Length; ti++)
				{
					Type t = types[ti];
					if (serviceFacadeClassNames.Contains(t.Name))
					{
						ServiceAssemblyNames.Add(a.FullName);
						break;
					}
				}
			}

			// Put ServiceFacade assembly last so non-facade classes are found first first

			MobiusAssemblies.Add(serviceFacadeAssembly);

			return;
		}

		static void CheckReferencedAssemblies(
			Assembly assembly,
			HashSet<Assembly> aDict,
			HashSet<string> aNamesTriedDict)
		{
			Assembly a2 = null;

			string dir = Path.GetDirectoryName(assembly.Location) + @"\";

			AssemblyName[] referencedAssemblies = assembly.GetReferencedAssemblies();

			foreach (AssemblyName aName in referencedAssemblies)
			{
				//if (Lex.Contains(aName.FullName, "SimSearchMx")) aNamesTriedDict = aNamesTriedDict; // debug

				if (aNamesTriedDict.Contains(aName.FullName)) continue;
				aNamesTriedDict.Add(aName.FullName);

				string aFileName = dir + aName.Name;
				if (!Lex.EndsWith(aFileName, ".dll")) aFileName += ".dll";
				if (!File.Exists(aFileName)) continue;

				try { a2 = Assembly.LoadFrom(aFileName); }
				catch (Exception ex) { continue; }

				if (!IsMobiusAssembly(assembly)) continue;

				aDict.Add(assembly);

				CheckReferencedAssemblies(a2, aDict, aNamesTriedDict); // recur
			}
		}

		public static bool IsMobiusAssembly(Assembly a)
		{
			if (a.GlobalAssemblyCache) return false; // ignore GAC assemblies

			Type[] types = a.GetTypes();
			string assyName = a.FullName;
			for (int ti = 0; ti < types.Length; ti++)
			{
				Type t = types[ti];
				if (Lex.Contains(t.FullName, "Mobius"))
				{
					MobiusTypeCount += types.Length; // count types
					return true;
				}
			}

			return false;
		}


	}
}
