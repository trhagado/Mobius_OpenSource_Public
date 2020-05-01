using Mobius.CdkMx;

using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors.Repository;

namespace Mobius.ClientComponents.Dialogs
{
	public partial class DynamicWebServiceViewer : Form
	{
		public DynamicWebServiceViewer()
		{
			InitializeComponent();

		}

		public void SetData(string jsonData)
		{
			DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(Rootobject));
			MemoryStream ms = new MemoryStream(ASCIIEncoding.ASCII.GetBytes(jsonData));
			Rootobject rootObject = (Rootobject)js.ReadObject(ms);

			DataTable dt = new DataTable();

			try
			{

				StringBuilder csv = new StringBuilder();
				foreach (string columnName in rootObject.returnValue.columnNames)
				{
					if (columnName.Contains("_SMI_"))
					{
						dt.Columns.Add(columnName, typeof(Image));
					}
					else
					{
						dt.Columns.Add(columnName);
					}

					if (csv.Length > 0) csv.Append(",");
					csv.Append(columnName);
				}
				csv.Append("\n");

				int rowsadded = 0;
				int totalRows = rootObject.returnValue.rows.Length;
				CdkMx.MoleculeControl hr = new CdkMx.MoleculeControl();
				hr.Preferences.BackColor = Color.Transparent;

				int MaxDisplayRows = 100;

				StringBuilder rowSb = new StringBuilder();

				foreach (Row row in rootObject.returnValue.rows)
				{
					rowSb.Clear();

					DataRow dr = dt.NewRow();
					for (int index = 0; index < row.data.Length; index++)
					{
						string colName = rootObject.returnValue.columnNames[index];
						if (colName == null) colName = "";

						if (index > 0) rowSb.Append(",");

						object vo = row.data[index];

						if (vo == null || vo is DBNull)
						{
							dr[index] = "";
							continue;
						}

						rowSb.Append(vo.ToString());

						if (colName.Contains("_SMI_"))
						{
							string smiles = (string)vo;
							if (!string.IsNullOrEmpty(smiles) && dt.Rows.Count < MaxDisplayRows)  // avoid running out of memory with  bitmaps
							{
								//double sbl = hr.Preferences.StandardBondLength;
								try
								{
									Bitmap bm = hr.PaintMolecule(smiles, StructureType.Smiles, 200, 200);
									dr[index] = bm;
								}
								catch (Exception ex)
								{
									ComOps.DebugLog.Message(ex.Message);
									Bitmap bitmap = new Bitmap(200, 200);
									dr[index] = bitmap;
								}
								continue;
							}
							dr[index] = "";
						}

						else
						{
							dr[index] = vo;
						}

					}
					dt.Rows.Add(dr);
					csv.Append(rowSb + "n");

					rowsadded++;
					Progress.Show("Loading Matched Pairs Viewer Row: " + rowsadded + " of " + totalRows, "Row Retrieval", false);
				}

				if (true) try // also write csv file
					{
						StreamWriter sw = new StreamWriter(@"c:\download\MMPDynamicWebServiceData.csv");
						sw.Write(csv.ToString());
						sw.Close();
					}
					catch (Exception ex) { }

				gridView1.RowHeight = 200;

				foreach (string columnName in rootObject.returnValue.columnNames)
				{
					if (columnName.Contains("_SMI_"))
					{
						gridView1.Columns[columnName].ColumnEdit = new RepositoryItemPictureEdit();
						gridView1.Columns[columnName].Width = 200;
					}
				}

				gridControl1.DataSource = dt;

			}

			catch (Exception ex)
			{
				throw new Exception(ex.Message, ex);
			}

			//foreach (string columnName in rootObject.returnValue.columnNames)
			//{
			//    if (columnName.Contains("_SMI_"))
			//    {
			//        gridView1.Columns[columnName].ColumnEdit = new RepositoryItemPictureEdit();
			//    }

			//}


			//for (int index = 0; index < rootObject.returnValue.columnNames.Length; index++)
			//{
			//    //if (rootObject.returnValue.columnDataTypes[index] == "String")
			//    //{
			//        GridColumn gridColumn = new GridColumn();
			//        gridColumn.Name = rootObject.returnValue.columnNames[index];
			//        gridView1.Columns.Add(gridColumn);


			//    //}

			//}

			//for (int index = 0; index < rootObject.returnValue.rows.Length; index++)
			//{
			//    //if (rootObject.returnValue.columnDataTypes[index] == "String")
			//    //{
			//    gridView1.AddNewRow();

			//    //}

			//}

			//List<Row> myRows = rootObject.returnValue.rows.ToList();
			//gridView1.DataSource = myRows;

		}

		public static byte[] ImageToByte(Image img)
		{
			ImageConverter converter = new ImageConverter();
			return (byte[])converter.ConvertTo(img, typeof(byte[]));
		}

		public class Rootobject
		{
			public bool isSuccess { get; set; }
			public string executionLog { get; set; }
			public string errorLog { get; set; }
			public string jobDirectory { get; set; }
			public int size { get; set; }
			public Returnvalue returnValue { get; set; }
		}

		public class Returnvalue
		{
			public string[] columnNames { get; set; }
			public string[] columnLabels { get; set; }
			public string[] columnDataTypes { get; set; }
			public Row[] rows { get; set; }
			public bool truncated { get; set; }
		}

		public class Row
		{
			public object[] data { get; set; }
		}

	}
}
