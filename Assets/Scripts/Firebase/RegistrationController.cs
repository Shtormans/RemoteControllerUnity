using TMPro;
using UnityEngine;

public class RegistrationController : MonoBehaviour
{
    [SerializeField] private TMP_InputField _email;
    [SerializeField] private TMP_InputField _password;
    [SerializeField] private TMP_InputField _confirmedPassword;

    [SerializeField] private ErrorDisplayer _errorDisplayer;

    public void Register()
    {
        var emailResult = Email.Create(_email.text);
        if (emailResult.IsFailure)
        {
            _errorDisplayer.DisplayError(emailResult.Error);
            return;
        }

        var passwordResult = Password.Create(_password.text);
        if (passwordResult.IsFailure)
        {
            _errorDisplayer.DisplayError(passwordResult.Error);
            return;
        }

        var confirmedPasswordResult = ConfirmedPassword.Create(_confirmedPassword.text, passwordResult.Value);
        if (confirmedPasswordResult.IsFailure)
        {
            _errorDisplayer.DisplayError(confirmedPasswordResult.Error);
            return;
        }

        User user = new User()
        {
            Email = emailResult.Value,
            Password = passwordResult.Value
        };

        FirebaseRepository.Instance.RegisterOrLogin(user, RegisterFinished);
    }

    private void RegisterFinished(Result result)
    {
        if (result.IsFailure)
        {
            _errorDisplayer.DisplayError(result.Error);
        }
        else
        {
            SceneController.ChangeSceneToGame();
        }
    }
}