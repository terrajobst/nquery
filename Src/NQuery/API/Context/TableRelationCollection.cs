using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;

using NQuery.Runtime;

namespace NQuery
{
	public sealed class TableRelationCollection : Collection<TableRelation>
	{
		private DataContext _dataContext;

		internal TableRelationCollection(DataContext dataContext)
		{
			_dataContext = dataContext;
		}

		private void OnChange(EventArgs eventArgs)
		{
			EventHandler<EventArgs> handler = Changed;
			if (handler != null)
				handler(this, eventArgs);
		}

		private void BeforeInsert(TableRelation tableRelation)
		{
			// Ensure that parent and child table are within the data context.

			if (_dataContext.Tables[tableRelation.ParentTable.Name] == null)
				throw ExceptionBuilder.ParentTableMustExistInDataContext("tableRelation");

			if (_dataContext.Tables[tableRelation.ChildTable.Name] == null)
				throw ExceptionBuilder.ChildTableMustExistInDataContext("tableRelation");

			// Ensure that no table relation with the same parent and child columns exists.

			foreach (TableRelation existingTableRelation in this)
			{
				if (existingTableRelation.ParentTable == tableRelation.ParentTable &&
					existingTableRelation.ChildTable == tableRelation.ChildTable)
				{
					// TODO: Compare the two and make sure that permutations of parent columns and
					//       child columns do not make any difference.
				}
			}
		}

		protected override void ClearItems()
		{
			base.ClearItems();
			OnChange(EventArgs.Empty);
		}

		protected override void InsertItem(int index, TableRelation item)
		{
			if (item == null)
				throw ExceptionBuilder.ArgumentNull("item");

			BeforeInsert(item);
			base.InsertItem(index, item);
			OnChange(EventArgs.Empty);
		}

		protected override void RemoveItem(int index)
		{
			base.RemoveItem(index);
			OnChange(EventArgs.Empty);
		}

		protected override void SetItem(int index, TableRelation item)
		{
			if (item == null)
				throw ExceptionBuilder.ArgumentNull("item");

			BeforeInsert(item);
			base.SetItem(index, item);
			OnChange(EventArgs.Empty);
		}

		public TableRelation Add(IList<ColumnBinding> parentColumns, IList<ColumnBinding> childColumns)
		{
			TableRelation tableRelation = new TableRelation(parentColumns, childColumns);
			Add(tableRelation);
			return tableRelation;
		}

		public TableRelation Add(TableBinding parentTable, IList<string> parentColumns, TableBinding childTable, IList<string> childColumns)
		{
			if (parentTable == null)
				throw ExceptionBuilder.ArgumentNull("parentTable");

			if (parentColumns == null)
				throw ExceptionBuilder.ArgumentNull("parentColumns");

			if (parentColumns.Count == 0)
				throw ExceptionBuilder.ArgumentArrayMustNotBeEmpty("parentColumns");

			if (childTable == null)
				throw ExceptionBuilder.ArgumentNull("childTable");

			if (childColumns == null)
				throw ExceptionBuilder.ArgumentNull("childColumns");

			if (childColumns.Count == 0)
				throw ExceptionBuilder.ArgumentArrayMustNotBeEmpty("childColumns");

			ColumnBinding[] parentColumnBindings = new ColumnBinding[parentColumns.Count];

			for (int i = 0; i < parentColumnBindings.Length; i++)
			{
				parentColumnBindings[i] = parentTable.GetColumn(parentColumns[i]);

				if (parentColumnBindings[i] == null)
					throw ExceptionBuilder.ParentColumnNotFound("parentColumns", parentTable, parentColumns[i]);
			}

			ColumnBinding[] childColumnBindings = new ColumnBinding[childColumns.Count];

			for (int i = 0; i < childColumnBindings.Length; i++)
			{
				childColumnBindings[i] = childTable.GetColumn(childColumns[i]);

				if (childColumnBindings[i] == null)
					throw ExceptionBuilder.ChildColumnNotFound("childColumns", childTable, childColumns[i]);
			}

			return Add(parentColumnBindings, childColumnBindings);
		}

		public TableRelation Add(string parentTable, IList<string> parentColumns, string childTable, IList<string> childColumns)
		{
			if (parentTable == null)
				throw ExceptionBuilder.ArgumentNull("parentTable");

			if (parentColumns == null)
				throw ExceptionBuilder.ArgumentNull("parentColumns");

			if (parentColumns.Count == 0)
				throw ExceptionBuilder.ArgumentArrayMustNotBeEmpty("parentColumns");

			if (childTable == null)
				throw ExceptionBuilder.ArgumentNull("childTable");

			if (childColumns == null)
				throw ExceptionBuilder.ArgumentNull("childColumns");

			if (childColumns.Count == 0)
				throw ExceptionBuilder.ArgumentArrayMustNotBeEmpty("childColumns");

			TableBinding parentTableBinding = _dataContext.Tables[parentTable];
			TableBinding childTableBinding = _dataContext.Tables[childTable];

			if (parentTableBinding == null)
				throw ExceptionBuilder.ParentTableMustExistInDataContext("parentTable");

			if (childTableBinding == null)
				throw ExceptionBuilder.ChildTableMustExistInDataContext("childTable");

			return Add(parentTableBinding, parentColumns, childTableBinding, childColumns);
		}

		public TableRelation Add(DataRelation dataRelation)
		{
			if (dataRelation == null)
				throw ExceptionBuilder.ArgumentNull("dataRelation");

			string[] parentColumns = new string[dataRelation.ParentColumns.Length];
			for (int i = 0; i < parentColumns.Length; i++)
				parentColumns[i] = dataRelation.ParentColumns[i].ColumnName;

			string[] childColumns = new string[dataRelation.ChildColumns.Length];
			for (int i = 0; i < childColumns.Length; i++)
				childColumns[i] = dataRelation.ChildColumns[i].ColumnName;

			return Add(dataRelation.ParentTable.TableName, parentColumns, dataRelation.ChildTable.TableName, childColumns);
		}

		public void AddRange(DataRelationCollection dataRelations)
		{
			if (dataRelations == null)
				throw ExceptionBuilder.ArgumentNull("dataRelations");

			foreach (DataRelation dataRelation in dataRelations)
				Add(dataRelation);
		}

		public IList<TableRelation> GetChildRelations(TableBinding table)
		{
			if (table == null)
				throw ExceptionBuilder.ArgumentNull("table");

			List<TableRelation> result = new List<TableRelation>();

			foreach (TableRelation tableRelation in this)
			{
				if (tableRelation.ParentTable == table)
					result.Add(tableRelation);
			}

			return result;
		}

		public IList<TableRelation> GetParentRelations(TableBinding table)
		{
			if (table == null)
				throw ExceptionBuilder.ArgumentNull("table");

			List<TableRelation> result = new List<TableRelation>();

			foreach (TableRelation tableRelation in this)
			{
				if (tableRelation.ChildTable == table)
					result.Add(tableRelation);
			}

			return result;
		}

		public IList<TableRelation> GetRelations(TableBinding table)
		{
			if (table == null)
				throw ExceptionBuilder.ArgumentNull("table");

			List<TableRelation> result = new List<TableRelation>();

			foreach (TableRelation tableRelation in this)
			{
				if (tableRelation.ParentTable == table || tableRelation.ChildTable == table)
					result.Add(tableRelation);
			}

			return result;
		}

		public IList<TableRelation> GetRelations(params TableBinding[] tables)
		{
			List<TableRelation> result = new List<TableRelation>();

			if (tables != null)
			{
				foreach (TableRelation tableRelation in this)
				{
					if (ArrayHelpers.Contains(tables, tableRelation.ParentTable) &&
						ArrayHelpers.Contains(tables, tableRelation.ChildTable))
						result.Add(tableRelation);
				}
			}

			return result;
		}

		public event EventHandler<EventArgs> Changed;
	}
}