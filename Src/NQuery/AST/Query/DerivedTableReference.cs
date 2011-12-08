using System;
using System.Collections.Generic;

using NQuery.Compilation;
using NQuery.Runtime;

namespace NQuery.Compilation
{
	internal sealed class DerivedTableReference : TableReference
	{
		private QueryNode _query;
		private Identifier _correlationName;
		private SourceRange _correlationNameSourceRange;
		private TableRefBinding _derivedTableBinding;

		public QueryNode Query
		{
			get { return _query; }
			set { _query = value; }
		}

		public Identifier CorrelationName
		{
			get { return _correlationName; }
			set { _correlationName = value; }
		}

		public SourceRange CorrelationNameSourceRange
		{
			get { return _correlationNameSourceRange; }
			set { _correlationNameSourceRange = value; }
		}

		public TableRefBinding DerivedTableBinding
		{
			get { return _derivedTableBinding; }
			set { _derivedTableBinding = value; }
		}

		public override AstNodeType NodeType
		{
			get { return AstNodeType.DerivedTableReference; }
		}

		public override AstElement Clone(Dictionary<AstElement, AstElement> alreadyClonedElements)
		{
			DerivedTableReference result = new DerivedTableReference();
			result.Query = (QueryNode)_query.Clone(alreadyClonedElements);
			result.CorrelationName = _correlationName;
			result.CorrelationNameSourceRange = _correlationNameSourceRange;
			return result;
		}
	}
}
