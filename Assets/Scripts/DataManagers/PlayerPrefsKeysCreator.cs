using Firebase.Auth;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerPrefsKeysCreator
{
    public static string CreateUserPasswordKey()
    {
        return $"{FirebaseAuth.DefaultInstance.CurrentUser.Email}_UserPassword";
    }

    public static string CreateDeviceIdKey()
    {
        return $"{FirebaseAuth.DefaultInstance.CurrentUser.Email}_DeviceId";
    }
}
