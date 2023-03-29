using System;

namespace Thoth;

public class ThothException: Exception
{
    public ThothException(string message): base(message){}
}