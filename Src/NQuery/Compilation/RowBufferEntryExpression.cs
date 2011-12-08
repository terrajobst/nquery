using System;
using System.Collections.Generic;

namespace NQuery.Compilation
{
	internal sealed class RowBufferEntryExpression : ExpressionNode
	{
		private RowBufferEntry _rowBufferEntry;
		private object[] _rowBuffer;
		private int _rowBufferIndex;

		public RowBufferEntryExpression()
		{
		}

		public RowBufferEntryExpression(RowBufferEntry rowBufferEntry)
		{
			_rowBufferEntry = rowBufferEntry;
		}

		public RowBufferEntry RowBufferEntry
		{
			get { return _rowBufferEntry; }
			set { _rowBufferEntry = value; }
		}

		public object[] RowBuffer
		{
			get { return _rowBuffer; }
			set { _rowBuffer = value; }
		}

		public int RowBufferIndex
		{
			get { return _rowBufferIndex; }
			set { _rowBufferIndex = value; }
		}

		public override Type ExpressionType
		{
			get { return _rowBufferEntry.DataType; }
		}

		public override object GetValue()
		{
			return _rowBuffer[_rowBufferIndex];
		}

		public override AstNodeType NodeType
		{
			get { return AstNodeType.RowBufferEntryExpression; }
		}

		public override AstElement Clone(Dictionary<AstElement, AstElement> alreadyClonedElements)
		{
			RowBufferEntryExpression result = new RowBufferEntryExpression();
			result.RowBufferEntry = _rowBufferEntry;
			return result;
		}
	}
}