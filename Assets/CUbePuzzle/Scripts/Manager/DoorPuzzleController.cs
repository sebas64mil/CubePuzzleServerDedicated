using System;
using UnityEngine;

public class DoorPuzzleController : MonoBehaviour
{
    [Tooltip("Referencia al PuzzleManager (si está vacío se buscará en la escena).")]
    [SerializeField] private PuzzleManager puzzleManager;

    [Tooltip("Material por defecto (puerta cerrada).")]
    [SerializeField] private Material closedMaterial;

    [Tooltip("Material cuando el puzzle se completa (puerta abierta).")]
    [SerializeField] private Material openMaterial;

    [Tooltip("Renderers sobre los que se aplicarán los materiales. Si queda vacío se usarán los Renderers hijos.")]
    [SerializeField] private Renderer[] targetRenderers;

    [Tooltip("Id del jugador para el que está pensada esta puerta (0 o 1). El inspector permite seleccionar 0 ó 1.")]
    [SerializeField, Range(0, 1)] private int requiredPlayerId = 0;

    [Tooltip("Tag que deben tener los jugadores (ej. 'Player').")]
    [SerializeField] private string requiredTag = "Player";

    [Tooltip("Color que se aplicará a la propiedad shader (por defecto '_Color') cuando SelectedPlayer.Id == 0.")]
    [SerializeField] private Color openColorForId0 = Color.cyan;

    [Tooltip("Color que se aplicará a la propiedad shader (por defecto '_Color') cuando SelectedPlayer.Id == 1.")]
    [SerializeField] private Color openColorForId1 = Color.magenta;

    [Tooltip("Nombre de la propiedad del shader que se debe ajustar (por ejemplo '_Color').")]
    [SerializeField] private string colorPropertyName = "_Color";

    private bool _isOpen;

    private bool _playerArrived;

    public event Action<DoorPuzzleController> OnPlayerArrivalChanged;

    public bool IsOpen => _isOpen;

    public bool PlayerArrived => _playerArrived;

    public int RequiredPlayerId => requiredPlayerId;

    private void Reset()
    {
        var r = GetComponent<Renderer>();
        if (r != null) targetRenderers = new[] { r };

        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    private void Start()
    {

        if (targetRenderers == null || targetRenderers.Length == 0)
        {
            targetRenderers = GetComponentsInChildren<Renderer>();
        }

        ApplyMaterialToTargets(closedMaterial);
        _isOpen = false;
        _playerArrived = false;
    }

    private void OnEnable()
    {
        if (puzzleManager != null)
            puzzleManager.OnPuzzleSolved += HandlePuzzleSolved;
    }

    private void OnDisable()
    {
        if (puzzleManager != null)
            puzzleManager.OnPuzzleSolved -= HandlePuzzleSolved;
    }

    private void HandlePuzzleSolved()
    {
        if (_isOpen) return;
        ApplyOpenMaterial();
        _isOpen = true;
    }

    private void ApplyMaterialToTargets(Material mat)
    {
        if (mat == null || targetRenderers == null) return;

        foreach (var rend in targetRenderers)
        {
            if (rend == null) continue;
            rend.material = new Material(mat);
        }
    }

    private void ApplyOpenMaterial()
    {
        if (openMaterial == null || targetRenderers == null) return;

        var intendedColor = requiredPlayerId == 0 ? openColorForId0 : openColorForId1;
        var finalColor = intendedColor;

        foreach (var rend in targetRenderers)
        {
            if (rend == null) continue;
            var inst = new Material(openMaterial);
            if (!string.IsNullOrEmpty(colorPropertyName) && inst.HasProperty(colorPropertyName))
            {
                inst.SetColor(colorPropertyName, finalColor);
            }
            rend.material = inst;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;
        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag)) return;

        var controller = other.GetComponentInParent<PlayerController>();
        if (controller == null) return;

        if (!_isOpen)
        {
            return;
        }

        if (controller.PlayerId == requiredPlayerId)
        {
            SetPlayerArrived(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == null) return;
        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag)) return;

        var controller = other.GetComponentInParent<PlayerController>();
        if (controller == null) return;

        if (controller.PlayerId == requiredPlayerId)
        {
            SetPlayerArrived(false);
        }
    }

    private void SetPlayerArrived(bool arrived)
    {
        if (_playerArrived == arrived) return;
        _playerArrived = arrived;
        Debug.Log($"DoorPuzzleController ({name}): PlayerArrived = {_playerArrived} (requiredId={requiredPlayerId})");
        OnPlayerArrivalChanged?.Invoke(this);
    }

    public void ResetToClosed()
    {
        _isOpen = false;
        _playerArrived = false;
        ApplyMaterialToTargets(closedMaterial);
        OnPlayerArrivalChanged?.Invoke(this);
    }
}