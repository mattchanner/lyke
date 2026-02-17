namespace Lyke.Core.Exceptions;

public class AccountLockedException : UnauthorizedException
{
    public DateTimeOffset? LockoutEnd { get; }

    public AccountLockedException(DateTimeOffset? lockoutEnd)
        : base(lockoutEnd.HasValue
            ? $"Account is locked until {lockoutEnd.Value.UtcDateTime:yyyy-MM-dd HH:mm:ss} UTC"
            : "Account is locked")
    {
        LockoutEnd = lockoutEnd;
    }
}
