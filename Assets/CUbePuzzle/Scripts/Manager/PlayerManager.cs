using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Header("Network")]
    [SerializeField] private ConetionManager conetionManager;
    [SerializeField] private string gameId = "1";
    [SerializeField] private float pollInterval = 0.2f;

    [Header("Players (index == playerId)")]
    [SerializeField] private List<PlayerController> players;

    private int myId;
    private int otherId;
    private Coroutine _pollCoroutine;

    private bool _remoteSeen = false;

    private PlayerClickMover _clickMover;

    void Start()
    {
        myId = SelectedPlayer.Id;
        otherId = myId == 0 ? 1 : 0;

        if (players == null || players.Count < 2)
        {
            Debug.LogError("PlayerManager: se requieren 2 PlayerController en 'players' (index == playerId).");
            return;
        }

        // Ocultar visual del remoto hasta recibir su primera posición
        var remote = players[otherId];
        if (remote != null && remote.gameObject != null)
        {
            remote.gameObject.SetActive(false);
        }

        if (conetionManager == null)
        {
            Debug.LogError("PlayerManager: ConetionManager no encontrado en la escena.");
            return;
        }

        conetionManager.OnDataReceived += OnDataReceived;

        // Obtener PlayerClickMover desde el PlayerController local (evita FindObjectOfType)
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

        // Empieza polling en coroutine (ConetionManager hace las llamadas async)
        _pollCoroutine = StartCoroutine(PollRoutine());
    }

    private void HandleTargetSelected(Vector3 targetPosition)
    {
        // Mover local
        var local = players[myId];
        if (local != null)
        {
            local.MovePlayer(targetPosition);
        }

        // Enviar posición inmediatamente al servidor (fire-and-forget)
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
            // Pedir posición del otro cliente (ConetionManager invocará OnDataReceived cuando llegue)
            _ = conetionManager.GetPlayerDataAsync(gameId, otherId.ToString());

            // Enviar nuestra posición periódicamente también (por si hay cambios fuera de clicks)
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

    // Ahora trabaja únicamente con PlayerData
    private void OnDataReceived(int playerId, PlayerData data)
    {
        if (players == null) return;
        if (playerId < 0 || playerId >= players.Count) return;
        var controller = players[playerId];
        if (controller == null || data == null) return;

        // activar remoto la primera vez que llega data
        if (playerId == otherId && !_remoteSeen)
        {
            var go = controller.gameObject;
            if (go != null) go.SetActive(true);
            _remoteSeen = true;
        }

        Vector3 position = new Vector3(data.posX, data.posY, data.posZ);
        controller.MovePlayer(position);
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
}