using FastPay.Application.Accounts.Commands;
using FastPay.Domain.Enums;
using FluentValidation;

namespace FastPay.Application.Accounts.Validators;

public class UpdateAccountStatusCommandValidator : AbstractValidator<UpdateAccountStatusCommand>
{
    public UpdateAccountStatusCommandValidator()
    {
        RuleFor(r => r.AccountId)
            .NotEmpty()
            .WithMessage("O campo Id da conta não pode ser vazio.");

        RuleFor(r => r.Status)
            .IsInEnum()
            .NotEmpty()
            .WithMessage("O status informado é inválido. Use: Active, Inactive ou Blocked.");
    }
}
