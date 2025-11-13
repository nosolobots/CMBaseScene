using UnityEngine;

[System.Flags]
public enum BruteAnimations
{
    None = 0,
    CrouchDefense = 1 << 0,
    AxeSwing = 1 << 1,
    BigStrike = 1 << 2,
}

public class BruteAnimationLooper : AnimationLooper
{
    [SerializeField] BruteAnimations _animations;

    protected override void Awake()
    {
        base.Awake();

        if ((_animations & BruteAnimations.CrouchDefense) != 0)
            _animationsList.Add(Animator.StringToHash(BruteAnimations.CrouchDefense.ToString()));
        if ((_animations & BruteAnimations.AxeSwing) != 0)
            _animationsList.Add(Animator.StringToHash(BruteAnimations.AxeSwing.ToString()));
        if ((_animations & BruteAnimations.BigStrike) != 0)
            _animationsList.Add(Animator.StringToHash(BruteAnimations.BigStrike.ToString()));
    }
}
