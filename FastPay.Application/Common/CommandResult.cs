using System.Text.Json.Serialization;

namespace FastPay.Application.Common;

public class CommandResult<T>
{
    public bool Success { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Message { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? Data { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyCollection<string>? Errors { get; init; }

    public static CommandResult<T> Ok(T data, string mensagem = "") => new()
    {
        Success = true,
        Message = mensagem,
        Data = data,
        Errors = null
    };

    public static CommandResult<T> Fail(string mensagem) => new()
    {
        Success = false,
        Message = null,
        Data = default,
        Errors = new List<string> { mensagem }
    };

    public static CommandResult<T> Fail(IEnumerable<string> errors) => new()
    {
        Success = false,
        Message = null,
        Data = default,
        Errors = errors.ToList()
    };
}
