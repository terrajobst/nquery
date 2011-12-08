using System;
using System.Reflection;
using System.Reflection.Emit;

using NQuery.Runtime;

namespace NQuery.Compilation
{
	internal sealed class ILTranslator : StandardVisitor
	{
		private ILEmitContext _ilEmitContext;
		private static readonly MethodInfo _propertyBindingGetValueMethod = typeof(PropertyBinding).GetMethod("GetValue", new Type[] { typeof(object) });
		private static readonly MethodInfo _parameterBindingGetValueMethod = typeof (ParameterBinding).GetProperty("Value").GetGetMethod();
		private static readonly MethodInfo _functionBindingInvokeMethod = typeof (FunctionBinding).GetMethod("Invoke", new Type[] {typeof (object[])});
		private static readonly MethodInfo _methodBindingInvokeMethod = typeof (MethodBinding).GetMethod("Invoke", new Type[] {typeof (object), typeof (object[])});
		private static readonly MethodInfo _unifyNullsMethod = typeof(NullHelper).GetMethod("UnifyNullRepresentation", new Type[] { typeof(object) });

		public ILTranslator(ILEmitContext ilEmitContext)
		{
			_ilEmitContext = ilEmitContext;
			_ilEmitContext.CreateDynamicMethod();
		}

		#region Helpers

		private int DeclareLocal()
		{
			LocalBuilder localBuilder = _ilEmitContext.ILGenerator.DeclareLocal(typeof(object));
			return localBuilder.LocalIndex;
		}

		private void EmitConversion(Type targetType)
		{
			if (targetType.IsValueType)
				_ilEmitContext.ILGenerator.Emit(OpCodes.Unbox_Any, targetType);
			else
				_ilEmitContext.ILGenerator.Emit(OpCodes.Castclass, targetType);
		}

		private void EmitLoadParameter(ILParameterDeclaration ilParameterDeclaration)
		{
			_ilEmitContext.ILGenerator.Emit(OpCodes.Ldarg_0);
			_ilEmitContext.ILGenerator.Emit(OpCodes.Ldc_I4, ilParameterDeclaration.Index);
			_ilEmitContext.ILGenerator.Emit(OpCodes.Ldelem_Ref);
		}

		private void EmitCall(MethodInfo method, ExpressionNode instance, params ExpressionNode[] args)
		{
			ParameterInfo[] parameterInfos = method.GetParameters();
			int instanceLocalIndex = -1;

			if (instance != null)
				instanceLocalIndex = DeclareLocal();

			int[] argLocalIndexes = new int[args.Length];

			for (int i = 0; i < args.Length; i++)
				argLocalIndexes[i] = DeclareLocal();

			if (instance != null)
			{
				Visit(instance);
				_ilEmitContext.ILGenerator.Emit(OpCodes.Stloc, instanceLocalIndex);
			}

			for (int i = 0; i < args.Length; i++)
			{
				Visit(args[i]);
				_ilEmitContext.ILGenerator.Emit(OpCodes.Stloc, argLocalIndexes[i]);
			}

			Label loadNullLabel = _ilEmitContext.ILGenerator.DefineLabel();
			Label finishLabel = _ilEmitContext.ILGenerator.DefineLabel();

			if (instance != null)
			{
				_ilEmitContext.ILGenerator.Emit(OpCodes.Ldloc, instanceLocalIndex);
				_ilEmitContext.ILGenerator.Emit(OpCodes.Brfalse, loadNullLabel);
			}

			for (int i = 0; i < args.Length; i++)
			{
				_ilEmitContext.ILGenerator.Emit(OpCodes.Ldloc, argLocalIndexes[i]);
				_ilEmitContext.ILGenerator.Emit(OpCodes.Brfalse, loadNullLabel);
			}

			if (instance != null)
			{
				_ilEmitContext.ILGenerator.Emit(OpCodes.Ldloc, instanceLocalIndex);
				EmitThisArgumentPointer(instance.ExpressionType);
			}

			for (int i = 0; i < args.Length; i++)
			{
				_ilEmitContext.ILGenerator.Emit(OpCodes.Ldloc, argLocalIndexes[i]);
				if (parameterInfos[i].ParameterType != typeof(object))
					EmitConversion(parameterInfos[i].ParameterType);
			}

			EmitRawCall(method, instance == null ? null : instance.ExpressionType);

			if (method.ReturnType.IsValueType)
				_ilEmitContext.ILGenerator.Emit(OpCodes.Box, method.ReturnType);
			_ilEmitContext.ILGenerator.Emit(OpCodes.Br, finishLabel);

			_ilEmitContext.ILGenerator.MarkLabel(loadNullLabel);
			_ilEmitContext.ILGenerator.Emit(OpCodes.Ldnull);

			_ilEmitContext.ILGenerator.MarkLabel(finishLabel);
		}

		private void EmitRawCall(MethodInfo method, Type instanceType)
		{
			if (!method.IsAbstract && !method.IsVirtual)
			{
				// Neither abstract nor virtual, make a simple call.
				_ilEmitContext.ILGenerator.EmitCall(OpCodes.Call, method, null);
			}
			else if (instanceType == null || !instanceType.IsValueType)
			{
				// Not a value type, but we must emit a virtual call.
				_ilEmitContext.ILGenerator.EmitCall(OpCodes.Callvirt, method, null);
			}
			else if (method.DeclaringType == instanceType)
			{
				// It is a value type but the declaring type is already the
				// expression type. Since there cannot be a derivative
				// we can safely emit a simple call.
				_ilEmitContext.ILGenerator.EmitCall(OpCodes.Call, method, null);
			}
			else
			{
				// It is a value type but the declaring type is not the
				// expression type. In order to perform a virtual call we
				// must box the value.
				MethodInfo baseDefinition = method.GetBaseDefinition();
				_ilEmitContext.ILGenerator.Emit(OpCodes.Ldobj, instanceType);
				_ilEmitContext.ILGenerator.Emit(OpCodes.Box, instanceType);
				_ilEmitContext.ILGenerator.EmitCall(OpCodes.Callvirt, baseDefinition, null);
			}
		}

		private void EmitThisArgumentPointer(Type instanceType)
		{
			if (!instanceType.IsValueType)
			{
				EmitConversion(instanceType);
			}
			else
			{
				int castedInstanceLocal = _ilEmitContext.ILGenerator.DeclareLocal(instanceType).LocalIndex;
				EmitConversion(instanceType);
				_ilEmitContext.ILGenerator.Emit(OpCodes.Stloc, castedInstanceLocal);
				_ilEmitContext.ILGenerator.Emit(OpCodes.Ldloca, castedInstanceLocal);
			}
		}

		#endregion

		public override AstNode Visit(AstNode node)
		{
			switch (node.NodeType)
			{
				case AstNodeType.Literal:
				case AstNodeType.UnaryExpression:
				case AstNodeType.BinaryExpression:
				case AstNodeType.IsNullExpression:
				case AstNodeType.CastExpression:
				case AstNodeType.CaseExpression:
				case AstNodeType.NullIfExpression:
				case AstNodeType.ParameterExpression:
				case AstNodeType.PropertyAccessExpression:
				case AstNodeType.FunctionInvocationExpression:
				case AstNodeType.MethodInvocationExpression:
				case AstNodeType.RowBufferEntryExpression:
					return base.Visit(node);

				default:
					throw ExceptionBuilder.UnhandledCaseLabel(node.NodeType);
			}
		}

		public override LiteralExpression VisitLiteralValue(LiteralExpression expression)
		{
			ILParameterDeclaration ilParameterDeclaration = _ilEmitContext.GetParameters(expression)[0];
			EmitLoadParameter(ilParameterDeclaration);

			return expression;
		}

		public override ExpressionNode VisitUnaryExpression(UnaryExpression expression)
		{
			EmitCall(expression.OperatorMethod, null, expression.Operand);
			return expression;
		}

		public override ExpressionNode VisitBinaryExpression(BinaryExpression expression)
		{
			bool isConjuctionOrDisjunction = expression.Op == BinaryOperator.LogicalAnd ||
			                                 expression.Op == BinaryOperator.LogicalOr;
			if (!isConjuctionOrDisjunction)
			{
				EmitCall(expression.OperatorMethod, null, expression.Left, expression.Right);
			}
			else
			{
				// He expression is a logical OR or a logical AND. In this case we have to pay
				// special attention to NULL values and short circuit evaluation.
				// Normally, a binary expression will yield NULL if any of the operands is NULL 
				// (as in function calls).
				//
				// For conjuctions and disjunctions this is not true. Sometimes these boolean
				// operators will return TRUE or FALSE though an operand was null. The following 
				// truth table must hold:
				//
				//    AND | F | T | N        OR | F | T | N
				//    ----+---+---+--        ---+---+---+--
				//    F   | F | F | F        F  | F | T | N
				//    T   | F | T | N        T  | T | T | T
				//    N   | F | N | N        N  | N | T | N
				//
				// In short, these exceptions exist:
				//
				// NULL AND FALSE ---> FALSE
				// FALSE AND NULL ---> FALSE
				//
				// NULL OR TRUE ---> TRUE
				// TRUE OR NULL ---> TRUE
				//
				// In addition we want to support short circuit evaluation.

				int left = DeclareLocal();
				int right = DeclareLocal();
				Label finish = _ilEmitContext.ILGenerator.DefineLabel();

				if (expression.Op == BinaryOperator.LogicalAnd)
				{
					// Peseudo code:
					//
					//	object left = expression.Left.Evaluate();
					//
					//	if (left != null && (bool)left == false)
					//		return false;
					//	else
					//	{
					//		object right = expression.Right.Evaluate();
					//
					//		if (right != null && (bool)right == false)
					//			return false;
					//
					//		if (rigth == null || left == null)
					//			return null;
					//
					//		return (bool)left & (bool)right;
					//	}

					Label checkRightSide = _ilEmitContext.ILGenerator.DefineLabel();
					Label checkAnyNull = _ilEmitContext.ILGenerator.DefineLabel();
					Label returnNull = _ilEmitContext.ILGenerator.DefineLabel();
					Label checkBothTrue = _ilEmitContext.ILGenerator.DefineLabel();

					VisitExpression(expression.Left);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Stloc, left);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Ldloc, left);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Brfalse, checkRightSide);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Ldloc, left);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Unbox_Any, typeof(Boolean));
					_ilEmitContext.ILGenerator.Emit(OpCodes.Brtrue, checkRightSide);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Ldc_I4_0);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Box, typeof(Boolean));
					_ilEmitContext.ILGenerator.Emit(OpCodes.Br, finish);

					_ilEmitContext.ILGenerator.MarkLabel(checkRightSide);					
					VisitExpression(expression.Right);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Stloc, right);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Ldloc, right);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Brfalse, checkAnyNull);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Ldloc, right);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Unbox_Any, typeof(Boolean));
					_ilEmitContext.ILGenerator.Emit(OpCodes.Brtrue, checkAnyNull);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Ldc_I4_0);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Box, typeof(Boolean));
					_ilEmitContext.ILGenerator.Emit(OpCodes.Br, finish);

					_ilEmitContext.ILGenerator.MarkLabel(checkAnyNull);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Ldloc, left);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Brfalse, returnNull);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Ldloc, right);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Brtrue, checkBothTrue);
					_ilEmitContext.ILGenerator.MarkLabel(returnNull);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Ldnull);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Br, finish);

					_ilEmitContext.ILGenerator.MarkLabel(checkBothTrue);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Ldloc, left);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Unbox_Any, typeof(Boolean));
					_ilEmitContext.ILGenerator.Emit(OpCodes.Ldloc, right);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Unbox_Any, typeof(Boolean));
					_ilEmitContext.ILGenerator.Emit(OpCodes.And);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Box, typeof(Boolean));
					_ilEmitContext.ILGenerator.Emit(OpCodes.Br, finish);
				}
				else
				{
					// Peseudo code:
					//
					//	object left = expression.Left.Evaluate();
					//
					//	if (left != null && (bool)left == true)
					//		return true;
					//	else
					//	{
					//		object right = expression.Right.Evaluate();
					//
					//		if (right != null && (bool)right == true)
					//			return true;
					//
					//		if (rigth == null || left == null)
					//			return null;
					//
					//		return (bool)left | (bool)right;
					//	}

					Label checkRightSide = _ilEmitContext.ILGenerator.DefineLabel();
					Label checkAnyNull = _ilEmitContext.ILGenerator.DefineLabel();
					Label returnNull = _ilEmitContext.ILGenerator.DefineLabel();
					Label checkAnyTrue = _ilEmitContext.ILGenerator.DefineLabel();

					VisitExpression(expression.Left);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Stloc, left);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Ldloc, left);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Brfalse, checkRightSide);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Ldloc, left);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Unbox_Any, typeof(Boolean));
					_ilEmitContext.ILGenerator.Emit(OpCodes.Brfalse, checkRightSide);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Ldc_I4_1);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Box, typeof(Boolean));
					_ilEmitContext.ILGenerator.Emit(OpCodes.Br, finish);

					_ilEmitContext.ILGenerator.MarkLabel(checkRightSide);
					VisitExpression(expression.Right);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Stloc, right);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Ldloc, right);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Brfalse, checkAnyNull);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Ldloc, right);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Unbox_Any, typeof(Boolean));
					_ilEmitContext.ILGenerator.Emit(OpCodes.Brfalse, checkAnyNull);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Ldc_I4_1);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Box, typeof(Boolean));
					_ilEmitContext.ILGenerator.Emit(OpCodes.Br, finish);

					_ilEmitContext.ILGenerator.MarkLabel(checkAnyNull);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Ldloc, left);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Brfalse, returnNull);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Ldloc, right);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Brtrue, checkAnyTrue);
					_ilEmitContext.ILGenerator.MarkLabel(returnNull);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Ldnull);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Br, finish);

					_ilEmitContext.ILGenerator.MarkLabel(checkAnyTrue);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Ldloc, left);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Unbox_Any, typeof(Boolean));
					_ilEmitContext.ILGenerator.Emit(OpCodes.Ldloc, right);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Unbox_Any, typeof(Boolean));
					_ilEmitContext.ILGenerator.Emit(OpCodes.Or);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Box, typeof(Boolean));
					_ilEmitContext.ILGenerator.Emit(OpCodes.Br, finish);
				}

				_ilEmitContext.ILGenerator.MarkLabel(finish);
			}

			return expression;
		}

		public override ExpressionNode VisitIsNullExpression(IsNullExpression expression)
		{
			Visit(expression.Expression);

			Label loadTrue = _ilEmitContext.ILGenerator.DefineLabel();
			Label finishLabel = _ilEmitContext.ILGenerator.DefineLabel();

			if (expression.Negated)
				_ilEmitContext.ILGenerator.Emit(OpCodes.Brtrue, loadTrue);
			else
				_ilEmitContext.ILGenerator.Emit(OpCodes.Brfalse, loadTrue);

			_ilEmitContext.ILGenerator.Emit(OpCodes.Ldc_I4_0);
			_ilEmitContext.ILGenerator.Emit(OpCodes.Br, finishLabel);

			_ilEmitContext.ILGenerator.MarkLabel(loadTrue);
			_ilEmitContext.ILGenerator.Emit(OpCodes.Ldc_I4_1);

			_ilEmitContext.ILGenerator.MarkLabel(finishLabel);
			_ilEmitContext.ILGenerator.Emit(OpCodes.Box, typeof(bool));

			return expression;
		}

		public override ExpressionNode VisitCastExpression(CastExpression expression)
		{
			if (expression.ConvertMethod != null)
			{
				// This is a cast using a conversion method
				EmitCall(expression.ConvertMethod, null, expression.Expression);
			}
			else
			{
				// It is a regular downcast.
				// Nothing to do.
				Visit(expression.Expression);
			}

			return expression;
		}

		public override ExpressionNode VisitCaseExpression(CaseExpression expression)
		{
			// NOTE: Must be searched CASE. The normalizer should have replaced it.

			Label finishLabel = _ilEmitContext.ILGenerator.DefineLabel();

			Label[] caseLabels = new Label[expression.WhenExpressions.Length + 1];
			for (int i = 0; i < expression.WhenExpressions.Length; i++)
				caseLabels[i] = _ilEmitContext.ILGenerator.DefineLabel();

			Label elseLabel = _ilEmitContext.ILGenerator.DefineLabel();
			caseLabels[expression.WhenExpressions.Length] = elseLabel;

			for (int i = 0; i < expression.WhenExpressions.Length; i++)
			{
				_ilEmitContext.ILGenerator.MarkLabel(caseLabels[i]);

				int whenLocalIndex = DeclareLocal();
				Visit(expression.WhenExpressions[i]);
				_ilEmitContext.ILGenerator.Emit(OpCodes.Stloc, whenLocalIndex);
				_ilEmitContext.ILGenerator.Emit(OpCodes.Ldloc, whenLocalIndex);
				_ilEmitContext.ILGenerator.Emit(OpCodes.Brfalse, caseLabels[i + 1]);
				_ilEmitContext.ILGenerator.Emit(OpCodes.Ldloc, whenLocalIndex);
				_ilEmitContext.ILGenerator.Emit(OpCodes.Unbox_Any, typeof (bool));
				_ilEmitContext.ILGenerator.Emit(OpCodes.Brfalse, caseLabels[i + 1]);

				Visit(expression.ThenExpressions[i]);
				_ilEmitContext.ILGenerator.Emit(OpCodes.Br, finishLabel);
			}

			_ilEmitContext.ILGenerator.MarkLabel(elseLabel);
			if (expression.ElseExpression != null)
				Visit(expression.ElseExpression);
			else
				_ilEmitContext.ILGenerator.Emit(OpCodes.Ldnull);

			_ilEmitContext.ILGenerator.MarkLabel(finishLabel);

			return expression;
		}

		public override ExpressionNode VisitParameterExpression(ParameterExpression expression)
		{
			ILParameterDeclaration parameterBinding = _ilEmitContext.GetParameters(expression)[0];
			EmitLoadParameter(parameterBinding);
			EmitThisArgumentPointer(typeof(ParameterBinding));
			_ilEmitContext.ILGenerator.EmitCall(OpCodes.Call, _parameterBindingGetValueMethod, null);

			return expression;
		}

		public override ExpressionNode VisitPropertyAccessExpression(PropertyAccessExpression expression)
		{
			ReflectionPropertyBinding reflectionPropertyBinding = expression.Property as ReflectionPropertyBinding;
			ReflectionFieldBinding reflectionFieldBinding = expression.Property as ReflectionFieldBinding;
			if (reflectionPropertyBinding != null)
			{
				EmitCall(reflectionPropertyBinding.PropertyInfo.GetGetMethod(), expression.Target);
				_ilEmitContext.ILGenerator.EmitCall(OpCodes.Call, _unifyNullsMethod, null);
			}
			else if (reflectionFieldBinding != null)
			{
				Label finishLabel = _ilEmitContext.ILGenerator.DefineLabel();
				Label loadNullLabel = _ilEmitContext.ILGenerator.DefineLabel();
				int instanceIndex = DeclareLocal();

				Visit(expression.Target);
				_ilEmitContext.ILGenerator.Emit(OpCodes.Stloc, instanceIndex);
				_ilEmitContext.ILGenerator.Emit(OpCodes.Ldloc, instanceIndex);
				_ilEmitContext.ILGenerator.Emit(OpCodes.Brfalse, loadNullLabel);
				_ilEmitContext.ILGenerator.Emit(OpCodes.Ldloc, instanceIndex);
				EmitThisArgumentPointer(expression.Target.ExpressionType);
				_ilEmitContext.ILGenerator.Emit(OpCodes.Ldfld, reflectionFieldBinding.FieldInfo);
				_ilEmitContext.ILGenerator.Emit(OpCodes.Br, finishLabel);
				_ilEmitContext.ILGenerator.MarkLabel(loadNullLabel);
				_ilEmitContext.ILGenerator.Emit(OpCodes.Ldnull);
				_ilEmitContext.ILGenerator.MarkLabel(finishLabel);
			}
			else
			{
				Label finishLabel = _ilEmitContext.ILGenerator.DefineLabel();
				Label loadNullLabel = _ilEmitContext.ILGenerator.DefineLabel();

				int argIndex = DeclareLocal();
				Visit(expression.Target);
				_ilEmitContext.ILGenerator.Emit(OpCodes.Stloc, argIndex);
				_ilEmitContext.ILGenerator.Emit(OpCodes.Ldloc, argIndex);
				_ilEmitContext.ILGenerator.Emit(OpCodes.Brfalse, loadNullLabel);

				ILParameterDeclaration customPropertyBinding = _ilEmitContext.GetParameters(expression)[0];
				EmitLoadParameter(customPropertyBinding);
				EmitThisArgumentPointer(typeof(PropertyBinding));
				_ilEmitContext.ILGenerator.Emit(OpCodes.Ldloc, argIndex);
				_ilEmitContext.ILGenerator.EmitCall(OpCodes.Callvirt, _propertyBindingGetValueMethod, null);
				_ilEmitContext.ILGenerator.EmitCall(OpCodes.Call, _unifyNullsMethod, null);
				_ilEmitContext.ILGenerator.Emit(OpCodes.Br, finishLabel);

				_ilEmitContext.ILGenerator.MarkLabel(loadNullLabel);
				_ilEmitContext.ILGenerator.Emit(OpCodes.Ldnull);

				_ilEmitContext.ILGenerator.MarkLabel(finishLabel);
			}
			
			return expression;
		}

		public override ExpressionNode VisitFunctionInvocationExpression(FunctionInvocationExpression expression)
		{
			ReflectionFunctionBinding reflectionFunctionBinding = expression.Function as ReflectionFunctionBinding;
			if (reflectionFunctionBinding != null)
			{
				MethodInfo method = reflectionFunctionBinding.Method;
				ParameterInfo[] parameterInfos = method.GetParameters();
				ILParameterDeclaration instance;

				if (reflectionFunctionBinding.Instance != null)
					instance = _ilEmitContext.GetParameters(expression)[0];
				else
					instance = null;

				int[] argLocalIndexes = new int[expression.Arguments.Length];
				for (int i = 0; i < expression.Arguments.Length; i++)
					argLocalIndexes[i] = DeclareLocal();

				for (int i = 0; i < expression.Arguments.Length; i++)
				{
					Visit(expression.Arguments[i]);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Stloc, argLocalIndexes[i]);
				}

				Label loadNullLabel = _ilEmitContext.ILGenerator.DefineLabel();
				Label finishLabel = _ilEmitContext.ILGenerator.DefineLabel();

				for (int i = 0; i < expression.Arguments.Length; i++)
				{
					_ilEmitContext.ILGenerator.Emit(OpCodes.Ldloc, argLocalIndexes[i]);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Brfalse, loadNullLabel);
				}

				if (instance != null)
				{
					EmitLoadParameter(instance);
					EmitThisArgumentPointer(instance.Type);
				}

				for (int i = 0; i < expression.Arguments.Length; i++)
				{
					_ilEmitContext.ILGenerator.Emit(OpCodes.Ldloc, argLocalIndexes[i]);
					if (parameterInfos[i].ParameterType != typeof(object))
						EmitConversion(parameterInfos[i].ParameterType);
				}

				EmitRawCall(method, instance == null ? null : instance.Type);

				if (method.ReturnType.IsValueType)
					_ilEmitContext.ILGenerator.Emit(OpCodes.Box, method.ReturnType);

				_ilEmitContext.ILGenerator.EmitCall(OpCodes.Call, _unifyNullsMethod, null);
				_ilEmitContext.ILGenerator.Emit(OpCodes.Br, finishLabel);

				_ilEmitContext.ILGenerator.MarkLabel(loadNullLabel);
				_ilEmitContext.ILGenerator.Emit(OpCodes.Ldnull);

				_ilEmitContext.ILGenerator.MarkLabel(finishLabel);
			}
			else
			{
				int[] argLocalIndexes = new int[expression.Arguments.Length];

				for (int i = 0; i < expression.Arguments.Length; i++)
					argLocalIndexes[i] = DeclareLocal();

				for (int i = 0; i < expression.Arguments.Length; i++)
				{
					Visit(expression.Arguments[i]);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Stloc, argLocalIndexes[i]);
				}

				Label loadNullLabel = _ilEmitContext.ILGenerator.DefineLabel();
				Label finishLabel = _ilEmitContext.ILGenerator.DefineLabel();

				for (int i = 0; i < expression.Arguments.Length; i++)
				{
					_ilEmitContext.ILGenerator.Emit(OpCodes.Ldloc, argLocalIndexes[i]);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Brfalse, loadNullLabel);
				}

				ILParameterDeclaration customFunctionBinding = _ilEmitContext.GetParameters(expression)[0];
				ILParameterDeclaration argsArray = _ilEmitContext.GetParameters(expression)[1];

				int argsArrayIndex = DeclareLocal();
				EmitLoadParameter(argsArray);
				_ilEmitContext.ILGenerator.Emit(OpCodes.Stloc, argsArrayIndex);

				for (int i = 0; i < expression.Arguments.Length; i++)
				{
					_ilEmitContext.ILGenerator.Emit(OpCodes.Ldloc, argsArrayIndex);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Ldc_I4, i);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Ldloc, argLocalIndexes[i]);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Stelem_Ref);
				}

				EmitLoadParameter(customFunctionBinding);
				EmitThisArgumentPointer(typeof(FunctionBinding));
				_ilEmitContext.ILGenerator.Emit(OpCodes.Ldloc, argsArrayIndex);
				_ilEmitContext.ILGenerator.EmitCall(OpCodes.Callvirt, _functionBindingInvokeMethod, null);
				_ilEmitContext.ILGenerator.EmitCall(OpCodes.Call, _unifyNullsMethod, null);
				_ilEmitContext.ILGenerator.Emit(OpCodes.Br, finishLabel);

				_ilEmitContext.ILGenerator.MarkLabel(loadNullLabel);
				_ilEmitContext.ILGenerator.Emit(OpCodes.Ldnull);

				_ilEmitContext.ILGenerator.MarkLabel(finishLabel);
			}

			return expression;
		}

		public override ExpressionNode VisitMethodInvocationExpression(MethodInvocationExpression expression)
		{
			ReflectionMethodBinding reflectionMethodBinding = expression.Method as ReflectionMethodBinding;
			if (reflectionMethodBinding != null)
			{
				EmitCall(reflectionMethodBinding.Method, expression.Target, expression.Arguments);
				_ilEmitContext.ILGenerator.EmitCall(OpCodes.Call, _unifyNullsMethod, null);
			}
			else
			{
				int instanceArgIndex = DeclareLocal();
				int[] argLocalIndexes = new int[expression.Arguments.Length];

				for (int i = 0; i < expression.Arguments.Length; i++)
					argLocalIndexes[i] = DeclareLocal();

				Visit(expression.Target);
				_ilEmitContext.ILGenerator.Emit(OpCodes.Stloc, instanceArgIndex);

				for (int i = 0; i < expression.Arguments.Length; i++)
				{
					Visit(expression.Arguments[i]);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Stloc, argLocalIndexes[i]);
				}

				Label loadNullLabel = _ilEmitContext.ILGenerator.DefineLabel();
				Label finishLabel = _ilEmitContext.ILGenerator.DefineLabel();

				_ilEmitContext.ILGenerator.Emit(OpCodes.Ldloc, instanceArgIndex);
				_ilEmitContext.ILGenerator.Emit(OpCodes.Brfalse, loadNullLabel);

				for (int i = 0; i < expression.Arguments.Length; i++)
				{
					_ilEmitContext.ILGenerator.Emit(OpCodes.Ldloc, argLocalIndexes[i]);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Brfalse, loadNullLabel);
				}

				ILParameterDeclaration customMethodBinding = _ilEmitContext.GetParameters(expression)[0];
				ILParameterDeclaration argsArray = _ilEmitContext.GetParameters(expression)[1];

				int argsArrayIndex = DeclareLocal();
				EmitLoadParameter(argsArray);
				_ilEmitContext.ILGenerator.Emit(OpCodes.Stloc, argsArrayIndex);

				for (int i = 0; i < expression.Arguments.Length; i++)
				{
					_ilEmitContext.ILGenerator.Emit(OpCodes.Ldloc, argsArrayIndex);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Ldc_I4, i);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Ldloc, argLocalIndexes[i]);
					_ilEmitContext.ILGenerator.Emit(OpCodes.Stelem_Ref);
				}

				EmitLoadParameter(customMethodBinding);
				EmitThisArgumentPointer(typeof(MethodBinding));
				_ilEmitContext.ILGenerator.Emit(OpCodes.Ldloc, instanceArgIndex);
				_ilEmitContext.ILGenerator.Emit(OpCodes.Ldloc, argsArrayIndex);
				_ilEmitContext.ILGenerator.EmitCall(OpCodes.Callvirt, _methodBindingInvokeMethod, null);
				_ilEmitContext.ILGenerator.EmitCall(OpCodes.Call, _unifyNullsMethod, null);
				_ilEmitContext.ILGenerator.Emit(OpCodes.Br, finishLabel);

				_ilEmitContext.ILGenerator.MarkLabel(loadNullLabel);
				_ilEmitContext.ILGenerator.Emit(OpCodes.Ldnull);

				_ilEmitContext.ILGenerator.MarkLabel(finishLabel);
			}

			return expression;
		}

		public override ExpressionNode VisitRowBufferEntryExpression(RowBufferEntryExpression expression)
		{
			ILParameterDeclaration rowBufferParameter = _ilEmitContext.GetParameters(expression)[0];
			EmitLoadParameter(rowBufferParameter);
			_ilEmitContext.ILGenerator.Emit(OpCodes.Castclass, typeof (object[]));
			_ilEmitContext.ILGenerator.Emit(OpCodes.Ldc_I4, expression.RowBufferIndex);
			_ilEmitContext.ILGenerator.Emit(OpCodes.Ldelem_Ref);
			return expression;
		}
	}
}