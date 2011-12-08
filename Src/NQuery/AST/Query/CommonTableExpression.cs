using System;
using System.Collections.Generic;

namespace NQuery.Compilation
{
	internal sealed class CommonTableExpression : AstElement
	{
		private Identifier _tableName;
		private SourceRange _tableNameSourceRange;
		private Identifier[] _columnNames;
		private QueryNode _queryDeclaration;
		private CommonTableBinding _commonTableBinding;

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

		public Identifier[] ColumnNames
		{
			get { return _columnNames; }
			set { _columnNames = value; }
		}

		public QueryNode QueryDeclaration
		{
			get { return _queryDeclaration; }
			set { _queryDeclaration = value; }
		}

		public CommonTableBinding CommonTableBinding
		{
			get { return _commonTableBinding; }
			set { _commonTableBinding = value; }
		}

		public override AstElement Clone(Dictionary<AstElement, AstElement> alreadyClonedElements)
		{
			CommonTableExpression result = new CommonTableExpression();
			result.TableName = _tableName;
			result.TableNameSourceRange = _tableNameSourceRange;
			result.ColumnNames = (Identifier[]) _columnNames.Clone();
			result.QueryDeclaration = (QueryNode)_queryDeclaration.Clone(alreadyClonedElements);
			result.CommonTableBinding = _commonTableBinding;
			return result;
		}
	}
}