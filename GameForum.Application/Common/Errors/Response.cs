namespace GameForum.Application.Common.Errors;

public class Response<TData>
{
    public string? ErrorMessage { get; set; }

    public TData? Data { get; set; }
}
