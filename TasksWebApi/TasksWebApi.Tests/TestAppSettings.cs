namespace TasksWebApi.Tests;

public class TestAppSettings
{
    public string XApiKey { get; set; }
    public ConnectionStrings ConnectionStrings { get; set; }
}

public class ConnectionStrings
{
    public string DataBaseConnection { get; set; }
}