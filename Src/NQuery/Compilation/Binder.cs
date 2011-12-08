using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;

using NQuery.Runtime;

namespace NQuery.Compilation
{
	internal sealed class Binder 
	{
		private IErrorReporter _errorReporter;
		private static readonly Dictionary<string, Type> _typeShortCutTable = CreateShortCutTable();

		private static readonly MethodInfo _unaryOperatorPlaceholder = GetSpecialBuiltInOperator("Placeholder", typeof(DBNull));
		private static readonly MethodInfo _binaryOperatorPlaceholder = GetSpecialBuiltInOperator("Placeholder", typeof(DBNull), typeof(DBNull));
		private static readonly MethodInfo _generalEqualityOperator = GetSpecialBuiltInOperator("op_Equality", typeof(object), typeof(object));
		private static readonly MethodInfo _generalInequalityOperator = GetSpecialBuiltInOperator("op_Inequality", typeof(object), typeof(object));
		private static readonly MethodInfo _enumBitAndOperator = GetSpecialBuiltInOperator("op_BitwiseAnd", typeof(Enum), typeof(Enum));
		private static readonly MethodInfo _enumBitOrOperator = GetSpecialBuiltInOperator("op_BitwiseOr", typeof(Enum), typeof(Enum));

		public Binder(IErrorReporter errorReporter) 
		{
			_errorReporter = errorReporter;
		}

		#region Static Initializers

		private static Dictionary<string, Type> CreateShortCutTable()
		{
			Dictionary<string, Type> result = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
			result.Add("BYTE", typeof(Byte));
			result.Add("SBYTE", typeof(SByte));
			result.Add("CHAR", typeof(Char));
			result.Add("INT16", typeof(Int16));
			result.Add("UINT16", typeof(UInt16));
			result.Add("INT32", typeof(Int32));
			result.Add("UINT32", typeof(UInt32));
			result.Add("INT64", typeof(Int64));
			result.Add("UINT64", typeof(UInt64));
			result.Add("SINGLE", typeof(Single));
			result.Add("DOUBLE", typeof(Double));
			result.Add("DECIMAL", typeof(Decimal));
			result.Add("STRING", typeof(String));
			result.Add("OBJECT", typeof(Object));
			result.Add("BOOL", typeof(Boolean));
			result.Add("BOOLEAN", typeof(Boolean));
			result.Add("DATETIME", typeof(DateTime));
			result.Add("TIMESPAN", typeof(TimeSpan));
			
			result.Add("BIGINT", typeof(Int64));
			result.Add("BINARY", typeof(Byte[]));
			result.Add("BIT", typeof(Boolean));
			//result.Add("CHAR", typeof(String));
			//result.Add("DATETIME", typeof(DateTime));
			//result.Add("DECIMAL", typeof(Decimal));
			result.Add("FLOAT", typeof(Double));
			result.Add("IMAGE", typeof(Byte[]));
			result.Add("INT", typeof(Int32));
			result.Add("MONEY", typeof(Decimal));
			result.Add("NCHAR", typeof(String));
			result.Add("NTEXT", typeof(String));
			result.Add("NUMERIC", typeof(Decimal));
			result.Add("NVARCHAR", typeof(String));
			result.Add("REAL", typeof(Single));
			result.Add("SMALLDATETIME", typeof(DateTime));
			result.Add("SMALLINT", typeof(Int16));
			result.Add("SMALLMONEY", typeof(Decimal));
			result.Add("SQL_VARIANT", typeof(Object));
			result.Add("TEXT", typeof(String));
			result.Add("TIMESTAMP", typeof(Byte[]));
			result.Add("TINYINT", typeof(Byte));
			result.Add("UNIQUEIDENTIFIER", typeof(Guid));
			result.Add("VARBINARY", typeof(Byte[]));
			result.Add("VARCHAR", typeof(String));
			result.Add("XML", typeof(String));
			
			return result;
		}

		private static MethodInfo GetSpecialBuiltInOperator(string methodName, params Type[] parameterTypes)
		{
			MethodInfo result = typeof(BuiltInOperators).GetMethod(methodName, parameterTypes);

			if (result == null)
				throw ExceptionBuilder.InternalError(
					"Could not found special method {0}.{1}({2})",
					typeof(BuiltInOperators).FullName,
					methodName,
					FormattingHelpers.FormatTypeList(parameterTypes)
				);

			return result;
		}

		#endregion

		#region Conversion Methods

		public static PrimitiveType GetPrimitiveType(Type type)
		{
			if (type == typeof(Boolean))
				return PrimitiveType.Boolean;

			if (type == typeof(Byte))
				return PrimitiveType.Byte;

			if (type == typeof(SByte))
				return PrimitiveType.SByte;

			if (type == typeof(Char))
				return PrimitiveType.Char;

			if (type == typeof(Int16))
				return PrimitiveType.Int16;

			if (type == typeof(UInt16))
				return PrimitiveType.UInt16;

			if (type == typeof(Int32))
				return PrimitiveType.Int32;

			if (type == typeof(UInt32))
				return PrimitiveType.UInt32;

			if (type == typeof(Int64))
				return PrimitiveType.Int64;

			if (type == typeof(UInt64))
				return PrimitiveType.UInt64;

			if (type == typeof(Single))
				return PrimitiveType.Single;

			if (type == typeof(Double))
				return PrimitiveType.Double;

			if (type == typeof(Decimal))
				return PrimitiveType.Decimal;

			if (type == typeof(String))
				return PrimitiveType.String;

			if (type == typeof(Object))
				return PrimitiveType.Object;

			if (type == typeof(DBNull))
				return PrimitiveType.Null;

			return PrimitiveType.None;
		}

		public static bool ConversionNeeded(Type fromType, Type targetType)
		{
			if (fromType == targetType)
				return false;

			// NULL does not need any conversion.

			if (fromType == typeof(DBNull) || targetType == typeof(DBNull))
				return false;

			// Check if fromType is derived from targetType. In this
			// case no conversion is needed.

			Type baseType = fromType.BaseType;

			while (baseType != null)
			{
				if (baseType == targetType)
					return false;

				baseType = baseType.BaseType;
			}

			if (fromType.IsInterface && targetType == typeof(object))
			{
				// Though interfaces are not derived from System.Object, they are
				// compatible to it.
				return false;
			}

			if (targetType.IsInterface)
			{
				// Check if fromType implements targetType. In this
				// case no conversion is needed.

				if (ArrayHelpers.Contains(fromType.GetInterfaces(), targetType))
					return false;
			}

			return true;
		}

		public bool ConversionExists(Type fromType, Type targetType, bool allowExplicit)
		{
			if (!ConversionNeeded(fromType, targetType))
				return true;

			return allowExplicit && (AllowsDownCast(fromType, targetType) ||
			                         GetExplicitConversionMethod(fromType, targetType) != null) ||
			       GetImplicitConversionMethod(fromType, targetType) != null;
		}

		public Type ChooseBetterTypeConversion(Type type1, Type type2)
		{
			// Make sure that boxing is better than converting

			if (type1 == typeof(object))
				return type1;

			if (type2 == typeof(object))
				return type2;

		    // Make sure every type is better than NULL
		    
		    if (type1 == typeof(DBNull))
		        return type2;

            if (type2 == typeof(DBNull))
                return type1;
		    
            if (ConversionNeeded(type1, type2))
			{
				// Check if type2 is the "better" type.
				//
				// That is the case if and only if
				//    an implicit conversion from type1 --> type2 exists
				//    and no implicit conversion from type2 --> type1 exists.

				if (ConversionExists(type1, type2, false) && !ConversionExists(type2, type1, false))
					return type2;
			}	

			return type1;
		}

		private ExpressionNode ConvertExpressionIfRequired(ExpressionNode expression, Type targetType, bool enableDownCast)
		{
			if (expression.ExpressionType == typeof(DBNull))
				return LiteralExpression.FromTypedNull(targetType);

			if (ConversionNeeded(expression.ExpressionType, targetType))
			{
				MethodInfo methodInfo = GetImplicitConversionMethod(expression.ExpressionType, targetType);

				if (methodInfo == null)
					methodInfo = GetExplicitConversionMethod(expression.ExpressionType, targetType);

				if (methodInfo != null)
				{
					CastExpression result = new CastExpression(expression, targetType);
					result.ConvertMethod = methodInfo;
					return result;
				}

				if (enableDownCast && AllowsDownCast(expression.ExpressionType, targetType))
				{
					CastExpression result = new CastExpression(expression, targetType);
					return result;
				}

				// Conversion not possible, report an error.
				_errorReporter.CannotCast(expression, targetType);
			}

			return expression;
		}

		public ExpressionNode ConvertExpressionIfRequired(ExpressionNode expression, Type targetType)
		{
			return ConvertExpressionIfRequired(expression, targetType, false);
		}

		public ExpressionNode ConvertOrDowncastExpressionIfRequired(ExpressionNode expression, Type targetType)
		{
			return ConvertExpressionIfRequired(expression, targetType, true);
		}

		private static bool AllowsDownCast(Type sourceType, Type targetType)
		{
			if (targetType.IsSubclassOf(sourceType))
				return true;

			if (targetType.IsInterface)
				return true;

			return false;
		}

		#endregion

		#region General Overload Resolution Helpers

		private static int TypeHierachyDistance(Type baseType, Type type)
		{
			int distance = 0;

			while (type != null && type != baseType)
			{
				distance++;
				type = type.BaseType;
			}

			return distance;
		}

		private int CompareConversions(Type source, Type type1, Type type2)
		{
			PrimitiveType ptSource = GetPrimitiveType(source);
			PrimitiveType ptType1 = GetPrimitiveType(type1);
			PrimitiveType ptType2 = GetPrimitiveType(type2);

			if (ptSource != PrimitiveType.None &&
				ptType1 != PrimitiveType.None &&
				ptType2 != PrimitiveType.None)
			{
				// All types are pimitive types, compare them in the predefined way.
				return CompareConversions(ptSource, ptType1, ptType2);
			}
			else
			{
				// At least one type is a custom type.

				bool type1IsAssignableFromSource = type1.IsAssignableFrom(source);
				bool type2IsAssignableFromSource = type2.IsAssignableFrom(source);

				if (type1IsAssignableFromSource && !type2IsAssignableFromSource)
				{
					return -1;
				}
				else if (!type1IsAssignableFromSource && type2IsAssignableFromSource)
				{
					return 1;
				}
				else if (type1IsAssignableFromSource && type2IsAssignableFromSource)
				{
					// Both are assignable. See which one is better.
					//
					// First we ensure that classes are always preferred over interfaces.

					if (!type1.IsInterface && type2.IsInterface)
					{
						return -1;
					}
					else if (type1.IsInterface && !type2.IsInterface)
					{
						return 1;
					}

					// Both are interfaces or both are classes. To determine which one
					// is better we assume that the 'closest' one (according to the
					// type hierarchy) to the source type is better.

					int type1Distance = TypeHierachyDistance(source, type1);
					int type2Distance = TypeHierachyDistance(source, type2);
					return type1Distance - type2Distance;
				}
				else
				{
					// The class hierarachy does not help. They can only be compatible by
					// custom casting operators.
					//
					// We check whaht kind of (implicit or explicit) to determine which one
					// is better.

					CastingOperatorType sourceToType1;

					if (GetImplicitConversionMethod(source, type1) != null)
						sourceToType1 = CastingOperatorType.Implicit;
					else
						sourceToType1 = CastingOperatorType.Explicit;

					CastingOperatorType sourceToType2;

					if (GetImplicitConversionMethod(source, type2) != null)
						sourceToType2 = CastingOperatorType.Implicit;
					else
						sourceToType2 = CastingOperatorType.Explicit;

					if (sourceToType1 == sourceToType2)
					{
						// Both are implicit or both implicit -- neither is better.
						return 0;
					}

					if (sourceToType1 == CastingOperatorType.Implicit)
					{
						// An implicit conversion from source to type1 exists, but only an
						// explicit conversion from source to type2 exists -- type1 is better.
						return -1;
					}
					else
					{
						// It is the other way around -- type2 is better.
						return 1;
					}
				}
			}	
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		private static int CompareConversions(PrimitiveType source, PrimitiveType type1, PrimitiveType type2)
		{
			// If type1 and type2 are equal neither are better

			if (type1 == type2)
				return 0;

			// If source is type1, type1 is better

			if (source == type1)
				return -1;

			// If source is type2, type2 is better

			if (source == type2)
				return 1;

			// Make sure we prefer System.Object for NULL values

			if (source == PrimitiveType.Null)
			{
				if (type1 == PrimitiveType.Object)
					return -1;

				if (type1 == PrimitiveType.Object)
					return 1;

				return 0;
			}

			// Make sure we prefer everything over generic System.Object

			if (type1 == PrimitiveType.Object)
				return 1;

			if (type2 == PrimitiveType.Object)
				return -1;

			// Make sure we prefer signed before unsigned

			if (type1 == PrimitiveType.SByte && (type2 == PrimitiveType.Byte || type2 == PrimitiveType.UInt16 || type2 == PrimitiveType.UInt32 || type2 == PrimitiveType.UInt64))
				return -1;

			if (type2 == PrimitiveType.SByte && (type1 == PrimitiveType.Byte || type1 == PrimitiveType.UInt16 || type1 == PrimitiveType.UInt32 || type1 == PrimitiveType.UInt64))
				return 1;

			if (type1 == PrimitiveType.Int16 && (type2 == PrimitiveType.UInt16 || type2 == PrimitiveType.UInt32 || type2 == PrimitiveType.UInt64))
				return -1;

			if (type2 == PrimitiveType.Int16 && (type1 == PrimitiveType.UInt16 || type1 == PrimitiveType.UInt32 || type1 == PrimitiveType.UInt64))
				return 1;

			if (type1 == PrimitiveType.Int32 && (type2 == PrimitiveType.UInt32 || type2 == PrimitiveType.UInt64))
				return -1;

			if (type2 == PrimitiveType.Int32 && (type1 == PrimitiveType.UInt32 || type1 == PrimitiveType.UInt64))
				return 1;

			if (type1 == PrimitiveType.Int64 && type2 == PrimitiveType.UInt64)
				return -1;

			if (type2 == PrimitiveType.Int64 && type1 == PrimitiveType.UInt64)
				return 1;

			if (source == PrimitiveType.Byte || source == PrimitiveType.Int16 || source == PrimitiveType.Int32 || source == PrimitiveType.Int64 ||
				source == PrimitiveType.SByte || source == PrimitiveType.UInt16 || source == PrimitiveType.UInt32 || source == PrimitiveType.UInt64)
			{
				// Make sure we prefer ints before reals

				if (type1 == PrimitiveType.Int64 && (type2 == PrimitiveType.Single || type2 == PrimitiveType.Double))
					return -1;

				if (type2 == PrimitiveType.Int64 && (type1 == PrimitiveType.Single || type1 == PrimitiveType.Double))
					return 1;
			}

			if (source == PrimitiveType.Byte || source == PrimitiveType.Int16 || source == PrimitiveType.Int32 || source == PrimitiveType.Int64 ||
				source == PrimitiveType.SByte || source == PrimitiveType.UInt16 || source == PrimitiveType.UInt32 || source == PrimitiveType.UInt64 ||
				source == PrimitiveType.Single || source == PrimitiveType.Double)
			{
				// Make sure if source is a number and the conversion is string we prefer the 
				// other conversion (which can't be a string anymore)

				if (type1 == PrimitiveType.String)
					return 1;

				if (type2 == PrimitiveType.String)
					return -1;
			}

			return 0;
		}

		private int CompareFunctions(Type[] argumentTypes, Type[] function1ParameterTypes, Type[] function2ParameterTypes)
		{
			int conversionSum = 0;

			for (int i = 0; i < argumentTypes.Length; i++)
			{
				conversionSum += CompareConversions(
					argumentTypes[i],
					function1ParameterTypes[i],
					function2ParameterTypes[i]
				);
			}

			return conversionSum;
		}

		#endregion

		#region Implicit/Explicit Casting Operator Helpers

		private MethodInfo GetImplicitConversionMethod(Type sourceType, Type targetType)
		{
			return GetConversionMethod(CastingOperatorType.Implicit, sourceType, targetType);
		}

		private MethodInfo GetExplicitConversionMethod(Type sourceType, Type targetType)
		{
			return GetConversionMethod(CastingOperatorType.Explicit, sourceType, targetType);
		}

		private MethodInfo GetConversionMethod(CastingOperatorType castingOperatorType, Type sourceType, Type targetType)
		{
			MethodInfo targetFromSource;
			MethodInfo sourceToTarget;

			// First search the target type for a suitable conversion method.

			targetFromSource = GetConversionMethod(OperatorMethodCache.GetOperatorMethods(targetType, castingOperatorType), sourceType, targetType);

			// Secondly search the source type for a suitable conversion method.

			sourceToTarget = GetConversionMethod(OperatorMethodCache.GetOperatorMethods(sourceType, castingOperatorType), sourceType, targetType);
	
			if (sourceToTarget == null && targetFromSource == null)
			{
				// Ok, no custom casting operators found. Look in the built-in conversions.

				return BuiltInConversions.GetConversion(castingOperatorType, sourceType, targetType);
			}
			else
			{
				// Now we might have two possible methods. We found an ambiguous match in that case.

				if (targetFromSource != sourceToTarget && targetFromSource != null && sourceToTarget != null)
				{
					// Ambiguous match, we don't know what to do.
					_errorReporter.AmbiguousOperator(castingOperatorType, targetFromSource, sourceToTarget);
					return targetFromSource;
				}

				if (targetFromSource != null)
					return targetFromSource;
				else
					return sourceToTarget;
			}
		}

		private static MethodInfo GetConversionMethod(IEnumerable<MethodInfo> candidates, Type sourceType, Type targetType)
		{
			MethodInfo lastMatchMethod = null;
			Type lastMatchParameterType = null;

			foreach (MethodInfo methodInfo in candidates)
			{
				Type parameterType = methodInfo.GetParameters()[0].ParameterType;

				if (methodInfo.ReturnType == targetType &&
					parameterType.IsAssignableFrom(sourceType))
				{
					if (lastMatchParameterType == null || parameterType.IsSubclassOf(lastMatchParameterType))
					{
						lastMatchMethod = methodInfo;
						lastMatchParameterType = parameterType;
					}
				}
			}

			return lastMatchMethod;
		}

		#endregion

		#region Bind Invocations

		private InvocableBinding[] GetApplicableInvocables(IEnumerable<InvocableBinding> invocables, Type[] argumentTypes, bool allowExplicit)
		{
			List<InvocableBinding> applicableFunctionList = new List<InvocableBinding>();

			foreach (InvocableBinding invocableBinding in invocables)
			{
				Type[] parameterTypes = invocableBinding.GetParameterTypes();

				if (parameterTypes.Length == argumentTypes.Length)
				{
					for (int i = 0; i < parameterTypes.Length; i++)
					{
						if (!ConversionExists(argumentTypes[i], parameterTypes[i], allowExplicit))
							goto nextBinding;
					}

					applicableFunctionList.Add(invocableBinding);
				}

			nextBinding:
				;
			}

			return applicableFunctionList.ToArray();
		}

		public InvocableBinding BindInvocation(InvocableBinding[] candidates, Type[] argumentTypes)
		{
			// Get applicable invocables
			
			// First we check if we can find any candidates if we only allow implicit conversions.
			InvocableBinding[] applicableInvocables = GetApplicableInvocables(candidates, argumentTypes, false);

			if (applicableInvocables == null || applicableInvocables.Length == 0)
			{
				// We could not find any candiate, lets see wether we find candidates 
				// if we also allow explicit conversions.
				applicableInvocables = GetApplicableInvocables(candidates, argumentTypes, true);
				
				if (applicableInvocables == null || applicableInvocables.Length == 0)
					return null;
			}

			// Now select the best one

			InvocableBinding bestInvocable = applicableInvocables[0];
			InvocableBinding lastEquallyGoodInvocable = null;

			for (int i = 1; i < applicableInvocables.Length; i++)
			{
				InvocableBinding invocable = applicableInvocables[i];

				int comparisonResult = CompareFunctions(argumentTypes, bestInvocable.GetParameterTypes(), invocable.GetParameterTypes());

				if (comparisonResult == 0)
				{
					lastEquallyGoodInvocable = invocable;
				}
				else if (comparisonResult > 0)
				{
					lastEquallyGoodInvocable = null;
					bestInvocable = invocable;
				}
			}

			// Ok, now check that our best function is really the best one

			if (lastEquallyGoodInvocable != null)
			{
				// We have also a function that is equally good. -- The call is ambiguous.
				_errorReporter.AmbiguousInvocation(bestInvocable, lastEquallyGoodInvocable, argumentTypes);

				// Return first to avoid cascading errors.
				return bestInvocable;
			}

			return bestInvocable;
		}

		#endregion

		#region Bind Operator Helper Methods

		[SuppressMessage("Microsoft.Design", "CA1064:ExceptionsShouldBePublic")]
		[SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
		[SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable")]
		private sealed class InvocationIsAmbiguousException : Exception
		{
			private MethodInfo _method1;
			private MethodInfo _method2;

			public InvocationIsAmbiguousException(MethodInfo method1, MethodInfo method2)
			{
				_method1 = method1;
				_method2 = method2;
			}

			public MethodInfo Method1
			{
				get { return _method1; }
			}

			public MethodInfo Method2
			{
				get { return _method2; }
			}
		}

		private MethodInfo[] GetApplicableMethods(IEnumerable<MethodInfo> functions, Type[] argumentTypes, bool allowExplicit)
		{
            List<MethodInfo> applicableFunctionList = new List<MethodInfo>();

			foreach (MethodInfo method in functions)
			{
				ParameterInfo[] methodParameters = method.GetParameters();

				if (methodParameters.Length == argumentTypes.Length)
				{
					for (int i = 0; i < methodParameters.Length; i++)
					{
						if (!ConversionExists(argumentTypes[i], methodParameters[i].ParameterType, allowExplicit))
							goto nextBinding;
					}

					applicableFunctionList.Add(method);
				}

			nextBinding:
				;
			}

			return applicableFunctionList.ToArray();
		}

		private static Type[] GetParamterTypes(MethodInfo methodInfo)
		{
			ParameterInfo[] parameterInfos = methodInfo.GetParameters();

			Type[] result = new Type[parameterInfos.Length];
			for (int i = 0; i < parameterInfos.Length; i++)
				result[i] = parameterInfos[i].ParameterType;

			return result;
		}

		private MethodInfo BindMethodInfo(IEnumerable<MethodInfo> candidates, Type[] argumentTypes)
		{
			// First we check if we can find any candidates if we only allow implicit conversions.
			MethodInfo[] applicableFunctions = GetApplicableMethods(candidates, argumentTypes, false);

			if (applicableFunctions == null || applicableFunctions.Length == 0)
			{
				// We could not find any candiate, lets see wether we find candidates 
				// if we also allow explicit conversions.
				applicableFunctions = GetApplicableMethods(candidates, argumentTypes, true);
				
				if (applicableFunctions == null || applicableFunctions.Length == 0)
					return null;
			}

			// Now select the best one

			MethodInfo bestFunction = applicableFunctions[0];
			MethodInfo lastEquallyGoodFunction = null;

			for (int i = 1; i < applicableFunctions.Length; i++)
			{
				MethodInfo function = applicableFunctions[i];

				int comparisonResult = CompareFunctions(argumentTypes, GetParamterTypes(bestFunction), GetParamterTypes(function));

				if (comparisonResult == 0)
				{
					// Special handling for builtin operators:
					//
					// The builtin operators must not be used if an equally good or
					// better overload is present.

					if (bestFunction.DeclaringType == typeof(BuiltInOperators))
					{
						// The new function is equally good, but the current best
						// function is from BuiltinOperators. The new function
						// is better than the current one.
						bestFunction = function;
					}
					else if (function.DeclaringType == typeof(BuiltInOperators))
					{
						// The new, equally good function is from the BuiltinOperators.
						// Just ignore it.
					}
					else
					{
						// The new functions is not from the BuiltinOperators and
						// it is equally good. Remeber this functions for possible
						// ambguity error message.
						lastEquallyGoodFunction = function;
					}
				}
				else if (comparisonResult > 0)
				{
					lastEquallyGoodFunction = null;
					bestFunction = function;
				}
			}

			// Ok, now check that our best function is really the best one

			if (lastEquallyGoodFunction != null)
			{
				// We have also a function that is equally good. -- The call is ambiguous.
				throw new InvocationIsAmbiguousException(bestFunction, lastEquallyGoodFunction);
			}

			return bestFunction;
		}

		#endregion

		#region Bind Operator Methods

		public MethodInfo BindOperator(UnaryOperator op, Type operandType)
		{
			if (operandType == typeof(DBNull))
				return _unaryOperatorPlaceholder;

			try
			{
				List<MethodInfo> methodList = new List<MethodInfo>();

				// Get user defined operator
				methodList.AddRange(OperatorMethodCache.GetOperatorMethods(operandType, op));
				
				// Get the operator method from the buitlin ones
				methodList.AddRange(OperatorMethodCache.GetOperatorMethods(typeof(BuiltInOperators), op));

				MethodInfo[] methods = methodList.ToArray();

				return BindMethodInfo(methods, new Type[] {operandType});
			}
			catch (InvocationIsAmbiguousException ex)
			{
				_errorReporter.AmbiguousOperator(op, operandType, ex.Method1, ex.Method2);

				// Avoid cascading errors
				return ex.Method1;
			}
		}

		public MethodInfo BindOperator(BinaryOperator op, Type leftOperandType, Type rightOperandType)
		{
			if (leftOperandType == typeof(DBNull) || rightOperandType == typeof(DBNull))
				return _binaryOperatorPlaceholder;

			MethodInfo result;

			try
			{
				List<MethodInfo> methodList = new List<MethodInfo>();

				// Get the operator method from left...
				methodList.AddRange(OperatorMethodCache.GetOperatorMethods(leftOperandType, op));

				if (leftOperandType != rightOperandType)
				{
					List<Type> declaringTypes = new List<Type>();
					foreach (MethodInfo info in methodList)
					{
						if (!declaringTypes.Contains(info.DeclaringType))
							declaringTypes.Add(info.DeclaringType);
					}

					foreach (MethodInfo rightOperatorMethod in OperatorMethodCache.GetOperatorMethods(rightOperandType, op))
					{
						if (!declaringTypes.Contains(rightOperatorMethod.DeclaringType))
							methodList.Add(rightOperatorMethod);
					}
				}

				// ...from the builtin ones.
				methodList.AddRange(OperatorMethodCache.GetOperatorMethods(typeof(BuiltInOperators), op));

				// Perform overload resolution.
				MethodInfo[] methods = methodList.ToArray();

				result = BindMethodInfo(methods, new Type[] {leftOperandType, rightOperandType});
			}
			catch (InvocationIsAmbiguousException ex)
			{
				_errorReporter.AmbiguousOperator(op, leftOperandType, rightOperandType, ex.Method1, ex.Method2);

				// Avoid cascading errors
				result = ex.Method1;
			}

			// Due to the lack of generic operator methods we have to manually some rules here.
			//
			// 1. = and <>. There exists an operator method op_X(object, object) that would allow
			//    comparing any type to any other type. This is not correct. We want to enforce
			//    that both type are compatible (or if any operand is an interface).
			// 2. & and | for enums. There exists an operator method op_X(Enum, Enum) that would allow
			//    combining any enum with any other enum. This is not correct. We want to enforce
			//    that both enums are the same.

			if (result == _generalEqualityOperator || result == _generalInequalityOperator)
			{
				if (!leftOperandType.IsAssignableFrom(rightOperandType) && 
					!rightOperandType.IsAssignableFrom(leftOperandType) &&
					!leftOperandType.IsInterface &&
					!rightOperandType.IsInterface)
					return null;
			}
			else if (result == _enumBitAndOperator || result == _enumBitOrOperator)
			{
				if (leftOperandType != rightOperandType)
					return null;
			}

			return result;
		}
		
		#endregion

		#region Bind Type

		private static void SplitTypeName(string typeName, out string qualifiedTypeName, out string assemblyName)
		{
			string[] parts = typeName.Split(',');

			qualifiedTypeName = parts[0].Trim();

			if (parts.Length == 2)
				assemblyName = parts[1].Trim();
			else
				assemblyName = null;
		}

		private static void AddTypes(ICollection<Type> target, Assembly assembly, string typeName, bool caseSensitive)
		{
			Type[] exportedTypes;
			
			try
			{
				exportedTypes = assembly.GetExportedTypes();
			}
			catch (FileNotFoundException)
			{
				// Caused by dynamic assemblies such as the Reshaper Unit Testing Framework.
				return;
			}

			StringComparison comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
			foreach (Type type in exportedTypes)
			{
				if (String.Compare(type.FullName, typeName, comparison) == 0)
					target.Add(type);
			}
		}

		public Type BindType(string assemblyQualifiedTypeName, bool caseSensitive)
		{
			string qualifiedTypeName;
			string assemblyName;

			SplitTypeName(assemblyQualifiedTypeName, out qualifiedTypeName, out assemblyName);

			if (!caseSensitive && assemblyName == null)
			{
				// First we try the shortcuts.

                Type primitiveType;
                if (_typeShortCutTable.TryGetValue(qualifiedTypeName, out primitiveType))
                    return primitiveType;
			}

			List<Type> typeList = new List<Type>();

			if (assemblyName != null)
			{
				Assembly assembly;

				try
				{
					assembly = Assembly.Load(assemblyName);
				}
				catch (FileNotFoundException ex)
				{
					_errorReporter.CannotLoadTypeAssembly(assemblyName, ex);
					return null;
				}
				catch (FileLoadException ex)
				{
					_errorReporter.CannotLoadTypeAssembly(assemblyName, ex);
					return null;
				}
				catch (BadImageFormatException ex)
				{
					_errorReporter.CannotLoadTypeAssembly(assemblyName, ex);
					return null;
				}

				AddTypes(typeList, assembly, qualifiedTypeName, caseSensitive);
			}
			else
			{
				Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

				foreach (Assembly assembly in assemblies)
					AddTypes(typeList, assembly, qualifiedTypeName, caseSensitive);
			}

			if (typeList.Count == 0)
				return null;

			if (typeList.Count > 1)
			{
				Type[] types = typeList.ToArray();
				_errorReporter.AmbiguousType(assemblyQualifiedTypeName, types);
			}

			return typeList[0];
		}

		#endregion
	}
}