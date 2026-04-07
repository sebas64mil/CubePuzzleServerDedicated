using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class PollingClient : IDisposable
{
    public string BaseUrl { get; }
    public string GameId { get; }
    public TimeSpan PollInterval { get; }

    public event Action<int, string> OnJsonReceived;

    private readonly HttpClient _http;
    private CancellationTokenSource _cts;

    public PollingClient(string baseUrl, string gameId, TimeSpan pollInterval)
    {
        BaseUrl = baseUrl?.TrimEnd('/') ?? throw new ArgumentNullException(nameof(baseUrl));
        GameId = gameId;
        PollInterval = pollInterval;
        _http = new HttpClient();
    }

    public async Task StartPollingAsync(int[] playerIds)
    {
        if (playerIds == null || playerIds.Length == 0) throw new ArgumentException("playerIds required", nameof(playerIds));
        if (_cts != null) throw new InvalidOperationException("Already polling");

        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        try
        {
            while (!token.IsCancellationRequested)
            {
                foreach (var playerId in playerIds)
                {
                    if (token.IsCancellationRequested) break;

                    try
                    {
                        string json = await GetPlayerJsonAsync(playerId).ConfigureAwait(false);
                        if (!string.IsNullOrEmpty(json))
                        {
                            OnJsonReceived?.Invoke(playerId, json);
                        }
                        // si json es null => skip (404 u otro error manejado)
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"PollingClient: error getting player {playerId} -> {ex.Message}");
                    }
                }

                await Task.Delay(PollInterval, token).ConfigureAwait(false);
            }
        }
        catch (TaskCanceledException)
        {
            // expected on stop
        }
    }

    public async Task<string> GetPlayerJsonAsync(int playerId)
    {
        string url = string.IsNullOrEmpty(GameId) ? $"{BaseUrl}/{playerId}" : $"{BaseUrl}/{GameId}/{playerId}";
        Debug.Log($"PollingClient: GET {url}");
        var response = await _http.GetAsync(url).ConfigureAwait(false);
        Debug.Log($"PollingClient: GET {url} -> {(int)response.StatusCode} {response.ReasonPhrase}");

        if (!response.IsSuccessStatusCode)
        {
            // evitar lanzar excepción para no spamear la consola; devolver null para indicar fallo
            return null;
        }

        return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
    }

    public void Stop()
    {
        if (_cts == null) return;
        _cts.Cancel();
        _cts.Dispose();
        _cts = null;
    }

    public void Dispose()
    {
        Stop();
        _http?.Dispose();
    }
}