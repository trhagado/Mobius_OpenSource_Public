using Mobius.Data;
using Mobius.ComOps;
using Mobius.ServiceFacade;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.Remoting.Messaging;

namespace Mobius.ClientComponents
{

	/// <summary> 
	/// SmallWorld Depiction management
	/// </summary>

	public class SmallWorldDepictions
	{
		public Dictionary<string, string>[] SvgDict = // cached Svgs, indexed by type, keyed by Cid, values are SVG for cid
			new Dictionary<string, string>[4]; 
		public int[] Status = new int[4]; // status for retrieval of each depiction type
		public int CurrentRequestId = 0;

		static bool Debug = false;

		/// <summary>
		/// Get depiction for a Cid if available on client
		/// </summary>
		/// <param name="hitListId"></param>
		/// <param name="rowId"></param>
		/// <param name="color"></param>
		/// <param name="align"></param>
		/// <returns></returns>

		public string GetDepiction(
			string cid,
			bool color,
			bool align)
		{
			int i = SmallWorldData.GetSvgOptionsIndex(color, align);
			if (SvgDict[i] == null) return null;
			if (!SvgDict[i].ContainsKey(cid)) return "";
			string xml = SvgDict[i][cid];
			return xml;
		}

/// <summary>
/// Start retieval of requested depiction type list for the supplied hitlist
/// </summary>
/// <param name="qc"></param>
/// <param name="color"></param>
/// <param name="align"></param>
/// <param name="getAdditionalDataDelegate"></param>

		public void StartDepictionRetrieval(
			QueryManager qm,
			QueryColumn qc,
			bool color,
			bool align)
		{
			int i = SmallWorldData.GetSvgOptionsIndex(color, align);
			if (Status[i] != 0) return; // just return if already requested

			string mcName = qc.MetaColumn.MetaTableDotMetaColumnName;
			string command = "GetDepictions " + mcName + " " + color + " " + align;

			QeGetAdditionalDataDelegate gadd = new QeGetAdditionalDataDelegate(qm.QueryEngine.GetAdditionalData);

			DepictionRetrievalArgs a = new DepictionRetrievalArgs();
			a.qm = qm;
			a.qc = qc;
			a.color = color;
			a.align = align;
			a.reqId = ++CurrentRequestId;
			a.startTime = DateTime.Now;

			Status[i] = a.reqId; // indicate requested
			
			if (Debug) DebugLog.Message("StartDepictionRetrieval " + CurrentRequestId + ", Mc " + mcName + ", Hilight " + color + ", Align " + align);
			gadd.BeginInvoke(command, new AsyncCallback(DepictionRetrievalComplete), a);
			return;
		}

/// <summary>
/// Retrieval complete. Update cache and request display of values
/// </summary>
/// <param name="r"></param>

		void DepictionRetrievalComplete(IAsyncResult r)
		{
			try
			{
				QeGetAdditionalDataDelegate d = (r as AsyncResult).AsyncDelegate as QeGetAdditionalDataDelegate;

				DepictionRetrievalArgs a = r.AsyncState as DepictionRetrievalArgs;
				int i = SmallWorldData.GetSvgOptionsIndex(a.color, a.align);
				Status[i] = -Status[i]; // set to negative to indicate complete

				string depictionsListString = d.EndInvoke(r) as string;
				SvgDict[i] = Deserialize(depictionsListString);
				int count = SvgDict[i].Count;

				if (Debug)
				{
					DebugLog.Message("DepictionRetrievalComplete " + a.reqId + ", Current: " + (a.reqId == CurrentRequestId) +
						", Count: " + count + ", Time: " + LogFile.FormatElapsedTimeInMs(a.startTime));
				}

				if (a.reqId == CurrentRequestId) // if latest request then display
				{
					QueryManager qm = a.qm;
					MoleculeGridControl grid = qm.MoleculeGrid;

					if (grid.InvokeRequired) // use invoke if not on UI thread
						grid.Invoke(new MethodInvoker(grid.RefreshDataSourceMx));

					else grid.RefreshDataSourceMx();
				}
			}
			catch (Exception ex)
			{
				DebugLog.Message("DepictionRetrievalComplete exception: " + DebugLog.FormatExceptionMessage(ex));
			}

		}

		internal class DepictionRetrievalArgs
		{
			public QueryManager qm;
			public QueryColumn qc;
			public bool color;
			public bool align;
			public int reqId;
			public DateTime startTime;
		}	

		Dictionary<string, string> Deserialize(string depictString)
		{
			string cid, hlidTxt, ridTxt, xml;
			int hitListId, rowId;

			Dictionary<string, string> dict = new Dictionary<string, string>();

			if (Lex.IsUndefined(depictString)) return null; // dict; 

			string[] sa = depictString.Split('\n');
			foreach (string s in sa)
			{
				if (Lex.IsUndefined(s)) continue;

				Lex.Split(s, "\t", out cid, out hlidTxt, out ridTxt, out xml);
				int.TryParse(hlidTxt, out hitListId);
				int.TryParse(ridTxt, out rowId);

				Int64 i64 = (Int64)hitListId * 1000000000 + rowId;

				dict[cid] = xml;
			}

			return dict;
		}


		/// <summary>
		/// Click function to open an external web site when clicking on a CID in a SmallWorld database display
		/// </summary>
		/// <param name="args"></param>

		public static void OpenUrlFromSmallWorldCid(
			string cid)
		{
			string[] px = { "PBCHM", "PDB" };

			RootTable rt = CompoundId.GetRootTableFromCid(cid);

			if (rt == null || Lex.IsUndefined(rt.CidUrl))
			{
				if (rt != null && Lex.Contains(rt.MetaTableName, MetaTable.PrimaryRootTable))
				{
					RelatedCompoundsDialog.Show(cid);
					return;
				}

				MessageBoxMx.ShowError("Unable to determine url link for compound Id: " + cid);
				return;
			}

			string url = rt.CidUrl;
			foreach (string p in px) // remove unwanted URLs
			{
				if (Lex.StartsWith(cid, p))
					cid = cid.Substring(p.Length);
			}

			url = Lex.Replace(url, "[CID]", cid);
			SystemUtil.StartProcess(url);
			return;
		}
	}

}
