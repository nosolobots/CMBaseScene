using System.Collections.Generic;
using UnityEngine;

public class AnimationLooper : MonoBehaviour
{
    [SerializeField] int delayBeforeAnimation = 2;
    [SerializeField] bool allowNone = true;

    protected List<int> _animationsList;
    protected Animator _animator;

    protected virtual void Awake()
    {
        _animator = GetComponent<Animator>();
        _animationsList = new List<int>();
    }

    void Start()
    {
        StartCoroutine(SelectAnimation(allowNone));
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    System.Collections.IEnumerator SelectAnimation(bool none=true)
    {
        if (_animator == null || _animationsList.Count == 0)
            yield break; // No hay animaciones para reproducir

        while (true)
        {
            // Esperar el tiempo antes de iniciar la animación
            yield return new WaitForSeconds(delayBeforeAnimation);

            if (none)
            {
                // Opción para no reproducir ninguna animación (50% de probabilidad)
                if (Random.Range(0, 2) == 0) 
                    continue;
            }

            // Seleccionar una animación aleatoria
            int randomIndex = Random.Range(0, _animationsList.Count);
            int selectedAnimation = _animationsList[randomIndex];

            // Activar la animación seleccionada
            _animator.SetTrigger(selectedAnimation);

            // Esperar hasta que la animación comience
            while (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.1f)
            {
                yield return null; // Esperar hasta que la animación comience
            }

            // Esperar hasta que la animación termine
            while (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            {
                yield return null; // Esperar hasta que la animación termine
            }
        }
    }
}