using Mobius.ComOps;

using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.BandedGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraEditors.ViewInfo;
using DevExpress.XtraGrid.Views.Layout.ViewInfo;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraGrid.Views.BandedGrid.ViewInfo;
using DevExpress.Data;
using DevExpress.Utils;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Mobius.ComOps
{
	public partial class BitmapTest : DevExpress.XtraEditors.XtraForm
	{

		DataTable FieldDataTable;
		bool InSetup = false;

		public BitmapTest()
		{
			InitializeComponent();

			SetupGrid();
		}

	/// <summary>
	/// SetupFieldGrid
	/// </summary>
	internal void SetupGrid()
		{
			InSetup = true;
			DataRow dr;
			DataColumn dc;
			RepositoryItemImageComboBox riicb;

			DataTable dt = new DataTable();
			dt.Columns.Add(new DataColumn("RowIdField", typeof(int)));

			dt.Columns.Add(new DataColumn("ColorSetField", typeof(int))); // value is index of item in combo box items list
			riicb = ColorSetImageComboBoxRepositoryItem;
			riicb.LargeImages = Bitmaps.I.ColorSetImages; // DevExpress.Utils.ImageCollection (appears System.Windows.Forms.ImageList doesn't work)

			dt.Columns.Add(new DataColumn("ColorScaleField", typeof(int)));
			riicb = ColorScaleImageComboBoxRepositoryItem;
			riicb.LargeImages = Bitmaps.I.ColorScaleImages;

			dt.Columns.Add(new DataColumn("DataBarsField", typeof(int)));
			riicb = DataBarsImageComboBoxRepositoryItem;
			riicb.LargeImages = Bitmaps.I.DataBarsImages;

			dt.Columns.Add(new DataColumn("IconSetField", typeof(int)));
			riicb = IconSetImageComboBoxRepositoryItem;
			riicb.SmallImages = Bitmaps.I.IconSetImages;

			dt.Columns.Add(new DataColumn("PictureEditField", typeof(Image))); // System.Drawing.Image seems to work here

			for (int i1 = 1; i1 < Bitmaps.Bitmaps16x16.Images.Count; i1++)
			{
				dr = dt.NewRow();

				dr["RowIdField"] = i1;

				if (i1 < Bitmaps.I.ColorSetImages.Images.Count)
				{
					dr["ColorSetField"] = i1; // just store combo box item index
					riicb = ColorSetImageComboBoxRepositoryItem;
					riicb.Items.Add(new ImageComboBoxItem(i1)); // also create the combo box item
				}

				if (i1 < Bitmaps.I.ColorScaleImages.Images.Count)
				{
					dr["ColorScaleField"] = i1;
					riicb = ColorScaleImageComboBoxRepositoryItem;
					riicb.Items.Add(new ImageComboBoxItem(i1));
				}

				if (i1 < Bitmaps.I.DataBarsImages.Images.Count)
				{
					dr["DataBarsField"] = i1;
					riicb = DataBarsImageComboBoxRepositoryItem;
					riicb.Items.Add(new ImageComboBoxItem(i1));
				}

				if (i1 < Bitmaps.I.IconSetImages.Images.Count)
				{
					dr["IconSetField"] = i1;
					riicb = IconSetImageComboBoxRepositoryItem;
					riicb.Items.Add(new ImageComboBoxItem(i1));
				}

				dr["PictureEditField"] = Bitmaps.Bitmaps16x16.Images[i1];

				dt.Rows.Add(dr);
			}

			FieldDataTable = dt;
			FieldGrid.DataSource = dt;

			InSetup = false;
			return;
		}

		private void GridView_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
		{

			if (e.Column.FieldName == "IconSetField")
			{
				var vi = (e.Cell as GridCellInfo).ViewInfo as ImageComboBoxEditViewInfo;
				if (vi == null) return;

				GridColumn gc = IconSetGridColumn;

				RepositoryItemImageComboBox riicb = ColorSetImageComboBoxRepositoryItem;
				int ii = vi.ImageIndex;
				object images = vi.Images;
				System.Drawing.Size imageSize = vi.ImageSize;
				return;
			}

			else if (e.Column.FieldName == "PictureEditField")
			{
				var vi2 = (e.Cell as GridCellInfo).ViewInfo as PictureEditViewInfo;
				if (vi2 == null) return;

				GridColumn gc = PictureEditColumn;

				RepositoryItemPictureEdit ripe = BitmapsPictureEditRepositoryItem;

				if (vi2.Image == null)
					vi2.Image = Bitmaps.I.IconSetImages.Images[0];
				return;
			}


		}

	}

}