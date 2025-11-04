using FastPay.Application.Transactions.Commands;
using FastPay.Domain.Constants;
using FluentValidation;

namespace FastPay.Application.Transactions.Validators;

public sealed class ProcessTransactionCommandValidator : AbstractValidator<ProcessTransactionCommand>
{
    public ProcessTransactionCommandValidator()
    {
        RuleFor(x => x.Operation)
            .Must(TransactionOperations.IsValid)
            .WithMessage("A operação informada é inválida.");

        RuleFor(x => x.Currency)
                    .NotEmpty().WithMessage("Currency é obrigatório.")
                    .Must(c => new[] { CurrencyCodes.BRL, CurrencyCodes.USD, CurrencyCodes.EUR }
                        .Contains(c.ToUpperInvariant()))
                    .WithMessage("Moeda inválida. Use BRL, USD ou EUR.");

        RuleFor(x => x.ReferenceId)
            .NotEmpty().WithMessage("ReferenceId é obrigatório.");

        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0).WithMessage("Amount deve ser maior ou igual a 0 (centavos).");

        When(x => string.Equals(x.Operation, TransactionOperations.Transfer, StringComparison.OrdinalIgnoreCase), () =>
        {
            RuleFor(x => x.SourceAccountId)
                .GreaterThan(0).WithMessage("SourceAccountId é obrigatório para transferência.");
            
            RuleFor(x => x.DestinationAccountId)
                .GreaterThan(0).WithMessage("DestinationAccountId é obrigatório para transferência.");
            
            RuleFor(x => x).Must(x => x.SourceAccountId != x.DestinationAccountId)
                .WithMessage("As contas de saida e entrada, não podem ser iguais.");
        });   
    }
}
