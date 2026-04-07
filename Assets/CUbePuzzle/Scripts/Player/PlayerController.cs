using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private int playerId;

    public PlayerClickMover clickMover;

    [Header("NavMeshAgent Avoidance")]
    [SerializeField] private float agentRadius = 0.5f;
    [SerializeField] private float agentHeight = 2f;
    [SerializeField] private int baseAvoidancePriority = 50;
    [SerializeField] private ObstacleAvoidanceType avoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
    [SerializeField] private bool useAutoBraking = true;

    private NavMeshAgent _agent;

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
    }

    public void MovePlayer(Vector3 position)
    {
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

}