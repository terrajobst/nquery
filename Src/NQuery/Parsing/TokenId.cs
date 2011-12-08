using System.Diagnostics.CodeAnalysis;

#region Code Analysis Suppressions

[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "AND", Scope = "member", Target = "NQuery.Compilation.TokenId.#AND")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "OR", Scope = "member", Target = "NQuery.Compilation.TokenId.#OR")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "IS", Scope = "member", Target = "NQuery.Compilation.TokenId.#IS")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "NULL", Scope = "member", Target = "NQuery.Compilation.TokenId.#NULL")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "NOT", Scope = "member", Target = "NQuery.Compilation.TokenId.#NOT")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "LIKE", Scope = "member", Target = "NQuery.Compilation.TokenId.#LIKE")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "SOUNDSLIKE", Scope = "member", Target = "NQuery.Compilation.TokenId.#SOUNDSLIKE")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "SIMILAR", Scope = "member", Target = "NQuery.Compilation.TokenId.#SIMILAR")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "BETWEEN", Scope = "member", Target = "NQuery.Compilation.TokenId.#BETWEEN")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "IN", Scope = "member", Target = "NQuery.Compilation.TokenId.#IN")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "CAST", Scope = "member", Target = "NQuery.Compilation.TokenId.#CAST")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "AS", Scope = "member", Target = "NQuery.Compilation.TokenId.#AS")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "COALESCE", Scope = "member", Target = "NQuery.Compilation.TokenId.#COALESCE")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "NULLIF", Scope = "member", Target = "NQuery.Compilation.TokenId.#NULLIF")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "CASE", Scope = "member", Target = "NQuery.Compilation.TokenId.#CASE")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "WHEN", Scope = "member", Target = "NQuery.Compilation.TokenId.#WHEN")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "THEN", Scope = "member", Target = "NQuery.Compilation.TokenId.#THEN")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ELSE", Scope = "member", Target = "NQuery.Compilation.TokenId.#ELSE")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "END", Scope = "member", Target = "NQuery.Compilation.TokenId.#END")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "TRUE", Scope = "member", Target = "NQuery.Compilation.TokenId.#TRUE")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "FALSE", Scope = "member", Target = "NQuery.Compilation.TokenId.#FALSE")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "TO", Scope = "member", Target = "NQuery.Compilation.TokenId.#TO")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "SELECT", Scope = "member", Target = "NQuery.Compilation.TokenId.#SELECT")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "TOP", Scope = "member", Target = "NQuery.Compilation.TokenId.#TOP")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "DISTINCT", Scope = "member", Target = "NQuery.Compilation.TokenId.#DISTINCT")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "FROM", Scope = "member", Target = "NQuery.Compilation.TokenId.#FROM")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "WHERE", Scope = "member", Target = "NQuery.Compilation.TokenId.#WHERE")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "GROUP", Scope = "member", Target = "NQuery.Compilation.TokenId.#GROUP")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "BY", Scope = "member", Target = "NQuery.Compilation.TokenId.#BY")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "HAVING", Scope = "member", Target = "NQuery.Compilation.TokenId.#HAVING")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ORDER", Scope = "member", Target = "NQuery.Compilation.TokenId.#ORDER")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ASC", Scope = "member", Target = "NQuery.Compilation.TokenId.#ASC")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "DESC", Scope = "member", Target = "NQuery.Compilation.TokenId.#DESC")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "UNION", Scope = "member", Target = "NQuery.Compilation.TokenId.#UNION")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ALL", Scope = "member", Target = "NQuery.Compilation.TokenId.#ALL")]		
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "INTERSECT", Scope = "member", Target = "NQuery.Compilation.TokenId.#INTERSECT")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "EXCEPT", Scope = "member", Target = "NQuery.Compilation.TokenId.#EXCEPT")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "EXISTS", Scope = "member", Target = "NQuery.Compilation.TokenId.#EXISTS")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ANY", Scope = "member", Target = "NQuery.Compilation.TokenId.#ANY")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "SOME", Scope = "member", Target = "NQuery.Compilation.TokenId.#SOME")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "JOIN", Scope = "member", Target = "NQuery.Compilation.TokenId.#JOIN")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "INNER", Scope = "member", Target = "NQuery.Compilation.TokenId.#INNER")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "CROSS", Scope = "member", Target = "NQuery.Compilation.TokenId.#CROSS")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "LEFT", Scope = "member", Target = "NQuery.Compilation.TokenId.#LEFT")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "RIGHT", Scope = "member", Target = "NQuery.Compilation.TokenId.#RIGHT")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "OUTER", Scope = "member", Target = "NQuery.Compilation.TokenId.#OUTER")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "FULL", Scope = "member", Target = "NQuery.Compilation.TokenId.#FULL")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ON", Scope = "member", Target = "NQuery.Compilation.TokenId.#ON")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "WITH", Scope = "member", Target = "NQuery.Compilation.TokenId.#WITH")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "TIES", Scope = "member", Target = "NQuery.Compilation.TokenId.#TIES")]

#endregion

namespace NQuery.Compilation
{
	internal enum TokenId
	{
		Unknown,
		Eof,

		Identifier,
		Number,
		String,
		Date,
		ParameterMarker,

		BitwiseNot,
		BitwiseAnd,
		BitwiseOr,
		BitwiseXor,
		LeftParentheses,
		RightParentheses,
		Plus,
		Minus,
		Multiply,
		Divide,
		Modulus,
		Power,
		Comma,
		Dot,
		Equals,
		Unequals,
		Less,
		LessOrEqual,
		Greater,
		GreaterOrEqual,
		NotLess,
		NotGreater,
		RightShift,
		LeftShift,

		AND,
		OR,
		IS,
		NULL,
		NOT,
		LIKE,
		SOUNDSLIKE,
		SIMILAR,
		BETWEEN,
		IN,
		CAST,
		AS,
		COALESCE,
		NULLIF,
		CASE,
		WHEN,
		THEN,
		ELSE,
		END,
		TRUE,
		FALSE,
		TO,

		SELECT,
		TOP,
		DISTINCT,
		FROM,
		WHERE,
		GROUP,
		BY,
		HAVING,
		ORDER,
		ASC,
		DESC,
		UNION,
		ALL,		
		INTERSECT,
		EXCEPT,
		EXISTS,
		ANY,
		SOME,
		JOIN,
		INNER,
		CROSS,
		LEFT,
		RIGHT,
		OUTER,
		FULL,
		ON,
		WITH,
		TIES
	}
}