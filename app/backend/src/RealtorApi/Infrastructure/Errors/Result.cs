using Microsoft.AspNetCore.Mvc;

namespace RealtorApi.Infrastructure.Errors;

public sealed record Result
{
    public bool IsSuccess { get; }
    public string? Code { get; }
    public string? Message { get; }
    public string? Detail { get; }

    private Result(bool isSuccess, string? code, string? message, string? detail)
    {
        IsSuccess = isSuccess;
        Code = code;
        Message = message;
        Detail = detail;
    }

    public static Result Success() => new(true, null, null, null);

    public static Result Fail(string code, string message, string? detail = null) =>
        new(false, code, message, detail);

    public static ProblemDetails ToProblemDetails(Result result)
    {
        var status = result.Code switch
        {
            "NOT_FOUND" => StatusCodes.Status404NotFound,
            "CONFLICT" => StatusCodes.Status409Conflict,
            "FORBIDDEN" => StatusCodes.Status403Forbidden,
            "VALIDATION" => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status400BadRequest
        };

        return new ProblemDetails
        {
            Title = result.Message ?? "Request failed",
            Detail = result.Detail ?? result.Message,
            Status = status,
            Extensions = { ["code"] = result.Code ?? "UNKNOWN" }
        };
    }
}

public static class ErrorMapper
{
    public static ProblemDetails ToProblemDetails(Result result) => Result.ToProblemDetails(result);
}
