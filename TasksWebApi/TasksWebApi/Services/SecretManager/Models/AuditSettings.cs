namespace TasksWebApi.Services;

public record AuditSettings(LogType Type, string TableName, string ConnectionString)
{
    public static AuditSettings FromDictionary(IDictionary<string, object> dictionary)
    {
        return new AuditSettings(
            Enum.Parse<LogType>(dictionary["Type"].ToString()!),
            dictionary["TableName"].ToString(),
            dictionary["ConnectionString"].ToString()
        );
    }
}