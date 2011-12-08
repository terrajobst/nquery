using System;
using System.Diagnostics;
using System.Globalization;

namespace NQuery
{
	/// <summary>
	/// Represents a range in the source code.
	/// </summary>
	[Serializable]
	[DebuggerDisplay("(Ln {StartLine}; Col {StartCol}) - (Ln {EndLine}; Col {EndCol})")]
	public struct SourceRange : IEquatable<SourceRange>
	{
		private SourceLocation _startLocation;
		private SourceLocation _endLocation;

		/// <summary>
		/// Creates a new source range spawning from the given start location to the given end location.
		/// </summary>
		/// <param name="startLocation">postion to start from</param>
		/// <param name="endLocation">Location to end</param>
		public SourceRange(SourceLocation startLocation, SourceLocation endLocation)
		{
			_startLocation = startLocation;
			_endLocation = endLocation;
		}

		/// <summary>
		/// Creates a new source range spawning from the given start location to the given end location.
		/// </summary>
		/// <param name="startCol">Column index to start from</param>
		/// <param name="startLine">Line index to start from</param>
		/// <param name="endCol">Column index to end</param>
		/// <param name="endLine">Line index to end</param>
		public SourceRange(int startCol, int startLine, int endCol, int endLine)
		{
			_startLocation = new SourceLocation(startCol, startLine);
			_endLocation = new SourceLocation(endCol, endLine);
		}

		/// <summary>
		/// Creates a new source range where start and end location refer to the same location.
		/// </summary>
		/// <param name="sourceLocation">Location to which <see cref="StartLocation"/> and <see cref="EndLocation"/> should refer</param>
		public SourceRange(SourceLocation sourceLocation)
		{
			_startLocation = sourceLocation;
			_endLocation = _startLocation;
		}

		/// <summary>
		/// Creates a new source range where start and end location refer to the same location.
		/// </summary>
		/// <param name="col">Column index for <see cref="StartLocation"/> and <see cref="EndLocation"/></param>
		/// <param name="line">Line index for <see cref="StartLocation"/> and <see cref="EndLocation"/></param>
		public SourceRange(int col, int line)
		{
			_startLocation = new SourceLocation(col, line);
			_endLocation = _startLocation;
		}

		/// <summary>
		/// Represents a source range that is not legal.
		/// </summary>
		public static readonly SourceRange None = new SourceRange(SourceLocation.None, SourceLocation.None);

		/// <summary>
		/// Represents an empty source range i.e. StartPos = (0/0), EndPos(0/0)
		/// </summary>
		public static readonly SourceRange Empty = new SourceRange(SourceLocation.Empty, SourceLocation.Empty);

		/// <summary>
		/// Represents an infinite range.
		/// </summary>
		public static readonly SourceRange Infinite = new SourceRange(SourceLocation.MinValue, SourceLocation.MaxValue);

		/// <summary>
		/// Gets or sets the start location for this range.
		/// </summary>
		public SourceLocation StartLocation
		{
			get { return _startLocation; }
			set { _startLocation = value; }
		}

		/// <summary>
		/// Gets or sets the end location for this range.
		/// </summary>
		public SourceLocation EndLocation
		{
			get { return _endLocation; }
			set { _endLocation = value; }
		}

		/// <summary>
		/// Gets or sets the column of the start location.
		/// </summary>
		public int StartColumn
		{
			get { return _startLocation.Column; }
			set { _startLocation.Column = value; }
		}

		/// <summary>
		/// Gets or sets the line of the start location.
		/// </summary>
		public int StartLine
		{
			get { return _startLocation.Line; }
			set { _startLocation.Line = value; }
		}

		/// <summary>
		/// Gets or sets the column of the end location.
		/// </summary>
		public int EndColumn
		{
			get { return _endLocation.Column; }
			set { _endLocation.Column = value; }
		}

		/// <summary>
		/// Gets or sets the line of the end location.
		/// </summary>
		public int EndLine
		{
			get { return _endLocation.Line; }
			set { _endLocation.Line = value; }
		}

		/// <summary>
		/// Normalizes this source range. For normalized ranges <see cref="StartLocation"/> is guaranteed to be before <see cref="EndLocation"/>.
		/// </summary>
		public void Normalize()
		{
			// swap if _endPos is before _startPos

			if (_endLocation < _startLocation)
			{
				SourceLocation temp = _startLocation;
				_startLocation = _endLocation;
				_endLocation = temp;
			}
		}

		/// <summary>
		/// Checks if this source range contains the given source location. The <see cref="StartLocation"/> and <see cref="EndLocation"/> are
		/// both considered to be part of the range.
		/// </summary>
		/// <param name="location">the location to check</param>
		public bool Contains(SourceLocation location)
		{
			return _startLocation <= location && location <= _endLocation;
		}

		/// <summary>
		/// Checks if this source range fully contains another given source range.
		/// </summary>
		/// <param name="range">range to check</param>
		public bool Contains(SourceRange range)
		{
			return Contains(range.StartLocation) && Contains(range.EndLocation);
		}

		public override int GetHashCode()
		{
			long value = _startLocation.GetHashCode() | (_endLocation.GetHashCode() << 32);
			return value.GetHashCode();
		}

		public bool Equals(SourceRange other)
		{
			return this == other;
		}

		public override bool Equals(object obj)
		{
			if (obj is SourceRange)
				return this == (SourceRange) obj;
			else
				return false;
		}

		public static bool operator ==(SourceRange left, SourceRange right)
		{
			return left.StartLocation == right.StartLocation && left.EndLocation == right.EndLocation;
		}

		public static bool operator !=(SourceRange left, SourceRange right)
		{
			return left.StartLocation != right.StartLocation || left.EndLocation != right.EndLocation;
		}
    
	    public override string ToString()
        {
            return String.Format(CultureInfo.InvariantCulture, "{0} - {1}", _startLocation, _endLocation);
        }
	}
}