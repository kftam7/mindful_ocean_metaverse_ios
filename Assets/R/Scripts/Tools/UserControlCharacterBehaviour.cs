using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserControlCharacterBehaviour : UserControlRotater
{
    protected Animator mCharacterAnimator;

    protected Animator CharacterAnimator
    {
        get
        {
            if (mCharacterAnimator == null)
            {
                mCharacterAnimator = GetComponentInChildren<Animator>();
            }

            return mCharacterAnimator;
        }
    }

    public AnimationRange RightAngleRnage = new AnimationRange("swim_right", 60, -60); //the range is useless
    public AnimationRange TopAngleRange = new AnimationRange("swim_top", 120, 60);
    public AnimationRange LeftAngleRange = new AnimationRange("swim_left", 240, 120);
    public AnimationRange DownAngleRange = new AnimationRange("swim_down", 300, 240);

    protected List<AnimationRange> checkRangeList;

    protected bool isAlive = false;

    protected override void OnEnable()
    {
        base.OnEnable();
        EventManager.StartListening(EventList.OnGameStatusChanged, OnGameStatusChanged);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        EventManager.StopListening(EventList.OnGameStatusChanged, OnGameStatusChanged);
    }

    void Start()
    {
        checkRangeList = new List<AnimationRange>();
        checkRangeList.Add(TopAngleRange);
        checkRangeList.Add(LeftAngleRange);
        checkRangeList.Add(DownAngleRange);
    }

    protected override void OnTargetAngleChanged(Vector2 force)
    {
        if (isAlive)
        {
            float angle = GetAngle(force);

            AnimationRange inRange = null;

            foreach (AnimationRange r in checkRangeList)
            {
                if (r.InclusiveRange(angle)) inRange = r;
            }

            if (inRange == null)
            {
                inRange = RightAngleRnage;
            }

            CharacterAnimator.CrossFade(inRange.Animation, 0.1f);

            fromRotation = this.transform.eulerAngles.z;
            targetRotation = angle - inRange.Mean;

            rotateProgress = 0;
        }
    }

    protected void OnGameStatusChanged(object result)
    {
        isAlive = (bool)result;
        if (isAlive)
        {
            //Start game
            CharacterAnimator.CrossFade("swim_right", 0);
        }
        else
        {
            //Game over
            CharacterAnimator.CrossFade("dead", 0);
        }
    }
}