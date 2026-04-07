using System;
using UnityEngine;
using UnityEngine.AI;

public class PlayerClickMover : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private LayerMask raycastMask = ~0;
    [SerializeField] private float maxRayDistance = 100f;
    [SerializeField] private float navSampleDistance = 1.0f;

    public event Action<Vector3> OnTargetSelected;

    void Start()
    {
        if (targetCamera == null) targetCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (targetCamera == null) return;

            Ray ray = targetCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, raycastMask))
            {
                if (NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, navSampleDistance, NavMesh.AllAreas))
                {
                    OnTargetSelected?.Invoke(navHit.position);
                }
                else
                {
                    Debug.LogWarning("PlayerClickMover: punto fuera de NavMesh.");
                }
            }
        }
    }
}