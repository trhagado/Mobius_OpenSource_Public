using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.Serialization;
using ServiceTypes = Mobius.Services.Types;

/// <summary>
/// NativeMethodTransportObject is used to identify the classes that need to be 
/// automatically serialized as parameters and return values for use in 
/// ServiceFacade.InvokeNativeMethod calls between the Mobius client and services.
/// </summary>

namespace Mobius.Services.Native
{
	[DataContract(Namespace = "http://server/MobiusServices/Native/v1.0")]
	[KnownType(typeof(System.Boolean))]
	[KnownType(typeof(System.DateTime))]
	[KnownType(typeof(System.Double))]
	[KnownType(typeof(System.Int32))]
	[KnownType(typeof(System.Int64))]
	[KnownType(typeof(System.String))]
	[KnownType(typeof(System.Collections.Hashtable))]
	[KnownType(typeof(byte[]))]
	[KnownType(typeof(int[]))]
	[KnownType(typeof(long[]))]
	[KnownType(typeof(object[]))]
	//[KnownType(typeof(string[]))] //doesn't play nicely with List<string>
	[KnownType(typeof(Dictionary<string, Mobius.Data.MetaTreeNode>))]
	[KnownType(typeof(Dictionary<string, Mobius.Services.Types.DictionaryMx>))]
	[KnownType(typeof(Dictionary<string, string>))]
	[KnownType(typeof(Dictionary<string, List<string>>))]
	[KnownType(typeof(List<Mobius.Services.Types.AnnotationVo>))]
	[KnownType(typeof(List<Mobius.Services.Types.DataRow>))]
	[KnownType(typeof(List<Mobius.Services.Types.UserObjectNode>))]
	[KnownType(typeof(List<string>))]
	[KnownType(typeof(Mobius.Data.MetaTreeNode))]

  [KnownType(typeof(Mobius.Services.Native.ScheduledTask))]
	[KnownType(typeof(Mobius.Services.Native.ScheduledTaskStatus))]
	[KnownType(typeof(Mobius.Services.Types.AnnotationVo))]
	[KnownType(typeof(Mobius.Services.Types.Bitmap))]
	[KnownType(typeof(Mobius.Services.Types.CalcField))]
	[KnownType(typeof(Mobius.Services.Types.CalcFieldMetaTable))]
	[KnownType(typeof(Mobius.Services.Types.ChemicalStructure))]
	[KnownType(typeof(Mobius.Services.Types.CidList))]
	[KnownType(typeof(Mobius.Services.Types.CidListElement))]
	[KnownType(typeof(Mobius.Services.Types.Color))]
	[KnownType(typeof(Mobius.Services.Types.CompoundId))]
	[KnownType(typeof(Mobius.Services.Types.CondFormat))]
	[KnownType(typeof(Mobius.Services.Types.CondFormatRule))]
	[KnownType(typeof(Mobius.Services.Types.CondFormatRules))]
	[KnownType(typeof(Mobius.Services.Types.DataRow))]
	[KnownType(typeof(Mobius.Services.Types.DateTimeEx))]
	[KnownType(typeof(Mobius.Services.Types.DictionaryMx))]
	[KnownType(typeof(Mobius.Services.Types.FindQueryByNameSubstringResult))]
	[KnownType(typeof(Mobius.Services.Types.ImageEx))]
	[KnownType(typeof(Mobius.Services.Types.ListLogicType))]
	[KnownType(typeof(Mobius.Services.Types.MetaTable))]
	[KnownType(typeof(Mobius.Services.Types.MetaColumn))]
	[KnownType(typeof(Mobius.Services.Types.MetaTableDescription))]
	[KnownType(typeof(Mobius.Services.Types.Node))]
	[KnownType(typeof(Mobius.Services.Types.NumberEx))]
	[KnownType(typeof(Mobius.Services.Types.QualifiedNumber))]
	[KnownType(typeof(Mobius.Services.Types.Query))]
	[KnownType(typeof(Mobius.Services.Types.Query[]))]
	[KnownType(typeof(Mobius.Services.Types.QueryEngineState))]
	[KnownType(typeof(Mobius.Services.Types.StringEx))]
	[KnownType(typeof(Mobius.Services.Types.TargetMap))]
	[KnownType(typeof(Mobius.Services.Types.TypedBlob))]
	[KnownType(typeof(Mobius.Services.Types.TypedClob))]
	[KnownType(typeof(Mobius.Services.Types.UcdbAlias))]
	[KnownType(typeof(Mobius.Services.Types.UcdbCompound))]
	[KnownType(typeof(Mobius.Services.Types.UcdbCompound[]))]
	[KnownType(typeof(Mobius.Services.Types.UcdbDatabase))]
	[KnownType(typeof(Mobius.Services.Types.UcdbDatabase[]))]
	[KnownType(typeof(Mobius.Services.Types.UcdbModel))]
	[KnownType(typeof(Mobius.Services.Types.UcdbModel[]))]
	[KnownType(typeof(Mobius.Services.Types.UcdbTestModelServiceResult))]
	[KnownType(typeof(Mobius.Services.Types.UserInfo))]
	[KnownType(typeof(Mobius.Services.Types.UserObjectNode))]
	public class NativeMethodTransportObject
	{
		[DataMember]
		public object Value;
		 
		public NativeMethodTransportObject()
		{
		}

		public NativeMethodTransportObject(object value)
		{
			if (value is string[])
			{ // convert string arrays to string Lists
				string[] sArray = value as string[];
				List<string> sList = new List<string>(sArray);
				value = sList;
			}

			Value = value;
		}

		/// <summary>
		/// Convert Value to a string array
		/// </summary>
		/// <returns></returns>

		public string[] ToStringArray()
		{
			if (Value == null) return null;
			else if (Value is string[]) return Value as string[];
			else if (Value is List<string>)
			{
				List<string> sList = Value as List<string>;
				return sList.ToArray();
			}
			else throw new Exception("Unexpected type: " + Value.GetType());

		}

	}
}
