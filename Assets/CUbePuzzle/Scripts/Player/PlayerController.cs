using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private int playerId;

    public PlayerClickMover clickMover;

    private NavMeshAgent _agent;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        if (_agent == null)
        {
            Debug.LogError("PlayerController: NavMeshAgent no encontrado.");
        }
    }

    public void MovePlayer(Vector3 position)
    {
        if (_agent != null && _agent.enabled)
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