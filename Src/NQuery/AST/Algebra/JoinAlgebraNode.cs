using System;
using System.Collections.Generic;

namespace NQuery.Compilation
{
	internal sealed class JoinAlgebraNode : AlgebraNode
	{
		public enum JoinOperator
		{
			InnerJoin,
			
			LeftOuterJoin,
			LeftSemiJoin,
			LeftAntiSemiJoin,
			
			RightOuterJoin,
			RightSemiJoin,
			RightAntiSemiJoin,
			
			FullOuterJoin
		}
		
		private AlgebraNode _left;
		private AlgebraNode _right;
		private JoinOperator _op;
		private ExpressionNode _predicate;
		private RowBufferEntry[] _outerReferences;
		private ExpressionNode _passthruPredicate;
		private RowBufferEntry _probeBufferEntry;

		public JoinAlgebraNode()
		{
		}

		public AlgebraNode Left
		{
			get { return _left; }
			set { _left = value; }
		}

		public AlgebraNode Right
		{
			get { return _right; }
			set { _right = value; }
		}

		public JoinOperator Op
		{
			get { return _op; }
			set { _op = value; }
		}

		public ExpressionNode Predicate
		{
			get { return _predicate; }
			set { _predicate = value; }
		}

		public RowBufferEntry[] OuterReferences
		{
			get { return _outerReferences; }
			set { _outerReferences = value; }
		}

		public ExpressionNode PassthruPredicate
		{
			get { return _passthruPredicate; }
			set { _passthruPredicate = value; }
		}

		public RowBufferEntry ProbeBufferEntry
		{
			get { return _probeBufferEntry; }
			set { _probeBufferEntry = value; }
		}

		public override AstNodeType NodeType
		{
			get { return AstNodeType.JoinAlgebraNode; }
		}

		public void SwapSides()
		{
			AlgebraNode oldLeft = _left;
			_left = _right;
			_right = oldLeft;
			
			switch (_op)
			{
				case JoinOperator.LeftOuterJoin:
					_op = JoinOperator.RightOuterJoin;
					break;
				case JoinOperator.LeftSemiJoin:
					_op = JoinOperator.RightSemiJoin;
					break;
				case JoinOperator.LeftAntiSemiJoin:
					_op = JoinOperator.RightAntiSemiJoin;
					break;

				case JoinOperator.RightOuterJoin:
					_op = JoinOperator.LeftOuterJoin;
					break;
				case JoinOperator.RightSemiJoin:
					_op = JoinOperator.LeftSemiJoin;
					break;
				case JoinOperator.RightAntiSemiJoin:
					_op = JoinOperator.LeftAntiSemiJoin;
					break;
										
				case JoinOperator.InnerJoin:
				case JoinOperator.FullOuterJoin:
					// Nothing to do.
					break;

				default:
					throw ExceptionBuilder.UnhandledCaseLabel(_op);
			}
		}

		public override AstElement Clone(Dictionary<AstElement, AstElement> alreadyClonedElements)
		{
			JoinAlgebraNode result = new JoinAlgebraNode();
			result.StatisticsIterator = StatisticsIterator;
			result.OutputList = ArrayHelpers.Clone(OutputList);
			result.Left = (AlgebraNode)_left.Clone(alreadyClonedElements);
			result.Right = (AlgebraNode)_right.Clone(alreadyClonedElements);
			result.Op = _op;
			if (_predicate != null)
				result.Predicate = (ExpressionNode)_predicate.Clone(alreadyClonedElements);
			result.OuterReferences = ArrayHelpers.Clone(_outerReferences);
			return result;
		}
	}
}