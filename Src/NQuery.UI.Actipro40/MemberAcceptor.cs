using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using ActiproSoftware.SyntaxEditor;

using NQuery.Runtime;

namespace NQuery.UI
{
	internal sealed class MemberAcceptor : IMemberCompletionAcceptor
	{
		private IntelliPromptMemberList _members;
		private Dictionary<string, List<InvocableBinding>> _functionsTable = new Dictionary<string, List<InvocableBinding>>();
		private Dictionary<string, List<MemberEntry>> _memberEntries = new Dictionary<string, List<MemberEntry>>();

		private class MemberEntry
		{
			public int ImageIndex;
			public string Description;
		}

		private const int AMBIGUOUS_NAME_IMG_INDEX = 0;
		private const int TABLE_IMG_INDEX = 1;
		private const int TABLE_REF_IMG_INDEX = 2;
		private const int PROPERTY_IMG_INDEX = 3;
		private const int FUNCTION_IMG_INDEX = 4;
		private const int KEYWORD_IMG_INDEX = 5;
		private const int AGGREGATE_IMG_INDEX = 6;
		private const int PARAMETER_IMG_INDEX = 7;
		private const int RELATION_IMG_INDEX = 8;

		public MemberAcceptor(IntelliPromptMemberList members)
		{
			_members = members;
		}

		private void Add(string name, string fullname, string datatype, int imageIndex)
		{
            List<MemberEntry> membersWithSameName;
		    
            if (!_memberEntries.TryGetValue(name, out membersWithSameName))
			{
				membersWithSameName = new List<MemberEntry>();
				_memberEntries.Add(name, membersWithSameName);
			}

			MemberEntry entry = new MemberEntry();
			entry.Description = String.Format(CultureInfo.CurrentCulture, "<b>{0}</b> : {1}", fullname, datatype);
			entry.ImageIndex = imageIndex;

			membersWithSameName.Add(entry);
		}

		private void AddWithoutDuplicationCheck(string name, string fullname, string datatype, int imageIndex)
		{
			string preText = MaskIdentifier(name);
			string description = String.Format(CultureInfo.CurrentCulture, "<b>{0}</b> : {1}", fullname, datatype); 
			IntelliPromptMemberListItem item = new IntelliPromptMemberListItem(name, imageIndex, description, preText, String.Empty);
			_members.Add(item);
		}

		public void FlushBuffer()
		{
			AddBufferedMembers();
			AddBufferedFunctions();	
		}

		private static string MaskIdentifier(string name)
		{
			if (name.IndexOfAny(new char[] {' ', '\t'}) >= 0)
				return "[" + name + "]";

			return name;
		}

		private void AddBufferedMembers()
		{
			foreach (string name in _memberEntries.Keys)
			{
                List<MemberEntry> membersWithSameName = _memberEntries[name];
                
				MemberEntry entry = membersWithSameName[0];

				if (membersWithSameName.Count == 1)
				{
					string preText = MaskIdentifier(name);
					IntelliPromptMemberListItem item = new IntelliPromptMemberListItem(name, entry.ImageIndex, entry.Description, preText, String.Empty);
					_members.Add(item);
				}
				else
				{
					StringBuilder sb = new StringBuilder();
					sb.Append("(Ambiguous name -- choose from the following:)");
					foreach (MemberEntry entryWithSameName in membersWithSameName)
					{
						sb.Append("<br/>");
						sb.Append(entryWithSameName.Description);
					}

					string description = sb.ToString();
					string preText = MaskIdentifier(name);
					IntelliPromptMemberListItem item = new IntelliPromptMemberListItem(name, AMBIGUOUS_NAME_IMG_INDEX, description, preText, String.Empty);
					_members.Add(item);												
				}
			}
		}

		private void AddBufferedFunctions()
		{
			foreach (string functionName in _functionsTable.Keys)
			{
                List<InvocableBinding> functionList = _functionsTable[functionName];
				InvocableBinding invocable = functionList[0];

				string description;
					
				if (invocable is FunctionBinding)
				{
					if (functionList.Count == 1)
						description = String.Format(CultureInfo.CurrentCulture, "<b>{0}</b> : {1}", invocable.Name, invocable.ReturnType.Name);
					else
						description = String.Format(CultureInfo.CurrentCulture, "<b>{0}</b> : {1} (+ {2} Overloadings)", invocable.Name, invocable.ReturnType.Name, functionList.Count);
				}
				else
				{
					MethodBinding method = (MethodBinding) invocable;

					if (functionList.Count == 1)
						description = String.Format(CultureInfo.CurrentCulture, "{0}.<b>{1}</b> : {2}", method.DeclaringType.Name, method.Name, method.ReturnType.Name);
					else
						description = String.Format(CultureInfo.CurrentCulture, "{0}.<b>{1}</b> : {2} (+ {3} Overloadings)", method.DeclaringType.Name, method.Name, method.ReturnType.Name, functionList.Count);
				}

				string preText = MaskIdentifier(invocable.Name);

				IntelliPromptMemberListItem item = new IntelliPromptMemberListItem(invocable.Name, FUNCTION_IMG_INDEX, description, preText, String.Empty);
				_members.Add(item);
			}
		}

		public void AcceptTable(TableBinding table)
		{
			AddWithoutDuplicationCheck(table.Name, table.GetFullName(), "TABLE", TABLE_IMG_INDEX);
		}

		public void AcceptTableRef(TableRefBinding tableRef)
		{
			Add(tableRef.Name, tableRef.GetFullName(), "TABLE REF", TABLE_REF_IMG_INDEX);
		}

		public void AcceptColumnRef(ColumnRefBinding columnRef)
		{
			Add(columnRef.Name, columnRef.GetFullName(), columnRef.ColumnBinding.DataType.Name, PROPERTY_IMG_INDEX);
		}

		public void AcceptProperty(PropertyBinding property)
		{
			Add(property.Name, property.GetFullName(), property.DataType.Name, PROPERTY_IMG_INDEX);
		}

		public void AcceptKeyword(string keyword)
		{
			AddWithoutDuplicationCheck(keyword, keyword, "KEYWORD", KEYWORD_IMG_INDEX);
		}

		public void AcceptAggregate(AggregateBinding aggregate)
		{
			Add(aggregate.Name, aggregate.GetFullName(), "AGGREGATE", AGGREGATE_IMG_INDEX);
		}

		public void AcceptFunction(FunctionBinding function)
		{
			AddInvocable(function);
		}

		public void AcceptMethod(MethodBinding method)
		{
			AddInvocable(method);
		}

		private void AddInvocable(InvocableBinding invocable)
		{
			List<InvocableBinding> functionList;

            if (!_functionsTable.TryGetValue(invocable.Name, out functionList))
			{
                functionList = new List<InvocableBinding>();
				_functionsTable.Add(invocable.Name, functionList);
			}
				
			functionList.Add(invocable);
		}

		public void AcceptParameter(ParameterBinding parameter)
		{
			Add("@" + parameter.Name, parameter.GetFullName(), parameter.DataType.Name, PARAMETER_IMG_INDEX);
			Add(parameter.Name, parameter.GetFullName(), parameter.DataType.Name, PARAMETER_IMG_INDEX);
		}

		public void AcceptConstant(ConstantBinding constant)
		{
			Add(constant.Name, constant.GetFullName(), constant.DataType.Name, PROPERTY_IMG_INDEX);
		}

		public void AcceptRelation(TableRefBinding parentBinding, TableRefBinding childBinding, TableRelation relation)
		{
			ColumnBindingCollection parentColumns;
			if (parentBinding.TableBinding == relation.ParentTable)
				parentColumns = relation.ParentColumns;
			else
				parentColumns = relation.ChildColumns;

			ColumnBindingCollection childColumns;
			if (childBinding.TableBinding == relation.ChildTable)
				childColumns = relation.ChildColumns;
			else
				childColumns = relation.ParentColumns;

			// Item in member list:
			// ~~~~~~~~~~~~~~~~~~~~
			// parentRef(col1, col2) ---> childRef(col1, col2)
				
			StringBuilder sb = new StringBuilder();
			sb.Append(parentBinding.Name);
			sb.Append(" (");
			for (int i = 0; i < relation.ColumnCount; i++)
			{
				if (i > 0)
					sb.Append(", ");
					
				sb.Append(parentColumns[i].Name);
			}
			sb.Append(") ---> ");
			sb.Append(childBinding.Name);
			sb.Append(" (");
			for (int i = 0; i < relation.ColumnCount; i++)
			{
				if (i > 0)
					sb.Append(", ");
					
				sb.Append(childColumns[i].Name);
			}
			sb.Append(")");

			string name = sb.ToString();
			sb.Length = 0;
				
			// Description:
			// ~~~~~~~~~~~~
			// Relation <b>parentTable parentRef</b> ---> <b>ChildTable childRef</b> <br/>
			// ON (parentRef.Col1 = childRef.Col1 AND parentRef.Col2 = childRef.Col2)
				
			sb.Append("Relation <br/>");
			sb.Append("<b>");
			sb.Append(MaskIdentifier(parentBinding.TableBinding.Name));
			sb.Append(" AS ");
			sb.Append(MaskIdentifier(parentBinding.Name));
			sb.Append("</b>");
			sb.Append(" ---> ");
			sb.Append("<b>");
			sb.Append(MaskIdentifier(childBinding.TableBinding.Name));
			sb.Append(" AS ");
			sb.Append(MaskIdentifier(childBinding.Name));
			sb.Append("</b><br/>ON (");
				
			for (int i = 0; i < relation.ColumnCount; i++)
			{
				if (i > 0)
					sb.Append(" AND ");
					
				sb.Append(MaskIdentifier(parentBinding.Name));
				sb.Append(".");
				sb.Append(MaskIdentifier(parentColumns[i].Name));
				sb.Append(" = ");
				sb.Append(MaskIdentifier(childBinding.Name));
				sb.Append(".");
				sb.Append(MaskIdentifier(childColumns[i].Name));
			}
			sb.Append(")");
				
			string description = sb.ToString();
			sb.Length = 0;
				
			// PreText:
			// ~~~~~~~~
			// parentAlias.Col1 = childAlias.Col1 AND parentAlias.Col2 = childAlias.Col2
				
			for (int i = 0; i < relation.ColumnCount; i++)
			{
				if (i > 0)
					sb.Append(" AND ");
					
				sb.Append(MaskIdentifier(parentBinding.Name));
				sb.Append(".");
				sb.Append(MaskIdentifier(parentColumns[i].Name));
				sb.Append(" = ");
				sb.Append(MaskIdentifier(childBinding.Name));
				sb.Append(".");
				sb.Append(MaskIdentifier(childColumns[i].Name));
			}

			string preText = sb.ToString();
			sb.Length = 0;
				
			IntelliPromptMemberListItem item = new IntelliPromptMemberListItem(name, RELATION_IMG_INDEX, description, preText, String.Empty);
			_members.Add(item);												
		}
	}
}