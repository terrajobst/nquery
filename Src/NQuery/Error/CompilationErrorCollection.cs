using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NQuery
{
    /// <summary>
    /// Represents a read-only collection of compile-time errors contained in <see cref="CompilationException.CompilationErrors"/>.
    /// </summary>
    [Serializable]
    public sealed class CompilationErrorCollection : ReadOnlyCollection<CompilationError>
    {
        internal CompilationErrorCollection(IList<CompilationError> errors)
            : base(errors)
        {
        }
    }
}