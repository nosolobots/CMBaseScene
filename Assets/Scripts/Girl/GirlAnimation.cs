using UnityEngine;

public class GirlAnimation : MonoBehaviour
{
    // Intervalo (segundos) para cambiar el parámetro 'idle'
    [SerializeField] float idleChangeInterval = 3f;
    // Duración (segundos) de la transición suave entre valores
    [SerializeField] float transitionDuration = 0.5f;
    
    // Valores posibles para el parámetro 'idle'
    int[] idleValues = new int[] { -1, 0, 1 };

    private Animator _animator;
    private Coroutine _idleRoutine;
    private static readonly int IdleParam = Animator.StringToHash("idle");

    void Awake()
    {
        _animator = GetComponent<Animator>();
        if (_animator == null)
        {
            Debug.LogWarning("GirlAnimation: No se encontró componente Animator en el GameObject.");
        }
    }

    void OnEnable()
    {
        // Inicia la rutina que cambia el parámetro 'idle'
        if (_animator != null && _idleRoutine == null)
            _idleRoutine = StartCoroutine(RandomIdleLoop());
    }

    void OnDisable()
    {
        // Detiene la rutina al desactivar el objeto
        if (_idleRoutine != null)
        {
            StopCoroutine(_idleRoutine);
            _idleRoutine = null;
        }
    }

    private System.Collections.IEnumerator RandomIdleLoop()
    {
        // En cada ciclo: elige un destino aleatorio y realiza una interpolación suave
        while (true)
        {
            if (_animator != null && idleValues != null && idleValues.Length > 0)
            {
                // Valor actual del parámetro
                float start = _animator.GetFloat(IdleParam);
                // Nuevo destino aleatorio
                float target = idleValues[Random.Range(0, idleValues.Length)];

                float elapsed = 0f;
                float dur = Mathf.Max(0f, transitionDuration);
                if (dur <= 0f)
                {
                    // Sin transición: aplicar directamente
                    _animator.SetFloat(IdleParam, target);
                }
                else
                {
                    // Interpolación lineal durante 'transitionDuration'
                    while (elapsed < dur)
                    {
                        elapsed += Time.deltaTime;
                        float t = Mathf.Clamp01(elapsed / dur);
                        float value = Mathf.Lerp(start, target, t);
                        _animator.SetFloat(IdleParam, value);
                        yield return null; // esperar siguiente frame
                    }
                    // Asegurar el valor final exacto
                    _animator.SetFloat(IdleParam, target);
                }
            }
            // Esperar hasta el próximo cambio
            yield return new WaitForSeconds(idleChangeInterval);
        }
    }
}
