namespace TasksWebApi.Services;

public record RedisSettings(string ConnectionString, string InstanceName)
{
    public static RedisSettings FromDictionary(IDictionary<string, object> dictionary)
    {
        return new RedisSettings(
            dictionary["ConnectionString"].ToString(),
            dictionary["InstanceName"].ToString()
        );
    }
}