using UnityEngine;

[System.Flags]
public enum BruteAnimations
{
    None = 0,
    Crouch = 1 << 0,
    AxeIdle = 1 << 1,
}

public class BruteAnimationLooper : AnimationLooper
{
    [SerializeField] BruteAnimations _animations;

    protected override void Awake()
    {
        base.Awake();

        if ((_animations & BruteAnimations.Crouch) != 0)
            _animationsList.Add(Animator.StringToHash(BruteAnimations.Crouch.ToString()));
        if ((_animations & BruteAnimations.AxeIdle) != 0)
            _animationsList.Add(Animator.StringToHash(BruteAnimations.AxeIdle.ToString()));
    }
}
