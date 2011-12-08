using System;
using System.Collections.Generic;

using NQuery.Runtime;

namespace NQuery.Compilation
{
	internal sealed class ColumnValueDefinition : ValueDefinition
	{
		private ColumnRefBinding _columnRefBinding;

		public ColumnRefBinding ColumnRefBinding
		{
			get { return _columnRefBinding; }
			set { _columnRefBinding = value; }
		}

		public override AstElement Clone(Dictionary<AstElement, AstElement> alreadyClonedElements)
		{
			ColumnValueDefinition result = new ColumnValueDefinition();
			result.Target = Target;
			result.ColumnRefBinding = _columnRefBinding;
			return result;
		}
	}
}