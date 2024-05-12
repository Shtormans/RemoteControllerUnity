using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneController
{
    public static void ChangeSceneToGame()
    {
        SceneManager.LoadSceneAsync(SceneNames.MainGame);
    }

    public static void ChangeSceneToMainMenu()
    {
        SceneManager.LoadSceneAsync(SceneNames.MainMenu);
    }
}