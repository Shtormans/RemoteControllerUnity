using System.Collections.Generic;
using System.Text.RegularExpressions;

public class Email : ValueObject
{
    public const int MaxLength = 255;

    private Email(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<Email> Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Result.Failure<Email>(new Error(
                "UserEmail.Empty",
                "Email is empty."));
        }

        if (email.Length > MaxLength)
        {
            return Result.Failure<Email>(new Error(
                "UserEmail.TooLong",
                $"Email must be maximum {MaxLength} characters long."));
        }

        if (!Regex.IsMatch(email, @"^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,3})+$"))
        {
            return Result.Failure<Email>(new Error(
                "UserEmail.WrongFormat",
                "Email has a wrong format."));
        }

        return new Email(email);
    }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }

    public static implicit operator string(Email userEmail) => userEmail.Value;
}
