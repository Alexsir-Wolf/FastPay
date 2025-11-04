using FluentValidation.Results;
using System.Reflection;
using FluentValidation;
using MediatR;

namespace FastPay.Application.Common.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);
            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken))
            );

            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f is not null)
                .ToList();

            if (failures.Count != 0)
            {
                var responseType = typeof(TResponse);
                if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(CommandResult<>))
                {
                    var errors = BuildErrorList(failures);
                    var genericArg = responseType.GetGenericArguments()[0];
                    var genericResultType = typeof(CommandResult<>).MakeGenericType(genericArg);
                    var failWithList = genericResultType.GetMethod(
                        name: nameof(CommandResult<object>.Fail),
                        BindingFlags.Public | BindingFlags.Static,
                        binder: null,
                        types: new[] { typeof(IEnumerable<string>) },
                        modifiers: null);

                    if (failWithList is not null)
                    {
                        var result = failWithList.Invoke(null, new object[] { errors });
                        return (TResponse)result!;
                    }

                    var failWithString = genericResultType.GetMethod(
                        name: nameof(CommandResult<object>.Fail),
                        BindingFlags.Public | BindingFlags.Static,
                        binder: null,
                        types: new[] { typeof(string) },
                        modifiers: null);

                    if (failWithString is not null)
                    {
                        var result = failWithString.Invoke(null, new object[] { string.Join("; ", errors) });
                        return (TResponse)result!;
                    }
                }
                throw new ValidationException(failures);
            }
        }

        return await next();
    }

    private static List<string> BuildErrorList(IReadOnlyCollection<ValidationFailure> failures)
    {
        return failures
            .Select(f =>
            {
                var property = string.IsNullOrWhiteSpace(f.PropertyName) ? "Campo" : f.PropertyName;
                return $"{property}: {f.ErrorMessage}";
            })
            .ToList();
    }
}
