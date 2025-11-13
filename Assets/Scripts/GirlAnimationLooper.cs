using UnityEngine;
using System.Collections.Generic;

[System.Flags]
public enum GirlAnimations
{
    None = 0,
    Hot = 1 << 0,
    Nails = 1 << 1,
}

public class GirlAnimationLooper : AnimationLooper
{
    [SerializeField] GirlAnimations _animations;

    protected override void Awake()
    {
        base.Awake();

        if ((_animations & GirlAnimations.Hot) != 0)
            _animationsList.Add(Animator.StringToHash(GirlAnimations.Hot.ToString()));
        if ((_animations & GirlAnimations.Nails) != 0)
            _animationsList.Add(Animator.StringToHash(GirlAnimations.Nails.ToString()));
    }
}
