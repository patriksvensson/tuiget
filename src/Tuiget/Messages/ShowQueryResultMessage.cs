namespace Tuiget;

public record ShowQueryResultMessage(List<TableItem> Items) : TeaMessage;