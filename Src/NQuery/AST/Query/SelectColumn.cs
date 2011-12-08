using System;
using System.Collections.Generic;

namespace NQuery.Compilation
{
	internal sealed class SelectColumn : AstElement
	{
		private ExpressionNode _expression;
		private Identifier _alias;

		public SelectColumn()
		{
		}

		public SelectColumn(ExpressionNode expression, Identifier columnAlias)
		{
			_expression = expression;
			_alias = columnAlias;
		}

		public SelectColumn(Identifier tableAlias)
		{
			_alias = tableAlias;
		}

		public override AstElement Clone(Dictionary<AstElement, AstElement> alreadyClonedElements)
		{
			SelectColumn result = new SelectColumn();
			if (_expression != null)
				result.Expression = (ExpressionNode)_expression.Clone(alreadyClonedElements);
			result.Alias = _alias;
			return result;
		}

		public ExpressionNode Expression
		{
			get { return _expression; }
			set { _expression = value; }
		}

		public Identifier Alias
		{
			get { return _alias; }
			set { _alias = value; }
		}

		public bool IsAsterisk
		{
			get { return _expression == null; }
		}
	}
}