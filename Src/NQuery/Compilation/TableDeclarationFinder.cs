using System;
using System.Collections.Generic;

using NQuery.Runtime;

namespace NQuery.Compilation
{
	internal sealed class TableDeclarationFinder : StandardVisitor
	{
		private List<TableRefBinding> _tableRefBindingList = new List<TableRefBinding>();
		
		public void Clear()
		{
			_tableRefBindingList.Clear();
		}
					
		public TableRefBinding[] GetDeclaredTables()
		{
			return _tableRefBindingList.ToArray();
		}
		
		public override TableReference VisitNamedTableReference(NamedTableReference node)
		{
			_tableRefBindingList.Add(node.TableRefBinding);
			return node;
		}

		// NOTE: Since we are only interetested in table references of the root query
		//       we don't visit nested queries.
		
		public override TableReference VisitDerivedTableReference(DerivedTableReference node)
		{
			_tableRefBindingList.Add(node.DerivedTableBinding);
			return node;
		}

		public override ExpressionNode VisitSingleRowSubselect(SingleRowSubselect expression)
		{
			return expression;
		}

		public override ExpressionNode VisitExistsSubselect(ExistsSubselect expression)
		{
			return expression;
		}

		public override ExpressionNode VisitAllAnySubselect(AllAnySubselect expression)
		{
			return expression;
		}
	}
}