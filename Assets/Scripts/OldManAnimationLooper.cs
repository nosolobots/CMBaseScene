using UnityEngine;
using System.Collections.Generic;

[System.Flags]
public enum OldManAnimations
{
    None = 0,
    Secret = 1 << 0,
    Look = 1 << 1,
}

public class OldManAnimationLooper : AnimationLooper
{
    [SerializeField] OldManAnimations _animations;

    protected override void Awake()
    {
        base.Awake();

        if ((_animations & OldManAnimations.Secret) != 0)
            _animationsList.Add(Animator.StringToHash(OldManAnimations.Secret.ToString()));
        if ((_animations & OldManAnimations.Look) != 0)
            _animationsList.Add(Animator.StringToHash(OldManAnimations.Look.ToString()));
    }
}
