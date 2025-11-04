using System.Text.Json.Serialization;

namespace FastPay.Application.Common;

public class CommandResult<T>
{
    public bool Success { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? Data { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyCollection<string>? Errors { get; init; }

    public static CommandResult<T> Ok(T data) => new()
    {
        Success = true,
        Data = data,
        Errors = null
    };

    public static CommandResult<T> Fail(string mensagem) => new()
    {
        Success = false,
        Data = default,
        Errors = new List<string> { mensagem }
    };

    public static CommandResult<T> Fail(IEnumerable<string> errors) => new()
    {
        Success = false,
        Data = default,
        Errors = errors.ToList()
    };
}
