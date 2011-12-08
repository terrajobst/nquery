using System;
using System.Collections.Generic;
using System.Reflection;

using NQuery.Runtime;

namespace NQuery
{
	public sealed class FunctionCollection : BindingCollection<FunctionBinding>
	{
		private Dictionary<string, List<FunctionBinding>> _functionTable = new Dictionary<string, List<FunctionBinding>>();

		internal FunctionCollection()
		{
		}

		private static IEnumerable<ReflectionFunctionBinding> CreateBindingsFromContainer(Type containerType)
		{
			List<ReflectionFunctionBinding> bindings = new List<ReflectionFunctionBinding>();
			foreach (MethodInfo methodInfo in containerType.GetMethods())
			{
				if (methodInfo.IsStatic)
				{
					FunctionBindingAttribute[] functionBindingAttributes = (FunctionBindingAttribute[])methodInfo.GetCustomAttributes(typeof(FunctionBindingAttribute), false);

					if (functionBindingAttributes.Length == 1)
					{
						string functionName = functionBindingAttributes[0].Name;
						bool isDeterministic = functionBindingAttributes[0].IsDeterministic;
						ReflectionFunctionBinding reflectionFunctionBinding = new ReflectionFunctionBinding(functionName, methodInfo, null, isDeterministic);
						bindings.Add(reflectionFunctionBinding);
					}
				}
			}
			return bindings;
		}

		private static IEnumerable<ReflectionFunctionBinding> CreateBindingsFromContainer(object container)
		{
			List<ReflectionFunctionBinding> bindings = new List<ReflectionFunctionBinding>();
			foreach (MethodInfo methodInfo in container.GetType().GetMethods())
			{
				object instance;

				if (methodInfo.IsStatic)
					instance = null;
				else
					instance = container;

				FunctionBindingAttribute[] functionBindingAttributes = (FunctionBindingAttribute[])methodInfo.GetCustomAttributes(typeof(FunctionBindingAttribute), false);

				if (functionBindingAttributes.Length == 1)
				{
					string functionName = functionBindingAttributes[0].Name;
					bool isDeterministic = functionBindingAttributes[0].IsDeterministic;
					ReflectionFunctionBinding reflectionFunctionBinding = new ReflectionFunctionBinding(functionName, methodInfo, instance, isDeterministic);
					bindings.Add(reflectionFunctionBinding);
				}
			}
			return bindings;
		}

		protected override void BeforeInsert(FunctionBinding binding)
		{
			// TODO: Ensure function does not have VOID return type.
			// TODO: Ensure function does not have REF or OUT parameter.

			List<FunctionBinding> functions;
			if (!_functionTable.TryGetValue(binding.Name, out functions))
			{
				functions = new List<FunctionBinding>();
				_functionTable.Add(binding.Name, functions);
			}
			else
			{
				// Check that no functions with the same parameter types already exists.

				Type[] newParameterTypes = binding.GetParameterTypes();
				foreach (FunctionBinding existingFunctionBinding in functions)
				{
					Type[] existingParameterTypes = existingFunctionBinding.GetParameterTypes();

					if (newParameterTypes.Length == existingParameterTypes.Length)
					{
						// Assume they are the same
						bool isSame = true;

						// Check if any parameter type is different.

						for (int i = 0; i < newParameterTypes.Length; i++)
						{
							if (newParameterTypes[i] != existingParameterTypes[i])
							{
								isSame = false;
								break;
							}
						}

						if (isSame)
							throw ExceptionBuilder.FunctionWithSameNameAndSignatureAlreadyInCollection("binding", binding);
					}
				}
			}

			functions.Add(binding);
		}

		protected override void AfterRemove(FunctionBinding binding)
		{
			List<FunctionBinding> functions;
			if (_functionTable.TryGetValue(binding.Name, out functions))
			{
				functions.Remove(binding);

				if (functions.Count == 0)
					_functionTable.Remove(binding.Name);
			}
		}

		protected override void ClearItems()
		{
			base.ClearItems();
			// Clear hashtable
			_functionTable.Clear();
		}

		public void AddDefaults()
		{
			AddFromContainer(typeof(BuiltInFunctions));
		}

		public FunctionBinding Add(string functionName, Delegate functionDelegate)
		{
			if (functionName == null)
				throw ExceptionBuilder.ArgumentNull("functionName");

			if (functionDelegate == null)
				throw ExceptionBuilder.ArgumentNull("functionDelegate");

			// Check return type

			if (functionDelegate.Method.ReturnType == typeof(void))
				throw ExceptionBuilder.FunctionMustNotBeVoid(functionDelegate);

			// Check parameters

			ParameterInfo[] parameters = functionDelegate.Method.GetParameters();

			foreach (ParameterInfo param in parameters)
			{
				if (param.IsOut || param.ParameterType.IsByRef)
					throw ExceptionBuilder.FunctionMustNotHaveRefOrOutParams(functionDelegate, param);

				if (param.IsOptional)
					throw ExceptionBuilder.FunctionMustNotHaveOptionalParams(functionDelegate, param);

				if (param.ParameterType.IsArray)
					throw ExceptionBuilder.FunctionMustNotHaveArrayParams(functionDelegate, param);
			}

			// Ok, everything seems to be fine.

			ReflectionFunctionBinding reflectionFunctionBinding = new ReflectionFunctionBinding(functionName, functionDelegate.Method, functionDelegate.Target, false);
			Add(reflectionFunctionBinding);
			return reflectionFunctionBinding;
		}

		public void AddFromContainer(Type containerType)
		{
			if (containerType == null)
				throw ExceptionBuilder.ArgumentNull("containerType");

			IEnumerable<ReflectionFunctionBinding> bindings = CreateBindingsFromContainer(containerType);
			foreach (ReflectionFunctionBinding binding in bindings)
				Add(binding);
		}

		public void AddFromContainer(object container)
		{
			if (container == null)
				throw ExceptionBuilder.ArgumentNull("container");

			IEnumerable<ReflectionFunctionBinding> bindings = CreateBindingsFromContainer(container);
			foreach (ReflectionFunctionBinding binding in bindings)
				Add(binding);
		}

		private void RemoveFromContainer(IEnumerable<ReflectionFunctionBinding> containerBindings)
		{
			List<FunctionBinding> bindigsToRemove = new List<FunctionBinding>();

			foreach (FunctionBinding functionBinding in this)
			{
				ReflectionFunctionBinding reflectionFunctionBinding = functionBinding as ReflectionFunctionBinding;
				if (reflectionFunctionBinding != null)
				{
					foreach (ReflectionFunctionBinding containerBinding in containerBindings)
					{
						if (reflectionFunctionBinding.Method == containerBinding.Method)
						{
							bindigsToRemove.Add(functionBinding);
							break;
						}
					}
				}
			}

			foreach (FunctionBinding binding in bindigsToRemove)
				Remove(binding);
		}

		public void RemoveFromContainer(Type containerType)
		{
			if (containerType == null)
				throw ExceptionBuilder.ArgumentNull("containerType");

			IEnumerable<ReflectionFunctionBinding> containerBindings = CreateBindingsFromContainer(containerType);
			RemoveFromContainer(containerBindings);
		}

		public void RemoveFromContainer(object container)
		{
			if (container == null)
				throw ExceptionBuilder.ArgumentNull("container");

			IEnumerable<ReflectionFunctionBinding> containerBindings = CreateBindingsFromContainer(container);
			RemoveFromContainer(containerBindings);
		}

		public override FunctionBinding[] Find(Identifier identifier)
		{
			if (identifier == null)
				throw ExceptionBuilder.ArgumentNull("identifier");

			List<FunctionBinding> result = new List<FunctionBinding>();

			foreach (string exitingsFunctionName in _functionTable.Keys)
			{
				if (identifier.Matches(exitingsFunctionName))
				{
					List<FunctionBinding> functions = _functionTable[exitingsFunctionName];
					result.AddRange(functions);
				}
			}

			return result.ToArray();
		}
	}
}