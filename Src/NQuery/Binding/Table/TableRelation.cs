using System;
using System.Collections.Generic;

using NQuery.Runtime;

namespace NQuery
{
	public sealed class TableRelation
	{
		private ColumnBindingCollection _parentColumns;
		private ColumnBindingCollection _childColumns;

		public TableRelation(IList<ColumnBinding> parentColumns, IList<ColumnBinding> childColumns)
		{
			if (parentColumns == null)
				throw ExceptionBuilder.ArgumentNull("parentColumns");
			
			if (childColumns == null)
				throw ExceptionBuilder.ArgumentNull("childColumns");

			if (parentColumns.Count == 0)
				throw ExceptionBuilder.ArgumentArrayMustNotBeEmpty("parentColumns");

			if (childColumns.Count != parentColumns.Count)
				throw ExceptionBuilder.ArgumentArrayMustHaveSameSize("childColumns", "parentColumns");
			
			TableBinding parentTable = parentColumns[0].Table;
			TableBinding childTable = childColumns[0].Table;

			for (int i = 1; i < parentColumns.Count; i++)
			{
				if (parentColumns[i].Table != parentTable)
					throw ExceptionBuilder.AllColumnsMustBelongToSameTable("parentColumns");
			}

			for (int i = 1; i < childColumns.Count; i++)
			{
				if (childColumns[i].Table != childTable)
					throw ExceptionBuilder.AllColumnsMustBelongToSameTable("childColumns");
			}

			_parentColumns = new ColumnBindingCollection(parentColumns);
			_childColumns = new ColumnBindingCollection(childColumns);
		}

		public int ColumnCount
		{
			get { return _parentColumns.Count; }
		}
		
		public TableBinding ParentTable
		{
			get { return _parentColumns[0].Table; }
		}

		public ColumnBindingCollection ParentColumns
		{
			get { return _parentColumns; }
		}

		public TableBinding ChildTable
		{
			get { return _childColumns[0].Table; }
		}

		public ColumnBindingCollection ChildColumns
		{
			get { return _childColumns; }
		}
	}
}