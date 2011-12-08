using System.Collections.Generic;

using NQuery.Runtime;

namespace NQuery.Compilation
{
	internal sealed class NamedTableReference : TableReference
	{
		private Identifier _tableName;
		private SourceRange _tableNameSourceRange;
		private Identifier _correlationName;
		private SourceRange _correlationNameSourceRange;
		private TableRefBinding _tableRefBinding;

		public Identifier TableName
		{
			get { return _tableName; }
			set { _tableName = value; }
		}

		public SourceRange TableNameSourceRange
		{
			get { return _tableNameSourceRange; }
			set { _tableNameSourceRange = value; }
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

		public TableRefBinding TableRefBinding
		{
			get { return _tableRefBinding; }
			set { _tableRefBinding = value; }
		}

		public override AstNodeType NodeType
		{
			get { return AstNodeType.NamedTableReference; }
		}

		public override AstElement Clone(Dictionary<AstElement, AstElement> alreadyClonedElements)
		{
			NamedTableReference result = new NamedTableReference();
			result.TableName = _tableName;
			result.TableNameSourceRange = _tableNameSourceRange;
			result.CorrelationName = _correlationName;
			result.CorrelationNameSourceRange = _correlationNameSourceRange;
			result.TableRefBinding = _tableRefBinding;
			return result;
		}
	}
}