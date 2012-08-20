using System;
using System.Collections.Generic;
using System.Reflection;

using NQuery.Compilation;

namespace NQuery.Runtime
{
	internal static class BuiltInConversions
	{
		private static object _lock = new object();
		private static bool _conversionsLoaded;
        private static Dictionary<ConversionKey, MethodInfo> _implicitConversionsTable = new Dictionary<ConversionKey, MethodInfo>();
        private static Dictionary<ConversionKey, MethodInfo> _explicitConversionsTable = new Dictionary<ConversionKey, MethodInfo>();

		private static void EnsureTablesLoaded()
		{
			if (!_conversionsLoaded)
			{
				lock (_lock)
				{
					if (!_conversionsLoaded)
					{
						LoadConversionTable(_implicitConversionsTable, typeof(ImplicitConversions));
						LoadConversionTable(_explicitConversionsTable, typeof(ExplicitConversions));
						_conversionsLoaded = true;
					}					
				}
			}
		}

		private static void LoadConversionTable(IDictionary<ConversionKey, MethodInfo> table, Type type)
		{
			BindingFlags bindingFlags = BindingFlags.Public | 
			                            BindingFlags.Static | 
			                            BindingFlags.DeclaredOnly;

			MethodInfo[] methods = type.GetMethods(bindingFlags);
			Array.Sort(methods, (x, y) => String.Compare(x.ToString(), y.ToString(), StringComparison.Ordinal));

			foreach (MethodInfo methodInfo in methods)
			{
				ConversionKey key = new ConversionKey();
				key.SourceType = methodInfo.GetParameters()[0].ParameterType;
				key.TargetType = methodInfo.ReturnType;
				table.Add(key, methodInfo);
			}
		}

		public static MethodInfo GetConversion(CastingOperatorType castingOperatorType, Type sourceType, Type targetType)
		{
			EnsureTablesLoaded();

            Dictionary<ConversionKey, MethodInfo> conversionTable;
			switch (castingOperatorType)
			{
				case CastingOperatorType.Implicit:
					conversionTable = _implicitConversionsTable;
					break;
				case CastingOperatorType.Explicit:
					conversionTable = _explicitConversionsTable;
					break;
				default:
					throw ExceptionBuilder.UnhandledCaseLabel(castingOperatorType);
			}
			
			ConversionKey key = new ConversionKey();
			key.SourceType = sourceType;
			key.TargetType = targetType;

            MethodInfo result;
            if (conversionTable.TryGetValue(key, out result))
                return result;
		    
			return null;
		}
				
		private struct ConversionKey
		{
			public Type SourceType;
			public Type TargetType;

			public override int GetHashCode()
			{
				return SourceType.GetHashCode() + 29*TargetType.GetHashCode();
			}

			public override bool Equals(object obj)
			{
				if (!(obj is ConversionKey))
					return false;
				
				ConversionKey conversionKey = (ConversionKey) obj;
				
				if (!Equals(SourceType, conversionKey.SourceType))
					return false;
				
				if (!Equals(TargetType, conversionKey.TargetType))
					return false;
				
				return true;
			}

			public static bool operator==(ConversionKey left, ConversionKey right)
			{
				return left.Equals(right);
			}

			public static bool operator !=(ConversionKey left, ConversionKey right)
			{
				return !left.Equals(right);
			}
		}

		internal class ImplicitConversions
		{
			#region FromSByte
				   	
			public static Int16 FromSByteToInt16(SByte value)
			{
				return value;
			}

			public static Int32 FromSByteToInt(SByte value)
			{
				return value;
			}

			public static Int64 FromSByteToLong(SByte value)
			{
				return value;
			}
			public static Single FromSByteToSingle(SByte value)
			{
				return value;
			}

			public static Double FromSByteToDouble(SByte value)
			{
				return value;
			}

			public static Decimal FromSByteToDecimal(SByte value)
			{
				return value;
			}
				   	
			#endregion
				   	
			#region FromByte
			
			public static Int16 FromByteToInt16(Byte value)
			{
				return value;
			}
			
			public static UInt16 FromByteToUInt16(Byte value)
			{
				return value;
			}
			
			public static Int32 FromByteToInt32(Byte value)
			{
				return value;
			}
			
			public static UInt32 FromByteToUInt32(Byte value)
			{
				return value;
			}
			
			public static Int64 FromByteToInt64(Byte value)
			{
				return value;
			}
			
			public static UInt64 FromByteToUInt64(Byte value)
			{
				return value;
			}
			
			public static Single FromByteToSingle(Byte value)
			{
				return value;
			}
			
			public static Double FromByteToDouble(Byte value)
			{
				return value;
			}
			
			public static Decimal FromByteToDecimal(Byte value)
			{
				return value;
			}
			
			#endregion
				   	
			#region FromInt16
			
			public static Int32 FromInt16ToInt32(Int16 value)
			{
				return value;
			}
			
			public static Int64 FromInt16ToInt64(Int16 value)
			{
				return value;
			}
			
			public static Single FromInt16ToSingle(Int16 value)
			{
				return value;
			}
			
			public static Double FromInt16ToDouble(Int16 value)
			{
				return value;
			}
			
			public static Decimal FromInt16ToDecimal(Int16 value)
			{
				return value;
			}
			
			#endregion
				   	
			#region FromUInt16
			
			public static Int32 FromUInt16ToInt32(UInt16 value)
			{
				return value;
			}
			
			public static UInt32 FromUInt16ToUInt32(UInt16 value)
			{
				return value;
			}
			
			public static Int64 FromUInt16ToInt64(UInt16 value)
			{
				return value;
			}
			
			public static UInt64 FromUInt16ToUInt64(UInt16 value)
			{
				return value;
			}
			
			public static Single FromUInt16ToSingle(UInt16 value)
			{
				return value;
			}
			
			public static Double FromUInt16ToDouble(UInt16 value)
			{
				return value;
			}
			
			public static Decimal FromUInt16ToDecimal(UInt16 value)
			{
				return value;
			}
			
			#endregion
				   	
			#region FromInt32
			
			public static Int64 FromInt32ToInt64(Int32 value)
			{
				return value;
			}
			
			public static Single FromInt32ToSingle(Int32 value)
			{
				return value;
			}
			
			public static Double FromInt32ToDouble(Int32 value)
			{
				return value;
			}
			
			public static Decimal FromInt32ToDecimal(Int32 value)
			{
				return value;
			}  
			
			#endregion
				   	
			#region FromUInt32
			
			public static Int64 FromUInt32ToInt64(UInt32 value)
			{
				return value;
			}
			
			public static UInt64 FromUInt32ToUInt64(UInt32 value)
			{
				return value;
			}
			
			public static Single FromUInt32ToSingle(UInt32 value)
			{
				return value;
			}
			
			public static Double FromUInt32ToDouble(UInt32 value)
			{
				return value;
			}
			
			public static Decimal FromUInt32ToDecimal(UInt32 value)
			{
				return value;
			}
			
			#endregion
				   	
			#region FromInt64
			
			public static Single FromInt64ToSingle(Int64 value)
			{
				return value;
			}
			
			public static Double FromInt64ToDouble(Int64 value)
			{
				return value;
			}
			
			public static Decimal FromInt64ToDecimal(Int64 value)
			{
				return value;
			}

			#endregion
				   	
			#region FromUInt64
			
			public static Single FromUInt64ToSingle(UInt64 value)
			{
				return value;
			}
			
			public static Double FromUInt64ToDouble(UInt64 value)
			{
				return value;
			}

			public static Decimal FromUInt64ToDecimal(UInt64 value)
			{
				return value;
			}
			
			#endregion
			
			#region FromChar
			
			public static UInt16 FromCharToUInt16(Char value)
			{
				return value;
			}
			
			public static Int32 FromCharToInt32(Char value)
			{
				return value;
			}
			
			public static UInt32 FromCharToUInt32(Char value)
			{
				return value;
			}
			
			public static Int64 FromCharToInt64(Char value)
			{
				return value;
			}
			
			public static UInt64 FromCharToUInt64(Char value)
			{
				return value;
			}
			
			public static Single FromCharToSingle(Char value)
			{
				return value;
			}
			
			public static Double FromCharToDouble(Char value)
			{
				return value;
			}
			
			public static Decimal FromCharToDecimal(Char value)
			{
				return value;
			}
			
			#endregion
			
			#region FromSingle
			
			public static Double FromSingleToDouble(Single value)
			{
				return value;
			}

			#endregion
		}

		internal class ExplicitConversions
		{
			#region FromSByte
			
			public static Byte FromSByteToByte(SByte value)
			{
				return (Byte) value;
			}
			
			public static UInt16 FromSByteToUInt16(SByte value)
			{
				return (UInt16) value;
			}
			
			public static UInt32 FromSByteToUInt32(SByte value)
			{
				return (UInt32) value;
			}
			
			public static UInt64 FromSByteToUInt64(SByte value)
			{
				return (UInt64) value;
			}
			
			public static Char FromSByteToChar(SByte value)
			{
				return (Char) value;
			}

			#endregion
			
			#region FromByte
			
			public static SByte FromByteToSByte(Byte value)
			{
				return (SByte) value;
			}

			public static Char FromByteToChar(Byte value)
			{
				return (Char) value;
			}

			#endregion

			#region FromInt16
			
			public static SByte FromInt16ToSByte(Int16 value)
			{
				return (SByte) value;
			}
			
			public static Byte FromInt16ToByte(Int16 value)
			{
				return (Byte) value;
			}
			
			public static UInt16 FromInt16ToUInt16(Int16 value)
			{
				return (UInt16) value;
			}
			
			public static UInt32 FromInt16ToUInt32(Int16 value)
			{
				return (UInt32) value;
			}
			
			public static UInt64 FromInt16ToUInt64(Int16 value)
			{
				return (UInt64) value;
			}
			
			public static Char FromInt16ToChar(Int16 value)
			{
				return (Char) value;
			}

			#endregion

			#region FromUInt16
			
			public static SByte FromUInt16ToSByte(UInt16 value)
			{
				return (SByte) value;
			}
			
			public static Byte FromUInt16ToByte(UInt16 value)
			{
				return (Byte) value;
			}
			
			public static Int16 FromUInt16ToInt16(UInt16 value)
			{
				return (Int16) value;
			}
			
			public static Char FromUInt16ToChar(UInt16 value)
			{
				return (Char) value;
			}

			#endregion

			#region FromInt32
			
			public static SByte FromInt32ToSByte(Int32 value)
			{
				return (SByte) value;
			}
			
			public static Byte FromInt32ToByte(Int32 value)
			{
				return (Byte) value;
			}
			
			public static Int16 FromInt32ToInt16(Int32 value)
			{
				return (Int16) value;
			}
			
			public static UInt16 FromInt32ToUInt16(Int32 value)
			{
				return (UInt16) value;
			}
			
			public static UInt32 FromInt32ToUInt32(Int32 value)
			{
				return (UInt32) value;
			}
			
			public static UInt64 FromInt32ToUInt64(Int32 value)
			{
				return (UInt64) value;
			}
			
			public static Char FromInt32ToChar(Int32 value)
			{
				return (Char) value;
			}

			#endregion

			#region FromUInt32
			
			public static SByte FromUInt32ToSByte(UInt32 value)
			{
				return (SByte) value;
			}

			public static Byte FromUInt32ToByte(UInt32 value)
			{
				return (Byte) value;
			}

			public static Int16 FromUInt32ToInt16(UInt32 value)
			{
				return (Int16) value;
			}

			public static UInt16 FromUInt32ToUInt16(UInt32 value)
			{
				return (UInt16) value;
			}

			public static Int32 FromUInt32ToInt32(UInt32 value)
			{
				return (Int32) value;
			}

			public static Char FromUInt32ToChar(UInt32 value)
			{
				return (Char) value;
			}


			#endregion

			#region FromInt64
			
			public static SByte FromInt64ToSByte(Int64 value)
			{
				return (SByte) value;
			}

			public static Byte FromInt64ToByte(Int64 value)
			{
				return (Byte) value;
			}

			public static Int16 FromInt64ToInt16(Int64 value)
			{
				return (Int16) value;
			}

			public static UInt16 FromInt64ToUInt16(Int64 value)
			{
				return (UInt16) value;
			}

			public static Int32 FromInt64ToInt32(Int64 value)
			{
				return (Int32) value;
			}

			public static UInt32 FromInt64ToUInt32(Int64 value)
			{
				return (UInt32) value;
			}

			public static UInt64 FromInt64ToUInt64(Int64 value)
			{
				return (UInt64) value;
			}

			public static Char FromInt64ToChar(Int64 value)
			{
				return (Char) value;
			}

			#endregion

			#region FromUInt64
			
			public static SByte FromUInt64ToSByte(UInt64 value)
			{
				return (SByte) value;
			}

			public static Byte FromUInt64ToByte(UInt64 value)
			{
				return (Byte) value;
			}

			public static Int16 FromUInt64ToInt16(UInt64 value)
			{
				return (Int16) value;
			}

			public static UInt16 FromUInt64ToUInt16(UInt64 value)
			{
				return (UInt16) value;
			}

			public static Int32 FromUInt64ToInt32(UInt64 value)
			{
				return (Int32) value;
			}

			public static UInt32 FromUInt64ToUInt32(UInt64 value)
			{
				return (UInt32) value;
			}

			public static Int64 FromUInt64ToInt64(UInt64 value)
			{
				return (Int64) value;
			}

			public static Char FromUInt64ToChar(UInt64 value)
			{
				return (Char) value;
			}

			#endregion

			#region FromChar
			
			public static SByte FromCharToSByte(Char value)
			{
				return (SByte) value;
			}

			public static Byte FromCharToByte(Char value)
			{
				return (Byte) value;
			}

			public static Int16 FromCharToInt16(Char value)
			{
				return (Int16) value;
			}

			#endregion

			#region FromSingle
			public static SByte FromSingleToSByte(Single value)
			{
				return (SByte) value;
			}

			public static Byte FromSingleToByte(Single value)
			{
				return (Byte) value;
			}

			public static Int16 FromSingleToInt16(Single value)
			{
				return (Int16) value;
			}

			public static UInt16 FromSingleToUInt16(Single value)
			{
				return (UInt16) value;
			}

			public static Int32 FromSingleToInt32(Single value)
			{
				return (Int32) value;
			}

			public static UInt32 FromSingleToUInt32(Single value)
			{
				return (UInt32) value;
			}

			public static Int64 FromSingleToInt64(Single value)
			{
				return (Int64) value;
			}

			public static UInt64 FromSingleToUInt64(Single value)
			{
				return (UInt64) value;
			}

			public static Char FromSingleToChar(Single value)
			{
				return (Char) value;
			}

			public static Decimal FromSingleToDecimal(Single value)
			{
				return (Decimal) value;
			}

			#endregion

			#region FromDouble
			
			public static SByte FromDoubleToSByte(Double value)
			{
				return (SByte) value;
			}

			public static Byte FromDoubleToByte(Double value)
			{
				return (Byte) value;
			}

			public static Int16 FromDoubleToInt16(Double value)
			{
				return (Int16) value;
			}

			public static UInt16 FromDoubleToUInt16(Double value)
			{
				return (UInt16) value;
			}

			public static Int32 FromDoubleToInt32(Double value)
			{
				return (Int32) value;
			}

			public static UInt32 FromDoubleToUInt32(Double value)
			{
				return (UInt32) value;
			}

			public static Int64 FromDoubleToInt64(Double value)
			{
				return (Int64) value;
			}

			public static UInt64 FromDoubleToUInt64(Double value)
			{
				return (UInt64) value;
			}

			public static Char FromDoubleToChar(Double value)
			{
				return (Char) value;
			}

			public static Single FromDoubleToSingle(Double value)
			{
				return (Single) value;
			}

			public static Decimal FromDoubleToDecimal(Double value)
			{
				return (Decimal) value;
			}

			#endregion

			#region FromDecimal
			public static SByte FromDecimalToSByte(Decimal value)
			{
				return (SByte) value;
			}

			public static Byte FromDecimalToByte(Decimal value)
			{
				return (Byte) value;
			}

			public static Int16 FromDecimalToInt16(Decimal value)
			{
				return (Int16) value;
			}

			public static UInt16 FromDecimalToUInt16(Decimal value)
			{
				return (UInt16) value;
			}

			public static Int32 FromDecimalToInt32(Decimal value)
			{
				return (Int32) value;
			}

			public static UInt32 FromDecimalToUInt32(Decimal value)
			{
				return (UInt32) value;
			}

			public static Int64 FromDecimalToInt64(Decimal value)
			{
				return (Int64) value;
			}

			public static UInt64 FromDecimalToUInt64(Decimal value)
			{
				return (UInt64) value;
			}

			public static Char FromDecimalToChar(Decimal value)
			{
				return (Char) value;
			}

			public static Single FromDecimalToSingle(Decimal value)
			{
				return (Single) value;
			}

			public static Double FromDecimalToDouble(Decimal value)
			{
				return (Double) value;
			}

			#endregion
		}
	}
}