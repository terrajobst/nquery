using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;

using ActiproSoftware.SyntaxEditor;

namespace NQuery.UI
{
	public partial class ActiproLink : Component
	{
		private SyntaxEditor _syntaxEditor;
		private Evaluatable _evaluatable;

		private bool _enableAutoReplace;
		private bool _enableAutoPopupAfterCharacter;
		private bool _enableAutoPopupAfterDot;
		private bool _enableAutoPopupAfterParenthesis;

		private bool _bound;
		private SourceLocation _parameterInfoLocation;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
		public ActiproLink()
		{
			InitializeComponent();

			Setup();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
		public ActiproLink(IContainer container)
		{
			if (container == null)
				throw new ArgumentNullException("container");

			container.Add(this);
			InitializeComponent();

			Setup();
		}

		#region Helpers

		private void Setup()
		{
			_enableAutoReplace = true;
			_enableAutoPopupAfterDot = true;
			_enableAutoPopupAfterParenthesis = true;
			_enableAutoPopupAfterCharacter = true;
		}

		private bool CanBind
		{
			get
			{
				return _syntaxEditor != null &&
					   _evaluatable != null &&
					   _evaluatable.DataContext != null &&
					   !_bound;
			}
		}

		private void EnsureBound()
		{
			if (_syntaxEditor == null)
				throw new InvalidOperationException("The SyntaxEditor property has not been initialized.");

			if (_evaluatable == null)
				throw new InvalidOperationException("The Evaluatable property has not been initialized.");

			if (!_bound)
				Bind();
		}

		private ScopeInfo GetScopeInfo()
		{
			string oldText = _evaluatable.Text;
			_evaluatable.Text = String.Empty;
			try
			{
				ScopeInfoBuilder builder = new ScopeInfoBuilder();

				ICodeAssistanceContextProvider codeAssistanceContextProvider = _evaluatable.GetCodeAssistanceContextProvider();
				IMemberCompletionContext context = codeAssistanceContextProvider.ProvideMemberCompletionContext(SourceLocation.Empty);
				context.Enumerate(builder);

				return builder.GetScopeInfo();
			}
			finally
			{
				_evaluatable.Text = oldText;
			}
		}

		private void Bind()
		{
			if (!_bound)
			{
				SetupLanguage();

				if (_enableAutoReplace)
					SetupAutoReplacement();

				_syntaxEditor.TriggerActivated += syntaxEditor_TriggerActivated;
				_syntaxEditor.KeyTyped += syntaxEditor_KeyTyped;
				_syntaxEditor.KeyDown += syntaxEditor_KeyDown;
				_syntaxEditor.SelectionChanged += syntaxEditor_SelectionChanged;

				_bound = true;
			}
		}

		private void SetupLanguage()
		{
			_syntaxEditor.Document.ResetLanguage();

			using (Stream languageFileStream = GetLanguageFileStream())
			{
				_syntaxEditor.Document.LoadLanguageFromXml(languageFileStream, 0);
			}
		}

		private Stream GetLanguageFileStream()
		{
			XmlDocument document;

			using (Stream languageFileStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(ActiproLink), "ActiproSoftware.SQL.xml"))
			{
				document = new XmlDocument();
				document.Load(languageFileStream);
			}

			XmlNamespaceManager namespaceManager = new XmlNamespaceManager(document.NameTable);
			namespaceManager.AddNamespace("ns", "http://ActiproSoftware/SyntaxEditor/4.0/LanguageDefinition");

			ScopeInfo scopeInfo = GetScopeInfo();

			string reservedWordsNodeXPath = "/ns:SyntaxLanguage/ns:States/ns:State[@Key='DefaultState']/ns:PatternGroups/ns:ExplicitPatternGroup[@TokenKey='ReservedWordToken']/ns:ExplicitPatterns";
			XmlNode reservedWordsNode = document.SelectSingleNode(reservedWordsNodeXPath, namespaceManager);
			reservedWordsNode.InnerText = scopeInfo.GetKeywords();

			string functionTokensNodeXPath = "/ns:SyntaxLanguage/ns:States/ns:State/ns:PatternGroups/ns:ExplicitPatternGroup[@TokenKey='FunctionToken']/ns:ExplicitPatterns";
			XmlNode functionTokensNode = document.SelectSingleNode(functionTokensNodeXPath, namespaceManager);
			functionTokensNode.InnerText = scopeInfo.GetFunctionsAndAggregates();

			MemoryStream memoryStream = new MemoryStream();
			try
			{
				document.Save(memoryStream);
				memoryStream.Position = 0;
				return memoryStream;
			}
			catch
			{
				memoryStream.Close();
				throw;
			}
		}

		private void Unbind()
		{
			if (_bound)
			{
				if (_enableAutoReplace)
					_syntaxEditor.AutoReplace.Clear();

				if (_syntaxEditor.Document != null)
					_syntaxEditor.Document.ResetLanguage();

				_syntaxEditor.TriggerActivated -= syntaxEditor_TriggerActivated;
				_syntaxEditor.KeyTyped -= syntaxEditor_KeyTyped;
				_syntaxEditor.KeyDown -= syntaxEditor_KeyDown;
				_syntaxEditor.SelectionChanged -= syntaxEditor_SelectionChanged;

				_bound = false;
			}
		}

		private SourceLocation CurrentLocation
		{
			get { return new SourceLocation(_syntaxEditor.Caret.DocumentPosition.Character, _syntaxEditor.Caret.DocumentPosition.Line); }
		}

		[SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
		private void SetupAutoReplacement()
		{
			ScopeInfo scopeInfo = GetScopeInfo();

			_syntaxEditor.AutoReplace.Clear();
			foreach (string reservedWord in scopeInfo.ReservedWords.Keys)
			{
				string lowerCaseReservedWord = reservedWord.ToLowerInvariant();
				string upperCaseReservedWord = reservedWord.ToUpperInvariant();
				_syntaxEditor.AutoReplace.Add(new AutoReplaceEntry(lowerCaseReservedWord, upperCaseReservedWord));
			}
		}

		private void ShowErrorQuickInfo(Exception exception)
		{
			// Calculate the point of the line above the current one and
			// display an info tip with the error message.

			DocumentPosition pos = _syntaxEditor.Caret.DocumentPosition;

			if (pos.Line > 0)
				pos = new DocumentPosition(pos.Line - 1, pos.Character);
			else
				pos = new DocumentPosition(pos.Line + 1, pos.Character);

			Point tipLocation;

			if (pos.Line >= 0 && pos.Line < _syntaxEditor.Document.Lines.Count)
			{
				int offset = _syntaxEditor.Document.PositionToOffset(pos);
				tipLocation = _syntaxEditor.SelectedView.GetCharacterBounds(offset).Location;
			}
			else
			{
				tipLocation = new Point(0, -20);
			}

			StringBuilder sb = new StringBuilder();

			using (StringWriter sw = new StringWriter(sb, CultureInfo.CurrentCulture))
			{
				XmlWriter writer = new XmlTextWriter(sw);

				writer.WriteStartElement("b");
				writer.WriteString("Error: ");
				writer.WriteEndElement();
				writer.WriteString(exception.Message);
			}

			string markup = sb.ToString();
			_syntaxEditor.IntelliPrompt.QuickInfo.Hide();
			_syntaxEditor.IntelliPrompt.QuickInfo.Show(tipLocation, markup);
		}

		public TextRange GetTextRange(SourceRange sourceRange)
		{
			EnsureBound();

			DocumentPosition startPosition = new DocumentPosition(sourceRange.StartLine, sourceRange.StartColumn);
			DocumentPosition endPosition = new DocumentPosition(sourceRange.EndLine, sourceRange.EndColumn);

			int startOffset = _syntaxEditor.Document.PositionToOffset(startPosition);
			int endOffset = _syntaxEditor.Document.PositionToOffset(endPosition) + 1;

			if (startOffset > _syntaxEditor.Document.Length)
				startOffset = _syntaxEditor.Document.Length - 1;

			if (endOffset > _syntaxEditor.Document.Length)
				endOffset = _syntaxEditor.Document.Length - 1;

			return new TextRange(startOffset, endOffset);
		}

		private void LoadErrors(IEnumerable<CompilationError> errors)
		{
			_syntaxEditor.Document.SpanIndicatorLayers.Clear();
			if (errors != null)
			{
				SpanIndicatorLayer spanIndicatorLayer = new SpanIndicatorLayer(null, 0);
				_syntaxEditor.Document.SpanIndicatorLayers.Add(spanIndicatorLayer);

				foreach (CompilationError error in errors)
				{
					if (error.SourceRange != SourceRange.None)
					{
						TextRange textRange = GetTextRange(error.SourceRange);
						SpanIndicator[] existingIndicators = spanIndicatorLayer.GetIndicatorsForTextRange(textRange);
						if (existingIndicators.Length == 0)
						{
							SyntaxErrorSpanIndicator errorIndicator = new SyntaxErrorSpanIndicator();
							errorIndicator.Tag = error;
							spanIndicatorLayer.Add(errorIndicator, textRange);
						}
					}
				}
			}
		}
		#endregion

		#region Events Subscriptions

		private void evaluatable_DataContextChanged(object sender, EventArgs e)
		{
			Unbind();

			if (CanBind)
				Bind();
		}

		private void evaluatable_CompilationFailed(object sender, CompilationFailedEventArgs e)
		{
			LoadErrors(e.CompilationErrors);
		}

		private void evaluatable_CompilationSucceeded(object sender, EventArgs e)
		{
			LoadErrors(null);
		}

		private void syntaxEditor_TriggerActivated(object sender, TriggerEventArgs e)
		{
			switch (e.Trigger.Key)
			{
				case "MemberListTrigger":
					if (_enableAutoPopupAfterDot)
						ListMembers();
					break;

				case "ShowParameterInfoTrigger":
				case "UpdateParameterInfoTrigger":
					if (_enableAutoPopupAfterParenthesis)
						ShowParameterInfoPopup();
					break;

				case "HideParameterInfoTrigger":
					if (_enableAutoPopupAfterParenthesis)
						HideParameterInfoPopup();
					break;
			}
		}

		private void syntaxEditor_KeyTyped(object sender, KeyTypedEventArgs e)
		{
			if (ShouldAutoPopupListMembers(e.KeyChar))
				ListMembers();
		}

		private void syntaxEditor_KeyDown(object sender, KeyEventArgs e)
		{
			if (_enableAutoReplace && e.KeyData == Keys.Tab && _syntaxEditor.IntelliPrompt.MemberList.Visible)
				_syntaxEditor.IntelliPrompt.MemberList.Complete();
		}

		private void syntaxEditor_SelectionChanged(object sender, SelectionEventArgs e)
		{
			UpdateParameterInfo();
		}

		#endregion

		#region Member Completion

		private void ShowMemberListPopup(bool autoComplete)
		{
			IntelliPromptMemberList memberList = _syntaxEditor.IntelliPrompt.MemberList;
			memberList.Clear();
			memberList.ImageList = memberImageList;
			memberList.Sorted = false;

			try
			{
				_evaluatable.Text = _syntaxEditor.Text;

				ICodeAssistanceContextProvider codeAssistanceContextProvider = _evaluatable.GetCodeAssistanceContextProvider();
				IMemberCompletionContext completionContext = codeAssistanceContextProvider.ProvideMemberCompletionContext(CurrentLocation);

				MemberAcceptor acceptor = new MemberAcceptor(memberList);
				completionContext.Enumerate(acceptor);
				acceptor.FlushBuffer();

				if (memberList.Count > 0)
				{
					memberList.Sorted = true;

					if (completionContext.RemainingPart == null)
					{
						memberList.Show(_syntaxEditor.Caret.Offset, 0);
					}
					else
					{
						IToken t = _syntaxEditor.SelectedView.GetCurrentToken();
						string tokenText = _syntaxEditor.Document.GetTokenText(t);

						string remainingText = completionContext.RemainingPart.Text.ToUpper(CultureInfo.InvariantCulture);
						if (t.StartOffset >= _syntaxEditor.Text.Length || !tokenText.ToUpper(CultureInfo.InvariantCulture).StartsWith(remainingText, StringComparison.OrdinalIgnoreCase))
							t = _syntaxEditor.SelectedView.GetPreviousToken();

						if (t != null)
						{
							if (autoComplete)
								memberList.CompleteWord(t.StartOffset, t.Length);
							else
								memberList.Show(t.StartOffset, t.Length);
						}
					}
				}
			}
			catch (NQueryException ex)
			{
				ShowErrorQuickInfo(ex);
			}
		}

		private void HideMemberListPopup()
		{
			IntelliPromptMemberList memberList = _syntaxEditor.IntelliPrompt.MemberList;
			memberList.Abort();
		}

		private static void FindPosition(TokenStream tokenStream, int offset)
		{
			tokenStream.Position = 0;
			if (offset < 0)
				return;

			while (tokenStream.Position < tokenStream.Length)
			{
				IToken currentToken = tokenStream.Peek();

				if (currentToken.StartOffset <= offset && offset < currentToken.StartOffset + currentToken.Length)
					break;
				else
					tokenStream.Read();
			}
		}

		private static IToken GetFirstNonWhitespaceToken(TokenStream tokenStream)
		{
			if (!tokenStream.FindNonWhitespace(false))
				return null;

			return tokenStream.Peek();
		}

		private bool IsAsKeyword(IToken token)
		{
			string tokenText = _syntaxEditor.Document.GetTokenText(token);
			return String.Compare(tokenText, "AS", StringComparison.OrdinalIgnoreCase) == 0;
		}

		private bool ShouldAutoPopupListMembers(char keyChar)
		{
			if (_enableAutoPopupAfterCharacter && !_syntaxEditor.IntelliPrompt.MemberList.Visible && Char.IsLetter(keyChar))
			{
				IToken tokenAfterCursor = _syntaxEditor.SelectedView.GetCurrentToken();

				TokenStream tokenStream = _syntaxEditor.Document.GetTokenStream(0);
				FindPosition(tokenStream, tokenAfterCursor.StartOffset - 1);

				IToken token0BeforeCursor = tokenStream.Peek();
				IToken token1BeforeCursor = GetFirstNonWhitespaceToken(tokenStream);
				IToken token2BeforeCursor = GetFirstNonWhitespaceToken(tokenStream);

				if (token0BeforeCursor.Key != "IdentifierToken")
					return false;

				if (token1BeforeCursor == null)
					return true;

				switch (token1BeforeCursor.Key)
				{
					case "ReservedWordToken":
						return !IsAsKeyword(token1BeforeCursor);

					case "IdentifierToken":
						if (token2BeforeCursor != null &&
							(token2BeforeCursor.Key == "IdentifierToken" || token2BeforeCursor.Key == "ReservedWordToken"))
							return true;
						return false;

					case "DefaultToken":
					case "OperatorToken":
					case "OpenParenthesisToken":
						return true;
				}
			}

			return false;
		}

		#endregion

		#region Parameter Info

		private void ShowParameterInfoPopup()
		{
			_parameterInfoLocation = CurrentLocation;

			_evaluatable.Text = _syntaxEditor.Text;

			IntelliPromptParameterInfo infoTip = _syntaxEditor.IntelliPrompt.ParameterInfo;
			int lastSelectedFunction = infoTip.SelectedIndex;
			infoTip.Info.Clear();

			try
			{
				ICodeAssistanceContextProvider codeAssistanceContextProvider = _evaluatable.GetCodeAssistanceContextProvider();
				IParameterInfoContext parameterInfoContext = codeAssistanceContextProvider.ProvideParameterInfoContext(CurrentLocation);
				ParameterInfoAcceptor acceptor = new ParameterInfoAcceptor(infoTip, parameterInfoContext.ParameterIndex);
				parameterInfoContext.Enumerate(acceptor);

				if (infoTip.Info.Count == 0)
				{
					infoTip.Hide();
				}
				else
				{
					infoTip.SelectedIndex = lastSelectedFunction;
					infoTip.Show(_syntaxEditor.Caret.Offset);
				}
			}
			catch (NQueryException ex)
			{

				ShowErrorQuickInfo(ex);
			}
		}

		private void HideParameterInfoPopup()
		{
			if (_syntaxEditor.IntelliPrompt.ParameterInfo.Visible)
				_syntaxEditor.IntelliPrompt.ParameterInfo.Hide();
		}

		private void UpdateParameterInfo()
		{
			if (CurrentLocation.Line != _parameterInfoLocation.Line)
				HideParameterInfoPopup();
			else if (_syntaxEditor.IntelliPrompt.ParameterInfo.Visible)
				ShowParameterInfoPopup();
		}

		#endregion

		#region Public API

		[DefaultValue(true)]
		public bool EnableAutoReplace
		{
			get { return _enableAutoReplace; }
			set
			{
				_enableAutoReplace = value;

				if (_bound)
				{
					if (value)
						SetupAutoReplacement();
					else
						_syntaxEditor.AutoReplace.Clear();
				}
			}
		}

		[DefaultValue(true)]
		public bool EnableAutoPopupAfterDot
		{
			get { return _enableAutoPopupAfterDot; }
			set { _enableAutoPopupAfterDot = value; }
		}

		[DefaultValue(true)]
		public bool EnableAutoPopupAfterParenthesis
		{
			get { return _enableAutoPopupAfterParenthesis; }
			set { _enableAutoPopupAfterParenthesis = value; }
		}

		[DefaultValue(true)]
		public bool EnableAutoPopupAfterCharacter
		{
			get { return _enableAutoPopupAfterCharacter; }
			set { _enableAutoPopupAfterCharacter = value; }
		}

		public SyntaxEditor SyntaxEditor
		{
			get { return _syntaxEditor; }
			set
			{
				Unbind();

				_syntaxEditor = value;

				if (CanBind)
					Bind();
			}
		}

		public Evaluatable Evaluatable
		{
			get { return _evaluatable; }
			set
			{
				if (_evaluatable != null)
				{
					_evaluatable.DataContextChanged -= evaluatable_DataContextChanged;
					_evaluatable.CompilationFailed -= evaluatable_CompilationFailed;
					_evaluatable.CompilationSucceeded -= evaluatable_CompilationSucceeded;
				}

				Unbind();

				_evaluatable = value;

				if (_evaluatable != null)
				{
					_evaluatable.DataContextChanged += evaluatable_DataContextChanged;
					_evaluatable.CompilationFailed += evaluatable_CompilationFailed;
					_evaluatable.CompilationSucceeded += evaluatable_CompilationSucceeded;
				}

				if (CanBind)
					Bind();
			}
		}

		public void ListMembers()
		{
			EnsureBound();

			IntelliPromptMemberList memberList = _syntaxEditor.IntelliPrompt.MemberList;
			if (memberList.Visible)
				HideMemberListPopup();
			else
				ShowMemberListPopup(false);
		}

		public void CompleteWord()
		{
			EnsureBound();

			IntelliPromptMemberList memberList = _syntaxEditor.IntelliPrompt.MemberList;
			if (memberList.Visible)
				HideMemberListPopup();
			else
				ShowMemberListPopup(true);
		}

		public void ParameterInfo()
		{
			EnsureBound();
			IntelliPromptParameterInfo infoTip = _syntaxEditor.IntelliPrompt.ParameterInfo;

			if (infoTip.Visible)
				HideParameterInfoPopup();
			else
				ShowParameterInfoPopup();
		}

		#endregion
	}
}
