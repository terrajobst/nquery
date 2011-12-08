using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using NQuery.Runtime.ExecutionPlan;

namespace NQuery.Compilation
{
	internal sealed class ShowPlanBuilder : StandardVisitor
	{
		private ShowPlanElement _currentElement;

		private ShowPlanBuilder()
		{
		}

		public static ShowPlanElement Convert(AlgebraNode algebraNode)
		{
			if (algebraNode == null)
				return new ShowPlanElement(ShowPlanOperator.Select, new ShowPlanProperty[0]);

			ShowPlanBuilder showPlanBuilder = new ShowPlanBuilder();
			return showPlanBuilder.ConvertNode(algebraNode);
		}

		#region Helpers

		private class PropertyListBuilder
		{
			private class Property
			{
				public string Name;
				public string Value;
				public List<Property> Children = new List<Property>();
			}

			private Property _root = new Property();
			private Stack<Property> _propertyStack = new Stack<Property>();

			private Property CurrentConatainer
			{
				get
				{
					if (_propertyStack.Count == 0)
						return _root;
					return _propertyStack.Peek();
				}
			}

			private static void AddChildrenToDictionary(List<ShowPlanProperty> target, string parentPath, Property property)
			{
				string path;
				if (parentPath == null)
					path = Escape(property.Name);
				else
					path = parentPath + "." + Escape(property.Name);

				if (property.Value != null)
				{
					ShowPlanProperty planProperty = new ShowPlanProperty(path, property.Value);
					target.Add(planProperty);
				}

				foreach (Property child in property.Children)
					AddChildrenToDictionary(target, path, child);
			}

			private static string Escape(string pathElement)
			{
				if (pathElement == null || !pathElement.Contains("."))
					return pathElement;

				StringBuilder sb = new StringBuilder();
				foreach (char c in pathElement)
				{
					if (c == '.')
						sb.Append("..");
					else
						sb.Append(c);
				}
				return sb.ToString();
			}

			public void Begin(string groupName)
			{
				Property property = new Property();
				property.Name = groupName;
				CurrentConatainer.Children.Add(property);
				_propertyStack.Push(property);
			}

			public void Begin()
			{
				Property property = new Property();
				property.Name = null;
				CurrentConatainer.Children.Add(property);
				_propertyStack.Push(property);
			}

			public void End()
			{
				Property closingGroup = _propertyStack.Pop();

				int maxDigitsOfIndex = System.Convert.ToString(closingGroup.Children.Count, CultureInfo.InvariantCulture).Length;
				string formatString = String.Format(CultureInfo.InvariantCulture, "[{{0:{0}}}]", new String('0', maxDigitsOfIndex));
				int groupMemberIndex = 0;

				foreach (Property groupMember in closingGroup.Children)
				{
					if (groupMember.Name == null)
						groupMember.Name = String.Format(CultureInfo.CurrentCulture, formatString, groupMemberIndex);
					groupMemberIndex++;
				}
			}

			public void Write(string name, string value)
			{
				Property property = new Property();
				property.Name = name;
				property.Value = value;
				CurrentConatainer.Children.Add(property);
			}

			public void SetGroupValue(string value)
			{
				Property groupProperty = _propertyStack.Peek();
				groupProperty.Value = value;
			}

			public IList<ShowPlanProperty> ToList()
			{
				List<ShowPlanProperty> properties = new List<ShowPlanProperty>();
				AddChildrenToDictionary(properties, null, _root);
				return properties;
			}
		}

		private ShowPlanElement ConvertNode(AlgebraNode node)
		{
			Visit(node);
			ShowPlanElement result = _currentElement;
			_currentElement = null;
			return result;
		}

		private static string LogicalOperatorToString(JoinAlgebraNode.JoinOperator op)
		{
			switch (op)
			{
				case JoinAlgebraNode.JoinOperator.InnerJoin:
					return Resources.ShowPlanLogicalOperatorInnerJoin;
				case JoinAlgebraNode.JoinOperator.LeftOuterJoin:
					return Resources.ShowPlanLogicalOperatorLeftOuterJoin;
				case JoinAlgebraNode.JoinOperator.LeftSemiJoin:
					return Resources.ShowPlanLogicalOperatorLeftSemiJoin;
				case JoinAlgebraNode.JoinOperator.LeftAntiSemiJoin:
					return Resources.ShowPlanLogicalOperatorLeftAntiSemiJoin;
				case JoinAlgebraNode.JoinOperator.RightOuterJoin:
					return Resources.ShowPlanLogicalOperatorRightOuterJoin;
				case JoinAlgebraNode.JoinOperator.RightSemiJoin:
					return Resources.ShowPlanLogicalOperatorRightSemiJoin;
				case JoinAlgebraNode.JoinOperator.RightAntiSemiJoin:
					return Resources.ShowPlanLogicalOperatorRightAntiSemiJoin;
				case JoinAlgebraNode.JoinOperator.FullOuterJoin:
					return Resources.ShowPlanLogicalOperatorFullOuterJoin;
				default:
					throw ExceptionBuilder.UnhandledCaseLabel(op);
			}
		}

		private static string SortOrderToString(SortOrder sortOrder)
		{
			switch (sortOrder)
			{
				case SortOrder.Ascending:
					return Resources.ShowPlanSortOrderAscending;
				case SortOrder.Descending:
					return Resources.ShowPlanSortOrderDescending;
				default:
					throw ExceptionBuilder.UnhandledCaseLabel(sortOrder);
			}
		}

		private static void AddStatistics(PropertyListBuilder builder, StatisticsIterator iterator)
		{
			if (iterator != null)
			{
				int openCount = iterator.OpenCount;
				int rowCount = iterator.RowCount;
				int avgRowCount = (openCount == 0) ? 0 : (int) Math.Round(rowCount /(double)openCount, 0);

				builder.Begin(Resources.ShowPlanGroupStatistics);
				builder.Write(Resources.ShowPlanKeyStatisticsOpenCount, openCount.ToString("N0", CultureInfo.InvariantCulture));
				builder.Write(Resources.ShowPlanKeyStatisticsRowCount, rowCount.ToString("N0", CultureInfo.InvariantCulture));
				builder.Write(Resources.ShowPlanKeyStatisticsAverageRowCount, avgRowCount.ToString("N0", CultureInfo.InvariantCulture));
				builder.End();
			}
		}

		private static void AddRowBufferEntries(PropertyListBuilder builder, string groupName, IEnumerable<RowBufferEntry> rowBufferColumns)
		{
			builder.Begin(groupName);
			foreach (RowBufferEntry rowBufferColumn in rowBufferColumns)
			{
				builder.Begin();
				builder.SetGroupValue(rowBufferColumn.Name);
				WriteRowBufferEntry(builder, rowBufferColumn);
				builder.End();
			}
			builder.End();
		}

		private static void WriteRowBufferEntry(PropertyListBuilder builder, string groupName, RowBufferEntry rowBufferColumn)
		{
			builder.Begin(groupName);
			builder.SetGroupValue(rowBufferColumn.Name);
			WriteRowBufferEntry(builder, rowBufferColumn);
			builder.End();
		}

		private static void WriteRowBufferEntry(PropertyListBuilder builder, RowBufferEntry rowBufferColumn)
		{
			string[] parts = rowBufferColumn.Name.Split('.');
			if (parts.Length != 2)
			{
				builder.Write(Resources.ShowPlanKeyColumn, rowBufferColumn.Name);
			}
			else
			{
				builder.Write(Resources.ShowPlanKeyTable, parts[0]);
				builder.Write(Resources.ShowPlanKeyColumn, parts[1]);
			}

			builder.Write(Resources.ShowPlanKeyDataType, rowBufferColumn.DataType.Name);
		}

		private static void AddDefinedValues(PropertyListBuilder builder, IEnumerable<ComputedValueDefinition> definedValues)
		{
			builder.Begin(Resources.ShowPlanGroupDefinedValues);

			foreach (ComputedValueDefinition definedValue in definedValues)
			{
				string sourceExpression = definedValue.Expression.GenerateSource();
				builder.Begin();
				builder.SetGroupValue(String.Format(CultureInfo.InvariantCulture, "{0} = {1}", definedValue.Target.Name, sourceExpression));
				builder.Write(Resources.ShowPlanKeyTarget, definedValue.Target.Name);
				builder.Write(Resources.ShowPlanKeyDataType, definedValue.Target.DataType.Name);
				builder.Write(Resources.ShowPlanKeySource, sourceExpression);
				builder.End();
			}
			builder.End();
		}

		private static void AddDefinedValues(PropertyListBuilder builder, IEnumerable<UnitedValueDefinition> definedValues)
		{
			builder.Begin(Resources.ShowPlanGroupDefinedValues);

			foreach (UnitedValueDefinition definedValue in definedValues)
			{
				builder.Begin(definedValue.Target.Name);

				StringBuilder sb = new StringBuilder();
				foreach (RowBufferEntry dependendValue in definedValue.DependendEntries)
				{
					builder.Begin();
					builder.SetGroupValue(dependendValue.Name);
					WriteRowBufferEntry(builder, dependendValue);
					builder.End();

					if (sb.Length > 0)
						sb.Append("; ");
					sb.Append(dependendValue.Name);
				}

				builder.SetGroupValue(sb.ToString());
				builder.End();
			}

			builder.End();
		}

		private static void AddDefinedValues(PropertyListBuilder builder, IEnumerable<AggregatedValueDefinition> definedValues)
		{
			builder.Begin(Resources.ShowPlanGroupDefinedValues);

			foreach (AggregatedValueDefinition definedValue in definedValues)
			{
				string source = String.Format(CultureInfo.InvariantCulture,
				                              "{0}({1})",
				                              definedValue.Aggregate.Name,
				                              definedValue.Argument.GenerateSource());

				builder.Begin();
				builder.SetGroupValue(String.Format(CultureInfo.InvariantCulture, "{0} = {1}", definedValue.Target.Name, source));
				builder.Write(Resources.ShowPlanKeyTarget, definedValue.Target.Name);
				builder.Write(Resources.ShowPlanKeyDataType, definedValue.Target.DataType.Name);
				builder.Write(Resources.ShowPlanKeySource, source);
				builder.End();
			}

			builder.End();
		}

		#endregion

		public override AlgebraNode VisitResultAlgebraNode(ResultAlgebraNode node)
		{
			ShowPlanElement inputElement = ConvertNode(node.Input);

			PropertyListBuilder propertyListBuilder = new PropertyListBuilder();
			propertyListBuilder.Begin(Resources.ShowPlanGroupOutputList);
			for (int i = 0; i < node.OutputList.Length; i++)
			{
				propertyListBuilder.Begin();
				propertyListBuilder.SetGroupValue(String.Format(CultureInfo.InvariantCulture, "{0} AS {1}", node.OutputList[i].Name, node.ColumnNames[i]));
				WriteRowBufferEntry(propertyListBuilder, node.OutputList[i]);
				propertyListBuilder.Write(Resources.ShowPlanKeyOutputName, node.ColumnNames[i]);
				propertyListBuilder.End();
			}
			propertyListBuilder.End();

			IList<ShowPlanProperty> properties = propertyListBuilder.ToList();

			ShowPlanElement element = new ShowPlanElement(ShowPlanOperator.Select, properties, inputElement);
			_currentElement = element;

			return node;
		}

		public override AlgebraNode VisitTableAlgebraNode(TableAlgebraNode node)
		{
			PropertyListBuilder propertyListBuilder = new PropertyListBuilder();
			propertyListBuilder.Write(Resources.ShowPlanKeyTable, String.Format(CultureInfo.InvariantCulture, "{0} AS {1}", node.TableRefBinding.TableBinding.Name, node.TableRefBinding.Name));
			AddRowBufferEntries(propertyListBuilder, Resources.ShowPlanGroupOutputList, node.OutputList);
			AddStatistics(propertyListBuilder, node.StatisticsIterator);

			IList<ShowPlanProperty> properties = propertyListBuilder.ToList();

			ShowPlanElement element = new ShowPlanElement(ShowPlanOperator.TableScan, properties);
			_currentElement = element;

			return node;
		}

		public override AlgebraNode VisitJoinAlgebraNode(JoinAlgebraNode node)
		{
			ShowPlanElement leftElement = ConvertNode(node.Left);
			ShowPlanElement rightElement = ConvertNode(node.Right);

			PropertyListBuilder propertyListBuilder = new PropertyListBuilder();
			AddRowBufferEntries(propertyListBuilder, Resources.ShowPlanGroupOutputList, node.OutputList);
			propertyListBuilder.Write(Resources.ShowPlanKeyLogicalOperator, LogicalOperatorToString(node.Op));
			AddStatistics(propertyListBuilder, node.StatisticsIterator);

			if (node.Predicate != null)
				propertyListBuilder.Write(Resources.ShowPlanKeyPredicate, node.Predicate.GenerateSource());

			if (node.OuterReferences != null && node.OuterReferences.Length > 0)
				AddRowBufferEntries(propertyListBuilder, Resources.ShowPlanGroupOuterReferences, node.OuterReferences);

			if (node.ProbeBufferEntry != null)
				WriteRowBufferEntry(propertyListBuilder, Resources.ShowPlanKeyProbeColumn, node.ProbeBufferEntry);

			if (node.PassthruPredicate != null)
				propertyListBuilder.Write(Resources.ShowPlanKeyPassthru, node.PassthruPredicate.GenerateSource());

			// We give a warning if a join has no predicate and no outer reference.
			// In this case the join would simply multiply the other side.
			// However, we supppress the warning if any side is known at produce at
			// most one row.
			if (node.Predicate == null &&
			    (node.OuterReferences == null || node.OuterReferences.Length == 0) &&
			    !AstUtil.WillProduceAtMostOneRow(node.Left) &&
			    !AstUtil.WillProduceAtMostOneRow(node.Right))
			{
				propertyListBuilder.Write(Resources.ShowPlanKeyWarning, Resources.ShowPlanWarningNoJoinPredicate);
			}

			IList<ShowPlanProperty> properties = propertyListBuilder.ToList();
			ShowPlanElement element = new ShowPlanElement(ShowPlanOperator.NestedLoops, properties, leftElement, rightElement);
			_currentElement = element;

			return node;
		}

		public override AlgebraNode VisitNullScanAlgebraNode(NullScanAlgebraNode node)
		{
			PropertyListBuilder propertyListBuilder = new PropertyListBuilder();
			propertyListBuilder.Write(Resources.ShowPlanKeyEmpty, Boolean.TrueString);
			AddRowBufferEntries(propertyListBuilder, Resources.ShowPlanGroupOutputList, node.OutputList);
			AddStatistics(propertyListBuilder, node.StatisticsIterator);

			IList<ShowPlanProperty> properties = propertyListBuilder.ToList();

			ShowPlanElement element = new ShowPlanElement(ShowPlanOperator.ConstantScan, properties);
			_currentElement = element;

			return node;
		}

		public override AlgebraNode VisitConstantScanAlgebraNode(ConstantScanAlgebraNode node)
		{
			PropertyListBuilder propertyListBuilder = new PropertyListBuilder();
			propertyListBuilder.Write(Resources.ShowPlanKeyEmpty, Boolean.FalseString);
			AddRowBufferEntries(propertyListBuilder, Resources.ShowPlanGroupOutputList, node.OutputList);
			AddStatistics(propertyListBuilder, node.StatisticsIterator);

			if (node.DefinedValues != null && node.DefinedValues.Length > 0)
				AddDefinedValues(propertyListBuilder, node.DefinedValues);

			IList<ShowPlanProperty> properties = propertyListBuilder.ToList();

			ShowPlanElement element = new ShowPlanElement(ShowPlanOperator.ConstantScan, properties);
			_currentElement = element;

			return node;
		}

		public override AlgebraNode VisitComputeScalarAlgebraNode(ComputeScalarAlgebraNode node)
		{
			ShowPlanElement inputElement = ConvertNode(node.Input);

			PropertyListBuilder propertyListBuilder = new PropertyListBuilder();
			AddRowBufferEntries(propertyListBuilder, Resources.ShowPlanGroupOutputList, node.OutputList);
			AddStatistics(propertyListBuilder, node.StatisticsIterator);
			AddDefinedValues(propertyListBuilder, node.DefinedValues);

			IList<ShowPlanProperty> properties = propertyListBuilder.ToList();

			ShowPlanElement element = new ShowPlanElement(ShowPlanOperator.ComputeScalar, properties, inputElement);
			_currentElement = element;

			return node;
		}

		public override AlgebraNode VisitConcatAlgebraNode(ConcatAlgebraNode node)
		{
			List<ShowPlanElement> inputList = new List<ShowPlanElement>();

			foreach (AlgebraNode inputNode in node.Inputs)
				inputList.Add(ConvertNode(inputNode));

			PropertyListBuilder propertyListBuilder = new PropertyListBuilder();
			AddRowBufferEntries(propertyListBuilder, Resources.ShowPlanGroupOutputList, node.OutputList);
			AddStatistics(propertyListBuilder, node.StatisticsIterator);
			AddDefinedValues(propertyListBuilder, node.DefinedValues);

			IList<ShowPlanProperty> properties = propertyListBuilder.ToList();

			ShowPlanElement element = new ShowPlanElement(ShowPlanOperator.Concatenation, properties, inputList.ToArray());
			_currentElement = element;

			return node;
		}

		public override AlgebraNode VisitSortAlgebraNode(SortAlgebraNode node)
		{
			ShowPlanElement inputElement = ConvertNode(node.Input);

			PropertyListBuilder propertyListBuilder = new PropertyListBuilder();
			AddRowBufferEntries(propertyListBuilder, Resources.ShowPlanGroupOutputList, node.OutputList);
			AddStatistics(propertyListBuilder, node.StatisticsIterator);

			if (node.Distinct)
				propertyListBuilder.Write(Resources.ShowPlanKeyLogicalOperator, Resources.ShowPlanLogicalOperatorDistinctSort);
			else
				propertyListBuilder.Write(Resources.ShowPlanKeyLogicalOperator, Resources.ShowPlanLogicalOperatorSort);

			propertyListBuilder.Begin(Resources.ShowPlanGroupOrderBy);
			for (int i = 0; i < node.SortEntries.Length; i++)
			{
				string sortOrder = SortOrderToString(node.SortOrders[i]);

				propertyListBuilder.Begin();
				propertyListBuilder.SetGroupValue(String.Format(CultureInfo.InvariantCulture, "{0} {1}", node.SortEntries[i].Name, sortOrder));
				WriteRowBufferEntry(propertyListBuilder, node.SortEntries[i]);
				propertyListBuilder.Write(Resources.ShowPlanKeyOrder, sortOrder);
				propertyListBuilder.End();
			}
			propertyListBuilder.End();

			IList<ShowPlanProperty> properties = propertyListBuilder.ToList();

			ShowPlanElement element = new ShowPlanElement(ShowPlanOperator.Sort, properties, inputElement);
			_currentElement = element;

			return node;
		}

		public override AlgebraNode VisitAggregateAlgebraNode(AggregateAlgebraNode node)
		{
			ShowPlanElement inputElement = ConvertNode(node.Input);

			PropertyListBuilder propertyListBuilder = new PropertyListBuilder();
			AddRowBufferEntries(propertyListBuilder, Resources.ShowPlanGroupOutputList, node.OutputList);
			AddStatistics(propertyListBuilder, node.StatisticsIterator);
			AddDefinedValues(propertyListBuilder, node.DefinedValues);

			if (node.Groups != null && node.Groups.Length > 0)
				AddRowBufferEntries(propertyListBuilder, Resources.ShowPlanGroupGroupBy, node.Groups);

			IList<ShowPlanProperty> properties = propertyListBuilder.ToList();

			ShowPlanElement element = new ShowPlanElement(ShowPlanOperator.StreamAggregate, properties, inputElement);
			_currentElement = element;

			return node;
		}

		public override AlgebraNode VisitTopAlgebraNode(TopAlgebraNode node)
		{
			ShowPlanElement inputElement = ConvertNode(node.Input);

			PropertyListBuilder propertyListBuilder = new PropertyListBuilder();
			AddRowBufferEntries(propertyListBuilder, Resources.ShowPlanGroupOutputList, node.OutputList);
			AddStatistics(propertyListBuilder, node.StatisticsIterator);

			propertyListBuilder.Write(Resources.ShowPlanKeyLimit, node.Limit.ToString(CultureInfo.InvariantCulture));

			if (node.TieEntries == null)
			{
				propertyListBuilder.Write(Resources.ShowPlanKeyWithTies, Boolean.FalseString);
			}
			else
			{
				propertyListBuilder.Write(Resources.ShowPlanKeyWithTies, Boolean.TrueString);
				AddRowBufferEntries(propertyListBuilder, Resources.ShowPlanGroupTieColumns, node.TieEntries);
			}

			IList<ShowPlanProperty> properties = propertyListBuilder.ToList();

			ShowPlanElement element = new ShowPlanElement(ShowPlanOperator.Top, properties, inputElement);
			_currentElement = element;

			return node;
		}

		public override AlgebraNode VisitFilterAlgebraNode(FilterAlgebraNode node)
		{
			ShowPlanElement inputElement = ConvertNode(node.Input);

			PropertyListBuilder propertyListBuilder = new PropertyListBuilder();
			AddRowBufferEntries(propertyListBuilder, Resources.ShowPlanGroupOutputList, node.OutputList);
			AddStatistics(propertyListBuilder, node.StatisticsIterator);

			propertyListBuilder.Write(Resources.ShowPlanKeyPredicate, node.Predicate.GenerateSource());

			IList<ShowPlanProperty> properties = propertyListBuilder.ToList();

			ShowPlanElement element = new ShowPlanElement(ShowPlanOperator.Filter, properties, inputElement);
			_currentElement = element;

			return node;
		}

		public override AlgebraNode VisitAssertAlgebraNode(AssertAlgebraNode node)
		{
			ShowPlanElement inputElement = ConvertNode(node.Input);

			PropertyListBuilder propertyListBuilder = new PropertyListBuilder();
			AddRowBufferEntries(propertyListBuilder, Resources.ShowPlanGroupOutputList, node.OutputList);
			AddStatistics(propertyListBuilder, node.StatisticsIterator);

			propertyListBuilder.Write(Resources.ShowPlanKeyPredicate, node.Predicate.GenerateSource());

			IList<ShowPlanProperty> properties = propertyListBuilder.ToList();

			ShowPlanElement element = new ShowPlanElement(ShowPlanOperator.Assert, properties, inputElement);
			_currentElement = element;

			return node;
		}

		public override AlgebraNode VisitIndexSpoolAlgebraNode(IndexSpoolAlgebraNode node)
		{
			ShowPlanElement inputElement = ConvertNode(node.Input);

			PropertyListBuilder propertyListBuilder = new PropertyListBuilder();
			AddRowBufferEntries(propertyListBuilder, Resources.ShowPlanGroupOutputList, node.OutputList);
			AddStatistics(propertyListBuilder, node.StatisticsIterator);
			propertyListBuilder.Write(Resources.ShowPlanKeyLogicalOperator, Resources.ShowPlanLogicalOperatorEagerSpool);
			propertyListBuilder.Write(Resources.ShowPlanKeyWithStack, Boolean.FalseString);

			WriteRowBufferEntry(propertyListBuilder, Resources.ShowPlanKeyIndex, node.IndexEntry);
			propertyListBuilder.Write(Resources.ShowPlanKeyProbe, node.ProbeExpression.GenerateSource());

			IList<ShowPlanProperty> properties = propertyListBuilder.ToList();

			ShowPlanElement element = new ShowPlanElement(ShowPlanOperator.IndexSpool, properties, inputElement);
			_currentElement = element;

			return node;
		}

		public override AstNode VisitStackedTableSpoolAlgebraNode(StackedTableSpoolAlgebraNode node)
		{
			ShowPlanElement inputElement = ConvertNode(node.Input);

			PropertyListBuilder propertyListBuilder = new PropertyListBuilder();
			AddRowBufferEntries(propertyListBuilder, Resources.ShowPlanGroupOutputList, node.OutputList);
			AddStatistics(propertyListBuilder, node.StatisticsIterator);
			propertyListBuilder.Write(Resources.ShowPlanKeyLogicalOperator, Resources.ShowPlanLogicalOperatorLazySpool);
			propertyListBuilder.Write(Resources.ShowPlanKeyWithStack, Boolean.TrueString);

			IList<ShowPlanProperty> properties = propertyListBuilder.ToList();

			ShowPlanElement element = new ShowPlanElement(ShowPlanOperator.IndexSpool, properties, inputElement);
			_currentElement = element;

			return node;
		}

		public override AstNode VisitTableSpoolRefAlgebraNode(StackedTableSpoolRefAlgebraNode node)
		{
			PropertyListBuilder propertyListBuilder = new PropertyListBuilder();
			AddRowBufferEntries(propertyListBuilder, Resources.ShowPlanGroupOutputList, node.OutputList);
			AddStatistics(propertyListBuilder, node.StatisticsIterator);
			propertyListBuilder.Write(Resources.ShowPlanKeyLogicalOperator, Resources.ShowPlanLogicalOperatorLazySpool);
			propertyListBuilder.Write(Resources.ShowPlanKeyWithStack, Boolean.TrueString);

			IList<ShowPlanProperty> properties = propertyListBuilder.ToList();

			ShowPlanElement element = new ShowPlanElement(ShowPlanOperator.TableSpool, properties);
			_currentElement = element;

			return node;
		}

		public override AlgebraNode VisitHashMatchAlgebraNode(HashMatchAlgebraNode node)
		{
			ShowPlanElement buildElement = ConvertNode(node.Left);
			ShowPlanElement probeElement = ConvertNode(node.Right);

			PropertyListBuilder propertyListBuilder = new PropertyListBuilder();
			AddRowBufferEntries(propertyListBuilder, Resources.ShowPlanGroupOutputList, node.OutputList);
			AddStatistics(propertyListBuilder, node.StatisticsIterator);

			WriteRowBufferEntry(propertyListBuilder, Resources.ShowPlanKeyHashKeysBuild, node.BuildKeyEntry);
			WriteRowBufferEntry(propertyListBuilder, Resources.ShowPlanKeyHashKeysProbe, node.ProbeEntry);
			propertyListBuilder.Write(Resources.ShowPlanKeyLogicalOperator, LogicalOperatorToString(node.Op));

			if (node.ProbeResidual != null)
				propertyListBuilder.Write(Resources.ShowPlanKeyProbeResidual, node.ProbeResidual.GenerateSource());

			IList<ShowPlanProperty> properties = propertyListBuilder.ToList();

			ShowPlanElement element = new ShowPlanElement(ShowPlanOperator.HashMatch, properties, buildElement, probeElement);
			_currentElement = element;

			return node;
		}
	}
}