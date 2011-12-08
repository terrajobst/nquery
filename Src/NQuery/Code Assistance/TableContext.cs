using System;

using NQuery.Runtime;

namespace NQuery.CodeAssistance
{
	internal sealed class TableContext : MemberCompletionContext
	{
		private Scope _scope;
		private TableBinding _tableBinding;
		private string _correlationName;

		public TableContext(SourceLocation sourceLocation, Identifier remainingPart, Scope scope, TableBinding tableBinding, string correlationName)
			: base(sourceLocation, remainingPart)
		{
			_scope = scope;
			_tableBinding = tableBinding;
			_correlationName = correlationName;
		}

		public override void Enumerate(IMemberCompletionAcceptor acceptor)
		{
			if (acceptor == null)
				throw ExceptionBuilder.ArgumentNull("acceptor");
			
			// Report all columns accessible by the table ref.
							
			TableRefBinding tableRefBinding = new TableRefBinding(null, _tableBinding, _correlationName);
			foreach (ColumnBinding columnBinding in _tableBinding.Columns)
			{
				ColumnRefBinding columnRefBinding = new ColumnRefBinding(tableRefBinding, columnBinding);
				acceptor.AcceptColumnRef(columnRefBinding);
			}
							
			// Now contribute any methods accessible by the row type.

			IMethodProvider methodProvider = _scope.DataContext.MetadataContext.MethodProviders[_tableBinding.RowType];
				
			if (methodProvider != null)
			{
				MethodBinding[] methods = methodProvider.GetMethods(_tableBinding.RowType);
				foreach (MethodBinding methodBinding in  methods)
					acceptor.AcceptMethod(methodBinding);
			}
		}
	}
}