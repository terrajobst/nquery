using System;
using System.Collections.Generic;

using NQuery.Runtime;

namespace NQuery.Compilation
{
	internal sealed class SubqueryExpander : StandardVisitor
	{
		private ExpressionNode _currentPassthruPredicate;
		private AlgebraNode _lastAlgebraNode;
		private ExpressionBuilder _expressionBuilder = new ExpressionBuilder();
		private Stack<bool> _probingEnabledStack = new Stack<bool>();

		#region Passthru Management

		private void SetWhenPassthru(ICollection<ExpressionNode> processedWhenExpressions)
		{
			if (processedWhenExpressions.Count == 0)
			{
				_currentPassthruPredicate = null;
			}
			else
			{
				foreach (ExpressionNode processedWhenExpression in processedWhenExpressions)
				{
					ExpressionNode clonedExpr = (ExpressionNode) processedWhenExpression.Clone();
					_expressionBuilder.Push(clonedExpr);
				}

				_expressionBuilder.PushNAry(LogicalOperator.Or);
				_currentPassthruPredicate = _expressionBuilder.Pop();
			}
		}

		private void SetThenPassthru(ICollection<ExpressionNode> processedWhenExpressions)
		{
			if (processedWhenExpressions.Count == 0)
			{
				_currentPassthruPredicate = null;
			}
			else
			{
				foreach (ExpressionNode processedWhenExpression in processedWhenExpressions)
				{
					ExpressionNode clonedExpr = (ExpressionNode)processedWhenExpression.Clone();
					_expressionBuilder.Push(clonedExpr);
				}
				_expressionBuilder.PushUnary(UnaryOperator.LogicalNot);
				_expressionBuilder.PushNAry(LogicalOperator.Or);

				_currentPassthruPredicate = _expressionBuilder.Pop();
			}
		}

		private ExpressionNode CurrentPassthruPredicate
		{
			get
			{
				return _currentPassthruPredicate;
			}
		}

		#endregion

		#region Probing
		
		private bool ProbingEnabled
		{
			get { return _probingEnabledStack.Peek(); }
		}

		private RowBufferEntry CreateProbeColumn()
		{
			if (!ProbingEnabled)
				return null;

			RowBufferEntry probeColumn = new RowBufferEntry(typeof(bool));
			return probeColumn;
		}

		private ExpressionNode CreateProbeColumnRef(RowBufferEntry probeColumn)
		{
			if (!ProbingEnabled)
				return LiteralExpression.FromBoolean(true);

			RowBufferEntryExpression probeColumnRef = new RowBufferEntryExpression(probeColumn);
			return probeColumnRef;
		}

		#endregion
		
		#region Last Algebra Node Handling

		private void SetLastAlgebraNode(AlgebraNode algebraNode)
		{
			_lastAlgebraNode = algebraNode;
		}

		private AlgebraNode GetAndResetLastNode()
		{
			AlgebraNode result = _lastAlgebraNode;
			_lastAlgebraNode = null;
			return result;
		}

		private AlgebraNode GetOrCreateInput()
		{
			AlgebraNode lastNode = GetAndResetLastNode();
			if (lastNode != null)
				return lastNode;

			return CreateConstantScan();
		}

		private static AlgebraNode CreateConstantScan()
		{
			ConstantScanAlgebraNode constantScanAlgebraNode = new ConstantScanAlgebraNode();
			constantScanAlgebraNode.DefinedValues = new ComputedValueDefinition[0];
			return constantScanAlgebraNode;
		}

		private static ResultAlgebraNode CreateAssertedSubquery(ResultAlgebraNode inputNode)
		{
			if (AstUtil.WillProduceAtMostOneRow(inputNode))
				return inputNode;

			RowBufferEntry inputEntry = inputNode.OutputList[0];

			AggregatedValueDefinition countDefinedValue = new AggregatedValueDefinition();
			countDefinedValue.Aggregate = new CountAggregateBinding("COUNT");
			countDefinedValue.Aggregator = countDefinedValue.Aggregate.CreateAggregator(typeof(int));
			countDefinedValue.Argument = LiteralExpression.FromInt32(0);

			RowBufferEntry countDefinedValueEntry = new RowBufferEntry(countDefinedValue.Aggregator.ReturnType);
			countDefinedValue.Target = countDefinedValueEntry;

			RowBufferEntryExpression anyAggregateArgument = new RowBufferEntryExpression();
			anyAggregateArgument.RowBufferEntry = inputEntry;

			AggregatedValueDefinition anyDefinedValue = new AggregatedValueDefinition();
			anyDefinedValue.Aggregate = new FirstAggregateBinding("ANY");
			anyDefinedValue.Aggregator = anyDefinedValue.Aggregate.CreateAggregator(inputEntry.DataType);
			anyDefinedValue.Argument = anyAggregateArgument;
			
			RowBufferEntry anyDefinedValueEntry = new RowBufferEntry(inputEntry.DataType);
			anyDefinedValue.Target = anyDefinedValueEntry;

			AggregateAlgebraNode aggregateAlgebraNode = new AggregateAlgebraNode();
			aggregateAlgebraNode.Input = inputNode.Input;
			aggregateAlgebraNode.DefinedValues = new AggregatedValueDefinition[] { countDefinedValue, anyDefinedValue };

			// CASE WHEN SubqueryCount > 1 THEN 0 ELSE NULL END

			ExpressionBuilder expressionBuilder = new ExpressionBuilder();
			expressionBuilder.Push(new RowBufferEntryExpression(countDefinedValueEntry));
			expressionBuilder.Push(LiteralExpression.FromInt32(1));
			expressionBuilder.PushBinary(BinaryOperator.Greater);
			ExpressionNode whenExpression = expressionBuilder.Pop();
			ExpressionNode thenExpression = LiteralExpression.FromInt32(0);

			CaseExpression caseExpression = new CaseExpression();
			caseExpression.WhenExpressions = new ExpressionNode[] { whenExpression };
			caseExpression.ThenExpressions = new ExpressionNode[] { thenExpression };

			expressionBuilder.Push(caseExpression);
			ExpressionNode predicate = expressionBuilder.Pop();

			AssertAlgebraNode assertAlgebraNode = new AssertAlgebraNode();
			assertAlgebraNode.Input = aggregateAlgebraNode;
			assertAlgebraNode.Predicate = predicate;
			assertAlgebraNode.AssertionType = AssertionType.MaxOneRow;

			ResultAlgebraNode resultAlgebraNode = new ResultAlgebraNode();
			resultAlgebraNode.Input = assertAlgebraNode;
			resultAlgebraNode.OutputList = new RowBufferEntry[] { anyDefinedValueEntry };
			resultAlgebraNode.ColumnNames = inputNode.ColumnNames;

			return resultAlgebraNode;
		}

		#endregion

		public override AstNode Visit(AstNode node)
		{
			if (!(node is ExpressionNode))
			{
				// Non-expressions are visited as usual.
				return base.Visit(node);
			}

			// First, we only visit expressions that contain a subquery.
			//
			// For correct handling of ProbeColumn we to decect whether probing
			// is required or not. Probing is only needed if the existance subquery
			// appears in an expression that is not a logical AND or logical OR (or
			// an existence subquery itself).

			if (AstUtil.ContainsSubselect(node))
			{
				BinaryExpression binaryExpression = node as BinaryExpression;
				bool isLogicalLink = binaryExpression != null &&
				                     (binaryExpression.Op == BinaryOperator.LogicalAnd ||
				                      binaryExpression.Op == BinaryOperator.LogicalOr);

				bool isExistenceSubquery = node.NodeType == AstNodeType.AllAnySubselect ||
				                           node.NodeType == AstNodeType.ExistsSubselect;

				bool requiresProbing = !isLogicalLink && !isExistenceSubquery;

				if (requiresProbing)
					_probingEnabledStack.Push(true);
				
				AstNode resultExpression = base.Visit(node);

				if (requiresProbing)
					_probingEnabledStack.Pop();

				return resultExpression;
			}

			// Don't visit expressions that do not contain suqueries.
			return node;
		}

		public override ExpressionNode VisitCaseExpression(CaseExpression expression)
		{
			// NOTE: It is assumed that simple case expressions are already transformed into
			//       searched case expressions, i.e.
			//       CASE
			//          WHEN Pred1 THEN Expr1 ...
			//          WHEN PredN THEN ExprN
			//          [ELSE ElseExpr]
			//       END

			List<ExpressionNode> processedWhenExpressions = new List<ExpressionNode>();

			for (int i = 0; i < expression.WhenExpressions.Length; i++)
			{
				SetWhenPassthru(processedWhenExpressions);
				expression.WhenExpressions[i] = VisitExpression(expression.WhenExpressions[i]);
				processedWhenExpressions.Add(expression.WhenExpressions[i]);

				SetThenPassthru(processedWhenExpressions);
				expression.ThenExpressions[i] = VisitExpression(expression.ThenExpressions[i]);
			}

			if (expression.ElseExpression != null)
			{
				SetWhenPassthru(processedWhenExpressions);
				expression.ElseExpression = VisitExpression(expression.ElseExpression);
			}

			_currentPassthruPredicate = null;

			return expression;
		}

		public override ExpressionNode VisitSingleRowSubselect(SingleRowSubselect expression)
		{
			AlgebraNode inputNode = GetOrCreateInput();
			ResultAlgebraNode algebrizedSubquery = Algebrizer.Convert(expression.Query);
			ResultAlgebraNode assertedSubquery = CreateAssertedSubquery(algebrizedSubquery);

			JoinAlgebraNode joinAlgebraNode = new JoinAlgebraNode();
			joinAlgebraNode.PassthruPredicate = CurrentPassthruPredicate;
			joinAlgebraNode.Op = JoinAlgebraNode.JoinOperator.LeftOuterJoin;
			joinAlgebraNode.Left = inputNode;
			joinAlgebraNode.Right = assertedSubquery;
			SetLastAlgebraNode(joinAlgebraNode);

			return new RowBufferEntryExpression(assertedSubquery.OutputList[0]);
		}

		public override ExpressionNode VisitExistsSubselect(ExistsSubselect expression)
		{
			AlgebraNode input = GetAndResetLastNode();
			ResultAlgebraNode algebrizedQuery = Algebrizer.Convert(expression.Query);

			if (!expression.Negated && AstUtil.WillProduceAtLeastOneRow(algebrizedQuery))
			{
				if (input == null)
					SetLastAlgebraNode(CreateConstantScan());
				else
					SetLastAlgebraNode(input);

				return LiteralExpression.FromBoolean(true);
			}


			if (!expression.Negated && !ProbingEnabled && input == null)
			{
				SetLastAlgebraNode(algebrizedQuery);
				return LiteralExpression.FromBoolean(true);
			}
			else
			{
				if (input == null)
					input = CreateConstantScan();

				RowBufferEntry probeColumn = CreateProbeColumn();

				JoinAlgebraNode joinAlgebraNode = new JoinAlgebraNode();
				joinAlgebraNode.PassthruPredicate = CurrentPassthruPredicate;
				joinAlgebraNode.ProbeBufferEntry = probeColumn;
				joinAlgebraNode.Left = input;
				joinAlgebraNode.Right = algebrizedQuery;
				joinAlgebraNode.Op = expression.Negated ? JoinAlgebraNode.JoinOperator.LeftAntiSemiJoin : JoinAlgebraNode.JoinOperator.LeftSemiJoin;
				SetLastAlgebraNode(joinAlgebraNode);

				return CreateProbeColumnRef(probeColumn);
			}
		}

		public override ExpressionNode VisitAllAnySubselect(AllAnySubselect expression)
		{
			expression.Left = VisitExpression(expression.Left);
			ResultAlgebraNode algebrizedQuery = Algebrizer.Convert(expression.Query);

			ExpressionNode leftExpression = expression.Left;
			RowBufferEntryExpression rightExpression = new RowBufferEntryExpression();
			rightExpression.RowBufferEntry = algebrizedQuery.OutputList[0];

			ExpressionBuilder expressionBuilder = new ExpressionBuilder();
			expressionBuilder.Push(leftExpression);
			expressionBuilder.Push(rightExpression);
			expressionBuilder.PushBinary(expression.Op);

			bool negated = (expression.Type == AllAnySubselect.AllAnyType.All);
			if (negated)
			{
				expressionBuilder.PushUnary(UnaryOperator.LogicalNot);
				expressionBuilder.Push(leftExpression);
				expressionBuilder.PushIsNull();
				expressionBuilder.Push(rightExpression);
				expressionBuilder.PushIsNull();
				expressionBuilder.PushNAry(LogicalOperator.Or);
			}

			ExpressionNode filterPredicate = expressionBuilder.Pop();

			FilterAlgebraNode filterAlgebraNode = new FilterAlgebraNode();
			filterAlgebraNode.Input = algebrizedQuery;
			filterAlgebraNode.Predicate = filterPredicate;

			AlgebraNode input = GetAndResetLastNode();

			if (!negated && !ProbingEnabled && input == null)
			{
				SetLastAlgebraNode(filterAlgebraNode);
				return LiteralExpression.FromBoolean(true);
			}
			else
			{
				if (input == null)
					input = CreateConstantScan();

				RowBufferEntry probeColumn = CreateProbeColumn();

				JoinAlgebraNode joinAlgebraNode = new JoinAlgebraNode();
				joinAlgebraNode.PassthruPredicate = CurrentPassthruPredicate;
				joinAlgebraNode.ProbeBufferEntry = probeColumn;
				joinAlgebraNode.Left = input;
				joinAlgebraNode.Right = filterAlgebraNode;
				joinAlgebraNode.Op = negated ? JoinAlgebraNode.JoinOperator.LeftAntiSemiJoin : JoinAlgebraNode.JoinOperator.LeftSemiJoin;

				SetLastAlgebraNode(joinAlgebraNode);
				return CreateProbeColumnRef(probeColumn);
			}
		}

		public override ExpressionNode VisitBinaryExpression(BinaryExpression expression)
		{
			if (expression.Op != BinaryOperator.LogicalAnd &&
			    expression.Op != BinaryOperator.LogicalOr)
				return base.VisitBinaryExpression(expression);

			if (expression.Op == BinaryOperator.LogicalAnd)
			{
				// AND

				expression.Left = VisitExpression(expression.Left);
				expression.Right = VisitExpression(expression.Right);

				return expression;
			}
			else
			{
				// OR

				AlgebraNode input = GetAndResetLastNode();
				_probingEnabledStack.Push(false);

				List<ExpressionNode> scalarOrParts = new List<ExpressionNode>();
				List<AlgebraNode> algebrizedOrParts = new List<AlgebraNode>();
				foreach (ExpressionNode orPart in AstUtil.SplitCondition(LogicalOperator.Or, expression))
				{
					if (!AstUtil.ContainsSubselect(orPart))
					{
						scalarOrParts.Add(orPart);
					}
					else
					{
						ExpressionNode replacedOrPart = VisitExpression(orPart);
						FilterAlgebraNode filterAlgebraNode = new FilterAlgebraNode();
						filterAlgebraNode.Input = GetAndResetLastNode();
						filterAlgebraNode.Predicate = replacedOrPart;
						algebrizedOrParts.Add(filterAlgebraNode);
					}
				}

				if (scalarOrParts.Count > 0)
				{
					FilterAlgebraNode filterAlgebraNode = new FilterAlgebraNode();
					filterAlgebraNode.Predicate = AstUtil.CombineConditions(LogicalOperator.Or, scalarOrParts);
					filterAlgebraNode.Input = CreateConstantScan();
					algebrizedOrParts.Insert(0, filterAlgebraNode);
				}

				_probingEnabledStack.Pop();

				ConcatAlgebraNode concat = new ConcatAlgebraNode();
				concat.DefinedValues = new UnitedValueDefinition[0];
				concat.Inputs = algebrizedOrParts.ToArray();

				RowBufferEntry probeColumn = CreateProbeColumn();
				JoinAlgebraNode leftSemiJoinBetweenInputAndConcat = new JoinAlgebraNode();
				leftSemiJoinBetweenInputAndConcat.Op = JoinAlgebraNode.JoinOperator.LeftSemiJoin;
				leftSemiJoinBetweenInputAndConcat.PassthruPredicate = CurrentPassthruPredicate;
				leftSemiJoinBetweenInputAndConcat.ProbeBufferEntry = probeColumn;
				leftSemiJoinBetweenInputAndConcat.Left = input;
				leftSemiJoinBetweenInputAndConcat.Right = concat;
				SetLastAlgebraNode(leftSemiJoinBetweenInputAndConcat);
				return CreateProbeColumnRef(probeColumn);
			}
		}

		public override AlgebraNode VisitJoinAlgebraNode(JoinAlgebraNode node)
		{
			if (node.Predicate == null || !AstUtil.ContainsSubselect(node.Predicate))
				return base.VisitJoinAlgebraNode(node);

			node.Left = VisitAlgebraNode(node.Left);
			node.Right = VisitAlgebraNode(node.Right);

			switch (node.Op)
			{
				case JoinAlgebraNode.JoinOperator.InnerJoin:
				{
					FilterAlgebraNode filterAlgebraNode = new FilterAlgebraNode();
					filterAlgebraNode.Predicate = node.Predicate;
					filterAlgebraNode.Input = node;
					node.Predicate = null;

					SetLastAlgebraNode(node);
					_probingEnabledStack.Push(false);
					filterAlgebraNode.Predicate = VisitExpression(filterAlgebraNode.Predicate);
					_probingEnabledStack.Pop();
					filterAlgebraNode.Input = GetAndResetLastNode();

					return filterAlgebraNode;

				}

				case JoinAlgebraNode.JoinOperator.LeftOuterJoin:
				{
					FilterAlgebraNode filterAlgebraNode = new FilterAlgebraNode();
					filterAlgebraNode.Predicate = node.Predicate;
					filterAlgebraNode.Input = node.Right;

					node.Right = filterAlgebraNode;
					node.Predicate = null;

					SetLastAlgebraNode(filterAlgebraNode.Input);
					_probingEnabledStack.Push(false);
					filterAlgebraNode.Predicate = VisitExpression(filterAlgebraNode.Predicate);
					_probingEnabledStack.Pop();
					filterAlgebraNode.Input = GetAndResetLastNode();

					return node;
				}

				case JoinAlgebraNode.JoinOperator.RightOuterJoin:
				{
					node.Op = JoinAlgebraNode.JoinOperator.LeftOuterJoin;
					AlgebraNode oldLeft = node.Left;
					node.Left = node.Right;
					node.Right = oldLeft;
					goto case JoinAlgebraNode.JoinOperator.LeftOuterJoin;
				}

				case JoinAlgebraNode.JoinOperator.FullOuterJoin:
					// TODO: Support subqueries in FULL OUTER JOIN.
					throw ExceptionBuilder.InternalError("FULL OUTER JOIN containing a subselect predicate in ON clause is not supported.");

				default:
					throw ExceptionBuilder.UnhandledCaseLabel(node.Op);
			}
		}

		public override AlgebraNode VisitComputeScalarAlgebraNode(ComputeScalarAlgebraNode node)
		{
			node.Input = VisitAlgebraNode(node.Input);

			SetLastAlgebraNode(node.Input);

			_probingEnabledStack.Push(true);
			foreach (ComputedValueDefinition definedValue in node.DefinedValues)
				definedValue.Expression = VisitExpression(definedValue.Expression);
			_probingEnabledStack.Pop();
			
			node.Input = GetAndResetLastNode();
			return node;
		}

		public override AlgebraNode VisitFilterAlgebraNode(FilterAlgebraNode node)
		{
			node.Input = VisitAlgebraNode(node.Input);

			SetLastAlgebraNode(node.Input);
			_probingEnabledStack.Push(false);
			node.Predicate = VisitExpression(node.Predicate);
			_probingEnabledStack.Pop();

			node.Input = GetAndResetLastNode();
			return node;
		}
	}
}