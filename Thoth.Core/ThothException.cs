using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Thoth.Core;

[Serializable]
[ExcludeFromCodeCoverage]
public class ThothException : Exception
{
    public ThothException(string message) : base(message)
    {
    }

    protected ThothException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}