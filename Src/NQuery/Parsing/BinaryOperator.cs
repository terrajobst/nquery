using System;
using System.Diagnostics.CodeAnalysis;

#region Code Analysis Supressions

[assembly: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "NQuery.Compilation.BinaryOperator.#Power")]
[assembly: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "NQuery.Compilation.BinaryOperator.#Multiply")]
[assembly: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "NQuery.Compilation.BinaryOperator.#Divide")]
[assembly: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "NQuery.Compilation.BinaryOperator.#Modulus")]
[assembly: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "NQuery.Compilation.BinaryOperator.#Add")]
[assembly: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "NQuery.Compilation.BinaryOperator.#Sub")]
[assembly: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "NQuery.Compilation.BinaryOperator.#Equal")]
[assembly: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "NQuery.Compilation.BinaryOperator.#NotEqual")]
[assembly: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "NQuery.Compilation.BinaryOperator.#Less")]
[assembly: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "NQuery.Compilation.BinaryOperator.#LessOrEqual")]
[assembly: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "NQuery.Compilation.BinaryOperator.#Greater")]
[assembly: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "NQuery.Compilation.BinaryOperator.#GreaterOrEqual")]
[assembly: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "NQuery.Compilation.BinaryOperator.#BitXor")]
[assembly: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "NQuery.Compilation.BinaryOperator.#BitAnd")]
[assembly: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "NQuery.Compilation.BinaryOperator.#BitOr")]
[assembly: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "NQuery.Compilation.BinaryOperator.#LeftShift")]
[assembly: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "NQuery.Compilation.BinaryOperator.#RightShift")]
[assembly: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "NQuery.Compilation.BinaryOperator.#In")]
[assembly: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "NQuery.Compilation.BinaryOperator.#Like")]
[assembly: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "NQuery.Compilation.BinaryOperator.#SimilarTo")]
[assembly: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "NQuery.Compilation.BinaryOperator.#Soundex")]
[assembly: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "NQuery.Compilation.BinaryOperator.#LogicalAnd")]
[assembly: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "NQuery.Compilation.BinaryOperator.#LogicalOr")]

[assembly: SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Scope = "member", Target = "NQuery.Compilation.BinaryOperator.#Soundex")]

#endregion

namespace NQuery.Compilation
{
	[Flags]
	internal enum BinaryOperatorAttributes
	{
		None                =   0x0000,
		Overloadable		=	0x0001,
		RightAssociative    =   0x0002,
		Commutative         =   0x0004
	}
	
	internal sealed class BinaryOperator : Operator
	{
		private BinaryOperatorAttributes _attributes;

		private BinaryOperator(int precedence, string tokenText, string methodName, BinaryOperatorAttributes attributes)
			: base(precedence, tokenText, methodName)
		{
			_attributes = attributes;				
		}

		public BinaryOperatorAttributes Attributes
		{
			get { return _attributes; }
		}

		public override bool IsRightAssociative
		{
			get { return (_attributes & BinaryOperatorAttributes.RightAssociative) == BinaryOperatorAttributes.RightAssociative; }
		}

		public bool IsCommutative
		{
			get { return (_attributes & BinaryOperatorAttributes.Commutative) == BinaryOperatorAttributes.Commutative; }
		}

		public override bool IsOverloadable
		{
			get { return (_attributes & BinaryOperatorAttributes.Overloadable) == BinaryOperatorAttributes.Overloadable; }
		}

		// -----+---------------------------------------------------------------------------------------------------
		// Prec |  Operators
		// -----+---------------------------------------------------------------------------------------------------
		//	10	| ** (power)
		//	 9	| + (identity)   - (negation)   ~ (bitwise NOT)
		//	 8	| * (multiply)   / (division)   % (modulus)
		//	 7	| + (Addition)   + (Concatenate)   - (Subtraction)
		//	 6	| =   <>   <   <=   >   >=   !<   !>
		//	 5	| ^ (bitwise exclusive OR)   & (bitwise AND)   | (bitwise OR)   << (left shift)   >> (right shift)
		//	 4	| NOT
		//	 3	| ALL, ANY, BETWEEN, IN, LIKE, SIMILAR TO, SOME, SOUNDEX
		//	 2	| AND
		//	 1	| OR
		//
		//
		//                                                                          ------+-------------+-------------------------+------------------------------------------------------------
		//                                                                          Prec  | TokenText   | Operator Method         | Attributes   
		//                                                                          ------+-------------+-------------------------+------------------------------------------------------------
		public static readonly BinaryOperator Power            = new BinaryOperator(   10,  "**",         "op_Power",               BinaryOperatorAttributes.None);

		public static readonly BinaryOperator Multiply         = new BinaryOperator(   8,  "*",          "op_Multiply",            BinaryOperatorAttributes.Overloadable | BinaryOperatorAttributes.Commutative);
		public static readonly BinaryOperator Divide           = new BinaryOperator(   8,  "/",          "op_Division",            BinaryOperatorAttributes.Overloadable);
		public static readonly BinaryOperator Modulus          = new BinaryOperator(   8,  "%",          "op_Modulus",             BinaryOperatorAttributes.Overloadable);

		public static readonly BinaryOperator Add              = new BinaryOperator(   7,  "+",          "op_Addition",            BinaryOperatorAttributes.Overloadable | BinaryOperatorAttributes.Commutative);
		public static readonly BinaryOperator Sub              = new BinaryOperator(   7,  "-",          "op_Subtraction",         BinaryOperatorAttributes.Overloadable);

		public static readonly BinaryOperator Equal            = new BinaryOperator(   6,  "=",          "op_Equality",            BinaryOperatorAttributes.Overloadable | BinaryOperatorAttributes.Commutative);
		public static readonly BinaryOperator NotEqual         = new BinaryOperator(   6,  "<>",         "op_Inequality",          BinaryOperatorAttributes.Overloadable | BinaryOperatorAttributes.Commutative);
		public static readonly BinaryOperator Less             = new BinaryOperator(   6,  "<",          "op_LessThan",            BinaryOperatorAttributes.Overloadable);
		public static readonly BinaryOperator LessOrEqual      = new BinaryOperator(   6,  "<=",         "op_LessThanOrEqual",     BinaryOperatorAttributes.Overloadable);
		public static readonly BinaryOperator Greater          = new BinaryOperator(   6,  ">",          "op_GreaterThan",         BinaryOperatorAttributes.Overloadable);
		public static readonly BinaryOperator GreaterOrEqual   = new BinaryOperator(   6,  ">=",         "op_GreaterThanOrEqual",  BinaryOperatorAttributes.Overloadable);

		public static readonly BinaryOperator BitXor           = new BinaryOperator(   5,  "^",          "op_ExclusiveOr",         BinaryOperatorAttributes.Overloadable | BinaryOperatorAttributes.Commutative);
		public static readonly BinaryOperator BitAnd           = new BinaryOperator(   5,  "&",          "op_BitwiseAnd",          BinaryOperatorAttributes.Overloadable | BinaryOperatorAttributes.Commutative);
		public static readonly BinaryOperator BitOr            = new BinaryOperator(   5,  "|",          "op_BitwiseOr",           BinaryOperatorAttributes.Overloadable | BinaryOperatorAttributes.Commutative);
		public static readonly BinaryOperator LeftShift        = new BinaryOperator(   5,  "<<",         "op_LeftShift",           BinaryOperatorAttributes.Overloadable);
		public static readonly BinaryOperator RightShift       = new BinaryOperator(   5,  ">>",         "op_RightShift",          BinaryOperatorAttributes.Overloadable);

		public static readonly BinaryOperator In               = new BinaryOperator(   3,  "IN",         "op_In",                  BinaryOperatorAttributes.None);
		public static readonly BinaryOperator Like             = new BinaryOperator(   3,  "LIKE",       "op_Like",                BinaryOperatorAttributes.None);
		public static readonly BinaryOperator SimilarTo        = new BinaryOperator(   3,  "SIMILAR TO", "op_SimilarTo",           BinaryOperatorAttributes.None);
		public static readonly BinaryOperator Soundex          = new BinaryOperator(   3,  "SOUNDSLIKE", "op_SoundsLike",          BinaryOperatorAttributes.Commutative);

		public static readonly BinaryOperator LogicalAnd       = new BinaryOperator(   2,  "AND",        "op_LogicalAnd",          BinaryOperatorAttributes.Overloadable | BinaryOperatorAttributes.Commutative);
		public static readonly BinaryOperator LogicalOr        = new BinaryOperator(   1,  "OR",         "op_LogicalOr",           BinaryOperatorAttributes.Overloadable | BinaryOperatorAttributes.Commutative);
	}
}