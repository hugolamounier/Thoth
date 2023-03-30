using System;

namespace Thoth.Core;

public class ThothException: Exception
{
    public ThothException(string message): base(message){}
}