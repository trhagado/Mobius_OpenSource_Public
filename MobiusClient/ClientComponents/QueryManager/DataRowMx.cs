using Mobius.ComOps;
using Mobius.Data;
using Mobius.ClientComponents;
using Mobius.ServiceFacade;

using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Columns;
using DevExpress.Data;

using System;
using System.IO;
using System.Text;
using System.Data;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;

namespace Mobius.ClientComponents
{
	/// <summary>
	/// DataRow object
	/// </summary>

	public class DataRowMx : IEditableObject
	{
		internal object[] _backupData;
		internal bool _inTxn = false; // in-transaction flag
		internal bool _inRowList = false; // true if the row is in the row list and should trigger events when changed

		/// <summary>
		/// Construct & assign parent DataTableMx
		/// </summary>
		/// <param name="table"></param>

		internal DataRowMx(DataTableMx table)
		{
			_table = table;
			_inRowList = false; // row created but not yet in the list
			_itemArray = new object[table.Columns.Count];
		}

		/// <summary>
		/// Table this row is a part of
		/// </summary>

		internal DataTableMx Table
		{ get { return _table; } }
		public DataTableMx _table; // associated DataTableMx

		/// <summary>
		/// Get/set a single element in the ItemArray object array
		/// </summary>
		/// <param name="columnIndex"></param>
		/// <returns></returns>

		public object this[int columnIndex]
		{
			get
			{
				if (_itemArray == null) DebugMx.InvalidConditionException("_itemArray == null");

				if (columnIndex < 0 || columnIndex >= _itemArray.Length)
				DebugMx.InvalidConditionException("columnIndex = " + columnIndex + " out of range for DataRow of size = " +_itemArray.Length);

				object o = _itemArray[columnIndex];
				if (o == null || o is DBNull) return o;

				else if (Table == null || Table.UseNativeDataTypes)
					return o;

				else return MobiusDataType.ConvertToPrimitiveValue(o);
			}

			set
			{
				_itemArray[columnIndex] = value;
				RowState = DataRowState.Modified; // say the row is now modified
				_table.CallDataChangedEventHandlers(ListChangedType.ItemChanged, this);
			}
		}

		/// <summary>
		/// Get/set a single element in the ItemArray object array by col name
		/// </summary>
		/// <param name="columnName"></param>
		/// <returns></returns>

		public object this[string columnName]
		{
			get
			{
				int index = Table.Columns.IndexOf(columnName);
				AssertMx.IsTrue(index >= 0, "Column not found: " + columnName);
				return this[index];
			}
			set
			{
				int index = Table.Columns.IndexOf(columnName);
				AssertMx.IsTrue(index >= 0, "Column not found: " + columnName);
				this[index] = value;
			}
		}

		/// <summary>
		/// The array of items for the row (an array of native or primitive types is created)
		/// </summary>

		public object[] ItemArray
		{
			get
			{
				object[] ia2 = new object[_table.Columns.Count];
				if (_table.UseNativeDataTypes)
					_itemArray.CopyTo(ia2, 0);

				else // convert to primitive types
				{
					for (int i1 = 0; i1 < _itemArray.Length; i1++)
					{
						ia2[i1] = MobiusDataType.ConvertToPrimitiveValue(_itemArray[i1]);
					}
				}

				return ia2;
			}

			set
			{
				_itemArray = new object[_table.Columns.Count];
				value.CopyTo(_itemArray, 0);
			}
		}
		internal object[] _itemArray;

		/// <summary>
		/// The reference to the array of items for the row (faster than ItemArray)
		/// </summary>

		public object[] ItemArrayRef
		{
			get
			{ return _itemArray; }
			set
			{ _itemArray = value; }
		}

/// <summary>
/// Get length of ItemArray
/// </summary>

		public long Length
		{
			get
			{
				if (_itemArray != null) return _itemArray.Length;
				else return -1; // not defined
			}
		}

		/// <summary>
		/// Begin edit of row
		/// </summary>

		void IEditableObject.BeginEdit()
		{
			if (!_inTxn)
			{
				_backupData = new object[ItemArray.Length];
				ItemArray.CopyTo(_backupData, 0);
				_inTxn = true;
			}
		}

		/// <summary>
		/// Cancel edit of row
		/// </summary>

		void IEditableObject.CancelEdit()
		{
			if (_inTxn)
			{
				_backupData.CopyTo(ItemArray, 0);
				_inTxn = false;
			}
		}

		/// <summary>
		/// End edit of row
		/// </summary>

		void IEditableObject.EndEdit()
		{
			if (_inTxn)
			{
				_inTxn = false;
			}
		}

		/// <summary>
		/// Row state
		/// </summary>

		public DataRowState RowState
		{
			get { return _rowState; }

			set { _rowState = value; }
		}
		DataRowState _rowState = DataRowState.Unchanged;

		/// <summary>
		/// ToString
		/// </summary>
		/// <returns></returns>

		public override string ToString()
		{
			if (ItemArray == null) return "null";

			string txt = "";
			foreach (object o in ItemArray)
			{
				if (o != ItemArray[0])
					txt += ", ";

				if (o == null) txt += "null";
				else txt += o.ToString();
			}

			return txt;
		}
	} // DataRowMx

}
