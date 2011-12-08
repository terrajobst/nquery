using System;
using System.Collections;
using System.Collections.Generic;

using NQuery.Runtime;

namespace NQuery.Compilation
{
	internal sealed class CommonTableBinding : TableBinding
	{
		private string _name;
		private QueryNode _anchorMember;
		private QueryNode[] _recursiveMembers;

		public CommonTableBinding(string name, QueryNode anchorMember)
		{
			_name = name;
			_anchorMember = anchorMember;
		}

		protected override IList<ColumnBinding> BuildColumns()
		{
			List<ColumnBinding> result = new List<ColumnBinding>();
			foreach (SelectColumn selectColumn in _anchorMember.GetColumns())
			{
				DerivedColumnBinding derivedColumnBinding = new DerivedColumnBinding(this, selectColumn.Alias.Text, selectColumn.Expression.ExpressionType);
				result.Add(derivedColumnBinding);
			}
			return result;
		}

		public QueryNode AnchorMember
		{
			get { return _anchorMember; }
		}

		public QueryNode[] RecursiveMembers
		{
			get { return _recursiveMembers; }
			set { _recursiveMembers = value; }
		}

		public bool IsRecursive
		{
			get { return _recursiveMembers != null && _recursiveMembers.Length > 0; }
		}

		public override Type RowType
		{
			get { throw new NotImplementedException(); }
		}

		public override IEnumerable GetRows(ColumnRefBinding[] neededColumns)
		{
			throw new NotImplementedException();
		}

		public override string Name
		{
			get { return _name; }
		}
	}
}