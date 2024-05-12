using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitAccountButton : MonoBehaviour
{
    public void ExitFromAccount()
    {
        FirebaseRepository.Instance.SignOut();

        SceneController.ChangeSceneToMainMenu();
    }
}
