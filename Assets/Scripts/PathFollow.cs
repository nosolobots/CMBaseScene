using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class PathFollow : MonoBehaviour
{
    [Header("Path Follow Settings")]
    [SerializeField] PathWaypoints pathWaypoints;
    [SerializeField] bool loop = true;
    [SerializeField] float waypointReachedDistance = 0.5f;

    List<Waypoint> _waypoints;
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

        // Obtenemos los waypoints
        _waypoints = pathWaypoints.Waypoints;
}

    void Start()
    {
        // Iniciar el movimiento hacia el primer waypoint
        MoveToNextWaypoint();
    }

    void Update()
    {
        if (_waypoints.Count == 0 || _agent == null)
            return;

        // Actualizar el parámetro del animator basado en la velocidad actual
        UpdateAnimatorMoveParameter();

        // Verificar si hemos llegado al waypoint actual
        if (!_isWaiting && !_agent.pathPending)
        {
            if (_agent.remainingDistance <= waypointReachedDistance)
            {
                // Iniciar la espera en el waypoint
                StartCoroutine(WaitAtWaypoint());
            }
        }
    }

    private void MoveToNextWaypoint()
    {
        if (_waypoints.Count == 0)
            return;

        // Verificar que el waypoint actual es válido
        if (_waypoints[_currentWaypointIndex].waypointTransform == null)
        {
            Debug.LogWarning($"Waypoint {_currentWaypointIndex} no tiene Transform asignado");
            AdvanceToNextWaypoint();
            return;
        }

        // Establecer el destino del NavMeshAgent
        _agent.SetDestination(_waypoints[_currentWaypointIndex].waypointTransform.position);
    }

    private IEnumerator WaitAtWaypoint()
    {
        _isWaiting = true;

        // Ajustar la orientación del agente al waypoint
        if (_waypoints[_currentWaypointIndex].orientAgent)
        {
            yield return StartCoroutine(OrientateAgentAsWaypoint());
        }

        // Esperar el tiempo configurado para este waypoint
        float waitTime = _waypoints[_currentWaypointIndex].waitTime;
        yield return new WaitForSeconds(waitTime);

        // Avanzar al siguiente waypoint
        AdvanceToNextWaypoint();

        _isWaiting = false;
    }

    private IEnumerator OrientateAgentAsWaypoint()
    {
        Quaternion targetRotation = _waypoints[_currentWaypointIndex].waypointTransform.rotation;
        
        float elapsedTime = 0f;
        while (elapsedTime < 1f)
        {
            _agent.transform.rotation = Quaternion.Slerp(
                _agent.transform.rotation,
                targetRotation,
                elapsedTime
            );
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        _agent.transform.rotation = targetRotation;
    }


    private void AdvanceToNextWaypoint()
    {
        _currentWaypointIndex++;

        // Si hemos llegado al final de la lista
        if (_currentWaypointIndex >= _waypoints.Count)
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
                _currentWaypointIndex = _waypoints.Count - 1;
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
        if (_waypoints == null || _waypoints.Count == 0)
            return;

        Gizmos.color = Color.yellow;

        for (int i = 0; i < _waypoints.Count; i++)
        {
            if (_waypoints[i].waypointTransform != null)
            {
                // Dibujar esfera en cada waypoint
                Gizmos.DrawWireSphere(_waypoints[i].waypointTransform.position, 0.3f);

                // Dibujar línea al siguiente waypoint
                int nextIndex = (i + 1) % _waypoints.Count;
                if (nextIndex < _waypoints.Count && _waypoints[nextIndex].waypointTransform != null)
                {
                    if (loop || nextIndex > i)
                    {
                        Gizmos.DrawLine(
                            _waypoints[i].waypointTransform.position,
                            _waypoints[nextIndex].waypointTransform.position
                        );
                    }
                }
            }
        }

        // Resaltar el waypoint actual en tiempo de ejecución
        if (Application.isPlaying && _currentWaypointIndex < _waypoints.Count)
        {
            if (_waypoints[_currentWaypointIndex].waypointTransform != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(_waypoints[_currentWaypointIndex].waypointTransform.position, 0.5f);
            }
        }
    }
}
