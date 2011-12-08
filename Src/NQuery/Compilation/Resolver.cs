using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using NQuery.Runtime;

namespace NQuery.Compilation
{
	internal class Resolver : OperatorTypeResolver
	{
		#region Table Reference Declaration Finder

		private sealed class TableReferenceDeclarationFinder : StandardVisitor
		{
			private List<NamedTableReference> _nameTableReferenceList = new List<NamedTableReference>();
			private List<DerivedTableReference> _derivedTableReferenceList = new List<DerivedTableReference>();

			public NamedTableReference[] GetNamedTableReferences()
			{
				return _nameTableReferenceList.ToArray();
			}

			public DerivedTableReference[] GetDerivedTableReferences()
			{
				return _derivedTableReferenceList.ToArray();
			}

			public override TableReference VisitNamedTableReference(NamedTableReference node)
			{
				_nameTableReferenceList.Add(node);
				return node;
			}

			public override TableReference VisitDerivedTableReference(DerivedTableReference node)
			{
				_derivedTableReferenceList.Add(node);
				return node;
			}

			// We don't want to visit subselect predicates in ON.

			public override ExpressionNode VisitSingleRowSubselect(SingleRowSubselect expression)
			{
				return expression;
			}

			public override ExpressionNode VisitExistsSubselect(ExistsSubselect expression)
			{
				return expression;
			}

			public override ExpressionNode VisitAllAnySubselect(AllAnySubselect expression)
			{
				return expression;
			}
		}

		#endregion

		private Scope _scope;
		private Stack<QueryScope> _queryScopes = new Stack<QueryScope>();

		public Resolver(IErrorReporter errorReporter, Scope scope)
			: base(errorReporter)
		{
			_scope = scope;

			// Needed for IntelliSense. This ensures
			// That CurrentScope always returns a legal scope.
			PushNewScope(null);
		}

		#region Scope Helpers

		private void PushScope(QueryScope queryScope)
		{
			_queryScopes.Push(queryScope);
		}

		private QueryScope PushNewScope(QueryScope queryScope)
		{
			if (queryScope == null)
				queryScope = new QueryScope(CurrentScope);
			PushScope(queryScope);
			return queryScope;
		}

		private void PopScope()
		{
			_queryScopes.Pop();
		}

		public QueryScope CurrentScope
		{
			get
			{
				if (_queryScopes.Count == 0)
					return null;

				return _queryScopes.Peek();
			}
		}

		#endregion

		#region Resolution Helpers

		private TableRefBinding ResolveTableRef(SourceRange sourceRange, Identifier identifier)
		{
			QueryScope lookupScope = CurrentScope;

			while (lookupScope != null)
			{
				TableRefBinding[] candidates = lookupScope.FindTableRef(identifier);

				if (candidates != null && candidates.Length > 0)
				{
					if (candidates.Length > 1)
						ErrorReporter.AmbiguousTableRef(sourceRange, identifier, candidates);

					return candidates[0];
				}

				lookupScope = lookupScope.ParentScope;
			}

			return null;
		}

		private ColumnRefBinding ResolveColumnRef(SourceRange sourceRange, Identifier identifier)
		{
			QueryScope lookupScope = CurrentScope;

			while (lookupScope != null)
			{
				ColumnRefBinding[] candidates = lookupScope.FindColumnRef(identifier);

				if (candidates != null && candidates.Length > 0)
				{
					if (candidates.Length > 1)
						ErrorReporter.AmbiguousColumnRef(sourceRange, identifier, candidates);

					return candidates[0];
				}

				lookupScope = lookupScope.ParentScope;
			}

			return null;
		}

		private ColumnRefBinding ResolveColumnRef(TableRefBinding tableRef, SourceRange sourceRange, Identifier identifier)
		{
			QueryScope lookupScope = tableRef.Scope;

			ColumnRefBinding[] candidates = lookupScope.FindColumnRef(tableRef, identifier);

			if (candidates == null || candidates.Length == 0)
				return null;

			if (candidates.Length > 1)
				ErrorReporter.AmbiguousColumnRef(sourceRange, identifier, candidates);

			return candidates[0];
		}

		private TableBinding ResolveTable(SourceRange sourceRange, Identifier identifier)
		{
			TableBinding[] candidates;
			
			// First we try to find a common table expression

			QueryScope lookupScope = CurrentScope;

			while (lookupScope != null)
			{
				candidates = lookupScope.FindCommonTable(identifier);

				if (candidates != null && candidates.Length > 0)
				{
					if (candidates.Length > 1)
						ErrorReporter.AmbiguousTable(sourceRange, identifier, candidates);

					return candidates[0];
				}

				lookupScope = lookupScope.ParentScope;
			}

			// Now we try to find a "global" table.

			candidates = _scope.DataContext.Tables.Find(identifier);

			if (candidates == null || candidates.Length == 0)
				return null;

			if (candidates.Length > 1)
				ErrorReporter.AmbiguousTable(sourceRange, identifier, candidates);

			return candidates[0];
		}

		private ConstantBinding ResolveConstant(SourceRange sourceRange, Identifier identifier)
		{
			ConstantBinding[] candidates = _scope.DataContext.Constants.Find(identifier);

			if (candidates == null || candidates.Length == 0)
				return null;

			if (candidates.Length > 1)
				ErrorReporter.AmbiguousConstant(sourceRange, identifier, candidates);

			return candidates[0];
		}

		private ParameterBinding ResolveParameter(SourceRange sourceRange, Identifier identifier)
		{
			ParameterBinding[] candidates = _scope.Parameters.Find(identifier);

			if (candidates == null || candidates.Length == 0)
				return null;

			if (candidates.Length > 1)
				ErrorReporter.AmbiguousParameter(sourceRange, identifier, candidates);

			return candidates[0];
		}

		private AggregateBinding ResolveAggregate(SourceRange sourceRange, Identifier identifier)
		{
			AggregateBinding[] candidates = _scope.DataContext.Aggregates.Find(identifier);

			if (candidates == null || candidates.Length == 0)
				return null;

			if (candidates.Length > 1)
				ErrorReporter.AmbiguousAggregate(sourceRange, identifier, candidates);

			return candidates[0];
		}

		private PropertyBinding ResolveTypeProperty(Type type, SourceRange sourceRange, Identifier identifier)
		{
			PropertyBinding[] candidates = _scope.DataContext.MetadataContext.FindProperty(type, identifier);

			if (candidates == null || candidates.Length == 0)
				return null;

			if (candidates.Length > 1)
				ErrorReporter.AmbiguousProperty(sourceRange, identifier, candidates);

			return candidates[0];
		}

		private PropertyBinding ResolveCustomProperty(IList<PropertyBinding> customProperties, SourceRange sourceRange, Identifier identifier)
		{
			PropertyBinding[] candidates = _scope.DataContext.MetadataContext.FindProperty(customProperties, identifier);

			if (candidates == null || candidates.Length == 0)
				return null;

			if (candidates.Length > 1)
				ErrorReporter.AmbiguousProperty(sourceRange, identifier, candidates);

			return candidates[0];
		}

		private FunctionBinding ResolveFunction(Identifier functionName, Type[] argumentTypes)
		{
			FunctionBinding[] functionGroup = _scope.DataContext.Functions.Find(functionName);
			return (FunctionBinding)Binder.BindInvocation(functionGroup, argumentTypes);
		}

		private MethodBinding ResolveMethod(Type declaringType, Identifier methodName, Type[] argumentTypes)
		{
			MethodBinding[] methodGroup = _scope.DataContext.MetadataContext.FindMethod(declaringType, methodName);
			return (MethodBinding)Binder.BindInvocation(methodGroup, argumentTypes);
		}

		private void ResolveOrderBy(SelectColumn[] inputSelectColumns, IEnumerable<OrderByColumn> orderByColumns)
		{
			// 1. ORDER BY expressions allow simple integer literals. They refer to an expression
			//    in the selection list.
			//
			// 2. ORDER BY allows to specify the column alias.

			foreach (OrderByColumn orderByColumn in orderByColumns)
			{
				orderByColumn.ColumnIndex = -1;

				// Check for positional form.

				bool expressionResolved = false;

				LiteralExpression literalExpression = orderByColumn.Expression as LiteralExpression;

				if (literalExpression != null && literalExpression.IsInt32Value)
				{
					int selectionListPos = literalExpression.AsInt32;
					if (selectionListPos < 1 || selectionListPos > inputSelectColumns.Length)
						ErrorReporter.OrderByColumnPositionIsOutOfRange(selectionListPos);
					else
					{
						int index = selectionListPos - 1;
						orderByColumn.Expression = inputSelectColumns[index].Expression;
						orderByColumn.ColumnIndex = index;
					}

					expressionResolved = true;
				}
				else
				{
					// Check for column alias form.

					NameExpression nameExpression = orderByColumn.Expression as NameExpression;

					if (nameExpression != null)
					{
						for (int i = 0; i < inputSelectColumns.Length; i++)
						{
							SelectColumn selectColumn = inputSelectColumns[i];
							if (nameExpression.Name == selectColumn.Alias)
							{
								orderByColumn.Expression = selectColumn.Expression;
								orderByColumn.ColumnIndex = i;
								expressionResolved = true;
								break;
							}
						}
					}
				}

				if (!expressionResolved)
				{
					orderByColumn.Expression = VisitExpression(orderByColumn.Expression);

					for (int i = 0; i < inputSelectColumns.Length; i++)
					{
						SelectColumn selectColumn = inputSelectColumns[i];
						if (selectColumn.Expression.IsStructuralEqualTo(orderByColumn.Expression))
						{
							orderByColumn.ColumnIndex = i;
							break;
						}
					}
				}
			}
		}

		#endregion

		#region Helpers

		private static bool IsRecursive(QueryNode query, Identifier tableName)
		{
			CommonTableExpressionRecursiveMemberChecker checker = new CommonTableExpressionRecursiveMemberChecker(tableName);
			checker.Visit(query);

			return checker.RecursiveReferences > 0 ||
			       checker.RecursiveReferenceInSubquery;
		}

		private void ValidateColumnNames(SelectColumn[] columns, CommonTableExpression commonTableExpression)
		{
			if (commonTableExpression.ColumnNames == null)
			{
				// Check that all columns have aliases.
				for (int i = 0; i < columns.Length; i++)
				{
					SelectColumn selectColumn = columns[i];
					if (selectColumn.Alias == null)
						ErrorReporter.NoColumnAliasSpecified(commonTableExpression.TableNameSourceRange, i, commonTableExpression.TableName);
				}
			}
			else
			{
				if (commonTableExpression.ColumnNames.Length < columns.Length)
					ErrorReporter.CteHasMoreColumnsThanSpecified(commonTableExpression.TableName);
				else if (commonTableExpression.ColumnNames.Length > columns.Length)
					ErrorReporter.CteHasFewerColumnsThanSpecified(commonTableExpression.TableName);
				else
				{
					// Check that all specified column names are unique.
					Dictionary<string, object> nameSet = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
					foreach (Identifier columnName in commonTableExpression.ColumnNames)
					{
						if (nameSet.ContainsKey(columnName.Text))
							ErrorReporter.CteHasDuplicateColumnName(columnName, commonTableExpression.TableName);
						else
							nameSet.Add(columnName.Text, null);
					}
				}

				if (!ErrorReporter.ErrorsSeen)
				{
					// Write explictly given column names into the real column list as aliases.
					for (int i = 0; i < columns.Length; i++)
						columns[i].Alias = commonTableExpression.ColumnNames[i];
				}
			}
		}

		#endregion

		public override ExpressionNode VisitNameExpression(NameExpression expression)
		{
			// Resolve column
			// -- or --
			// Resolve constant
			// -- or --
			// Resolve parameter (without @)
			// -- or --
			// Reference to row object

			ColumnRefBinding columnRefBinding = ResolveColumnRef(expression.NameSourceRange, expression.Name);
			ConstantBinding constantBinding = ResolveConstant(expression.NameSourceRange, expression.Name);
			ParameterBinding parameterBinding = ResolveParameter(expression.NameSourceRange, expression.Name);
			TableRefBinding tableRefBinding = ResolveTableRef(expression.NameSourceRange, expression.Name);

			if (columnRefBinding != null)
			{
				if (constantBinding != null)
					ErrorReporter.AmbiguousReference(expression.NameSourceRange, expression.Name, new Binding[] { columnRefBinding, constantBinding });

				if (parameterBinding != null)
					ErrorReporter.AmbiguousReference(expression.NameSourceRange, expression.Name, new Binding[] { columnRefBinding, parameterBinding });

				if (tableRefBinding != null)
					ErrorReporter.AmbiguousReference(expression.NameSourceRange, expression.Name, new Binding[] { columnRefBinding, tableRefBinding });

				return new ColumnExpression(columnRefBinding);
			}

			if (constantBinding != null)
			{
				if (parameterBinding != null)
					ErrorReporter.AmbiguousReference(expression.NameSourceRange, expression.Name, new Binding[] { constantBinding, parameterBinding });

				if (tableRefBinding != null)
					ErrorReporter.AmbiguousReference(expression.NameSourceRange, expression.Name, new Binding[] { constantBinding, tableRefBinding });

				return new NamedConstantExpression(constantBinding);
			}

			if (parameterBinding != null)
			{
				if (tableRefBinding != null)
					ErrorReporter.AmbiguousReference(expression.NameSourceRange, expression.Name, new Binding[] { parameterBinding, tableRefBinding });

				ParameterExpression parameterExpression = new ParameterExpression();
				parameterExpression.Name = expression.Name;
				parameterExpression.NameSourceRange = expression.NameSourceRange;
				parameterExpression.Parameter = parameterBinding;
				return parameterExpression;
			}

			if (tableRefBinding != null)
			{
				if (tableRefBinding.TableBinding is DerivedTableBinding ||
					tableRefBinding.TableBinding is CommonTableBinding)
				{
					ErrorReporter.InvalidRowReference(expression.NameSourceRange, tableRefBinding);
					return expression;
				}
				else
				{
					ColumnRefBinding cab = tableRefBinding.Scope.DeclareRowColumnRef(tableRefBinding);
					return new ColumnExpression(cab);
				}
			}

			// Check if there is any function with this name declared. This helps to give a better
			// error message.

			FunctionBinding[] functionBindings = _scope.DataContext.Functions.Find(expression.Name);

			if (functionBindings != null && functionBindings.Length > 0)
			{
				// Report that parentheses are required in function calls.
				ErrorReporter.InvocationRequiresParentheses(expression.NameSourceRange, functionBindings);
			}
			else
			{
				// Report the regular undeclared entity error message.
				ErrorReporter.UndeclaredEntity(expression.NameSourceRange, expression.Name);
			}

			return expression;
		}

		public override ExpressionNode VisitParameterExpression(ParameterExpression expression)
		{
			ParameterBinding parameter = ResolveParameter(expression.NameSourceRange, expression.Name);

			if (parameter == null)
				ErrorReporter.UndeclaredParameter(expression.NameSourceRange, expression.Name);

			expression.Parameter = parameter;

			return expression;
		}

		public override ExpressionNode VisitPropertyAccessExpression(PropertyAccessExpression expression)
		{
			// Resolve column
			// -- or --
			// Resolve property

			NameExpression targetAsNameExpression = expression.Target as NameExpression;

			if (targetAsNameExpression != null)
			{
				// Ok, the target is a name expression. Lets see whether it refers to a table
				// or constant.

				TableRefBinding tableRefBinding = ResolveTableRef(targetAsNameExpression.NameSourceRange, targetAsNameExpression.Name);
				ConstantBinding constantBinding = ResolveConstant(targetAsNameExpression.NameSourceRange, targetAsNameExpression.Name);

				if (tableRefBinding != null)
				{
					if (constantBinding != null)
					{
						// It can both refer to a constant and a table. We cannot resolve
						// the ambiguity so we report an error and do nothing.
						ErrorReporter.AmbiguousReference(expression.NameSourceRange, expression.Name, new Binding[] { constantBinding, tableRefBinding });
						return expression;
					}

					// The target only refers to table. Resolve the column or method on the row type.

					ColumnRefBinding columnRefBinding = ResolveColumnRef(tableRefBinding, expression.NameSourceRange, expression.Name);

					if (columnRefBinding != null)
						return new ColumnExpression(columnRefBinding);

					if (!(tableRefBinding.TableBinding is DerivedTableBinding) && !(tableRefBinding.TableBinding is CommonTableBinding))
					{
						// Check if there is any method with this name declared. This helps to give a better
						// error message.

						MethodBinding[] methodBindings = _scope.DataContext.MetadataContext.FindMethod(tableRefBinding.TableBinding.RowType, expression.Name);

						if (methodBindings != null && methodBindings.Length > 0)
						{
							// Report that methods calls require parentheses
							ErrorReporter.InvocationRequiresParentheses(expression.NameSourceRange, methodBindings);
							return expression;
						}
					}

					// Report that no column with this name exists.
					ErrorReporter.UndeclaredColumn(expression.NameSourceRange, tableRefBinding, expression.Name);
					return expression;
				}

				// NOTE: If name does not refer to a table but to constant
				//       we fall through. The name node is resolved below
				//       (actually it is replaced in VisitNameExpression)
			}

			// Ok, it is not a table. Try to resolve a property.

			expression.Target = VisitExpression(expression.Target);

			if (expression.Target.ExpressionType == null)
				return expression;

			// To support custom properties we check if the target is a named constant or a parameter.

			NamedConstantExpression namedConstantExpression = expression.Target as NamedConstantExpression;
			ParameterExpression parameterExpression = expression.Target as ParameterExpression;

			if (namedConstantExpression == null && parameterExpression == null)
			{
				// We cannot use custom properties in this case. Therfore
				// we resolve the property using type specific properties.

				expression.Property = ResolveTypeProperty(expression.Target.ExpressionType, expression.NameSourceRange, expression.Name);
			}
			else
			{
				// We can use custom properties. Get that value and the dataype.

				IList<PropertyBinding> customProperties;

				if (namedConstantExpression != null)
					customProperties = namedConstantExpression.Constant.CustomProperties;
				else
					customProperties = parameterExpression.Parameter.CustomProperties;

				if (customProperties != null)
				{
					// Resolve instance property.
					expression.Property = ResolveCustomProperty(customProperties, expression.NameSourceRange, expression.Name);
				}
				else
				{
					// The constant or parameter did not have custom properties.
					expression.Property = ResolveTypeProperty(expression.Target.ExpressionType, expression.NameSourceRange, expression.Name);
				}
			}

			if (expression.Property == null)
			{
				// Check if there is any method with this name declared. This helps to give a better
				// error message.

				MethodBinding[] methodBindings = _scope.DataContext.MetadataContext.FindMethod(expression.Target.ExpressionType, expression.Name);

				if (methodBindings != null && methodBindings.Length > 0)
				{
					// Report that methods calls require parentheses
					ErrorReporter.InvocationRequiresParentheses(expression.NameSourceRange, methodBindings);
				}
				else
				{
					// Report that no property with this name exists.
					ErrorReporter.UndeclaredProperty(expression.NameSourceRange, expression.Target.ExpressionType, expression.Name);
				}
			}

			return expression;
		}

		public override ExpressionNode VisitCastExpression(CastExpression expression)
		{
			expression.Expression = VisitExpression(expression.Expression);

			if (expression.TypeReference.ResolvedType == null)
			{
				Type targetType = Binder.BindType(expression.TypeReference.TypeName, expression.TypeReference.CaseSensitve);
				expression.TypeReference.ResolvedType = targetType;

				if (targetType == null)
				{
					ErrorReporter.UndeclaredType(expression.TypeReference.TypeNameSourceRange, expression.TypeReference.TypeName);
				}
				else if (expression.Expression.ExpressionType != null)
				{
					return Binder.ConvertOrDowncastExpressionIfRequired(expression.Expression, targetType);
				}
			}

			return expression;
		}

		public override ExpressionNode VisitFunctionInvocationExpression(FunctionInvocationExpression expression)
		{
			// Resolve aggregation
			// -- or --
			// Resolve method

			// First all parameters must be resolved.

			base.VisitFunctionInvocationExpression(expression);

			// If the type of any argument could not be resolved we cannot resolve the method.

			foreach (ExpressionNode argument in expression.Arguments)
			{
				if (argument.ExpressionType == null)
					return expression;
			}

			// Resolve aggregate

			bool canBeAggregate = expression.Arguments.Length == 1 || (expression.Arguments.Length == 0 && expression.HasAsteriskModifier);
			if (canBeAggregate)
			{
				AggregateBinding aggregateBinding = ResolveAggregate(expression.NameSourceRange, expression.Name);

				if (aggregateBinding != null)
				{
					ExpressionNode aggregateArgument;

					if (!expression.HasAsteriskModifier)
					{
						aggregateArgument = expression.Arguments[0];
					}
					else
					{
						// Only COUNT can have the asterisk modifier.

						Identifier countName = Identifier.CreateNonVerbatim("COUNT");
						if (!countName.Matches(aggregateBinding.Name))
							ErrorReporter.AsteriskModifierNotAllowed(expression.NameSourceRange, expression);

						// The semantic of COUNT(*) says that it counts all rows of the bound
						// query. The same result can be accomplished by using COUNT(0) (or any 
						// otherc onstant non-null expression as argument). Therefore we use a 
						// literal zero as the argument.
						aggregateArgument = LiteralExpression.FromInt32(0);
					}

					IAggregator aggregator = aggregateBinding.CreateAggregator(aggregateArgument.ExpressionType);

					if (aggregator == null)
					{
						ErrorReporter.AggregateDoesNotSupportType(aggregateBinding, aggregateArgument.ExpressionType);
						return expression;
					}

					AggregateExpression aggregateExpression = new AggregateExpression();
					aggregateExpression.Aggregate = aggregateBinding;
					aggregateExpression.Aggregator = aggregator;
					aggregateExpression.Argument = aggregateArgument;
					aggregateExpression.HasAsteriskModifier = expression.HasAsteriskModifier;
					return aggregateExpression;
				}
			}

			// Resolve method

			if (expression.HasAsteriskModifier)
			{
				// Simple invocations cannot have asterisk modifier.
				ErrorReporter.AsteriskModifierNotAllowed(expression.NameSourceRange, expression);

				// Leave to avoid cascading errors.
				return expression;
			}

			Type[] argumentTypes = new Type[expression.Arguments.Length];
			for (int i = 0; i < argumentTypes.Length; i++)
				argumentTypes[i] = expression.Arguments[i].ExpressionType;

			expression.Function = ResolveFunction(expression.Name, argumentTypes);

			if (expression.Function == null)
			{
				ErrorReporter.UndeclaredFunction(expression.NameSourceRange, expression.Name, argumentTypes);
			}
			else
			{
				// Convert all arguments if necessary

				Type[] parameterTypes = expression.Function.GetParameterTypes();

				for (int i = 0; i < expression.Arguments.Length; i++)
					expression.Arguments[i] = Binder.ConvertExpressionIfRequired(expression.Arguments[i], parameterTypes[i]);
			}

			return expression;
		}

		public override ExpressionNode VisitMethodInvocationExpression(MethodInvocationExpression expression)
		{
			// First the target and all parameters must be resolved.

			base.VisitMethodInvocationExpression(expression);

			// If the type of the target could not be resolved we cannot resolve the method.

			if (expression.Target.ExpressionType == null)
				return expression;

			// If the type of any argument could not be resolved we cannot resolve the method.

			foreach (ExpressionNode argument in expression.Arguments)
			{
				if (argument.ExpressionType == null)
					return expression;
			}

			// Build argument type array.

			Type[] argumentTypes = new Type[expression.Arguments.Length];
			for (int i = 0; i < argumentTypes.Length; i++)
				argumentTypes[i] = expression.Arguments[i].ExpressionType;

			expression.Method = ResolveMethod(expression.Target.ExpressionType, expression.Name, argumentTypes);

			if (expression.Method == null)
			{
				ErrorReporter.UndeclaredMethod(expression.NameSourceRange, expression.Target.ExpressionType, expression.Name, argumentTypes);
			}
			else
			{
				// Convert all arguments if necessary

				Type[] parameterTypes = expression.Method.GetParameterTypes();

				for (int i = 0; i < expression.Arguments.Length; i++)
					expression.Arguments[i] = Binder.ConvertExpressionIfRequired(expression.Arguments[i], parameterTypes[i]);
			}

			return expression;
		}

		public override TableReference VisitJoinedTableReference(JoinedTableReference node)
		{
			// Resolve children

			node.Left = VisitTableReference(node.Left);
			node.Right = VisitTableReference(node.Right);

			if (node.Condition != null)
			{
				node.Condition = VisitExpression(node.Condition);

				// Get all tables that are referenced by node.Condition.

				MetaInfo metaInfo = AstUtil.GetMetaInfo(node.Condition);

				// Now get a list with all tables are really introduced by one of the node's
				// children. 

				TableDeclarationFinder tableDeclarationFinder = new TableDeclarationFinder();
				tableDeclarationFinder.Visit(node.Left);
				tableDeclarationFinder.Visit(node.Right);
				TableRefBinding[] declaredTables = tableDeclarationFinder.GetDeclaredTables();

				// Any tables that are not declared by children of node are inaccessible in the 
				// current context and referencing them in node.Condition is invalid.
				//
				// NOTE: This is only partially true. It could also be an outer reference. Therefore
				//       we also check that the scope is the same.

				foreach (TableRefBinding referencedTable in metaInfo.TableDependencies)
				{
					if (referencedTable.Scope == CurrentScope)
					{
						if (!ArrayHelpers.Contains(declaredTables, referencedTable))
							ErrorReporter.TableRefInaccessible(referencedTable);
					}
				}
			}

			return node;
		}

		public override TableReference VisitDerivedTableReference(DerivedTableReference node)
		{
			// Don't visit the query since we have visited it in VisitSelectQuery() already.
			return node;
		}

		public override QueryNode VisitCommonTableExpressionQuery(CommonTableExpressionQuery query)
		{
			QueryScope queryScope = new QueryScope(null);
			PushNewScope(queryScope);
			try
			{
				foreach (CommonTableExpression commonTableExpression in query.CommonTableExpressions)
				{
					CommonTableBinding[] existingCommonTables = queryScope.FindCommonTable(commonTableExpression.TableName);
					if (existingCommonTables != null && existingCommonTables.Length > 0)
					{
						ErrorReporter.CteHasDuplicateTableName(commonTableExpression.TableName);
						break;
					}

					// Check if CTE is recursive.
					bool recursive = IsRecursive(commonTableExpression.QueryDeclaration, commonTableExpression.TableName);

					if (!recursive)
					{
						commonTableExpression.QueryDeclaration = VisitQuery(commonTableExpression.QueryDeclaration);
						ValidateColumnNames(commonTableExpression.QueryDeclaration.GetColumns(), commonTableExpression);

						if (ErrorReporter.ErrorsSeen)
							return query;

						commonTableExpression.CommonTableBinding = queryScope.DeclareCommonTableExpression(commonTableExpression.TableName, commonTableExpression.QueryDeclaration);
					}
					else
					{
						// If recursive, we must check the structure. The structure is as follows:
						//
						//    {One or more anchor members}
						//    UNION ALL
						//    {One or more recursive members}

						BinaryQuery unionAllQuery = commonTableExpression.QueryDeclaration as BinaryQuery;
						if (unionAllQuery == null || unionAllQuery.Op != BinaryQueryOperator.UnionAll)
						{
							ErrorReporter.CteDoesNotHaveUnionAll(commonTableExpression.TableName);
							break;
						}

						List<QueryNode> recursiveMembers = AstUtil.FlattenBinaryQuery(unionAllQuery);
						List<QueryNode> anchorMembers = new List<QueryNode>();
						foreach (QueryNode queryNode in recursiveMembers)
						{
							if (!IsRecursive(queryNode, commonTableExpression.TableName))
								anchorMembers.Add(queryNode);
							else
								break;
						}
						recursiveMembers.RemoveRange(0, anchorMembers.Count);
						QueryNode anchorMember = AstUtil.CombineQueries(anchorMembers, BinaryQueryOperator.UnionAll);

						if (anchorMembers.Count == 0)
						{
							ErrorReporter.CteDoesNotHaveAnchorMember(commonTableExpression.TableName);
							return query;
						}

						// Resolve anchor member and use it to construct a common table definition.
						anchorMember = VisitQuery(anchorMember);

						// Check that all columns have aliases.
						ValidateColumnNames(anchorMember.GetColumns(), commonTableExpression);

						if (ErrorReporter.ErrorsSeen)
							return query;

						commonTableExpression.CommonTableBinding = queryScope.DeclareCommonTableExpression(commonTableExpression.TableName, anchorMember);
						SelectColumn[] anchorColumns = anchorMember.GetColumns();

						// Now resolve all recursive members and add them to the common table definition.
						for (int i = 0; i < recursiveMembers.Count; i++)
						{
							recursiveMembers[i] = VisitQuery(recursiveMembers[i]);

							// Make sure the column count and data type match.
							// NOTE: Due to the recursive nature there is no implicit conversion support for the UNION ALL
							//       in common table expressions. Instead, the types must match exactly.
							SelectColumn[] recursiveColumns = recursiveMembers[i].GetColumns();
							if (recursiveColumns.Length != anchorColumns.Length)
								ErrorReporter.DifferentExpressionCountInBinaryQuery();
							else
							{
								for (int columnIndex = 0; columnIndex < anchorColumns.Length; columnIndex++)
								{
									Type anchorColumnType = anchorColumns[columnIndex].Expression.ExpressionType;
									Type recursiveColumnType = recursiveColumns[columnIndex].Expression.ExpressionType;
									if (recursiveColumnType != null && recursiveColumnType != anchorColumnType)
										ErrorReporter.CteHasTypeMismatchBetweenAnchorAndRecursivePart(anchorColumns[columnIndex].Alias, commonTableExpression.TableName);
								}
							}
						}
						commonTableExpression.CommonTableBinding.RecursiveMembers = recursiveMembers.ToArray();
					}
				}

				if (ErrorReporter.ErrorsSeen)
					return query;

				query.Input = VisitQuery(query.Input);
			}
			finally
			{
				PopScope();
			}
			return query;
		}

		public override QueryNode VisitBinaryQuery(BinaryQuery query)
		{
			base.VisitBinaryQuery(query);

			if (ErrorReporter.ErrorsSeen)
				return query;

			SelectColumn[] leftSelectColumns = query.Left.GetColumns();
			SelectColumn[] rightSelectColumns = query.Right.GetColumns();

			if (leftSelectColumns.Length != rightSelectColumns.Length)
			{
				ErrorReporter.DifferentExpressionCountInBinaryQuery();
			}
			else
			{
				// Check that all column expressions share a common type.
				//
				// If the types are not equal an CAST node is inserted in the tree.

				// To do this and to support good error reporting we first try to find
				// the best common type. Any needed conversions or type errors are
				// ignored.

				Type[] commonTypes = new Type[leftSelectColumns.Length];

				for (int i = 0; i < leftSelectColumns.Length; i++)
				{
					Type leftType = leftSelectColumns[i].Expression.ExpressionType;
					Type rightType = rightSelectColumns[i].Expression.ExpressionType;
					commonTypes[i] = Binder.ChooseBetterTypeConversion(leftType, rightType);
				}

				// Now we know that commonType is the best type for all column expressions.
				//
				// Insert cast nodes for all expressions that have a different type but are
				// implicit convertible and report errors for all expressions that not convertible.

				for (int i = 0; i < leftSelectColumns.Length; i++)
				{
					SelectColumn leftSelectColumn = leftSelectColumns[i];
					SelectColumn rightSelectColumn = rightSelectColumns[i];

					leftSelectColumn.Expression = Binder.ConvertExpressionIfRequired(leftSelectColumn.Expression, commonTypes[i]);
					rightSelectColumn.Expression = Binder.ConvertExpressionIfRequired(rightSelectColumn.Expression, commonTypes[i]);
				}
			}

			return query;
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public override QueryNode VisitSelectQuery(SelectQuery query)
		{
			query.QueryScope = PushNewScope(query.QueryScope);
			try
			{
				if (query.TableReferences != null)
				{
					// Declare all tables

					TableReferenceDeclarationFinder tableReferenceDeclarationFinder = new TableReferenceDeclarationFinder();
					tableReferenceDeclarationFinder.Visit(query.TableReferences);

					NamedTableReference[] namedTableReferences = tableReferenceDeclarationFinder.GetNamedTableReferences();
					DerivedTableReference[] derivedTableReferences = tableReferenceDeclarationFinder.GetDerivedTableReferences();

					foreach (NamedTableReference namedTableReference in namedTableReferences)
					{
						TableBinding tableBinding = ResolveTable(namedTableReference.TableNameSourceRange, namedTableReference.TableName);

						if (tableBinding == null)
						{
							ErrorReporter.UndeclaredTable(namedTableReference.TableNameSourceRange, namedTableReference.TableName);
						}
						else
						{
							Identifier tableReferenceIdentifer;

							if (namedTableReference.CorrelationName == null)
								tableReferenceIdentifer = Identifier.CreateVerbatim(tableBinding.Name);
							else
								tableReferenceIdentifer = namedTableReference.CorrelationName;

							TableRefBinding existingTableRef = ResolveTableRef(SourceRange.None, tableReferenceIdentifer);

							if (existingTableRef != null && existingTableRef.Scope == query.QueryScope)
								ErrorReporter.DuplicateTableRefInFrom(tableReferenceIdentifer);
							else
								namedTableReference.TableRefBinding = CurrentScope.DeclareTableRef(tableBinding, tableReferenceIdentifer);
						}
					}

					foreach (DerivedTableReference derivedTableReference in derivedTableReferences)
					{
						derivedTableReference.Query = VisitQuery(derivedTableReference.Query);

						// Make sure we are only declaring derived tables that have aliases for all expressions.
						SelectColumn[] selectColumns = derivedTableReference.Query.GetColumns();
						for (int i = 0; i < selectColumns.Length; i++)
						{
							if (selectColumns[i].Alias == null)
								ErrorReporter.NoColumnAliasSpecified(derivedTableReference.CorrelationNameSourceRange, i, derivedTableReference.CorrelationName);
						}

						if (!ErrorReporter.ErrorsSeen)
						{
							TableRefBinding existingTableRef = ResolveTableRef(SourceRange.None, derivedTableReference.CorrelationName);

							if (existingTableRef != null && existingTableRef.Scope == query.QueryScope)
								ErrorReporter.DuplicateTableRefInFrom(derivedTableReference.CorrelationName);
							else
							{
								DerivedTableBinding derivedTableBinding = new DerivedTableBinding(derivedTableReference.CorrelationName.Text, derivedTableReference.Query);
								derivedTableReference.DerivedTableBinding = query.QueryScope.DeclareTableRef(derivedTableBinding, derivedTableReference.CorrelationName);
							}
						}
					}

					// If we could not declare all tables, we stop resolving this query.
					if (ErrorReporter.ErrorsSeen)
						return query;

					// Resolve joins

					query.TableReferences = VisitTableReference(query.TableReferences);
				}

				// Ensure that we tables specified if using the asterisk.

				if (query.TableReferences == null)
				{
					foreach (SelectColumn column in query.SelectColumns)
					{
						if (column.IsAsterisk)
							ErrorReporter.MustSpecifyTableToSelectFrom();
					}
				}

				// If we could not resolve all tables or there are duplicates
				// we must stop here.

				if (ErrorReporter.ErrorsSeen)
					return query;

				// Expand all * and <alias>.* columns with the corresponding columns.
				//
				// Build a list of all column sources to replace the query.SelectColumns property.

				List<SelectColumn> columnSources = new List<SelectColumn>();

				foreach (SelectColumn columnSource in query.SelectColumns)
				{
					if (!columnSource.IsAsterisk)
					{
						// Nothing to expand, just add the column source
						// to the list.
						columnSources.Add(columnSource);
					}
					else
					{
						// Expand the asterisk.

						if (columnSource.Alias == null)
						{
							// No alias, expand the asterisk to all columns
							// of all tables.

							foreach (TableRefBinding tableRefBinding in CurrentScope.GetAllTableRefBindings())
							{
								foreach (ColumnBinding columnBinding in tableRefBinding.TableBinding.Columns)
								{
									ColumnRefBinding columnRefBinding = CurrentScope.GetColumnRef(tableRefBinding, columnBinding);
									columnSources.Add(new SelectColumn(new ColumnExpression(columnRefBinding), null));
								}
							}
						}
						else
						{
							// Resolve the alias of the column to a table and just
							// expand the asterisk to the columns of this table.

							TableRefBinding tableRefBinding = ResolveTableRef(SourceRange.None, columnSource.Alias);

							if (tableRefBinding == null)
							{
								ErrorReporter.UndeclaredTable(SourceRange.None, columnSource.Alias);
							}
							else
							{
								foreach (ColumnBinding columnDefinition in tableRefBinding.TableBinding.Columns)
								{
									ColumnRefBinding columnRefBinding = CurrentScope.GetColumnRef(tableRefBinding, columnDefinition);
									columnSources.Add(new SelectColumn(new ColumnExpression(columnRefBinding), null));
								}
							}
						}
					}
				}

				// Now we have expanded all asterisks so we can replace the query.SelectColumns
				// property with the expanded list.

				query.SelectColumns = columnSources.ToArray();

				// Resolve 
				// - all column selection expressions
				// - WHERE clause,
				// - GROUP BY clause,
				// - HAVING clause
				// - ORDER BY clause.

				for (int i = 0; i < query.SelectColumns.Length; i++)
					query.SelectColumns[i].Expression = VisitExpression(query.SelectColumns[i].Expression);

				if (query.WhereClause != null)
					query.WhereClause = VisitExpression(query.WhereClause);

				if (query.GroupByColumns != null)
				{
					for (int i = 0; i < query.GroupByColumns.Length; i++)
						query.GroupByColumns[i] = VisitExpression(query.GroupByColumns[i]);
				}

				if (query.HavingClause != null)
					query.HavingClause = VisitExpression(query.HavingClause);

				if (query.OrderByColumns != null)
					ResolveOrderBy(query.SelectColumns, query.OrderByColumns);

				// Infer column aliases for the following three expressions:
				//
				// - ColumnExpression
				// - PropertyExpression
				// - NamedConstantExpression
				//
				// In regular SQL only the first one is possible.

				foreach (SelectColumn columnSource in query.SelectColumns)
				{
					if (columnSource.Alias == null)
					{
						ColumnExpression exprAsColumnExpression = columnSource.Expression as ColumnExpression;
						PropertyAccessExpression exprAsPropertyAccessExpression = columnSource.Expression as PropertyAccessExpression;
						NamedConstantExpression exprAsNamedConstant = columnSource.Expression as NamedConstantExpression;

						if (exprAsColumnExpression != null)
							columnSource.Alias = Identifier.CreateVerbatim(exprAsColumnExpression.Column.Name);
						else if (exprAsPropertyAccessExpression != null)
							columnSource.Alias = exprAsPropertyAccessExpression.Name;
						else if (exprAsNamedConstant != null)
							columnSource.Alias = Identifier.CreateVerbatim(exprAsNamedConstant.Constant.Name);
					}
				}

				// Ensure WHERE clause is a boolean expression

				if (query.WhereClause != null)
				{
					if (query.WhereClause.ExpressionType == typeof(DBNull))
					{
						// This means the user entered something like SELECT ... WHERE null = null
						//
						// NOTE: We cannot test on literals since constant folding will be applied
						//       later.

						query.WhereClause = LiteralExpression.FromBoolean(false);
					}
					else if (query.WhereClause.ExpressionType != null &&
							 query.WhereClause.ExpressionType != typeof(bool))
					{
						ErrorReporter.WhereClauseMustEvaluateToBool();
					}
				}

				// Ensure HAVING clause is boolean expression

				if (query.HavingClause != null)
				{
					if (query.HavingClause.ExpressionType == typeof(DBNull))
					{
						// See WHERE clause handler above.

						query.HavingClause = LiteralExpression.FromBoolean(false);
					}
					else if (query.HavingClause.ExpressionType != null &&
							 query.HavingClause.ExpressionType != typeof(bool))
					{
						ErrorReporter.HavingClauseMustEvaluateToBool();
					}
				}

				MetaInfo metaInfo = AstUtil.GetMetaInfo(query);
				query.ColumnDependencies = metaInfo.ColumnDependencies;

				return query;
			}
			finally
			{
				PopScope();
			}
		}

		public override QueryNode VisitSortedQuery(SortedQuery query)
		{
			// It is legal but meaningless to sort a select query that have no
			// tables. So we simply optimize this SortedQuery node away.

			SelectQuery selectQuery = query.Input as SelectQuery;
			if (selectQuery != null && selectQuery.TableReferences == null)
				return VisitQuery(query.Input);

			// First we resolve the input.

			query.Input = VisitQuery(query.Input);

			// Find innermost SELECT query.
			//
			// This is the SELECT query the COLUMN definition is bought from.

			QueryNode currentInput = query.Input;
			SelectQuery innerMostInput = null;

			while (innerMostInput == null)
			{
				innerMostInput = currentInput as SelectQuery;

				if (innerMostInput == null)
				{
					if (currentInput.NodeType == AstNodeType.BinaryQuery)
					{
						BinaryQuery inputAsBinaryQuery = (BinaryQuery)currentInput;
						currentInput = inputAsBinaryQuery.Left;
					}
					else
					{
						throw ExceptionBuilder.InternalError("Unexpected input node: {0}\nSource:{1}", currentInput.NodeType, currentInput.GenerateSource());
					}

				}
			}

			PushScope(innerMostInput.QueryScope);

			try
			{
				ResolveOrderBy(query.Input.GetColumns(), query.OrderByColumns);
				return query;
			}
			finally
			{
				PopScope();
			}
		}
	}
}
