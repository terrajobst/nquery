using System;

namespace NQuery.Compilation
{
	internal abstract class ExpressionNode : AstNode
	{
		public abstract Type ExpressionType { get; }
		public abstract object GetValue();
	}
}