using Mobius.ComOps;
using Mobius.Data;
using Mobius.ClientComponents;

using Mobius.SpotfireDocument;
using Mobius.SpotfireClient;

using DevExpress.XtraEditors;
using DevExpress.XtraTab;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mobius.SpotfireClient
{
	public partial class DataMapOptionsDialog : DevExpress.XtraEditors.XtraForm
	{

		DataTableMapMsx DataMap;  // associated DataMap
		SpotfireViewProps SVP => DataMap?.SVP; // view manager associated with this dialog
		DataTableMapsMsx DataTableMaps => SVP?.DataTableMaps;

		DataTableMapsMsx DtMaps => DataTableMaps; // alias

		public static QueryTable QueryTable = null;

		public static bool WriteSingleDataFile = true;

		public static bool SummarizationOneRowPerKey = true;

		bool InSetup = false;

		public DataMapOptionsDialog()
		{
			InitializeComponent();
		}

		/// <summary>
		/// ShowDialog
		/// </summary>
		/// <param name="dataMap"></param>
		/// <param name="initialTabName"></param>
		/// <returns></returns>

		public static DialogResult ShowDialog(
			DataTableMapMsx dataMap)
		{
			DataMapOptionsDialog dmd = new DataMapOptionsDialog();
			dmd.DataMap = dataMap;

			dmd.Setup();

			return dmd.ShowDialog();
		}

		void Setup()
		{
			InSetup = true;

			//ExportSingleFileCheckEdit.Checked = DtMaps.WriteSingleDataFile;
			//ExportMultipleFilesCheckEdit.Checked = DtMaps.WriteMultipleDataFiles;

			QtSelectorControl.Setup(DataMap);
			QueryTable = null;

			//SummarizationOneRowPerKeyCheckEdit.Checked = DtMaps.SummarizationOneRowPerKey;
			//SummarizationAsIsCheckEdit.Checked = DtMaps.SummarizationAsIs;

			QueryTable = DataMap.QueryTable;

			InSetup = false;

			return;
		}

		private void OKButton_Click(object sender, EventArgs e)
		{
			QueryTable = QtSelectorControl.SelectedQueryTable;

			WriteSingleDataFile = ExportSingleFileCheckEdit.Checked;

			//SummarizationOneRowPerKey = SummarizationOneRowPerKeyCheckEdit.Checked;

			DialogResult = DialogResult.OK;
			return;
		}

	}
}
