namespace TasksWebApi.Extensions;

public static class IQueryableExtensions
{
    public static IQueryable<T> Paginate<T>(this IQueryable<T> queryable, int pageSize, int pageNumber)
    {
        return queryable
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
    }
}