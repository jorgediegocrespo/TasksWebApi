namespace TasksWebApi.Exceptions;

public class NotValidOperationException(string code, string description) : Exception(code)
{
    public string Description { get; protected set; } = description;

    public string Code { get; protected set; } = code;
}