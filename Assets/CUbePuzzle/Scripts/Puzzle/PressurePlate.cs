using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PressurePlate : MonoBehaviour
{
    [Tooltip("Tag del GameObject que puede activar esta placa (ej. Player1, Player2).")]
    [SerializeField] private string requiredTag = "Player";

    [Tooltip("Si es true, una placa presionada queda bloqueada y no se liberará al salir del collider hasta que se reinicie la secuencia.")]
    [SerializeField] private bool latchOnPress = true;

    [Tooltip("Animator que controla la animación de la placa.")]
    [SerializeField] private Animator _animator;

    // Control global para habilitar/deshabilitar la detección de placas durante el cooldown del puzzle.
    public static bool GlobalEnabled { get; set; } = true;

    public int PlateIndex { get; set; }

    public bool IsPressed { get; private set; }

    private bool _latched;
    private Collider _collider;

    public event Action<PressurePlate, Collider> OnPressed;
    public event Action<PressurePlate, Collider> OnReleased;

    private static readonly int IsPressedHash = Animator.StringToHash("IsPressed");

    private void Reset()
    {
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    private void Awake()
    {
        _collider = GetComponent<Collider>();

        if (_animator == null)
        {
            _animator = GetComponent<Animator>();
            if (_animator == null)
                _animator = GetComponentInChildren<Animator>();
        }

        if (_animator == null)
        {
            Debug.LogWarning($"PressurePlate[{PlateIndex}]: Animator no asignado en el inspector ni encontrado en el GameObject.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;
        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag)) return;

        if (!GlobalEnabled)
        {
            Debug.Log($"PressurePlate[{PlateIndex}]: OnTriggerEnter ignorado porque GlobalEnabled=false ({other.name}).");
            return;
        }

        if (!IsPressed)
        {
            IsPressed = true;
            OnPressed?.Invoke(this, other);

            TrySetAnimatorBool(true);

            Debug.Log($"PressurePlate[{PlateIndex}]: presionada por {other.name}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == null) return;
        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag)) return;

        if (_latched)
        {
            Debug.Log($"PressurePlate[{PlateIndex}]: salida ignorada por bloqueo (latched) {other.name}");
            return;
        }

        if (IsPressed)
        {
            IsPressed = false;
            OnReleased?.Invoke(this, other);
            TrySetAnimatorBool(false);
            Debug.Log($"PressurePlate[{PlateIndex}]: liberada por {other.name}");
        }
    }


    public void DetectOccupants()
    {
        if (_collider == null) _collider = GetComponent<Collider>();
        if (_collider == null) return;

        if (IsPressed) return; 

        var center = _collider.bounds.center;
        var extents = _collider.bounds.extents;
        Collider[] hits = Physics.OverlapBox(center, extents, transform.rotation, ~0, QueryTriggerInteraction.Collide);

        foreach (var hit in hits)
        {
            if (hit == null) continue;
            if (!string.IsNullOrEmpty(requiredTag) && !hit.CompareTag(requiredTag)) continue;

            IsPressed = true;
            OnPressed?.Invoke(this, hit);
            TrySetAnimatorBool(true);

            Debug.Log($"PressurePlate[{PlateIndex}]: DetectOccupants encontró {hit.name} y marcó la placa presionada.");
            return;
        }
    }


    public void ApplyPressResult(bool isCorrect)
    {
        if (isCorrect)
        {
            IsPressed = true;
            if (latchOnPress) _latched = true;

            TrySetAnimatorBool(true);

            Debug.Log($"PressurePlate[{PlateIndex}]: ApplyPressResult -> CORRECTA (latched={_latched})");
        }
        else
        {
            if (IsPressed)
            {
                IsPressed = false;
                OnReleased?.Invoke(this, null);
            }

            TrySetAnimatorBool(false);

            _latched = false;

            Debug.Log($"PressurePlate[{PlateIndex}]: ApplyPressResult -> INCORRECTA. Revertida a false.");
        }
    }

    public void ForceReset()
    {
        if (IsPressed)
        {
            IsPressed = false;
            OnReleased?.Invoke(this, null);
        }

        TrySetAnimatorBool(false);

        _latched = false;

        Debug.Log($"PressurePlate[{PlateIndex}]: ForceReset ejecutado. IsPressed={IsPressed}, latched={_latched}");
    }

    private void TrySetAnimatorBool(bool value)
    {
        if (_animator == null)
        {
            Debug.LogWarning($"PressurePlate[{PlateIndex}]: no hay Animator asignado, no se puede cambiar 'IsPressed' a {value}.");
            return;
        }

        if (!_animator.isActiveAndEnabled)
        {
            Debug.LogWarning($"PressurePlate[{PlateIndex}]: Animator no activo/habilitado al intentar setear 'IsPressed' a {value}.");
            _animator.SetBool(IsPressedHash, value);
            return;
        }

        try
        {
            _animator.SetBool(IsPressedHash, value);
            Debug.Log($"PressurePlate[{PlateIndex}]: Animator 'IsPressed' seteado a {value}.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"PressurePlate[{PlateIndex}]: error al setear parametro Animator: {ex.Message}");
        }
    }
}