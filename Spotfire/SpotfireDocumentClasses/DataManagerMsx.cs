using Mobius.SpotfireComOps;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Mobius.SpotfireDocument
{

	/// <summary>
	/// DataManager
	/// </summary>

	public class DataManagerMsx : NodeMsx
	{

		public DataTableCollectionMsx TableCollection = new DataTableCollectionMsx();

		//public DataMarkingSelectionCollection Markings { get; }
		//public HighlightSelection Highlight { get; }
		//public DataFilteringSelectionCollection Filterings { get; }
		//public DataPropertyRegistry Properties { get; }
		//public DataRelationCollection Relations { get; }
		//public ColumnRelationCollection ColumnRelations { get; }
		//public DataSelection AllRows { get; }
		//public DataFunctionCollection DataFunctions { get; }
		//public DataFunctionExpressionFunctionCollection ExpressionFunctions { get; }

		/// <summary>
		/// Update secondary object xxxMsx class object references to the
		/// associated Guid Ids prior to serialization.
		/// This method is normally overridden by each xxxMsx class to update 
		/// the references for that class.
		/// </summary>

		public override void UpdatePreSerializationSecondaryReferences()
		{
			UpdatePreSerializationSecondaryReferences(TableCollection);

			return;
		}

		/// <summary>
		/// Update secondary references for each Mobius.SpotfireDocument.xxxMsx class 
		/// These secondary references are not serialized and need to be updated from 
		/// other Ids (usually Guids) after deserializing a Document. They include references
		/// to DataTableMsx, DataColumnMsx, VisualMsx, PageMsx ...
		/// </summary>

		public override void UpdatePostDeserializationSecondaryReferences()
		{
			ValidateNode();

			UpdatePostDeserializationSecondaryReferences(TableCollection);

			return;
		}

	}

	/// <summary>
	/// DataTableCollection
	/// </summary>

	public class DataTableCollectionMsx : NodeMsx, IEnumerable<DataTableMsx>, IEnumerable
	{
		public DataTableMsx this[int index] => TableList[index]; // Indexer declaration  

		public int Count => TableList.Count;

		public DataTableMsx Add(DataTableMsx dt)
		{
			TableList.Add(dt);
			return dt;
		}
		
		public List<DataTableMsx> TableList = new List<DataTableMsx>(); // list of data tables 

		[XmlIgnore]
		public DataTableMsx DefaultDataTableReference = null;
		public string DefaultDataTableReferenceSerializedId = "";

		/// <summary>
		/// Static method to get a datatable by name from a supplied list
		/// </summary>
		/// <param name="name"></param>
		/// <param name="tableList"></param>
		/// <returns></returns>

		public static DataTableMsx GetTableByName(
			string name,
			List<DataTableMsx> tableList)
		{
			foreach (DataTableMsx dt0 in tableList)
			{
				if (Lex.Eq(dt0.Name, name))
				{
					return dt0;
				}
			}

			return null;
		}

		/// <summary>
		///  GetTableByNameWithException
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public DataTableMsx GetTableByNameWithException(string name)
		{
			DataTableMsx table;

			if (TryGetTableByName(name, out table))
				return table;

			else throw new Exception("Can't find DataTable by Name: " + name);
		}

		/// <summary>
		/// TryGetTableByName
		/// </summary>
		/// <param name="name"></param>
		/// <param name="table"></param>
		/// <returns></returns>

		public bool TryGetTableByName(
			string name,
			out DataTableMsx table)
		{
			foreach (DataTableMsx dt0 in TableList)
			{
				if (Lex.Eq(dt0.Name, name))
				{
					table = dt0;
					return true;
				}
			}

			table = null;
			return false;
		}


		/// <summary>
		/// GetTableById
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>

		public DataTableMsx GetTableById(string id)
		{
			DataTableMsx table;

			TryGetTableById(id, out table);
			return table;
		}

		/// <summary>
		///  GetTableByIdWithException
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>

		public DataTableMsx GetTableByIdWithException(string id)
		{
			DataTableMsx table;

			if (TryGetTableById(id, out table))
				return table;

			else throw new Exception("Can't find DataTable by Id: " + id);
		}

		/// <summary>
		/// TryGetTableById
		/// </summary>
		/// <param name="id"></param>
		/// <param name="table"></param>
		/// <returns></returns>

		public bool TryGetTableById(
			string id,
			out DataTableMsx table)
		{
			foreach (DataTableMsx dt0 in TableList)
			{
				if (Lex.Eq(dt0.Id, id))
				{
					table = dt0;
					return true;
				}
			}

			table = null;
			return false;
		}

		/// <summary>
		/// Replace a table with an updated version of the table
		/// </summary>
		/// <param name="dt"></param>

		public void ReplaceTableWithUpdatedVersion(DataTableMsx dt)
		{
			DataTableMsx dt0 = GetTableByIdWithException(dt.Id); // get existing instance
			int dti = TableList.IndexOf(dt0); // and its index

			TableList[dti] = dt; // replace it with the updated version
			dt.Owner = dt0.Owner; // point to parent list

			return;
		}

		/// <summary>
		/// Update secondary object xxxMsx class object references to the
		/// associated Guid Ids prior to serialization.
		/// This method is normally overridden by each xxxMsx class to update 
		/// the references for that class.
		/// </summary>

		public override void UpdatePreSerializationSecondaryReferences()
		{
			DefaultDataTableReferenceSerializedId = DefaultDataTableReference?.Id;

			foreach (DataTableMsx dt in TableList)
			{
				UpdatePreSerializationSecondaryReferences(dt);
			}

			return;
		}

		/// <summary>
		/// Update secondary references for each Mobius.SpotfireDocument.xxxMsx class 
		/// These secondary references are not serialized and need to be updated from 
		/// other Ids (usually Guids) after deserializing a Document. They include references
		/// to DataTableMsx, DataColumnMsx, VisualMsx, PageMsx ...
		/// </summary>

		public override void UpdatePostDeserializationSecondaryReferences()
		{
			ValidateNode();

			TryGetTableById(DefaultDataTableReferenceSerializedId, out DefaultDataTableReference);

			foreach (DataTableMsx dt in TableList)
			{
				UpdatePostDeserializationSecondaryReferences(dt);
			}

			return;
		}

		/// <summary>
		/// Returns a DataTableMsx-typed IEnumerator  to interate through TableList
		/// </summary>
		/// <returns></returns>

		IEnumerator<DataTableMsx> IEnumerable<DataTableMsx>.GetEnumerator()
		{
			return TableList.GetEnumerator();
		}

		/// <summary>
		/// Returns an IEnumerator to interate through TableList
		/// </summary>
		/// <returns></returns>

		IEnumerator IEnumerable.GetEnumerator()
		{
			return TableList.GetEnumerator();
		}



	}
}
