using System;
using System.Diagnostics.CodeAnalysis;

using NQuery.Compilation;

#region Code Analysis Supressions

[assembly: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "NQuery.Compilation.UnaryOperator.#Identity")]
[assembly: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "NQuery.Compilation.UnaryOperator.#Negation")]
[assembly: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "NQuery.Compilation.UnaryOperator.#Complement")]
[assembly: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "NQuery.Compilation.UnaryOperator.#LogicalNot")]

#endregion

namespace NQuery.Compilation
{
	[Flags]
	internal enum UnaryOperatorAttributes
	{
		None                =   0x0000,
		Overloadable		=	0x0001
	}

	internal sealed class UnaryOperator : Operator
	{
		private UnaryOperatorAttributes _attributes;

		private UnaryOperator(int precedence, string tokenText, string methodName, UnaryOperatorAttributes attributes)
			: base(precedence, tokenText, methodName)
		{
			_attributes = attributes;
		}

		public override bool IsOverloadable
		{
			get { return (_attributes & UnaryOperatorAttributes.Overloadable) == UnaryOperatorAttributes.Overloadable; }
		}

		public override bool IsRightAssociative
		{
			get { return false; }
		}

		//                                                                          ------+------------+-------------------------+------------------------------------------------------------------------------
		//                                                                          Prec  | TokenText  | Operator Method         | Attributes   
		//                                                                          ------+------------+-------------------------+------------------------------------------------------------------------------
		public static readonly UnaryOperator  Identity         =  new UnaryOperator(    9,  "+",         "op_UnaryPlus",          UnaryOperatorAttributes.Overloadable);
		public static readonly UnaryOperator  Negation         =  new UnaryOperator(    9,  "-",         "op_UnaryNegation",      UnaryOperatorAttributes.Overloadable);
		public static readonly UnaryOperator  Complement       =  new UnaryOperator(    9,  "~",         "op_OnesComplement",     UnaryOperatorAttributes.Overloadable);

		public static readonly UnaryOperator  LogicalNot       =  new UnaryOperator(    4,  "NOT",       "op_Logicalnot",         UnaryOperatorAttributes.Overloadable);

	}
}