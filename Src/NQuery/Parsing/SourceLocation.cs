using System;
using System.Diagnostics;
using System.Globalization;

namespace NQuery
{
	/// <summary>
	/// Represents a location in the source code. Due to the fact that line and column are stored together on 32 bit the
	/// maximum line is limited to 2^21 (2,097,152) whereby the maximum column is limited to 2^11 (2,048). That should not
	/// be a hard restriction since the same limits apply to the C# compiler.
	/// </summary>
	/// <remarks>
	/// Both <see cref="Line"/> and <see cref="Column"/> are zero based.
	/// </remarks>
	[Serializable]
	[DebuggerDisplay("Ln {Line}; Col {Column}")]
	public struct SourceLocation : IEquatable<SourceLocation>, IComparable<SourceLocation>, IComparable
	{
		/// <summary>
		/// Maximum column index.
		/// </summary>
		public static readonly int MaxColumn = (2 << 10) - 1; // 11 bit for col - 1 (= 2,047)

		/// <summary>
		/// Maximum line index.
		/// </summary>
		public static readonly int MaxLine = (2 << 20) - 1; // 21 bit for line - 1 (= 2,097,151)

		private const int COLUMN_MASK = 0x000007FF;
		private const int LINE_MASK = unchecked (~COLUMN_MASK);

		private int _pos;

		/// <summary>
		/// Creates a new <c>SourceLocation</c> with the specified 32 bit value representing the line/col information. The line
		/// index is stored in the higher 21 bit whereby the column index is stored in the lower 11 bit.
		/// </summary>
		/// <param name="pos">Column and line index stored in one <see cref="Int32"/></param>
		private SourceLocation(int pos)
		{
			_pos = pos;
		}

		/// <summary>
		/// Creates a new <c>SourceLocation</c> with the specified column and line.
		/// </summary>
		/// <param name="column">Column index (must not exceed 2,048)</param>
		/// <param name="line">Line index (must not exceed 2,097,152)</param>
		public SourceLocation(int column, int line)
		{
			if (column < 0 || column > MaxColumn)
				throw ExceptionBuilder.ArgumentOutOfRange("column", column, 0, MaxColumn);

			if (line < 0 || line > MaxLine)
				throw ExceptionBuilder.ArgumentOutOfRange("line", line, 0, MaxLine);

			_pos = column;
			_pos |= line << 11;
		}

		/// <summary>
		/// Represents a source pos that is not legal.
		/// </summary>
		public static readonly SourceLocation None = new SourceLocation(int.MaxValue);

		/// <summary>
		/// Represents an empty source location i.e. col = 0, line = 0.
		/// </summary>
		public static readonly SourceLocation Empty = new SourceLocation();

		/// <summary>
		/// Represents the smallest possible value of a <see cref="SourceLocation"/>.
		/// </summary>
		public static readonly SourceLocation MinValue = new SourceLocation(0, 0);

		/// <summary>
		/// Represents the largest possible value of a <see cref="SourceLocation"/>.
		/// </summary>
		public static readonly SourceLocation MaxValue = new SourceLocation(MaxColumn, MaxLine);

		/// <summary>
		/// Gets or sets the column index for this source location (zero based). Must not exceed 2,048.
		/// </summary>
		public int Column
		{
			get
			{
				// return col part

				return _pos & COLUMN_MASK;
			}

			set
			{
				if (value < 0 || value > MaxColumn)
					throw ExceptionBuilder.ArgumentOutOfRange("value", value, 0, MaxColumn);

				// clear col part

				_pos &= LINE_MASK;

				// assign col part

				_pos |= value;
			}
		}

		/// <summary>
		/// Gets or sets the line index for this source location (zero based). Must not exceed 2,097,152.
		/// </summary>
		public int Line
		{
			get
			{
				// Return higher 21 bit
				//
				// NOTE: The conversion from int -> uint -> int is needed to avoid the first bit being interpreted
				// as negation bit.

				return (int) (((uint) _pos) >> 11);
			}

			set
			{
				if (value < 0 || value > MaxLine)
					throw ExceptionBuilder.ArgumentOutOfRange("value", value, 0, MaxLine);

				// clear higher 21 bit

				_pos &= COLUMN_MASK;

				// assign line part

				_pos |= (value << 11);
			}
		}

		public override int GetHashCode()
		{
			return _pos.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj is SourceLocation)
				return this == (SourceLocation) obj;
			else
				return false;
		}

		public bool Equals(SourceLocation other)
		{
			return this == other;
		}

		public bool IsBefore(SourceLocation location)
		{
			return this < location;
		}

		public bool IsBeforeOrEqual(SourceLocation location)
		{
			return this <= location;
		}

		public bool IsAfter(SourceLocation location)
		{
			return this > location;
		}

		public bool IsAfterOrEqual(SourceLocation location)
		{
			return this >= location;
		}

		public SourceLocation Increment()
		{
			return this++;
		}

		public SourceLocation Decrement()
		{
			return this--;
		}

		public static SourceLocation operator ++(SourceLocation sourceLocation)
		{
			return new SourceLocation(sourceLocation.Column + 1, sourceLocation.Line);
		}

		public static SourceLocation operator --(SourceLocation sourceLocation)
		{
			if (sourceLocation.Column > 0)
				return new SourceLocation(sourceLocation.Column - 1, sourceLocation.Line);

			if (sourceLocation.Column == 0 && sourceLocation.Line > 0)
				return new SourceLocation(MaxColumn, sourceLocation.Line - 1);

			return Empty;
		}

		public static bool operator ==(SourceLocation left, SourceLocation right)
		{
			return left.Line == right.Line && left.Column == right.Column;
		}

		public static bool operator !=(SourceLocation left, SourceLocation right)
		{
			return left.Line != right.Line || left.Column != right.Column;
		}

		public static bool operator <=(SourceLocation left, SourceLocation right)
		{
			return left.Line < right.Line || left.Line == right.Line && left.Column <= right.Column;
		}

		public static bool operator >=(SourceLocation left, SourceLocation right)
		{
			return left.Line > right.Line || left.Line == right.Line && left.Column >= right.Column;
		}

		public static bool operator <(SourceLocation left, SourceLocation right)
		{
			return left.Line < right.Line || left.Line == right.Line && left.Column < right.Column;
		}

		public static bool operator >(SourceLocation left, SourceLocation right)
		{
			return left.Line > right.Line || left.Line == right.Line && left.Column > right.Column;
		}

		public int CompareTo(SourceLocation other)
		{
			if (this < other)
				return -1;

			if (this > other)
				return 1;

			return 0;
		}

		public int CompareTo(object obj)
		{
			if (obj == null)
				return 1;

			if (!(obj is SourceLocation))
				throw ExceptionBuilder.ArgumentMustBeOfType("obj", typeof(SourceLocation));

			SourceLocation location = (SourceLocation) obj;

			return CompareTo(location);
		}

		public override string ToString()
		{
			return String.Format(CultureInfo.InvariantCulture, "Ln {0}; Col {1}", Line, Column);
		}
	}
}