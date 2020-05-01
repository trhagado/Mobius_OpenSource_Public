using Mobius.ComOps;
using Mobius.Data;
using Mobius.CdkMx;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	public partial class RelatedStructureBrowser : DevExpress.XtraEditors.XtraForm
	{
		RelatedStructureControlManager RSM; // RelatedStructureManager for this form

		DateTime LastCompoundIdControlKeyDownTime = DateTime.MinValue;
		string PreviousInput = "";
		bool InTimer_Tick = false;

		bool InSetup
		{
			get { return RSM.InSetup; }
			set { RSM.InSetup = value; }
		}

		public static StructureSearchType StructureSearchTypes = // all search types except SSS
			StructureSearchType.FullStructure |
			StructureSearchType.MolSim |
			StructureSearchType.MatchedPairs |
			StructureSearchType.SmallWorld;

		const int InitialPopupPos = 100;
		private static int PopupPos = InitialPopupPos; // incremental popup positioning

		/// <summary>
		/// Constructor
		/// </summary>
		public RelatedStructureBrowser()
		{
			InitializeComponent();

			RelatedStructuresControl.BorderStyle = BorderStyle.None; // hide border needed at design time

			RSM = new RelatedStructureControlManager();
			RelatedStructuresControl.RSM = RSM;
			RSM.RSC = RelatedStructuresControl;
			
			return;
		}

		/// <summary>
		/// Show related cid counts and structures for supplied CID
		/// </summary>
		/// <param name="initialCid"></param>
		/// <param name="mt"></param>

		public static void Show(
			string initialCid,
			string queryType,
			bool excludeCurrentCompounds)
		{
			RelatedStructureBrowser i = new RelatedStructureBrowser();
			i.ShowInstance(initialCid, queryType, excludeCurrentCompounds);
		}

/// <summary>
/// Show related structures for supplied CID
/// </summary>
/// <param name="initialCid"></param>
/// <param name="queryType"></param>
/// <param name="excludeCurrentCompounds"></param>

		public void ShowInstance(
			string initialCid,
			string queryType,
			bool excludeCurrentCompounds)
		{
			try
			{
				InSetup = true;

				MetaTable mt = CompoundId.GetRootMetaTableFromCid(initialCid); // set title
				string extCid = CompoundId.Format(initialCid, mt);
				string txt = extCid + " and Related Structures";
				if (mt != null) txt = mt.KeyMetaColumn.Label + " " + txt;
				Text = txt;

				RelatedStructuresControl.MoleculeControl.ClearMolecule();
				RelatedStructuresControl.SetupCheckmarks();

				CompoundIdControl.Text = initialCid; // set cid to trigger start of search and UI update on next tick

				Left = PopupPos; // position
				Top = PopupPos;
				PopupPos += 125;
				if (PopupPos > InitialPopupPos + (5 * 125)) PopupPos = InitialPopupPos;

				Show(); // show (not modal)
				BringToFront();
				return;
			}

			catch (Exception ex) { ex = ex; }

			finally
			{
				InSetup = false;
			}
		}

		private void RelatedStructureBrowser_Activated(object sender, EventArgs e)
		{
			Timer.Enabled = true;
			RSM.Timer.Enabled = true;
			return;
		}

		private void RelatedStructureBrowser_Deactivate(object sender, EventArgs e)
		{
			Timer.Enabled = false;
			RSM.Timer.Enabled = false;
			RelatedStructuresControl.PreserveRelatedCheckmarkPreferences();
			return;
		}

		private void CompoundIdControl_KeyDown(object sender, KeyEventArgs e)
		{
			LastCompoundIdControlKeyDownTime = DateTime.Now; // save last keydown time
		}

		/// <summary>
		/// Handle UI operations during tick of timer
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void Timer_Tick(object sender, EventArgs e)
		{
			if (ServiceFacade.ServiceFacade.InServiceCall) return; // avoid multiplexing service calls that could cause problems
			if (InTimer_Tick) return;
			InTimer_Tick = true;

			try
			{
				string txt = CompoundIdControl.Text;

				if (txt == PreviousInput) return;
				if (DateTime.Now.Subtract(LastCompoundIdControlKeyDownTime).TotalSeconds < .5)
				{ // wait for a pause in typing before doing retrieve
					//SystemUtil.Beep(); // debug
					return; 
				}

				PreviousInput = txt;

				MetaTable mt = CompoundId.GetRootMetaTableFromCid(txt);
				string cid = CompoundId.Normalize(txt, mt);
				string mtName = mt.Name;
				MoleculeMx queryStruct = MoleculeUtil.SelectMoleculeForCid(cid, mt);
				RSM.StartSearchAndRetrievalOfRelatedStructures(mtName, cid, queryStruct, StructureSearchTypes);

				//Text = "Structures related to " + txt; // set form header

				InSetup = true; // set structure without triggering another search
				RelatedStructuresControl.MoleculeControl.Molecule = queryStruct;
				InSetup = false;
			}

			catch (Exception ex) { ex = ex; }
			finally { InTimer_Tick = false; } 

#if false // code to handle either CID or structure input
			bool userDefinedStructure = Lex.Eq(CurrentInput, "User Defined");
			string mtName = "";

			if (!userDefinedStructure)
			{
				CidMt = CompoundId.GetRootTableFromCid(CurrentInput);
				mtName = CidMt.Name;
				Cid = CompoundId.Normalize(CurrentInput, CidMt);
				QueryStruct = MoleculeUtil.SelectChemicalStructureFromCid(Cid, CidMt);
				InSetup = true;
				Text = "Structures related to " + CurrentInput;
				ChemicalStructure.SetRendererStructure(SQuery, QueryStruct);
				InSetup = false;
			}

			else // structure directly entered
			{
				if (!StructureChanged)
				{
					InTimer_Tick = false;
					return;
				}

				Cid = SearchId.ToString();
				CidMt = null;
				mtName = "UserDefined";
				QueryStruct = new ChemicalStructure(StructureFormat.MolFile, SQuery.MolfileString);
				StructureChanged = false;
			}
#endif
	}


		/// <summary>
		/// Get the current hit list if any
		/// </summary>
		/// <returns></returns>

		HashSet<string> GetCurrentHitList()
		{
			List<string> hitList = null;

			Query q = QbUtil.Query; // current query if any
			if (q == null) return null;
			if (q.ResultKeys == null) return null;
			else return new HashSet<string>(q.ResultKeys);
		}

		private void ExcludeSearchResultsCids_CheckedChanged(object sender, EventArgs e)
		{
			if (InSetup) return;
			RSM.RenderResultsRequested = true; // set flag to rerender
			return;
		}

		private void SQuery_EditorReturned(object sender, EditorReturnedEventArgs e)
		{
			if (InSetup) return;
			return;
		}

		private void SQuery_StructureChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			CompoundIdControl.Text = "User Defined";
			RSM.StructureChanged = true;
			return;
		}

	}
}