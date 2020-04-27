using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Reflection;

namespace Mobius.Services.Util.TypeConversionUtil
{
	public class TypeConversionHelper
	{
		public enum ContextRetentionMode
		{
			NoRetention = 0,
			RememberAndUpdateMobiusObjects = 1,
			RememberAndUpdateServiceObjects = 2,
			RememberAllObjects = 3
		}

		public ContextRetentionMode RetainContextBetweenCalls = ContextRetentionMode.NoRetention;
		internal static List<SourceTypeTargetTypePair> knownTypeConversions =
				new List<SourceTypeTargetTypePair>();

		public TypeConversionHelper()
		{
		}

		public TypeConversionHelper(ContextRetentionMode retainContextBetweenCalls)
		{
			RetainContextBetweenCalls = retainContextBetweenCalls;
		}

		public TargetType Convert<SourceType, TargetType>(SourceType objectToConvert)
		{
			TargetType result = default(TargetType);
			if (objectToConvert != null)
			{
				//check that we know how to perform a (SourceType --> TargetType) conversion
				SourceTypeTargetTypePair sourceTypeTargetTypePair = SourceTypeTargetTypePair.GetTypePair(typeof(SourceType), typeof(TargetType));
				if (!IsKnownConversion(sourceTypeTargetTypePair))
				{
					throw new InvalidCastException(
							"No mapping exists to convert " + typeof(SourceType).FullName + " to " + typeof(TargetType).FullName + ".");
				}

				//create context -- includes a list to hold bread crumbs for the current call (to shortcut out of object conversion recursion)
				TypeConversionContext context = new TypeConversionContext();

				//delegate the conversion
				result = (TargetType)DelegateConversion(typeof(SourceType), typeof(TargetType), (object)objectToConvert, context);
			}

			ClearSharedContext(RetainContextBetweenCalls);

			return result;
		}

		public TargetType ConvertObject<TargetType>(object objectToConvert)
		{
			TargetType result = default(TargetType);
			if (objectToConvert != null)
			{
				//check that we know how to perform a (SourceType --> TargetType) conversion
				Type sourceType = objectToConvert.GetType();
				SourceTypeTargetTypePair sourceTypeTargetTypePair =
						SourceTypeTargetTypePair.GetTypePair(sourceType, typeof(TargetType));
				if (!IsKnownConversion(sourceTypeTargetTypePair))
				{
					throw new InvalidCastException(
							"No mapping exists to convert " + sourceType.FullName + " to " + typeof(TargetType).FullName + ".");
				}

				//create context -- includes a list to hold bread crumbs for the current call (to shortcut out of object conversion recursion)
				TypeConversionContext context = new TypeConversionContext();

				//delegate the conversion
				result = (TargetType)DelegateConversion(sourceType, typeof(TargetType), (object)objectToConvert, context);
			}

			ClearSharedContext(RetainContextBetweenCalls);

			return result;
		}

		public object ConvertObject(object objectToConvert)
		{
			object result = null;
			if (objectToConvert != null)
			{
				//check that there's exactly ONE applicable conversion
				Type sourceType = objectToConvert.GetType();
				Type targetType = null;
				foreach (SourceTypeTargetTypePair pair in TypeConversionHelper.knownTypeConversions)
				{
					if (pair.SourceType == sourceType)
					{
						if (targetType == null)
						{
							targetType = pair.TargetType;
						}
						else
						{
							throw new InvalidCastException(
									"More than one mapping exists for source type " + sourceType.FullName);
						}
					}
				}
				if (targetType == null)
				{
					throw new InvalidCastException(
							"No mappings exist to type " + sourceType.FullName);
				}

				//create context -- includes a list to hold bread crumbs for the current call (to shortcut out of object conversion recursion)
				TypeConversionContext context = new TypeConversionContext();

				//delegate the conversion
				result = DelegateConversion(sourceType, targetType, (object)objectToConvert, context);
			}

			if ((RetainContextBetweenCalls & ContextRetentionMode.RememberAndUpdateMobiusObjects) == 0)
			{
				ClearMobiusObjectCache();
			}
			if ((RetainContextBetweenCalls & ContextRetentionMode.RememberAndUpdateServiceObjects) == 0)
			{
				ClearServiceObjectCache();
			}

			return result;
		}

		private bool IsKnownConversion(SourceTypeTargetTypePair sourceTypeTargetTypePair)
		{
			return knownTypeConversions.Contains(sourceTypeTargetTypePair);
		}



		static TypeConversionHelper()
		{
			//this MUST match capabilities of the DelegateConversion method for this class to function properly
			//this needs to be built using reflection...  This isn't sustainable
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(Mobius.Services.Types.Node),
					typeof(Mobius.Data.MetaTreeNode)));
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(Mobius.Services.Types.MetaTable),
					typeof(Mobius.Data.MetaTable)));
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(Mobius.Data.MetaTable),
					typeof(Mobius.Services.Types.MetaTable)));
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(Mobius.Services.Types.MetaColumn),
					typeof(Mobius.Data.MetaColumn)));
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(Mobius.Data.MetaColumn),
					typeof(Mobius.Services.Types.MetaColumn)));
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(Mobius.Services.Types.MetaTableDescription),
					typeof(Mobius.Data.TableDescription)));
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(Mobius.Data.TableDescription),
					typeof(Mobius.Services.Types.MetaTableDescription)));
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(List<Mobius.Data.UserObject>),
					typeof(List<Mobius.Services.Types.UserObjectNode>)));
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(Mobius.Data.UserObject),
					typeof(Mobius.Services.Types.UserObjectNode)));
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(Mobius.Data.UserObject),
					typeof(Mobius.Services.Types.Node)));
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(List<Mobius.Services.Types.UserObjectNode>),
					typeof(List<Mobius.Data.UserObject>)));
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(Mobius.Services.Types.UserObjectNode),
					typeof(Mobius.Data.UserObject)));
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(Mobius.Data.Query),
					typeof(Mobius.Services.Types.Query)));
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(Mobius.Services.Types.Query),
					typeof(Mobius.Data.Query)));

			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(Mobius.Services.Types.QueryEngineState),
					typeof(Mobius.Data.QueryEngineState)));
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(Mobius.Data.QueryEngineState),
					typeof(Mobius.Services.Types.QueryEngineState)));

			//query engine result types
			//  System types...
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(System.Int32),
					typeof(System.Int32)));
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(System.Int64),
					typeof(System.Int64)));
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(System.Double),
					typeof(System.Double)));
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(System.String),
					typeof(System.String)));
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(System.DateTime),
					typeof(System.DateTime)));
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(System.Drawing.Bitmap),
					typeof(Mobius.Services.Types.Bitmap)));
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(Mobius.Services.Types.Bitmap),
					typeof(System.Drawing.Bitmap)));
			//  Mobius's formatted data types
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(Mobius.Data.QualifiedNumber),
					typeof(Mobius.Services.Types.QualifiedNumber)));
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(Mobius.Services.Types.QualifiedNumber),
					typeof(Mobius.Data.QualifiedNumber)));

			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(Mobius.Data.MoleculeMx),
					typeof(Mobius.Services.Types.ChemicalStructure)));
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(Mobius.Services.Types.ChemicalStructure),
					typeof(Mobius.Data.MoleculeMx)));

			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(Mobius.Data.CompoundId),
					typeof(Mobius.Services.Types.CompoundId)));
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(Mobius.Services.Types.CompoundId),
					typeof(Mobius.Data.CompoundId)));

			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(Mobius.Data.DateTimeMx),
					typeof(Mobius.Services.Types.DateTimeEx)));
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(Mobius.Services.Types.DateTimeEx),
					typeof(Mobius.Data.DateTimeMx)));

			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(Mobius.Data.StringMx),
					typeof(Mobius.Services.Types.StringEx)));
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(Mobius.Services.Types.StringEx),
					typeof(Mobius.Data.StringMx)));

			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(Mobius.Data.ImageMx),
					typeof(Mobius.Services.Types.ImageEx)));
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(Mobius.Services.Types.ImageEx),
					typeof(Mobius.Data.ImageMx)));

			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(Mobius.Data.NumberMx),
					typeof(Mobius.Services.Types.NumberEx)));
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(Mobius.Services.Types.NumberEx),
					typeof(Mobius.Data.NumberMx)));


			//build the (Mobius.Data.UserObjectType --> Mobius.Services.Types.NodeType) map for translation of "special case" types
			mobiusUserObjectTypeToServiceNodeType = new Dictionary<Mobius.Data.UserObjectType, Mobius.Services.Types.NodeType>();
			//types we expect to need often
			// names matched
			mobiusUserObjectTypeToServiceNodeType.Add(Mobius.Data.UserObjectType.CnList, Mobius.Services.Types.NodeType.CnList);
			mobiusUserObjectTypeToServiceNodeType.Add(Mobius.Data.UserObjectType.Query, Mobius.Services.Types.NodeType.Query);
			mobiusUserObjectTypeToServiceNodeType.Add(Mobius.Data.UserObjectType.CalcField, Mobius.Services.Types.NodeType.CalcField);
			mobiusUserObjectTypeToServiceNodeType.Add(Mobius.Data.UserObjectType.Annotation, Mobius.Services.Types.NodeType.Annotation);
			mobiusUserObjectTypeToServiceNodeType.Add(Mobius.Data.UserObjectType.CondFormat, Mobius.Services.Types.NodeType.CondFormat);
			// names mismatched
			mobiusUserObjectTypeToServiceNodeType.Add(Mobius.Data.UserObjectType.Folder, Mobius.Services.Types.NodeType.UserFolder);
			mobiusUserObjectTypeToServiceNodeType.Add(Mobius.Data.UserObjectType.MultiTable, Mobius.Services.Types.NodeType.MetaTable);
			mobiusUserObjectTypeToServiceNodeType.Add(Mobius.Data.UserObjectType.UserDatabase, Mobius.Services.Types.NodeType.Database);

			//security service
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(Mobius.ComOps.UserInfo),
					typeof(Mobius.Services.Types.UserInfo)));
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(Mobius.Services.Types.UserInfo),
					typeof(Mobius.ComOps.UserInfo)));

			//ucdb service
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(Mobius.Data.UcdbDatabase),
					typeof(Mobius.Services.Types.UcdbDatabase)));
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(Mobius.Services.Types.UcdbDatabase),
					typeof(Mobius.Data.UcdbDatabase)));
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(Mobius.Data.UcdbDatabase[]),
					typeof(Mobius.Services.Types.UcdbDatabase[])));
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(Mobius.Services.Types.UcdbDatabase[]),
					typeof(Mobius.Data.UcdbDatabase[])));
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(Mobius.Data.UcdbModel),
					typeof(Mobius.Services.Types.UcdbModel)));
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(Mobius.Services.Types.UcdbModel),
					typeof(Mobius.Data.UcdbModel)));
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(Mobius.Data.UcdbModel[]),
					typeof(Mobius.Services.Types.UcdbModel[])));
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(Mobius.Services.Types.UcdbModel[]),
					typeof(Mobius.Data.UcdbModel[])));
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(Mobius.Data.UcdbCompound),
					typeof(Mobius.Services.Types.UcdbCompound)));
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(Mobius.Services.Types.UcdbCompound),
					typeof(Mobius.Data.UcdbCompound)));
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(Mobius.Data.UcdbCompound[]),
					typeof(Mobius.Services.Types.UcdbCompound[])));
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(Mobius.Services.Types.UcdbCompound[]),
					typeof(Mobius.Data.UcdbCompound[])));
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(Mobius.Data.UcdbAlias),
					typeof(Mobius.Services.Types.UcdbAlias)));
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(Mobius.Services.Types.UcdbAlias),
					typeof(Mobius.Data.UcdbAlias)));

			//annotation  service
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(Mobius.Data.AnnotationVo),
					typeof(Mobius.Services.Types.AnnotationVo)));
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(Mobius.Services.Types.AnnotationVo),
					typeof(Mobius.Data.AnnotationVo)));
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(List<Mobius.Data.AnnotationVo>),
					typeof(List<Mobius.Services.Types.AnnotationVo>)));
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(List<Mobius.Services.Types.AnnotationVo>),
					typeof(List<Mobius.Data.AnnotationVo>)));

			//CidList service
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(Mobius.Data.CidList),
					typeof(Mobius.Services.Types.CidList)));
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(Mobius.Services.Types.CidList),
					typeof(Mobius.Data.CidList)));
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(Mobius.Data.CidListElement),
					typeof(Mobius.Services.Types.CidListElement)));
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(Mobius.Services.Types.CidListElement),
					typeof(Mobius.Data.CidListElement)));
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(Mobius.Data.ListLogicType),
					typeof(Mobius.Services.Types.ListLogicType)));
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(Mobius.Services.Types.ListLogicType),
					typeof(Mobius.Data.ListLogicType)));

			//TargetMap
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(Mobius.Data.TargetMap),
					typeof(Mobius.Services.Types.TargetMap)));
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(Mobius.Services.Types.TargetMap),
					typeof(Mobius.Data.TargetMap)));

			//DictionaryFactory
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(Mobius.Data.DictionaryMx),
					typeof(Mobius.Services.Types.DictionaryMx)));
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(Mobius.Services.Types.DictionaryMx),
					typeof(Mobius.Data.DictionaryMx)));
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(Dictionary<string, Mobius.Data.DictionaryMx>),
					typeof(Dictionary<string, Mobius.Services.Types.DictionaryMx>)));
			knownTypeConversions.Add(SourceTypeTargetTypePair.GetTypePair(
					typeof(Dictionary<string, Mobius.Services.Types.DictionaryMx>),
					typeof(Dictionary<string, Mobius.Data.DictionaryMx>)));

			/////////////////////////////////////////////////////////////////
		}

		public void AddKnownObject<ObjectType>(ObjectType knownObject)
		{
			//Do we really want to set this for ALL users?  Is there any harm in doing so?  (Doesn't seem to be...)
			if (knownObject != null)
			{
				Type objectType = typeof(ObjectType);
				if (objectType == typeof(Data.MetaTable))
				{
					Data.MetaTable mobiusMetaTable = knownObject as Data.MetaTable;
					lock (knownMobiusMetaTables)
					{
						if (knownMobiusMetaTables.ContainsKey(mobiusMetaTable.Name))
						{
							knownMobiusMetaTables[mobiusMetaTable.Name] = mobiusMetaTable;
						}
						else
						{
							knownMobiusMetaTables.Add(mobiusMetaTable.Name, mobiusMetaTable);
						}
					}
				}
				//TODO:  Add any/all other types that we need to be able to manually add...
				else
				{
					throw new InvalidCastException("Could not set the provided object as a known object.");
				}
			}
		}

		public void ClearSharedContext()
		{
			//clear all cross-thread context information -- this needs to be delegated...
			ClearMobiusObjectCache();
			ClearServiceObjectCache();
		}

		public void ClearSharedContext(ContextRetentionMode retentionMode)
		{
			if ((RetainContextBetweenCalls & ContextRetentionMode.RememberAndUpdateMobiusObjects) == 0)
			{
				ClearMobiusObjectCache();
			}
			if ((RetainContextBetweenCalls & ContextRetentionMode.RememberAndUpdateServiceObjects) == 0)
			{
				ClearServiceObjectCache();
			}
		}

		public void ClearMobiusObjectCache()
		{
			lock (knownMobiusMetaTables) { knownMobiusMetaTables.Clear(); }
			lock (knownMobiusMetaColumns) { knownMobiusMetaColumns.Clear(); }
			lock (knownMobiusCondFormats) { knownMobiusCondFormats.Clear(); }

			lock (knownMobiusQueries) { knownMobiusQueries.Clear(); }
			lock (knownMobiusQueryTables) { knownMobiusQueryTables.Clear(); }
			lock (knownMobiusQueryColumns) { knownMobiusQueryColumns.Clear(); }
			lock (knownMobiusCalcFields) { knownMobiusCalcFields.Clear(); }

			lock (knownMobiusDictionaries) { knownMobiusDictionaries.Clear(); }
		}

		public void ClearServiceObjectCache()
		{
			lock (knownServiceMetaTables) { knownServiceMetaTables.Clear(); }
			lock (knownServiceMetaColumns) { knownServiceMetaColumns.Clear(); }
			lock (knownServiceCondFormats) { knownServiceCondFormats.Clear(); }

			lock (knownServiceQueries) { knownServiceQueries.Clear(); }
			lock (knownServiceQueryTables) { knownServiceQueryTables.Clear(); }
			lock (knownServiceQueryColumns) { knownServiceQueryColumns.Clear(); }
			lock (knownServiceCalcFields) { knownServiceCalcFields.Clear(); }

			lock (knownServiceDictionaries) { knownServiceDictionaries.Clear(); }
		}

		private object DelegateConversion(Type sourceType, Type targetType, object objectToConvert, TypeConversionContext context)
		{
			//should REALLY split this up into multiple, but this is FAR simpler for now
			//make it work -- pretty can come later
			object result = null;

			//if the input oject is null, then there's no work to do -- the output object should be too
			if (objectToConvert != null)
			{
				//want to use a dictionary to delegates/conversion classes here in the future
				// -- the check for an existing/equivalent object,
				//    the conversion logic, and the context that must be maintained for that logic
				//    are all type-specific
				// Probably will want a bag of type-specific context objects (eg MetaColumn needs parent MetaTable)

				SourceTypeTargetTypePair sourceTypeTargetTypePair = SourceTypeTargetTypePair.GetTypePair(sourceType, targetType);

				//check for conversions from service types to Mobius types first
				if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(Mobius.Services.Types.Node),
						typeof(Mobius.Data.MetaTreeNode)))
				{
					Mobius.Services.Types.Node node = objectToConvert as Mobius.Services.Types.Node;
					Mobius.Data.MetaTreeNode mtn = ConvertServiceNodeToMobiusMetaTreeNode(node, context);
					result = mtn;
				}
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(Mobius.Services.Types.MetaTable),
						typeof(Mobius.Data.MetaTable)))
				{
					Mobius.Services.Types.MetaTable serviceMetaTable = objectToConvert as Mobius.Services.Types.MetaTable;
					Mobius.Data.MetaTable mobiusMetaTable = ConvertServiceMetaTableToMobiusMetaTable(serviceMetaTable, context);
					result = mobiusMetaTable;
				}
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(Mobius.Data.MetaTable),
						typeof(Mobius.Services.Types.MetaTable)))
				{
					Mobius.Data.MetaTable mobiusMetaTable = objectToConvert as Mobius.Data.MetaTable;
					Mobius.Services.Types.MetaTable serviceMetaTable = ConvertMobiusMetaTableToServiceMetaTable(mobiusMetaTable, context);
					result = serviceMetaTable;
				}
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(Mobius.Services.Types.MetaColumn),
						typeof(Mobius.Data.MetaColumn)))
				{
					Mobius.Services.Types.MetaColumn serviceMetaColumn = objectToConvert as Mobius.Services.Types.MetaColumn;
					Mobius.Data.MetaColumn mobiusMetaColumn = ConvertServiceMetaColumnToMobiusMetaColumn(serviceMetaColumn, context);
					result = mobiusMetaColumn;
				}
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(Mobius.Data.MetaColumn),
						typeof(Mobius.Services.Types.MetaColumn)))
				{
					Mobius.Data.MetaColumn mobiusMetaColumn = objectToConvert as Mobius.Data.MetaColumn;
					Mobius.Services.Types.MetaColumn serviceMetaColumn = ConvertMobiusMetaColumnToServiceMetaColumn(mobiusMetaColumn, context);
					result = serviceMetaColumn;
				}
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(List<Mobius.Data.UserObject>),
						typeof(List<Mobius.Services.Types.UserObjectNode>)))
				{
					List<Mobius.Data.UserObject> mobiusUserObjects = objectToConvert as List<Mobius.Data.UserObject>;
					List<Mobius.Services.Types.UserObjectNode> serviceUserObjects =
							ConvertMobiusUserObjectsToServiceUserObjectNodes(mobiusUserObjects, context);
					result = serviceUserObjects;
				}
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(Mobius.Data.UserObject),
						typeof(Mobius.Services.Types.UserObjectNode)))
				{
					Mobius.Data.UserObject mobiusUserObject = objectToConvert as Mobius.Data.UserObject;
					Mobius.Services.Types.UserObjectNode serviceUserObject =
							ConvertMobiusUserObjectToServiceUserObjectNode(mobiusUserObject, context);
					result = serviceUserObject;
				}
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(Mobius.Data.UserObject),
						typeof(Mobius.Services.Types.Node)))
				{
					Mobius.Data.UserObject mobiusUserObject = objectToConvert as Mobius.Data.UserObject;
					Mobius.Services.Types.Node serviceNode = ConvertMobiusUserObjectToServiceNode(mobiusUserObject, context);
					result = serviceNode;
				}
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(List<Mobius.Services.Types.UserObjectNode>),
						typeof(List<Mobius.Data.UserObject>)))
				{
					List<Mobius.Services.Types.UserObjectNode> serviceUserObjectNodes = objectToConvert as List<Mobius.Services.Types.UserObjectNode>;
					List<Mobius.Data.UserObject> mobiusUserObjects = ConvertServiceUserObjectsToMobiusUserObjects(serviceUserObjectNodes, context);
					result = mobiusUserObjects;
				}
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(Mobius.Services.Types.UserObjectNode),
						typeof(Mobius.Data.UserObject)))
				{
					Mobius.Services.Types.UserObjectNode serviceUserObject = objectToConvert as Mobius.Services.Types.UserObjectNode;
					Mobius.Data.UserObject mobiusUserObject = ConvertServiceUserObjectNodeToMobiusUserObject(serviceUserObject, context);
					result = mobiusUserObject;
				}
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(Mobius.Data.Query),
						typeof(Mobius.Services.Types.Query)))
				{
					Mobius.Data.Query mobiusQuery = objectToConvert as Mobius.Data.Query;
					Mobius.Services.Types.Query serviceQuery = ConvertMobiusQueryToServiceQuery(mobiusQuery, context);
					result = serviceQuery;
				}
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(Mobius.Services.Types.Query),
						typeof(Mobius.Data.Query)))
				{
					Mobius.Services.Types.Query serviceQuery = objectToConvert as Mobius.Services.Types.Query;
					Mobius.Data.Query mobiusQuery = ConvertServiceQueryToMobiusQuery(serviceQuery, context);
					result = mobiusQuery;
				}
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(Mobius.Services.Types.QueryEngineState),
						typeof(Mobius.Data.QueryEngineState)))
				{
					Mobius.Services.Types.QueryEngineState serviceQueryEngineState = (Mobius.Services.Types.QueryEngineState)objectToConvert;
					Mobius.Data.QueryEngineState mobiusQueryEngineState = (Mobius.Data.QueryEngineState)((int)serviceQueryEngineState);
					result = mobiusQueryEngineState;
				}
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(Mobius.Data.QueryEngineState),
						typeof(Mobius.Services.Types.QueryEngineState)))
				{
					Mobius.Data.QueryEngineState mobiusQueryEngineState = (Mobius.Data.QueryEngineState)objectToConvert;
					Mobius.Services.Types.QueryEngineState serviceQueryEngineState = (Mobius.Services.Types.QueryEngineState)((int)objectToConvert);
					result = serviceQueryEngineState;
				}
				//query engine result types
				//  System types...
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(System.Int32),
						typeof(System.Int32)))
				{
					result = objectToConvert;
				}
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(System.Double),
						typeof(System.Double)))
				{
					result = objectToConvert;
				}
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(System.String),
						typeof(System.String)))
				{
					result = objectToConvert;
				}
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(System.DateTime),
						typeof(System.DateTime)))
				{
					result = objectToConvert;
				}
				//the following bitmap inter-conversions are silly, but...  Well, nice to have the converter be more complete, I guess.
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(System.Drawing.Bitmap),
						typeof(Mobius.Services.Types.Bitmap)))
				{
					result = new Mobius.Services.Types.Bitmap((System.Drawing.Bitmap)objectToConvert);
				}
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(Mobius.Services.Types.Bitmap),
						typeof(System.Drawing.Bitmap)))
				{
					result = (System.Drawing.Bitmap)(objectToConvert as Mobius.Services.Types.Bitmap);
				}
				//  Mobius data types
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(Mobius.Data.MoleculeMx),
						typeof(Mobius.Services.Types.ChemicalStructure)))
				{
					Mobius.Data.MoleculeMx mobiusChemicalStructure = objectToConvert as Mobius.Data.MoleculeMx;
					Mobius.Services.Types.ChemicalStructure serviceChemicalStructure =
							ConvertMobiusChemicalStructureToServiceChemicalStructure(mobiusChemicalStructure, context);
					result = serviceChemicalStructure;
				}

				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(Mobius.Services.Types.ChemicalStructure),
						typeof(Mobius.Data.MoleculeMx)))
				{
					Mobius.Services.Types.ChemicalStructure serviceChemicalStructure = objectToConvert as Mobius.Services.Types.ChemicalStructure;
					Mobius.Data.MoleculeMx mobiusChemicalStructure =
							ConvertServiceChemicalStructureToMobiusChemicalStructure(serviceChemicalStructure, context);
					result = mobiusChemicalStructure;
				}

				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(Mobius.Data.QualifiedNumber),
						typeof(Mobius.Services.Types.QualifiedNumber)))
				{
					Mobius.Data.QualifiedNumber mobiusQualifiedNumber = objectToConvert as Mobius.Data.QualifiedNumber;
					Mobius.Services.Types.QualifiedNumber serviceQualifiedNumber =
							ConvertMobiusQualifiedNumberToServiceQualifiedNumber(mobiusQualifiedNumber, context);
					result = serviceQualifiedNumber;
				}
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(Mobius.Services.Types.QualifiedNumber),
						typeof(Mobius.Data.QualifiedNumber)))
				{
					Mobius.Services.Types.QualifiedNumber serviceQualifiedNumber = objectToConvert as Mobius.Services.Types.QualifiedNumber;
					Mobius.Data.QualifiedNumber mobiusQualifiedNumber =
							ConvertServiceQualifiedNumberToMobiusQualifiedNumber(serviceQualifiedNumber, context);
					result = mobiusQualifiedNumber;
				}
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(Mobius.Data.NumberMx),
						typeof(Mobius.Services.Types.NumberEx)))
				{
					Mobius.Data.NumberMx mobiusNumberEx = objectToConvert as Mobius.Data.NumberMx;
					Mobius.Services.Types.NumberEx serviceNumberEx =
							ConvertMobiusNumberExToServiceNumberEx(mobiusNumberEx, context);
					result = serviceNumberEx;
				}
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(Mobius.Services.Types.NumberEx),
						typeof(Mobius.Data.NumberMx)))
				{
					Mobius.Services.Types.NumberEx serviceNumberEx = objectToConvert as Mobius.Services.Types.NumberEx;
					Mobius.Data.NumberMx mobiusNumberEx =
							ConvertServiceNumberExToMobiusNumberEx(serviceNumberEx, context);
					result = mobiusNumberEx;
				}
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(Mobius.Data.StringMx),
						typeof(Mobius.Services.Types.StringEx)))
				{
					Mobius.Data.StringMx mobiusStringEx = objectToConvert as Mobius.Data.StringMx;
					Mobius.Services.Types.StringEx serviceStringEx =
							ConvertMobiusStringExToServiceStringEx(mobiusStringEx, context);
					result = serviceStringEx;
				}
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(Mobius.Services.Types.StringEx),
						typeof(Mobius.Data.StringMx)))
				{
					Mobius.Services.Types.StringEx serviceStringEx = objectToConvert as Mobius.Services.Types.StringEx;
					Mobius.Data.StringMx mobiusStringEx =
							ConvertServiceStringExToMobiusStringEx(serviceStringEx, context);
					result = mobiusStringEx;
				}
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(Mobius.Data.DateTimeMx),
						typeof(Mobius.Services.Types.DateTimeEx)))
				{
					Mobius.Data.DateTimeMx mobiusDateTimeEx = objectToConvert as Mobius.Data.DateTimeMx;
					Mobius.Services.Types.DateTimeEx serviceDateTimeEx =
							ConvertMobiusDateTimeExToServiceDateTimeEx(mobiusDateTimeEx, context);
					result = serviceDateTimeEx;
				}
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(Mobius.Services.Types.DateTimeEx),
						typeof(Mobius.Data.DateTimeMx)))
				{
					Mobius.Services.Types.DateTimeEx serviceDateTimeEx = objectToConvert as Mobius.Services.Types.DateTimeEx;
					Mobius.Data.DateTimeMx mobiusDateTimeEx =
							ConvertServiceDateTimeExToMobiusDateTimeEx(serviceDateTimeEx, context);
					result = mobiusDateTimeEx;
				}
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(Mobius.Data.ImageMx),
						typeof(Mobius.Services.Types.ImageEx)))
				{
					Mobius.Data.ImageMx mobiusImageEx = objectToConvert as Mobius.Data.ImageMx;
					Mobius.Services.Types.ImageEx serviceImageEx =
							ConvertMobiusImageExToServiceImageEx(mobiusImageEx, context);
					result = serviceImageEx;
				}
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(Mobius.Services.Types.ImageEx),
						typeof(Mobius.Data.ImageMx)))
				{
					Mobius.Services.Types.ImageEx serviceImageEx = objectToConvert as Mobius.Services.Types.ImageEx;
					Mobius.Data.ImageMx mobiusImageEx =
							ConvertServiceImageExToMobiusImageEx(serviceImageEx, context);
					result = mobiusImageEx;
				}
				//security service
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(Mobius.ComOps.UserInfo),
						typeof(Mobius.Services.Types.UserInfo)))
				{
					Mobius.ComOps.UserInfo mobiusUserInfo = objectToConvert as Mobius.ComOps.UserInfo;
					Mobius.Services.Types.UserInfo serviceUserInfo =
							ConvertMobiusUserInfoToServiceUserInfo(mobiusUserInfo, context);
					result = serviceUserInfo;
				}
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(Mobius.Services.Types.UserInfo),
						typeof(Mobius.ComOps.UserInfo)))
				{
					Mobius.Services.Types.UserInfo serviceUserInfo = objectToConvert as Mobius.Services.Types.UserInfo;
					Mobius.ComOps.UserInfo mobiusUserInfo =
							ConvertServiceUserInfoToMobiusUserInfo(serviceUserInfo, context);
					result = mobiusUserInfo;
				}
				//ucdb service
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(Mobius.Data.UcdbDatabase),
						typeof(Mobius.Services.Types.UcdbDatabase)))
				{
					Mobius.Data.UcdbDatabase mobiusUcdbDatabase = objectToConvert as Mobius.Data.UcdbDatabase;
					Mobius.Services.Types.UcdbDatabase serviceUcdbDatabase =
							ConvertMobiusUcdbDatabaseToServiceUcdbDatabase(mobiusUcdbDatabase, context);
					result = serviceUcdbDatabase;
				}
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(Mobius.Services.Types.UcdbDatabase),
						typeof(Mobius.Data.UcdbDatabase)))
				{
					Mobius.Services.Types.UcdbDatabase serviceUcdbDatabase = objectToConvert as Mobius.Services.Types.UcdbDatabase;
					Mobius.Data.UcdbDatabase mobiusUcdbDatabase =
							ConvertServiceUcdbDatabaseToMobiusUcdbDatabase(serviceUcdbDatabase, context);
					result = mobiusUcdbDatabase;
				}
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(Mobius.Data.UcdbDatabase[]),
						typeof(Mobius.Services.Types.UcdbDatabase[])))
				{
					Mobius.Data.UcdbDatabase[] mobiusUcdbDatabases = objectToConvert as Mobius.Data.UcdbDatabase[];
					Mobius.Services.Types.UcdbDatabase[] serviceUcdbDatabases =
							ConvertMobiusUcdbDatabasesToServiceUcdbDatabases(mobiusUcdbDatabases, context);
					result = serviceUcdbDatabases;
				}
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(Mobius.Services.Types.UcdbDatabase[]),
						typeof(Mobius.Data.UcdbDatabase[])))
				{
					Mobius.Services.Types.UcdbDatabase[] serviceUcdbDatabases = objectToConvert as Mobius.Services.Types.UcdbDatabase[];
					Mobius.Data.UcdbDatabase[] mobiusUcdbDatabases =
							ConvertServiceUcdbDatabasesToMobiusUcdbDatabases(serviceUcdbDatabases, context);
					result = mobiusUcdbDatabases;
				}
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(Mobius.Data.UcdbModel),
						typeof(Mobius.Services.Types.UcdbModel)))
				{
					Mobius.Data.UcdbModel mobiusUcdbModel = objectToConvert as Mobius.Data.UcdbModel;
					Mobius.Services.Types.UcdbModel serviceUcdbModel =
							ConvertMobiusUcdbModelToServiceUcdbModel(mobiusUcdbModel, context);
					result = serviceUcdbModel;
				}
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(Mobius.Services.Types.UcdbModel),
						typeof(Mobius.Data.UcdbModel)))
				{
					Mobius.Services.Types.UcdbModel serviceUcdbModel = objectToConvert as Mobius.Services.Types.UcdbModel;
					Mobius.Data.UcdbModel mobiusUcdbModel =
							ConvertServiceUcdbModelToMobiusUcdbModel(serviceUcdbModel, context);
					result = mobiusUcdbModel;
				}
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(Mobius.Data.UcdbModel[]),
						typeof(Mobius.Services.Types.UcdbModel[])))
				{
					Mobius.Data.UcdbModel[] mobiusUcdbModels = objectToConvert as Mobius.Data.UcdbModel[];
					Mobius.Services.Types.UcdbModel[] serviceUcdbModels =
							ConvertMobiusUcdbModelsToServiceUcdbModels(mobiusUcdbModels, context);
					result = serviceUcdbModels;
				}
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(Mobius.Services.Types.UcdbModel[]),
						typeof(Mobius.Data.UcdbModel[])))
				{
					Mobius.Services.Types.UcdbModel[] serviceUcdbModels = objectToConvert as Mobius.Services.Types.UcdbModel[];
					Mobius.Data.UcdbModel[] mobiusUcdbModels =
							ConvertServiceUcdbModelsToMobiusUcdbModels(serviceUcdbModels, context);
					result = mobiusUcdbModels;
				}
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(Mobius.Data.UcdbCompound),
						typeof(Mobius.Services.Types.UcdbCompound)))
				{
					Mobius.Data.UcdbCompound mobiusUcdbCompound = objectToConvert as Mobius.Data.UcdbCompound;
					Mobius.Services.Types.UcdbCompound serviceUcdbCompound =
							ConvertMobiusUcdbCompoundToServiceUcdbCompound(mobiusUcdbCompound, context);
					result = serviceUcdbCompound;
				}
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(Mobius.Services.Types.UcdbCompound),
						typeof(Mobius.Data.UcdbCompound)))
				{
					Mobius.Services.Types.UcdbCompound serviceUcdbCompound = objectToConvert as Mobius.Services.Types.UcdbCompound;
					Mobius.Data.UcdbCompound mobiusUcdbCompound =
							ConvertServiceUcdbCompoundToMobiusUcdbCompound(serviceUcdbCompound, context);
					result = mobiusUcdbCompound;
				}
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(Mobius.Data.UcdbCompound[]),
						typeof(Mobius.Services.Types.UcdbCompound[])))
				{
					Mobius.Data.UcdbCompound[] mobiusUcdbCompounds = objectToConvert as Mobius.Data.UcdbCompound[];
					Mobius.Services.Types.UcdbCompound[] serviceUcdbCompounds =
							ConvertMobiusUcdbCompoundsToServiceUcdbCompounds(mobiusUcdbCompounds, context);
					result = serviceUcdbCompounds;
				}
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(Mobius.Services.Types.UcdbCompound[]),
						typeof(Mobius.Data.UcdbCompound[])))
				{
					Mobius.Services.Types.UcdbCompound[] serviceUcdbCompounds = objectToConvert as Mobius.Services.Types.UcdbCompound[];
					Mobius.Data.UcdbCompound[] mobiusUcdbCompounds =
							ConvertServiceUcdbCompoundsToMobiusUcdbCompounds(serviceUcdbCompounds, context);
					result = mobiusUcdbCompounds;
				}
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(Mobius.Data.UcdbAlias),
						typeof(Mobius.Services.Types.UcdbAlias)))
				{
					Mobius.Data.UcdbAlias mobiusUcdbAlias = objectToConvert as Mobius.Data.UcdbAlias;
					Mobius.Services.Types.UcdbAlias serviceUcdbAlias =
							ConvertMobiusUcdbAliasToServiceUcdbAlias(mobiusUcdbAlias, context);
					result = serviceUcdbAlias;
				}
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(Mobius.Services.Types.UcdbAlias),
						typeof(Mobius.Data.UcdbAlias)))
				{
					Mobius.Services.Types.UcdbAlias serviceUcdbAlias = objectToConvert as Mobius.Services.Types.UcdbAlias;
					Mobius.Data.UcdbAlias mobiusUcdbAlias =
							ConvertServiceUcdbAliasToMobiusUcdbAlias(serviceUcdbAlias, context);
					result = mobiusUcdbAlias;
				}
				//annotation service
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(Mobius.Data.AnnotationVo),
						typeof(Mobius.Services.Types.AnnotationVo)))
				{
					Mobius.Data.AnnotationVo mobiusAnnotationVo = objectToConvert as Mobius.Data.AnnotationVo;
					Mobius.Services.Types.AnnotationVo serviceAnnotationVo =
							ConvertMobiusAnnotationVoToServiceAnnotationVo(mobiusAnnotationVo, context);
					result = serviceAnnotationVo;
				}
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(Mobius.Services.Types.AnnotationVo),
						typeof(Mobius.Data.AnnotationVo)))
				{
					Mobius.Services.Types.AnnotationVo serviceAnnotationVo = objectToConvert as Mobius.Services.Types.AnnotationVo;
					Mobius.Data.AnnotationVo mobiusAnnotationVo =
							ConvertServiceAnnotationVoToMobiusAnnotationVo(serviceAnnotationVo, context);
					result = mobiusAnnotationVo;
				}
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(List<Mobius.Data.AnnotationVo>),
						typeof(List<Mobius.Services.Types.AnnotationVo>)))
				{
					List<Mobius.Data.AnnotationVo> mobiusAnnotationVos = objectToConvert as List<Mobius.Data.AnnotationVo>;
					List<Mobius.Services.Types.AnnotationVo> serviceAnnotationVos =
							ConvertMobiusAnnotationVosToServiceAnnotationVos(mobiusAnnotationVos, context);
					result = serviceAnnotationVos;
				}
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(List<Mobius.Services.Types.AnnotationVo>),
						typeof(List<Mobius.Data.AnnotationVo>)))
				{
					List<Mobius.Services.Types.AnnotationVo> serviceAnnotationVos = objectToConvert as List<Mobius.Services.Types.AnnotationVo>;
					List<Mobius.Data.AnnotationVo> mobiusAnnotationVos =
							ConvertServiceAnnotationVosToMobiusAnnotationVos(serviceAnnotationVos, context);
					result = mobiusAnnotationVos;
				}
				//CidList
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(Mobius.Data.CidList),
						typeof(Mobius.Services.Types.CidList)))
				{
					Mobius.Data.CidList mobiusCidList = objectToConvert as Mobius.Data.CidList;
					Mobius.Services.Types.CidList serviceCidList =
							ConvertMobiusCidListToServiceCidList(mobiusCidList, context);
					result = serviceCidList;
				}
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(Mobius.Services.Types.CidList),
						typeof(Mobius.Data.CidList)))
				{
					Mobius.Services.Types.CidList serviceCidList = objectToConvert as Mobius.Services.Types.CidList;
					Mobius.Data.CidList mobiusCidList =
							ConvertServiceCidListToMobiusCidList(serviceCidList, context);
					result = mobiusCidList;
				}
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(Mobius.Data.CidListElement),
						typeof(Mobius.Services.Types.CidListElement)))
				{
					Mobius.Data.CidListElement mobiusCidListElement = objectToConvert as Mobius.Data.CidListElement;
					Mobius.Services.Types.CidListElement serviceCidListElement =
							ConvertMobiusCidListElementToServiceCidListElement(mobiusCidListElement, context);
					result = serviceCidListElement;
				}
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(Mobius.Services.Types.CidListElement),
						typeof(Mobius.Data.CidListElement)))
				{
					Mobius.Services.Types.CidListElement serviceCidListElement = objectToConvert as Mobius.Services.Types.CidListElement;
					Mobius.Data.CidListElement mobiusCidListElement =
							ConvertServiceCidListElementToMobiusCidListElement(serviceCidListElement, context);
					result = mobiusCidListElement;
				}
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(Mobius.Data.ListLogicType),
						typeof(Mobius.Services.Types.ListLogicType)))
				{
					Mobius.Services.Types.ListLogicType serviceListLogicType = (Mobius.Services.Types.ListLogicType)((int)objectToConvert);
					result = serviceListLogicType;
				}
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(Mobius.Services.Types.ListLogicType),
						typeof(Mobius.Data.ListLogicType)))
				{
					Mobius.Data.ListLogicType mobiusListLogicType = (Mobius.Data.ListLogicType)((int)objectToConvert);
					result = mobiusListLogicType;
				}
				//TargetMap
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(Mobius.Data.TargetMap),
						typeof(Mobius.Services.Types.TargetMap)))
				{
					Mobius.Data.TargetMap mobiusTargetMap = objectToConvert as Mobius.Data.TargetMap;
					Mobius.Services.Types.TargetMap serviceTargetMap =
							ConvertMobiusTargetMapToServiceTargetMap(mobiusTargetMap, context);
					result = serviceTargetMap;
				}
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(Mobius.Services.Types.TargetMap),
						typeof(Mobius.Data.TargetMap)))
				{
					Mobius.Services.Types.TargetMap serviceTargetMap = objectToConvert as Mobius.Services.Types.TargetMap;
					Mobius.Data.TargetMap mobiusTargetMap =
							ConvertServiceTargetMapToMobiusTargetMap(serviceTargetMap, context);
					result = mobiusTargetMap;
				}
				//Dictionary
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(Mobius.Data.DictionaryMx),
						typeof(Mobius.Services.Types.DictionaryMx)))
				{
					Mobius.Data.DictionaryMx mobiusDictionaryMx = objectToConvert as Mobius.Data.DictionaryMx;
					Mobius.Services.Types.DictionaryMx serviceDictionaryMx =
							ConvertMobiusDictionaryMxToServiceDictionaryMx(mobiusDictionaryMx, context);
					result = serviceDictionaryMx;
				}
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(Mobius.Services.Types.DictionaryMx),
						typeof(Mobius.Data.DictionaryMx)))
				{
					Mobius.Services.Types.DictionaryMx serviceDictionaryMx = objectToConvert as Mobius.Services.Types.DictionaryMx;
					Mobius.Data.DictionaryMx mobiusDictionaryMx =
							ConvertServiceDictionaryMxToMobiusDictionaryMx(serviceDictionaryMx, context);
					result = mobiusDictionaryMx;
				}
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(Dictionary<string, Mobius.Data.DictionaryMx>),
						typeof(Dictionary<string, Mobius.Services.Types.DictionaryMx>)))
				{
					Dictionary<string, Mobius.Data.DictionaryMx> mobiusDictionaryMxDict = objectToConvert as Dictionary<string, Mobius.Data.DictionaryMx>;
					Dictionary<string, Mobius.Services.Types.DictionaryMx> serviceDictionaryMxDict =
							ConvertMobiusDictionaryMxDictToServiceDictionaryMxDict(mobiusDictionaryMxDict, context);
					result = serviceDictionaryMxDict;
				}
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(Dictionary<string, Mobius.Services.Types.DictionaryMx>),
						typeof(Dictionary<string, Mobius.Data.DictionaryMx>)))
				{
					Dictionary<string, Mobius.Services.Types.DictionaryMx> serviceDictionaryMxDict = objectToConvert as Dictionary<string, Mobius.Services.Types.DictionaryMx>;
					Dictionary<string, Mobius.Data.DictionaryMx> mobiusDictionaryMxDict =
							ConvertServiceDictionaryMxDictToMobiusDictionaryMxDict(serviceDictionaryMxDict, context);
					result = mobiusDictionaryMxDict;
				}

				//MetaTable description -- late addition to MetaTable service

				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(Mobius.Services.Types.MetaTableDescription),
						typeof(Mobius.Data.TableDescription)))
				{
					Mobius.Services.Types.MetaTableDescription serviceMetaTableDescription = objectToConvert as Mobius.Services.Types.MetaTableDescription;
					Mobius.Data.TableDescription mobiusMetaTableDescription = ConvertServiceMetaTableToMobiusMetaTable(serviceMetaTableDescription, context);
					result = mobiusMetaTableDescription;
				}
				else if (sourceTypeTargetTypePair == SourceTypeTargetTypePair.GetTypePair(
						typeof(Mobius.Data.TableDescription),
						typeof(Mobius.Services.Types.MetaTableDescription)))
				{
					Mobius.Data.TableDescription mobiusMetaTableDescription = objectToConvert as Mobius.Data.TableDescription;
					Mobius.Services.Types.MetaTableDescription serviceMetaTableDescription = ConvertMobiusMetaTableToServiceMetaTable(mobiusMetaTableDescription, context);
					result = serviceMetaTableDescription;
				}

				//No cast exists?
				else
				{
					throw new InvalidCastException(
							"No mapping exists to convert " + sourceType.FullName + " to " + targetType.FullName + ".");
				}
			}
			return result;
		}



		private Mobius.Data.MetaTreeNode ConvertServiceNodeToMobiusMetaTreeNode(Mobius.Services.Types.Node node, TypeConversionContext context)
		{
			Mobius.Data.MetaTreeNode mtn = null;
			if (node != null)
			{
				//no need to check for an equivalent object for this type since it references no other objects,
				// so ignore the bread crumbs

				//create the target object
				mtn = new Mobius.Data.MetaTreeNode();

				//translate the members
				mtn.Label = node.Label;
				mtn.Name = node.Name;
				mtn.Owner = node.Owner;
				mtn.Shared = node.Shared;
				mtn.Size = node.Size;
				mtn.Target = node.Target;
				mtn.UpdateDateTime = node.LastUpdateTimestamp;

				//converting type in THIS direction is safe...
				// converting in the OTHER direction would be problematic for UserObjectType.Folder
				// (UserObjectType.Folder --> NodeType.UserFolder and
				//  NodeType.UserFolder -X-> UserObjectType.Folder)
				//These two enums are NOT integer-wise equivalent!
				mtn.Type = (Mobius.Data.MetaTreeNodeType)Enum.Parse(typeof(Mobius.Data.MetaTreeNodeType), node.Type.ToString());
			}
			//return the result
			return mtn;
		}



		private Dictionary<string, Mobius.Data.MetaTable> knownMobiusMetaTables = new Dictionary<string, Mobius.Data.MetaTable>();
		private Mobius.Data.MetaTable ConvertServiceMetaTableToMobiusMetaTable(
				Mobius.Services.Types.MetaTable serviceMetaTable, TypeConversionContext context)
		{
			Mobius.Data.MetaTable mobiusMetaTable = null;
			if (serviceMetaTable != null)
			{
				//Is this object conversion already "in the stack"?
				List<BreadCrumb> breadCrumbs = context.BreadCrumbs;
				//Add it if not
				BreadCrumb breadCrumb = new BreadCrumb(MethodBase.GetCurrentMethod().Name, serviceMetaTable);
				bool alreadyInProgress = true;
				if (!breadCrumbs.ContainsEquivalentBreadCrumb(breadCrumb))
				{
					alreadyInProgress = false;
					breadCrumbs.Add(breadCrumb);
				}

				if (alreadyInProgress)
				{
					//should already be a known object (in the current conversion context)
					mobiusMetaTable = (Mobius.Data.MetaTable)context.KnownObjects[serviceMetaTable];
				}
				else
				{
					//is this "from" object in the shared context?  (ie, a shared "known" object)
					lock (knownMobiusMetaTables)
					{
						if (knownMobiusMetaTables.ContainsKey(serviceMetaTable.Name))
						{
							//we've seen this one before...
							mobiusMetaTable = knownMobiusMetaTables[serviceMetaTable.Name];
						}
						else
						{
							//create the new metatable
							mobiusMetaTable = new Mobius.Data.MetaTable();
							//save a reference in the shared context
							knownMobiusMetaTables.Add(serviceMetaTable.Name, mobiusMetaTable);
						}
					}

					//save a reference in the thread/call context
					context.KnownObjects.Add(serviceMetaTable, mobiusMetaTable);

					//set values
					mobiusMetaTable.AllowColumnMerging = serviceMetaTable.AllowColumnMerging;
					mobiusMetaTable.Code = serviceMetaTable.Code;
					//mobiusMetaTable.Creator = serviceMetaTable.Creator;
					mobiusMetaTable.Description = serviceMetaTable.Description;
					mobiusMetaTable.Id = serviceMetaTable.Id;
					mobiusMetaTable.Label = serviceMetaTable.Label;
					mobiusMetaTable.MetaBrokerType = (Mobius.Data.MetaBrokerType)((int)serviceMetaTable.MetaBrokerType);
					mobiusMetaTable.Name = serviceMetaTable.Name;
					mobiusMetaTable.PivotColumns = serviceMetaTable.PivotColumns;
					mobiusMetaTable.PivotMergeColumns = serviceMetaTable.PivotMergeColumns;
					mobiusMetaTable.QnLinkValue = serviceMetaTable.QnLinkValue;
					mobiusMetaTable.QnNumberValue = serviceMetaTable.QnNumberValue;
					mobiusMetaTable.QnNumberValueHigh = serviceMetaTable.QnNumberValueHigh;
					mobiusMetaTable.QnNValue = serviceMetaTable.QnNValue;
					mobiusMetaTable.QnNValueTested = serviceMetaTable.QnNValueTested;
					mobiusMetaTable.QnQualifier = serviceMetaTable.QnQualifier;
					mobiusMetaTable.QnStandardDeviation = serviceMetaTable.QnStandardDeviation;
					mobiusMetaTable.QnStandardError = serviceMetaTable.QnStandardError;
					mobiusMetaTable.QnTextValue = serviceMetaTable.QnTextValue;
					mobiusMetaTable.MultiPivot = serviceMetaTable.RemapForRetrieval;
					mobiusMetaTable.RowCountSql = serviceMetaTable.RowCountSql;
					mobiusMetaTable.ShortLabel = serviceMetaTable.ShortLabel;
					mobiusMetaTable.TableMap = serviceMetaTable.Source;
					mobiusMetaTable.SubstanceSummarizationType = (Mobius.Data.SubstanceSummarizationLevel)((int)serviceMetaTable.SummarizationType);
					mobiusMetaTable.SummarizedExists = serviceMetaTable.SummarizedExists;
					mobiusMetaTable.TableFilterColumns = serviceMetaTable.TableFilterColumns;
					mobiusMetaTable.TableFilterValues = serviceMetaTable.TableFilterValues;
					mobiusMetaTable.UpdateDateTimeSql = serviceMetaTable.UpdateDateTimeSql;
					mobiusMetaTable.UseSummarizedData = serviceMetaTable.UseSummarizedData;

					//set references (and update referenced objects?)
					mobiusMetaTable.ImportParms = ConvertServiceUserImportParamsToMobiusUserImportParams(serviceMetaTable.ImportParms, context);
					//KeyMetaTable is a static...  Never been set?
					if (Mobius.Data.MetaTable.KeyMetaTable == null &&
							serviceMetaTable.KeyMetaTable != null)
					{
						Mobius.Data.MetaTable.KeyMetaTable = ConvertServiceMetaTableToMobiusMetaTable(serviceMetaTable.KeyMetaTable, context);
					}
					mobiusMetaTable.MetaColumns = ConvertServiceMetaColumnsToMobiusMetaColumns(serviceMetaTable.MetaColumns, context);
					mobiusMetaTable.Parent = ConvertServiceMetaTableToMobiusMetaTable(serviceMetaTable.Parent, context);
				}
			}
			return mobiusMetaTable;
		}

		private List<Mobius.Data.MetaColumn> ConvertServiceMetaColumnsToMobiusMetaColumns(
				List<Mobius.Services.Types.MetaColumn> serviceMetaColumns, TypeConversionContext context)
		{
			List<Mobius.Data.MetaColumn> mobiusMetaColumns = null;
			if (serviceMetaColumns != null)
			{
				//since we don't maintain a cache of these, no way to shortcut out at this level
				// so ignore the bread crumbs

				mobiusMetaColumns = new List<Mobius.Data.MetaColumn>();
				foreach (Mobius.Services.Types.MetaColumn serviceMetaColumn in serviceMetaColumns)
				{
					Mobius.Data.MetaColumn metaColumn = ConvertServiceMetaColumnToMobiusMetaColumn(serviceMetaColumn, context);
					mobiusMetaColumns.Add(metaColumn);
				}
			}
			return mobiusMetaColumns;
		}

		private Dictionary<Mobius.Services.Types.MetaColumn, Mobius.Data.MetaColumn> knownMobiusMetaColumns =
				new Dictionary<Mobius.Services.Types.MetaColumn, Mobius.Data.MetaColumn>();
		private Mobius.Data.MetaColumn ConvertServiceMetaColumnToMobiusMetaColumn(
				Mobius.Services.Types.MetaColumn serviceMetaColumn, TypeConversionContext context)
		{
			Mobius.Data.MetaColumn mobiusMetaColumn = null;
			if (serviceMetaColumn != null)
			{
				//Is this object conversion already "in the stack"?
				List<BreadCrumb> breadCrumbs = context.BreadCrumbs;
				//Add it if not
				BreadCrumb breadCrumb = new BreadCrumb(MethodBase.GetCurrentMethod().Name, serviceMetaColumn);
				bool alreadyInProgress = true;
				if (!breadCrumbs.ContainsEquivalentBreadCrumb(breadCrumb))
				{
					alreadyInProgress = false;
					breadCrumbs.Add(breadCrumb);
				}

				if (alreadyInProgress)
				{
					//should already be a known object (in the current conversion context)
					mobiusMetaColumn = (Mobius.Data.MetaColumn)context.KnownObjects[serviceMetaColumn];
				}
				else
				{
					lock (knownMobiusMetaColumns)
					{
						if (knownMobiusMetaColumns.ContainsKey(serviceMetaColumn))
						{
							mobiusMetaColumn = knownMobiusMetaColumns[serviceMetaColumn];
						}
						else
						{
							mobiusMetaColumn = new Mobius.Data.MetaColumn();
							knownMobiusMetaColumns.Add(serviceMetaColumn, mobiusMetaColumn);
						}
					}
					context.KnownObjects.Add(serviceMetaColumn, mobiusMetaColumn);

					//set values
					mobiusMetaColumn.BrokerFiltering = serviceMetaColumn.BrokerFiltering;
					mobiusMetaColumn.ClickFunction = serviceMetaColumn.ClickFunction;
					mobiusMetaColumn.ColumnMap = serviceMetaColumn.ColumnMap;
					//mobiusMetaColumn.Creator = serviceMetaColumn.Creator;
					mobiusMetaColumn.DataTransform = serviceMetaColumn.DataTransform;
					mobiusMetaColumn.DataType = (Mobius.Data.MetaColumnType)((int)serviceMetaColumn.DataType);
					mobiusMetaColumn.Decimals = serviceMetaColumn.Decimals;
					mobiusMetaColumn.Description = serviceMetaColumn.Description;
					mobiusMetaColumn.DetailsAvailable = serviceMetaColumn.DetailsAvailable;
					mobiusMetaColumn.Dictionary = serviceMetaColumn.Dictionary;
					mobiusMetaColumn.DictionaryMultipleSelect = serviceMetaColumn.DictionaryMultipleSelect;
					mobiusMetaColumn.Format = (Mobius.Data.ColumnFormatEnum)((int)serviceMetaColumn.Format);
					mobiusMetaColumn.Id = serviceMetaColumn.Id;
					mobiusMetaColumn.IsSearchable = serviceMetaColumn.Searchable;
					mobiusMetaColumn.ImportFilePosition = serviceMetaColumn.ImportFilePosition;
					mobiusMetaColumn.KeyPosition = serviceMetaColumn.KeyPosition;
					mobiusMetaColumn.Label = serviceMetaColumn.Label;
					mobiusMetaColumn.LabelImage = serviceMetaColumn.LabelImage;
					mobiusMetaColumn.MetaBrokerType = (Mobius.Data.MetaBrokerType)((int)serviceMetaColumn.MetaBrokerType);
					//metaColumn.MetaTableId = metaTable.Id;
					mobiusMetaColumn.MultiPoint = serviceMetaColumn.MultiPoint;
					mobiusMetaColumn.Name = serviceMetaColumn.Name;
					mobiusMetaColumn.PivotValues = serviceMetaColumn.PivotValues;
					//mobiusMetaColumn.Position = serviceMetaColumn.Position;
					mobiusMetaColumn.PrimaryResult = serviceMetaColumn.PrimaryResult;
					mobiusMetaColumn.ResultCode = serviceMetaColumn.ResultCode;
					mobiusMetaColumn.SecondaryResult = serviceMetaColumn.SecondaryResult;
					mobiusMetaColumn.InitialSelection = (Mobius.Data.ColumnSelectionEnum)((int)serviceMetaColumn.Selection);
					mobiusMetaColumn.ShortLabel = serviceMetaColumn.ShortLabel;
					mobiusMetaColumn.SinglePoint = serviceMetaColumn.SinglePoint;
					mobiusMetaColumn.StorageType = (Mobius.Data.MetaColumnStorageType)((int)serviceMetaColumn.StorageType);
					mobiusMetaColumn.SummarizationRole = (Mobius.Data.SummarizationRole)((int)serviceMetaColumn.SummarizationRole);
					mobiusMetaColumn.SummarizedExists = serviceMetaColumn.SummarizedExists;
					mobiusMetaColumn.ResultCode2 = serviceMetaColumn.SummaryResultCode;
					mobiusMetaColumn.TableMap = serviceMetaColumn.TableMap;
					mobiusMetaColumn.TextCase = (Mobius.Data.ColumnTextCaseEnum)((int)serviceMetaColumn.TextCase);
					//mobiusMetaColumn.UnitId = serviceMetaColumn.UnitId;
					mobiusMetaColumn.Units = serviceMetaColumn.Units;
					mobiusMetaColumn.UnsummarizedExists = serviceMetaColumn.UnsummarizedExists;
					//mobiusMetaColumn.UpdateDateTime = serviceMetaColumn.UpdateDateTime;
					mobiusMetaColumn.Width = serviceMetaColumn.Width;

					//set references (and update referenced objects?)
					mobiusMetaColumn.CondFormat = ConvertServiceCondFormatToMobiusCondFormat(serviceMetaColumn.CondFormat, context);
					mobiusMetaColumn.MetaTable = ConvertServiceMetaTableToMobiusMetaTable(serviceMetaColumn.MetaTable, context);
					mobiusMetaColumn.Position = (mobiusMetaColumn.MetaTable == null) ? -1 : mobiusMetaColumn.MetaTable.Id;
				}
			}
			return mobiusMetaColumn;
		}

		private Dictionary<Mobius.Services.Types.CalcField, Mobius.Data.CalcField> knownMobiusCalcFields =
				new Dictionary<Mobius.Services.Types.CalcField, Mobius.Data.CalcField>();
		private Mobius.Data.CalcField ConvertServiceCalcFieldToMobiusCalcField(
				Mobius.Services.Types.CalcField serviceCalcField, TypeConversionContext context)
		{
			Mobius.Data.CalcField mobiusCalcField = null;
			if (serviceCalcField != null)
			{
				//Is this object conversion already "in the stack"?
				List<BreadCrumb> breadCrumbs = context.BreadCrumbs;
				//Add it if not
				BreadCrumb breadCrumb = new BreadCrumb(MethodBase.GetCurrentMethod().Name, serviceCalcField);
				bool alreadyInProgress = true;
				if (!breadCrumbs.ContainsEquivalentBreadCrumb(breadCrumb))
				{
					alreadyInProgress = false;
					breadCrumbs.Add(breadCrumb);
				}

				if (alreadyInProgress)
				{
					//should already be a known object
					mobiusCalcField = (Mobius.Data.CalcField)context.KnownObjects[serviceCalcField];
				}
				else
				{
					lock (knownMobiusCalcFields)
					{
						if (knownMobiusCalcFields.ContainsKey(serviceCalcField))
						{
							mobiusCalcField = knownMobiusCalcFields[serviceCalcField];
						}
						else
						{
							mobiusCalcField = new Mobius.Data.CalcField();
							knownMobiusCalcFields.Add(serviceCalcField, mobiusCalcField);
						}
					}
					context.KnownObjects.Add(serviceCalcField, mobiusCalcField);

					//copy values
					//mobiusCalcField.AdvancedExpr = serviceCalcField.AdvancedExprs;
					//mobiusCalcField.CalcType = (Mobius.Data.CalcTypeEnum)((int)serviceCalcField.CalcType);
					//mobiusCalcField.Classification = ConvertServiceCondFormatToMobiusCondFormat(serviceCalcField.Classification, context);
					//mobiusCalcField.Column1 = ConvertServiceMetaColumnToMobiusMetaColumn(serviceCalcField.Column1, context);
					//mobiusCalcField.Column2 = ConvertServiceMetaColumnToMobiusMetaColumn(serviceCalcField.Column2, context);
					//mobiusCalcField.Constant1 = serviceCalcField.Constant1;
					//mobiusCalcField.Constant1Double = serviceCalcField.Constant1Double;
					//mobiusCalcField.Constant2 = serviceCalcField.Constant2;
					//mobiusCalcField.Constant2Double = serviceCalcField.Constant2Double;
					//mobiusCalcField.Description = serviceCalcField.Description;
					//mobiusCalcField.FinalResultType = (Mobius.Data.MetaColumnType)((int)serviceCalcField.FinalResultType);
					//mobiusCalcField.Function1 = serviceCalcField.Function1;
					//mobiusCalcField.Function1Enum = (Mobius.Data.CalcFuncEnum)((int)serviceCalcField.Function1Enum);
					//mobiusCalcField.Function2 = serviceCalcField.Function2;
					//mobiusCalcField.Function2Enum = (Mobius.Data.CalcFuncEnum)((int)serviceCalcField.Function2Enum);
					//mobiusCalcField.OpEnum = (Mobius.Data.CalcOpEnum)((int)serviceCalcField.OpEnum);
					//mobiusCalcField.Operation = serviceCalcField.Operation;
					//mobiusCalcField.PreclassificationlResultType = (Mobius.Data.MetaColumnType)((int)serviceCalcField.PreclassificationlResultType);
					//mobiusCalcField.Prompt = serviceCalcField.Prompt;
					//mobiusCalcField.SourceColumnType = (Mobius.Data.MetaColumnType)((int)serviceCalcField.SourceColumnType);

					//copy references (and update referenced objects?)
					mobiusCalcField.UserObject = ConvertServiceUserObjectNodeToMobiusUserObject(serviceCalcField.UserObjectNode, context);
				}
			}
			return mobiusCalcField;
		}

		Dictionary<Mobius.Services.Types.CondFormat, Mobius.Data.CondFormat> knownMobiusCondFormats =
				new Dictionary<Mobius.Services.Types.CondFormat, Mobius.Data.CondFormat>();
		private Mobius.Data.CondFormat ConvertServiceCondFormatToMobiusCondFormat(
				Mobius.Services.Types.CondFormat serviceCondFormat, TypeConversionContext context)
		{
			Mobius.Data.CondFormat mobiusCondFormat = null;
			if (serviceCondFormat != null)
			{
				//since we don't maintain a cache of these, no way to shortcut out at this level
				// so ignore the bread crumbs
				lock (knownMobiusCondFormats)
				{
					if (knownMobiusCondFormats.ContainsKey(serviceCondFormat))
					{
						mobiusCondFormat = knownMobiusCondFormats[serviceCondFormat];
					}
					else
					{
						mobiusCondFormat = new Mobius.Data.CondFormat();
						knownMobiusCondFormats.Add(serviceCondFormat, mobiusCondFormat);
					}
				}

				mobiusCondFormat.ColumnType = (Mobius.Data.MetaColumnType)((int)serviceCondFormat.ColumnType);
				mobiusCondFormat.Name = serviceCondFormat.Name;
				mobiusCondFormat.Option1 = serviceCondFormat.Option1;
				mobiusCondFormat.Option2 = serviceCondFormat.Option2;
				if (serviceCondFormat.Rules != null)
				{
					Mobius.Data.CondFormatRules mobiusRules = new Mobius.Data.CondFormatRules();
					//rules.ColorContinuously = mobiusMetaColumn.CondFormat.Rules.ColorContinuously;
					mobiusCondFormat.Rules = mobiusRules;
					foreach (Mobius.Services.Types.CondFormatRule serviceRule in serviceCondFormat.Rules.rulesList)
					{
						Mobius.Data.CondFormatRule mobiusRule = new Mobius.Data.CondFormatRule();
						mobiusCondFormat.Rules.Add(mobiusRule);

						mobiusRule.BackColor1 = (System.Drawing.Color)serviceRule.BackColor;
						mobiusRule.Epsilon = serviceRule.Epsilon;
						mobiusRule.Font = (System.Drawing.Font)serviceRule.Font;
						mobiusRule.ForeColor = (System.Drawing.Color)serviceRule.ForeColor;
						mobiusRule.Name = serviceRule.Name;
						mobiusRule.Op = serviceRule.Op;
						mobiusRule.OpCode = (Mobius.Data.CondFormatOpCode)((int)serviceRule.OpCode);
						mobiusRule.Value = serviceRule.Value;
						mobiusRule.Value2 = serviceRule.Value2;
						mobiusRule.Value2Normalized = serviceRule.Value2Normalized;
						mobiusRule.Value2Number = serviceRule.Value2Number;
						mobiusRule.ValueDict = serviceRule.ValueDict;
						mobiusRule.ValueNormalized = serviceRule.ValueNormalized;
						mobiusRule.ValueNumber = serviceRule.ValueNumber;
					}
				}
			}
			return mobiusCondFormat;
		}

		private Mobius.Data.UserDataImportParms ConvertServiceUserImportParamsToMobiusUserImportParams(
				Mobius.Services.Types.UserDataImportParms serviceImportParms, TypeConversionContext context)
		{
			Mobius.Data.UserDataImportParms mobiusImportParams = null;
			if (serviceImportParms != null)
			{
				//no cache, so breadcrumbs pass thru

				mobiusImportParams = new Mobius.Data.UserDataImportParms();
				mobiusImportParams.CheckForFileUpdates = serviceImportParms.CheckForFileUpdates;
				mobiusImportParams.ClientFileModified = serviceImportParms.ClientFileModified;
				mobiusImportParams.DeleteExisting = serviceImportParms.DeleteExisting;
				mobiusImportParams.Delim = serviceImportParms.Delim;
				mobiusImportParams.FileName = serviceImportParms.FileName;
				mobiusImportParams.FirstLineHeaders = serviceImportParms.FirstLineHeaders;
				mobiusImportParams.ImportInBackground = serviceImportParms.ImportInBackground;
				mobiusImportParams.Labels = serviceImportParms.Labels;
				mobiusImportParams.MultDelimsAsSingle = serviceImportParms.MultDelimsAsSingle;
				mobiusImportParams.TextQualifier = serviceImportParms.TextQualifier;

				if (serviceImportParms.Fc2Mc != null)
				{
					mobiusImportParams.Fc2Mc = new Dictionary<string, Mobius.Data.MetaColumn>(serviceImportParms.Fc2Mc.Count);
					foreach (string key in serviceImportParms.Fc2Mc.Keys)
					{
						Mobius.Services.Types.MetaColumn serviceMetaColumn =
								serviceImportParms.Fc2Mc[key];
						Mobius.Data.MetaColumn mobiusMetaColumn =
								ConvertServiceMetaColumnToMobiusMetaColumn(serviceMetaColumn, context);
						mobiusImportParams.Fc2Mc.Add(key, mobiusMetaColumn);
					}
				}
				if (serviceImportParms.Types != null)
				{
					mobiusImportParams.Types = new List<Mobius.Data.MetaColumnType>(serviceImportParms.Types.Count);
					for (int i = 0; i < serviceImportParms.Types.Count; i++)
					{
						Mobius.Services.Types.MetaColumnType serviceMetaColumnType = serviceImportParms.Types[i];
						Mobius.Data.MetaColumnType mobiusMetaColumnType = (Mobius.Data.MetaColumnType)((int)serviceMetaColumnType);
						mobiusImportParams.Types.Add(mobiusMetaColumnType);
					}
				}
			}
			return mobiusImportParams;
		}



		private Dictionary<string, Mobius.Services.Types.MetaTable> knownServiceMetaTables = new Dictionary<string, Mobius.Services.Types.MetaTable>();
		private Mobius.Services.Types.MetaTable ConvertMobiusMetaTableToServiceMetaTable(
				Mobius.Data.MetaTable mobiusMetaTable, TypeConversionContext context)
		{
			Mobius.Services.Types.MetaTable serviceMetaTable = null;
			if (mobiusMetaTable != null)
			{
				//Is this object conversion already "in the stack"?
				List<BreadCrumb> breadCrumbs = context.BreadCrumbs;
				//Add it if not
				BreadCrumb breadCrumb = new BreadCrumb(MethodBase.GetCurrentMethod().Name, mobiusMetaTable);
				bool alreadyInProgress = true;
				if (!breadCrumbs.ContainsEquivalentBreadCrumb(breadCrumb))
				{
					alreadyInProgress = false;
					breadCrumbs.Add(breadCrumb);
				}

				if (alreadyInProgress)
				{
					//should already be a known object
					serviceMetaTable = (Mobius.Services.Types.MetaTable)context.KnownObjects[mobiusMetaTable];
				}
				else
				{
					lock (knownServiceMetaTables)
					{
						if (knownServiceMetaTables.ContainsKey(mobiusMetaTable.Name))
						{
							serviceMetaTable = knownServiceMetaTables[mobiusMetaTable.Name];
						}
						else
						{
							//create the new metatable
							serviceMetaTable = new Types.MetaTable();
							//so that we can shortcut out if we encounter this table again
							knownServiceMetaTables.Add(mobiusMetaTable.Name, serviceMetaTable);
						}
					}
					context.KnownObjects.Add(mobiusMetaTable, serviceMetaTable);

					//copy values
					serviceMetaTable.AllowColumnMerging = mobiusMetaTable.AllowColumnMerging;
					serviceMetaTable.Code = mobiusMetaTable.Code;
					//serviceMetaTable.Creator = mobiusMetaTable.Creator;
					serviceMetaTable.Description = mobiusMetaTable.Description;
					serviceMetaTable.Id = mobiusMetaTable.Id;
					serviceMetaTable.Label = mobiusMetaTable.Label;
					serviceMetaTable.MetaBrokerType = (Mobius.Services.Types.MetaBrokerType)((int)mobiusMetaTable.MetaBrokerType);
					serviceMetaTable.Name = mobiusMetaTable.Name;
					serviceMetaTable.PivotColumns = mobiusMetaTable.PivotColumns;
					serviceMetaTable.PivotMergeColumns = mobiusMetaTable.PivotMergeColumns;
					serviceMetaTable.QnLinkValue = mobiusMetaTable.QnLinkValue;
					serviceMetaTable.QnNumberValue = mobiusMetaTable.QnNumberValue;
					serviceMetaTable.QnNumberValueHigh = mobiusMetaTable.QnNumberValueHigh;
					serviceMetaTable.QnNValue = mobiusMetaTable.QnNValue;
					serviceMetaTable.QnNValueTested = mobiusMetaTable.QnNValueTested;
					serviceMetaTable.QnQualifier = mobiusMetaTable.QnQualifier;
					serviceMetaTable.QnStandardDeviation = mobiusMetaTable.QnStandardDeviation;
					serviceMetaTable.QnStandardError = mobiusMetaTable.QnStandardError;
					serviceMetaTable.QnTextValue = mobiusMetaTable.QnTextValue;
					serviceMetaTable.RemapForRetrieval = mobiusMetaTable.MultiPivot;
					serviceMetaTable.RowCountSql = mobiusMetaTable.RowCountSql;
					serviceMetaTable.ShortLabel = mobiusMetaTable.ShortLabel;
					serviceMetaTable.Source = mobiusMetaTable.TableMap;
					serviceMetaTable.SummarizationType = (Mobius.Services.Types.SummarizationTypeEnum)((int)mobiusMetaTable.SubstanceSummarizationType);
					serviceMetaTable.SummarizedExists = mobiusMetaTable.SummarizedExists;
					serviceMetaTable.TableFilterColumns = mobiusMetaTable.TableFilterColumns;
					serviceMetaTable.TableFilterValues = mobiusMetaTable.TableFilterValues;
					serviceMetaTable.UnsummarizedExists = mobiusMetaTable.UnsummarizedExists;
					serviceMetaTable.UpdateDateTimeSql = mobiusMetaTable.UpdateDateTimeSql;
					serviceMetaTable.UseSummarizedData = mobiusMetaTable.UseSummarizedData;

					//copy referenced objects (update referenced objects?)
					serviceMetaTable.ImportParms = ConvertMobiusUserImportParamsToServiceUserImportParams(mobiusMetaTable.ImportParms, context);
					//KeyMetaTable is a static...?  Not really sure that this is safe...
					serviceMetaTable.KeyMetaTable = ConvertMobiusMetaTableToServiceMetaTable(Mobius.Data.MetaTable.KeyMetaTable, context);
					serviceMetaTable.MetaColumns = ConvertMobiusMetaColumnsToServiceMetaColumns(mobiusMetaTable.MetaColumns, context);
					serviceMetaTable.Parent = ConvertMobiusMetaTableToServiceMetaTable(mobiusMetaTable.Parent, context);
				}
			}
			return serviceMetaTable;
		}

		private Mobius.Services.Types.UserDataImportParms ConvertMobiusUserImportParamsToServiceUserImportParams(
				Mobius.Data.UserDataImportParms mobiusImportParms, TypeConversionContext context)
		{
			Mobius.Services.Types.UserDataImportParms serviceImportParams = null;
			if (mobiusImportParms != null)
			{
				//no cache, so breadcrumbs pass thru

				serviceImportParams = new Mobius.Services.Types.UserDataImportParms();
				serviceImportParams.CheckForFileUpdates = mobiusImportParms.CheckForFileUpdates;
				serviceImportParams.ClientFileModified = mobiusImportParms.ClientFileModified;
				serviceImportParams.DeleteExisting = mobiusImportParms.DeleteExisting;
				serviceImportParams.Delim = mobiusImportParms.Delim;
				serviceImportParams.FileName = mobiusImportParms.FileName;
				serviceImportParams.FirstLineHeaders = mobiusImportParms.FirstLineHeaders;
				serviceImportParams.ImportInBackground = mobiusImportParms.ImportInBackground;
				serviceImportParams.Labels = mobiusImportParms.Labels;
				serviceImportParams.MultDelimsAsSingle = mobiusImportParms.MultDelimsAsSingle;
				serviceImportParams.TextQualifier = mobiusImportParms.TextQualifier;

				if (mobiusImportParms.Fc2Mc != null)
				{
					serviceImportParams.Fc2Mc = new Dictionary<string, Mobius.Services.Types.MetaColumn>(mobiusImportParms.Fc2Mc.Count);
					foreach (string key in mobiusImportParms.Fc2Mc.Keys)
					{
						Mobius.Data.MetaColumn mobiusMetaColumn =
								mobiusImportParms.Fc2Mc[key];
						Mobius.Services.Types.MetaColumn serviceMetaColumn =
								ConvertMobiusMetaColumnToServiceMetaColumn(mobiusMetaColumn, context);
						serviceImportParams.Fc2Mc.Add(key, serviceMetaColumn);
					}
				}
				if (mobiusImportParms.Types != null)
				{
					serviceImportParams.Types = new List<Mobius.Services.Types.MetaColumnType>(mobiusImportParms.Types.Count);
					for (int i = 0; i < mobiusImportParms.Types.Count; i++)
					{
						Mobius.Data.MetaColumnType mobiusMetaColumnType = mobiusImportParms.Types[i];
						Mobius.Services.Types.MetaColumnType serviceMetaColumnType = (Mobius.Services.Types.MetaColumnType)((int)mobiusMetaColumnType);
						serviceImportParams.Types.Add(serviceMetaColumnType);
					}
				}
			}
			return serviceImportParams;
		}

		private List<Mobius.Services.Types.MetaColumn> ConvertMobiusMetaColumnsToServiceMetaColumns(
				List<Mobius.Data.MetaColumn> mobiusMetaColumns, TypeConversionContext context)
		{
			List<Mobius.Services.Types.MetaColumn> metaColumns = null;
			if (mobiusMetaColumns != null)
			{
				//since we don't maintain a cache of these, no way to shortcut out at this level
				// so ignore the bread crumbs

				metaColumns = new List<Mobius.Services.Types.MetaColumn>();
				foreach (Mobius.Data.MetaColumn mobiusMetaColumn in mobiusMetaColumns)
				{
					Mobius.Services.Types.MetaColumn metaColumn = ConvertMobiusMetaColumnToServiceMetaColumn(mobiusMetaColumn, context);
					metaColumns.Add(metaColumn);
				}
			}
			return metaColumns;
		}

		private Dictionary<Mobius.Data.MetaColumn, Mobius.Services.Types.MetaColumn> knownServiceMetaColumns =
				new Dictionary<Mobius.Data.MetaColumn, Mobius.Services.Types.MetaColumn>();
		private Mobius.Services.Types.MetaColumn ConvertMobiusMetaColumnToServiceMetaColumn(
				Mobius.Data.MetaColumn mobiusMetaColumn, TypeConversionContext context)
		{
			Mobius.Services.Types.MetaColumn serviceMetaColumn = null;

			if (mobiusMetaColumn != null)
			{
				//Is this object conversion already "in the stack"?
				List<BreadCrumb> breadCrumbs = context.BreadCrumbs;
				//Add it if not
				BreadCrumb breadCrumb = new BreadCrumb(MethodBase.GetCurrentMethod().Name, mobiusMetaColumn);
				bool alreadyInProgress = true;
				if (!breadCrumbs.ContainsEquivalentBreadCrumb(breadCrumb))
				{
					alreadyInProgress = false;
					breadCrumbs.Add(breadCrumb);
				}

				if (alreadyInProgress)
				{
					//should already be a known object
					serviceMetaColumn = (Mobius.Services.Types.MetaColumn)context.KnownObjects[mobiusMetaColumn];
				}
				else
				{
					lock (knownServiceMetaColumns)
					{
						if (knownServiceMetaColumns.ContainsKey(mobiusMetaColumn))
						{
							serviceMetaColumn = knownServiceMetaColumns[mobiusMetaColumn];
						}
						else
						{
							serviceMetaColumn = new Mobius.Services.Types.MetaColumn();
							knownServiceMetaColumns.Add(mobiusMetaColumn, serviceMetaColumn);
						}
					}
					context.KnownObjects.Add(mobiusMetaColumn, serviceMetaColumn);

					//copy values
					serviceMetaColumn.BrokerFiltering = mobiusMetaColumn.BrokerFiltering;
					serviceMetaColumn.ClickFunction = mobiusMetaColumn.ClickFunction;
					serviceMetaColumn.ColumnMap = mobiusMetaColumn.ColumnMap;
					//serviceMetaColumn.Creator = mobiusMetaColumn.Creator;
					serviceMetaColumn.DataTransform = mobiusMetaColumn.DataTransform;
					serviceMetaColumn.DataType = (Mobius.Services.Types.MetaColumnType)((int)mobiusMetaColumn.DataType);
					serviceMetaColumn.Decimals = mobiusMetaColumn.Decimals;
					serviceMetaColumn.Description = mobiusMetaColumn.Description;
					serviceMetaColumn.DetailsAvailable = mobiusMetaColumn.DetailsAvailable;
					serviceMetaColumn.Dictionary = mobiusMetaColumn.Dictionary;
					serviceMetaColumn.DictionaryMultipleSelect = mobiusMetaColumn.DictionaryMultipleSelect;
					serviceMetaColumn.Format = (Mobius.Services.Types.ColumnFormatEnum)((int)mobiusMetaColumn.Format);
					serviceMetaColumn.Id = mobiusMetaColumn.Id;
					serviceMetaColumn.Searchable = mobiusMetaColumn.IsSearchable;
					serviceMetaColumn.ImportFilePosition = mobiusMetaColumn.ImportFilePosition;
					serviceMetaColumn.KeyPosition = mobiusMetaColumn.KeyPosition;
					serviceMetaColumn.Label = mobiusMetaColumn.Label;
					serviceMetaColumn.LabelImage = mobiusMetaColumn.LabelImage;
					serviceMetaColumn.MetaBrokerType = (Mobius.Services.Types.MetaBrokerType)((int)mobiusMetaColumn.MetaBrokerType);
					serviceMetaColumn.MultiPoint = mobiusMetaColumn.MultiPoint;
					serviceMetaColumn.Name = mobiusMetaColumn.Name;
					serviceMetaColumn.PivotValues = mobiusMetaColumn.PivotValues;
					//serviceMetaColumn.Position = mobiusMetaColumn.Position;
					serviceMetaColumn.PrimaryResult = mobiusMetaColumn.PrimaryResult;
					serviceMetaColumn.ResultCode = mobiusMetaColumn.ResultCode;
					serviceMetaColumn.SecondaryResult = mobiusMetaColumn.SecondaryResult;
					serviceMetaColumn.Selection = (Mobius.Services.Types.ColumnSelectionEnum)((int)mobiusMetaColumn.InitialSelection);
					serviceMetaColumn.ShortLabel = mobiusMetaColumn.ShortLabel;
					serviceMetaColumn.SinglePoint = mobiusMetaColumn.SinglePoint;
					// todo: fix this - serviceMetaColumn.StorageType = (Mobius.Services.Types.MetaColumnType)((int)mobiusMetaColumn.StorageType);
					serviceMetaColumn.SummarizationRole = (Mobius.Services.Types.SummarizationRole)((int)mobiusMetaColumn.SummarizationRole);
					serviceMetaColumn.SummarizedExists = mobiusMetaColumn.SummarizedExists;
					serviceMetaColumn.SummaryResultCode = mobiusMetaColumn.ResultCode2;
					serviceMetaColumn.TableMap = mobiusMetaColumn.TableMap;
					serviceMetaColumn.TextCase = (Mobius.Services.Types.ColumnTextCaseEnum)((int)mobiusMetaColumn.TextCase);
					//serviceMetaColumn.UnitId = mobiusMetaColumn.UnitId;
					serviceMetaColumn.Units = mobiusMetaColumn.Units;
					serviceMetaColumn.UnsummarizedExists = mobiusMetaColumn.UnsummarizedExists;
					//serviceMetaColumn.UpdateDateTime = mobiusMetaColumn.UpdateDateTime;
					serviceMetaColumn.Width = mobiusMetaColumn.Width;

					//copy references (and update referenced objects?)
					serviceMetaColumn.CondFormat = ConvertMobiusCondFormatToServiceCondFormat(mobiusMetaColumn.CondFormat, context);
					serviceMetaColumn.MetaTable = ConvertMobiusMetaTableToServiceMetaTable(mobiusMetaColumn.MetaTable, context);
					serviceMetaColumn.MetaTableId = (serviceMetaColumn.MetaTable == null) ? -1 : serviceMetaColumn.MetaTable.Id;
				}
			}
			return serviceMetaColumn;
		}

		private Dictionary<Mobius.Data.CalcField, Mobius.Services.Types.CalcField> knownServiceCalcFields =
				new Dictionary<Mobius.Data.CalcField, Mobius.Services.Types.CalcField>();
		private Mobius.Services.Types.CalcField ConvertMobiusCalcFieldToServiceCalcField(
				Mobius.Data.CalcField mobiusCalcField, TypeConversionContext context)
		{
			Mobius.Services.Types.CalcField serviceCalcField = null;
			if (mobiusCalcField != null)
			{
				//Is this object conversion already "in the stack"?
				List<BreadCrumb> breadCrumbs = context.BreadCrumbs;
				//Add it if not
				BreadCrumb breadCrumb = new BreadCrumb(MethodBase.GetCurrentMethod().Name, mobiusCalcField);
				bool alreadyInProgress = true;
				if (!breadCrumbs.ContainsEquivalentBreadCrumb(breadCrumb))
				{
					alreadyInProgress = false;
					breadCrumbs.Add(breadCrumb);
				}

				if (alreadyInProgress)
				{
					//should already be a known object
					serviceCalcField = (Mobius.Services.Types.CalcField)context.KnownObjects[mobiusCalcField];
				}
				else
				{
					lock (knownServiceCalcFields)
					{
						if (knownServiceCalcFields.ContainsKey(mobiusCalcField))
						{
							serviceCalcField = knownServiceCalcFields[mobiusCalcField];
						}
						else
						{
							serviceCalcField = new Mobius.Services.Types.CalcField();
							knownServiceCalcFields.Add(mobiusCalcField, serviceCalcField);
						}
					}
					context.KnownObjects.Add(mobiusCalcField, serviceCalcField);

					//copy values
					//serviceCalcField.AdvancedExprs = mobiusCalcField.AdvancedExpr;
					//serviceCalcField.CalcType = (Mobius.Services.Types.CalcTypeEnum)((int)mobiusCalcField.CalcType);
					//serviceCalcField.Classification = ConvertMobiusCondFormatToServiceCondFormat(mobiusCalcField.Classification, context);
					//serviceCalcField.Column1 = ConvertMobiusMetaColumnToServiceMetaColumn(mobiusCalcField.Column1, context);
					//serviceCalcField.Column2 = ConvertMobiusMetaColumnToServiceMetaColumn(mobiusCalcField.Column2, context);
					//serviceCalcField.Constant1 = mobiusCalcField.Constant1;
					//serviceCalcField.Constant1Double = mobiusCalcField.Constant1Double;
					//serviceCalcField.Constant2 = mobiusCalcField.Constant2;
					//serviceCalcField.Constant2Double = mobiusCalcField.Constant2Double;
					//serviceCalcField.Description = mobiusCalcField.Description;
					//serviceCalcField.FinalResultType = (Mobius.Services.Types.MetaColumnType)((int)mobiusCalcField.FinalResultType);
					//serviceCalcField.Function1 = mobiusCalcField.Function1;
					//serviceCalcField.Function1Enum = (Mobius.Services.Types.CalcFuncEnum)((int)mobiusCalcField.Function1Enum);
					//serviceCalcField.Function2 = mobiusCalcField.Function2;
					//serviceCalcField.Function2Enum = (Mobius.Services.Types.CalcFuncEnum)((int)mobiusCalcField.Function2Enum);
					//serviceCalcField.OpEnum = (Mobius.Services.Types.CalcOpEnum)((int)mobiusCalcField.OpEnum);
					//serviceCalcField.Operation = mobiusCalcField.Operation;
					//serviceCalcField.PreclassificationlResultType = (Mobius.Services.Types.MetaColumnType)((int)mobiusCalcField.PreclassificationlResultType);
					//serviceCalcField.Prompt = mobiusCalcField.Prompt;
					//serviceCalcField.SourceColumnType = (Mobius.Services.Types.MetaColumnType)((int)mobiusCalcField.SourceColumnType);

					//copy references (and update referenced objects?)
					serviceCalcField.UserObjectNode = ConvertMobiusUserObjectToServiceUserObjectNode(mobiusCalcField.UserObject, context);
				}
			}
			return serviceCalcField;
		}

		private Dictionary<Mobius.Data.CondFormat, Mobius.Services.Types.CondFormat> knownServiceCondFormats =
				new Dictionary<Mobius.Data.CondFormat, Mobius.Services.Types.CondFormat>();
		private Mobius.Services.Types.CondFormat ConvertMobiusCondFormatToServiceCondFormat(
				Mobius.Data.CondFormat mobiusCondFormat, TypeConversionContext context)
		{
			Mobius.Services.Types.CondFormat serviceCondFormat = null;
			if (mobiusCondFormat != null)
			{
				//since we don't maintain a cache of these, no way to shortcut out at this level
				// so ignore the bread crumbs

				lock (knownServiceCondFormats)
				{
					if (knownServiceCondFormats.ContainsKey(mobiusCondFormat))
					{
						serviceCondFormat = knownServiceCondFormats[mobiusCondFormat];
					}
					else
					{
						serviceCondFormat = new Mobius.Services.Types.CondFormat();
						knownServiceCondFormats.Add(mobiusCondFormat, serviceCondFormat);
					}
				}

				serviceCondFormat.ColumnType = (Mobius.Services.Types.MetaColumnType)((int)mobiusCondFormat.ColumnType);
				serviceCondFormat.Name = mobiusCondFormat.Name;
				serviceCondFormat.Option1 = mobiusCondFormat.Option1;
				serviceCondFormat.Option2 = mobiusCondFormat.Option2;
				if (mobiusCondFormat.Rules != null)
				{
					Mobius.Services.Types.CondFormatRules serviceCondFormatRules = new Mobius.Services.Types.CondFormatRules();
					//rules.ColorContinuously = mobiusCondFormat.Rules.ColorContinuously;
					serviceCondFormat.Rules = serviceCondFormatRules;
					foreach (Mobius.Data.CondFormatRule mobiusRule in mobiusCondFormat.Rules)
					{
						Mobius.Services.Types.CondFormatRule serviceRule = new Mobius.Services.Types.CondFormatRule();
						serviceCondFormat.Rules.rulesList.Add(serviceRule);

						serviceRule.BackColor = new Mobius.Services.Types.Color(mobiusRule.BackColor1);
						serviceRule.Epsilon = mobiusRule.Epsilon;
						serviceRule.Font = new Mobius.Services.Types.Font(mobiusRule.Font);
						serviceRule.ForeColor = new Mobius.Services.Types.Color(mobiusRule.ForeColor);
						serviceRule.Name = mobiusRule.Name;
						serviceRule.Op = mobiusRule.Op;
						serviceRule.OpCode = (Mobius.Services.Types.CondFormatOpCode)((int)mobiusRule.OpCode);
						serviceRule.Value = mobiusRule.Value;
						serviceRule.Value2 = mobiusRule.Value2;
						serviceRule.Value2Normalized = mobiusRule.Value2Normalized;
						serviceRule.Value2Number = mobiusRule.Value2Number;
						serviceRule.ValueDict = mobiusRule.ValueDict;
						serviceRule.ValueNormalized = mobiusRule.ValueNormalized;
						serviceRule.ValueNumber = mobiusRule.ValueNumber;
					}
				}
			}
			return serviceCondFormat;
		}



		private List<Mobius.Services.Types.UserObjectNode> ConvertMobiusUserObjectsToServiceUserObjectNodes(
				List<Mobius.Data.UserObject> mobiusUserObjects, TypeConversionContext context)
		{
			List<Mobius.Services.Types.UserObjectNode> serviceUserObjectNodes = null;
			if (mobiusUserObjects != null)
			{
				//since we don't maintain a cache of these, no way to shortcut out at this level
				// so ignore the bread crumbs

				serviceUserObjectNodes = new List<Mobius.Services.Types.UserObjectNode>();
				foreach (Mobius.Data.UserObject mobiusUserObject in mobiusUserObjects)
				{
					serviceUserObjectNodes.Add(ConvertMobiusUserObjectToServiceUserObjectNode(mobiusUserObject, context));
				}
			}
			return serviceUserObjectNodes;
		}

		private Mobius.Services.Types.UserObjectNode ConvertMobiusUserObjectToServiceUserObjectNode(
				Mobius.Data.UserObject mobiusUserObject, TypeConversionContext context)
		{
			Mobius.Services.Types.UserObjectNode serviceUserObjectNode = null;
			if (mobiusUserObject != null)
			{
				//since we don't maintain a cache of these, no way to shortcut out at this level
				// so ignore the bread crumbs

				serviceUserObjectNode = new Mobius.Services.Types.UserObjectNode();
				serviceUserObjectNode.AccessLevel = (Mobius.Services.Types.UserObjectAccess)mobiusUserObject.AccessLevel;
				serviceUserObjectNode.ACL = mobiusUserObject.ACL;
				serviceUserObjectNode.Content = mobiusUserObject.Content;
				serviceUserObjectNode.Count = mobiusUserObject.Count;
				serviceUserObjectNode.CreationDateTime = mobiusUserObject.CreationDateTime;
				serviceUserObjectNode.Description = mobiusUserObject.Description;
				serviceUserObjectNode.Id = mobiusUserObject.Id;
				serviceUserObjectNode.Name = mobiusUserObject.Name;
				serviceUserObjectNode.Owner = mobiusUserObject.Owner;
				serviceUserObjectNode.ParentFolder = mobiusUserObject.ParentFolder;
				serviceUserObjectNode.ParentFolderType = (Mobius.Services.Types.FolderTypeEnum)mobiusUserObject.ParentFolderType;
				serviceUserObjectNode.Type = (Mobius.Services.Types.UserObjectType)mobiusUserObject.Type;
				serviceUserObjectNode.UpdateDateTime = mobiusUserObject.UpdateDateTime;
			}
			return serviceUserObjectNode;
		}

		private static Dictionary<Mobius.Data.UserObjectType, Mobius.Services.Types.NodeType> mobiusUserObjectTypeToServiceNodeType = null;
		private Mobius.Services.Types.Node ConvertMobiusUserObjectToServiceNode(
				Mobius.Data.UserObject mobiusUserObject, TypeConversionContext context)
		{
			Mobius.Services.Types.Node serviceNode = null;
			if (mobiusUserObject != null)
			{
				//since we don't maintain a cache of these, no way to shortcut out at this level
				// so ignore the bread crumbs

				serviceNode = new Mobius.Services.Types.Node();
				serviceNode.CreationTimestamp = mobiusUserObject.CreationDateTime;
				serviceNode.Description = mobiusUserObject.Description;
				serviceNode.Label = mobiusUserObject.Name;
				serviceNode.LastUpdateTimestamp = mobiusUserObject.UpdateDateTime;
				serviceNode.Name = mobiusUserObject.Type.ToString().ToUpper() + "_" + mobiusUserObject.Id;
				serviceNode.Owner = mobiusUserObject.Owner;
				serviceNode.Shared = (mobiusUserObject.AccessLevel == Mobius.Data.UserObjectAccess.Public);
				serviceNode.Size = mobiusUserObject.Count;
				serviceNode.Target = serviceNode.Name;

				//icky type conversion...  Use a custom map to handle "special cases" (and expected "normal" cases) first
				if (mobiusUserObjectTypeToServiceNodeType.ContainsKey(mobiusUserObject.Type))
				{
					//remapped types
					serviceNode.Type = mobiusUserObjectTypeToServiceNodeType[mobiusUserObject.Type];
				}
				else
				{
					//assume the corresponding type has the same name
					serviceNode.Type = (Mobius.Services.Types.NodeType)Enum.Parse(typeof(Mobius.Services.Types.NodeType), mobiusUserObject.Type.ToString());
				}
			}
			return serviceNode;
		}



		private List<Mobius.Data.UserObject> ConvertServiceUserObjectsToMobiusUserObjects(
				List<Mobius.Services.Types.UserObjectNode> serviceUserObjectNodes, TypeConversionContext context)
		{
			List<Mobius.Data.UserObject> mobiusUserObjects = null;
			if (serviceUserObjectNodes != null)
			{
				//since we don't maintain a cache of these, no way to shortcut out at this level
				// so ignore the bread crumbs

				mobiusUserObjects = new List<Mobius.Data.UserObject>();
				foreach (Mobius.Services.Types.UserObjectNode serviceUserObjectNode in serviceUserObjectNodes)
				{
					mobiusUserObjects.Add(ConvertServiceUserObjectNodeToMobiusUserObject(serviceUserObjectNode, context));
				}
			}
			return mobiusUserObjects;
		}

		private Mobius.Data.UserObject ConvertServiceUserObjectNodeToMobiusUserObject(
				Mobius.Services.Types.UserObjectNode serviceUserObjectNode, TypeConversionContext context)
		{
			Mobius.Data.UserObject mobiusUserObject = null;
			if (serviceUserObjectNode != null)
			{
				//since we don't maintain a cache of these, no way to shortcut out at this level
				// so ignore the bread crumbs

				mobiusUserObject = new Mobius.Data.UserObject();
				mobiusUserObject.AccessLevel = (Mobius.Data.UserObjectAccess)serviceUserObjectNode.AccessLevel;
				mobiusUserObject.ACL = serviceUserObjectNode.ACL;
				mobiusUserObject.Content = serviceUserObjectNode.Content;
				mobiusUserObject.Count = serviceUserObjectNode.Count;
				mobiusUserObject.CreationDateTime = serviceUserObjectNode.CreationDateTime;
				mobiusUserObject.Description = serviceUserObjectNode.Description;
				mobiusUserObject.Id = serviceUserObjectNode.Id;
				mobiusUserObject.Name = serviceUserObjectNode.Name;
				mobiusUserObject.Owner = serviceUserObjectNode.Owner;
				mobiusUserObject.ParentFolder = serviceUserObjectNode.ParentFolder;
				mobiusUserObject.ParentFolderType = (Mobius.Data.FolderTypeEnum)serviceUserObjectNode.ParentFolderType;
				mobiusUserObject.Type = (Mobius.Data.UserObjectType)serviceUserObjectNode.Type;
				mobiusUserObject.UpdateDateTime = serviceUserObjectNode.UpdateDateTime;
			}
			return mobiusUserObject;
		}


		private Dictionary<int, Mobius.Services.Types.Query> knownServiceQueries = new Dictionary<int, Mobius.Services.Types.Query>();
		private Mobius.Services.Types.Query ConvertMobiusQueryToServiceQuery(
				Mobius.Data.Query mobiusQuery, TypeConversionContext context)
		{
			Mobius.Services.Types.Query serviceQuery = null;
			if (mobiusQuery != null)
			{
				//Is this object conversion already "in the stack"?
				List<BreadCrumb> breadCrumbs = context.BreadCrumbs;
				//Add it if not
				BreadCrumb breadCrumb = new BreadCrumb(MethodBase.GetCurrentMethod().Name, mobiusQuery);
				bool alreadyInProgress = true;
				if (!breadCrumbs.ContainsEquivalentBreadCrumb(breadCrumb))
				{
					alreadyInProgress = false;
					breadCrumbs.Add(breadCrumb);
				}

				if (alreadyInProgress)
				{
					//should already be a known object
					serviceQuery = (Mobius.Services.Types.Query)context.KnownObjects[mobiusQuery];
				}
				else
				{
					lock (knownServiceQueries)
					{
						if (knownServiceQueries.ContainsKey(mobiusQuery.InstanceId))
						{
							serviceQuery = knownServiceQueries[mobiusQuery.InstanceId];
						}
						else
						{
							serviceQuery = new Mobius.Services.Types.Query();
							knownServiceQueries.Add(mobiusQuery.InstanceId, serviceQuery);
						}
					}
					context.KnownObjects.Add(mobiusQuery, serviceQuery);

					//copy values
					serviceQuery.Id = mobiusQuery.InstanceId;
					serviceQuery.AlertInterval = mobiusQuery.AlertInterval;
					serviceQuery.AlertQueryState = mobiusQuery.AlertQueryState;
					serviceQuery.AllowColumnMerging = mobiusQuery.AllowColumnMerging;
					serviceQuery.PrefetchImages = mobiusQuery.PrefetchImages;
					serviceQuery.BrowseExistingResultsWhenOpened = mobiusQuery.BrowseSavedResultsUponOpen;
					serviceQuery.ComplexCriteria = mobiusQuery.ComplexCriteria;
					serviceQuery.FilterNullRows = mobiusQuery.FilterNullRows;
					serviceQuery.FilterNullRowsStrong = mobiusQuery.FilterNullRowsStrong;
					serviceQuery.FilterResults = mobiusQuery.FilterResults;
					serviceQuery.GroupSalts = mobiusQuery.GroupSalts;
					serviceQuery.KeyCriteria = mobiusQuery.KeyCriteria;
					serviceQuery.KeyCriteriaDisplay = mobiusQuery.KeyCriteriaDisplay;
					serviceQuery.KeySortOrder = mobiusQuery.KeySortOrder;
					serviceQuery.LogicType = (Mobius.Services.Types.QueryLogicType)((int)mobiusQuery.LogicType);
					serviceQuery.MinimizeDbLinkUse = mobiusQuery.MinimizeDbLinkUse;
					serviceQuery.Mode = (Mobius.Services.Types.QueryMode)Enum.Parse(typeof(Mobius.Services.Types.QueryMode), mobiusQuery.Mode.ToString());
					serviceQuery.Multitable = mobiusQuery.Multitable;
					serviceQuery.OldMode = (Mobius.Services.Types.QueryMode)Enum.Parse(typeof(Mobius.Services.Types.QueryMode), mobiusQuery.OldMode.ToString());
					serviceQuery.Preview = mobiusQuery.Preview;
					serviceQuery.PrintMargins = mobiusQuery.PrintMargins;
					serviceQuery.PrintOrientation = (Mobius.Services.Types.Orientation)((int)mobiusQuery.PrintOrientation);
					serviceQuery.PrintScale = mobiusQuery.PrintScale;
					serviceQuery.RepeatReport = mobiusQuery.RepeatReport;
					serviceQuery.ResultKeys = mobiusQuery.ResultKeys;
					serviceQuery.RunQueryWhenOpened = mobiusQuery.RunQueryWhenOpened;
					serviceQuery.SerializeMetaTables = mobiusQuery.SerializeMetaTablesWithQuery;
					serviceQuery.ShowCondFormatLabels = mobiusQuery.ShowCondFormatLabels;
					serviceQuery.ShowGridCheckBoxes = mobiusQuery.ShowGridCheckBoxes;
					serviceQuery.ShowStereoComments = mobiusQuery.ShowStereoComments;
					serviceQuery.SingleStepExecution = mobiusQuery.SingleStepExecution;
					serviceQuery.SmallSsqMaxAtoms = mobiusQuery.SmallSsqMaxAtoms;
					serviceQuery.StatDisplayFormat = (int)mobiusQuery.StatDisplayFormat;
					serviceQuery.Timeout = mobiusQuery.Timeout;
					serviceQuery.UseResultKeys = mobiusQuery.UseResultKeys;
					serviceQuery.ViewScale = mobiusQuery.ViewScale;
					serviceQuery.InaccessibleData = mobiusQuery.InaccessableData;
					//serviceQuery.MobileDefault = mobiusQuery.MobileDefault;
					serviceQuery.Mobile = mobiusQuery.Mobile;

					//copy referenced objects (update references?)
					serviceQuery.CurrentTable = ConvertMobiusQueryTableToServiceQueryTable(mobiusQuery.CurrentTable, context);
					serviceQuery.Tables = ConvertMobiusQueryTablesToServiceQueryTables(mobiusQuery.Tables, context);
					serviceQuery.UserObjectNode = ConvertMobiusUserObjectToServiceUserObjectNode(mobiusQuery.UserObject, context);
				}
			}
			return serviceQuery;
		}

		private List<Mobius.Services.Types.QueryTable> ConvertMobiusQueryTablesToServiceQueryTables(
				List<Mobius.Data.QueryTable> mobiusQueryTables, TypeConversionContext context)
		{
			List<Mobius.Services.Types.QueryTable> serviceQueryTables = null;
			if (mobiusQueryTables != null)
			{
				//since we don't maintain a cache of these, no way to shortcut out at this level
				// so ignore the bread crumbs

				serviceQueryTables = new List<Mobius.Services.Types.QueryTable>(mobiusQueryTables.Count);
				foreach (Mobius.Data.QueryTable mobiusQueryTable in mobiusQueryTables)
				{
					serviceQueryTables.Add(ConvertMobiusQueryTableToServiceQueryTable(mobiusQueryTable, context));
				}
			}
			return serviceQueryTables;
		}

		private Dictionary<Mobius.Data.QueryTable, Mobius.Services.Types.QueryTable> knownServiceQueryTables =
				new Dictionary<Mobius.Data.QueryTable, Mobius.Services.Types.QueryTable>();
		private Mobius.Services.Types.QueryTable ConvertMobiusQueryTableToServiceQueryTable(
				Mobius.Data.QueryTable mobiusQueryTable, TypeConversionContext context)
		{
			Mobius.Services.Types.QueryTable serviceQueryTable = null;
			if (mobiusQueryTable != null)
			{
				//Is this object conversion already "in the stack"?
				List<BreadCrumb> breadCrumbs = context.BreadCrumbs;
				//Add it if not
				BreadCrumb breadCrumb = new BreadCrumb(MethodBase.GetCurrentMethod().Name, mobiusQueryTable);
				bool alreadyInProgress = true;
				if (!breadCrumbs.ContainsEquivalentBreadCrumb(breadCrumb))
				{
					alreadyInProgress = false;
					breadCrumbs.Add(breadCrumb);
				}

				if (alreadyInProgress)
				{
					//should already be a known object
					serviceQueryTable = (Mobius.Services.Types.QueryTable)context.KnownObjects[mobiusQueryTable];
				}
				else
				{
					lock (knownServiceQueryTables)
					{
						if (knownServiceQueryTables.ContainsKey(mobiusQueryTable))
						{
							serviceQueryTable = knownServiceQueryTables[mobiusQueryTable];
						}
						else
						{
							serviceQueryTable = new Mobius.Services.Types.QueryTable();
							knownServiceQueryTables.Add(mobiusQueryTable, serviceQueryTable);
						}
					}
					context.KnownObjects.Add(mobiusQueryTable, serviceQueryTable);

					//copy values
					serviceQueryTable.Alias = mobiusQueryTable.Alias;
					serviceQueryTable.AllowColumnMerging = mobiusQueryTable.AllowColumnMerging;
					serviceQueryTable.HeaderBackgroundColor =
							((mobiusQueryTable.HeaderBackgroundColor == System.Drawing.Color.Empty)
							 ? null : new Mobius.Services.Types.Color(mobiusQueryTable.HeaderBackgroundColor));
					serviceQueryTable.Label = mobiusQueryTable.Label;
					serviceQueryTable.VoPosition = mobiusQueryTable.VoPosition;

					//update references (and update referenced objects?)
					serviceQueryTable.MetaTable = ConvertMobiusMetaTableToServiceMetaTable(mobiusQueryTable.MetaTable, context);
					serviceQueryTable.Query = ConvertMobiusQueryToServiceQuery(mobiusQueryTable.Query, context);
					serviceQueryTable.QueryColumns = ConvertMobiusQueryColumnsToServiceQueryColumns(mobiusQueryTable.QueryColumns, context);
				}
			}
			return serviceQueryTable;
		}

		private List<Mobius.Services.Types.QueryColumn> ConvertMobiusQueryColumnsToServiceQueryColumns(
				List<Mobius.Data.QueryColumn> mobiusQueryColumns, TypeConversionContext context)
		{
			List<Mobius.Services.Types.QueryColumn> serviceQueryColumns = null;
			if (mobiusQueryColumns != null)
			{
				//since we don't maintain a cache of these, no way to shortcut out at this level
				// so ignore the bread crumbs

				serviceQueryColumns = new List<Mobius.Services.Types.QueryColumn>(mobiusQueryColumns.Count);
				foreach (Mobius.Data.QueryColumn mobiusQueryColumn in mobiusQueryColumns)
				{
					serviceQueryColumns.Add(ConvertMobiusQueryColumnToServiceQueryColumn(mobiusQueryColumn, context));
				}
			}
			return serviceQueryColumns;
		}

		Dictionary<Mobius.Data.QueryColumn, Mobius.Services.Types.QueryColumn> knownServiceQueryColumns =
				new Dictionary<Mobius.Data.QueryColumn, Mobius.Services.Types.QueryColumn>();
		private Mobius.Services.Types.QueryColumn ConvertMobiusQueryColumnToServiceQueryColumn(
				Mobius.Data.QueryColumn mobiusQueryColumn, TypeConversionContext context)
		{
			Mobius.Services.Types.QueryColumn serviceQueryColumn = null;
			if (mobiusQueryColumn != null)
			{
				//Is this object conversion already "in the stack"?
				List<BreadCrumb> breadCrumbs = context.BreadCrumbs;
				//Add it if not
				BreadCrumb breadCrumb = new BreadCrumb(MethodBase.GetCurrentMethod().Name, mobiusQueryColumn);
				bool alreadyInProgress = true;
				if (!breadCrumbs.ContainsEquivalentBreadCrumb(breadCrumb))
				{
					alreadyInProgress = false;
					breadCrumbs.Add(breadCrumb);
				}

				if (alreadyInProgress)
				{
					//should already be a known object
					serviceQueryColumn = (Mobius.Services.Types.QueryColumn)context.KnownObjects[mobiusQueryColumn];
				}
				else
				{
					lock (knownServiceQueryColumns)
					{
						if (knownServiceQueryColumns.ContainsKey(mobiusQueryColumn))
						{
							serviceQueryColumn = knownServiceQueryColumns[mobiusQueryColumn];
						}
						else
						{
							serviceQueryColumn = new Mobius.Services.Types.QueryColumn();
							knownServiceQueryColumns.Add(mobiusQueryColumn, serviceQueryColumn);
						}
					}
					context.KnownObjects.Add(mobiusQueryColumn, serviceQueryColumn);

					//copy values
					serviceQueryColumn.Criteria = mobiusQueryColumn.Criteria;
					serviceQueryColumn.CriteriaDisplay = mobiusQueryColumn.CriteriaDisplay;
					serviceQueryColumn.Decimals = mobiusQueryColumn.Decimals;
					serviceQueryColumn.DisplayFormat = (Mobius.Services.Types.ColumnFormatEnum)((int)mobiusQueryColumn.DisplayFormat);
					serviceQueryColumn.DisplayWidth = mobiusQueryColumn.DisplayWidth;
					serviceQueryColumn.FilterRetrieval = mobiusQueryColumn.FilterRetrieval;
					serviceQueryColumn.FilterSearch = mobiusQueryColumn.FilterSearch;
					//serviceQueryColumn.Aggregation = mobiusQueryColumn.AggregationType;
					serviceQueryColumn.Hidden = mobiusQueryColumn.Hidden;
					serviceQueryColumn.HorizontalAlignment = (Mobius.Services.Types.HorizontalAlignmentEx)((int)mobiusQueryColumn.HorizontalAlignment);
					serviceQueryColumn.Label = mobiusQueryColumn.Label;
					serviceQueryColumn.LabelImage = mobiusQueryColumn.LabelImage;
					serviceQueryColumn.Merge = mobiusQueryColumn.Merge;
					//serviceQueryColumn.MetaColumnIdx = mobiusQueryColumn.TempInt;
					serviceQueryColumn.MolFile = mobiusQueryColumn.MolString;
					serviceQueryColumn.SecondaryCriteria = mobiusQueryColumn.SecondaryCriteria;
					serviceQueryColumn.SecondaryCriteriaDisplay = mobiusQueryColumn.SecondaryCriteriaDisplay;
					serviceQueryColumn.SecondaryFilterType = (Mobius.Services.Types.FilterType)((int)mobiusQueryColumn.SecondaryFilterType);
					serviceQueryColumn.Selected = mobiusQueryColumn.Selected;
					serviceQueryColumn.ShowInFilterPanel = mobiusQueryColumn.ShowInFilterPanel;
					serviceQueryColumn.ShowOnCriteriaForm = mobiusQueryColumn.ShowOnCriteriaForm;
					serviceQueryColumn.SortOrder = mobiusQueryColumn.SortOrder;
					serviceQueryColumn.VerticalAlignment = (Mobius.Services.Types.VerticalAlignmentEx)((int)mobiusQueryColumn.VerticalAlignment);
					serviceQueryColumn.VoPosition = mobiusQueryColumn.VoPosition;

					//statics
					Mobius.Services.Types.QueryColumn.SessionDefaultHAlignment =
							(Mobius.Services.Types.HorizontalAlignmentEx)((int)Mobius.Data.MetaColumn.SessionDefaultHAlignment);
					Mobius.Services.Types.QueryColumn.SessionDefaultVAlignment =
							(Mobius.Services.Types.VerticalAlignmentEx)((int)Mobius.Data.MetaColumn.SessionDefaultVAlignment);

					//update references (and referenced objects?)
					serviceQueryColumn.CondFormat = ConvertMobiusCondFormatToServiceCondFormat(mobiusQueryColumn.CondFormat, context);
					serviceQueryColumn.MetaColumn = ConvertMobiusMetaColumnToServiceMetaColumn(mobiusQueryColumn.MetaColumn, context);
					serviceQueryColumn.QueryTable = ConvertMobiusQueryTableToServiceQueryTable(mobiusQueryColumn.QueryTable, context);
				}
			}
			return serviceQueryColumn;
		}


		private Dictionary<int, Mobius.Data.Query> knownMobiusQueries =
				new Dictionary<int, Mobius.Data.Query>();
		private Mobius.Data.Query ConvertServiceQueryToMobiusQuery(
				Mobius.Services.Types.Query serviceQuery, TypeConversionContext context)
		{
			Mobius.Data.Query mobiusQuery = null;
			if (serviceQuery != null)
			{
				//Is this object conversion already "in the stack"?
				List<BreadCrumb> breadCrumbs = context.BreadCrumbs;
				//Add it if not
				BreadCrumb breadCrumb = new BreadCrumb(MethodBase.GetCurrentMethod().Name, serviceQuery);
				bool alreadyInProgress = true;
				if (!breadCrumbs.ContainsEquivalentBreadCrumb(breadCrumb))
				{
					alreadyInProgress = false;
					breadCrumbs.Add(breadCrumb);
				}

				if (alreadyInProgress)
				{
					//should already be a known object
					mobiusQuery = (Mobius.Data.Query)context.KnownObjects[serviceQuery];
				}
				else
				{
					lock (knownMobiusQueries)
					{
						if (knownMobiusQueries.ContainsKey(serviceQuery.Id))
						{
							mobiusQuery = knownMobiusQueries[serviceQuery.Id];
						}
						else
						{
							mobiusQuery = new Mobius.Data.Query();
							knownMobiusQueries.Add(serviceQuery.Id, mobiusQuery);
						}
					}
					context.KnownObjects.Add(serviceQuery, mobiusQuery);

					//copy values
					mobiusQuery.InstanceId = serviceQuery.Id;
					mobiusQuery.AlertInterval = serviceQuery.AlertInterval;
					mobiusQuery.AlertQueryState = serviceQuery.AlertQueryState;
					mobiusQuery.AllowColumnMerging = serviceQuery.AllowColumnMerging;
					mobiusQuery.PrefetchImages = serviceQuery.PrefetchImages;
					mobiusQuery.BrowseSavedResultsUponOpen = serviceQuery.BrowseExistingResultsWhenOpened;
					mobiusQuery.ComplexCriteria = serviceQuery.ComplexCriteria;
					mobiusQuery.FilterNullRowsStrong = serviceQuery.FilterNullRowsStrong;
					mobiusQuery.FilterResults = serviceQuery.FilterResults;
					mobiusQuery.GroupSalts = serviceQuery.GroupSalts;
					mobiusQuery.KeyCriteria = serviceQuery.KeyCriteria;
					mobiusQuery.KeyCriteriaDisplay = serviceQuery.KeyCriteriaDisplay;
					mobiusQuery.KeySortOrder = serviceQuery.KeySortOrder;
					mobiusQuery.LogicType = (Mobius.Data.QueryLogicType)((int)serviceQuery.LogicType);
					mobiusQuery.MinimizeDbLinkUse = serviceQuery.MinimizeDbLinkUse;
					mobiusQuery.Mode = (Mobius.Data.QueryMode)Enum.Parse(typeof(Mobius.Data.QueryMode), serviceQuery.Mode.ToString());
					mobiusQuery.Multitable = serviceQuery.Multitable;
					mobiusQuery.OldMode = (Mobius.Data.QueryMode)Enum.Parse(typeof(Mobius.Data.QueryMode), serviceQuery.OldMode.ToString());
					mobiusQuery.Preview = serviceQuery.Preview;
					mobiusQuery.PrintMargins = serviceQuery.PrintMargins;
					mobiusQuery.PrintOrientation = (System.Windows.Forms.Orientation)((int)serviceQuery.PrintOrientation);
					mobiusQuery.PrintScale = serviceQuery.PrintScale;
					mobiusQuery.RepeatReport = serviceQuery.RepeatReport;
					mobiusQuery.ResultKeys = serviceQuery.ResultKeys;
					mobiusQuery.RunQueryWhenOpened = serviceQuery.RunQueryWhenOpened;
					mobiusQuery.SerializeMetaTablesWithQuery = serviceQuery.SerializeMetaTables;
					mobiusQuery.ShowCondFormatLabels = serviceQuery.ShowCondFormatLabels;
					mobiusQuery.ShowGridCheckBoxes = serviceQuery.ShowGridCheckBoxes;
					mobiusQuery.ShowStereoComments = serviceQuery.ShowStereoComments;
					mobiusQuery.SingleStepExecution = serviceQuery.SingleStepExecution;
					mobiusQuery.SmallSsqMaxAtoms = serviceQuery.SmallSsqMaxAtoms;
					mobiusQuery.StatDisplayFormat = (Mobius.Data.QnfEnum)serviceQuery.StatDisplayFormat;
					mobiusQuery.Timeout = serviceQuery.Timeout;
					mobiusQuery.UseResultKeys = serviceQuery.UseResultKeys;
					mobiusQuery.ViewScale = serviceQuery.ViewScale;
					mobiusQuery.InaccessableData = serviceQuery.InaccessibleData;
					//mobiusQuery.MobileDefault = serviceQuery.MobileDefault;
					mobiusQuery.Mobile = serviceQuery.Mobile;

					//updates references (and update referenced objects?)
					mobiusQuery.CurrentTable = ConvertServiceQueryTableToMobiusQueryTable(serviceQuery.CurrentTable, context);
					mobiusQuery.Tables = ConvertServiceQueryTablesToMobiusQueryTables(serviceQuery.Tables, context);
					mobiusQuery.UserObject = ConvertServiceUserObjectNodeToMobiusUserObject(serviceQuery.UserObjectNode, context);
				}
			}
			return mobiusQuery;
		}

		private Dictionary<Mobius.Services.Types.QueryTable, Mobius.Data.QueryTable> knownMobiusQueryTables =
				new Dictionary<Mobius.Services.Types.QueryTable, Mobius.Data.QueryTable>();
		private Mobius.Data.QueryTable ConvertServiceQueryTableToMobiusQueryTable(
				Mobius.Services.Types.QueryTable serviceQueryTable, TypeConversionContext context)
		{
			Mobius.Data.QueryTable mobiusQueryTable = null;
			if (serviceQueryTable != null)
			{
				//Is this object conversion already "in the stack"?
				List<BreadCrumb> breadCrumbs = context.BreadCrumbs;
				//Add it if not
				BreadCrumb breadCrumb = new BreadCrumb(MethodBase.GetCurrentMethod().Name, serviceQueryTable);
				bool alreadyInProgress = true;
				if (!breadCrumbs.ContainsEquivalentBreadCrumb(breadCrumb))
				{
					alreadyInProgress = false;
					breadCrumbs.Add(breadCrumb);
				}

				if (alreadyInProgress)
				{
					//should already be a known object
					mobiusQueryTable = (Mobius.Data.QueryTable)context.KnownObjects[serviceQueryTable];
				}
				else
				{
					lock (knownMobiusQueryTables)
					{
						if (knownMobiusQueryTables.ContainsKey(serviceQueryTable))
						{
							mobiusQueryTable = knownMobiusQueryTables[serviceQueryTable];
						}
						else
						{
							mobiusQueryTable = new Mobius.Data.QueryTable();
							knownMobiusQueryTables.Add(serviceQueryTable, mobiusQueryTable);
						}
					}
					context.KnownObjects.Add(serviceQueryTable, mobiusQueryTable);

					//copy values
					mobiusQueryTable.Alias = serviceQueryTable.Alias;
					mobiusQueryTable.AllowColumnMerging = serviceQueryTable.AllowColumnMerging;
					mobiusQueryTable.HeaderBackgroundColor = (serviceQueryTable.HeaderBackgroundColor == null)
							? System.Drawing.Color.Empty :
								System.Drawing.Color.FromArgb(
										serviceQueryTable.HeaderBackgroundColor.A, serviceQueryTable.HeaderBackgroundColor.R,
										serviceQueryTable.HeaderBackgroundColor.G, serviceQueryTable.HeaderBackgroundColor.B);
					mobiusQueryTable.Label = serviceQueryTable.Label;
					mobiusQueryTable.VoPosition = serviceQueryTable.VoPosition;

					//update references (and referenced objects?)
					mobiusQueryTable.MetaTable = ConvertServiceMetaTableToMobiusMetaTable(serviceQueryTable.MetaTable, context);
					mobiusQueryTable.Query = ConvertServiceQueryToMobiusQuery(serviceQueryTable.Query, context);
					mobiusQueryTable.QueryColumns = ConvertServiceQueryColumnsToMobiusQueryColumns(serviceQueryTable.QueryColumns, context);
				}
			}
			return mobiusQueryTable;
		}

		private List<Mobius.Data.QueryColumn> ConvertServiceQueryColumnsToMobiusQueryColumns(
				List<Mobius.Services.Types.QueryColumn> serviceQueryColumns, TypeConversionContext context)
		{
			List<Mobius.Data.QueryColumn> mobiusQueryColumns = null;
			if (serviceQueryColumns != null)
			{
				//since we don't maintain a cache of these, no way to shortcut out at this level
				// so ignore the bread crumbs

				mobiusQueryColumns = new List<Mobius.Data.QueryColumn>(serviceQueryColumns.Count);
				foreach (Mobius.Services.Types.QueryColumn serviceQueryColumn in serviceQueryColumns)
				{
					mobiusQueryColumns.Add(ConvertServiceQueryColumnToMobiusQueryColumn(serviceQueryColumn, context));
				}
			}
			return mobiusQueryColumns;
		}

		Dictionary<Mobius.Services.Types.QueryColumn, Mobius.Data.QueryColumn> knownMobiusQueryColumns =
				new Dictionary<Mobius.Services.Types.QueryColumn, Mobius.Data.QueryColumn>();
		private Mobius.Data.QueryColumn ConvertServiceQueryColumnToMobiusQueryColumn(
				Mobius.Services.Types.QueryColumn serviceQueryColumn, TypeConversionContext context)
		{
			Mobius.Data.QueryColumn mobiusQueryColumn = null;
			if (serviceQueryColumn != null)
			{
				//Is this object conversion already "in the stack"?
				List<BreadCrumb> breadCrumbs = context.BreadCrumbs;
				//Add it if not
				BreadCrumb breadCrumb = new BreadCrumb(MethodBase.GetCurrentMethod().Name, serviceQueryColumn);
				bool alreadyInProgress = true;
				if (!breadCrumbs.ContainsEquivalentBreadCrumb(breadCrumb))
				{
					alreadyInProgress = false;
					breadCrumbs.Add(breadCrumb);
				}

				if (alreadyInProgress)
				{
					//should already be a known object
					mobiusQueryColumn = (Mobius.Data.QueryColumn)context.KnownObjects[serviceQueryColumn];
				}
				else
				{
					lock (knownMobiusQueryColumns)
					{
						if (knownMobiusQueryColumns.ContainsKey(serviceQueryColumn))
						{
							mobiusQueryColumn = knownMobiusQueryColumns[serviceQueryColumn];
						}
						else
						{
							mobiusQueryColumn = new Mobius.Data.QueryColumn();
							knownMobiusQueryColumns.Add(serviceQueryColumn, mobiusQueryColumn);
						}
					}
					context.KnownObjects.Add(serviceQueryColumn, mobiusQueryColumn);

					//copy values
					mobiusQueryColumn.Criteria = serviceQueryColumn.Criteria;
					mobiusQueryColumn.CriteriaDisplay = serviceQueryColumn.CriteriaDisplay;
					mobiusQueryColumn.Decimals = serviceQueryColumn.Decimals;
					mobiusQueryColumn.DisplayFormat = (Mobius.Data.ColumnFormatEnum)((int)serviceQueryColumn.DisplayFormat);
					mobiusQueryColumn.DisplayWidth = serviceQueryColumn.DisplayWidth;
					mobiusQueryColumn.FilterRetrieval = serviceQueryColumn.FilterRetrieval;
					mobiusQueryColumn.FilterSearch = serviceQueryColumn.FilterSearch;
					//mobiusQueryColumn.AggregationType = serviceQueryColumn.Aggregation;
					mobiusQueryColumn.Hidden = serviceQueryColumn.Hidden;
					mobiusQueryColumn.HorizontalAlignment = (Mobius.Data.HorizontalAlignmentEx)((int)serviceQueryColumn.HorizontalAlignment);
					mobiusQueryColumn.Label = serviceQueryColumn.Label;
					mobiusQueryColumn.LabelImage = serviceQueryColumn.LabelImage;
					mobiusQueryColumn.Merge = serviceQueryColumn.Merge;
					//mobiusQueryColumn.TempInt = serviceQueryColumn.MetaColumnIdx;
					mobiusQueryColumn.MolString = serviceQueryColumn.MolFile;
					mobiusQueryColumn.SecondaryCriteria = serviceQueryColumn.SecondaryCriteria;
					mobiusQueryColumn.SecondaryCriteriaDisplay = serviceQueryColumn.SecondaryCriteriaDisplay;
					mobiusQueryColumn.SecondaryFilterType = (Mobius.Data.FilterType)((int)serviceQueryColumn.SecondaryFilterType);
					mobiusQueryColumn.Selected = serviceQueryColumn.Selected;
					mobiusQueryColumn.ShowInFilterPanel = serviceQueryColumn.ShowInFilterPanel;
					mobiusQueryColumn.ShowOnCriteriaForm = serviceQueryColumn.ShowOnCriteriaForm;
					mobiusQueryColumn.SortOrder = serviceQueryColumn.SortOrder;
					mobiusQueryColumn.VerticalAlignment = (Mobius.Data.VerticalAlignmentEx)((int)serviceQueryColumn.VerticalAlignment);
					mobiusQueryColumn.VoPosition = serviceQueryColumn.VoPosition;

					//statics
					Mobius.Data.MetaColumn.SessionDefaultHAlignment =
							(Mobius.Data.HorizontalAlignmentEx)((int)Mobius.Services.Types.QueryColumn.SessionDefaultHAlignment);
					Mobius.Data.MetaColumn.SessionDefaultVAlignment =
							(Mobius.Data.VerticalAlignmentEx)((int)Mobius.Services.Types.QueryColumn.SessionDefaultVAlignment);

					//update references (and referenced objects?)
					mobiusQueryColumn.CondFormat = ConvertServiceCondFormatToMobiusCondFormat(serviceQueryColumn.CondFormat, context);
					mobiusQueryColumn.MetaColumn = ConvertServiceMetaColumnToMobiusMetaColumn(serviceQueryColumn.MetaColumn, context);
					mobiusQueryColumn.QueryTable = ConvertServiceQueryTableToMobiusQueryTable(serviceQueryColumn.QueryTable, context);
				}
			}
			return mobiusQueryColumn;
		}

		private List<Mobius.Data.QueryTable> ConvertServiceQueryTablesToMobiusQueryTables(
				List<Mobius.Services.Types.QueryTable> serviceQueryTables, TypeConversionContext context)
		{
			List<Mobius.Data.QueryTable> mobiusQueryTables = null;
			if (serviceQueryTables != null)
			{
				//since we don't maintain a cache of these, no way to shortcut out at this level
				// so ignore the bread crumbs

				mobiusQueryTables = new List<Mobius.Data.QueryTable>(serviceQueryTables.Count);
				foreach (Mobius.Services.Types.QueryTable serviceQueryTable in serviceQueryTables)
				{
					mobiusQueryTables.Add(ConvertServiceQueryTableToMobiusQueryTable(serviceQueryTable, context));
				}
			}
			return mobiusQueryTables;
		}


		private void CopyMobiusDataTypeFormattingToServiceDataType(
				Mobius.Data.MobiusDataType mobiusObject, Mobius.Services.Types.FormattedDataType serviceObject)
		{
			serviceObject.BackColor = new Mobius.Services.Types.Color(mobiusObject.BackColor);
			serviceObject.DbLink = mobiusObject.DbLink;
			serviceObject.Filtered = mobiusObject.Filtered;
			serviceObject.ForeColor = new Mobius.Services.Types.Color(mobiusObject.ForeColor);
			serviceObject.FormattedBitmap = ((mobiusObject.FormattedBitmap == null) ? null : new Mobius.Services.Types.Bitmap(mobiusObject.FormattedBitmap));
			serviceObject.FormattedText = mobiusObject.FormattedText;
			serviceObject.Hyperlink = mobiusObject.Hyperlink;
			serviceObject.IsNull = mobiusObject.IsNull;
			serviceObject.Modified = mobiusObject.Modified;
		}

		private void CopyServiceDataTypeFormattingToMobiusDataType(
				Mobius.Services.Types.FormattedDataType serviceObject, Mobius.Data.MobiusDataType mobiusObject)
		{
			mobiusObject.BackColor = (System.Drawing.Color)serviceObject.BackColor;
			mobiusObject.DbLink = serviceObject.DbLink;
			mobiusObject.Filtered = serviceObject.Filtered;
			mobiusObject.ForeColor = (System.Drawing.Color)serviceObject.ForeColor;
			mobiusObject.FormattedBitmap = (System.Drawing.Bitmap)serviceObject.FormattedBitmap;
			mobiusObject.FormattedText = serviceObject.FormattedText;
			mobiusObject.Hyperlink = serviceObject.Hyperlink;
			mobiusObject.Modified = serviceObject.Modified;
		}

		private Mobius.Services.Types.QualifiedNumber ConvertMobiusQualifiedNumberToServiceQualifiedNumber(
				Mobius.Data.QualifiedNumber mobiusQualifiedNumber, TypeConversionContext context)
		{
			Mobius.Services.Types.QualifiedNumber serviceQualifiedNumber = null;
			if (mobiusQualifiedNumber != null)
			{
				//since we don't maintain a cache of these, no way to shortcut out at this level
				// so ignore the bread crumbs

				serviceQualifiedNumber = new Mobius.Services.Types.QualifiedNumber();
				CopyMobiusDataTypeFormattingToServiceDataType(mobiusQualifiedNumber, serviceQualifiedNumber);

				serviceQualifiedNumber.NumberValue = mobiusQualifiedNumber.NumberValue;
				//serviceQualifiedNumber.NumberValueHigh = mobiusQualifiedNumber.NumberValueHigh;
				serviceQualifiedNumber.NValue = mobiusQualifiedNumber.NValue;
				serviceQualifiedNumber.NValueTested = mobiusQualifiedNumber.NValueTested;
				serviceQualifiedNumber.Qualifier = mobiusQualifiedNumber.Qualifier;
				serviceQualifiedNumber.StandardDeviation = mobiusQualifiedNumber.StandardDeviation;
				serviceQualifiedNumber.StandardError = mobiusQualifiedNumber.StandardError;
				serviceQualifiedNumber.TextValue = mobiusQualifiedNumber.TextValue;
				//serviceQualifiedNumber.Units = mobiusQualifiedNumber.Units;
			}
			return serviceQualifiedNumber;
		}

		private Mobius.Data.QualifiedNumber ConvertServiceQualifiedNumberToMobiusQualifiedNumber(
				Mobius.Services.Types.QualifiedNumber serviceQualifiedNumber, TypeConversionContext context)
		{
			Mobius.Data.QualifiedNumber mobiusQualifiedNumber = null;
			if (serviceQualifiedNumber != null)
			{
				//since we don't maintain a cache of these, no way to shortcut out at this level
				// so ignore the bread crumbs

				mobiusQualifiedNumber = new Mobius.Data.QualifiedNumber();
				CopyServiceDataTypeFormattingToMobiusDataType(serviceQualifiedNumber, mobiusQualifiedNumber);

				mobiusQualifiedNumber.NumberValue = serviceQualifiedNumber.NumberValue;
				//mobiusQualifiedNumber.NumberValueHigh = serviceQualifiedNumber.NumberValueHigh;
				mobiusQualifiedNumber.NValue = serviceQualifiedNumber.NValue;
				mobiusQualifiedNumber.NValueTested = serviceQualifiedNumber.NValueTested;
				mobiusQualifiedNumber.Qualifier = serviceQualifiedNumber.Qualifier;
				mobiusQualifiedNumber.StandardDeviation = serviceQualifiedNumber.StandardDeviation;
				mobiusQualifiedNumber.StandardError = serviceQualifiedNumber.StandardError;
				mobiusQualifiedNumber.TextValue = serviceQualifiedNumber.TextValue;
				//mobiusQualifiedNumber.Units = serviceQualifiedNumber.Units;
			}
			return mobiusQualifiedNumber;
		}



		private Mobius.Services.Types.ChemicalStructure ConvertMobiusChemicalStructureToServiceChemicalStructure(
				Mobius.Data.MoleculeMx mobiusChemicalStructure, TypeConversionContext context)
		{
			Mobius.Services.Types.ChemicalStructure serviceChemicalStructure = null;
			if (mobiusChemicalStructure != null)
			{
				//since we don't maintain a cache of these, no way to shortcut out at this level
				// so ignore the bread crumbs

				serviceChemicalStructure = new Mobius.Services.Types.ChemicalStructure();
				CopyMobiusDataTypeFormattingToServiceDataType(mobiusChemicalStructure, serviceChemicalStructure);

				serviceChemicalStructure.Id = mobiusChemicalStructure.Id;
				serviceChemicalStructure.Caption = mobiusChemicalStructure.Caption;

				//need to ensure that the structure is in a string format so that we know it can pass on the wire

				if (mobiusChemicalStructure.IsChemStructureFormat)
				{
					if (mobiusChemicalStructure.PrimaryValue != null && mobiusChemicalStructure.PrimaryValue.GetType() == typeof(string))
					{
						serviceChemicalStructure.Type = (Mobius.Services.Types.StructureFormat)((int)mobiusChemicalStructure.PrimaryFormat);
						serviceChemicalStructure.Value = mobiusChemicalStructure.PrimaryValue.ToString();
					}
					else
					{
						//convert to a wire-friendly format
						serviceChemicalStructure.Type = Mobius.Services.Types.StructureFormat.MolFile;
						serviceChemicalStructure.Value = mobiusChemicalStructure.GetMolfileString();
					}
				}

				else if (mobiusChemicalStructure.IsBiopolymerFormat) // probably Helm, see if molfile is available
				{
					serviceChemicalStructure.Type = Mobius.Services.Types.StructureFormat.MolFile;
					serviceChemicalStructure.Value = mobiusChemicalStructure.GetMolfileString();
				}
			}

			return serviceChemicalStructure;
		}

		private Mobius.Data.MoleculeMx ConvertServiceChemicalStructureToMobiusChemicalStructure(
				Mobius.Services.Types.ChemicalStructure serviceChemicalStructure, TypeConversionContext context)
		{
			Mobius.Data.MoleculeMx molMx = null;
			if (serviceChemicalStructure != null)
			{
				//since we don't maintain a cache of these, no way to shortcut out at this level
				// so ignore the bread crumbs

				molMx = new Mobius.Data.MoleculeMx();
				CopyServiceDataTypeFormattingToMobiusDataType(serviceChemicalStructure, molMx);

				molMx.Id = serviceChemicalStructure.Id;
				molMx.Caption = serviceChemicalStructure.Caption;

				//the structure is ALWAYS in a string format so we know that it can pass on the wire

				molMx.SetPrimaryTypeAndValue(
					(Mobius.Data.MoleculeFormat)((int)serviceChemicalStructure.Type),
					serviceChemicalStructure.Value);

			}
			return molMx;
		}

		private Mobius.Services.Types.CompoundId ConvertMobiusCompoundIdToServiceCompoundId(
				Mobius.Data.CompoundId mobiusCompoundId, TypeConversionContext context)
		{
			Mobius.Services.Types.CompoundId serviceCompoundId = null;
			if (mobiusCompoundId != null)
			{
				//since we don't maintain a cache of these, no way to shortcut out at this level
				// so ignore the bread crumbs

				serviceCompoundId = new Mobius.Services.Types.CompoundId();
				CopyMobiusDataTypeFormattingToServiceDataType(mobiusCompoundId, serviceCompoundId);

				serviceCompoundId.Value = mobiusCompoundId.Value;
			}
			return serviceCompoundId;
		}

		private Mobius.Data.CompoundId ConvertServiceCompoundIdToMobiusCompoundId(
				Mobius.Services.Types.CompoundId serviceCompoundId, TypeConversionContext context)
		{
			Mobius.Data.CompoundId mobiusCompoundId = null;
			if (serviceCompoundId != null)
			{
				//since we don't maintain a cache of these, no way to shortcut out at this level
				// so ignore the bread crumbs

				mobiusCompoundId = new Mobius.Data.CompoundId();
				CopyServiceDataTypeFormattingToMobiusDataType(serviceCompoundId, mobiusCompoundId);

				mobiusCompoundId.Value = serviceCompoundId.Value;
			}
			return mobiusCompoundId;
		}



		private Mobius.Services.Types.DateTimeEx ConvertMobiusDateTimeExToServiceDateTimeEx(
		Mobius.Data.DateTimeMx mobiusDateTimeEx, TypeConversionContext context)
		{
			Mobius.Services.Types.DateTimeEx serviceDateTimeEx = null;
			if (mobiusDateTimeEx != null)
			{
				//since we don't maintain a cache of these, no way to shortcut out at this level
				// so ignore the bread crumbs

				serviceDateTimeEx = new Mobius.Services.Types.DateTimeEx();
				CopyMobiusDataTypeFormattingToServiceDataType(mobiusDateTimeEx, serviceDateTimeEx);

				serviceDateTimeEx.Value = mobiusDateTimeEx.Value;
			}
			return serviceDateTimeEx;
		}

		private Mobius.Data.DateTimeMx ConvertServiceDateTimeExToMobiusDateTimeEx(
				Mobius.Services.Types.DateTimeEx serviceDateTimeEx, TypeConversionContext context)
		{
			Mobius.Data.DateTimeMx mobiusDateTimeEx = null;
			if (serviceDateTimeEx != null)
			{
				//since we don't maintain a cache of these, no way to shortcut out at this level
				// so ignore the bread crumbs

				mobiusDateTimeEx = new Mobius.Data.DateTimeMx();
				CopyServiceDataTypeFormattingToMobiusDataType(serviceDateTimeEx, mobiusDateTimeEx);

				mobiusDateTimeEx.Value = serviceDateTimeEx.Value;
			}
			return mobiusDateTimeEx;
		}



		private Mobius.Services.Types.StringEx ConvertMobiusStringExToServiceStringEx(
		Mobius.Data.StringMx mobiusStringEx, TypeConversionContext context)
		{
			Mobius.Services.Types.StringEx serviceStringEx = null;
			if (mobiusStringEx != null)
			{
				//since we don't maintain a cache of these, no way to shortcut out at this level
				// so ignore the bread crumbs

				serviceStringEx = new Mobius.Services.Types.StringEx();
				CopyMobiusDataTypeFormattingToServiceDataType(mobiusStringEx, serviceStringEx);

				serviceStringEx.Value = mobiusStringEx.Value;
			}
			return serviceStringEx;
		}

		private Mobius.Data.StringMx ConvertServiceStringExToMobiusStringEx(
				Mobius.Services.Types.StringEx serviceStringEx, TypeConversionContext context)
		{
			Mobius.Data.StringMx mobiusStringEx = null;
			if (serviceStringEx != null)
			{
				//since we don't maintain a cache of these, no way to shortcut out at this level
				// so ignore the bread crumbs

				mobiusStringEx = new Mobius.Data.StringMx();
				CopyServiceDataTypeFormattingToMobiusDataType(serviceStringEx, mobiusStringEx);

				mobiusStringEx.Value = serviceStringEx.Value;
			}
			return mobiusStringEx;
		}



		private Mobius.Services.Types.ImageEx ConvertMobiusImageExToServiceImageEx(
		Mobius.Data.ImageMx mobiusImageEx, TypeConversionContext context)
		{
			Mobius.Services.Types.ImageEx serviceImageEx = null;
			if (mobiusImageEx != null)
			{
				//since we don't maintain a cache of these, no way to shortcut out at this level
				// so ignore the bread crumbs

				serviceImageEx = new Mobius.Services.Types.ImageEx();
				CopyMobiusDataTypeFormattingToServiceDataType(mobiusImageEx, serviceImageEx);

				serviceImageEx.Value = new Mobius.Services.Types.Bitmap(mobiusImageEx.Value);
			}
			return serviceImageEx;
		}

		private Mobius.Data.ImageMx ConvertServiceImageExToMobiusImageEx(
				Mobius.Services.Types.ImageEx serviceImageEx, TypeConversionContext context)
		{
			Mobius.Data.ImageMx mobiusImageEx = null;
			if (serviceImageEx != null)
			{
				//since we don't maintain a cache of these, no way to shortcut out at this level
				// so ignore the bread crumbs

				mobiusImageEx = new Mobius.Data.ImageMx();
				CopyServiceDataTypeFormattingToMobiusDataType(serviceImageEx, mobiusImageEx);

				mobiusImageEx.Value = (System.Drawing.Bitmap)serviceImageEx.Value;
			}
			return mobiusImageEx;
		}



		private Mobius.Services.Types.NumberEx ConvertMobiusNumberExToServiceNumberEx(
		Mobius.Data.NumberMx mobiusNumberEx, TypeConversionContext context)
		{
			Mobius.Services.Types.NumberEx serviceNumberEx = null;
			if (mobiusNumberEx != null)
			{
				//since we don't maintain a cache of these, no way to shortcut out at this level
				// so ignore the bread crumbs

				serviceNumberEx = new Mobius.Services.Types.NumberEx();
				CopyMobiusDataTypeFormattingToServiceDataType(mobiusNumberEx, serviceNumberEx);

				serviceNumberEx.Value = mobiusNumberEx.Value;
			}
			return serviceNumberEx;
		}

		private Mobius.Data.NumberMx ConvertServiceNumberExToMobiusNumberEx(
				Mobius.Services.Types.NumberEx serviceNumberEx, TypeConversionContext context)
		{
			Mobius.Data.NumberMx mobiusNumberEx = null;
			if (serviceNumberEx != null)
			{
				//since we don't maintain a cache of these, no way to shortcut out at this level
				// so ignore the bread crumbs

				mobiusNumberEx = new Mobius.Data.NumberMx();
				CopyServiceDataTypeFormattingToMobiusDataType(serviceNumberEx, mobiusNumberEx);

				mobiusNumberEx.Value = serviceNumberEx.Value;
			}
			return mobiusNumberEx;
		}



		private Mobius.Services.Types.UserInfo ConvertMobiusUserInfoToServiceUserInfo(
				Mobius.ComOps.UserInfo mobiusUserInfo, TypeConversionContext context)
		{
			Mobius.Services.Types.UserInfo serviceUserInfo = null;
			if (mobiusUserInfo != null)
			{
				//since we don't maintain a cache of these, no way to shortcut out at this level
				// so ignore the bread crumbs

				serviceUserInfo = new Mobius.Services.Types.UserInfo();
				serviceUserInfo.Company = mobiusUserInfo.Company;
				serviceUserInfo.Department = mobiusUserInfo.Department;
				serviceUserInfo.EmailAddress = mobiusUserInfo.EmailAddress;
				serviceUserInfo.FirstName = mobiusUserInfo.FirstName;
				serviceUserInfo.LastName = mobiusUserInfo.LastName;
				serviceUserInfo.MiddleInitial = mobiusUserInfo.MiddleInitial;
				serviceUserInfo.Site = mobiusUserInfo.Site;
				serviceUserInfo.UserDomainName = mobiusUserInfo.UserDomainName;
				serviceUserInfo.UserName = mobiusUserInfo.UserName;
			}
			return serviceUserInfo;
		}

		private Mobius.ComOps.UserInfo ConvertServiceUserInfoToMobiusUserInfo(
				Mobius.Services.Types.UserInfo serviceUserInfo, TypeConversionContext context)
		{
			Mobius.ComOps.UserInfo mobiusUserInfo = null;
			if (serviceUserInfo != null)
			{
				//since we don't maintain a cache of these, no way to shortcut out at this level
				// so ignore the bread crumbs

				mobiusUserInfo = new Mobius.ComOps.UserInfo();
				mobiusUserInfo.Company = serviceUserInfo.Company;
				mobiusUserInfo.Department = serviceUserInfo.Department;
				mobiusUserInfo.EmailAddress = serviceUserInfo.EmailAddress;
				mobiusUserInfo.FirstName = serviceUserInfo.FirstName;
				mobiusUserInfo.LastName = serviceUserInfo.LastName;
				mobiusUserInfo.MiddleInitial = serviceUserInfo.MiddleInitial;
				mobiusUserInfo.Site = serviceUserInfo.Site;
				mobiusUserInfo.UserDomainName = serviceUserInfo.UserDomainName;
				mobiusUserInfo.UserName = serviceUserInfo.UserName;
			}
			return mobiusUserInfo;
		}




		private Mobius.Services.Types.UcdbDatabase ConvertMobiusUcdbDatabaseToServiceUcdbDatabase(
				Mobius.Data.UcdbDatabase mobiusUcdbDatabase, TypeConversionContext context)
		{
			Mobius.Services.Types.UcdbDatabase serviceUcdbDatabase = null;
			if (mobiusUcdbDatabase != null)
			{
				//since we don't maintain a cache of these, no way to shortcut out at this level
				// so ignore the bread crumbs

				serviceUcdbDatabase = new Mobius.Services.Types.UcdbDatabase();
				serviceUcdbDatabase.AllowDuplicateStructures = mobiusUcdbDatabase.AllowDuplicateStructures;
				serviceUcdbDatabase.CompoundCount = mobiusUcdbDatabase.CompoundCount;
				serviceUcdbDatabase.CompoundIdType = (Mobius.Services.Types.CompoundIdTypeEnum)((int)mobiusUcdbDatabase.CompoundIdType);
				serviceUcdbDatabase.CreationDate = mobiusUcdbDatabase.CreationDate;
				serviceUcdbDatabase.DatabaseId = mobiusUcdbDatabase.DatabaseId;
				serviceUcdbDatabase.Description = mobiusUcdbDatabase.Description;
				serviceUcdbDatabase.ModelCount = mobiusUcdbDatabase.ModelCount;
				serviceUcdbDatabase.Name = mobiusUcdbDatabase.Name;
				serviceUcdbDatabase.NameSpace = mobiusUcdbDatabase.NameSpace;
				serviceUcdbDatabase.OwnerUserName = mobiusUcdbDatabase.OwnerUserName;
				serviceUcdbDatabase.PendingCompoundCount = mobiusUcdbDatabase.PendingCompoundCount;
				serviceUcdbDatabase.PendingCompoundId = mobiusUcdbDatabase.PendingCompoundId;
				serviceUcdbDatabase.PendingStatus = (Mobius.Services.Types.UcdbWaitState)((int)mobiusUcdbDatabase.PendingStatus);
				serviceUcdbDatabase.PendingUpdateDate = mobiusUcdbDatabase.PendingUpdateDate;
				serviceUcdbDatabase.Public = mobiusUcdbDatabase.Public;
				serviceUcdbDatabase.RowState = (Mobius.Services.Types.UcdbRowState)((int)mobiusUcdbDatabase.RowState);
				serviceUcdbDatabase.StructureSearchSupported = mobiusUcdbDatabase.StructureSearchSupported;
				serviceUcdbDatabase.UpdateDate = mobiusUcdbDatabase.UpdateDate;

				serviceUcdbDatabase.Compounds = ConvertMobiusUcdbCompoundsToServiceUcdbCompounds(mobiusUcdbDatabase.Compounds, context);
				serviceUcdbDatabase.Models = ConvertMobiusUcdbModelsToServiceUcdbModels(mobiusUcdbDatabase.Models, context);
			}
			return serviceUcdbDatabase;
		}

		private Mobius.Data.UcdbDatabase ConvertServiceUcdbDatabaseToMobiusUcdbDatabase(
				Mobius.Services.Types.UcdbDatabase serviceUcdbDatabase, TypeConversionContext context)
		{
			Mobius.Data.UcdbDatabase mobiusUcdbDatabase = null;
			if (serviceUcdbDatabase != null)
			{
				//since we don't maintain a cache of these, no way to shortcut out at this level
				// so ignore the bread crumbs

				mobiusUcdbDatabase = new Mobius.Data.UcdbDatabase();
				mobiusUcdbDatabase.AllowDuplicateStructures = serviceUcdbDatabase.AllowDuplicateStructures;
				mobiusUcdbDatabase.CompoundCount = serviceUcdbDatabase.CompoundCount;
				mobiusUcdbDatabase.CompoundIdType = (Mobius.Data.CompoundIdTypeEnum)((int)serviceUcdbDatabase.CompoundIdType);
				mobiusUcdbDatabase.CreationDate = serviceUcdbDatabase.CreationDate;
				mobiusUcdbDatabase.DatabaseId = serviceUcdbDatabase.DatabaseId;
				mobiusUcdbDatabase.Description = serviceUcdbDatabase.Description;
				mobiusUcdbDatabase.ModelCount = serviceUcdbDatabase.ModelCount;
				mobiusUcdbDatabase.Name = serviceUcdbDatabase.Name;
				mobiusUcdbDatabase.NameSpace = serviceUcdbDatabase.NameSpace;
				mobiusUcdbDatabase.OwnerUserName = serviceUcdbDatabase.OwnerUserName;
				mobiusUcdbDatabase.PendingCompoundCount = serviceUcdbDatabase.PendingCompoundCount;
				mobiusUcdbDatabase.PendingCompoundId = serviceUcdbDatabase.PendingCompoundId;
				mobiusUcdbDatabase.PendingStatus = (Mobius.Data.UcdbWaitState)((int)serviceUcdbDatabase.PendingStatus);
				mobiusUcdbDatabase.PendingUpdateDate = serviceUcdbDatabase.PendingUpdateDate;
				mobiusUcdbDatabase.Public = serviceUcdbDatabase.Public;
				mobiusUcdbDatabase.RowState = (Mobius.Data.UcdbRowState)((int)serviceUcdbDatabase.RowState);
				mobiusUcdbDatabase.StructureSearchSupported = serviceUcdbDatabase.StructureSearchSupported;
				mobiusUcdbDatabase.UpdateDate = serviceUcdbDatabase.UpdateDate;

				mobiusUcdbDatabase.Compounds = ConvertServiceUcdbCompoundsToMobiusUcdbCompounds(serviceUcdbDatabase.Compounds, context);
				mobiusUcdbDatabase.Models = ConvertServiceUcdbModelsToMobiusUcdbModels(serviceUcdbDatabase.Models, context);
			}
			return mobiusUcdbDatabase;
		}

		private Mobius.Services.Types.UcdbDatabase[] ConvertMobiusUcdbDatabasesToServiceUcdbDatabases(
				Mobius.Data.UcdbDatabase[] mobiusUcdbDatabases, TypeConversionContext context)
		{
			Mobius.Services.Types.UcdbDatabase[] serviceUcdbDatabases = null;
			if (mobiusUcdbDatabases != null)
			{
				serviceUcdbDatabases = new Mobius.Services.Types.UcdbDatabase[mobiusUcdbDatabases.LongLength];
				for (long i = 0; i < mobiusUcdbDatabases.LongLength; i++)
				{
					Mobius.Data.UcdbDatabase mobiusUcdbDatabase = mobiusUcdbDatabases[i];
					serviceUcdbDatabases[i] = ConvertMobiusUcdbDatabaseToServiceUcdbDatabase(mobiusUcdbDatabase, context);
				}
			}
			return serviceUcdbDatabases;
		}

		private Mobius.Data.UcdbDatabase[] ConvertServiceUcdbDatabasesToMobiusUcdbDatabases(
				Mobius.Services.Types.UcdbDatabase[] serviceUcdbDatabases, TypeConversionContext context)
		{
			Mobius.Data.UcdbDatabase[] mobiusUcdbDatabases = null;
			if (serviceUcdbDatabases != null)
			{
				mobiusUcdbDatabases = new Mobius.Data.UcdbDatabase[serviceUcdbDatabases.LongLength];
				for (long i = 0; i < serviceUcdbDatabases.LongLength; i++)
				{
					Mobius.Services.Types.UcdbDatabase serviceUcdbDatabase = serviceUcdbDatabases[i];
					mobiusUcdbDatabases[i] = ConvertServiceUcdbDatabaseToMobiusUcdbDatabase(serviceUcdbDatabase, context);
				}
			}
			return mobiusUcdbDatabases;
		}

		private Mobius.Services.Types.UcdbModel[] ConvertMobiusUcdbModelsToServiceUcdbModels(
				Mobius.Data.UcdbModel[] mobiusUcdbModels, TypeConversionContext context)
		{
			Mobius.Services.Types.UcdbModel[] serviceUcdbModels = null;
			if (mobiusUcdbModels != null)
			{
				serviceUcdbModels = new Mobius.Services.Types.UcdbModel[mobiusUcdbModels.LongLength];
				for (long i = 0; i < mobiusUcdbModels.LongLength; i++)
				{
					Mobius.Data.UcdbModel mobiusUcdbModel = mobiusUcdbModels[i];
					serviceUcdbModels[i] = ConvertMobiusUcdbModelToServiceUcdbModel(mobiusUcdbModel, context);
				}
			}
			return serviceUcdbModels;
		}

		private Mobius.Services.Types.UcdbModel ConvertMobiusUcdbModelToServiceUcdbModel(
				Mobius.Data.UcdbModel mobiusUcdbModel, TypeConversionContext context)
		{
			Mobius.Services.Types.UcdbModel serviceUcdbModel = null;
			if (mobiusUcdbModel != null)
			{
				//since we don't maintain a cache of these, no way to shortcut out at this level
				// so ignore the bread crumbs

				serviceUcdbModel = new Mobius.Services.Types.UcdbModel();
				serviceUcdbModel.BuildId = mobiusUcdbModel.BuildId;
				serviceUcdbModel.CreationDate = mobiusUcdbModel.CreationDate;
				serviceUcdbModel.DatabaseId = mobiusUcdbModel.DatabaseId;
				serviceUcdbModel.DbModelId = mobiusUcdbModel.DbModelId;
				serviceUcdbModel.ModelId = mobiusUcdbModel.ModelId;
				serviceUcdbModel.PendingStatus = (Mobius.Services.Types.UcdbWaitState)((int)mobiusUcdbModel.PendingStatus);
				serviceUcdbModel.RowState = (Mobius.Services.Types.UcdbRowState)((int)mobiusUcdbModel.RowState);
				serviceUcdbModel.UpdateDate = mobiusUcdbModel.UpdateDate;
			}
			return serviceUcdbModel;
		}

		private Mobius.Data.UcdbModel[] ConvertServiceUcdbModelsToMobiusUcdbModels(
				Mobius.Services.Types.UcdbModel[] serviceUcdbModels, TypeConversionContext context)
		{
			Mobius.Data.UcdbModel[] mobiusUcdModels = null;
			if (serviceUcdbModels != null)
			{
				mobiusUcdModels = new Mobius.Data.UcdbModel[serviceUcdbModels.LongLength];
				for (long i = 0; i < serviceUcdbModels.LongLength; i++)
				{
					Mobius.Services.Types.UcdbModel serviceUcdbModel = serviceUcdbModels[i];
					mobiusUcdModels[i] = ConvertServiceUcdbModelToMobiusUcdbModel(serviceUcdbModel, context);
				}
			}
			return mobiusUcdModels;
		}

		private Mobius.Data.UcdbModel ConvertServiceUcdbModelToMobiusUcdbModel(
				Mobius.Services.Types.UcdbModel serviceUcdbModel, TypeConversionContext context)
		{
			Mobius.Data.UcdbModel mobiusUcdbModel = null;
			if (serviceUcdbModel != null)
			{
				//since we don't maintain a cache of these, no way to shortcut out at this level
				// so ignore the bread crumbs

				mobiusUcdbModel = new Mobius.Data.UcdbModel();
				mobiusUcdbModel.BuildId = serviceUcdbModel.BuildId;
				mobiusUcdbModel.CreationDate = serviceUcdbModel.CreationDate;
				mobiusUcdbModel.DatabaseId = serviceUcdbModel.DatabaseId;
				mobiusUcdbModel.DbModelId = serviceUcdbModel.DbModelId;
				mobiusUcdbModel.ModelId = serviceUcdbModel.ModelId;
				mobiusUcdbModel.PendingStatus = (Mobius.Data.UcdbWaitState)((int)serviceUcdbModel.PendingStatus);
				mobiusUcdbModel.RowState = (Mobius.Data.UcdbRowState)((int)serviceUcdbModel.RowState);
				mobiusUcdbModel.UpdateDate = serviceUcdbModel.UpdateDate;
			}
			return mobiusUcdbModel;
		}

		private Mobius.Services.Types.UcdbCompound[] ConvertMobiusUcdbCompoundsToServiceUcdbCompounds(
				Mobius.Data.UcdbCompound[] mobiusUcdbCompounds, TypeConversionContext context)
		{
			Mobius.Services.Types.UcdbCompound[] serviceUcdbCompounds = null;
			if (mobiusUcdbCompounds != null)
			{
				serviceUcdbCompounds = new Mobius.Services.Types.UcdbCompound[mobiusUcdbCompounds.LongLength];
				for (long i = 0; i < mobiusUcdbCompounds.LongLength; i++)
				{
					Mobius.Data.UcdbCompound mobiusUcdbCompound = mobiusUcdbCompounds[i];
					serviceUcdbCompounds[i] = ConvertMobiusUcdbCompoundToServiceUcdbCompound(mobiusUcdbCompound, context);
				}
			}
			return serviceUcdbCompounds;
		}

		private Mobius.Services.Types.UcdbCompound ConvertMobiusUcdbCompoundToServiceUcdbCompound(
				Mobius.Data.UcdbCompound mobiusUcdbCompound, TypeConversionContext context)
		{
			Mobius.Services.Types.UcdbCompound serviceUcdbCompound = null;
			if (mobiusUcdbCompound != null)
			{
				//since we don't maintain a cache of these, no way to shortcut out at this level
				// so ignore the bread crumbs

				serviceUcdbCompound = new Mobius.Services.Types.UcdbCompound();
				serviceUcdbCompound.Comment = mobiusUcdbCompound.Comment;
				serviceUcdbCompound.CompoundId = mobiusUcdbCompound.CompoundId;
				serviceUcdbCompound.CreationDate = mobiusUcdbCompound.CreationDate;
				serviceUcdbCompound.CreatorUserName = mobiusUcdbCompound.CreatorUserName;
				serviceUcdbCompound.DatabaseId = mobiusUcdbCompound.DatabaseId;
				serviceUcdbCompound.ExtCmpndIdNbr = mobiusUcdbCompound.ExtCmpndIdNbr;
				serviceUcdbCompound.ExtCmpndIdTxt = mobiusUcdbCompound.ExtCmpndIdTxt;
				serviceUcdbCompound.MolFormula = mobiusUcdbCompound.MolFormula;
				serviceUcdbCompound.MolKeys = mobiusUcdbCompound.MolKeys;
				serviceUcdbCompound.MolStructure = mobiusUcdbCompound.MolStructure;
				serviceUcdbCompound.MolStructureFormat = (Mobius.Services.Types.MolStructureFormatEnum)((int)mobiusUcdbCompound.MolStructureFormat);
				serviceUcdbCompound.MolWeight = mobiusUcdbCompound.MolWeight;
				serviceUcdbCompound.PendingStatus = (Mobius.Services.Types.UcdbWaitState)((int)mobiusUcdbCompound.PendingStatus);
				serviceUcdbCompound.RowState = (Mobius.Services.Types.UcdbRowState)((int)mobiusUcdbCompound.RowState);
				serviceUcdbCompound.UpdateDate = mobiusUcdbCompound.UpdateDate;

				serviceUcdbCompound.Aliases = ConvertMobiusUcdbAliasesToServiceUcdbAliases(mobiusUcdbCompound.Aliases, context);
			}
			return serviceUcdbCompound;
		}

		private Mobius.Data.UcdbCompound[] ConvertServiceUcdbCompoundsToMobiusUcdbCompounds(
				Mobius.Services.Types.UcdbCompound[] serviceUcdbCompounds, TypeConversionContext context)
		{
			Mobius.Data.UcdbCompound[] mobiusUcdbCompounds = null;
			if (serviceUcdbCompounds != null)
			{
				mobiusUcdbCompounds = new Mobius.Data.UcdbCompound[serviceUcdbCompounds.LongLength];
				for (long i = 0; i < serviceUcdbCompounds.LongLength; i++)
				{
					Mobius.Services.Types.UcdbCompound serviceUcdbCompound = serviceUcdbCompounds[i];
					Mobius.Data.UcdbCompound mobiusUcdbCompound = ConvertServiceUcdbCompoundToMobiusUcdbCompound(serviceUcdbCompound, context);
					mobiusUcdbCompounds[i] = mobiusUcdbCompound;
				}
			}
			return mobiusUcdbCompounds;
		}

		private Mobius.Data.UcdbCompound ConvertServiceUcdbCompoundToMobiusUcdbCompound(
				Mobius.Services.Types.UcdbCompound serviceUcdbCompound, TypeConversionContext context)
		{
			Mobius.Data.UcdbCompound mobiusUcdbCompound = null;
			if (serviceUcdbCompound != null)
			{
				//since we don't maintain a cache of these, no way to shortcut out at this level
				// so ignore the bread crumbs

				mobiusUcdbCompound = new Mobius.Data.UcdbCompound();
				mobiusUcdbCompound.Comment = serviceUcdbCompound.Comment;
				mobiusUcdbCompound.CompoundId = serviceUcdbCompound.CompoundId;
				mobiusUcdbCompound.CreationDate = serviceUcdbCompound.CreationDate;
				mobiusUcdbCompound.CreatorUserName = serviceUcdbCompound.CreatorUserName;
				mobiusUcdbCompound.DatabaseId = serviceUcdbCompound.DatabaseId;
				mobiusUcdbCompound.ExtCmpndIdNbr = serviceUcdbCompound.ExtCmpndIdNbr;
				mobiusUcdbCompound.ExtCmpndIdTxt = serviceUcdbCompound.ExtCmpndIdTxt;
				mobiusUcdbCompound.MolFormula = serviceUcdbCompound.MolFormula;
				mobiusUcdbCompound.MolKeys = serviceUcdbCompound.MolKeys;
				mobiusUcdbCompound.MolStructure = serviceUcdbCompound.MolStructure;
				mobiusUcdbCompound.MolStructureFormat = (Mobius.Data.MolStructureFormatEnum)((int)serviceUcdbCompound.MolStructureFormat);
				mobiusUcdbCompound.MolWeight = serviceUcdbCompound.MolWeight;
				mobiusUcdbCompound.PendingStatus = (Mobius.Data.UcdbWaitState)((int)serviceUcdbCompound.PendingStatus);
				mobiusUcdbCompound.RowState = (Mobius.Data.UcdbRowState)((int)serviceUcdbCompound.RowState);
				mobiusUcdbCompound.UpdateDate = serviceUcdbCompound.UpdateDate;

				mobiusUcdbCompound.Aliases = ConvertServiceUcdbAliasesToMobiusUcdbAliases(serviceUcdbCompound.Aliases, context);
			}
			return mobiusUcdbCompound;
		}

		private Mobius.Services.Types.UcdbAlias[] ConvertMobiusUcdbAliasesToServiceUcdbAliases(
				Mobius.Data.UcdbAlias[] mobiusUcdbAliases, TypeConversionContext context)
		{
			Mobius.Services.Types.UcdbAlias[] serviceUcdbAliases = null;
			if (mobiusUcdbAliases != null)
			{
				serviceUcdbAliases = new Mobius.Services.Types.UcdbAlias[mobiusUcdbAliases.LongLength];
				for (long i = 0; i < mobiusUcdbAliases.LongLength; i++)
				{
					Mobius.Data.UcdbAlias mobiusUcdbAlias = mobiusUcdbAliases[i];
					serviceUcdbAliases[i] = ConvertMobiusUcdbAliasToServiceUcdbAlias(mobiusUcdbAlias, context);
				}
			}
			return serviceUcdbAliases;
		}

		private Mobius.Services.Types.UcdbAlias ConvertMobiusUcdbAliasToServiceUcdbAlias(
				Mobius.Data.UcdbAlias mobiusUcdbAlias, TypeConversionContext context)
		{
			Mobius.Services.Types.UcdbAlias serviceUcdbAlias = null;
			if (mobiusUcdbAlias != null)
			{
				//since we don't maintain a cache of these, no way to shortcut out at this level
				// so ignore the bread crumbs

				serviceUcdbAlias = new Mobius.Services.Types.UcdbAlias();
				serviceUcdbAlias.AliasId = mobiusUcdbAlias.AliasId;
				serviceUcdbAlias.CompoundId = mobiusUcdbAlias.CompoundId;
				serviceUcdbAlias.CreationDate = mobiusUcdbAlias.CreationDate;
				serviceUcdbAlias.Name = mobiusUcdbAlias.Name;
				serviceUcdbAlias.RowState = (Mobius.Services.Types.UcdbRowState)((int)mobiusUcdbAlias.RowState);
				serviceUcdbAlias.Type = mobiusUcdbAlias.Type;
				serviceUcdbAlias.UpdateDate = mobiusUcdbAlias.UpdateDate;
			}
			return serviceUcdbAlias;
		}

		private Mobius.Data.UcdbAlias[] ConvertServiceUcdbAliasesToMobiusUcdbAliases(
				Mobius.Services.Types.UcdbAlias[] serviceUcdbAliases, TypeConversionContext context)
		{
			Mobius.Data.UcdbAlias[] mobiusUcdbAliases = null;
			if (serviceUcdbAliases != null)
			{
				mobiusUcdbAliases = new Mobius.Data.UcdbAlias[serviceUcdbAliases.LongLength];
				for (long i = 0; i < serviceUcdbAliases.LongLength; i++)
				{
					Mobius.Services.Types.UcdbAlias serviceUcdbAlias = serviceUcdbAliases[i];
					mobiusUcdbAliases[i] = ConvertServiceUcdbAliasToMobiusUcdbAlias(serviceUcdbAlias, context);
				}
			}
			return mobiusUcdbAliases;
		}

		private Mobius.Data.UcdbAlias ConvertServiceUcdbAliasToMobiusUcdbAlias(
		Mobius.Services.Types.UcdbAlias serviceUcdbAlias, TypeConversionContext context)
		{
			Mobius.Data.UcdbAlias mobiusUcdbAlias = null;
			if (serviceUcdbAlias != null)
			{
				//since we don't maintain a cache of these, no way to shortcut out at this level
				// so ignore the bread crumbs

				mobiusUcdbAlias = new Mobius.Data.UcdbAlias();
				mobiusUcdbAlias.AliasId = serviceUcdbAlias.AliasId;
				mobiusUcdbAlias.CompoundId = serviceUcdbAlias.CompoundId;
				mobiusUcdbAlias.CreationDate = serviceUcdbAlias.CreationDate;
				mobiusUcdbAlias.Name = serviceUcdbAlias.Name;
				mobiusUcdbAlias.RowState = (Mobius.Data.UcdbRowState)((int)serviceUcdbAlias.RowState);
				mobiusUcdbAlias.Type = serviceUcdbAlias.Type;
				mobiusUcdbAlias.UpdateDate = serviceUcdbAlias.UpdateDate;
			}
			return mobiusUcdbAlias;
		}



		private Mobius.Services.Types.AnnotationVo ConvertMobiusAnnotationVoToServiceAnnotationVo(
				Mobius.Data.AnnotationVo mobiusAnnotationVo, TypeConversionContext context)
		{
			Mobius.Services.Types.AnnotationVo serviceAnnotationVo = null;
			if (mobiusAnnotationVo != null)
			{
				//since we don't maintain a cache of these, no way to shortcut out at this level
				// so ignore the bread crumbs

				serviceAnnotationVo = new Mobius.Services.Types.AnnotationVo();
				serviceAnnotationVo.chng_op_cd = mobiusAnnotationVo.chng_op_cd;
				serviceAnnotationVo.chng_usr_id = mobiusAnnotationVo.chng_usr_id;
				serviceAnnotationVo.cmnt_txt = mobiusAnnotationVo.cmnt_txt;
				serviceAnnotationVo.crt_dt = mobiusAnnotationVo.crt_dt;
				serviceAnnotationVo.dc_lnk = mobiusAnnotationVo.dc_lnk;
				serviceAnnotationVo.ext_cmpnd_id_nbr = mobiusAnnotationVo.ext_cmpnd_id_nbr;
				serviceAnnotationVo.ext_cmpnd_id_txt = mobiusAnnotationVo.ext_cmpnd_id_txt;
				serviceAnnotationVo.mthd_vrsn_id = mobiusAnnotationVo.mthd_vrsn_id;
				serviceAnnotationVo.rslt_grp_id = mobiusAnnotationVo.rslt_grp_id;
				serviceAnnotationVo.rslt_id = mobiusAnnotationVo.rslt_id;
				serviceAnnotationVo.rslt_typ_id = mobiusAnnotationVo.rslt_typ_id;
				serviceAnnotationVo.rslt_val_dt = mobiusAnnotationVo.rslt_val_dt;
				serviceAnnotationVo.rslt_val_nbr = mobiusAnnotationVo.rslt_val_nbr;
				serviceAnnotationVo.rslt_val_prfx_txt = mobiusAnnotationVo.rslt_val_prfx_txt;
				serviceAnnotationVo.rslt_val_txt = mobiusAnnotationVo.rslt_val_txt;
				serviceAnnotationVo.src_db_id = mobiusAnnotationVo.src_db_id;
				serviceAnnotationVo.uom_id = mobiusAnnotationVo.uom_id;
				serviceAnnotationVo.updt_dt = mobiusAnnotationVo.updt_dt;
			}
			return serviceAnnotationVo;
		}

		private Mobius.Data.AnnotationVo ConvertServiceAnnotationVoToMobiusAnnotationVo(
				Mobius.Services.Types.AnnotationVo serviceAnnotationVo, TypeConversionContext context)
		{
			Mobius.Data.AnnotationVo mobiusAnnotationVo = null;
			if (serviceAnnotationVo != null)
			{
				//since we don't maintain a cache of these, no way to shortcut out at this level
				// so ignore the bread crumbs

				mobiusAnnotationVo = new Mobius.Data.AnnotationVo();
				mobiusAnnotationVo.chng_op_cd = serviceAnnotationVo.chng_op_cd;
				mobiusAnnotationVo.chng_usr_id = serviceAnnotationVo.chng_usr_id;
				mobiusAnnotationVo.cmnt_txt = serviceAnnotationVo.cmnt_txt;
				mobiusAnnotationVo.crt_dt = serviceAnnotationVo.crt_dt;
				mobiusAnnotationVo.dc_lnk = serviceAnnotationVo.dc_lnk;
				mobiusAnnotationVo.ext_cmpnd_id_nbr = serviceAnnotationVo.ext_cmpnd_id_nbr;
				mobiusAnnotationVo.ext_cmpnd_id_txt = serviceAnnotationVo.ext_cmpnd_id_txt;
				mobiusAnnotationVo.mthd_vrsn_id = serviceAnnotationVo.mthd_vrsn_id;
				mobiusAnnotationVo.rslt_grp_id = serviceAnnotationVo.rslt_grp_id;
				mobiusAnnotationVo.rslt_id = serviceAnnotationVo.rslt_id;
				mobiusAnnotationVo.rslt_typ_id = serviceAnnotationVo.rslt_typ_id;
				mobiusAnnotationVo.rslt_val_dt = serviceAnnotationVo.rslt_val_dt;
				mobiusAnnotationVo.rslt_val_nbr = serviceAnnotationVo.rslt_val_nbr;
				mobiusAnnotationVo.rslt_val_prfx_txt = serviceAnnotationVo.rslt_val_prfx_txt;
				mobiusAnnotationVo.rslt_val_txt = serviceAnnotationVo.rslt_val_txt;
				mobiusAnnotationVo.src_db_id = serviceAnnotationVo.src_db_id;
				mobiusAnnotationVo.uom_id = serviceAnnotationVo.uom_id;
				mobiusAnnotationVo.updt_dt = serviceAnnotationVo.updt_dt;
			}
			return mobiusAnnotationVo;
		}

		private List<Mobius.Services.Types.AnnotationVo> ConvertMobiusAnnotationVosToServiceAnnotationVos(
				List<Mobius.Data.AnnotationVo> mobiusAnnotationVos, TypeConversionContext context)
		{
			List<Mobius.Services.Types.AnnotationVo> serviceAnnotationVos = null;
			if (mobiusAnnotationVos != null)
			{
				serviceAnnotationVos = new List<Mobius.Services.Types.AnnotationVo>(mobiusAnnotationVos.Count);
				foreach (Mobius.Data.AnnotationVo mobiusAnnotationVo in mobiusAnnotationVos)
				{
					Mobius.Services.Types.AnnotationVo serviceAnnotationVo =
							ConvertMobiusAnnotationVoToServiceAnnotationVo(mobiusAnnotationVo, context);
					serviceAnnotationVos.Add(serviceAnnotationVo);
				}
			}
			return serviceAnnotationVos;
		}

		private List<Mobius.Data.AnnotationVo> ConvertServiceAnnotationVosToMobiusAnnotationVos(
				List<Mobius.Services.Types.AnnotationVo> serviceAnnotationVos, TypeConversionContext context)
		{
			List<Mobius.Data.AnnotationVo> mobiusAnnotationVos = null;
			if (serviceAnnotationVos != null)
			{
				mobiusAnnotationVos = new List<Mobius.Data.AnnotationVo>(serviceAnnotationVos.Count);
				foreach (Mobius.Services.Types.AnnotationVo serviceAnnotationVo in serviceAnnotationVos)
				{
					Mobius.Data.AnnotationVo mobiusAnnotationVo =
							ConvertServiceAnnotationVoToMobiusAnnotationVo(serviceAnnotationVo, context);
					mobiusAnnotationVos.Add(mobiusAnnotationVo);
				}
			}
			return mobiusAnnotationVos;
		}



		private Mobius.Services.Types.CidList ConvertMobiusCidListToServiceCidList(
				Mobius.Data.CidList mobiusCidList, TypeConversionContext context)
		{
			Mobius.Services.Types.CidList serviceCidList = null;
			if (mobiusCidList != null)
			{
				//since we don't maintain a cache of these, no way to shortcut out at this level
				// so ignore the bread crumbs

				serviceCidList = new Mobius.Services.Types.CidList();
				serviceCidList.List = ConvertMobiusCidListElementListToServiceCidListElementList(mobiusCidList.List, context);
				serviceCidList.UserObjectNode = ConvertMobiusUserObjectToServiceUserObjectNode(mobiusCidList.UserObject, context);

				if (mobiusCidList.Dict != null)
				{
					serviceCidList.Dict = new Dictionary<string, Mobius.Services.Types.CidListElement>();
					foreach (string commonKey in mobiusCidList.Dict.Keys)
					{
						Mobius.Data.CidListElement mobiusCidListElement = mobiusCidList.Dict[commonKey];
						Mobius.Services.Types.CidListElement serviceCidListElement =
								ConvertMobiusCidListElementToServiceCidListElement(mobiusCidListElement, context);
						serviceCidList.Dict.Add(commonKey, serviceCidListElement);
					}
				}
			}
			return serviceCidList;
		}

		private Mobius.Data.CidList ConvertServiceCidListToMobiusCidList(
				Mobius.Services.Types.CidList serviceCidList, TypeConversionContext context)
		{
			Mobius.Data.CidList mobiusCidList = null;
			if (serviceCidList != null)
			{
				//since we don't maintain a cache of these, no way to shortcut out at this level
				// so ignore the bread crumbs

				mobiusCidList = new Mobius.Data.CidList();
				mobiusCidList.List = ConvertServiceCidListElementListToMobiusCidListElementList(serviceCidList.List, context);
				mobiusCidList.UserObject = ConvertServiceUserObjectNodeToMobiusUserObject(serviceCidList.UserObjectNode, context);

				if (serviceCidList.Dict != null)
				{
					mobiusCidList.Dict = new Dictionary<string, Mobius.Data.CidListElement>();
					foreach (string commonKey in serviceCidList.Dict.Keys)
					{
						Mobius.Services.Types.CidListElement serviceCidListElement = serviceCidList.Dict[commonKey];
						Mobius.Data.CidListElement mobiusCidListElement =
								ConvertServiceCidListElementToMobiusCidListElement(serviceCidListElement, context);
						mobiusCidList.Dict.Add(commonKey, mobiusCidListElement);
					}
				}
			}
			return mobiusCidList;
		}

		private List<Mobius.Data.CidListElement> ConvertServiceCidListElementListToMobiusCidListElementList(
				List<Mobius.Services.Types.CidListElement> serviceCidListElementList, TypeConversionContext context)
		{
			List<Mobius.Data.CidListElement> mobiusCidListElementList = null;
			if (serviceCidListElementList != null)
			{
				mobiusCidListElementList = new List<Mobius.Data.CidListElement>(serviceCidListElementList.Count);
				foreach (Mobius.Services.Types.CidListElement serviceCidListElement in serviceCidListElementList)
				{
					Mobius.Data.CidListElement mobiusCidListElement =
							ConvertServiceCidListElementToMobiusCidListElement(serviceCidListElement, context);
					mobiusCidListElementList.Add(mobiusCidListElement);
				}
			}
			return mobiusCidListElementList;
		}

		private List<Mobius.Services.Types.CidListElement> ConvertMobiusCidListElementListToServiceCidListElementList(
				List<Mobius.Data.CidListElement> mobiusCidListElementList, TypeConversionContext context)
		{
			List<Mobius.Services.Types.CidListElement> serviceCidListElementList = null;
			if (mobiusCidListElementList != null)
			{
				serviceCidListElementList = new List<Mobius.Services.Types.CidListElement>(mobiusCidListElementList.Count);
				foreach (Mobius.Data.CidListElement mobiusCidListElement in mobiusCidListElementList)
				{
					Mobius.Services.Types.CidListElement serviceCidListElement =
							ConvertMobiusCidListElementToServiceCidListElement(mobiusCidListElement, context);
					serviceCidListElementList.Add(serviceCidListElement);
				}
			}
			return serviceCidListElementList;
		}

		private Mobius.Services.Types.CidListElement ConvertMobiusCidListElementToServiceCidListElement(
				Mobius.Data.CidListElement mobiusCidListElement, TypeConversionContext context)
		{
			Mobius.Services.Types.CidListElement serviceCidListElement = null;
			if (mobiusCidListElement != null)
			{
				//since we don't maintain a cache of these, no way to shortcut out at this level
				// so ignore the bread crumbs

				serviceCidListElement = new Mobius.Services.Types.CidListElement();
				serviceCidListElement.Cid = mobiusCidListElement.Cid;
				serviceCidListElement.StringTag = mobiusCidListElement.StringTag;
				serviceCidListElement.Tag = mobiusCidListElement.Tag;
			}
			return serviceCidListElement;
		}

		private Mobius.Data.CidListElement ConvertServiceCidListElementToMobiusCidListElement(
				Mobius.Services.Types.CidListElement serviceCidListElement, TypeConversionContext context)
		{
			Mobius.Data.CidListElement mobiusCidListElement = null;
			if (serviceCidListElement != null)
			{
				//since we don't maintain a cache of these, no way to shortcut out at this level
				// so ignore the bread crumbs

				mobiusCidListElement = new Mobius.Data.CidListElement();
				mobiusCidListElement.Cid = serviceCidListElement.Cid;
				mobiusCidListElement.StringTag = serviceCidListElement.StringTag;
				mobiusCidListElement.Tag = serviceCidListElement.Tag;
			}
			return mobiusCidListElement;
		}



		private Mobius.Services.Types.TargetMap ConvertMobiusTargetMapToServiceTargetMap(
				Mobius.Data.TargetMap mobiusTargetMap, TypeConversionContext context)
		{
			Mobius.Services.Types.TargetMap serviceTargetMap = null;
			if (mobiusTargetMap != null)
			{
				//since we don't maintain a cache of these, no way to shortcut out at this level
				// so ignore the bread crumbs

				serviceTargetMap = new Mobius.Services.Types.TargetMap();
				serviceTargetMap.Bounds = new Mobius.Services.Types.Rectangle(mobiusTargetMap.Bounds);
				serviceTargetMap.CoordsFile = mobiusTargetMap.CoordsFile;
				serviceTargetMap.ImageFile = mobiusTargetMap.ImageFile;
				serviceTargetMap.ImageType = mobiusTargetMap.ImageType;
				serviceTargetMap.Label = mobiusTargetMap.Label;
				serviceTargetMap.MarkBounds = mobiusTargetMap.MarkBounds;
				serviceTargetMap.Name = mobiusTargetMap.Name;
				serviceTargetMap.Source = mobiusTargetMap.Source;
				serviceTargetMap.Type = mobiusTargetMap.Type;

				if (mobiusTargetMap.Coords != null)
				{
					Dictionary<int, List<Mobius.Data.TargetMapCoords>> mobiusCoords = mobiusTargetMap.Coords;
					Dictionary<int, List<Mobius.Services.Types.TargetMapCoords>> serviceCoords =
							new Dictionary<int, List<Mobius.Services.Types.TargetMapCoords>>(mobiusCoords.Keys.Count);
					serviceTargetMap.Coords = serviceCoords;
					foreach (int commonKey in mobiusCoords.Keys)
					{
						List<Mobius.Data.TargetMapCoords> mobiusTargetMapCoordsList = mobiusCoords[commonKey];
						List<Mobius.Services.Types.TargetMapCoords> serviceTargetMapCoordsList =
								ConvertMobiusTargetMapCoordsListToServiceTargetMapCoordsList(mobiusTargetMapCoordsList, context);
						serviceCoords.Add(commonKey, serviceTargetMapCoordsList);
					}
				}

			}
			return serviceTargetMap;
		}


		private List<Mobius.Services.Types.TargetMapCoords> ConvertMobiusTargetMapCoordsListToServiceTargetMapCoordsList(
				List<Mobius.Data.TargetMapCoords> mobiusTargetMapCoordsList, TypeConversionContext context)
		{
			List<Mobius.Services.Types.TargetMapCoords> serviceTargetMapCoordsList = null;
			if (mobiusTargetMapCoordsList != null)
			{
				serviceTargetMapCoordsList = new List<Mobius.Services.Types.TargetMapCoords>(mobiusTargetMapCoordsList.Count);
				foreach (Mobius.Data.TargetMapCoords mobiusTargetMapCoords in mobiusTargetMapCoordsList)
				{
					Mobius.Services.Types.TargetMapCoords serviceTargetMapCoords =
							ConvertMobiusTargetMapCoordsToServiceTargetMapCoords(mobiusTargetMapCoords, context);
					serviceTargetMapCoordsList.Add(serviceTargetMapCoords);
				}
			}
			return serviceTargetMapCoordsList;
		}

		private Mobius.Services.Types.TargetMapCoords ConvertMobiusTargetMapCoordsToServiceTargetMapCoords(
				Mobius.Data.TargetMapCoords mobiusTargetMapCoords, TypeConversionContext context)
		{
			Mobius.Services.Types.TargetMapCoords serviceTargetMapCoords = null;
			if (mobiusTargetMapCoords != null)
			{
				//since we don't maintain a cache of these, no way to shortcut out at this level
				// so ignore the bread crumbs

				serviceTargetMapCoords = new Mobius.Services.Types.TargetMapCoords();
				serviceTargetMapCoords.TargetId = mobiusTargetMapCoords.TargetId;
				serviceTargetMapCoords.X = mobiusTargetMapCoords.X;
				serviceTargetMapCoords.X2 = mobiusTargetMapCoords.X2;
				serviceTargetMapCoords.Y = mobiusTargetMapCoords.Y;
				serviceTargetMapCoords.Y2 = mobiusTargetMapCoords.Y2;
			}
			return serviceTargetMapCoords;
		}

		private Mobius.Data.TargetMap ConvertServiceTargetMapToMobiusTargetMap(
				Mobius.Services.Types.TargetMap serviceTargetMap, TypeConversionContext context)
		{
			Mobius.Data.TargetMap mobiusTargetMap = null;
			if (serviceTargetMap != null)
			{
				//since we don't maintain a cache of these, no way to shortcut out at this level
				// so ignore the bread crumbs

				mobiusTargetMap = new Mobius.Data.TargetMap();
				mobiusTargetMap.Bounds = (System.Drawing.Rectangle)serviceTargetMap.Bounds;
				mobiusTargetMap.CoordsFile = serviceTargetMap.CoordsFile;
				mobiusTargetMap.ImageFile = serviceTargetMap.ImageFile;
				mobiusTargetMap.ImageType = serviceTargetMap.ImageType;
				mobiusTargetMap.Label = serviceTargetMap.Label;
				mobiusTargetMap.MarkBounds = serviceTargetMap.MarkBounds;
				mobiusTargetMap.Name = serviceTargetMap.Name;
				mobiusTargetMap.Source = serviceTargetMap.Source;
				mobiusTargetMap.Type = serviceTargetMap.Type;

				if (serviceTargetMap.Coords != null)
				{
					Dictionary<int, List<Mobius.Services.Types.TargetMapCoords>> serviceCoords = serviceTargetMap.Coords;
					Dictionary<int, List<Mobius.Data.TargetMapCoords>> mobiusCoords =
							new Dictionary<int, List<Mobius.Data.TargetMapCoords>>(serviceCoords.Keys.Count);
					mobiusTargetMap.Coords = mobiusCoords;
					foreach (int commonKey in serviceCoords.Keys)
					{
						List<Mobius.Services.Types.TargetMapCoords> serviceTargetMapCoordsList = serviceCoords[commonKey];
						List<Mobius.Data.TargetMapCoords> mobiusTargetMapCoordsList =
								ConvertServiceTargetMapCoordsListToMobiusTargetMapCoordsList(serviceTargetMapCoordsList, context);
						mobiusCoords.Add(commonKey, mobiusTargetMapCoordsList);
					}
				}

			}
			return mobiusTargetMap;
		}


		private List<Mobius.Data.TargetMapCoords> ConvertServiceTargetMapCoordsListToMobiusTargetMapCoordsList(
				List<Mobius.Services.Types.TargetMapCoords> serviceTargetMapCoordsList, TypeConversionContext context)
		{
			List<Mobius.Data.TargetMapCoords> mobiusTargetMapCoordsList = null;
			if (serviceTargetMapCoordsList != null)
			{
				mobiusTargetMapCoordsList = new List<Mobius.Data.TargetMapCoords>(serviceTargetMapCoordsList.Count);
				foreach (Mobius.Services.Types.TargetMapCoords serviceTargetMapCoords in serviceTargetMapCoordsList)
				{
					Mobius.Data.TargetMapCoords mobiusTargetMapCoords =
							ConvertServiceTargetMapCoordsToMobiusTargetMapCoords(serviceTargetMapCoords, context);
					mobiusTargetMapCoordsList.Add(mobiusTargetMapCoords);
				}
			}
			return mobiusTargetMapCoordsList;
		}

		private Mobius.Data.TargetMapCoords ConvertServiceTargetMapCoordsToMobiusTargetMapCoords(
				Mobius.Services.Types.TargetMapCoords serviceTargetMapCoords, TypeConversionContext context)
		{
			Mobius.Data.TargetMapCoords mobiusTargetMapCoords = null;
			if (serviceTargetMapCoords != null)
			{
				//since we don't maintain a cache of these, no way to shortcut out at this level
				// so ignore the bread crumbs

				mobiusTargetMapCoords = new Mobius.Data.TargetMapCoords();
				mobiusTargetMapCoords.TargetId = serviceTargetMapCoords.TargetId;
				mobiusTargetMapCoords.X = serviceTargetMapCoords.X;
				mobiusTargetMapCoords.X2 = serviceTargetMapCoords.X2;
				mobiusTargetMapCoords.Y = serviceTargetMapCoords.Y;
				mobiusTargetMapCoords.Y2 = serviceTargetMapCoords.Y2;
			}
			return mobiusTargetMapCoords;
		}



		//Dictionary
		private Dictionary<string, Mobius.Services.Types.DictionaryMx> knownServiceDictionaries =
				new Dictionary<string, Mobius.Services.Types.DictionaryMx>();
		private Mobius.Services.Types.DictionaryMx ConvertMobiusDictionaryMxToServiceDictionaryMx(
				Mobius.Data.DictionaryMx mobiusDictionaryMx, TypeConversionContext context)
		{
			Mobius.Services.Types.DictionaryMx serviceDictionaryMx = null;
			if (mobiusDictionaryMx != null)
			{
				//since no run-away recursion is possible for these, ignore the bread crumbs

				string dictionaryName = mobiusDictionaryMx.Name;
				lock (knownServiceDictionaries)
				{
					if (knownServiceDictionaries.ContainsKey(dictionaryName))
					{
						//we've seen this one before...
						serviceDictionaryMx = knownServiceDictionaries[dictionaryName];
					}
					else
					{
						//create the new dictionary
						serviceDictionaryMx = new Mobius.Services.Types.DictionaryMx();
						knownServiceDictionaries.Add(dictionaryName, serviceDictionaryMx);
					}
				}
				serviceDictionaryMx.DefinitionLookup = mobiusDictionaryMx.DefinitionLookup;
				serviceDictionaryMx.Name = mobiusDictionaryMx.Name;
				serviceDictionaryMx.WordLookup = mobiusDictionaryMx.WordLookup;
				serviceDictionaryMx.Words = mobiusDictionaryMx.Words;
			}
			return serviceDictionaryMx;
		}

		private Dictionary<string, Mobius.Data.DictionaryMx> knownMobiusDictionaries = new Dictionary<string, Mobius.Data.DictionaryMx>();
		private Mobius.Data.DictionaryMx ConvertServiceDictionaryMxToMobiusDictionaryMx(
				Mobius.Services.Types.DictionaryMx serviceDictionaryMx, TypeConversionContext context)
		{
			Mobius.Data.DictionaryMx mobiusDictionaryMx = null;
			if (serviceDictionaryMx != null)
			{
				//since no run-away recursion is possible for these, ignore the bread crumbs

				string dictionaryName = serviceDictionaryMx.Name;
				lock (knownMobiusDictionaries)
				{
					if (knownMobiusDictionaries.ContainsKey(dictionaryName))
					{
						//we've seen this one before...
						mobiusDictionaryMx = knownMobiusDictionaries[dictionaryName];
					}
					else
					{
						//create the new dictionary
						mobiusDictionaryMx = new Mobius.Data.DictionaryMx();
						knownMobiusDictionaries.Add(dictionaryName, mobiusDictionaryMx);
					}
				}
				mobiusDictionaryMx.DefinitionLookup = serviceDictionaryMx.DefinitionLookup;
				mobiusDictionaryMx.Name = serviceDictionaryMx.Name;
				mobiusDictionaryMx.WordLookup = serviceDictionaryMx.WordLookup;
				mobiusDictionaryMx.Words = serviceDictionaryMx.Words;
			}
			return mobiusDictionaryMx;
		}

		private Dictionary<string, Mobius.Services.Types.DictionaryMx> ConvertMobiusDictionaryMxDictToServiceDictionaryMxDict(
				Dictionary<string, Mobius.Data.DictionaryMx> mobiusDictionaryMxDict, TypeConversionContext context)
		{
			Dictionary<string, Mobius.Services.Types.DictionaryMx> serviceDictionaryMxDict = null;
			if (mobiusDictionaryMxDict != null)
			{
				//since we don't maintain a cache of these, no way to shortcut out at this level
				// so ignore the bread crumbs

				serviceDictionaryMxDict = new Dictionary<string, Mobius.Services.Types.DictionaryMx>(mobiusDictionaryMxDict.Keys.Count);
				foreach (string commonKey in mobiusDictionaryMxDict.Keys)
				{
					Mobius.Data.DictionaryMx mobiusDictionaryMx = mobiusDictionaryMxDict[commonKey];
					Mobius.Services.Types.DictionaryMx serviceDictionaryMx =
							ConvertMobiusDictionaryMxToServiceDictionaryMx(mobiusDictionaryMx, context);
					serviceDictionaryMxDict.Add(commonKey, serviceDictionaryMx);
				}
			}
			return serviceDictionaryMxDict;
		}

		private Dictionary<string, Mobius.Data.DictionaryMx> ConvertServiceDictionaryMxDictToMobiusDictionaryMxDict(
				Dictionary<string, Mobius.Services.Types.DictionaryMx> serviceDictionaryMxDict, TypeConversionContext context)
		{
			Dictionary<string, Mobius.Data.DictionaryMx> mobiusDictionaryMxDict = null;
			if (serviceDictionaryMxDict != null)
			{
				//since we don't maintain a cache of these, no way to shortcut out at this level
				// so ignore the bread crumbs

				mobiusDictionaryMxDict = new Dictionary<string, Mobius.Data.DictionaryMx>(serviceDictionaryMxDict.Keys.Count);
				foreach (string commonKey in serviceDictionaryMxDict.Keys)
				{
					Mobius.Services.Types.DictionaryMx serviceDictionaryMx = serviceDictionaryMxDict[commonKey];
					Mobius.Data.DictionaryMx mobiusDictionaryMx =
							ConvertServiceDictionaryMxToMobiusDictionaryMx(serviceDictionaryMx, context);
					mobiusDictionaryMxDict.Add(commonKey, mobiusDictionaryMx);
				}
			}
			return mobiusDictionaryMxDict;
		}

		private Mobius.Data.TableDescription ConvertServiceMetaTableToMobiusMetaTable(
				Mobius.Services.Types.MetaTableDescription serviceMetaTableDescription, TypeConversionContext context)
		{
			Mobius.Data.TableDescription mobiusMetaTableDescription = null;
			if (serviceMetaTableDescription != null)
			{
				//since we don't maintain a cache of these, no way to shortcut out at this level
				// so ignore the bread crumbs

				mobiusMetaTableDescription = new Mobius.Data.TableDescription();
				mobiusMetaTableDescription.BinaryDescription = serviceMetaTableDescription.BinaryDescription;
				mobiusMetaTableDescription.TextDescription = serviceMetaTableDescription.TextDescription;
				mobiusMetaTableDescription.TypeName = serviceMetaTableDescription.TypeName;
			}
			return mobiusMetaTableDescription;
		}

		private Mobius.Services.Types.MetaTableDescription ConvertMobiusMetaTableToServiceMetaTable(
				Mobius.Data.TableDescription mobiusMetaTableDescription, TypeConversionContext context)
		{
			Mobius.Services.Types.MetaTableDescription serviceMetaTableDescription = null;
			if (mobiusMetaTableDescription != null)
			{
				//since we don't maintain a cache of these, no way to shortcut out at this level
				// so ignore the bread crumbs

				serviceMetaTableDescription = new Mobius.Services.Types.MetaTableDescription();
				serviceMetaTableDescription.BinaryDescription = mobiusMetaTableDescription.BinaryDescription;
				serviceMetaTableDescription.TextDescription = mobiusMetaTableDescription.TextDescription;
				serviceMetaTableDescription.TypeName = mobiusMetaTableDescription.TypeName;
			}
			return serviceMetaTableDescription;
		}

	}
}
