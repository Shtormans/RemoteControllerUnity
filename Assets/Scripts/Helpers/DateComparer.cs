using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public static class DateComparer
{
    private const string WorldTimeAPIURL = "https://worldtimeapi.org/api/ip";

    public static IEnumerator GetCurrentDateAndTime(Action<DateTime> callback)
    {
        UnityWebRequest request = UnityWebRequest.Get(WorldTimeAPIURL);

        request.SendWebRequest().completed += (asyncOperation) =>
        {
            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                callback.Invoke(DateTime.UtcNow);
            }
            else
            {
                string jsonResponse = request.downloadHandler.text;
                WorldTimeResponse response = JsonUtility.FromJson<WorldTimeResponse>(jsonResponse);

                callback.Invoke(DateTime.Parse(response.utc_datetime));
            }
        };

        yield return new WaitUntil(() => request.isDone);
    }

    [Serializable]
    public class WorldTimeResponse
    {
        public string utc_datetime;
    }
}