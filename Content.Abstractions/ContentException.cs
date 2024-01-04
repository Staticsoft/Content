using System;

namespace Staticsoft.Content.Abstractions;

public class ContentException(string message)
    : Exception(message);
