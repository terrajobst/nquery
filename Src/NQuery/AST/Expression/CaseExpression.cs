using System;
using System.Collections.Generic;
using System.Globalization;

namespace NQuery.Compilation
{
	internal sealed class CaseExpression : ExpressionNode
	{
		private ExpressionNode _inputExpression;
		private ExpressionNode[] _whenExpressions;
		private ExpressionNode[] _thenExpressions;
		private ExpressionNode _elseExpression;
		private Type _resultType;

		public CaseExpression() 
		{
		}

		public override AstNodeType NodeType
		{
			get { return AstNodeType.CaseExpression; }
		}

		public override AstElement Clone(Dictionary<AstElement, AstElement> alreadyClonedElements)
		{
			CaseExpression result = new CaseExpression();
			
			if (_inputExpression != null)
				result.InputExpression = (ExpressionNode)_inputExpression.Clone(alreadyClonedElements);

			if (_elseExpression != null)
				result.ElseExpression = (ExpressionNode)_elseExpression.Clone(alreadyClonedElements);

			result.WhenExpressions = new ExpressionNode[_whenExpressions.Length];
			for (int i = 0; i < _whenExpressions.Length; i++)
				result.WhenExpressions[i] = (ExpressionNode)_whenExpressions[i].Clone(alreadyClonedElements);

			result.ThenExpressions = new ExpressionNode[_thenExpressions.Length];
			for (int i = 0; i < _thenExpressions.Length; i++)
				result.ThenExpressions[i] = (ExpressionNode)_thenExpressions[i].Clone(alreadyClonedElements);

			result.ResultType = _resultType;

			return result;
		}

		public ExpressionNode InputExpression
		{
			get { return _inputExpression; }
			set { _inputExpression = value; }
		}

		public ExpressionNode[] WhenExpressions
		{
			get { return _whenExpressions; }
			set { _whenExpressions = value; }
		}

		public ExpressionNode[] ThenExpressions
		{
			get { return _thenExpressions; }
			set { _thenExpressions = value; }
		}

		public ExpressionNode ElseExpression
		{
			get { return _elseExpression; }
			set { _elseExpression = value; }
		}

		public Type ResultType
		{
			get { return _resultType; }
			set { _resultType = value; }
		}

		public override Type ExpressionType
		{
			get { return _resultType; }
		}

		public override object GetValue()
		{
			if (_whenExpressions == null || _thenExpressions == null || _whenExpressions.Length != _thenExpressions.Length)
				return null;

			if (_inputExpression != null)
			{
				// Simple case

				object inputValue = _inputExpression.GetValue();

				if (inputValue == null)
				{
					if (_elseExpression == null)
						return null;
					else
						return _elseExpression.GetValue();
				}

				for (int i = 0; i < _whenExpressions.Length; i++)
				{
					object whenValue = _whenExpressions[i].GetValue();

					if (whenValue != null && Equals(inputValue, whenValue))
						return _thenExpressions[i].GetValue();
				}

				if (_elseExpression != null)
					return _elseExpression.GetValue();

				return null;
			}
			else
			{
				// Searched case

				for (int i = 0; i < _whenExpressions.Length; i++)
				{
					object whenConditionValue = _whenExpressions[i].GetValue();

					if (whenConditionValue != null && Convert.ToBoolean(whenConditionValue, CultureInfo.InvariantCulture))
						return _thenExpressions[i].GetValue();
				}

				if (_elseExpression != null)
					return _elseExpression.GetValue();

				return null;
			}
		}
	}
}