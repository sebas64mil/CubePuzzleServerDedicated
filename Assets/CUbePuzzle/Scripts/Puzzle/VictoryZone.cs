using System;
using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
public class VictoryZone : MonoBehaviour
{
    [Tooltip("Puertas relacionadas con este objetivo. El sistema comprobará que todas hayan recibido a sus jugadores (PlayerArrived).")]
    [SerializeField] private DoorPuzzleController[] doors;

    [Tooltip("Id del jugador requerido (0 o 1). Si es -1 se ignora y se usa SelectedPlayer.Id para elegir el panel.")]
    [SerializeField] private int requiredPlayerId = -1;

    [Tooltip("Panel(es) de victoria indexados por playerId. Asigna tamaño 2: index 0 -> panel jugador 0, index 1 -> panel jugador 1.")]
    [SerializeField] private GameObject[] winPanels;

    [Tooltip("Referencia al LevelManager (asignar en el inspector). Se usa para bloquear el input de pausa sin desactivar el componente.")]
    [SerializeField] private LevelManager levelManager;

    [Tooltip("Referencia al PlayerManager (asignar en el inspector). Se usará para bloquear el input por click de los players al ganar.")]
    [SerializeField] private PlayerManager playerManager;

    [Tooltip("Si true, bloqueará la pausa global y forzará el estado de pausa del juego.")]
    [SerializeField] private bool preventPause = true;

    [Tooltip("Tiempo de tolerancia (segundos) para esperar a que lleguen los otros jugadores antes de declarar victoria.\n" +
             "Usar un pequeño valor (ej. 0.15 - 0.35) para cubrir frames de lag/lag de red.")]
    [SerializeField, Min(0f)] private float arrivalGracePeriod = 0.25f;

    private bool _victoryTriggered;
    private Coroutine _debounceCoroutine;


    private void Start()
    {
        if (winPanels == null || winPanels.Length == 0)
            winPanels = new GameObject[2];

        SubscribeToDoors();
        CheckVictoryCondition();
    }

    private void OnEnable()
    {
        SubscribeToDoors();
    }

    private void OnDisable()
    {
        UnsubscribeFromDoors();

        if (_debounceCoroutine != null)
        {
            StopCoroutine(_debounceCoroutine);
            _debounceCoroutine = null;
        }
    }

    private void SubscribeToDoors()
    {
        if (doors == null) return;
        foreach (var d in doors)
        {
            if (d == null) continue;
            d.OnPlayerArrivalChanged -= OnDoorArrivalChanged;
            d.OnPlayerArrivalChanged += OnDoorArrivalChanged;
        }
    }

    private void UnsubscribeFromDoors()
    {
        if (doors == null) return;
        foreach (var d in doors)
        {
            if (d == null) continue;
            d.OnPlayerArrivalChanged -= OnDoorArrivalChanged;
        }
    }

    private void OnDoorArrivalChanged(DoorPuzzleController door)
    {
        if (_victoryTriggered) return;

        if (AreAllDoorsArrived())
        {
            CheckVictoryCondition();
            return;
        }

        if (_debounceCoroutine != null)
        {
            StopCoroutine(_debounceCoroutine);
        }
        _debounceCoroutine = StartCoroutine(WaitAndRecheck());
    }

    private IEnumerator WaitAndRecheck()
    {
        yield return new WaitForSeconds(arrivalGracePeriod);
        _debounceCoroutine = null;
        CheckVictoryCondition();
    }

    private bool AreAllDoorsArrived()
    {
        if (doors == null || doors.Length == 0) return true;
        foreach (var d in doors)
        {
            if (d == null) continue;
            if (!d.PlayerArrived) return false;
        }
        return true;
    }

    private void CheckVictoryCondition()
    {
        if (_victoryTriggered) return;

        if (!AreAllDoorsArrived()) return;

        int winnerId = requiredPlayerId >= 0 ? requiredPlayerId : SelectedPlayer.Id;
        TriggerVictory(winnerId);
    }

    private void TriggerVictory(int playerId)
    {
        if (_victoryTriggered) return;
        _victoryTriggered = true;

        Debug.Log($"VictoryZone: jugador {playerId} ha ganado.");

        if (winPanels != null && playerId >= 0 && playerId < winPanels.Length && winPanels[playerId] != null)
        {
            winPanels[playerId].SetActive(true);
        }
        else
        {
            if (winPanels != null && winPanels.Length > 0 && winPanels[0] != null)
                winPanels[0].SetActive(true);
        }

        if (playerManager != null)
        {
            playerManager.SetAllPlayersInputAllowed(false);
        }

        if (preventPause)
        {
            if (levelManager != null)
            {
                levelManager.SetPauseAllowed(false);
                levelManager.PauseMenuVisible(false);
            }

        }
    }

    public void ForceCheck() => CheckVictoryCondition();
}