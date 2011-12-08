using System;
using System.Collections.Generic;

namespace NQuery.Compilation
{
	internal sealed class NameExpression : ExpressionNode
	{
		private Identifier _name;
		private SourceRange _nameSourceRange;

		public NameExpression()
		{
		}

		public override AstElement Clone(Dictionary<AstElement, AstElement> alreadyClonedElements)
		{
			NameExpression result = new NameExpression();
			result.Name = _name;
			result.NameSourceRange = _nameSourceRange;
			return result;
		}

		public override AstNodeType NodeType
		{
			get { return AstNodeType.NameExpression; }
		}

		public override Type ExpressionType
		{
			get { return null; }
		}

		public override object GetValue()
		{
			// NameNode entries in the AST are either replaced by ColumnNodes or
			// ConstantNodes.

			throw ExceptionBuilder.InternalErrorGetValueNotSupported(GetType());
		}

		public Identifier Name
		{
			get { return _name; }
			set { _name = value; }
		}

		public SourceRange NameSourceRange
		{
			get { return _nameSourceRange; }
			set { _nameSourceRange = value; }
		}
	}
}