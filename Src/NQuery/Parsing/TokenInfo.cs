using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace NQuery.Compilation
{
	internal sealed class TokenInfo
	{
		private string _text;
		private bool _isKeyword;
		private bool _isQueryKeyword;
		private TokenId _tokenId;
		private UnaryOperator _unaryOperator;
		private BinaryOperator _binaryOperator;
		private static Dictionary<TokenId, TokenInfo> _infosById = new Dictionary<TokenId, TokenInfo>();
		private static Dictionary<string, TokenInfo> _infosByText = new Dictionary<string, TokenInfo>(StringComparer.InvariantCultureIgnoreCase);

		private TokenInfo(TokenId tokenId, string text, bool isKeyword, bool isQueryKeyword, UnaryOperator unOp, BinaryOperator binOp)
		{
			_tokenId = tokenId;
			_text = text;
			_isKeyword = isKeyword;
			_isQueryKeyword = isQueryKeyword;
			_unaryOperator = unOp;
			_binaryOperator = binOp;
		}

		public string Text
		{
			get { return _text; }
		}

		public TokenId TokenId
		{
			get { return _tokenId; }
		}

		public bool IsKeyword
		{
			get { return _isKeyword; }
		}

		public bool IsQueryKeyword
		{
			get { return _isQueryKeyword; }
		}

		public UnaryOperator UnaryOperator
		{
			get { return _unaryOperator; }
		}

		public BinaryOperator BinaryOperator
		{
			get { return _binaryOperator; }
		}

		public bool HasDynamicText
		{
			get { return _tokenId == TokenId.Identifier || _tokenId == TokenId.Number || _tokenId == TokenId.String || _tokenId == TokenId.Date; }
		}

		private static void Add(TokenId tokenId, string text, bool isKeyword, bool isQueryKeyword, UnaryOperator unOp, BinaryOperator binOp)
		{
			TokenInfo info = new TokenInfo(tokenId, text, isKeyword, isQueryKeyword, unOp, binOp);

			_infosById.Add(tokenId, info);

			if (text != null)
				_infosByText.Add(text, info);
		}

		public static TokenInfo FromTokenId(TokenId tokenId)
		{
			return _infosById[tokenId];
		}

		public static TokenInfo FromText(string text)
		{
			if (!_infosByText.ContainsKey(text))
				return FromTokenId(TokenId.Identifier);
			else
				return _infosByText[text];
		}

		[SuppressMessage("Microsoft.Globalization", "CA1309:UseOrdinalStringComparison", MessageId = "System.Collections.Generic.Dictionary`2<System.String,NQuery.Compilation.TokenInfo>.#ctor(System.Collections.Generic.IEqualityComparer`1<System.String>)"), SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
		static TokenInfo()
		{
			//  --------------------------+----------------------------------------+--------+--------+---------------------------+--------------------------------
			//  TokenID                   | Token Text                             | KW     | Qry KW | Unary Operator            | Binary Operator
			//  --------------------------+----------------------------------------+--------+--------+---------------------------+--------------------------------

			Add(TokenId.Unknown,           Resources.TokenUnknown,                  false,   false,   null,                       null);
			Add(TokenId.Eof,               Resources.TokenEndOfFile,                false,   false,   null,                       null);
			Add(TokenId.Identifier,        Resources.TokenIdentifier,               false,   false,   null,                       null);
			Add(TokenId.Number,            Resources.TokenNumberLiteral,            false,   false,   null,                       null);
			Add(TokenId.String,            Resources.TokenStringLiteral,            false,   false,   null,                       null);
			Add(TokenId.Date,              Resources.TokenDateLiteral,              false,   false,   null,                       null);
			Add(TokenId.ParameterMarker,   "@",                                     false,   false,   null,                       null);

			Add(TokenId.BitwiseNot,        "~",                                     false,   false,   UnaryOperator.Complement,   null);
			Add(TokenId.BitwiseAnd,        "&",                                     false,   false,   null,                       BinaryOperator.BitAnd);
			Add(TokenId.BitwiseOr,         "|",                                     false,   false,   null,                       BinaryOperator.BitOr);
			Add(TokenId.BitwiseXor,        "^",                                     false,   false,   null,                       BinaryOperator.BitXor);
			Add(TokenId.LeftParentheses,   "(",                                     false,   false,   null,                       null);
			Add(TokenId.RightParentheses,  ")",                                     false,   false,   null,                       null);
			Add(TokenId.Plus,              "+",                                     false,   false,   UnaryOperator.Identity,     BinaryOperator.Add);
			Add(TokenId.Minus,             "-",                                     false,   false,   UnaryOperator.Negation,     BinaryOperator.Sub);
			Add(TokenId.Multiply,          "*",                                     false,   false,   null,                       BinaryOperator.Multiply);
			Add(TokenId.Divide,            "/",                                     false,   false,   null,                       BinaryOperator.Divide);
			Add(TokenId.Modulus,           "%",                                     false,   false,   null,                       BinaryOperator.Modulus);
			Add(TokenId.Power,             "**",                                    false,   false,   null,                       BinaryOperator.Power);
			Add(TokenId.Comma,             ",",                                     false,   false,   null,                       null);
			Add(TokenId.Dot,               ".",                                     false,   false,   null,                       null);
			Add(TokenId.Equals,            "=",                                     false,   false,   null,                       BinaryOperator.Equal);
			Add(TokenId.Unequals,          "<>",                                    false,   false,   null,                       BinaryOperator.NotEqual);
			Add(TokenId.Less,              "<",                                     false,   false,   null,                       BinaryOperator.Less);
			Add(TokenId.LessOrEqual,       "<=",                                    false,   false,   null,                       BinaryOperator.LessOrEqual);
			Add(TokenId.Greater,           ">",                                     false,   false,   null,                       BinaryOperator.Greater);
			Add(TokenId.GreaterOrEqual,    ">=",                                    false,   false,   null,                       BinaryOperator.GreaterOrEqual);
			Add(TokenId.NotLess,           "!<",                                    false,   false,   null,                       BinaryOperator.GreaterOrEqual);
			Add(TokenId.NotGreater,        "!>",                                    false,   false,   null,                       BinaryOperator.LessOrEqual);
			Add(TokenId.LeftShift,         "<<",                                    false,   false,   null,                       BinaryOperator.LeftShift);
			Add(TokenId.RightShift,        ">>",                                    false,   false,   null,                       BinaryOperator.RightShift);

			Add(TokenId.AND,               "AND",                                   true,    false,   null,                       BinaryOperator.LogicalAnd);
			Add(TokenId.OR,                "OR",                                    true,    false,   null,                       BinaryOperator.LogicalOr);
			Add(TokenId.IS,                "IS",                                    true,    false,   null,                       null);
			Add(TokenId.NULL,              "NULL",                                  true,    false,   null,                       null);
			Add(TokenId.NOT,               "NOT",                                   true,    false,   UnaryOperator.LogicalNot,   null);
			Add(TokenId.LIKE,              "LIKE",                                  true,    false,   null,                       BinaryOperator.Like);
			Add(TokenId.SOUNDSLIKE,        "SOUNDSLIKE",                            true,    false,   null,                       BinaryOperator.Soundex);
			Add(TokenId.SIMILAR,           "SIMILAR",                               true,    false,   null,                       BinaryOperator.SimilarTo);
			Add(TokenId.TO,                "TO",                                    true,    false,   null,                       null);
			Add(TokenId.BETWEEN,           "BETWEEN",                               true,    false,   null,                       null);
			Add(TokenId.IN,                "IN",                                    true,    false,   null,                       BinaryOperator.In);
			Add(TokenId.CAST,              "CAST",                                  true,    false,   null,                       null);
			Add(TokenId.AS,                "AS",                                    true,    false,   null,                       null);
			Add(TokenId.COALESCE,          "COALESCE",                              true,    false,   null,                       null);
			Add(TokenId.NULLIF,            "NULLIF",                                true,    false,   null,                       null);
			Add(TokenId.CASE,              "CASE",                                  true,    false,   null,                       null);
			Add(TokenId.WHEN,              "WHEN",                                  true,    false,   null,                       null);
			Add(TokenId.THEN,              "THEN",                                  true,    false,   null,                       null);
			Add(TokenId.ELSE,              "ELSE",                                  true,    false,   null,                       null);
			Add(TokenId.END,               "END",                                   true,    false,   null,                       null);
			Add(TokenId.TRUE,              "TRUE",                                  true,    false,   null,                       null);
			Add(TokenId.FALSE,             "FALSE",                                 true,    false,   null,                       null);

			Add(TokenId.SELECT,            "SELECT",                                true,    false,   null,                       null);
			Add(TokenId.TOP,               "TOP",                                   true,    true,    null,                       null);
			Add(TokenId.DISTINCT,          "DISTINCT",                              true,    true,    null,                       null);
			Add(TokenId.FROM,              "FROM",                                  true,    true,    null,                       null);
			Add(TokenId.WHERE,             "WHERE",                                 true,    true,    null,                       null);
			Add(TokenId.GROUP,             "GROUP",                                 true,    true,    null,                       null);
			Add(TokenId.BY,                "BY",                                    true,    true,    null,                       null);
			Add(TokenId.HAVING,            "HAVING",                                true,    true,    null,                       null);
			Add(TokenId.ORDER,             "ORDER",                                 true,    true,    null,                       null);
			Add(TokenId.ASC,               "ASC",                                   true,    true,    null,                       null);
			Add(TokenId.DESC,              "DESC",                                  true,    true,    null,                       null);
			Add(TokenId.UNION,             "UNION",                                 true,    true,    null,                       null);
			Add(TokenId.ALL,               "ALL",                                   true,    true,    null,                       null);
			Add(TokenId.INTERSECT,         "INTERSECT",                             true,    true,    null,                       null);
			Add(TokenId.EXCEPT,            "EXCEPT",                                true,    true,    null,                       null);
			Add(TokenId.EXISTS,            "EXISTS",                                true,    true,    null,                       null);
			Add(TokenId.ANY,               "ANY",                                   true,    true,    null,                       null);
			Add(TokenId.SOME,              "SOME",                                  true,    true,    null,                       null);
			Add(TokenId.JOIN,              "JOIN",                                  true,    true,    null,                       null);
			Add(TokenId.INNER,             "INNER",                                 true,    true,    null,                       null);
			Add(TokenId.CROSS,             "CROSS",                                 true,    true,    null,                       null);
			Add(TokenId.LEFT,              "LEFT",                                  true,    true,    null,                       null);
			Add(TokenId.RIGHT,             "RIGHT",                                 true,    true,    null,                       null);
			Add(TokenId.OUTER,             "OUTER",                                 true,    true,    null,                       null);
			Add(TokenId.FULL,              "FULL",                                  true,    true,    null,                       null);
			Add(TokenId.ON,                "ON",                                    true,    true,    null,                       null);
			Add(TokenId.WITH,              "WITH",                                  true,    true,    null,                       null);
			Add(TokenId.TIES,              "TIES",                                  true,    true,    null,                       null);
		}
	}
}