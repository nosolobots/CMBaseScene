using UnityEngine;
using System.Collections.Generic;

[System.Flags]
public enum OldManAnimations
{
    None = 0,
    Secret = 1 << 0,
    Look = 1 << 1,
}

public class OldManAnimation : MonoBehaviour
{
    //static readonly int TellingASecretParam = Animator.StringToHash("Secret");
    //static readonly int IdleLookingParam = Animator.StringToHash("Look");

    [Header("Animaciones")]
    [SerializeField] int delayBefore = 2;
    [SerializeField] OldManAnimations _animations;

    List<int> _animationsList = new List<int>();

    Animator _animator;

    void Awake()
    {
        _animator = GetComponent<Animator>();
        
        if ((_animations & OldManAnimations.Secret) != 0)
            //_animationsList.Add(TellingASecretParam);
            _animationsList.Add(Animator.StringToHash(OldManAnimations.Secret.ToString()));
        if ((_animations & OldManAnimations.Look) != 0)
            //_animationsList.Add(IdleLookingParam);
            _animationsList.Add(Animator.StringToHash(OldManAnimations.Look.ToString()));
    }

    void Start()
    {
        StartCoroutine(OldManIdleRoutine());
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    System.Collections.IEnumerator OldManIdleRoutine()
    {
        if (_animator == null || _animationsList.Count == 0)
            yield break; // No hay animaciones para reproducir

        while (true)
        {
            // Esperar el tiempo antes de iniciar la animación
            yield return new WaitForSeconds(delayBefore);

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
