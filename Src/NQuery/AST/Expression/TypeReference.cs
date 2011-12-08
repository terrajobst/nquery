using System;
using System.Collections.Generic;

namespace NQuery.Compilation
{
	internal sealed class TypeReference : AstElement
	{
		private string _typeName;
		private SourceRange _typeNameSourceRange;
		private bool _caseSensitve;
		private Type _resolvedType;

		public TypeReference()
		{
		}

		public override AstElement Clone(Dictionary<AstElement, AstElement> alreadyClonedElements)
		{
			TypeReference result = new TypeReference();
			result.TypeName = _typeName;
			result.TypeNameSourceRange = _typeNameSourceRange;
			result.CaseSensitve = _caseSensitve;
			result.ResolvedType = _resolvedType;

			return result;
		}

		public string TypeName
		{
			get { return _typeName; }
			set { _typeName = value; }
		}

		public SourceRange TypeNameSourceRange
		{
			get { return _typeNameSourceRange; }
			set { _typeNameSourceRange = value; }
		}

		public Type ResolvedType
		{
			get { return _resolvedType; }
			set { _resolvedType = value; }
		}

		public bool CaseSensitve
		{
			get { return _caseSensitve; }
			set { _caseSensitve = value; }
		}
	}
}