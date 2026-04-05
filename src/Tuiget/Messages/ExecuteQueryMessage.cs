namespace Tuiget;

public sealed record ExecuteQueryMessage(string Query) : TeaMessage;