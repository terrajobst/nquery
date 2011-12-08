using System.Collections.Generic;

namespace NQuery.Compilation
{
	internal sealed class JoinedTableReference : TableReference
	{
		private JoinType _joinType;
		private TableReference _left;
		private TableReference _right;
		private ExpressionNode _condition;

		public JoinType JoinType
		{
			get { return _joinType; }
			set { _joinType = value; }
		}

		public TableReference Left
		{
			get { return _left; }
			set { _left = value; }
		}

		public TableReference Right
		{
			get { return _right; }
			set { _right = value; }
		}

		public ExpressionNode Condition
		{
			get { return _condition; }
			set { _condition = value; }
		}

		public override AstNodeType NodeType
		{
			get { return AstNodeType.JoinedTableReference; }
		}

		public override AstElement Clone(Dictionary<AstElement, AstElement> alreadyClonedElements)
		{
			JoinedTableReference result = new JoinedTableReference();
			result.Left = (TableReference)_left.Clone(alreadyClonedElements);
			result.JoinType = _joinType;
			result.Right = (TableReference)_right.Clone(alreadyClonedElements);
			
			if (_condition != null)
				result.Condition = (ExpressionNode)_condition.Clone(alreadyClonedElements);
									
			return result;
		}
	}
}