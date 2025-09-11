public record PagedResult<T>(
    int Page,
    int PageSize,
    int Total,
    List<T> Items
);
