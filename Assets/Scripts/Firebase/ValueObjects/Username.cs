using System.Collections.Generic;

public class Username : ValueObject
{
    public const int MaxLength = 15;
    public const int MinLength = 3;

    private Username(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<Username> Create(string username)
    {
        if (username.Length < MinLength)
        {
            return Result.Failure<Username>(new Error(
                "Username.TooShort",
                $"Username must be minimum {MinLength} characters long."));
        }

        if (username.Length > MaxLength)
        {
            return Result.Failure<Username>(new Error(
                "Username.TooLong",
                $"Username must be maximum {MaxLength} characters long."));
        }

        return new Username(username);
    }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }

    public static implicit operator string(Username username) => username.Value;
}
