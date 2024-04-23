using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneController
{
    public static void ChangeSceneToGame()
    {
        SceneManager.LoadScene(SceneNames.MainGame);
    }

    public static void ChangeSceneToMainMenu()
    {
        SceneManager.LoadScene(SceneNames.MainMenu);
    }
}