using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

namespace ProGamer.UIAnimation
{
    public class UITweenPopupAnimation : AnimationBasic
    {
        [SerializeField] protected Vector3 fadeInFrom;
        [SerializeField] protected AnimationCurve fadeInCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 1f), new Keyframe(1f, 1f, 1f, 0f));
        [SerializeField] protected float fadeInDuration = 0.5f;
        [SerializeField] protected Vector3 targetPosition;
        [SerializeField] protected AnimationCurve fadeOutCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 1f), new Keyframe(1f, 1f, 1f, 0f));
        [SerializeField] protected float fadeOutDuration = 0.5f;


        [SerializeField] protected bool resetPositionOnEnable = false;
        [SerializeField] protected bool resetOnce = false;

        [SerializeField] protected bool fixedX = false;
        [SerializeField] protected bool fixedY = false;

        [SerializeField] protected UnityEvent onFadeInStarted;
        [SerializeField] protected UnityEvent onFadeOutStarted;

        [SerializeField] protected UnityEvent onFadeInCompleted;
        [SerializeField] protected UnityEvent onFadeOutCompleted;

        public Vector3 FadeInFrom
        {
            get { return fadeInFrom; }
            set { fadeInFrom = value; }
        }

        public Vector3 TargetPosition
        {
            get { return targetPosition; }
            set { targetPosition = value; }
        }

        public void SetInitStatus(bool resetPositionOnEnable, bool resetOnce)
        {
            this.resetPositionOnEnable = resetPositionOnEnable;
            this.resetOnce = resetOnce;
        }

        private RectTransform targetRectTransform;

        public RectTransform TargetRecTransform
        {
            get
            {
                if (targetRectTransform == null)
                {
                    targetRectTransform = this.gameObject.GetComponent<RectTransform>();
                }

                return targetRectTransform;
            }
        }

        public UnityEvent OnFadeInCompleted
        {
            get { return onFadeInCompleted; }
        }


        public UnityEvent OnFadeOutCompleted
        {
            get { return onFadeOutCompleted; }
        }

        protected virtual void OnEnable()
        {
            if (resetPositionOnEnable)
            {
                ResetPosition();
                if (resetOnce) resetPositionOnEnable = false;
            }
        }

        public virtual void ResetPosition()
        {
            Vector3 tem = fadeInFrom;
            if (fixedX) tem.x = TargetRecTransform.anchoredPosition3D.x;
            if (fixedY) tem.y = TargetRecTransform.anchoredPosition3D.y;

            TargetRecTransform.anchoredPosition3D = tem;
        }

        public virtual void FadeIn()
        {
            if (onFadeInStarted != null) onFadeInStarted.Invoke();
            SetAnimation(fadeInCurve, fadeInDuration, TargetRecTransform.anchoredPosition3D, targetPosition, true);
        }

        public virtual void FadeOut()
        {
            if (onFadeOutStarted != null) onFadeOutStarted.Invoke();
            SetAnimation(fadeOutCurve, fadeOutDuration, TargetRecTransform.anchoredPosition3D, fadeInFrom, false);
        }

        protected virtual void SetAnimation(AnimationCurve curve, float duration, Vector3 from, Vector3 to, bool isIn)
        {
            this.progress = 0;
            this.dif = to - from;
            this.targetFrom = from;
            this.actingCurve = curve;
            this.duration = duration;
            this.isIn = isIn;

            if (fixedX) dif.x = 0;
            if (fixedY) dif.y = 0;

            base.Play();
        }

        private AnimationCurve actingCurve;

        private Vector3 dif;

        private float startTime;
        private bool isIn;
        private Vector3 targetFrom;
        protected float progressInPercentage;

        public override void AnimationUpdate(float deltaTime)
        {
            base.AnimationUpdate(deltaTime);
            if (actingCurve == null) return;
            progressInPercentage = actingCurve.Evaluate(progress / duration);
            TargetRecTransform.anchoredPosition3D = (targetFrom + progressInPercentage * dif);
        }

        public override void Stop()
        {
            base.Stop();
            if (isIn)
            {
                if (onFadeInCompleted != null) onFadeInCompleted.Invoke();
            }
            else
            {
                if (onFadeOutCompleted != null) onFadeOutCompleted.Invoke();
            }
        }
    }
}