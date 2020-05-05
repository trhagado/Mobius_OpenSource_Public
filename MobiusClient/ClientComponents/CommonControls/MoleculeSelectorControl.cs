using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
/// <summary>
/// This class provides the UI for selecting a molecule from a database, file, favorites or MRU list
/// </summary>

	public partial class MoleculeSelectorControl : DevExpress.XtraEditors.XtraForm
	{
		MoleculeControl MolEditorCtl = null; // the editor control to place the selected molecule in
		QueryColumn Qc = null; // QueryColumn associated with molecule if any
		StructureSearchType StructureSearchType = StructureSearchType.Unknown;

		public static MoleculeList MruList = null; // list of recently used criteria structures
		public static int MaxMruItems = 32;
		public static string MruPreferencesParmName = "MruStructures";

		public static MoleculeList FavoritesList = null; // list of favorite criteria structures
		public static string FavoritesPreferencesParmName = "FavoriteStructures";

		public MoleculeSelectorControl()
		{
			InitializeComponent();
		}

		/// <summary>
		/// ShowModelSelectionMenu - static method
		/// </summary>
		/// <param name="location"></param>
		/// <param name="molEditorCtl"></param>
		/// <param name="qc"></param>

		public static void ShowModelSelectionMenu(
			Point location,
			MoleculeControl molEditorCtl,
			StructureSearchType searchType,
			QueryColumn qc = null)
		{
			MoleculeSelectorControl i = new MoleculeSelectorControl();
			i.MolEditorCtl = molEditorCtl;
			i.Qc = qc;
			i.StructureSearchType = searchType;
			i.NewMoleculeMenu.Show(location);
			return;
		}
		/// <summary>
		/// ShowMruMoleculeMenu
		/// </summary>
		/// <param name="location"></param>
		/// <param name="molEditorCtl"></param>
		/// <param name="qc"></param>

		public static void SelectMruMolecule(
			MoleculeControl molEditorCtl,
			QueryColumn qc = null)
		{
			MoleculeSelectorControl i = new MoleculeSelectorControl();
			i.MolEditorCtl = molEditorCtl;
			i.Qc = qc;
			MoleculeList sl = GetMruMoleculesList();

			i.Text = "Recent Query Structures";

			//for (int i1 = 0; i1 < sl.Count; i1++) // rename to integers 1 to N
			//{
			//	sl[i1].Name = (i1 + 1).ToString();
			//}

			i.StructureListControl.Setup(sl, i.ListItemSelectedCallback);
			i.ShowDialog(SessionManager.ActiveForm);
			return;
		}

		/// <summary>
		/// AddToMruList  
		/// </summary>
		/// <param name="molEditorCtl"></param>
		/// <param name="searchType"></param>

		public static void AddToMruList(
			MoleculeControl molEditorCtl,
			StructureSearchType searchType)
		{
			MoleculeListItem sli = null;
			DialogResult dr;

			MoleculeMx mol = molEditorCtl.Molecule;
			if (Lex.IsUndefined(mol.PrimaryValue))
				return;

			mol = mol.Clone(); // make a copy of the mol

			sli = FindStructure(mol, GetMruMoleculesList());
			if (sli != null)
			{
				MruList.ItemList.Remove(sli);
			}

			sli = new MoleculeListItem();
			sli.Name = molEditorCtl.GetTemporaryStructureTag(); // get any associated pre-edit mol name
			sli.Molecule = mol;
			sli.UpdateDate = DateTime.Now;
			sli.MoleculeType = StructureSearchTypeUtil.StructureSearchTypeToExternalName(searchType);
			MruList.ItemList.Insert(0, sli);

			while (MruList.Count > MaxMruItems)
			{
				MruList.ItemList.RemoveAt(MaxMruItems - 1);
			}

			SaveStructureList(MruList, MruPreferencesParmName);
			return;
		}

		/// <summary>
		/// ShowFavoriteMoleculeMenu
		/// </summary>
		/// <param name="location"></param>
		/// <param name="molEditorCtl"></param>
		/// <param name="qc"></param>

		public static void SelectFavoriteMolecule(
			MoleculeControl molEditorCtl,
			QueryColumn qc = null)
		{
			MoleculeSelectorControl i = new MoleculeSelectorControl();
			i.MolEditorCtl = molEditorCtl;
			i.Qc = qc;
			i.SelectFavoriteMolecule();
		}

		void SelectFavoriteMolecule()
		{
			Text = "Favorite Query Structures";
			StructureListControl.Setup(GetFavoriteStructuresList(), ListItemSelectedCallback);
			DialogResult dr = ShowDialog(SessionManager.ActiveForm);
			return;
		}

		void ListItemSelectedCallback(MoleculeListItem sli)
		{
			if (MolEditorCtl == null) return;

			if (sli == null)
			{
				DialogResult = DialogResult.Cancel;
				return;
			}

			MoleculeMx mol = sli.Molecule;
			MolEditorCtl.SetPrimaryTypeAndValue(mol.PrimaryFormat, mol.PrimaryValue);
			MolEditorCtl.TagString = sli.Name + "\t" + sli.Molecule.PrimaryValue; // store name to use if molecule is unchanged
			DialogResult = DialogResult.OK;
		}

		/// <summary>
		/// Retrieve an existing molecule and store in MoleculeControl
		/// </summary>
		/// <param name="molCtl"></param>
		/// <param name="mc"></param>

		public static void RetrieveDatabaseStructure(
			MoleculeControl molCtl, 
			MetaColumn mc)
		{
			MetaTable mt = null;
			string cid = "", txt;

			txt = "compound id";
			if (mc != null)
			{
				mt = mc.MetaTable;
				txt += " (" + mt.KeyMetaColumn.Label + ", etc.)";
			}

			txt = "Enter the " + txt + " that you want to use as a model.";

			while (true)
			{
				cid = InputCompoundId.Show(txt, "Retrieve Molecule from Database", cid, mt);
				if (String.IsNullOrEmpty(cid)) return;

				string molString = InputCompoundId.Molecule.PrimaryValue;
				if (String.IsNullOrEmpty(molString))
				{
					MessageBoxMx.ShowError("Compound number is not in the database, try again.");
					continue;
				}

				MoleculeMx cs = InputCompoundId.Molecule;

				cs.RemoveStructureCaption(); // remove any caption
				molCtl.SetPrimaryTypeAndValue(cs.PrimaryFormat, cs.PrimaryValue); // set mol
				molCtl.SetTemporaryMoleculeTag(cid); // associate the cid in case saved as history/favorite
				break;
			}
		}

		/// <summary>
		/// Retrieve a saved molecule and store in Renditor
		/// </summary>
		/// <param name="ctl"></param>

		public static void RetrieveSavedMolecule(MoleculeControl molCtl)
		{
			MoleculeMx cs;
			string fileName = null;

			if (!UIMisc.ReadMoleculeFileDialog(out cs, out fileName)) return;

			molCtl.SetupAndRenderMolecule(cs);  
			molCtl.SetTemporaryMoleculeTag(Path.GetFileNameWithoutExtension(fileName)); // associate the file name in case saved as history/favorite
			return;
		}

		private void NewChemicalStructureMenuItem_Click(object sender, EventArgs e)
		{
			MolEditorCtl.SetupAndEditNewMoleculeAsynch(MoleculeFormat.Molfile);
		}

		private void NewBiopolymerMenuItem_Click(object sender, EventArgs e)
		{
			MolEditorCtl.SetupAndEditNewMoleculeAsynch(MoleculeFormat.Helm);
		}

		private void FromDatabaseMenuItem_Click(object sender, EventArgs e)
		{
			RetrieveDatabaseStructure(MolEditorCtl, Qc?.MetaColumn);
		}

		private void SavedStructureMenuItem_Click(object sender, EventArgs e)
		{
			RetrieveSavedMolecule(MolEditorCtl);
		}

		private void FromRecentStructure_Click(object sender, EventArgs e)
		{
			SelectMruMolecule(MolEditorCtl);
		}

		private void FromFavoriteMenuItem_Click(object sender, EventArgs e)
		{
			SelectFavoriteMolecule();
		}

		private void AddToFavoritesMenuItem_Click(object sender, EventArgs e)
		{
			AddToFavoritesList(MolEditorCtl, StructureSearchType);
		}

		private void ClearStructureMenuItem_Click(object sender, EventArgs e)
		{
			MolEditorCtl.ClearMolecule();
		}

		public static MoleculeList GetFavoriteStructuresList()
		{
			if (FavoritesList == null)
				LoadMoleculeList(FavoritesPreferencesParmName, ref FavoritesList);

			return FavoritesList;
		}

		/// <summary>
		/// Load list of recently accessed molecule objects
		/// </summary>

		public static MoleculeList GetMruMoleculesList()
		{
			if (MruList == null)
				LoadMoleculeList(MruPreferencesParmName, ref MruList);

			return MruList;
		}

		public static void LoadMoleculeList(
			string listName,
			ref MoleculeList list)
		{
			StructureListControl.LoadStructureListFromPreferences(listName, ref list);
		}

		public static void SaveStructureList(
			MoleculeList list,
			string listName)
		{
			StructureListControl.SaveStructureListToPreferences(list, listName);
		}

		/// <summary>
		/// AddToFavoritesList  
		/// </summary>
		/// <param name="molEditorCtl"></param>
		/// <param name="searchType"></param>

		public static void AddToFavoritesList(
			MoleculeControl molEditorCtl,
			StructureSearchType searchType)
		{
			MoleculeListItem sli = null;
			DialogResult dr;

			MoleculeMx mol = molEditorCtl.Molecule;
			if (MoleculeMx.IsUndefined(mol)) return;

			mol = mol.Clone(); // make a copy for the list

			sli = FindStructure(mol, GetFavoriteStructuresList());
			if (sli != null)
			{
				dr = MessageBoxMx.Show("The current molecule already exists in Favorites list under the name: " + sli.Name + "\r\n" +
					"Do you still want the add the current query molecule to the Favorites list?", "Molecule already in Favorites", MessageBoxButtons.YesNoCancel);

				if (dr != DialogResult.Yes) return;
			}

			string name = molEditorCtl.GetTemporaryStructureTag();
			name = InputBoxMx.Show("Enter the name that you want to assign to the molecule", "Enter Name", name);
			if (Lex.IsUndefined(name)) return;

			sli = new MoleculeListItem();
			sli.Name = name;
			sli.Molecule = mol;
			sli.UpdateDate = DateTime.Now;
			sli.MoleculeType = StructureSearchTypeUtil.StructureSearchTypeToExternalName(searchType);
			FavoritesList.ItemList.Insert(0, sli);

			SaveStructureList(FavoritesList, FavoritesPreferencesParmName);
			return;
		}

		public static MoleculeListItem FindStructure(
			MoleculeMx query,
			MoleculeList csl)
		{
			MoleculeListItem sli = null;

			StructureMatcher sm = new StructureMatcher();

			for (int si = 0; si < csl.Count; si++)
			{
				sli = csl[si];
				if (sli.Molecule == null || sli.Molecule.IsNull) continue;

				if (sm.IsFSSMatch(query, sli.Molecule)) return sli;
			}

			return null;
		}

		private void CloseButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}

	}
}