using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

using NQuery.Runtime;

namespace NQuery.Compilation
{
	internal sealed class XmlProducer : StandardVisitor
	{
		private XmlWriter _xmlWriter;

		private XmlProducer(XmlWriter xmlWriter)
		{
			_xmlWriter = xmlWriter;
		}

		public static void ProduceFile(string fileName, AstNode node)
		{
			using (StreamWriter sw = new StreamWriter(fileName))
			{
				XmlTextWriter xmlWriter = new XmlTextWriter(sw);
				XmlProducer producer = new XmlProducer(xmlWriter);

				xmlWriter.WriteStartElement("ast");
				if (node != null)
					producer.Visit(node);
				xmlWriter.WriteEndElement();
			}
		}

		#region Xml Write Helpers

		private void WriteTypeAttribute(Type type)
		{
			if (type != null)
				_xmlWriter.WriteAttributeString("type", type.FullName);
		}

		private void WriteAstNode(string name, AstNode node)
		{
			if (node != null)
			{
				_xmlWriter.WriteStartElement(name);
				Visit(node);
				_xmlWriter.WriteEndElement();
			}
		}

		private void WriteTableRef(TableRefBinding tableRefBinding)
		{
			_xmlWriter.WriteAttributeString("table", tableRefBinding.TableBinding.Name);
			_xmlWriter.WriteAttributeString("tableRef", tableRefBinding.Name);
		}

		private void WriteColumnRef(ColumnRefBinding columnRefBinding)
		{
			_xmlWriter.WriteAttributeString("column", columnRefBinding.Name);
			WriteTableRef(columnRefBinding.TableRefBinding);
		}

		private void WriteColumns(SelectColumn[] selectColumns)
		{
			for (int i = 0; i < selectColumns.Length; i++)
			{
				SelectColumn selectSelectColumn = selectColumns[i];

				_xmlWriter.WriteStartElement("column");
				_xmlWriter.WriteAttributeString("index", XmlConvert.ToString(i));
				_xmlWriter.WriteAttributeString("isStar", XmlConvert.ToString(selectSelectColumn.IsAsterisk));

				if (selectSelectColumn.Alias != null)
					_xmlWriter.WriteAttributeString("alias", selectSelectColumn.Alias.ToSource());

				if (selectSelectColumn.Expression != null)
					Visit(selectSelectColumn.Expression);

				_xmlWriter.WriteEndElement();
			}
		}

		private void WriteOrderBy(OrderByColumn[] orderByColumns)
		{
			if (orderByColumns != null)
			{
				_xmlWriter.WriteStartElement("orderBy");
				_xmlWriter.WriteStartElement("columns");
				for (int i = 0; i < orderByColumns.Length; i++)
				{
					OrderByColumn column = orderByColumns[i];

					_xmlWriter.WriteStartElement("column");
					_xmlWriter.WriteAttributeString("index", XmlConvert.ToString(i));

					WriteAstNode("expression", column.Expression);

					_xmlWriter.WriteEndElement();
				}
				_xmlWriter.WriteEndElement();
				_xmlWriter.WriteEndElement();
			}
		}

		private void WriteRowBufferEntries(string name, IEnumerable<RowBufferEntry> rowBufferEntries)
		{
			if (rowBufferEntries != null)
			{
				_xmlWriter.WriteStartElement(name);

				foreach (RowBufferEntry rowBufferEntry in rowBufferEntries)
				{
					_xmlWriter.WriteStartElement("entry");
					_xmlWriter.WriteAttributeString("name", rowBufferEntry.Name);
					_xmlWriter.WriteEndElement();
				}

				_xmlWriter.WriteEndElement();
			}
		}

		#endregion

		#region Expressions

		public override LiteralExpression VisitLiteralValue(LiteralExpression expression)
		{
			_xmlWriter.WriteStartElement("literalExpression");

			_xmlWriter.WriteStartElement("value");
			WriteTypeAttribute(expression.ExpressionType);

			object value = expression.GetValue();
			if (NullHelper.IsNull(value))
				_xmlWriter.WriteAttributeString("value", "<null>");
			else
				_xmlWriter.WriteAttributeString("value", value.ToString());
			_xmlWriter.WriteEndElement();

			_xmlWriter.WriteEndElement();

			return expression;
		}

		public override ExpressionNode VisitUnaryExpression(UnaryExpression expression)
		{
			_xmlWriter.WriteStartElement("unaryExpression");
			_xmlWriter.WriteAttributeString("operator", expression.Op.TokenText);
			Visit(expression.Operand);
			_xmlWriter.WriteEndElement();

			return expression;
		}

		public override ExpressionNode VisitBinaryExpression(BinaryExpression expression)
		{
			_xmlWriter.WriteStartElement("binaryExpression");
			_xmlWriter.WriteAttributeString("operator", expression.Op.TokenText);
			WriteTypeAttribute(expression.ExpressionType);

			Visit(expression.Left);
			Visit(expression.Right);

			_xmlWriter.WriteEndElement();
			return expression;
		}

		public override ExpressionNode VisitBetweenExpression(BetweenExpression expression)
		{
			_xmlWriter.WriteStartElement("betweenExpression");
			WriteTypeAttribute(expression.ExpressionType);

			WriteAstNode("lowerBound", expression.LowerBound);
			WriteAstNode("upperBound", expression.UpperBound);

			_xmlWriter.WriteEndElement();

			return expression;
		}

		public override ExpressionNode VisitIsNullExpression(IsNullExpression expression)
		{
			_xmlWriter.WriteStartElement("isNullExpression");
			_xmlWriter.WriteAttributeString("negated", XmlConvert.ToString(expression.Negated));
			WriteTypeAttribute(expression.ExpressionType);

			WriteAstNode("expression", expression.Expression);

			_xmlWriter.WriteEndElement();

			return expression;
		}

		public override ExpressionNode VisitCastExpression(CastExpression expression)
		{
			_xmlWriter.WriteStartElement("castExpression");
			_xmlWriter.WriteAttributeString("typeName", expression.TypeReference.TypeName);
			WriteTypeAttribute(expression.ExpressionType);

			WriteAstNode("expression", expression.Expression);

			_xmlWriter.WriteEndElement();

			return expression;
		}

		public override ExpressionNode VisitCaseExpression(CaseExpression expression)
		{
			_xmlWriter.WriteStartElement("caseExpression");
			WriteTypeAttribute(expression.ExpressionType);

			if (expression.InputExpression != null)
				WriteAstNode("inputExpression", expression.InputExpression);

			for (int i = 0; i < expression.WhenExpressions.Length; i++)
			{
				_xmlWriter.WriteStartElement("whenThenPair");
				_xmlWriter.WriteAttributeString("index", XmlConvert.ToString(i));

				WriteAstNode("whenExpression", expression.WhenExpressions[i]);
				WriteAstNode("thenExpression", expression.ThenExpressions[i]);

				_xmlWriter.WriteEndElement();
			}

			if (expression.ElseExpression != null)
				WriteAstNode("elseExpression", expression.ElseExpression);

			_xmlWriter.WriteEndElement();

			return expression;
		}

		public override ExpressionNode VisitCoalesceExpression(CoalesceExpression expression)
		{
			_xmlWriter.WriteStartElement("coalesceExpression");
			WriteTypeAttribute(expression.ExpressionType);

			for (int i = 0; i < expression.Expressions.Length; i++)
			{
				_xmlWriter.WriteStartElement("expression");
				_xmlWriter.WriteAttributeString("index", XmlConvert.ToString(i));
				Visit(expression.Expressions[i]);
				_xmlWriter.WriteEndElement();
			}

			_xmlWriter.WriteEndElement();

			return expression;
		}

		public override ExpressionNode VisitNullIfExpression(NullIfExpression expression)
		{
			_xmlWriter.WriteStartElement("nullIfExpression");
			WriteTypeAttribute(expression.ExpressionType);

			WriteAstNode("leftExpression", expression.LeftExpression);
			WriteAstNode("rightExpression", expression.RightExpression);

			_xmlWriter.WriteEndElement();

			return expression;
		}

		public override ExpressionNode VisitInExpression(InExpression expression)
		{
			_xmlWriter.WriteStartElement("inExpression");
			WriteTypeAttribute(expression.ExpressionType);

			WriteAstNode("left", expression.Left);

			for (int i = 0; i < expression.RightExpressions.Length; i++)
			{
				_xmlWriter.WriteStartElement("rightExpression");
				_xmlWriter.WriteAttributeString("index", XmlConvert.ToString(i));
				Visit(expression.RightExpressions[i]);
				_xmlWriter.WriteEndElement();
			}

			_xmlWriter.WriteEndElement();

			return expression;
		}

		public override ExpressionNode VisitNamedConstantExpression(NamedConstantExpression expression)
		{
			_xmlWriter.WriteStartElement("namedConstantExpression");
			_xmlWriter.WriteAttributeString("constant", expression.Constant.Name);
			WriteTypeAttribute(expression.ExpressionType);
			_xmlWriter.WriteEndElement();

			return expression;
		}

		public override ExpressionNode VisitParameterExpression(ParameterExpression expression)
		{
			_xmlWriter.WriteStartElement("parameterExpression");
			_xmlWriter.WriteAttributeString("parameter", expression.Name.ToSource());
			WriteTypeAttribute(expression.ExpressionType);
			_xmlWriter.WriteEndElement();

			return expression;
		}

		public override ExpressionNode VisitNameExpression(NameExpression expression)
		{
			_xmlWriter.WriteStartElement("nameExpression");
			_xmlWriter.WriteAttributeString("identifier", expression.Name.ToSource());
			WriteTypeAttribute(expression.ExpressionType);
			_xmlWriter.WriteEndElement();

			return expression;
		}

		public override ExpressionNode VisitPropertyAccessExpression(PropertyAccessExpression expression)
		{
			_xmlWriter.WriteStartElement("propertyExpression");
			_xmlWriter.WriteAttributeString("identifier", expression.Name.ToSource());
			WriteTypeAttribute(expression.ExpressionType);

			WriteAstNode("target", expression.Target);

			_xmlWriter.WriteEndElement();

			return expression;
		}

		public override ExpressionNode VisitFunctionInvocationExpression(FunctionInvocationExpression expression)
		{
			_xmlWriter.WriteStartElement("functionInvocationExpression");
			_xmlWriter.WriteAttributeString("identifier", expression.Name.ToSource());
			WriteTypeAttribute(expression.ExpressionType);

			for (int i = 0; i < expression.Arguments.Length; i++)
			{
				_xmlWriter.WriteStartElement("argument");
				_xmlWriter.WriteAttributeString("index", XmlConvert.ToString(i));
				Visit(expression.Arguments[i]);
				_xmlWriter.WriteEndElement();
			}

			_xmlWriter.WriteEndElement();

			return expression;
		}

		public override ExpressionNode VisitMethodInvocationExpression(MethodInvocationExpression expression)
		{
			_xmlWriter.WriteStartElement("functionInvocationExpression");
			_xmlWriter.WriteAttributeString("identifier", expression.Name.ToSource());
			WriteTypeAttribute(expression.ExpressionType);

			WriteAstNode("target", expression.Target);

			for (int i = 0; i < expression.Arguments.Length; i++)
			{
				_xmlWriter.WriteStartElement("argument");
				_xmlWriter.WriteAttributeString("index", XmlConvert.ToString(i));
				Visit(expression.Arguments[i]);
				_xmlWriter.WriteEndElement();
			}

			_xmlWriter.WriteEndElement();

			return expression;
		}

		public override ExpressionNode VisitColumnExpression(ColumnExpression expression)
		{
			_xmlWriter.WriteStartElement("columnExpression");
			WriteColumnRef(expression.Column);
			WriteTypeAttribute(expression.ExpressionType);
			_xmlWriter.WriteEndElement();

			return expression;
		}

		public override ExpressionNode VisitAggregagateExpression(AggregateExpression expression)
		{
			_xmlWriter.WriteStartElement("aggregagateExpression");
			_xmlWriter.WriteAttributeString("aggregateFunction", expression.Aggregate.Name);
			WriteTypeAttribute(expression.ExpressionType);

			WriteAstNode("argument", expression.Argument);

			_xmlWriter.WriteEndElement();

			return expression;
		}

		public override ExpressionNode VisitSingleRowSubselect(SingleRowSubselect expression)
		{
			_xmlWriter.WriteStartElement("singleRowSubselect");
			WriteTypeAttribute(expression.ExpressionType);

			WriteAstNode("query", expression.Query);

			_xmlWriter.WriteEndElement();

			return expression;
		}

		public override ExpressionNode VisitExistsSubselect(ExistsSubselect expression)
		{
			_xmlWriter.WriteStartElement("existsSubselect");
			_xmlWriter.WriteAttributeString("negated", XmlConvert.ToString(expression.Negated));
			WriteTypeAttribute(expression.ExpressionType);

			WriteAstNode("query", expression.Query);

			_xmlWriter.WriteEndElement();

			return expression;
		}

		public override ExpressionNode VisitAllAnySubselect(AllAnySubselect expression)
		{
			_xmlWriter.WriteStartElement("allAnySubselect");
			_xmlWriter.WriteAttributeString("op", expression.Op.TokenText);
			_xmlWriter.WriteAttributeString("type", expression.Type.ToString());
			WriteTypeAttribute(expression.ExpressionType);

			WriteAstNode("left", expression.Left);
			WriteAstNode("query", expression.Query);

			_xmlWriter.WriteEndElement();

			return expression;
		}

		#endregion

		#region Query

		public override TableReference VisitNamedTableReference(NamedTableReference node)
		{
			_xmlWriter.WriteStartElement("namedTableReference");
			_xmlWriter.WriteAttributeString("table", node.TableName.ToSource());
			_xmlWriter.WriteEndElement();

			return node;
		}

		public override TableReference VisitJoinedTableReference(JoinedTableReference node)
		{
			_xmlWriter.WriteStartElement("joinedTableReference");
			_xmlWriter.WriteAttributeString("joinType", node.JoinType.ToString());

			WriteAstNode("condition", node.Condition);
			WriteAstNode("left", node.Left);
			WriteAstNode("right", node.Right);

			_xmlWriter.WriteEndElement();

			return node;
		}

		public override TableReference VisitDerivedTableReference(DerivedTableReference node)
		{
			_xmlWriter.WriteStartElement("queryTableReference");
			_xmlWriter.WriteAttributeString("correlationName", node.CorrelationName.ToSource());
			WriteAstNode("query", node.Query);

			_xmlWriter.WriteEndElement();

			return node;
		}

		public override QueryNode VisitSelectQuery(SelectQuery query)
		{
			_xmlWriter.WriteStartElement("selectQuery");

			_xmlWriter.WriteStartElement("columns");
			WriteColumns(query.SelectColumns);
			_xmlWriter.WriteEndElement();

			WriteAstNode("from", query.TableReferences);
			WriteAstNode("where", query.WhereClause);

			if (query.GroupByColumns != null)
			{
				_xmlWriter.WriteStartElement("groupBy");

				for (int i = 0; i < query.GroupByColumns.Length; i++)
				{
					ExpressionNode groupByExpression = query.GroupByColumns[i];

					_xmlWriter.WriteStartElement("column");
					_xmlWriter.WriteAttributeString("index", XmlConvert.ToString(i));
					Visit(groupByExpression);
					_xmlWriter.WriteEndElement();
				}
				_xmlWriter.WriteEndElement();
			}

			WriteAstNode("having", query.HavingClause);

			if (query.OrderByColumns != null)
				WriteOrderBy(query.OrderByColumns);

			_xmlWriter.WriteEndElement();

			return query;
		}

		public override QueryNode VisitBinaryQuery(BinaryQuery query)
		{
			_xmlWriter.WriteStartElement("binaryQuery");
			_xmlWriter.WriteAttributeString("op", query.Op.ToString());

			WriteAstNode("left", query.Left);
			WriteAstNode("right", query.Right);

			_xmlWriter.WriteEndElement();

			return query;
		}

		public override QueryNode VisitSortedQuery(SortedQuery query)
		{
			_xmlWriter.WriteStartElement("sortedQuery");

			WriteAstNode("input", query.Input);
			WriteOrderBy(query.OrderByColumns);

			_xmlWriter.WriteEndElement();

			return query;
		}

		public override QueryNode VisitCommonTableExpressionQuery(CommonTableExpressionQuery query)
		{
			_xmlWriter.WriteStartElement("commonTableExpressionQuery");

			_xmlWriter.WriteStartElement("commonTableExpressions");
			foreach (CommonTableExpression expression in query.CommonTableExpressions)
			{
				_xmlWriter.WriteStartElement("table");
				_xmlWriter.WriteAttributeString("name", expression.TableName.ToString());
				WriteAstNode("queryDeclaration", expression.QueryDeclaration);
				_xmlWriter.WriteEndElement();
			}
			_xmlWriter.WriteEndElement();

			WriteAstNode("input", query.Input);

			_xmlWriter.WriteEndElement();

			return query;
		}

		#endregion

		#region Algebra Nodes

		public override AlgebraNode VisitTableAlgebraNode(TableAlgebraNode node)
		{
			_xmlWriter.WriteStartElement("tableAlgebraNode");
			WriteTableRef(node.TableRefBinding);

			_xmlWriter.WriteEndElement();

			return node;
		}

		public override AlgebraNode VisitJoinAlgebraNode(JoinAlgebraNode node)
		{
			_xmlWriter.WriteStartElement("joinAlgebraNode");
			_xmlWriter.WriteAttributeString("op", node.Op.ToString());

			WriteRowBufferEntries("externalColumnDependencies", node.OuterReferences);
			WriteAstNode("leftNode", node.Left);
			WriteAstNode("rightNode", node.Right);

			WriteAstNode("joinCondition", node.Predicate);

			_xmlWriter.WriteEndElement();

			return node;
		}

		public override AlgebraNode VisitConstantScanAlgebraNode(ConstantScanAlgebraNode node)
		{
			_xmlWriter.WriteStartElement("constantScanAlgebraNode");
			_xmlWriter.WriteEndElement();

			return node;
		}

		public override AlgebraNode VisitConcatAlgebraNode(ConcatAlgebraNode node)
		{
			_xmlWriter.WriteStartElement("concatAlgebraNode");

			for (int i = 0; i < node.Inputs.Length; i++)
				Visit(node.Inputs[i]);

			_xmlWriter.WriteEndElement();

			return node;
		}

		public override AlgebraNode VisitSortAlgebraNode(SortAlgebraNode node)
		{
			_xmlWriter.WriteStartElement("sortAlgebraNode");
			_xmlWriter.WriteAttributeString("distinct", XmlConvert.ToString(node.Distinct));

			//WriteAstNode("input", node.Input);

			//for (int i = 0; i < node.SelectColumns.Length; i++)
			//{
			//    _xmlWriter.WriteStartElement("column");   
			//    _xmlWriter.WriteAttributeString("item", node.SelectColumns[i].Name);
			//    _xmlWriter.WriteAttributeString("order", node.SortOrders[i].ToString());
			//    _xmlWriter.WriteEndElement();
			//}

			_xmlWriter.WriteEndElement();

			return node;
		}

		public override AlgebraNode VisitAggregateAlgebraNode(AggregateAlgebraNode node)
		{
			_xmlWriter.WriteStartElement("aggregateAlgebraNode");

			WriteAstNode("input", node.Input);
			WriteRowBufferEntries("groups", node.Groups);
			//WriteAstNodes("aggregates", node.Aggregates);

			_xmlWriter.WriteEndElement();
			return node;
		}

		public override AlgebraNode VisitTopAlgebraNode(TopAlgebraNode node)
		{
			_xmlWriter.WriteStartElement("topAlgebraNode");
			_xmlWriter.WriteAttributeString("limit", XmlConvert.ToString(node.Limit));

			WriteAstNode("input", node.Input);

			_xmlWriter.WriteEndElement();

			return node;
		}

		public override AlgebraNode VisitFilterAlgebraNode(FilterAlgebraNode node)
		{
			_xmlWriter.WriteStartElement("filterAlgebraNode");

			WriteAstNode("input", node.Input);
			WriteAstNode("predicate", node.Predicate);

			_xmlWriter.WriteEndElement();

			return node;
		}


		public override AlgebraNode VisitAssertAlgebraNode(AssertAlgebraNode node)
		{
			_xmlWriter.WriteStartElement("assertAlgebraNode");
			_xmlWriter.WriteAttributeString("assertionType", node.AssertionType.ToString());

			WriteAstNode("input", node.Input);
			WriteAstNode("predicate", node.Predicate);

			_xmlWriter.WriteEndElement();

			return node;
		}

		#endregion
	}
}
