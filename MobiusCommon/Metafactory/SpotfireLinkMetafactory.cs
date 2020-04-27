using Mobius.UAL;
using Mobius.ComOps;
using Mobius.Data;
using Mobius.QueryEngineLibrary;

using System;
using System.Collections.Generic;

namespace Mobius.MetaFactoryNamespace
{
	/// <summary>
	/// Spotfire Analysis link metafactory
	/// </summary>
	/// 
	public class SpotfireLinkMetafactory : IMetaFactory
	{
		int RecursionDepth = 0;

		public SpotfireLinkMetafactory()
		{
			return;
		}

		/// <summary>
		/// Build metatable for a Spotfire analysis link
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public MetaTable GetMetaTable(
			string name)
		{
			UserObject uo;
			SpotfireViewProps sl;
			MetaTable mt, mt2;
			MetaColumn mc;
			int objectId;

			string prefix = "spotfirelink_"; 
			if (name.ToLower().IndexOf(prefix) != 0) return null;
			string tok = name.Substring(prefix.Length); // get the object id
			if (!int.TryParse(tok, out objectId)) return null;

			uo = UserObjectDao.Read(objectId);
			if (uo == null) return null;
			if (!Permissions.UserHasReadAccess(Security.UserName, uo)) return null;

			try 
			{ 
				sl = SpotfireViewProps.Deserialize(uo.Content);
				if (sl == null) throw new Exception("Null Deserialize result for object " + uo.Id);
			}
			catch (Exception ex)
			{
				DebugLog.Message(ex);
				return null;
			}

			MetaTable root = MetaTableCollection.GetWithException(MetaTable.PrimaryRootTable);

			mt = MetaTableCollection.GetWithException("SPOTFIRELINK_MODEL");
			mt = mt.Clone(); // build metatable here

			mt.Name = "SPOTFIRELINK_" + uo.Id;
			mt.Code = uo.Content; // store serialized SpotfireLink content in Code field

			mt.Parent = root;
			mt.MetaBrokerType = MetaBrokerType.SpotfireLink;
			mt.TableMap = root.TableMap; // allow searches on CorpId
			mt.Label = uo.Name;

			foreach (MetaColumn mc0 in mt.MetaColumns)
			{
				mc0.InitialSelection = ColumnSelectionEnum.Hidden;
			}

			foreach (SpotfireLinkParameter sp in sl.SpotfireLinkParameters.Values)
			{
				mc = mt.GetMetaColumnByName(sp.Name);
				if (mc == null) continue;

				bool b = false;
				bool.TryParse(sp.Value, out b);

				mc.InitialSelection = b ? ColumnSelectionEnum.Selected : ColumnSelectionEnum.Hidden;
			}

			if (mt.KeyMetaColumn.InitialSelection != ColumnSelectionEnum.Selected)
				mt.KeyMetaColumn.InitialSelection = ColumnSelectionEnum.Unselected;

			mc = mt.GetMetaColumnByName("VISUALIZATION");
			if (mc != null) mc.InitialSelection = ColumnSelectionEnum.Selected;

			SetAllInQueryInitialCriteria(mt, "invitro_assays", "invitro_assays_default_to_all", "in (ALL_ASSAY_INVITRO_ASSAYS_IN_THIS_QUERY)");
			SetAllInQueryInitialCriteria(mt, "insilico_models", "insilico_models_default_to_all", "in (ALL_SPM_INSILICO_MODELS_IN_THIS_QUERY)");

			return mt;
		}

/// <summary>
/// SetAllInQueryInitialCriteria
/// </summary>
/// <param name="mt"></param>
/// <param name="mcName"></param>

		void SetAllInQueryInitialCriteria(
			MetaTable mt, 
			string mcName,
			string mc2name, 
			string criteria)
		{
			MetaColumn mc = mt.GetMetaColumnByName(mcName);
			if (mc == null) return;

			MetaColumn mc2 = mt.GetMetaColumnByName(mc2name);
			if (mc2 == null) return;
 			if (mc2.InitialSelection != ColumnSelectionEnum.Selected) return;

			mc.InitialCriteria = criteria;
			mc2.InitialSelection = ColumnSelectionEnum.Hidden;
			return;
		}

		MetaColumn SetMetaColumnVisibility(
			MetaTable mt,
			string mcName,
			SpotfireViewProps sl,
			string parmName)
		{
			MetaColumn mc = mt.GetMetaColumnByName(mcName);
			if (mc == null) return null;

			ColumnSelectionEnum mcis = (sl.IsParameterValueTrue(parmName) ? ColumnSelectionEnum.Selected : ColumnSelectionEnum.Hidden);
			mc.InitialSelection = mcis;
			return mc;
		}

		/// <summary>
		/// Calculate & persist metatable stats for tables belonging to broker
		/// </summary>
		/// <returns></returns>

		public int UpdateMetaTableStatistics()
		{
			return 0;
		}

		/// <summary>
		/// Load persisted metatable stats
		/// </summary>
		/// <param name="metaTableStats"></param>
		/// <returns></returns>

		public int LoadMetaTableStats(
			Dictionary<string, MetaTableStats> metaTableStats)
		{
			return 0;
		}

	}
}
