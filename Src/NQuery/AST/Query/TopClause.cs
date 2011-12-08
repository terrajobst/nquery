using System;
using System.Collections.Generic;

namespace NQuery.Compilation
{
	internal sealed class TopClause : AstElement
	{
		private int _value;
		private bool _withTies;

		public TopClause()
		{
			
		}

		public override AstElement Clone(Dictionary<AstElement, AstElement> alreadyClonedElements)
		{
			TopClause result = new TopClause();
			result.Value = _value;
			result.WithTies = _withTies;
			return result;
		}

		public int Value
		{
			get { return _value; }
			set { _value = value; }
		}

		public bool WithTies
		{
			get { return _withTies; }
			set { _withTies = value; }
		}
	}
}