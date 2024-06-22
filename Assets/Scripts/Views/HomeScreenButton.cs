using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomeScreenButton : MonoBehaviour
{
    public void MoveToHomeScreen()
    {
        SceneController.ChangeSceneToGame();
    }
}
