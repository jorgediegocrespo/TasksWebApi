namespace TasksWebApi.Services;

public record SerilogSettings(LogType Type, string TableName, string ConnectionString)
{
    public static SerilogSettings FromDictionary(IDictionary<string, object> dictionary)
    {
        return new SerilogSettings(
            Enum.Parse<LogType>(dictionary["Type"].ToString()!),
            dictionary["TableName"].ToString(),
            dictionary["ConnectionString"].ToString()
        );
    }
}