using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.BandedGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraGrid.Dragging;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.Remoting.Messaging;

namespace Mobius.ClientComponents
{
	/// <summary>
	/// Handle OnCustomUnboundColumnData calls from the grid to get and set data
	/// </summary>

	public partial class MoleculeGridHelpers : Form
	{

		private delegate GetImageBitmapArgs GetImageBitmapDelegate(CellInfo ci, DataRowMx dr, int listSourceRowIndex, object fieldValue, ImageMx imageMx, int callId);

		/// <summary>
		/// Handle conversion of Mobius custom data types between the grid and the underlying DataSet
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		public void OnCustomUnboundColumnData(object sender, CustomColumnDataEventArgs e)
		{
			try
			{
				if (InCustomUnboundColumnData)
				{
					if (DebugDetails) // check for reentry which can cause display issues
						ClientLog.Message("CustomUnboundColumnData reentry: " + new StackTrace(true));
					//return;
				}

				InCustomUnboundColumnData = true;
				ProcessCustomUnboundColumnDataEvent(sender, e);
			}

			catch (Exception ex)
			{ ClientLog.Message(DebugLog.FormatExceptionMessage(ex)); }

			InCustomUnboundColumnData = false;
			return;
		}

		/// <summary>
		/// Handle conversion of Mobius custom data types between the grid and the underlying DataSet
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		void ProcessCustomUnboundColumnDataEvent(object sender, CustomColumnDataEventArgs e)
		{
			MobiusDataType mdt = null;
			FormattedFieldInfo ffi = null;
			bool formatted = false;
			NumberMx numberEx;
			StringMx stringEx;
			ImageMx imageMx;
			DateTimeMx dateTimeEx;
			DataRowMx dr = null;
			BandedGridView mbgv;
			int nonNullDri;
			string debugMsg;

			if (DebugMx.False) // fillValuesImmediately debug test
			{
				e.Value = "XXX";
				return;
			}

			DateTime t0 = DateTime.Now;
			int callId = ++UnboundCalls; // get id for this call

			if (sender is BandedGridView)
				mbgv = sender as BandedGridView;

			if (Grid.DataSource == null) return;
			CellInfo ci = Grid.GetDataTableCellInfo(e.ListSourceRowIndex, e.Column);
			//if (e.ListSourceRowIndex == 2 && ci.DataRowIndex == 1) e = e; // debug
			//if (ci.Mc.DataType == MetaColumnType.CompoundId && ci.DataRowIndex > 0) ci = ci; // debug
			//DebugLog.Message("CustomUnboundColumnData " + ci.DataRowIndex + ", " + ci.DataColIndex + ", " + ci.DataValue.ToString());
			if (ci.Rfld == null) return;

			QueryManager qm = Grid.QueryManager;
			ResultsFormatter fmtr = qm.ResultsFormatter;
			ResultsFormat rf = qm.ResultsFormat;

			if (e.ListSourceRowIndex == GridControl.NewItemRowHandle || e.ListSourceRowIndex >= Qm.DataTable.Rows.Count) // new row being created
			{
				if (NewRow == null)
				{
					NewRow = Qm.DataTable.NewRow();
				}
				dr = NewRow;
			}

			else if (e.ListSourceRowIndex >= 0 && ci.DataRowIndex >= 0) // row exist in DataTable (i.e. not new row)
			{
				AdjustDataRowToRender(ci);
				if (ci.DataRowIndex >= 0)
				{
					dr = DataTable.Rows[ci.DataRowIndex];
					if (ci.DataRowIndex == 0)
					{
					}
				}
			}

			else
			{
				if (DebugDetails) ClientLog.Message("Fail 1");
				return; // something else, ignore
			}

			// Store edited data for unbound column in DataTable

			if (e.IsSetData)
			{
				SetGridData(e, ci);
				return;
			}

			else if (!e.IsGetData) return; // just return if not GetData as expected


// Get data from underlying unbound dataset & return in format for grid

			//if (ci.Mc.DataType == MetaColumnType.Structure) ci = ci; // debug
			//if (ci.Mc.DataType == MetaColumnType.String) ci = ci; // debug
			//if (ci.DataValue is StringEx && ((StringEx)ci.DataValue).Value == " ") ci = ci; // debug

			UnboundGets++;

			if (dr == null)
			{
				e.Value = null;
				if (DebugDetails) ClientLog.Message("Fail 2"); // debug
				return;
			}

			if (Dtm.IsRetrievingDataMessageRow(dr) && ci.Mc != null && !ci.Mc.IsGraphical)
			{
				//if (ci.Mc.IsKey) // show retrieving data message for key field
					e.Value = "Retrieving data...";

				return;
			}

			object fieldValue = dr[ci.DataColIndex];

			if (DebugDetails && fieldValue is MoleculeMx) // debug
			{
				MoleculeMx cs = fieldValue as MoleculeMx;
				MoleculeFormat csType = cs.PrimaryFormat;
				string csValue = cs.PrimaryValue;
				int csLen = cs.PrimaryValue.Length;
				string molfile = cs.GetMolfileString();
				molfile = molfile;
			}

			try
			{

				// If already formatted use existing formatting info

				ffi = null;
				if (fieldValue is MobiusDataType)
				{
					mdt = (MobiusDataType)fieldValue;
					if (mdt.FormattedBitmap != null)
					{
						ffi = new FormattedFieldInfo();
						ffi.FormattedBitmap = mdt.FormattedBitmap;
					}

					else if (mdt.FormattedText != null)
					{
						ffi = new FormattedFieldInfo();
						ffi.FormattedText = mdt.FormattedText;
					}

					if (ffi != null) // if formatted then copy other format attributes as well
					{
						ffi.BackColor = mdt.BackColor;
						ffi.ForeColor = mdt.ForeColor;
						ffi.Hyperlink = mdt.Hyperlink;
					}
				}

				// If not formatted then format

				if (ffi == null) // need to format?
				{

// Format non-image field (including structures)

					if (ci.Mc.DataType != MetaColumnType.Image) // format other than image immediately
					{
						//if (ci.Mc.DataType == MetaColumnType.Structure)  // debug
						//{
						//	DebugLog.Message(fieldValue.ToString());
						//	UIMisc.Beep();
						//}

						ffi = fmtr.FormatField(ci.Rt, ci.TableIndex, ci.Rfld, ci.FieldIndex, dr, e.ListSourceRowIndex, fieldValue, ci.DataRowIndex, false);

						FormatFieldCalls++;
						if (ci.Mc.DataType == MetaColumnType.Structure)
							FormatStructureFieldCalls++;

						StoreFormattingInformationInMdt(ffi, fieldValue, dr, ci, mdt);
						formatted = true;
					}

// Image: start asynch call to get image in background as necessary since it is too slow to wait for it here

					else 
					{
						if (fieldValue is ImageMx)
							imageMx = fieldValue as ImageMx;

						else
						{
							imageMx = new ImageMx();
							if (fieldValue != null)
							{
								if (fieldValue is MobiusDataType) imageMx.DbLink = (fieldValue as MobiusDataType).DbLink;
								else imageMx.DbLink = fieldValue.ToString(); // store field value as dblink
							}
							dr.ItemArrayRef[ci.DataColIndex] = imageMx; // store new image object without firing event
						}

						mdt = imageMx; // copy image object to general  MobiusDataType
						ffi = new FormattedFieldInfo();

						if (imageMx.FormattedBitmap != null) // already have bitmap?
							ffi.FormattedBitmap = imageMx.FormattedBitmap;

						else if (imageMx.Value != null) // have the bitmap, just need to scale it
						{
							int fieldWidth = ci.Rfld.FieldWidth; // current field width in milliinches
							int desiredWidth = (int)((fieldWidth / 1000.0) * GraphicsMx.LogicalPixelsX * 1.0); // width in pixels 

							imageMx.FormattedBitmap = BitmapUtil.ScaleBitmap(imageMx.Value, desiredWidth);
							ffi.FormattedBitmap = imageMx.FormattedBitmap;
						}

						else if (imageMx.IsRetrievingValue) // already retrieving?
							ffi.FormattedBitmap = (Bitmap)RetrievingImageMsg.Image; // put up the processing image

						else if (SS.I.AsyncImageRetrieval) // start async image retrieval
						{
							FormatImageFieldCalls++;
							GetImageBitmapAsync(ci, dr, e.ListSourceRowIndex, fieldValue, imageMx, callId);
							ffi.FormattedBitmap = (Bitmap)RetrievingImageMsg.Image; // put up the processing image
						}

						else // do synchronous image retrieval
						{
							GetImageBitmap(ci, dr, e.ListSourceRowIndex, fieldValue, imageMx, callId);
							ffi.FormattedBitmap = imageMx.FormattedBitmap;
							formatted = true;
						}
					}
				}

				//if (ci.Mc.DataType == MetaColumnType.CompoundId && String.IsNullOrEmpty(fmtdFld.Hyperlink)) ci = ci; // debug
				//if (mdt is CompoundId) mdt = mdt; // debug

				if (e.Column.ColumnEdit is RepositoryItemPictureEdit)
				{
						if (ffi != null && ffi.FormattedBitmap != null) e.Value = ffi.FormattedBitmap;
						else e.Value = new Bitmap(1, 1); // avoid no-image data message

						//ffi.FormattedBitmap.Save(@"c:\download\test.bmp"); // debug
				}

				else e.Value = ffi.FormattedText; // non-picture column

				if (ci.DataRowIndex == DataTable.Rows.Count - 1 && // if at end of DataTable && more rows available, request them
				 !Dtm.RowRetrievalComplete)
				{
					//Progress.Show("Retrieving data..."); // put up progress dialog if not already up

					//if (WaitForMoreDataStartTime.Equals(DateTime.MinValue)) // say we've started waiting for data
					//{
					//  WaitForMoreDataStartTime = DateTime.Now;
					//  ClientLog.Message("Set WaitForMoreDataStartTime: " + WaitForMoreDataStartTime.ToLongTimeString());
					//}

					if (Dtm.RowRetrievalState == RowRetrievalState.Paused)
						Dtm.StartRowRetrieval(); // .ReadNextRowsFromQueryEngine(); // restart retrieval
				}

				else if (Lex.StartsWith(Progress.GetCaption(), "Retrieving data...") || Lex.IsUndefined(Progress.GetCaption()))
				{ // hide any "Retrieving data..." message
					Progress.Hide();
					//SystemUtil.Beep();
				}

				Grid.LastRowRendered = e.ListSourceRowIndex;

				if (DebugDetails)
				{
					debugMsg =
						"Grid.GetData: " + callId + ", e.Row = " + e.ListSourceRowIndex +
						", e.Col = " + e.Column.AbsoluteIndex + ", Formatted = " + (formatted ? "T" : "F") + ", Time(ms) = " + TimeOfDay.Delta(t0) +
						", ColLabel = " + ci.Qc.ActiveLabel;
					debugMsg += ", FieldValue = ";
					if (fieldValue != null) debugMsg += fieldValue.ToString();
					debugMsg += ", e.Value = ";
					if (e.Value != null) debugMsg += e.Value.ToString();
					else debugMsg += "null";
					ClientLog.Message(debugMsg);
				}

				// TODO: This does a some unnecessary hides which cause flashing of the window frame
				// This happens when we are not at the end of the DataTable but don't know if any additional requests
				// for rendering will occur. May be better to move this to DataTableManger when we detect
				// that we have retrieved a row that is below the level of those displayed in the grid or all rows have been retrieved.
				// Also maybe in MoleculeGridControl.RetrievalMonitorTimer_Tick
			}

			catch (Exception ex)
			{
				if (e.Column.ColumnEdit is RepositoryItemPictureEdit)
					e.Value = new Bitmap(1, 1); // avoid no-image data message
				else e.Value = ex.Message;

				string msg = "ColumnView_CustomUnboundColumnData Exception";
				if (ci.Rfld != null) msg += ",  MetaColumn: " + ci.Rfld.MetaColumn.MetaTable.Name + "." + ci.Rfld.MetaColumn.Name;
				msg += ",  DataColIndex: " + ci.DataColIndex + ",  DataRowIndex: " + ci.DataRowIndex;
				if (fieldValue != null) msg += ",  Value: " + fieldValue.ToString();
				ClientLog.Message(msg);
			}


			//			t0 = TimeOfDay.Milliseconds() - t0;
			//			ClientLog.Message("CustomUnboundColumnData event time: " + t0);

		}

		/// <summary>
		/// Store edited data for unbound column in DataTable
		/// </summary>
		/// <param name="e"></param>
		/// <param name="ci"></param>

		void SetGridData(CustomColumnDataEventArgs e, CellInfo ci)
		{
			UnboundSets++;
			if (DataTableManager.DebugDetails)
			{
				ClientLog.Message("SetData");
				//SystemUtil.Beep();
			}

			Grid.GetEditor().CellTextEdited(e, ci);
			return;
		}

/// <summary>
/// Start async call to get image if not already in progress
/// </summary>
/// <param name="ci"></param>
/// <param name="dr"></param>
/// <param name="listSourceRowIndex"></param>
/// <param name="fieldValue"></param>
/// <param name="imageMx"></param>
/// <param name="callId"></param>

		private void GetImageBitmapAsync(CellInfo ci, DataRowMx dr, int listSourceRowIndex, object fieldValue, ImageMx imageMx, int callId)
		{

			if (!imageMx.IsRetrievingValue) // start retrieve if not already started
			{
				//if (Debug) 
					ClientLog.Message("Dispatching async callId: " + callId);

				imageMx.IsRetrievingValue = true;
				GetImageBitmapDelegate d = new GetImageBitmapDelegate(GetImageBitmap);
				d.BeginInvoke(ci, dr, listSourceRowIndex, fieldValue, imageMx, callId, new AsyncCallback(ImageBitmapRetrieved), null);
				return;
			}

			else return;
		}

/// <summary>
/// Method to get bitmap 
/// </summary>
/// <param name="index"></param>
/// <returns></returns>

		private GetImageBitmapArgs GetImageBitmap(CellInfo ci, DataRowMx dr, int listSourceRowIndex, object fieldValue, ImageMx imageMx, int callId)
		{
			DateTime t0 = DateTime.Now;

			QueryManager qm = Grid.QueryManager;
			ResultsFormatter fmtr = qm.ResultsFormatter;

			FormattedFieldInfo ffi = fmtr.FormatField(ci.Rt, ci.TableIndex, ci.Rfld, ci.FieldIndex, dr, listSourceRowIndex, fieldValue, ci.DataRowIndex, false);
			if (ffi.FormattedBitmap == null) ffi.FormattedBitmap = new Bitmap(1, 1);
			imageMx.FormattedBitmap = ffi.FormattedBitmap; // store ref to bitmap
			imageMx.Value = imageMx.FormattedBitmap; // value is also the bitmap

			imageMx.IsRetrievingValue = false; // retrieve is complete

			StoreFormattingInformationInMdt(ffi, fieldValue, dr, ci, imageMx);

			//if (Debug) 
				ClientLog.Message("GetImageBitmap complete for callId: " + callId + ", Time(ms) = " + TimeOfDay.Delta(t0));

			GetImageBitmapArgs a = new GetImageBitmapArgs();
			a.ci = ci;
			a.dr = dr;
			a.listSourceRowIndex = listSourceRowIndex;
			a.fieldValue = fieldValue;
			a.imageMx = imageMx;
			a.callId = callId;
			return a;
		}

/// <summary>
/// Store selected formatting information for possible use later needed later
/// </summary>
/// <param name="ffi"></param>
/// <param name="fieldValue"></param>
/// <param name="dr"></param>
/// <param name="ci"></param>
/// <param name="mdt"></param>

		void StoreFormattingInformationInMdt(FormattedFieldInfo ffi, object fieldValue, DataRowMx dr, CellInfo ci, MobiusDataType mdt)
		{
			bool hyperLinked = !String.IsNullOrEmpty(ffi.Hyperlink);
			if (ffi.ForeColor != Color.Black || ffi.BackColor != Color.Empty || hyperLinked)
			{ // store any formatting info in a MDT so that it's available for the later display RowCellStyle event
				if (mdt == null && !NullValue.IsNull(fieldValue)) // need to create MDT?
				{
					//if (ci.Mc.DataType == MetaColumnType.Structure) ci = ci; // debug
					mdt = MobiusDataType.New(ci.Mc.DataType, fieldValue);
					dr.ItemArrayRef[ci.DataColIndex] = mdt; // store new value without firing event
				}

				if (mdt.BackColor != ffi.BackColor) mdt.BackColor = ffi.BackColor; // set in DataTable only if changed to avoid repaints
				if (mdt.ForeColor != ffi.ForeColor) mdt.ForeColor = ffi.ForeColor;
				if (mdt.Hyperlink != ffi.Hyperlink) mdt.Hyperlink = ffi.Hyperlink;
				if (mdt.Hyperlinked != hyperLinked) mdt.Hyperlinked = hyperLinked;

				if (ffi.FormattedBitmap != null) mdt.FormattedBitmap = ffi.FormattedBitmap;
				if (ffi.FormattedText != null) mdt.FormattedText = ffi.FormattedText;
			}
		}

/// <summary>
/// Callback after image is retrieved refreshes the grid with the new value
/// </summary>
/// <param name="r"></param>

		void ImageBitmapRetrieved(IAsyncResult r)
		{
			GetImageBitmapDelegate d = (r as AsyncResult).AsyncDelegate as GetImageBitmapDelegate;

			GetImageBitmapArgs args = d.EndInvoke(r); // end invoke and get the handle of the row that has been updated

			RefreshGridForImageValue();
			if (DebugDetails) ClientLog.Message("ImageBitmapRetrieved callback called for callId: " + args.callId);
		}

/// <summary>
/// Refresh grid to show current image bitmaps
/// </summary>

		void RefreshGridForImageValue()
		{
			try
			{
				if (Grid.InvokeRequired)
					Grid.Invoke(new MethodInvoker(delegate
					{
						Grid.MainView.LayoutChanged();
					}));

				else
					Grid.MainView.LayoutChanged();

				// Other possible refresh methods
				//Grid.BGV.RefreshRow(args.ci.GridRowHandle);
				//Grid.MainView.RefreshData();
				//Grid.RefreshDataSource();

			}

			catch (Exception ex) { ex = ex; }

			return;
		}

	}

/// <summary>
/// GetImageBitmapArgs
/// </summary>

	class GetImageBitmapArgs
	{
		internal CellInfo ci;
		internal DataRowMx dr;
		internal int listSourceRowIndex;
		internal object fieldValue;
		internal ImageMx imageMx;
		internal int callId;
	}
}