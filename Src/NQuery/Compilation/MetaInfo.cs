using System;

using NQuery.Runtime;

namespace NQuery.Compilation
{
	internal sealed class MetaInfo
	{
		private ColumnRefBinding[] _columnDependencies;
		private TableRefBinding[] _tableDependencies;
		private bool _containsSingleRowSubselects;
		private bool _containsExistenceSubselects;

		public MetaInfo(ColumnRefBinding[] columnDependencies, 
		                TableRefBinding[] tableDependencies,
		                bool containsSingleRowSubselects, 
		                bool containsExistenceSubselects)
		{
			_columnDependencies = columnDependencies;
			_tableDependencies = tableDependencies;
			_containsSingleRowSubselects = containsSingleRowSubselects;
			_containsExistenceSubselects = containsExistenceSubselects;
		}

		public ColumnRefBinding[] ColumnDependencies
		{
			get { return _columnDependencies; }
		}

		public TableRefBinding[] TableDependencies
		{
			get { return _tableDependencies; }
		}

		public bool ContainsSingleRowSubselects
		{
			get { return _containsSingleRowSubselects; }
			set { _containsSingleRowSubselects = value; }
		}

		public bool ContainsExistenceSubselects
		{
			get { return _containsExistenceSubselects; }
			set { _containsExistenceSubselects = value; }
		}
	}
}