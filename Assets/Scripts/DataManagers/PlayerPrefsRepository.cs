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


    public static bool TryGetDeviceIdKey(out string result)
    {
        string key = PlayerPrefsKeysCreator.CreateDeviceIdKey();

        if (!PlayerPrefs.HasKey(key))
        {
            result = string.Empty;
            return false;
        }

        result = PlayerPrefs.GetString(key);
        return true;
    }

    public static void SetUserPassword(string password)
    {
        string key = PlayerPrefsKeysCreator.CreateUserPasswordKey();
        PlayerPrefs.SetString(key, password);
    }

    public static void SetDeviceIdKey(string deviceId)
    {
        string key = PlayerPrefsKeysCreator.CreateDeviceIdKey();
        PlayerPrefs.SetString(key, deviceId);
    }
}
