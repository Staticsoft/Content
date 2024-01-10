using System;

namespace Staticsoft.Content.Abstractions;

public class ContentException : Exception
{
    public ContentException(string message)
        : base(message) { }

    public ContentException(string message, Exception innerException)
        : base(message, innerException) { }
}
