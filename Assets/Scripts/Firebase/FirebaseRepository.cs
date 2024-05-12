using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using static System.Collections.Specialized.BitVector32;

public class FirebaseRepository : MonoBehaviour
{
    [SerializeField] private int _amountInScoreTable = 100;
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

    public void SaveScore(int score, Action<Result> action = null)
    {
        StartCoroutine(SaveUsername(action));
        StartCoroutine(UpdateScore(score, action));
    }

    public void GetMaxScore(Action<Result<int>> action)
    {
        StartCoroutine(AwaitGetMaxScore(action));
    }

    public void GetLeaderboard(Action<Result<List<UserWithScore>>> action = null)
    {
        StartCoroutine(GetScoreBoard(action));
    }

    public void SetTempKey(string tempKey)
    {
        StartCoroutine(SetTempKeyCoroutine(tempKey));
    }

    public void GetUserDevices(Action<List<DeviceModel>> action)
    {
        StartCoroutine(GetUserDevicesCoroutine(action));
    }

    private IEnumerator AwaitGetMaxScore(Action<Result<int>> action)
    {
        var userId = _auth.CurrentUser.UserId;

        var dbTask = _dbReference.Child("Users").Child(userId).Child("Score").GetValueAsync();

        yield return new WaitUntil(() => dbTask.IsCompleted);

        if (dbTask.Exception != null)
        {
            Debug.LogWarning(dbTask.Exception.Message);
            var callbackResult = Result.Failure<int>(
                new Error("Database.Exception", dbTask.Exception.Message)
            );

            action?.Invoke(callbackResult);

            yield break;
        }

        DataSnapshot snapshot = dbTask.Result;
        var result = int.Parse(snapshot.GetValue(false).ToString());

        action?.Invoke(result);
    }

    private IEnumerator GetScoreBoard(Action<Result<List<UserWithScore>>> action)
    {
        var dbTask = _dbReference.Child("Users").OrderByChild("Score").LimitToFirst(_amountInScoreTable).GetValueAsync();

        yield return new WaitUntil(() => dbTask.IsCompleted);

        if (dbTask.Exception != null)
        {
            Debug.LogWarning(dbTask.Exception.Message);
            var callbackResult = Result.Failure<List<UserWithScore>>(
                new Error("Database.Exception", dbTask.Exception.Message)
            );

            action?.Invoke(callbackResult);

            yield break;
        }

        DataSnapshot snapshot = dbTask.Result;

        var result = snapshot.Children.Reverse()
            .Select(item =>
            {
                return new UserWithScore()
                {
                    UserId = item.Key.ToString(),
                    Username = item.Child("Username").Value.ToString(),
                    Score = int.Parse(item.Child("Score").Value.ToString())
                };
            })
            .ToList();

        action?.Invoke(result);
    }

    private IEnumerator SaveUsername(Action<Result> action)
    {
        var dbTask = _dbReference.Child("Users").Child(_auth.CurrentUser.UserId).Child("Username").SetValueAsync(_auth.CurrentUser.DisplayName);

        yield return new WaitUntil(() => dbTask.IsCompleted);

        if (dbTask.Exception != null)
        {
            var result = Result.Failure(
                new Error("Database.Exception", dbTask.Exception.Message)
            );

            action?.Invoke(result);
        }
        else
        {
            var result = Result.Success();
            action?.Invoke(result);
        }
    }

    private IEnumerator UpdateScore(int score, Action<Result> action)
    {
        var dbTask = _dbReference.Child("Users").Child(_auth.CurrentUser.UserId).Child("Score").SetValueAsync(score);

        yield return new WaitUntil(() => dbTask.IsCompleted);

        if (dbTask.Exception != null)
        {
            var result = Result.Failure(
                new Error("Database.Exception", dbTask.Exception.Message)
            );

            action?.Invoke(result);
        }
        else
        {
            var result = Result.Success();
            action?.Invoke(result);
        }
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

    private IEnumerator GetUserDevicesCoroutine(Action<List<DeviceModel>> action)
    {
        var dbTask = _dbReference.Child("Users").Child(_auth.CurrentUser.UserId).Child("Devices").GetValueAsync();

        yield return new WaitUntil(() => dbTask.IsCompleted);

        DataSnapshot snapshot = dbTask.Result;

        PlayerPrefsRepository.TryGetDeviceIdKey(out string devideId);

        List<DeviceModel> deviceModels = snapshot.Children
            .Select(item =>
            {
                return new DeviceModel()
                {
                    Id = item.Key.ToString(),
                    Name = item.Value.ToString(),
                    IsCurrentDevice = devideId == item.Key.ToString()
                };
            })
            .ToList();

        action?.Invoke(deviceModels);
    }
}