// NOTE: The SAVE_ASSEMBLY directive is used to write the compiled expressions (using regular System.Reflection.Emit
//       means, i.e. the *Builder classes) as an assembly on hard disk.
//       Normally, we are using the new lightweight code generation (LCG) feature of .NET 2.0 (DynamicMethod).

// #define SAVE_ASSEMBLY

using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace NQuery.Compilation
{
	internal sealed class ILEmitContext
	{
		private string _source;
		private Dictionary<AstNode, List<ILParameterDeclaration>> _parameterDictionary = new Dictionary<AstNode, List<ILParameterDeclaration>>();
		private List<ILParameterDeclaration> _parameters = new List<ILParameterDeclaration>();
		private DynamicMethod _dynamicMethod;
		private ILGenerator _ilGenerator;
#if SAVE_ASSEMBLY
		private static AssemblyBuilder _assemblyBuilder;
		private static ModuleBuilder _moduleBuilder;
		private static int _typeCount;
		private TypeBuilder _typeBuilder;
		private MethodBuilder _methodBuilder;
#endif

		public ILEmitContext(string source)
		{
			_source = source;
		}

		public void AddParameter(AstNode target, object value, Type type)
		{
			ILParameterDeclaration ilParameterDeclaration = new ILParameterDeclaration();
			ilParameterDeclaration.Index = _parameters.Count;
			ilParameterDeclaration.Value = value;
			ilParameterDeclaration.Type = type;
			_parameters.Add(ilParameterDeclaration);

			List<ILParameterDeclaration> targetParameters;
			if (!_parameterDictionary.TryGetValue(target, out targetParameters))
			{
				targetParameters = new List<ILParameterDeclaration>();
				_parameterDictionary.Add(target, targetParameters);
			}

			targetParameters.Add(ilParameterDeclaration);
		}

		public ILParameterDeclaration[] GetParameters(AstNode target)
		{
			return _parameterDictionary[target].ToArray();
		}

		public object[] GetArguments()
		{
			List<object> arguments = new List<object>();
			foreach (ILParameterDeclaration parameter in _parameters)
				arguments.Add(parameter.Value);
			return arguments.ToArray();
		}

		public void CreateDynamicMethod()
		{
#if SAVE_ASSEMBLY
			if (_assemblyBuilder == null)
			{
				AssemblyName assemblyName = new AssemblyName("ExpressionAssembly");
				_assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave, "I:\\Trash");
				_moduleBuilder = _assemblyBuilder.DefineDynamicModule("ExpressionModule", "ExpressionModule.module");
			}

			string typeName = String.Format("Expression{0}", _typeCount);
			_typeCount++;

			_typeBuilder = _moduleBuilder.DefineType(typeName, TypeAttributes.Class | TypeAttributes.Public);
			
			FieldBuilder filedBuilder = _typeBuilder.DefineField("Source", typeof(string), FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.Literal);
			filedBuilder.SetConstant(_source);

			_methodBuilder = _typeBuilder.DefineMethod("Evaluate", MethodAttributes.Public | MethodAttributes.Static, typeof(object), new Type[] { typeof(object[]) });
			_ilGenerator = _methodBuilder.GetILGenerator();
#else
			_dynamicMethod = new DynamicMethod("Expression", typeof(object), new Type[] { typeof(object[]) }, GetType().Module);
			_ilGenerator = _dynamicMethod.GetILGenerator();
#endif
		}

#if SAVE_ASSEMBLY
		private static object Placeholder(object[] args)
		{
			return null;
		}
#endif

		public CompiledExpressionDelegate CreateDelegate()
		{
			_ilGenerator.Emit(OpCodes.Ret);
#if SAVE_ASSEMBLY
			_typeBuilder.CreateType();
			return Placeholder;
#else
			return (CompiledExpressionDelegate)_dynamicMethod.CreateDelegate(typeof(CompiledExpressionDelegate));
#endif
		}

		public static void CompleteILCompilation()
		{
#if SAVE_ASSEMBLY
			_assemblyBuilder.Save("ExpressionAssembly.dll");
			throw new Exception("Assembly for debugging purposes saved");
#endif
		}

		public ILGenerator ILGenerator
		{
			get { return _ilGenerator; }
		}

		public override string ToString()
		{
			return _source;
		}
	}
}