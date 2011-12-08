using System;
using System.Collections.Generic;

using NQuery.Runtime;

namespace NQuery.Compilation
{
	internal sealed class PropertyAccessExpression : ExpressionNode
	{
		private ExpressionNode _target;
		private Identifier _name;
		private SourceRange _nameSourceRange;
		private PropertyBinding _property;

		public PropertyAccessExpression()
		{
		}

		public override AstNodeType NodeType
		{
			get { return AstNodeType.PropertyAccessExpression; }
		}

		public override AstElement Clone(Dictionary<AstElement, AstElement> alreadyClonedElements)
		{
			PropertyAccessExpression result = new PropertyAccessExpression();
			result.Target = (ExpressionNode)_target.Clone(alreadyClonedElements);
			result.Name = _name;
			result.NameSourceRange = _nameSourceRange;
			result.Property = _property;

			return result;
		}

		public override Type ExpressionType
		{
			get
			{
				if (_property == null)
					return null;
				else
					return _property.DataType;
			}
		}

		public override object GetValue()
		{
			if (_property == null)
				return null;

			object instance = _target.GetValue();

			if (NullHelper.IsNull(instance))
				return null;

			object result;

			try
			{
				result = _property.GetValue(instance);
			}
			catch (NQueryException)
			{
				throw;
			}
			catch (Exception ex)
			{
				throw ExceptionBuilder.PropertyBindingGetValueFailed(ex);
			}

			return NullHelper.UnifyNullRepresentation(result);
		}

		public ExpressionNode Target
		{
			get { return _target; }
			set { _target = value; }
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

		public PropertyBinding Property
		{
			get { return _property; }
			set { _property = value; }
		}
	}
}