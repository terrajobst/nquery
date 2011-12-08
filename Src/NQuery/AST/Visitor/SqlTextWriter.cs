using System;
using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;

namespace NQuery.Compilation
{
	[SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
	internal sealed class SqlTextWriter
	{
		private StringBuilder _sb;
		private IndentedTextWriter _textWriter;

		public SqlTextWriter()
		{
			_sb = new StringBuilder();
			_textWriter = new IndentedTextWriter(new StringWriter(_sb, CultureInfo.InvariantCulture), " ");
		}

		public int Indent
		{
			get { return _textWriter.Indent; }
			set { _textWriter.Indent = value; }
		}

		public void Write(string text)
		{
			_textWriter.Write(text);
		}

		public void WriteLine(string text)
		{
			_textWriter.WriteLine(text);
		}

		public void WriteLine()
		{
			_textWriter.WriteLine();
		}
		
		public void WriteIdentifier(string text)
		{
			WriteIdentifier(Identifier.CreateVerbatim(text));
		}
		
		public void WriteIdentifier(Identifier identifier)
		{
			Write(identifier.ToSource());
		}
		
		public void WriteNull()
		{
			Write("NULL");			
		}

		public void WriteLiteral(bool value)
		{
			if (value)
				Write("TRUE");
			else
				Write("FALSE");
		}

		public void WriteLiteral(DateTime value)
		{
			Write("#");
			Write(value.ToString(CultureInfo.InvariantCulture));
			Write("#");	
		}

		public void WriteLiteral(string value)
		{
			StringBuilder sb = new StringBuilder(value.Length + 2);
			sb.Append("'");
			
			foreach (char c in value)
			{
				if (c == '\'')
					sb.Append("'");
				
				sb.Append(c);
			}
			
			sb.Append("'");
			Write(sb.ToString());
		}

		public void WriteLiteral(char value)
		{
			Write("CAST(");
			WriteLiteral(value.ToString());
			Write(" AS 'System.Char')");
		}

		public void WriteLiteral(long value, Type realType)
		{
			Write("CAST(");
			Write(value.ToString(CultureInfo.InvariantCulture));
			Write(" AS '");
			Write(realType.FullName);
			Write("')");			
		}

		public void WriteLiteral(double value, Type realType)
		{
			Write("CAST(");
			Write(value.ToString(CultureInfo.InvariantCulture));
			Write(" AS '");
			Write(realType.FullName);
			Write("')");
		}

		public void WriteLiteral(long value)
		{
			Write(value.ToString(CultureInfo.InvariantCulture));
		}

		public void WriteLiteral(double value)
		{
			Write(value.ToString("E", CultureInfo.InvariantCulture));
		}

		public void WriteLiteral(object value)
		{
			Write("<Non-Printable Type (");
			Write(value.GetType().FullName);
			Write("): ");
			Write(value.ToString());
			Write(">");
		}

		public override string ToString()
		{
			return _sb.ToString();
		}
	}
}