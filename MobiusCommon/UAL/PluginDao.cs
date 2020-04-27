using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Mobius.UAL
{
/// <summary>
/// Call external tools
/// </summary>
	public class PluginDao
	{
		public delegate object CallMethodDelegate (
			string methodRef,
			List<object> args);

		public static CallMethodDelegate CallMethodDelegateRef;
		public static Assembly AssemblyLastCalled;

/// <summary>
/// Call a tool method
/// </summary>
		/// <param name="methodRef">reference to method</param>
/// <param name="args">Argument array</param>
/// <returns>Result object</returns>
/// 
		public static object CallPluginMethod (
			string methodRef,
			List<object> args)
		{
			if (CallMethodDelegateRef == null)
				throw new Exception("CallMethodDelegateRef not defined");

			object result = CallMethodDelegateRef(methodRef, args);
			return result;
		}

/// <summary>
/// Call an internal method whose module should already be loaded
/// </summary>
/// <param name="methodRef"></param>
/// <param name="args"></param>
/// <returns></returns>

		public static object CallInternalMethod (
			string methodRef,
			List<object> args)
		{
			Type type = null;
			int i1 = methodRef.LastIndexOf(".");
			string className = methodRef.Substring(0, i1);
			string methodName = methodRef.Substring(i1 + 1);

			if (AssemblyLastCalled != null)
				type = AssemblyLastCalled.GetType(className, false, true);

			if (type == null)
			{
				foreach (Assembly a in Thread.GetDomain().GetAssemblies())
				{
					type = a.GetType(className, false, true);
					if (type != null)
					{
						AssemblyLastCalled = a;
						break;
					}
				}
			}

			if (type == null) throw new Exception("Invalid Class.Method: " + methodRef);

			object target = type.Assembly.CreateInstance(type.FullName); // create instance object

			object[] invokeArgs = args.ToArray();

			BindingFlags bindFlags =
				BindingFlags.InvokeMethod |
				BindingFlags.IgnoreCase |
				BindingFlags.Static |
				BindingFlags.Instance |
				BindingFlags.Public;

			object returnValue = type.InvokeMember(
				methodName, // method to call
				bindFlags, 
				null, // use default binder
				target, // object to call
				invokeArgs);

			return returnValue;
		}
	}
}
