using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Header("Network")]
    [SerializeField] private ConetionManager conetionManager;
    [SerializeField] private string gameId = "1";
    [SerializeField] private float pollInterval = 0.2f;
    [SerializeField] private float remoteTimeout = 1.0f;

    [Header("Players (index == playerId)")]
    [SerializeField] private List<PlayerController> players;

    private int myId;
    private int otherId;
    private Coroutine _pollCoroutine;

    private bool _remoteSeen = false;
    private float _lastRemoteReceivedTime = Mathf.NegativeInfinity;

    private PlayerClickMover _clickMover;

    private Vector3[] _initialPositions;

    void Start()
    {
        myId = SelectedPlayer.Id;
        otherId = myId == 0 ? 1 : 0;

        if (players == null || players.Count < 2)
        {
            Debug.LogError("PlayerManager: se requieren 2 PlayerController en 'players' (index == playerId).");
            return;
        }

        _initialPositions = new Vector3[players.Count];
        for (int i = 0; i < players.Count; i++)
        {
            var p = players[i];
            _initialPositions[i] = p != null ? p.transform.position : Vector3.zero;
        }

        var remote = players[otherId];
        if (remote != null && remote.gameObject != null)
        {
            remote.MovePlayer(_initialPositions[otherId]);
            remote.gameObject.SetActive(false);
        }

        if (conetionManager == null)
        {
            Debug.LogError("PlayerManager: ConetionManager no encontrado en la escena.");
            return;
        }

        conetionManager.OnDataReceived += OnDataReceived;

        var localController = players[myId];
        if (localController != null && localController.clickMover != null)
        {
            _clickMover = localController.clickMover;
            _clickMover.OnTargetSelected += HandleTargetSelected;
        }
        else
        {
            Debug.LogWarning("PlayerManager: PlayerClickMover no asignado en el PlayerController local. Asigna 'clickMover' en el prefab o en escena.");
        }

        _pollCoroutine = StartCoroutine(PollRoutine());
    }

    private void Update()
    {
        if (_remoteSeen && Time.time - _lastRemoteReceivedTime > remoteTimeout)
        {
            var remote = players[otherId];
            if (remote != null && remote.gameObject != null)
            {
                remote.MovePlayer(_initialPositions[otherId]);
                remote.gameObject.SetActive(false);
            }
            _remoteSeen = false;
        }
    }

    private void HandleTargetSelected(Vector3 targetPosition)
    {
        var local = players[myId];
        if (local != null)
        {
            local.MovePlayer(targetPosition);
        }

        if (conetionManager != null)
        {
            var pdata = new PlayerData
            {
                posX = targetPosition.x,
                posY = targetPosition.y,
                posZ = targetPosition.z
            };

            _ = conetionManager.PostPlayerDataAsync(gameId, myId.ToString(), pdata);
        }
    }

    private IEnumerator PollRoutine()
    {
        var wait = new WaitForSeconds(pollInterval);

        while (true)
        {
            _ = conetionManager.GetPlayerDataAsync(gameId, otherId.ToString());

            var localController = players[myId];
            if (localController != null)
            {
                Vector3 pos = localController.GetPosition();
                var pdata = new PlayerData
                {
                    posX = pos.x,
                    posY = pos.y,
                    posZ = pos.z
                };

                _ = conetionManager.PostPlayerDataAsync(gameId, myId.ToString(), pdata);
            }

            yield return wait;
        }
    }

    private void OnDataReceived(int playerId, PlayerData data)
    {
        if (players == null) return;
        if (playerId < 0 || playerId >= players.Count) return;
        var controller = players[playerId];
        if (controller == null || data == null) return;

        if (playerId == otherId)
        {
            _lastRemoteReceivedTime = Time.time;

            if (!_remoteSeen)
            {
                var go = controller.gameObject;
                if (go != null) go.SetActive(true);
                _remoteSeen = true;
            }

            Vector3 position = new Vector3(data.posX, data.posY, data.posZ);
            controller.MovePlayer(position);
        }
        else
        {
            Vector3 position = new Vector3(data.posX, data.posY, data.posZ);
            controller.MovePlayer(position);
        }
    }

    void OnDisable()
    {
        if (conetionManager != null)
            conetionManager.OnDataReceived -= OnDataReceived;

        if (_clickMover != null)
            _clickMover.OnTargetSelected -= HandleTargetSelected;

        if (_pollCoroutine != null)
        {
            StopCoroutine(_pollCoroutine);
            _pollCoroutine = null;
        }
    }

    public void SetAllPlayersInputAllowed(bool allowed)
    {
        if (players == null) return;
        foreach (var p in players)
        {
            if (p == null) continue;
            if (p.clickMover != null)
            {
                p.clickMover.enabled = allowed;
            }
        }
    }
}