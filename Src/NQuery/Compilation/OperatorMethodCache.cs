using System;
using System.Collections.Generic;
using System.Reflection;

namespace NQuery.Compilation
{
	internal static class OperatorMethodCache
	{
		private class OperatorEntry
		{
			public Dictionary<string,List<MethodInfo>> OperatorMethods;		
		}
		
		private const string OP_IMPLICIT_METHOD_NAME = "op_Implicit";
		private const string OP_EXPLICIT_METHOD_NAME = "op_Explicit";

		private static Dictionary<Type, OperatorEntry> _typeOperatorHashtable = new Dictionary<Type, OperatorEntry>();
        private static Dictionary<string, Operator> _overloadableOperatorMethodNames = GetOverloadableOperatorMethodNames();

        private static Dictionary<string, Operator> GetOverloadableOperatorMethodNames()
		{
			// Get all fields holding operators.

			const BindingFlags bindingFlags = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static;

			FieldInfo[] unaryOperators = typeof(UnaryOperator).GetFields(bindingFlags);
			FieldInfo[] binaryOperators = typeof(BinaryOperator).GetFields(bindingFlags);

			List<FieldInfo> allOperatorFields = new List<FieldInfo>();
			allOperatorFields.AddRange(unaryOperators);
			allOperatorFields.AddRange(binaryOperators);

			for (int i = allOperatorFields.Count - 1; i >= 0; i--)
			{
				FieldInfo fieldInfo = allOperatorFields[i];

				if (!fieldInfo.FieldType.IsSubclassOf(typeof(Operator)) || !fieldInfo.IsInitOnly)
					allOperatorFields.RemoveAt(i);
			}

			// Hashtable containing the names of overloadable operator method names.

            Dictionary<string, Operator> overloadableOperatorMethodNames = new Dictionary<string, Operator>();
			foreach (FieldInfo fieldInfo in allOperatorFields)
			{
				Operator op = (Operator) fieldInfo.GetValue(null);

				// NOTE: Don't refuse non-overloadable operators. Otherwise we cannot
				//       retreive non-overloadable operators from the BuiltinOperators class!

				overloadableOperatorMethodNames.Add(op.MethodName, op);
			}

			return overloadableOperatorMethodNames;
		}

		private static OperatorEntry GetOperatorEntry(Type type)
		{
		    OperatorEntry result;
            if (!_typeOperatorHashtable.TryGetValue(type, out result))
            {
                lock (_typeOperatorHashtable)
                {
                    if (!_typeOperatorHashtable.TryGetValue(type, out result))
                    {
                        result = new OperatorEntry();
                    	result.OperatorMethods = new Dictionary<string, List<MethodInfo>>();

                        MethodInfo[] methodInfos = type.GetMethods(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static);
                        Array.Sort(methodInfos, (x, y) => String.Compare(x.ToString(), y.ToString(), StringComparison.Ordinal));

                        foreach (MethodInfo methodInfo in methodInfos)
                        {
                            Operator op = GetOverloadableOperator(methodInfo.Name);

                            if (op != null ||
                                methodInfo.Name == OP_IMPLICIT_METHOD_NAME ||
                                methodInfo.Name == OP_EXPLICIT_METHOD_NAME)
                            {
                                List<MethodInfo> operatorMethods;
                                if (!result.OperatorMethods.TryGetValue(methodInfo.Name, out operatorMethods))
                                {
                                    operatorMethods = new List<MethodInfo>();
                                    result.OperatorMethods.Add(methodInfo.Name, operatorMethods);
                                }

                                operatorMethods.Add(methodInfo);
                            }
                        }

                        _typeOperatorHashtable.Add(type, result);
                    }
                }
            }
		    
		    return result;
		}

	    private static Operator GetOverloadableOperator(string opMethodName)
	    {
	        Operator result;
	        if (_overloadableOperatorMethodNames.TryGetValue(opMethodName, out result))
	            return result;
	        
	        return null;
	    }

	    public static MethodInfo[] GetOperatorMethods(Type type, Operator op)
		{
			OperatorEntry entry = GetOperatorEntry(type);

            List<MethodInfo> methods;
            if (!entry.OperatorMethods.TryGetValue(op.MethodName, out methods))
				return new MethodInfo[0];

			return methods.ToArray();
		}

		public static MethodInfo[] GetOperatorMethods(Type type, CastingOperatorType op)
		{
			string opMethodName;

			if (op == CastingOperatorType.Implicit)
				opMethodName = OP_IMPLICIT_METHOD_NAME;
			else
				opMethodName = OP_EXPLICIT_METHOD_NAME;

			OperatorEntry entry = GetOperatorEntry(type);

            List<MethodInfo> methods;
		    if (!entry.OperatorMethods.TryGetValue(opMethodName, out methods))
				return new MethodInfo[0];

			return methods.ToArray();
		}
	}
}