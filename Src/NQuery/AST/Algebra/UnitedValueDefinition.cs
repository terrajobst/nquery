using System;
using System.Collections.Generic;

namespace NQuery.Compilation
{
	internal sealed class UnitedValueDefinition : ValueDefinition
	{
		private RowBufferEntry[] _dependendEntries;

		public RowBufferEntry[] DependendEntries
		{
			get { return _dependendEntries; }
			set { _dependendEntries = value; }
		}

		public override AstElement Clone(Dictionary<AstElement, AstElement> alreadyClonedElements)
		{
			UnitedValueDefinition result = new UnitedValueDefinition();
			result.Target = Target;
			result.DependendEntries = ArrayHelpers.Clone(_dependendEntries);
			return result;
		}
	}
}