namespace NQuery.Compilation
{
	internal enum AstNodeType
	{
		Literal,
		
		UnaryExpression,
		BinaryExpression,
		BetweenExpression,
		IsNullExpression,
		CastExpression,
		CaseExpression,
		CoalesceExpression,
		NullIfExpression,
		InExpression,
		
		NamedConstantExpression,
		ParameterExpression,
		NameExpression,
		PropertyAccessExpression,
		FunctionInvocationExpression,
		MethodInvocationExpression,
		
		ColumnExpression,
		RowBufferEntryExpression,
		AggregateExpression,
		SingleRowSubselect,
		ExistsSubselect,
		AllAnySubselect,

		NamedTableReference,
		JoinedTableReference,
		DerivedTableReference,
		
		SelectQuery,
		BinaryQuery,
		SortedQuery,
		CommonTableExpressionQuery,
		
		TableAlgebraNode,
		ConstantScanAlgebraNode,
		NullScanAlgebraNode,
		ComputeScalarAlgebraNode,
		JoinAlgebraNode,
		ConcatAlgebraNode,
		SortAlgebraNode,
		AggregateAlgebraNode,
		TopAlgebraNode,
		FilterAlgebraNode,
		ResultAlgebraNode,
		AssertAlgebraNode,
		IndexSpoolAlgebraNode,
		StackedTableSpoolAlgebraNode,
		StackedTableSpoolRefAlgebraNode,
		HashMatchAlgebraNode
	}
}