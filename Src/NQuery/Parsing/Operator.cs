using System;

namespace NQuery.Compilation 
{
	internal abstract class Operator
	{
		private int _precedence;
		private string _tokenText;
		private string _methodName;

		protected Operator(int precedence, string tokenText, string methodName)
		{
			_precedence = precedence;
			_tokenText = tokenText;
			_methodName = methodName;
		}

		public int Precedence
		{
			get { return _precedence; }
		}

		public string TokenText
		{
			get { return _tokenText; }
		}

		public string MethodName
		{
			get { return _methodName; }
		}

		public abstract bool IsOverloadable { get; }
		public abstract bool IsRightAssociative { get; }

		public override string ToString()
		{
			return _tokenText;
		}

		internal const int BETWEEN_PRECEDENCE = 4;
	}
}