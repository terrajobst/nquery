using System;
using System.Collections.Generic;
using System.Reflection;

namespace NQuery.Compilation
{
	internal sealed class CastExpression : ExpressionNode
	{
		private ExpressionNode _expression;
		private TypeReference _typeReference;
		private MethodInfo _convertMethod;

		public CastExpression() 
		{
		}

		public CastExpression(ExpressionNode expression, Type targetType)
		{			
			_expression = expression;
			_typeReference = new TypeReference();
			_typeReference.TypeName = targetType.AssemblyQualifiedName;
			_typeReference.ResolvedType = targetType;
		}

		public CastExpression(ExpressionNode expression, TypeReference typeReference)
		{
			_expression = expression;
			_typeReference = typeReference;
		}

		public override AstNodeType NodeType
		{
			get { return AstNodeType.CastExpression; }
		}

		public override AstElement Clone(Dictionary<AstElement, AstElement> alreadyClonedElements)
		{
			CastExpression result = new CastExpression();
			result.Expression = (ExpressionNode)_expression.Clone(alreadyClonedElements);
			result.TypeReference = (TypeReference)_typeReference.Clone(alreadyClonedElements);
			result.ConvertMethod = _convertMethod;
			return result;
		}

		public ExpressionNode Expression
		{
			get { return _expression; }
			set { _expression = value; }
		}

		public TypeReference TypeReference
		{
			get { return _typeReference; }
			set { _typeReference = value; }
		}

		public MethodInfo ConvertMethod
		{
			get { return _convertMethod; }
			set { _convertMethod = value; }
		}

		public override Type ExpressionType
		{
			get
			{
				if (_typeReference == null)
					return null;

				return _typeReference.ResolvedType;
			}
		}

		public override object GetValue()
		{
			object value = _expression.GetValue();

			if (value == null)
				return null;

			try
			{
				return _convertMethod.Invoke(null, new object[] {value});
			}
			catch (NQueryException)
			{
				throw;
			}
			catch (Exception ex)
			{
				throw ExceptionBuilder.CastingOperatorFailed(ex);
			}
		}
	}
}