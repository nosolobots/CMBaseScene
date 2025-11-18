using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Waypoint
{
    public Transform waypointTransform;
    [Min(0f)]
    public float waitTime = 2f;
}

public class PathFollow : MonoBehaviour
{
    [Header("Waypoint Settings")]
    [SerializeField] private List<Waypoint> waypoints = new List<Waypoint>();
    [SerializeField] private bool loop = true;
    [SerializeField] private float waypointReachedDistance = 0.5f;

    Animator _animator;
    int _moveParameterID;
    
    private NavMeshAgent _agent;
    private int _currentWaypointIndex = 0;
    private bool _isWaiting = false;
    private float _maxSpeed;

    void Awake()
    {
        // Asegurarse de que el NavMeshAgent está presente
        _agent = GetComponent<NavMeshAgent>();
        if (_agent == null)
        {
            Debug.LogError("PathFollow requiere un NavMeshAgent en el GameObject");
            enabled = false;
        }

        _animator = GetComponent<Animator>();
        if (_animator != null)
        {
            _moveParameterID = Animator.StringToHash("Move");
        }
    }

    void Start()
    {
        // Obtener el NavMeshAgent
        _agent = GetComponent<NavMeshAgent>();
        
        if (_agent == null)
        {
            Debug.LogError("PathFollow requiere un NavMeshAgent en el GameObject");
            enabled = false;
            return;
        }

        // Guardar la velocidad máxima del agente
        _maxSpeed = _agent.speed;

        // Verificar que hay waypoints
        if (waypoints.Count == 0)
        {
            Debug.LogWarning("No hay waypoints asignados en PathFollow");
            return;
        }

        // Iniciar el movimiento hacia el primer waypoint
        MoveToNextWaypoint();
    }

    void Update()
    {
        if (waypoints.Count == 0 || _agent == null)
            return;

        // Actualizar el parámetro del animator basado en la velocidad actual
        UpdateAnimatorMoveParameter();

        // Verificar si hemos llegado al waypoint actual
        if (!_isWaiting && !_agent.pathPending)
        {
            if (_agent.remainingDistance <= waypointReachedDistance)
            {
                StartCoroutine(WaitAtWaypoint());
            }
        }
    }

    private void MoveToNextWaypoint()
    {
        if (waypoints.Count == 0)
            return;

        // Verificar que el waypoint actual es válido
        if (waypoints[_currentWaypointIndex].waypointTransform == null)
        {
            Debug.LogWarning($"Waypoint {_currentWaypointIndex} no tiene Transform asignado");
            AdvanceToNextWaypoint();
            return;
        }

        // Establecer el destino del NavMeshAgent
        _agent.SetDestination(waypoints[_currentWaypointIndex].waypointTransform.position);
    }

    private IEnumerator WaitAtWaypoint()
    {
        _isWaiting = true;

        // Esperar el tiempo configurado para este waypoint
        float waitTime = waypoints[_currentWaypointIndex].waitTime;
        yield return new WaitForSeconds(waitTime);

        // Avanzar al siguiente waypoint
        AdvanceToNextWaypoint();

        _isWaiting = false;
    }

    private void AdvanceToNextWaypoint()
    {
        _currentWaypointIndex++;

        // Si hemos llegado al final de la lista
        if (_currentWaypointIndex >= waypoints.Count)
        {
            if (loop)
            {
                // Volver al inicio
                _currentWaypointIndex = 0;
                MoveToNextWaypoint();
            }
            else
            {
                // Detener el movimiento
                _currentWaypointIndex = waypoints.Count - 1;
                _agent.isStopped = true;
            }
        }
        else
        {
            // Moverse al siguiente waypoint
            MoveToNextWaypoint();
        }
    }

    private void UpdateAnimatorMoveParameter()
    {
        if (_animator == null)
            return;

        // Calcular el valor normalizado de la velocidad [0,1]
        float normalizedSpeed = 0f;
        
        if (_maxSpeed > 0)
        {
            normalizedSpeed = _agent.velocity.magnitude / _maxSpeed;
            normalizedSpeed = Mathf.Clamp01(normalizedSpeed);
        }

        // Actualizar el parámetro del animator
        _animator.SetFloat(_moveParameterID, normalizedSpeed);
    }

    // Métodos públicos para control externo
    public void StopMovement()
    {
        if (_agent != null)
            _agent.isStopped = true;
    }

    public void ResumeMovement()
    {
        if (_agent != null)
        {
            _agent.isStopped = false;
            if (!_isWaiting)
                MoveToNextWaypoint();
        }
    }

    public void ResetPath()
    {
        _currentWaypointIndex = 0;
        _isWaiting = false;
        if (_agent != null)
        {
            _agent.isStopped = false;
            MoveToNextWaypoint();
        }
    }

    // Visualización en el editor
    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Count == 0)
            return;

        Gizmos.color = Color.yellow;

        for (int i = 0; i < waypoints.Count; i++)
        {
            if (waypoints[i].waypointTransform != null)
            {
                // Dibujar esfera en cada waypoint
                Gizmos.DrawWireSphere(waypoints[i].waypointTransform.position, 0.3f);

                // Dibujar línea al siguiente waypoint
                int nextIndex = (i + 1) % waypoints.Count;
                if (nextIndex < waypoints.Count && waypoints[nextIndex].waypointTransform != null)
                {
                    if (loop || nextIndex > i)
                    {
                        Gizmos.DrawLine(
                            waypoints[i].waypointTransform.position,
                            waypoints[nextIndex].waypointTransform.position
                        );
                    }
                }
            }
        }

        // Resaltar el waypoint actual en tiempo de ejecución
        if (Application.isPlaying && _currentWaypointIndex < waypoints.Count)
        {
            if (waypoints[_currentWaypointIndex].waypointTransform != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(waypoints[_currentWaypointIndex].waypointTransform.position, 0.5f);
            }
        }
    }
}
