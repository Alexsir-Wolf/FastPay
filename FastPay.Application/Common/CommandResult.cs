namespace FastPay.Application.Common;

public class CommandResult<T>
{
    public bool Success { get; init; }
    public string Mensagem { get; init; } = string.Empty;
    public T? Data { get; init; }

    public static CommandResult<T> Ok(T data, string mensagem = "") => new()
    {
        Success = true,
        Mensagem = mensagem,
        Data = data
    };

    public static CommandResult<T> Fail(string mensagem) => new()
    {
        Success = false,
        Mensagem = mensagem,
        Data = default
    };
}

