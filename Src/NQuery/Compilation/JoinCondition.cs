using System;
using System.Text;

using NQuery.Runtime;

namespace NQuery.Compilation
{
	internal class JoinCondition
	{
		private JoinOperator _op;
		private ExpressionNode _leftExpression;
		private TableRefBinding _leftTable;
		private ExpressionNode _rightExpression;
		private TableRefBinding _rightTable;

		public JoinCondition()
		{
		}

		public JoinOperator Op
		{
			get { return _op; }
			set { _op = value; }
		}
		
		public ExpressionNode LeftExpression
		{
			get { return _leftExpression; }
			set { _leftExpression = value; }
		}

		public TableRefBinding LeftTable
		{
			get { return _leftTable; }
			set { _leftTable = value; }
		}

		public ExpressionNode RightExpression
		{
			get { return _rightExpression; }
			set { _rightExpression = value; }
		}

		public TableRefBinding RightTable
		{
			get { return _rightTable; }
			set { _rightTable = value; }
		}

		public JoinCondition Clone()
		{
			JoinCondition joinCondition = new JoinCondition();
			joinCondition.Op = _op;
			joinCondition.LeftExpression = _leftExpression;
			joinCondition.LeftTable = _leftTable;
			joinCondition.RightExpression = _rightExpression;
			joinCondition.RightTable = _rightTable;
			return joinCondition;
		}

		public override int GetHashCode()
		{
			int result = _op.GetHashCode();
			result = 29 * result + _leftExpression.GetHashCode();
			result = 29 * result + _leftTable.GetHashCode();
			result = 29 * result + _rightExpression.GetHashCode();
			result = 29 * result + _rightTable.GetHashCode();
			return result;
		}

		public bool Equals(JoinCondition joinCondition)
		{
			if (ReferenceEquals(this, joinCondition))
				return true;
			
			if (joinCondition.Op != _op)
				return false;
			
			if (joinCondition.LeftTable == _leftTable && joinCondition.RightTable == _rightTable)
			{
				if (!joinCondition.LeftExpression.IsStructuralEqualTo(_leftExpression))
					return false;
			
				if (!joinCondition.RightExpression.IsStructuralEqualTo(_rightExpression))
					return false;
				
				return true;
			}
			else if (joinCondition.LeftTable == _rightTable && joinCondition.RightTable == _leftTable)
			{
				if (!joinCondition.LeftExpression.IsStructuralEqualTo(_rightExpression))
					return false;
			
				if (!joinCondition.RightExpression.IsStructuralEqualTo(_leftExpression))
					return false;
				
				return true;
			}
			
			return false;
		}

		public override bool Equals(object obj)
		{
			if (this == obj)
					return true;
			
			JoinCondition joinCondition = obj as JoinCondition;
			
			if (joinCondition == null)
				return false;
			
			return Equals(joinCondition);
		}

		public ExpressionNode ToExpression()
		{
			BinaryOperator op;

			switch (_op)
			{
				case JoinOperator.Equal:
					op = BinaryOperator.Equal;
					break;
				case JoinOperator.Less:
					op = BinaryOperator.Less;
					break;
				case JoinOperator.LessOrEqual:
					op = BinaryOperator.LessOrEqual;
					break;
				case JoinOperator.Greater:
					op = BinaryOperator.Greater;
					break;
				case JoinOperator.GreaterOrEqual:
					op = BinaryOperator.GreaterOrEqual;
					break;
				default:
					throw ExceptionBuilder.UnhandledCaseLabel(_op);
			}

			// HACK: Here we need to create a fully resolved binary expression node
			//       Using the code (Binder.BindOperator) is NOT the way it should be...

			Binder binder = new Binder(ExceptionErrorProvider.Instance);
			BinaryExpression binaryExpression = new BinaryExpression(op, _leftExpression, _rightExpression);
			binaryExpression.OperatorMethod = binder.BindOperator(op, _leftExpression.ExpressionType, _rightExpression.ExpressionType);

			return binaryExpression;
		}

		private static JoinOperator SwapJoinOperator(JoinOperator joinOperator)
		{
			switch (joinOperator)
			{
				case JoinOperator.None:
				case JoinOperator.Equal:
					return joinOperator;
				case JoinOperator.Less:
					return JoinOperator.Greater;
				case JoinOperator.LessOrEqual:
					return JoinOperator.GreaterOrEqual;
				case JoinOperator.Greater:
					return JoinOperator.Less;
				case JoinOperator.GreaterOrEqual:
					return JoinOperator.LessOrEqual;
				default:
					throw ExceptionBuilder.UnhandledCaseLabel(joinOperator);
			}
		}
		
		public void SwapSides()
		{
			ExpressionNode oldLeftExpression = _leftExpression;
			TableRefBinding oldLeftTable = _leftTable;

			_op = SwapJoinOperator(_op);
			_leftExpression = _rightExpression;
			_leftTable = _rightTable;
			_rightExpression = oldLeftExpression;
			_rightTable = oldLeftTable;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			if (_leftTable == null)
			{
				sb.Append("(NONE)");
			}
			else
			{
				sb.Append(_leftTable.Name);
				sb.Append(": ");
				sb.Append(_leftTable.TableBinding.Name);
			}

			sb.Append(" ----> ");
			sb.Append(_rightTable.Name);
			sb.Append(": ");
			sb.Append(_rightTable.TableBinding.Name);
			sb.Append(Environment.NewLine);

			sb.Append("{");
			sb.Append(_leftExpression.ToString());
			sb.Append("}");
			sb.Append(" ");
			sb.Append(_op.ToString());
			sb.Append(" ");
			sb.Append("{");
			sb.Append(_rightExpression.ToString());
			sb.Append("}");
			sb.Append(Environment.NewLine);

			return sb.ToString();
		}
	}
}