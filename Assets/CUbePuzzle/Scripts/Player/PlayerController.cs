using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private int playerId;

    // Public getter to allow other components to verify the player ID
    public int PlayerId => playerId;

    public PlayerClickMover clickMover;

    [Header("NavMeshAgent Avoidance")]
    [SerializeField] private float agentRadius = 0.5f;
    [SerializeField] private float agentHeight = 2f;
    [SerializeField] private int baseAvoidancePriority = 50;
    [SerializeField] private ObstacleAvoidanceType avoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
    [SerializeField] private bool useAutoBraking = true;

    [Header("OffMeshLink Jump")]
    [Tooltip("Nombre del area NavMesh que identifica links de salto. Dejar vacío para aceptar cualquier OffMeshLink.")]
    [SerializeField] private string jumpAreaName = "Jump";
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float jumpDuration = 0.6f;

    private NavMeshAgent _agent;
    private bool _isTraversingLink = false;

    private bool _movementAllowed = true;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        if (_agent == null)
        {
            Debug.LogError("PlayerController: NavMeshAgent no encontrado.");
            return;
        }

        _agent.radius = agentRadius;
        _agent.height = agentHeight;
        _agent.obstacleAvoidanceType = avoidanceType;
        _agent.autoBraking = useAutoBraking;

        int priority = Mathf.Clamp(baseAvoidancePriority + playerId * 10, 1, 99);
        _agent.avoidancePriority = priority;

        _agent.updatePosition = true;
        _agent.updateRotation = true;

        _agent.autoTraverseOffMeshLink = false;
    }

    void Update()
    {
        if (_agent != null && _agent.isOnNavMesh && _agent.isOnOffMeshLink && !_isTraversingLink)
        {
            var data = _agent.currentOffMeshLinkData;
            bool shouldJump = false;

            if (data.owner != null)
            {

                var owner = data.owner;
                var ownerType = owner.GetType();
                var areaProp = ownerType.GetProperty("area", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (areaProp != null)
                {
                    try
                    {
                        object val = areaProp.GetValue(owner);
                        if (val is int linkArea)
                        {
                            if (string.IsNullOrEmpty(jumpAreaName))
                            {
                                shouldJump = true;
                            }
                            else
                            {
                                int jumpArea = NavMesh.GetAreaFromName(jumpAreaName);
                                if (linkArea == jumpArea) shouldJump = true;
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(jumpAreaName)) shouldJump = true;
                        }
                    }
                    catch
                    {
                        if (string.IsNullOrEmpty(jumpAreaName)) shouldJump = true;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(jumpAreaName)) shouldJump = true;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(jumpAreaName)) shouldJump = true;
            }

            if (shouldJump)
            {
                StartCoroutine(TraverseJumpOffMeshLink(data.startPos, data.endPos));
            }
            else
            {
                _agent.CompleteOffMeshLink();
            }
        }
    }

    public void MovePlayer(Vector3 position)
    {
        if (!_movementAllowed) return;

        if (_agent != null && _agent.enabled && _agent.isOnNavMesh)
        {
            _agent.SetDestination(position);
        }
        else
        {
            transform.position = position;
        }
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    private IEnumerator TraverseJumpOffMeshLink(Vector3 startPos, Vector3 endPos)
    {
        _isTraversingLink = true;
        _agent.updatePosition = false;

        float elapsed = 0f;
        while (elapsed < jumpDuration)
        {
            float t = Mathf.Clamp01(elapsed / jumpDuration);
            Vector3 pos = Vector3.Lerp(startPos, endPos, t);
            pos.y += Mathf.Sin(t * Mathf.PI) * jumpHeight;
            _agent.transform.position = pos;
            elapsed += Time.deltaTime;
            yield return null;
        }

        _agent.transform.position = endPos;
        _agent.CompleteOffMeshLink();
        _agent.updatePosition = true;
        _isTraversingLink = false;
    }

    public void SetMovementAllowed(bool allowed)
    {
        _movementAllowed = allowed;
        if (_agent != null)
        {
            _agent.isStopped = !allowed;
            if (!allowed)
            {
                _agent.ResetPath();
            }
        }

        if (clickMover != null)
            clickMover.enabled = allowed;
    }
}