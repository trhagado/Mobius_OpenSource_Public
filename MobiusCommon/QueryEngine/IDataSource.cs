using Mobius.Data;
using Mobius.UAL;

using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Mobius.QueryEngineLibrary
{
	/// <summary>
	/// This interface provides access to a query, its associated results and the 
	/// formatting information to be used to format the results.
	/// </summary>

	public interface IDataSource
	{

		/// <summary>
		/// Get the next row of data
		/// </summary>

		object [] NextRow();

		/// <summary>
		/// Close the DataSource
		/// </summary>

		void Close();

		/// <summary>
		/// Reopen cursor on DataSource
		/// </summary>

		void Reopen();

		/// <summary>
		/// Method to call to check for cancel by user
		/// </summary>

		ICheckForCancel CheckForCancel
		{
			get;
			set;
		}

		/// <summary>
		/// See if cancelled by user
		/// </summary>

		bool Cancelled
		{
			get;
			set;
		}

		/// <summary>
		/// Sort result set
		/// </summary>
		/// <param name="qc"></param>
		/// <param name="direction"></param>

		void SortResultSet(
			QueryColumn qc,
			SortOrder direction);

/// <summary>
/// Sort a result set on multiple columns and reopen sorted data
/// </summary>
/// <param name="sortColumns">

		void SortResultSet(
			List<SortColumn> sortColumns);

	}
}
