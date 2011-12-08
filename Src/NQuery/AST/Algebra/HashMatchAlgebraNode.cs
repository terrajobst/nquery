using System;
using System.Collections.Generic;

namespace NQuery.Compilation
{
	internal sealed class HashMatchAlgebraNode : AlgebraNode
	{
		private AlgebraNode _left;
		private AlgebraNode _right;
		private JoinAlgebraNode.JoinOperator _op;
		private RowBufferEntry _buildKeyEntry;
		private RowBufferEntry _probeEntry;
		private ExpressionNode _probeResidual;

		public AlgebraNode Left
		{
			get { return _left; }
			set { _left = value; }
		}

		public AlgebraNode Right
		{
			get { return _right; }
			set { _right = value; }
		}

		public JoinAlgebraNode.JoinOperator Op
		{
			get { return _op; }
			set { _op = value; }
		}

		public RowBufferEntry BuildKeyEntry
		{
			get { return _buildKeyEntry; }
			set { _buildKeyEntry = value; }
		}

		public RowBufferEntry ProbeEntry
		{
			get { return _probeEntry; }
			set { _probeEntry = value; }
		}

		public ExpressionNode ProbeResidual
		{
			get { return _probeResidual; }
			set { _probeResidual = value; }
		}

		public override AstNodeType NodeType
		{
			get { return AstNodeType.HashMatchAlgebraNode; }
		}

		public override AstElement Clone(Dictionary<AstElement, AstElement> alreadyClonedElements)
		{
			HashMatchAlgebraNode result = new HashMatchAlgebraNode();
			result.StatisticsIterator = StatisticsIterator;
			result.OutputList = ArrayHelpers.Clone(OutputList);
			result.Left = (AlgebraNode)_left.Clone(alreadyClonedElements);
			result.Right = (AlgebraNode)_right.Clone(alreadyClonedElements);
			result.Op = _op;
			result.BuildKeyEntry = _buildKeyEntry;
			result.ProbeEntry = _probeEntry;
			result.ProbeResidual = (ExpressionNode)_probeResidual.Clone(alreadyClonedElements);
			return result;
		}
	}
}