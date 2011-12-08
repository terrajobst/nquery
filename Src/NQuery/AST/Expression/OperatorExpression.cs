using System;
using System.Reflection;

namespace NQuery.Compilation
{
	internal abstract class OperatorExpression : ExpressionNode
	{
		private MethodInfo _operatorMethod;

		public MethodInfo OperatorMethod
		{
			get { return _operatorMethod; }
			set { _operatorMethod = value; }
		}

		public sealed override Type ExpressionType
		{
			get
			{
				if (_operatorMethod == null)
					return null;

				return _operatorMethod.ReturnType;
			}
		}

		public Operator Op
		{
			get { return InternalGetOp(); }
		}

		protected abstract Operator InternalGetOp();
	}
}