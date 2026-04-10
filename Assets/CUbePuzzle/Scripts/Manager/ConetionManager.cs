using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class ConetionManager : MonoBehaviour
{
    public string baseUrl = "http://localhost:5005/server";

    public event Action<int, PlayerData> OnDataReceived;

    // GET async
    public async Task GetPlayerDataAsync(string gameId, string playerId)
    {
        string url = $"{baseUrl}/{gameId}/{playerId}";

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            await SendWebRequestAsync(webRequest);

            if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                webRequest.result == UnityWebRequest.Result.ProtocolError)
            {

                return;
            }

            string text = webRequest.downloadHandler.text;
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            if (!(text.Contains("posX") || text.Contains("posY") || text.Contains("posZ")))
            {
                return;
            }

            try
            {
                var data = JsonUtility.FromJson<PlayerData>(text);
                if (data == null)
                {
                    return;
                }

                OnDataReceived?.Invoke(Convert.ToInt32(playerId), data);
            }
            catch (Exception ex)
            {
                Debug.LogError($"GET parse error: {ex.Message}");
            }
        }
    }

    // POST async
    public async Task PostPlayerDataAsync(string gameId, string playerId, PlayerData data)
    {
        string url = $"{baseUrl}/{gameId}/{playerId}";
        string jsonData = JsonUtility.ToJson(data);

        using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            await SendWebRequestAsync(webRequest);

            if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"POST Error: {webRequest.error}");
                Debug.LogError($"Response: {webRequest.downloadHandler.text}");
            }
            else
            {
            }
        }
    }

    private static Task SendWebRequestAsync(UnityWebRequest request)
    {
        var tcs = new TaskCompletionSource<object>();
        var op = request.SendWebRequest();
        op.completed += _ =>
        {
            tcs.TrySetResult(null);
        };
        return tcs.Task;
    }
}