using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationRange : Range
{
    public string Animation { get; set; }

    public AnimationRange() : base() { }

    public AnimationRange(string animation, float max, float min) : base(max, min)
    {
        this.Animation = animation;
    }
}