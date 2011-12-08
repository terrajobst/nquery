using System;
using System.Collections.Generic;

using NQuery.Runtime;

namespace NQuery.Compilation
{	
	internal sealed class SelectQuery : QueryNode
	{
		private bool _isDistinct;
		private TopClause _topClause;
		private SelectColumn[] _selectColumns;
		private TableReference _tableReferences;
		private ExpressionNode _whereClause;
		private ExpressionNode[] _groupByColumns;
		private ExpressionNode _havingClause;
		private OrderByColumn[] _orderByColumns;
		private AggregateExpression[] _aggregateDependencies;
		private ColumnRefBinding[] _columnDependencies;
		private QueryScope _queryScope;
		private RowBufferCalculator _rowBufferCalculator;

		public override AstNodeType NodeType
		{
			get { return AstNodeType.SelectQuery; }
		}

		public override AstElement Clone(Dictionary<AstElement, AstElement> alreadyClonedElements)
		{
			SelectQuery result = new SelectQuery();
			result.IsDistinct = _isDistinct;
			
			if (_topClause != null)
				result.TopClause = (TopClause)_topClause.Clone(alreadyClonedElements);

			result.SelectColumns = ArrayHelpers.CreateDeepCopyOfAstElementArray(_selectColumns, alreadyClonedElements);

			if (_tableReferences != null)
				result.TableReferences = (TableReference)_tableReferences.Clone(alreadyClonedElements);
			
			if (_whereClause != null)
				result.WhereClause = (ExpressionNode)_whereClause.Clone(alreadyClonedElements);
			
			if (_groupByColumns != null)
				result.GroupByColumns = ArrayHelpers.CreateDeepCopyOfAstElementArray(_groupByColumns, alreadyClonedElements);
			
			if (_orderByColumns != null)
				result.OrderByColumns = ArrayHelpers.CreateDeepCopyOfAstElementArray(_orderByColumns, alreadyClonedElements);

			result.AggregateDependencies = ArrayHelpers.CreateDeepCopyOfAstElementArray(_aggregateDependencies, alreadyClonedElements);
			result.ColumnDependencies =  ArrayHelpers.Clone(_columnDependencies);

			return result;
		}

		public override SelectColumn[] GetColumns()
		{
			return _selectColumns;
		}
		
		public bool IsDistinct
		{
			get { return _isDistinct; }
			set { _isDistinct = value; }
		}

		public TopClause TopClause
		{
			get { return _topClause; }
			set { _topClause = value; }
		}

		public SelectColumn[] SelectColumns
		{
			get { return _selectColumns; }
			set { _selectColumns = value; }
		}

		public TableReference TableReferences
		{
			get { return _tableReferences; }
			set { _tableReferences = value; }
		}

		public ExpressionNode WhereClause
		{
			get { return _whereClause; }
			set { _whereClause = value; }
		}

		public ExpressionNode[] GroupByColumns
		{
			get { return _groupByColumns; }
			set { _groupByColumns = value; }
		}

		public ExpressionNode HavingClause
		{
			get { return _havingClause; }
			set { _havingClause = value; }
		}

		public OrderByColumn[] OrderByColumns
		{
			get { return _orderByColumns; }
			set { _orderByColumns = value; }
		}

		public bool IsAggregated
		{
			get { return _aggregateDependencies != null && _aggregateDependencies.Length > 0; }
		}

		public AggregateExpression[] AggregateDependencies
		{
			get { return _aggregateDependencies; }
			set { _aggregateDependencies = value; }
		}

		public ColumnRefBinding[] ColumnDependencies
		{
			get { return _columnDependencies; }
			set { _columnDependencies = value; }
		}

		public QueryScope QueryScope
		{
			get { return _queryScope; }
			set { _queryScope = value; }
		}

		public RowBufferCalculator RowBufferCalculator
		{
			get { return _rowBufferCalculator; }
			set { _rowBufferCalculator = value; }
		}
	}
}