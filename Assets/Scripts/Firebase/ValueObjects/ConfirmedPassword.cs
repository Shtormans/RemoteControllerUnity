using System.Collections.Generic;

public class ConfirmedPassword : ValueObject
{
    public const int MaxLength = 15;
    public const int MinLength = 3;

    private ConfirmedPassword(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<ConfirmedPassword> Create(string confirmedPassword, Password regularPassword)
    {
        if (confirmedPassword != regularPassword.Value)
        {
            return Result.Failure<ConfirmedPassword>(new Error(
                "ConfirmedPassword.NotIdentical",
                $"Passwords must be equal."));
        }

        return new ConfirmedPassword(confirmedPassword);
    }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }

    public static implicit operator string(ConfirmedPassword password) => password.Value;
}
