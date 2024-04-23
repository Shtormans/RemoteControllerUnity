using System.Collections.Generic;

public class Password : ValueObject
{
    public const int MaxLength = 15;
    public const int MinLength = 8;

    private Password(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<Password> Create(string password)
    {
        if (password.Length < MinLength)
        {
            return Result.Failure<Password>(new Error(
                "Password.TooShort",
                $"Password must be minimum {MinLength} characters long."));
        }

        if (password.Length > MaxLength)
        {
            return Result.Failure<Password>(new Error(
                "Password.TooLong",
                $"Password must be maximum {MaxLength} characters long."));
        }

        return new Password(password);
    }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }

    public static implicit operator string(Password password) => password.Value;
}
