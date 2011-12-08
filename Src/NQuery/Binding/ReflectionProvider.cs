using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace NQuery.Runtime
{
	/// <summary>
	/// This class provides a basic property and method provider that uses reflection to provide definitions 
	/// for all fields, properties and methods of the given type. This class can be inherited to customize the 
	/// list of returned properties.
	/// </summary>
	/// <remarks>
	/// Note to inheritors: Override <see cref="CreateProperty(PropertyInfo)"/> or <see cref="CreateProperty(FieldInfo)"/>
	/// to customize the returned properties. To customize the returned methods override <see cref="CreateMethod"/>.
	/// </remarks>
	public class ReflectionProvider : IPropertyProvider, IMethodProvider
	{
		private BindingFlags _bindingFlags;

		[SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flags")]
		public ReflectionProvider(BindingFlags bindingFlags)
		{
			_bindingFlags = bindingFlags;
		}

		public ReflectionProvider()
			: this(BindingFlags.Instance | BindingFlags.Public)
		{
		}

		[SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flags")]
		public BindingFlags BindingFlags
		{
			get { return _bindingFlags; }
			set { _bindingFlags = value; }
		}

		private static bool ExistingMemberIsMoreSpecific(Type type, MemberInfo existingMember, MemberInfo newMember)
		{
			Type existingMemberDeclaringType = existingMember.DeclaringType;
			Type newMemberDeclaringType = newMember.DeclaringType;

			int existingMemberDistance = 0;
			int newMemberDistance = 0;

			while (existingMemberDeclaringType != null && existingMemberDeclaringType != type)
			{
				existingMemberDistance++;
				existingMemberDeclaringType = existingMemberDeclaringType.BaseType;
			}

			while (newMemberDeclaringType != null && newMemberDeclaringType != type)
			{
				newMemberDistance++;
				newMemberDeclaringType = newMemberDeclaringType.BaseType;
			}

			return existingMemberDistance > newMemberDistance;
		}

		private class PropertyTable
		{
			private Dictionary<string, Entry> _table = new Dictionary<string, Entry>();

			public class Entry
			{
				public PropertyBinding PropertyBinding;
				public MemberInfo MemberInfo;
			}

			public void Add(PropertyBinding propertyBinding, MemberInfo memberInfo)
			{
				Entry entry = new Entry();
				entry.PropertyBinding = propertyBinding;
				entry.MemberInfo = memberInfo;
				_table.Add(propertyBinding.Name, entry);
			}

			public void Remove(Entry entry)
			{
				_table.Remove(entry.PropertyBinding.Name);
			}

			public Entry this[string propertyName]
			{
				get
				{
                    Entry result;
				    if (_table.TryGetValue(propertyName, out result))
				        return result;
				    
				    return null;
				}
			}
		}
		
		private class MethodTable
		{
			private Dictionary<string, Entry> _table = new Dictionary<string, Entry>();

			public class Entry
			{
				public MethodBinding MethodBinding;
				public MethodInfo MethodInfo;
				public string Key;
			}

			private static string GenerateKey(string methodName, Type[] parameterTypes)
			{
				string parameterList = FormattingHelpers.FormatTypeList(parameterTypes);
				return methodName + "(" + parameterList + ")";
			}

			public void Add(MethodBinding methodBinding, MethodInfo methodInfo)
			{
				Entry entry = new Entry();
				entry.Key = GenerateKey(methodBinding.Name, methodBinding.GetParameterTypes());
				entry.MethodBinding = methodBinding;
				entry.MethodInfo = methodInfo;
				_table.Add(entry.Key, entry);
			}

			public void Remove(Entry entry)
			{
				_table.Remove(entry.Key);
			}

			public Entry this[string methodName, Type[] parameterTypes]
			{
				get
				{
					string key = GenerateKey(methodName, parameterTypes);
                    Entry result;
				    if (_table.TryGetValue(key, out result))
				        return result;
				    
					return null;
				}
			}
		}

		private static void AddProperty(PropertyTable propertyTable, ICollection<PropertyBinding> memberList, Type declaringType, PropertyBinding memberBinding, MemberInfo memberInfo)
		{
			// Check if we already have a member with the same name declared.
			PropertyTable.Entry exisitingMemberEntry = propertyTable[memberBinding.Name];
	
			if (exisitingMemberEntry != null)
			{
				// Ok we have one. Check if the existing member is not more specific.
				if (ExistingMemberIsMoreSpecific(declaringType, exisitingMemberEntry.MemberInfo, memberInfo))
				{
					// The existing member is more specific. So we don't add the new one.
					return;
				}
				else
				{
					// The new member is more specific. Remove the old one.
					propertyTable.Remove(exisitingMemberEntry);
					memberList.Remove(exisitingMemberEntry.PropertyBinding);
				}
			}
	
			// Either the new member is more specific or we don't had
			// a member with same name.
			propertyTable.Add(memberBinding, memberInfo);
			memberList.Add(memberBinding);
		}

		private static void AddMethod(MethodTable methodTable, ICollection<MethodBinding> methodList, Type declaringType, MethodBinding methodBinding, MethodInfo methodInfo)
		{
			// Check if we already have a method with the same name and parameters declared.
			MethodTable.Entry exisitingMethodEntry = methodTable[methodBinding.Name, methodBinding.GetParameterTypes()];
	
			if (exisitingMethodEntry != null)
			{
				// Ok we have one. Check if the existing member is not more specific.
				if (ExistingMemberIsMoreSpecific(declaringType, exisitingMethodEntry.MethodInfo, methodInfo))
				{
					// The existing member is more specific. So we don't add the new one.
					return;
				}
				else
				{
					// The new member is more specific. Remove the old one.
					methodTable.Remove(exisitingMethodEntry);
					methodList.Remove(exisitingMethodEntry.MethodBinding);
				}
			}
	
			// Either the new member is more specific or we don't had
			// a member with same name.
			methodTable.Add(methodBinding, methodInfo);
			methodList.Add(methodBinding);
		}

		public PropertyBinding[] GetProperties(Type type)
		{
			if (type == null)
				throw ExceptionBuilder.ArgumentNull("type");

			PropertyTable propertyTable = new PropertyTable();
			List<PropertyBinding> propertyList = new List<PropertyBinding>();

			// Convert CLR Properties
			
			PropertyInfo[] propertyInfos = type.GetProperties(_bindingFlags);
			
			foreach (PropertyInfo currentPropertyInfo in propertyInfos)
			{
				// Ignore indexer
				ParameterInfo[] indexParameters = currentPropertyInfo.GetIndexParameters();
				if (indexParameters != null && indexParameters.Length > 0)
					continue;

				PropertyBinding property = CreateProperty(currentPropertyInfo);

				if (property != null)
					AddProperty(propertyTable, propertyList, type, property, currentPropertyInfo);
			}

			// Convert CLR Fields
			
			FieldInfo[] fieldInfos = type.GetFields(_bindingFlags);

			foreach (FieldInfo currentFieldInfo in fieldInfos)
			{
				PropertyBinding property = CreateProperty(currentFieldInfo);

				if (property != null)
					AddProperty(propertyTable, propertyList, type, property, currentFieldInfo);
			}

			return propertyList.ToArray();
		}

		/// <summary>
		/// Checks whether the given <see cref="MethodInfo"/> is invocable by the query engine, i.e. it can be used 
		/// as <see cref="InvocableBinding"/>.
		/// </summary>
		/// <remarks>
		/// A method cannot be invoked if any of the following is true:
		/// <ul>
		///		<li><paramref name="methodInfo"/> has a special name (e.g. it is getter, setter, indexer or operator method)</li>
		///		<li><paramref name="methodInfo"/> has abstract modifier</li>
		///		<li><paramref name="methodInfo"/> has return type <see cref="Void"/></li>
		///		<li><paramref name="methodInfo"/> has unsafe parameter types</li>
		///		<li><paramref name="methodInfo"/> has dynamical argument lists (e.g. params modifier)</li>
		///		<li><paramref name="methodInfo"/> has out or ref parameters</li>
		/// </ul>
		/// </remarks>
		/// <param name="methodInfo">The method info to check.</param>
		public static bool IsInvocable(MethodInfo methodInfo)
		{
			if (methodInfo == null)
				throw ExceptionBuilder.ArgumentNull("methodInfo");

			if (methodInfo.IsSpecialName || 
				methodInfo.IsAbstract || 
				methodInfo.ReturnType == typeof(void) ||
				methodInfo.ReturnType.IsPointer ||
				(methodInfo.CallingConvention & CallingConventions.VarArgs) == CallingConventions.VarArgs)
				return false;
				
			foreach (ParameterInfo parameterInfo in methodInfo.GetParameters())
			{
				bool hasParamsModifier = parameterInfo.GetCustomAttributes(typeof (ParamArrayAttribute), false).Length > 0;
					
				if (hasParamsModifier ||
					parameterInfo.IsOut || 
					parameterInfo.ParameterType.IsByRef ||
					parameterInfo.ParameterType.IsPointer)
				{
					return false;
				}
			}
			
			return true;
		}
		
		public MethodBinding[] GetMethods(Type type)
		{
			if (type == null)
				throw ExceptionBuilder.ArgumentNull("type");

			MethodTable methodTable = new MethodTable();
			List<MethodBinding> methodList = new List<MethodBinding>();

			MethodInfo[] methodInfos = type.GetMethods(_bindingFlags);
			
			foreach (MethodInfo currentMethodInfo in methodInfos)
			{
				if (IsInvocable(currentMethodInfo))
				{
					MethodBinding methodBinding = CreateMethod(currentMethodInfo);

					if (methodBinding != null)
						AddMethod(methodTable, methodList, type, methodBinding, currentMethodInfo);
				}
			}

			return methodList.ToArray();
		}

		/// <summary>
		/// Creates a method binding for the given <see cref="MethodInfo"/>.
		/// </summary>
		/// <param name="methodInfo">The .NET framework property info.</param>
		/// <returns>If the property should not be visible this method returns <see langword="null"/>.</returns>
		protected virtual MethodBinding CreateMethod(MethodInfo methodInfo)
		{
			return new ReflectionMethodBinding(methodInfo, methodInfo.Name);
		}

		/// <summary>
		/// Creates a property binding for the given <see cref="PropertyInfo"/>.
		/// </summary>
		/// <param name="propertyInfo">The .NET framework property info.</param>
		/// <returns>If the property should not be visible this method returns <see langword="null"/>.</returns>
		protected virtual PropertyBinding CreateProperty(PropertyInfo propertyInfo)
		{
			return new ReflectionPropertyBinding(propertyInfo, propertyInfo.Name);
		}

		/// <summary>
		/// Creates a property binding for the given <see cref="FieldInfo"/>.
		/// </summary>
		/// <param name="fieldInfo">The .NET framework field info.</param>
		/// <returns>If the field should not be visible this method returns <see langword="null"/>.</returns>
		protected virtual PropertyBinding CreateProperty(FieldInfo fieldInfo)
		{
			return new ReflectionFieldBinding(fieldInfo, fieldInfo.Name);
		}
	}	
}