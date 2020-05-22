using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	public partial class CriteriaStructure : DevExpress.XtraEditors.XtraForm
	{
		internal QueryColumn Qc; // query column being edited
		internal ParsedStructureCriteria Psc; // parsed version of criteria
		internal bool ShowOptions = true;

		Size InitialFormSize; // form size without options or search results preview panels
		int BottomCtlsHeight; // height of botton divider and OK/Cancel buttons

		Size SmallWorldSize;// initial SmallWorld form size, changes as form is resized

		bool InSetup { get { return SetupDepth > 0; } }
		int SetupDepth = 0;

		static bool InEdit = false; // prevent reentry

		static CriteriaStructure Instance;

		/// <summary>
		/// Constructor
		/// </summary>

		public CriteriaStructure()
		{
			SetupDepth++;

			InitializeComponent();

			if (SystemUtil.InDesignMode) return;

			//Ensure the correct BorderWidth and TitleBarHeight are calculated so that the form is the correct height nwr 3/1/2019
			ClientSize = new Size(Size.Width - 6, Size.Height - 40);

			// Calulate and save initial form size and location information
			FormMetricsInfo fmi = WindowsHelper.GetFormMetrics(this);

			QueryMolCtl.AllowEditing = true;

			int pad = QueryMolCtl.Left; // left side padding
			int cw = QueryMolCtl.Width + pad * 2; // client width with no rt-side search results preview panel
			int fw = cw + fmi.BorderWidth * 2; // associated form width

			int ch = fmi.ClientSize.Height + (RetrieveModel.Top - OK.Top); // client height without a search options panel
			int fh = ch + fmi.TitleBarHeight + fmi.BorderWidth * 2; // associated form height

			InitialFormSize = new Size(fw, fh);
			BottomCtlsHeight = fmi.ClientSize.Height - BottomDivider.Top;

			Screen screen = Screen.PrimaryScreen; // calc initial size for form with SmallWorld search selected
			int swWidth = 1000; // predefined width to include preview results
			if (swWidth > screen.WorkingArea.Width) swWidth = screen.WorkingArea.Width;
			int swHeight = InitialFormSize.Height + SmallWorldOptions.Height + BottomCtlsHeight;
			if (swHeight > screen.WorkingArea.Height) swHeight = screen.WorkingArea.Height;
			SmallWorldSize = new Size(swWidth, swHeight);

			Size = InitialFormSize; // set initial form size
			Location = WindowsHelper.GetCenteredScreenLocation(SmallWorldSize); // set location of form based on SmallWorld form size

			PreviewPanel.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;

			PreviewCtl.BorderStyle = BorderStyle.None;
			PreviewCtl.Dock = DockStyle.Fill; // dock ctl within PreviewPanel

			SSOptions.BorderStyle = BorderStyle.None;
			SSOptions.Location = SmallWorldOptions.Location; // use SmallWorld locate other option controls

			SimOptions.BorderStyle = BorderStyle.None;
			SimOptions.Location = SmallWorldOptions.Location;

			FSOptions.BorderStyle = BorderStyle.None;
			FSOptions.Location = SmallWorldOptions.Location;

			SmallWorldOptions.BorderStyle = BorderStyle.None;

			// Hide SmallWorld option if not permitted for this user

			if (!SmallWorldIsAvailableForUser)
			{
				SmallWorld.Visible = false;
				None.Location = SmallWorld.Location;
			}

			SetupDepth--;

			return;
		}

		/// <summary>
		/// Allow user to edit the form
		/// </summary>
		/// <returns></returns>

		public static bool Edit(
			QueryColumn qc)
		{
			if (InEdit) return false; // prevent reentry via multiple clicks on the edit button

			InEdit = true;

			try
			{
				if (!SS.I.UserInfo.Privileges.CanRetrieveStructures)
				{
					MessageBoxMx.ShowError("Structure Access Not Authorized");
					return false;
				}

				//if (Instance == null) // use single instance
				Instance = new CriteriaStructure();

				Instance.SetupForm(qc);

				DialogResult dr = Instance.ShowDialog(SessionManager.ActiveForm);
				if (dr == DialogResult.OK) return true;
				else return false;
			}
			finally { InEdit = false; }
		}

		/// <summary>
		/// Setup Form
		/// </summary>
		/// <param name="qc"></param>

		void SetupForm(
			QueryColumn qc)
		{
			bool enabled;

			SetupDepth++;

			Qc = qc; // column we will be specifying criteria for

			// Setup dialog boxes with current settings

			Psc = ParsedStructureCriteria.Parse(Qc);

			// Enable Search option controls

			enabled = ChemSearchIsSupportedForColumn;
			SubStruct.Enabled = Similarity.Enabled = Full.Enabled = enabled;
			if (Psc.SearchType == StructureSearchType.Unknown) // default to SS search if not defined yet
				Psc.SearchType = StructureSearchType.Substructure;

			// Enable SmallWorld Search option controls

			enabled = SmallWorldSearchIsSupportedForColumn;
			SmallWorld.Enabled = enabled;
			if (enabled)
			{
				if (Psc.SearchType == StructureSearchType.Unknown) // default to SmallWorld search
					Psc.SearchType = StructureSearchType.SmallWorld;
			}
			else if (Psc.SearchType == StructureSearchType.SmallWorld) // smallworld not allowed
				Psc.SearchType = StructureSearchType.Unknown;

			 DisplayPreferences.SetStandardDisplayPreferences(QueryMolCtl.KekuleControl.Preferences);

			if (Psc.SearchType == StructureSearchType.Substructure)
			{
				SubStruct.Checked = true;
				QueryMolCtl.KekuleControl.Preferences.HydrogenDisplayMode = HydrogenDisplayMode.Off; // no hydrogen display for SSS
			}

			else if (Psc.SearchType == StructureSearchType.FullStructure)
				Full.Checked = true;

			else if (Psc.SearchType == StructureSearchType.MolSim)
				Similarity.Checked = true;

			else if (Psc.SearchType == StructureSearchType.SmallWorld)
				SmallWorld.Checked = true;

			else None.Checked = true;

			QueryMolCtl.SetupAndRenderMolecule(Psc.Molecule);

			QueryMolCtl.EditValueChanged += new System.EventHandler(QueryMolCtl_EditValueChanged);

			//SetMolDisplayFormatText();

			SetupOptions(true);
			SetupDepth--;

			return;
		}

		//void SetMolDisplayFormatText()
		//{
		//	MolDisplayFormatEdit.Text =
		//			QueryMolCtl.Molecule.PrimaryDisplayFormat == MoleculeRenderer.Helm ? "Biopolymer" : "Structure";
		//}

		/// <summary>
		/// Get currently selected search type from form
		/// </summary>
		/// <returns></returns>

		StructureSearchType GetSearchType()
		{
			StructureSearchType searchType = StructureSearchType.Unknown;

			if (SubStruct.Checked) searchType = StructureSearchType.Substructure;

			else if (Full.Checked) searchType = StructureSearchType.FullStructure;

			else if (Similarity.Checked) searchType = StructureSearchType.MolSim;

			else if (SmallWorld.Checked) searchType = StructureSearchType.SmallWorld;

			return searchType;
		}

		/// <summary>
		/// See if Chem-type searching is available for the current query column
		/// </summary>

		bool ChemSearchIsSupportedForColumn
		{
			get
			{
				if (Qc == null || Qc.MetaColumn == null || Qc.QueryTable == null || Qc.QueryTable.MetaTable == null)
					return true; // assume available if no info

				MetaColumn mc = Qc.MetaColumn;
				MetaTable mt = mc.MetaTable;

				if (mc.DataType != MetaColumnType.Structure || !mc.IsSearchable) return false;

				if (mc.IsChemistryCartridgeSearchable) return true;

				else return false;
			}
		}

		/// <summary>
		/// Return true if user is authorized to use SmallWorld & Qc supports Smallworld searching
		/// </summary>

		public bool SmallWorldSearchIsSupportedForColumn
		{
			get
			{
				if (!SmallWorldIsAvailableForUser) return false;

				if (Qc == null || Qc.MetaColumn == null || Qc.QueryTable == null || Qc.QueryTable.MetaTable == null)
					return false; // NOT available if no info

				MetaColumn mc = Qc.MetaColumn;
				MetaTable mt = mc.MetaTable;

				if (mc.DataType != MetaColumnType.Structure || !mc.IsSearchable) return false;

				if (mc.IsSmallWorldSearchable) return true;

				else return false;
			}
		}

/// <summary>
/// Return true if SmallWorld searching is available to the current user
/// </summary>

		public static bool SmallWorldIsAvailableForUser
		{
			get 
			{
				string userName = SS.I.UserName;
				DictionaryMx d = DictionaryMx.Get("SmallWorldUsers");
				if ((d != null && d.LookupDefinition(userName) != null)) return true; // must be in allowed user list 
				//if (Security.IsAdministrator(SS.I.UserName)) return true; // or a Mobius admin for now

				else return false;
			}
		}

		/// <summary>
		/// Display proper options subform or hide options if not showing
		/// based on the Psc settings
		/// </summary>

		void SetupOptions(bool initialSetup = false)
		{
			FormMetricsInfo fmi = WindowsHelper.GetFormMetrics(this);

			SetupDepth++;
			bool showOptions = ShowOptions;

			if (Psc.SearchType != StructureSearchType.Unknown)
				// && Psc.SearchType != StructureSearchType.SSS) // don't show SS options for now
			{
				ShowOptionsButton.Enabled = true;
			}

			else // no options available
			{
				showOptions = false;
				ShowOptionsButton.Enabled = false;
			}

			ShowOptionsButton.ImageIndex = (showOptions ? 1 : 0); // set proper ShowOptions Expand/Contract image button
			BottomDivider.Visible = (showOptions || Psc.SearchType == StructureSearchType.SmallWorld); // line above OK/Cancel

			if (Psc.SearchType == StructureSearchType.Substructure)
			{
				FSOptions.Visible = SimOptions.Visible = SmallWorldOptions.Visible = PreviewPanel.Visible = false;
				SSOptions.Visible = showOptions;

				int optHeight = showOptions ? SSOptions.Height + BottomCtlsHeight : 0;
				Size = new Size(InitialFormSize.Width, InitialFormSize.Height + optHeight);
				SSOptions.Setup(Psc);
			}

			else if (Psc.SearchType == StructureSearchType.MolSim)
			{
				SSOptions.Visible = FSOptions.Visible = SmallWorldOptions.Visible = PreviewPanel.Visible = false;
				SimOptions.Visible = showOptions;
				int optHeight = showOptions ? SimOptions.Height + BottomCtlsHeight : 0;
				Size = new Size(InitialFormSize.Width, InitialFormSize.Height + optHeight);
				SimOptions.Setup(Psc);
			}

			else if (Psc.SearchType == StructureSearchType.FullStructure)
			{
				SSOptions.Visible = SimOptions.Visible = SmallWorldOptions.Visible = PreviewPanel.Visible = false;
				FSOptions.Visible = showOptions;

				int optHeight = showOptions ? FSOptions.Height + BottomCtlsHeight : 0;
				Size = new Size(InitialFormSize.Width, InitialFormSize.Height + optHeight);
				FSOptions.Setup(Psc);
			}

			else if (Psc.SearchType == StructureSearchType.SmallWorld)
			{
				SSOptions.Visible = FSOptions.Visible = SimOptions.Visible = false;
				SmallWorldOptions.Visible = showOptions;

				SmallWorldOptions.CriteriaStructureForm = this; // link SmallWorldOptions to us

				SmallWorldPredefinedParameters swp = Psc.SmallWorldParameters;

				SmallWorldOptions.Setup(swp);

				Size = SmallWorldSize; // set form size same as last time

				if (!PreviewPanel.Visible) // if preview not visible then adjust size and show
				{
					PreviewCtl.InitializeView();

					Size cs = this.ClientRectangle.Size;
					PreviewPanel.Width = cs.Width - PreviewPanel.Left - 2;
					PreviewPanel.Height = BottomDivider.Top - PreviewPanel.Top + 2;
					PreviewPanel.Visible = true;
				}

				if (initialSetup) SmallWorldOptions.StartInitialQueryExecution(); // start search if initial setup
			}

			else Size = InitialFormSize; // no search type

			Refresh();

			SetupDepth--;
			return;
		}

		private void SubStruct_CheckedChanged(object sender, EventArgs e)
		{
			if (!SubStruct.Checked) return;

			QueryMolCtl.KekuleControl.Preferences.HydrogenDisplayMode = HydrogenDisplayMode.Off; // no hydrogens for ss
			Psc.SearchType = StructureSearchType.Substructure;
			SetupOptions();
		}

		private void Full_CheckedChanged(object sender, EventArgs e)
		{
			if (!Full.Checked) return;

			QueryMolCtl.KekuleControl.Preferences.HydrogenDisplayMode = HydrogenDisplayMode.Hetero;
			Psc.SearchType = StructureSearchType.FullStructure;
			SetupOptions();
		}

		private void Similarity_CheckedChanged(object sender, EventArgs e)
		{
			if (!Similarity.Checked) return;

			QueryMolCtl.KekuleControl.Preferences.HydrogenDisplayMode = HydrogenDisplayMode.Hetero;
			Psc.SearchType = StructureSearchType.MolSim;
			//CriteriaStructureSimOptions.InitSimOptionsIfUndefined(Psc);
			SetupOptions();
		}

		private void SmallWorld_CheckedChanged(object sender, EventArgs e)
		{
			if (InSetup) return;
			if (!SmallWorld.Checked) return;

			Psc.SearchType = StructureSearchType.SmallWorld;
			SetupOptions();
			SmallWorldOptions.StartQueryExecution(); // start search if enough data
			return;
		}

		private void EditStructureButton_Click(object sender, EventArgs e)
		{
			DelayedCallback.Schedule(QueryMolCtl.EditMolecule); // invoke editor after returning from this event
		}

		private void EditStructureButton_ArrowButtonClick(object sender, EventArgs e)
		{
			Point p = EditStructureButton.PointToScreen(new Point(0, EditStructureButton.Height));
			EditMoleculeDropdownMenu.Show(p);
		}

		private void NewChemicalStructureMenuItem_Click(object sender, EventArgs e)
		{
			QueryMolCtl.Molecule = new MoleculeMx(MoleculeFormat.Molfile);
			QueryMolCtl.RenderMolecule();
			DelayedCallback.Schedule(QueryMolCtl.EditMolecule); // invoke editor after returning from this event
		}

		private void NewBiopolymerMenuItem_Click(object sender, EventArgs e)
		{
			QueryMolCtl.Molecule = new MoleculeMx(MoleculeFormat.Helm);
			QueryMolCtl.RenderMolecule();
			DelayedCallback.Schedule(QueryMolCtl.EditMolecule); // invoke editor after returning from this event
		}


		private void RetrieveModel_Click(object sender, EventArgs e)
		{
			Point p = RetrieveModel.PointToScreen(new System.Drawing.Point(0, RetrieveModel.Height));
			MoleculeSelectorControl.ShowModelSelectionMenu(p, QueryMolCtl, GetSearchType());
		}

		private void RetrieveRecentButton_Click(object sender, EventArgs e)
		{
			MoleculeSelectorControl.SelectMruMolecule(QueryMolCtl);
			QueryMolCtl.Focus(); // move focus away
		}

		private void RetrieveFavoritesButton_Click(object sender, EventArgs e)
		{
			MoleculeSelectorControl.SelectFavoriteMolecule(QueryMolCtl);
			QueryMolCtl.Focus(); // move focus away
		}

		private void AddToFavoritesButton_Click(object sender, EventArgs e)
		{
			Point p = RetrieveModel.PointToScreen(new System.Drawing.Point(0, AddToFavoritesButton.Height));
			MoleculeSelectorControl.AddToFavoritesList(QueryMolCtl, GetSearchType());
			QueryMolCtl.Focus(); // move focus away
		}

		private void EditStructure_Click(object sender, EventArgs e)
		{
			DelayedCallback.Schedule(QueryMolCtl.EditMolecule); // invoke editor after returning from this event
		}

		private void EditNewChemicalStructureMenuItem_Click(object sender, EventArgs e)
		{
			QueryMolCtl.SetupAndEditNewMoleculeAsynch(MoleculeFormat.Molfile);
		}

		private void EditNewBiopolymerMenuItem_Click(object sender, EventArgs e)
		{
			QueryMolCtl.SetupAndEditNewMoleculeAsynch(MoleculeFormat.Helm);
		}

		private void None_CheckedChanged(object sender, EventArgs e)
		{
			if (!None.Checked) return;

			Psc.SearchType = StructureSearchType.Unknown;
			SetupOptions();
		}

		private void OK_Click(object sender, EventArgs e)
		{
			Psc.Molecule = QueryMolCtl.Molecule.Clone();
			Psc.Molecule.RemoveStructureCaption(); // remove any structure caption that may cause a problem

			try
			{
				if (None.Checked || Lex.IsNullOrEmpty(Psc.Molecule.PrimaryValue)) // say none if checked or no structure
					Psc.SearchType = StructureSearchType.Unknown;
				else if (SubStruct.Checked) SSOptions.GetValues(Psc);
				else if (Similarity.Checked) SimOptions.GetValues(Psc);
				else if (Full.Checked) FSOptions.GetValues(Psc);
				else if (SmallWorld.Checked)
				{
					SmallWorldOptions.GetValues(Psc);
					UpdateAssociatedSmallWorldQueryColumns(Psc);
				}

				else throw new Exception("No recognized search type checked"); // shouldn't happen

				Psc.ConvertToQueryColumnCriteria(Qc);
			}

			catch (Exception ex)
			{
				MessageBoxMx.ShowError(ex.Message);
				return;
			}

			MoleculeSelectorControl.AddToMruList(QueryMolCtl, Psc.SearchType);

			DialogResult = DialogResult.OK;
			return;
		}

		bool UpdateAssociatedSmallWorldQueryColumns(
			ParsedStructureCriteria psc)
		{

			if (Qc == null || Qc.QueryTable == null) return false;

			SmallWorldPredefinedParameters swp = psc.SmallWorldParameters;
			if (swp == null) return false;

			MetaTable mt = Qc.MetaColumn.MetaTable;

			if (Lex.Eq(mt.Name, MetaTable.SmallWorldMetaTableName)) // special SmallWorld table
			{
				string dbCriteria = Lex.IsDefined(swp.Database) ? "Database in (" + swp.Database + ")" : "";
				SetSwQcCriteria("Database", dbCriteria, swp.Database);

				SetSwQcCriteria("distance", swp.Distance.Format());
				SetSwQcCriteria("terminalUp", swp.TerminalUp.Format());
				SetSwQcCriteria("terminalDown", swp.TerminalDown.Format());
				SetSwQcCriteria("ringUp", swp.RingUp.Format());
				SetSwQcCriteria("ringDown", swp.RingDown.Format());
				SetSwQcCriteria("linkerUp", swp.LinkerUp.Format());
				SetSwQcCriteria("linkerDown", swp.LinkerDown.Format());
				SetSwQcCriteria("minorMutations", swp.MutationMinor.Format());
				SetSwQcCriteria("majorMutations", swp.MutationMajor.Format());
				SetSwQcCriteria("substitutions", swp.SubstitutionRange.Format());
				SetSwQcCriteria("hybridisation", swp.HybridisationChange.Format());

				return true;
			}

			else if (mt.DatabaseListMetaColumn != null) // basic structure table
			{
				MetaColumn dbSetMc = mt.DatabaseListMetaColumn;
				QueryColumn dbSetQc = Qc.QueryTable.GetQueryColumnByName(dbSetMc.Name);
				if (dbSetQc == null) return false;

				dbSetQc.Criteria = dbSetMc.Name + " in (" + swp.Database + ")";
				dbSetQc.CriteriaDisplay = swp.Database;
				return true;
			}

			else return false; // no other cols
		}

		void SetSwQcCriteria(
			string mcName,
			string criteria,
			string criteriaDisplay = null)
		{
			if (Lex.IsUndefined(criteriaDisplay)) criteriaDisplay = criteria;

			QueryColumn qc = Qc.QueryTable.GetQueryColumnByName(mcName);
			if (qc != null)
			{
				qc.Criteria = criteria;
				qc.CriteriaDisplay = criteriaDisplay;
			}

			return;
		}

		private void Cancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}

		private void ShowOptionsButton_Click(object sender, EventArgs e)
		{
			ShowOptions = !ShowOptions;
			SetupOptions();
		}

		private void CriteriaStructure_SizeChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			if (SmallWorld.Checked) SmallWorldSize = this.Size; // keep track of form size for SmallWorld
			return;
		}

		private void QueryMolCtl_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			//SetMolDisplayFormatText();

			if (SmallWorld.Checked)
			{
				SmallWorldOptions.StartQueryExecution(); // start preview search if SmallWorld query
			}
		}

	}

}