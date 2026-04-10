using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class Clue : MonoBehaviour
{
    [Tooltip("Tag del GameObject que puede activar esta zona (ej. Player1, Player2).")]
    [SerializeField] private string requiredTag = "Player";

    [Tooltip("Canvas en screen-space que contiene el texto de petición (prompt).")]
    [SerializeField] private GameObject promptCanvasRoot;

    [Tooltip("Componente TextMeshProUGUI dentro del canvas no-world-space que muestra la instrucción.")]
    [SerializeField] private TextMeshProUGUI promptText;

    [Tooltip("Mensaje del prompt. Use {0} para insertar la tecla/acción configurada.")]
    [SerializeField] private string promptMessage = "Presiona {0} para ver la pista";

    [Tooltip("Panel de pista (UI) que se mostrará al activar la acción. Debe estar en un canvas screen-space y por defecto inactivo.")]
    [SerializeField] private GameObject cluePanel;

    [Tooltip("Referencia a la Input Action (nuevo Input System) que activa la pista.")]
    [SerializeField] private InputActionReference activateAction;

    private bool _playerInRange;
    private Collider _currentPlayer;
    private PlayerController _currentPlayerController;
    private bool _clueVisible;

    private void Reset()
    {
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    private void Start()
    {
        if (promptCanvasRoot != null) promptCanvasRoot.SetActive(false);
        if (promptText != null) promptText.gameObject.SetActive(false);
        if (cluePanel != null) cluePanel.SetActive(false);
        _clueVisible = false;
    }

    private void OnEnable()
    {
        if (activateAction != null && activateAction.action != null)
        {
            activateAction.action.Enable();
            activateAction.action.performed += OnActivatePerformed;
        }
    }

    private void OnDisable()
    {
        if (activateAction != null && activateAction.action != null)
        {
            activateAction.action.performed -= OnActivatePerformed;
            activateAction.action.Disable();
        }

        HideAllUi();
        _playerInRange = false;
        _currentPlayer = null;
        _currentPlayerController = null;
        _clueVisible = false;
    }

    private void OnTriggerEnter(Collider other)
    {

        Debug.Log($"Clue: OnTriggerEnter detectado con '{other.name}' (tag={other.tag}).");

        if (other == null) return;
        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag)) return;

        var controller = other.GetComponentInParent<PlayerController>();
        if (controller == null) return;
        if (controller.PlayerId != SelectedPlayer.Id) return;

        _playerInRange = true;
        _currentPlayer = other;
        _currentPlayerController = controller;
        _clueVisible = false;

        ShowPrompt();

        if (cluePanel != null) cluePanel.SetActive(false);

        Debug.Log($"Clue: jugador '{other.name}' (id={controller.PlayerId}) entró en zona de pista.");
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == null) return;
        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag)) return;

        // Asegurarse de que el collider que sale corresponde al jugador que activó la zona
        var controller = other.GetComponentInParent<PlayerController>();
        if (controller == null) return;
        if (controller.PlayerId != SelectedPlayer.Id) return;
        if (!_playerInRange) return;

        _playerInRange = false;
        _currentPlayer = null;
        _currentPlayerController = null;
        _clueVisible = false;

        HideAllUi();

        Debug.Log($"Clue: jugador '{other.name}' (id={controller.PlayerId}) salió de zona de pista.");
    }

    private void OnActivatePerformed(InputAction.CallbackContext context)
    {
        if (!_playerInRange) return;
        if (_currentPlayerController == null) return;
        if (_currentPlayerController.PlayerId != SelectedPlayer.Id) return;

        ToggleClue();
    }

    private void ToggleClue()
    {
        if (_clueVisible)
        {
            HideClue();
            ShowPrompt();
        }
        else
        {
            ShowClue();
        }

        _clueVisible = !_clueVisible;
    }

    private void ShowPrompt()
    {
        if (promptCanvasRoot != null) promptCanvasRoot.SetActive(true);

        if (promptText != null)
        {
            promptText.gameObject.SetActive(true);
            promptText.text = string.Format(promptMessage, GetActionDisplayName());
        }

        if (cluePanel != null) cluePanel.SetActive(false);
    }

    private void ShowClue()
    {
        if (promptText != null) promptText.gameObject.SetActive(false);
        else if (promptCanvasRoot != null) promptCanvasRoot.SetActive(false);

        if (cluePanel != null) cluePanel.SetActive(true);

        Debug.Log("Clue: acción activada, mostrando pista.");
    }

    private void HideClue()
    {
        if (cluePanel != null) cluePanel.SetActive(false);
        Debug.Log("Clue: pista ocultada.");
    }

    private void HideAllUi()
    {
        if (promptCanvasRoot != null) promptCanvasRoot.SetActive(false);
        if (promptText != null) promptText.gameObject.SetActive(false);
        if (cluePanel != null) cluePanel.SetActive(false);
    }

    private string GetActionDisplayName()
    {
        if (activateAction == null || activateAction.action == null) return "E";

        try
        {
            var action = activateAction.action;
            var display = action.GetBindingDisplayString();
            if (!string.IsNullOrEmpty(display)) return display;
            return action.name;
        }
        catch
        {
            return activateAction.action.name;
        }
    }
}
