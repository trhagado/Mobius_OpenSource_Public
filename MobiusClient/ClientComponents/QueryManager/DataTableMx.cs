using DevExpress.Data;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using Mobius.ClientComponents;
using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Text;

namespace Mobius.ClientComponents
{

	/// <summary>
	/// The DataTableMx class holds the set of retrieved rows that are displayed, printed, exported and imported.
	/// DataTableMx is modeled after the System.Data.DataTable.
	/// Insert performance into a DataTableMx table is about 20% faster that that of a native DataTable.
	/// The added ItemArrayRef property provides faster access to the _itemArray by using the reference rather
	/// than creating and copying a new array for each access as is the normal behavior for ItemArray.
	/// This class also supports custom functionality such as DataChangedEventHandlersEnabled.
	/// </summary>

	public class DataTableMx : IBindingList, IList, ITypedList, ICollection, IEnumerable, IDataTableMx
	{
		internal int Id = InstanceCount++; // id of this instance
		internal static int InstanceCount = 0; // instance count

		/// <summary>
		/// Name of the table
		/// </summary>

		public string TableName
		{
			get { return _tableName; }
			set { _tableName = value; }
		}
		string _tableName = "";

/// <summary>
/// The list of Columns
/// </summary>

		public DataColumnCollection Columns 
		{ 
			get {	return _columnsDataTable.Columns; }
		}
		internal DataTable _columnsDataTable = new DataTable(); // maintain native data type column collection within empty System.Data.DataTable

		public string ColumnsString // for debug
		{ get { return GetColumnsString(); } }

		public string RowsString // for debug
		{ get { return GetRowsString(); } }

		public string ColumnAndRowsString // for debug
		{ get { return GetColumnsAndRowsString(); } }

		/// <summary>
		/// The list of rows
		/// </summary>

		public RowCollectionMx Rows { get { return _rows; } }
		protected RowCollectionMx _rows = new RowCollectionMx();

		// Support use of secondary view of data that returns primitive data type values

		internal bool UseNativeDataTypes = true; // use native MobiusDataTypes
		internal bool UsePrimitiveDataTypes  // use primitive data types for this data table rather than native MobiusDataTypes
		{
			get { return !UseNativeDataTypes; }
			set { UseNativeDataTypes = !value; }
		}

		public DataTableMx PrimitiveDataTypeView // Associated DataTableMx that provides a PrimitiveDataTypeView of the data
		{
			get
			{
				if (_primitiveDataTableMx == null)
					CreateCompatiblePrimitiveDataTableMx();

				return _primitiveDataTableMx;
			}
		}
		internal DataTableMx _primitiveDataTableMx; // primitive version of table
		internal DataTableMx _nativeDataTableMx; // native verion of table

		/// <summary>
		/// CreateCompatiblePrimitiveDataTable
		/// </summary>

		void CreateCompatiblePrimitiveDataTableMx()
		{
			DataTableMx pt = new DataTableMx(); // create primitive table
			pt.UsePrimitiveDataTypes = true;

			this._primitiveDataTableMx = pt; // link this native table to the primitive table
			pt._nativeDataTableMx = this; // link primitive table to native

			foreach (DataColumn c0 in Columns) // create set of columns with "primitive" types
			{
				DataColumn dc = new DataColumn();
				dc.ColumnName = c0.ColumnName;
				dc.Caption = c0.Caption;
				foreach (string key in dc.ExtendedProperties.Keys)
					c0.ExtendedProperties.Add(key, dc.ExtendedProperties[key]);

				dc.DataType = c0.DataType;

				if (c0.ExtendedProperties.ContainsKey("MobiusDataType"))
				{ // if MobiusDataType specified then map to primitive type
					Type type = c0.ExtendedProperties["MobiusDataType"] as Type;
					if (type != null) dc.DataType = MobiusDataType.ConvertToPrimitiveType(type);
				}

				pt.Columns.Add(dc);
			}

			pt._rows = _rows; // share the same set of rows
			return;
		}

/// <summary>
/// Event handlers
/// </summary>

		public bool DataChangedEventHandlersEnabled
		{
			get { return _dataChangedEventHandlersEnabled; }
			set { _dataChangedEventHandlersEnabled = value; }
		}
		bool _dataChangedEventHandlersEnabled = true;

		bool _dataChangedEventHandlersExist // return true if any handlers exist
		{ get {
			bool exist = 
			(_listChangedEventHandlers != null && _listChangedEventHandlersCount > 0) || 
			(_rowChangedEventHandlers != null && _rowChangedEventHandlersCount > 0);
			return exist;
		}}

		protected ListChangedEventHandler _listChangedEventHandlers; // handlers to call when list item changes
		protected int _listChangedEventHandlersCount = 0; // number of handlers assigned
		public static int ListChangedHandlerCalls = 0; // (debug)
		public static int ListChangedHandlerCallErrors = 0; // (debug)

		protected DataRowMxChangeEventHandler _rowChangedEventHandlers; // handlers to call when a row changes
		protected int _rowChangedEventHandlersCount = 0; // number of handlers assigned
		public static int RowChangedHandlerCalls = 0; // (debug)

		protected ListChangedEventArgs _resetEvent = new ListChangedEventArgs(ListChangedType.Reset, -1);

/// <summary>
/// Enable / disable handlers returning old value
/// </summary>
/// <param name="value"></param>
/// <returns></returns>

		public bool EnableDataChangedEventHandlers(bool value)
		{
			bool oldValue = DataChangedEventHandlersEnabled;
			DataChangedEventHandlersEnabled = value;
			return oldValue;
		}

/// <summary>
/// Basic constructor
/// </summary>

		public DataTableMx()
		{
			Rows._dataTable = this;
			return;
		}

/// <summary>
/// Constructor with table name
/// </summary>
/// <param name="tableName"></param>

		public DataTableMx(string tableName)
		{
			TableName = tableName;
			Rows._dataTable = this;
			return;
		}

		/////////////////////////////
		/// IBindingList methods  ///
		/////////////////////////////

		/// <summary>
		/// Adds a new item to the list
		/// </summary>
		/// <returns></returns>

		object IBindingList.AddNew()
		{
			DataRowMx dr = new DataRowMx(this);
			Rows.Add(dr);
			return dr;
		}

		void IBindingList.AddIndex(PropertyDescriptor property)
		{
			throw new NotSupportedException();
		}

		void IBindingList.ApplySort(PropertyDescriptor property, ListSortDirection direction)
		{
			throw new NotSupportedException();
		}

		int IBindingList.Find(PropertyDescriptor property, object key)
		{
			throw new NotSupportedException();
		}

		void IBindingList.RemoveIndex(PropertyDescriptor property)
		{
			throw new NotSupportedException();
		}

		void IBindingList.RemoveSort()
		{
			throw new NotSupportedException();
		}

		///////////////////////////////
		/// IBindingList properties ///
		///////////////////////////////

		bool IBindingList.AllowEdit
		{
			get { return true; }
		}

		bool IBindingList.AllowNew
		{
			get { return true; }
		}

		bool IBindingList.AllowRemove
		{
			get { return true; }
		}

		bool IBindingList.SupportsChangeNotification
		{
			get { return true; }
		}

		bool IBindingList.SupportsSearching
		{
			get { return false; }
		}

		bool IBindingList.SupportsSorting
		{
			get { return false; }
		}


		bool IBindingList.IsSorted
		{
			get { throw new NotSupportedException(); }
		}

		ListSortDirection IBindingList.SortDirection
		{
			get { throw new NotSupportedException(); }
		}

		PropertyDescriptor IBindingList.SortProperty
		{
			get { throw new NotSupportedException(); }
		}

		///////////////////////////
		/// IBindingList Events ///
		///////////////////////////

		/// <summary>
		/// Add or remove a IBindingList.ListChanged event handler
		/// </summary>

		public event ListChangedEventHandler ListChanged
		{
			add
			{
				_listChangedEventHandlers += value;
				_listChangedEventHandlersCount++;
			}
			remove
			{
				_listChangedEventHandlers -= value;
				_listChangedEventHandlersCount--;
			}
		}

/// <summary>
/// Call any data changed event handlers for both the IList and DataTable interfaces
/// </summary>
/// <param name="listChangedType"></param>
/// <param name="row"></param>

		internal void CallDataChangedEventHandlers(
			ListChangedType listChangedType,
			DataRowMx row)
		{
			CallDataChangedEventHandlers(listChangedType, row, -1);
		}

/// <summary>
/// Call any data changed event handlers for both the IList and DataTable interfaces
/// </summary>
/// <param name="listChangedType"></param>
/// <param name="row"></param>
/// <param name="rowIndex"></param>

		internal void CallDataChangedEventHandlers(
			ListChangedType listChangedType,
			DataRowMx row,
			int rowIndex)
		{
			if (!_dataChangedEventHandlersEnabled ||
				!_dataChangedEventHandlersExist) return;

			if (!row._inRowList && listChangedType != ListChangedType.ItemDeleted) return; // no events if not in the list yet

			if (rowIndex < 0) // get index if not supplied
				rowIndex = Rows.GetRowIndex(row); // Note: slow

			//DebugLog.Message("CallDataChangedEventHandlers " + GetRowIndex(row));

			try
			{

				if (_listChangedEventHandlers != null && _listChangedEventHandlersCount > 0) // call any IBindingList ListChanged event handlers
				{
					ListChangedHandlerCalls++;
					ListChangedEventArgs ev = new ListChangedEventArgs(listChangedType, rowIndex);
					_listChangedEventHandlers(this, ev);
				}

				if (_rowChangedEventHandlers != null && _rowChangedEventHandlersCount > 0) // call any DataRowMx changed event handlers
				{
					RowChangedHandlerCalls++;
					DataRowAction action;
					if (listChangedType == ListChangedType.ItemAdded) action = DataRowAction.Add;
					else if (listChangedType == ListChangedType.ItemChanged) action = DataRowAction.Change;
					else if (listChangedType == ListChangedType.ItemDeleted) action = DataRowAction.Delete;
					else throw new Exception("Unsupported ListChangeType: " + listChangedType);

					DataRowMxChangeEventArgs ea = new DataRowMxChangeEventArgs(row, action);
					_rowChangedEventHandlers(this, ea);
				}
			}

			catch (Exception ex)
			{
				ListChangedHandlerCallErrors++; // ignore any errors
				return; 

				//if (Lex.Contains(ex.Message, "enumeration operation may not execute") && Lex.Contains(ex.StackTrace.ToString(), "UpdateVisibleCardCaptions()"))
				//{
				//  ListChangedHandlerCallErrors++;
				//  return; // ignore for now
				//}

				//else throw new Exception(ex.Message, ex); // pass it up
			}

			return;
		}

		/////////////////////
		/// IList members ///
		/////////////////////

		/// <summary>
		/// Gets or sets the IList element at the specified index.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>

		public object this[int index]
		{
			get { return _rows[index]; }

			set
			{
				if (!(value is DataRowMx))
					throw new Exception("Value to add must be a DataRowMx");

				DataRowMx dr = value as DataRowMx;
				_rows[index] = dr;
			}
		}

		/// <summary>
		/// Adds an item to the System.Collections.IList
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>

		public int Add(object value)
		{
			if (!(value is DataRowMx))
				throw new Exception("Value to add must be a DataRowMx");

			DataRowMx dr = value as DataRowMx;
			Rows.Add(dr);
			return Rows.Count - 1;
		}

		/// <summary>
		/// Inserts an item to the System.Collections.IList at the specified index.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="value"></param>

		public void Insert(int index, object value)
		{
			if (!(value is DataRowMx))
				throw new Exception("Value to add must be a DataRowMx");

			DataRowMx dr = value as DataRowMx;
			Rows.InsertAt(dr, index);
		}

		/// <summary>
		/// Removes the first occurrence of a specific object from the System.Collections.IList.
		/// </summary>
		/// <param name="value"></param>

		public void Remove(object value)
		{
			if (!(value is DataRowMx))
				throw new Exception("Value to add must be a DataRowMx");

			DataRowMx dr = value as DataRowMx;
			Rows.Remove(dr);
			return;
		}

		/// <summary>
		/// Removes the System.Collections.IList item at the specified index.
		/// </summary>
		/// <param name="index"></param>

		public void RemoveAt(int index)
		{
			Rows.RemoveAt(index);
		}

/// <summary>
/// Removes all items from the System.Collections.IList.
/// </summary>
		
		public void Clear()
		{
			Rows._rows.Clear();
		}

/// <summary>
/// Determines whether the System.Collections.IList contains a specific value.
/// </summary>
/// <param name="value"></param>
/// <returns></returns>

		public bool Contains(object value)
		{
			if (IndexOf(value) >= 0) return true;
			else return false;
		}

/// <summary>
/// Determines the index of a specific item in the System.Collections.IList.
/// </summary>
/// <param name="value"></param>
/// <returns></returns>

		public int IndexOf(object value)
		{
			for (int dri = 0; dri < Rows.Count; dri++)
			{
				DataRowMx dr = Rows[dri];
				if (dr == value) return dri;
			}

			return -1;
		}

/// <summary>
/// Gets a value indicating whether the System.Collections.IList has a fixed size.
/// </summary>
/// 
		public bool IsFixedSize { get { return false; } }

/// <summary>
/// Gets a value indicating whether the System.Collections.IList is read-only.
/// </summary>

		public bool IsReadOnly { get { return false; } }


		///////////////////////////
		/// ITypedList members ///
		///////////////////////////

/// <summary>
/// Get the list of properties for the DataTableMx DataColumns
/// </summary>
/// <param name="listAccessors"></param>
/// <returns></returns>

		public PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors)
		{
			//if (listAccessors != null && listAccessors.Length > 0)
			//{
			//  // Return child list shape.
			//  pdc = ListBindingHelper.GetListItemProperties(listAccessors[0].PropertyType);
			//}
			//else
			//{
			//  // Return properties in sort order.
			//  pdc = properties;
			//}

			List<PropertyDescriptor> props = new List<PropertyDescriptor>();
			foreach (DataColumn col in _columnsDataTable.Columns)
			{
				props.Add(new PropertyDescriptorMx(this, col));
			}

			PropertyDescriptor[] propArray = new PropertyDescriptor[props.Count];
			props.CopyTo(propArray);
			PropertyDescriptorCollection pdc = new PropertyDescriptorCollection(propArray);
			return pdc;
		}

/// <summary>
/// This method is only used in the design-time framework 
/// and by the obsolete DataGrid control.
/// </summary>
/// <param name="listAccessors"></param>
/// <returns></returns>

		public string GetListName(PropertyDescriptor[] listAccessors)
		{
			return TableName;
		}

		///////////////////////////
		/// ICollection members ///
		///////////////////////////

/// <summary>
/// Gets the number of elements contained in the System.Collections.ICollection.
/// </summary>

		public int Count
		{ 
			get { return Rows.Count; } 
		}

/// <summary>
/// Gets a value indicating whether access to the System.Collections.ICollection
///     is synchronized (thread safe).
/// </summary>

		public bool IsSynchronized
		{
			get { return false; }
		}

/// <summary>
/// Gets an object that can be used to synchronize access to the System.Collections.ICollection.
/// </summary>

		public object SyncRoot
		{
			get { return null; }
		}

/// <summary>
/// Copies the elements of the System.Collections.ICollection to an System.Array,
/// starting at a particular System.Array index.
/// </summary>
/// <param name="array"></param>
/// <param name="index"></param>

		public void CopyTo(Array array, int index)
		{
			throw new NotImplementedException();
		}

		///////////////////////////
		/// IEnumerator members ///
		///////////////////////////

	/// <summary>
	/// Returns an IEnumerator that can iterate through a collection.
	/// </summary>
	/// <returns></returns>

		public IEnumerator GetEnumerator()
		{
			return Rows.GetEnumerator();
		}

/// <summary>
/// OnClear
/// </summary>

		//protected override void OnClear()
		//{
		//  throw new NotImplementedException();
		//}

/// <summary>
/// OnClearComplete
/// </summary>

		//protected override void OnClearComplete()
		//{
		//  OnListChanged(_resetEvent);
		//}

/// <summary>
/// OnInsertComplete
/// </summary>
/// <param name="index"></param>
/// <param name="value"></param>

		//protected override void OnInsertComplete(int index, object value)
		//{
		//  OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, index));
		//}

/// <summary>
/// OnRemoveComplete
/// </summary>
/// <param name="index"></param>
/// <param name="value"></param>

		//protected override void OnRemoveComplete(int index, object value)
		//{
		//  OnListChanged(new ListChangedEventArgs(ListChangedType.ItemDeleted, index));
		//}

/// <summary>
/// OnSetComplete
/// </summary>
/// <param name="index"></param>
/// <param name="oldValue"></param>
/// <param name="newValue"></param>

		//protected override void OnSetComplete(int index, object oldValue, object newValue)
		//{
		//  if (oldValue != newValue)
		//  {

		//    DataRowMx oldRow = (DataRowMx)oldValue;
		//    DataRowMx newRow = (DataRowMx)newValue;

		//    oldRow.Table = null;
		//    newRow.Table = this;

		//    OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, index));
		//  }
		//}

		////////////////////////////////////////////////
		/// Other DataTable-like methods & properties ///
		////////////////////////////////////////////////

		/// <summary>
		/// Creates a new DataRowMx with the same schema as the table.
		/// </summary>
		/// <returns></returns>

		public DataRowMx NewRow()
		{
			DataRowMx dr = new DataRowMx(this);
			dr.RowState = DataRowState.Detached;

			return dr;
		}

		/// <summary>
		/// Add or remove a DataRowMxChange event handler
		/// </summary>

		public event DataRowMxChangeEventHandler RowChanged
		{
			add
			{
				_rowChangedEventHandlers += value;
				_rowChangedEventHandlersCount++;
			}
			remove
			{
				_rowChangedEventHandlers -= value;
				_rowChangedEventHandlersCount--;
			}
		}

		public string GetColumnsAndRowsString()
		{
			string s = GetColumnsString() + "\r\n\r\n" + GetRowsString();
			return s;
		}

		public string GetColumnsString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("DataTableId: " + Id + ", Col Count: " + Columns.Count + "\r\n");
			sb.Append("Index\tName\tType\r\n");

			for (int ci = 0; ci < Columns.Count; ci++)
			{
				DataColumn c = Columns[ci];
				sb.Append(ci.ToString() + "\t" + c.ColumnName + "\t" + c.DataType.ToString() + "\r\n");
			}

			sb.Append("\r\n");
			return sb.ToString();
		}

		public string GetRowsString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("DataTableId: " + Id + ", Col Count: " + Columns.Count + ", Row Count: " + Rows.Count + "\r\n");
			sb.Append("\r\n");

			for (int ri = 0; ri < Rows.Count; ri++)
			{
				if (ri < 10 || Rows.Count == 11 || ri >= Rows.Count - 10)
				{
					DataRowMx r = Rows[ri];
					sb.Append("Row: " + ri.ToString() + "\r\n");
					for (int voi = 0; voi < r.Length; voi++)
						{
					}
					sb.Append("\r\n");
				}

				else if (ri == 10)
					sb.Append("...\r\n");

			}

			sb.Append("\r\n");
			return sb.ToString();
		}



	} // DataTableMx

	/// <summary>
	/// Creation and processing of descriptors for properties corresponding to
	/// the set of columns and values in the DataTableMx and DataRowMx
	/// </summary>

	public class PropertyDescriptorMx : PropertyDescriptor
	{
		protected DataTableMx _dataTable; // the associated DataTable
		protected DataColumn _dataColumn; // the associated DataColumn

/// <summary>
/// Create the PropertyDescriptor saving a ref to the associated DataColumn
/// </summary>
/// <param name="table"></param>
/// <param name="column"></param>

		public PropertyDescriptorMx(
			DataTableMx table,
			DataColumn column) :
			 base(column.ColumnName, null)
		{
			_dataTable = table;
			_dataColumn = column;
		}

		/// <summary>
		/// Gets the value of the associated ItemArray entry
		/// </summary>
		/// <param name="component"></param>
		/// <returns></returns>

		public override object GetValue(object component)
		{
			DataRowMx row = component as DataRowMx;
			return row[_dataColumn.Ordinal];
		}

		/// <summary>
		/// Set the value of the associated ItemArray entry
		/// </summary>
		/// <param name="component"></param>
		/// <param name="value"></param>

		public override void SetValue(object component, object value)
		{
			DataRowMx row = (DataRowMx)component;
			row[_dataColumn.Ordinal] = value;
			_dataTable.CallDataChangedEventHandlers(ListChangedType.ItemChanged, row); 
		}

		/// <summary>
		/// Get the type of the containing DataRowMx
		/// </summary>

		public override Type ComponentType
		{
			get { return typeof(DataRowMx); }
		}

		/// <summary>
		/// Get the type of the DataColumn
		/// </summary>

		public override Type PropertyType
		{
			get { return _dataColumn.DataType; }
		}

		/// <summary>
		/// CanResetValue
		/// </summary>
		/// <param name="component"></param>
		/// <returns></returns>

		public override bool CanResetValue(object component)
		{
			return false;
		}

		/// <summary>
		/// ResetValue
		/// </summary>
		/// <param name="component"></param>

		public override void ResetValue(object component)
		{
			return;
		}
		/// <summary>
		/// IsReadOnly
		/// </summary>

		public override bool IsReadOnly
		{
			get { return false; }
		}

		/// <summary>
		/// ShouldSerializeValue
		/// </summary>
		/// <param name="component"></param>
		/// <returns></returns>

		public override bool ShouldSerializeValue(object component)
		{
			return false;
		}
	} // PropertyDescriptorMx

	/// <summary>
	/// Delegate for row change
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>

	public delegate void DataRowMxChangeEventHandler(object sender, DataRowMxChangeEventArgs e);

/// <summary>
/// Args for DataRowMxChange event
/// </summary>

	public class DataRowMxChangeEventArgs : EventArgs
	{

		/// <summary>
		/// Construct
		/// </summary>
		/// <param name="row"></param>
		/// <param name="action"></param>

		public DataRowMxChangeEventArgs(DataRowMx row, DataRowAction action)
		{
			Row = row;
			Action = action;
		}

		/// <summary>
		///  Gets the action that has occurred on a System.Data.DataRow.
		/// </summary>

		public DataRowAction Action;

		/// <summary>
		/// Gets the row upon which an action has occurred
		/// </summary>

		public DataRowMx Row;

		/// <summary>
		/// Used to lookup members for development
		/// </summary>

		void MemberLookupForDevPurposes() // 
		{
			DataTable dt = null;
			DataRow dr = dt.NewRow();
			int count = dt.Rows.Count;
			object[] ia = dt.Rows[0].ItemArray;
			//dt.RowChanged += new DataRowChangeEventHandler(DataRowChangeEventHandler);
		}


	} // DataRowMxChangeEventArgs

/// <summary>
/// The collection of DataRowMx rows
/// </summary>

	public class RowCollectionMx : IEnumerable
	{

		internal DataTableMx _dataTable; // associated DataTableMx

		public static int RowIndexOfCallCount = 0; 

/// <summary>
/// The internal list of rows
/// </summary>

		internal List<DataRowMx> _rows = new List<DataRowMx>(); // the list of rows

/// <summary>
/// Gets the row at the specified position
/// </summary>
/// <param name="index"></param>
/// <returns></returns>

		public DataRowMx this[int index]
		{
			get 
			{
				if (index < 0 || index >= _rows.Count) return null; // out of range
				return _rows[index]; 
			}

			set 
			{ 
				_rows[index] = value; 
			}
		}

/// <summary>
/// Gets the total number of System.Data.DataRow objects in this collection.
/// </summary>

		public int Count
		{ get { return _rows.Count; } }

		/// <summary>
		/// Returns an IEnumerator that can iterate through a collection.
		/// </summary>
		/// <returns></returns>

		public IEnumerator GetEnumerator()
		{
			return _rows.GetEnumerator();
		}

		/// <summary>
		/// Creates a row using specified values and adds it to the System.Data.DataRowCollection.
		/// </summary>
		/// <param name="values"></param>
		/// <returns></returns>

		public DataRowMx Add(params object[] values)
		{
			DataRowMx dr = new DataRowMx(_dataTable);
			dr.ItemArray = values;
			Add(dr);
			return dr;
		}

		/// <summary>
		/// Adds the specified DataRowMx to the DataRowMxCollection
		/// </summary>
		/// <param name="row"></param>

		public void Add(DataRowMx row)
		{
			_rows.Add(row);
			int rowIndex = _rows.Count - 1;
			row._inRowList = true; // set the flag that the row is now in the list
			row.RowState = DataRowState.Added;

			_dataTable.CallDataChangedEventHandlers(ListChangedType.ItemAdded, row, rowIndex);
			return;
		}

		/// <summary>
		/// Insert row at specified position
		/// </summary>
		/// <param name="row"></param>
		/// <param name="pos"></param>

		public void InsertAt(DataRowMx row, int pos)
		{
			_rows.Insert(pos, row);
			row._inRowList = true;
			row.RowState = DataRowState.Added;

			_dataTable.CallDataChangedEventHandlers(ListChangedType.ItemAdded, row, pos);
		}

/// <summary>
/// Removes the specified DataRowMx from the collection.
/// </summary>
/// <param name="row"></param>

		public bool Remove(DataRowMx row)
		{
			int rowIndex = GetRowIndex(row);
			if (rowIndex < 0) return false;

			RemoveAt(rowIndex);
			row.RowState = DataRowState.Detached;

			return true;
		}

		public int GetRowIndex(DataRowMx row)
		{
			int rowIndex = _rows.IndexOf(row); // Note: slow
			RowIndexOfCallCount++;
			return rowIndex;
		}

/// <summary>
/// Removes the row at the specified index from the collection.
/// </summary>
/// <param name="index"></param>

		public void RemoveAt(int rowIndex)
		{
			DataRowMx row = _rows[rowIndex];
			_rows.RemoveAt(rowIndex);
			row._inRowList = false;
			row.RowState = DataRowState.Detached;
			_dataTable.CallDataChangedEventHandlers(ListChangedType.ItemDeleted, row, rowIndex);
			return;
		}

	} // RowCollectionMx 

}
