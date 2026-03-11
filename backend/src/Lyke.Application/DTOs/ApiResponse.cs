namespace Lyke.Application.DTOs;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public ApiError? Error { get; set; }
    public PaginationMeta? Meta { get; set; }

    public static ApiResponse<T> Ok(T data) => new() { Success = true, Data = data };

    public static ApiResponse<T> Ok(T data, PaginationMeta meta) =>
        new()
        {
            Success = true,
            Data = data,
            Meta = meta,
        };

    public static ApiResponse<T> Fail(string code, string message) =>
        new()
        {
            Success = false,
            Error = new ApiError { Code = code, Message = message },
        };

    public static ApiResponse<T> Fail(
        string code,
        string message,
        IDictionary<string, string[]> details
    ) =>
        new()
        {
            Success = false,
            Error = new ApiError
            {
                Code = code,
                Message = message,
                Details = details,
            },
        };
}

public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse Ok() => new() { Success = true };

    public static new ApiResponse Fail(string code, string message) =>
        new()
        {
            Success = false,
            Error = new ApiError { Code = code, Message = message },
        };
}

public class ApiError
{
    public required string Code { get; set; }
    public required string Message { get; set; }
    public IDictionary<string, string[]>? Details { get; set; }
}

public class PaginationMeta
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
