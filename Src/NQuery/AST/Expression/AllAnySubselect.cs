using System;
using System.Collections.Generic;

namespace NQuery.Compilation
{
	internal sealed class AllAnySubselect : SubselectExpression
	{
		public enum AllAnyType
		{
			All,
			Any
		}
		
		private ExpressionNode _left;
		private BinaryOperator _op;
		private AllAnyType _type;

		public ExpressionNode Left
		{
			get { return _left; }
			set { _left = value; }
		}

		public BinaryOperator Op
		{
			get { return _op; }
			set { _op = value; }
		}

		public AllAnyType Type
		{
			get { return _type; }
			set { _type = value; }
		}

		public override object GetValue()
		{
			throw ExceptionBuilder.InternalErrorGetValueNotSupported(GetType());
		}

		public override AstNodeType NodeType
		{
			get { return AstNodeType.AllAnySubselect; }
		}

		public override Type ExpressionType
		{
			get { return typeof(bool); }
		}

		public override AstElement Clone(Dictionary<AstElement, AstElement> alreadyClonedElements)
		{
			AllAnySubselect allAnySubselect = new AllAnySubselect();
			allAnySubselect.Left = (ExpressionNode)_left.Clone(alreadyClonedElements);
			allAnySubselect.Op = _op;
			allAnySubselect.Type = _type;
			allAnySubselect.Query = (QueryNode)Query.Clone(alreadyClonedElements);
			return allAnySubselect;
		}
	}
}