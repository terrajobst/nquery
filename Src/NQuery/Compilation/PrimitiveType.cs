namespace NQuery.Compilation
{
	internal enum PrimitiveType
	{
		None,
		Boolean,
		Byte,
		SByte,
		Char,
		Int16,
		UInt16,
		Int32,
		UInt32,
		Int64,
		UInt64,
		Single,
		Double,
		Decimal,   // Not a primitive from CLR perspective
		String,    // Not a primitive from CLR perspective
		Object,    // Not a primitive from CLR perspective
		Null,      // Not a primitive from CLR perspective
	}
}