using System;

namespace NQuery.Compilation
{
	internal abstract class ValueDefinition : AstElement
	{
		private RowBufferEntry _target;

		public RowBufferEntry Target
		{
			get { return _target; }
			set { _target = value; }
		}
	}
}