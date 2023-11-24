namespace TasksWebApi.Constants;

public abstract class ErrorCodes
{
    public const string ITEM_NOT_EXISTS = "ITEM_NOT_EXISTS";
    public const string TASK_LIST_NOT_EXISTS = "TASK_LIST_NOT_EXISTS";
    public const string TASK_LIST_NAME_EXISTS = "TASK_LIST_NAME_EXISTS";
    public const string TASK_LIST_WITH_TASKS = "TASK_LIST_WITH_TASKS";
    public const string USER_WITH_LISTS = "USER_WITH_LISTS";
    public const string CONCURRENCY_ERROR = "CONCURRENCY_ERROR";
}