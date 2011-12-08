namespace NQuery.Compilation
{
	internal sealed class Token
	{
		private string _text;
		private TokenId _tokenId;
		private int _pos;
		private SourceRange _range;

		public Token(string text, TokenId tokenId, int pos, SourceRange range)
		{
			_text = text;
			_tokenId = tokenId;
			_pos = pos;
			_range = range;
		}

		public string Text
		{
			get
			{
				TokenInfo info = TokenInfo.FromTokenId(_tokenId);

				if (info.HasDynamicText)
					return _text;
				else
					return info.Text;
			}
		}

		public TokenId Id
		{
			get { return _tokenId; }
		}

		public int Pos
		{
			get { return _pos; }
		}

		public SourceRange Range
		{
			get { return _range; }
		}

		public TokenInfo Info
		{
			get { return TokenInfo.FromTokenId(_tokenId); }
		}

		public override string ToString()
		{
			return Text;
		}
	}
}