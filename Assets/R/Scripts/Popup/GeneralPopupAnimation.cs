using UnityEngine;
using System.Collections;
using ProGamer;

public class GeneralPopupAnimation : PopupAnimation
{
    [SerializeField] protected Transform target;
    [SerializeField] protected float openDuration = 0.2f;
    [SerializeField] protected AnimationCurve inAnimationCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 1f), new Keyframe(1f, 1f, 1f, 0f));
    [SerializeField] protected float closeDuration = 0.2f;
    [SerializeField] protected AnimationCurve outAnimationCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 1f), new Keyframe(1f, 1f, 1f, 0f));
    [SerializeField] protected bool executeCallbackIfStopDuringAnimation = false;
    [SerializeField] protected bool alphaCross = false;
    [SerializeField] protected bool ignoreTimeScale = false;

    protected Vector3 closeScale = new Vector3(1, 0, 1);
    protected CanvasGroup targetCanvas;

    protected bool isPlaying = false;
    protected float lastTime = 0;

    protected virtual void OnEnable()
    {
        target.localScale = closeScale;
        if (alphaCross)
        {
            if (targetCanvas == null)
            {
                targetCanvas = this.gameObject.GetComponent<CanvasGroup>();
                if (targetCanvas == null)
                {
                    targetCanvas = this.gameObject.AddComponent<CanvasGroup>();
                }
            }

            targetCanvas.alpha = 0;
        }

        mProgress = 0;
    }

    public override void Show(SimplePopupCallback callback)
    {
        if (isPlaying && executeCallbackIfStopDuringAnimation)
        {
            if (mCallback != null) mCallback();
        }

        mCurrentScale = closeScale;
        mTargetScale = Vector3.one;
        mCallback = callback;
        mDuration = openDuration;
        mCurve = inAnimationCurve;
        if (alphaCross)
        {
            mCurrentAlpha = targetCanvas.alpha;
            mTargetAlpha = 1;
        }

        mProgress = 0;
        transitionStatus = 1;

        lastTime = Time.realtimeSinceStartup;
        isPlaying = true;
    }


    public override void Close(SimplePopupCallback callback)
    {
        if (isPlaying && executeCallbackIfStopDuringAnimation)
        {
            if (mCallback != null) mCallback();
        }

        mCallback = callback;
        mCurrentScale = target.localScale;
        mTargetScale = closeScale;
        mCallback = callback;
        mDuration = closeDuration;
        mCurve = outAnimationCurve;
        if (alphaCross)
        {
            mCurrentAlpha = targetCanvas.alpha;
            mTargetAlpha = 0;
        }

        mProgress = 0;
        transitionStatus = -1;

        lastTime = Time.realtimeSinceStartup;
        isPlaying = true;
    }

    private SimplePopupCallback mCallback;
    private float mProgress;
    private float mProgressPrecentage;
    private Vector3 mCurrentScale;
    private Vector3 mTargetScale;
    private float mDuration;
    private AnimationCurve mCurve;
    private float mTargetAlpha;
    private float mCurrentAlpha;

    //0: idle, 1 fade in, -1 fade out
    protected int transitionStatus = 0;

    protected virtual void Update()
    {
        if (isPlaying)
        {
            if (mProgress < mDuration)
            {
                if (ignoreTimeScale)
                {
                    mProgress += Time.realtimeSinceStartup - lastTime;
                    lastTime = Time.realtimeSinceStartup;
                }
                else
                {
                    mProgress += Time.deltaTime;
                }

                mProgressPrecentage = mProgress / mDuration;
                target.localScale = Vector3.Lerp(mCurrentScale, mTargetScale, mCurve.Evaluate(mProgressPrecentage));
                if (alphaCross)
                {
                    targetCanvas.alpha = Mathf.Lerp(mCurrentAlpha, mTargetAlpha, mProgressPrecentage);
                }
            }
            else
            {
                target.localScale = Vector3.Lerp(mCurrentScale, mTargetScale, mCurve.Evaluate(1));
                if (alphaCross)
                {
                    targetCanvas.alpha = mTargetAlpha;
                }

                TransitionCallback();

                transitionStatus = 0;
                isPlaying = false;
            }
        }
    }

    protected virtual void TransitionCallback()
    {
        if (mCallback != null)
        {
            mCallback();
            mCallback = null;
        }
    }
}