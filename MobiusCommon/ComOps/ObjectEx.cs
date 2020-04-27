using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Text;

namespace Mobius.ComOps
{
	public class ObjectEx
	{

/// <summary>
/// Return true if the specified object is the same or a subclass of the supplied base class
/// </summary>
/// <param name="subclassObj"></param>
/// <param name="baseClass"></param>
/// <returns></returns>

		public static bool IsSameOrSubclassOf(
			object subclassObj,
			Type baseClass)
		{
			if (subclassObj == null) return false;
			return baseClass.IsAssignableFrom(subclassObj.GetType());
		}

/// <summary>
/// Shallow copy object contents from one existing object to another of the same type
/// </summary>
/// <param name="source"></param>
/// <param name="dest"></param>

		public static void MemberwiseCopy(object source, object dest)
		{
			if (!(dest.GetType().IsAssignableFrom(source.GetType()))) throw new Exception("Dest is not subclasss of source");
			Type t  = dest.GetType();

			FieldInfo[] fia = t.GetFields();
			foreach (FieldInfo fi in fia) // do fields
			{
				if (fi.IsStatic) continue;

				object o = fi.GetValue(source);
				fi.SetValue(dest, o);
			}

			PropertyInfo[] pia = t.GetProperties();
			foreach (PropertyInfo pi in pia) // do properties
			{
				if (!pi.CanWrite || !pi.CanRead) return;

				object o = pi.GetValue(source, null);
				pi.SetValue(dest, o, null);
			}

			return;
		}

		/// <summary>
		/// Another variation on a shallow object to object copy.
		/// Copies the data of one object to another. The target object 'pulls' properties of the first. 
		/// This any matching properties are written to the target.
		/// 
		/// The object copy is a shallow copy only. Any nested types will be copied as 
		/// whole values rather than individual property assignments (ie. via assignment)
		/// </summary>
		/// <param name="source">The source object to copy from</param>
		/// <param name="target">The object to copy to</param>
		/// <param name="excludedProperties">A comma delimited list of properties that should not be copied</param>
		/// <param name="memberAccess">Reflection binding access</param>
		
		public static void CopyObjectData(object source, object target, string excludedProperties, BindingFlags memberAccess)
		{
			string[] excluded = null;
			if (!String.IsNullOrEmpty(excludedProperties))
				excluded = excludedProperties.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);

			MemberInfo[] miT = target.GetType().GetMembers(memberAccess);

			foreach (MemberInfo Field in miT)
			{
				string name = Field.Name;

				// Skip over any property exceptions
				if (!String.IsNullOrEmpty(excludedProperties) &&
						excludedProperties.Contains(name))
					continue;

				if (Field.MemberType == MemberTypes.Field)
				{
					FieldInfo SourceField = source.GetType().GetField(name);
					if (SourceField == null)
						continue;

					object SourceValue = SourceField.GetValue(source);
					((FieldInfo)Field).SetValue(target, SourceValue);
				}
				else if (Field.MemberType == MemberTypes.Property)
				{
					PropertyInfo piTarget = Field as PropertyInfo;
					PropertyInfo SourceField = source.GetType().GetProperty(name, memberAccess);
					if (SourceField == null)
						continue;

					if (piTarget.CanWrite && SourceField.CanRead)
					{
						object SourceValue = SourceField.GetValue(source, null);
						piTarget.SetValue(target, SourceValue, null);
					}
				}
			}
		}
	}
}
