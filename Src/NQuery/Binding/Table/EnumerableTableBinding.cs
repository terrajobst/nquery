using System;
using System.Collections;
using System.Collections.Generic;

namespace NQuery.Runtime
{
	public class EnumerableTableBinding : TableBinding
	{
		private IEnumerable _enumerable;
		private Type _rowType;
		private PropertyBinding[] _properties;
		private string _tableName;

		public EnumerableTableBinding(IEnumerable enumerable, Type rowType, PropertyBinding[] properties, string tableName)
		{
			if (enumerable == null)
				throw ExceptionBuilder.ArgumentNull("enumerable");
			
			if (rowType == null)
				throw ExceptionBuilder.ArgumentNull("rowType");
			
			if (properties == null)
				throw ExceptionBuilder.ArgumentNull("properties");

			if (tableName == null)
				throw ExceptionBuilder.ArgumentNull("tableName");

			_enumerable	= enumerable;
			_rowType = rowType;
			_properties = properties;
			_tableName = tableName;
		}

		public override string Name
		{
			get { return _tableName; }
		}

		protected override IList<ColumnBinding> BuildColumns()
		{
			List<ColumnBinding> columnDefinitionList = new List<ColumnBinding>();

			foreach (PropertyBinding propertyBinding in _properties)
			{
				PropertyColumnBinding propertyColumnBinding = new PropertyColumnBinding(this, propertyBinding);
				columnDefinitionList.Add(propertyColumnBinding);
			}

			return columnDefinitionList;
		}

		public override Type RowType
		{
			get { return  _rowType; }
		}

		public override IEnumerable GetRows(ColumnRefBinding[] neededColumns)
		{
			return _enumerable;
		}
	}
}