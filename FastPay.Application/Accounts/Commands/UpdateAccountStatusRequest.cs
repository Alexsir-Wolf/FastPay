using FastPay.Domain.Enums;

namespace FastPay.Application.Accounts.Commands;

public record UpdateAccountStatusRequest 
{
    public AccountStatus Status { get; set; }
}
