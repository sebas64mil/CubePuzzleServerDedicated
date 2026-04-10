using System;
using System.Collections;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    [Tooltip("Array de placas en el orden requerido para resolver el puzzle.")]
    [SerializeField] private PressurePlate[] plates;

    [Tooltip("Si se pulsa una placa fuera de orden, reiniciar la secuencia.")]
    [SerializeField] private bool resetOnWrongPress = true;

    [Tooltip("Tiempo (s) que se espera después de un reinicio antes de permitir intentos de nuevo.")]
    [SerializeField] private float retryCooldown = 1.0f;

    public event Action OnPuzzleSolved;

    private int _expectedIndex = 0;
    private bool _isSolved = false;

    private Coroutine _resetCoroutine;

    private void Start()
    {
        if (plates == null || plates.Length == 0)
        {
            Debug.LogWarning("PuzzleManager: no hay placas asignadas.");
            return;
        }

        for (int i = 0; i < plates.Length; i++)
        {
            var p = plates[i];
            if (p == null)
            {
                Debug.LogWarning($"PuzzleManager: placa nula en índice {i}.");
                continue;
            }

            p.PlateIndex = i;
            p.OnPressed += HandlePlatePressed;
        }
    }

    private void OnDestroy()
    {
        if (plates == null) return;
        foreach (var p in plates)
        {
            if (p == null) continue;
            p.OnPressed -= HandlePlatePressed;
        }
    }

    private void HandlePlatePressed(PressurePlate plate, Collider by)
    {
        if (_isSolved) return;
        if (plate == null) return;

        int idx = plate.PlateIndex;
        bool isCorrect = idx == _expectedIndex;

        if (isCorrect)
        {
            Debug.Log($"PuzzleManager: placa correcta [{idx}]");
            plate.ApplyPressResult(true);

            _expectedIndex++;

            if (_expectedIndex >= plates.Length)
            {
                _isSolved = true;
                Debug.Log("PuzzleManager: puzzle resuelto.");
                OnPuzzleSolved?.Invoke();
            }
        }
        else
        {
            Debug.Log($"PuzzleManager: placa incorrecta [{idx}] (esperaba {_expectedIndex})");

            if (resetOnWrongPress)
            {
                ResetSequence();
            }
            else
            {
                plate.ApplyPressResult(false);
            }
        }
    }



    private void ResetSequence()
    {
        _expectedIndex = 0;

        PressurePlate.GlobalEnabled = false;

        if (_resetCoroutine != null)
        {
            StopCoroutine(_resetCoroutine);
            _resetCoroutine = null;
        }

        _resetCoroutine = StartCoroutine(ResetCooldownRoutine());

        Debug.Log("PuzzleManager: secuencia reiniciada (cooldown activado). Las placas se mantienen presionadas hasta el fin del cooldown.");
    }

    private IEnumerator ResetCooldownRoutine()
    {
        yield return new WaitForSeconds(retryCooldown);

        if (plates != null)
        {
            foreach (var p in plates)
            {
                if (p != null) p.ForceReset();
            }
        }

        PressurePlate.GlobalEnabled = true;

        _resetCoroutine = null;
        Debug.Log("PuzzleManager: cooldown terminado, todas las placas liberadas y se permiten nuevos intentos.");
    }

    public void ResetPuzzle()
    {
        _isSolved = false;
        ResetSequence();
    }
}
