using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerPrefsRepository
{
    public static string GetUserPassword()
    {
        string key = PlayerPrefsKeysCreator.CreateUserPasswordKey();
        return PlayerPrefs.GetString(key);
    }

    public static void SetUserPassword(string password)
    {
        string key = PlayerPrefsKeysCreator.CreateUserPasswordKey();
        PlayerPrefs.SetString(key, password);
    }
}
