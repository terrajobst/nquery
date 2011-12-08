using System;
using System.Collections.Generic;

using NQuery.Runtime;

namespace NQuery.Compilation
{
	internal sealed class ColumnExpression : ExpressionNode
	{
		private ColumnRefBinding _columnRefBinding;

		public ColumnExpression(ColumnRefBinding columnRefBinding)
		{
			_columnRefBinding = columnRefBinding;
		}

		public override AstNodeType NodeType
		{
			get { return AstNodeType.ColumnExpression; }
		}

		public override AstElement Clone(Dictionary<AstElement, AstElement> alreadyClonedElements)
		{
			return new ColumnExpression(_columnRefBinding);
		}

		public ColumnRefBinding Column
		{
			get { return _columnRefBinding; }
		}

		public override Type ExpressionType
		{
			get { return _columnRefBinding.ColumnBinding.DataType; }
		}

		public override object GetValue()
		{
			// ColumnExpression entries in the AST are replaced by RowBufferEntryExpression entries.

			throw ExceptionBuilder.InternalErrorGetValueNotSupported(GetType());
		}
	}
}