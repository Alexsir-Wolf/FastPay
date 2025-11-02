using FluentValidation;
using FastPay.Application.Accounts.Commands;

namespace FastPay.Application.Accounts.Validators;

public sealed class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountCommandValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty().WithMessage("ClientId é obrigatório.")
            .MinimumLength(3).WithMessage("ClientId deve ter pelo menos 3 caracteres.")
            .MaximumLength(30).WithMessage("ClientId deve ter no máximo 30 caracteres.");

        RuleFor(x => x.InitialBalance)
            .GreaterThanOrEqualTo(0).WithMessage("Saldo inicial não pode ser negativo.");

        RuleFor(x => x.CreditLimit)
            .GreaterThanOrEqualTo(0).WithMessage("Limite de crédito não pode ser negativo.");
    }
}

