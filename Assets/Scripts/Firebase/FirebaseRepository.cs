using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

public class FirebaseRepository : MonoBehaviour
{
    private FirebaseAuth _auth;
    private DatabaseReference _dbReference;

    private static FirebaseRepository _instance;

    public event Action DatabaseLoaded;

    public static FirebaseRepository Instance
    {
        get { return _instance; }
    }

    public bool IsDatabaseLoaded { get; private set; }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance == this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;

            if (dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
            }

            IsDatabaseLoaded = true;
            DatabaseLoaded?.Invoke();
        });
    }

    private void InitializeFirebase()
    {
        _auth = FirebaseAuth.DefaultInstance;

        _dbReference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public bool CheckIfPlayerIsSignedIn()
    {
        return _auth.CurrentUser != null;
    }

    public string GetCurrentUserId()
    {
        return _auth.CurrentUser.UserId;
    }

    public void RegisterOrLogin(User user, Action<Result> action)
    {
        StartCoroutine(RegisterOrLoginCoroutine(user, action));
    }

    public void SignOut()
    {
        _auth.SignOut();
    }

    public void SetTempKey(string tempKey)
    {
        StartCoroutine(SetTempKeyCoroutine(tempKey));
    }

    public void GetUserDevices(Action<List<DeviceModel>> action)
    {
        StartCoroutine(GetUserDevicesCoroutine(action));
    }

    public void GetDevice(string tempKey, Action<Result<DeviceModel>> action)
    {
        StartCoroutine(GetDeviceCoroutine(tempKey, action));
    }

    private IEnumerator RegisterOrLoginCoroutine(User user, Action<Result> action)
    {
        var loginTask = _auth.SignInWithEmailAndPasswordAsync(user.Email.Value, user.Password.Value);

        yield return new WaitUntil(() => loginTask.IsCompleted);

        if (loginTask.Exception == null)
        {
            FirebaseUser firebaseUser = loginTask.Result.User;

            yield return UpdateUserData();

            action?.Invoke(Result.Success(user));

            yield break;
        }

        var registerTask = _auth.CreateUserWithEmailAndPasswordAsync(user.Email.Value, user.Password.Value);

        yield return new WaitUntil(() => registerTask.IsCompleted);

        if (registerTask.Exception != null)
        {
            var result = Result.Failure(
                new Error("Authentication.Exception", registerTask.Exception.Message)
            );

            action?.Invoke(result);
            yield break;
        }

        yield return UpdateUserData();

        action?.Invoke(Result.Success());
    }

    private IEnumerator UpdateUserData()
    {
        if (!PlayerPrefsRepository.TryGetDeviceIdKey(out string deviceId))
        {
            yield return CreateNewDevice();

            PlayerPrefsRepository.TryGetDeviceIdKey(out deviceId);
        }

        string tempName = Guid.NewGuid().ToString()[..5];

        var dbTask = _dbReference.Child("Users").Child(_auth.CurrentUser.UserId).Child("Devices").Child(deviceId).SetValueAsync(tempName);

        yield return new WaitUntil(() => dbTask.IsCompleted);
    }

    private IEnumerator CreateNewDevice()
    {
        string id = Guid.NewGuid().ToString();

        var dbTask = _dbReference.Child("Devices").Child(id).Child("Enabled").SetValueAsync(DateTime.UtcNow.ToString("M.d.yyyy HH:mm:ss"));

        yield return new WaitUntil(() => dbTask.IsCompleted);

        PlayerPrefsRepository.SetDeviceIdKey(id);
    }

    private IEnumerator SetTempKeyCoroutine(string tempKey)
    {
        PlayerPrefsRepository.TryGetDeviceIdKey(out string id);

        var dbTask = _dbReference.Child("Devices").Child(id).Child("TempKey").SetValueAsync(tempKey);

        yield return new WaitUntil(() => dbTask.IsCompleted);
    }

    public IEnumerator GetUserDevicesCoroutine(Action<List<DeviceModel>> action)
    {
        var dbTask = _dbReference.Child("Users").Child(_auth.CurrentUser.UserId).Child("Devices").GetValueAsync();

        yield return new WaitUntil(() => dbTask.IsCompleted);

        DataSnapshot snapshot = dbTask.Result;

        PlayerPrefsRepository.TryGetDeviceIdKey(out string deviceId);

        List<DeviceModel> deviceModels = snapshot.Children
            .Select(item =>
            {
                return new DeviceModel()
                {
                    Id = item.Key.ToString(),
                    Name = item.Value.ToString(),
                    IsCurrentDevice = deviceId == item.Key.ToString()
                };
            })
            .ToList();

        action?.Invoke(deviceModels);
    }

    public IEnumerator RenameDeviceCoroutine(string deviceId, string deviceName)
    {
        var dbTask = _dbReference.Child("Users").Child(_auth.CurrentUser.UserId).Child("Devices").Child(deviceId).SetValueAsync(deviceName);

        yield return new WaitUntil(() => dbTask.IsCompleted);
    }

    public IEnumerator GetDeviceCoroutine(string tempKey, Action<Result<DeviceModel>> action)
    {
        var dbTask = _dbReference.Child("Devices").GetValueAsync();

        yield return new WaitUntil(() => dbTask.IsCompleted);

        PlayerPrefsRepository.TryGetDeviceIdKey(out string deviceId);

        DataSnapshot snapshot = dbTask.Result;

        DeviceModel deviceModel = snapshot.Children
            .Where(item => item.HasChild("TempKey") && item.Child("TempKey").Value.ToString() == tempKey)
            .Select(item =>
            {
                return new DeviceModel()
                {
                    Id = item.Key.ToString(),
                    IsCurrentDevice = deviceId == item.Key.ToString(),
                    LastUpdated = DateTime.ParseExact(item.Child("Enabled").Value.ToString(), "M.d.yyyy HH:mm:ss", CultureInfo.InvariantCulture)
                };
            })
            .FirstOrDefault();

        if (deviceModel == null)
        {
            action?.Invoke(Result.Failure<DeviceModel>(Error.NullValue));
            yield break;
        }

        DateTime now = DateTime.UtcNow;
        Action<DateTime> dateTimeAction = (value) =>
        {
            now = value;
        };

        yield return DateComparer.GetCurrentDateAndTime(dateTimeAction);

        TimeSpan difference = now - deviceModel.LastUpdated;

        if (difference.TotalSeconds > Constants.FirebaseConstants.DeviceEnabledTime)
        {
            action?.Invoke(Result.Failure<DeviceModel>(Error.NullValue));
        }
        else
        {
            action?.Invoke(deviceModel);
        }
    }

    public void SendCommand(string command, string deviceId)
    {
        StartCoroutine(SendCommandCoroutine(command, deviceId));
    }

    public IEnumerator SendCommandCoroutine(string command, string deviceId)
    {
        var dbTask = _dbReference.Child("Devices").Child(deviceId).Child("Command").SetValueAsync(command);

        yield return new WaitUntil(() => dbTask.IsCompleted);
    }
}