using System;

namespace NQuery.Compilation
{
	internal abstract class UnaryAlgebraNode : AlgebraNode
	{
		private AlgebraNode _input;

		public AlgebraNode Input
		{
			get { return _input; }
			set { _input = value; }
		}
	}
}