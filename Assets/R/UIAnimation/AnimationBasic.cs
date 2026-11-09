/// <summary>
/// Author: Keith Chan
/// Description: AnimatingBasic, the basic class of animation for ignore time scale.
/// Override AnimationUpdate to control animation by deltaTime instead of Time.deltaTime
/// </summary>

using UnityEngine;
using System.Collections;
using UnityEngine.Events;

namespace ProGamer.UIAnimation
{
    public abstract class AnimationBasic : MonoBehaviour
    {
        [SerializeField] protected bool ignoreTimescale;
        [SerializeField] protected bool playOnStart = false;
        [SerializeField] protected float duration = 0.5f;
        [SerializeField] protected UnityEvent onAnimationCompleted;

        protected float progress = 0;
        protected bool animationUpdating = true;
        private bool isPlaying = false;
        private float lastRealTime;

        #region Getter & Setter

        public bool IgnoreTimescale
        {
            get { return ignoreTimescale; }
            set
            {
                if (value) lastRealTime = Time.realtimeSinceStartup;
                ignoreTimescale = value;
            }
        }

        public float Progress
        {
            get { return progress; }
        }

        public bool AnimationUpdating
        {
            get { return animationUpdating; }
            set { animationUpdating = value; }
        }

        public virtual float Duration
        {
            get { return duration; }
            set { duration = value; }
        }

        public virtual bool IsPlaying
        {
            get { return isPlaying; }
        }

        public UnityEvent OnAnimationCompleted
        {
            get { return onAnimationCompleted; }
        }

        #endregion

        protected virtual void Start()
        {
            if (playOnStart)
            {
                PlayOnStart();
            }
        }

        public virtual void PlayOnStart()
        {
            Play();
        }

        public virtual void Play()
        {
            lastRealTime = Time.realtimeSinceStartup;
            progress = 0;
            isPlaying = true;
        }

        public virtual void Stop()
        {
            isPlaying = false;
            if (onAnimationCompleted != null) onAnimationCompleted.Invoke();
        }

        private void Update()
        {
            if (isPlaying && animationUpdating)
            {
                float deltaTime = Time.deltaTime;
                if (ignoreTimescale)
                {
                    deltaTime = (Time.realtimeSinceStartup - lastRealTime);
                    lastRealTime = Time.realtimeSinceStartup;
                }

                UpdateProgress(deltaTime);
            }
        }

        public virtual float UpdateProgress(float deltaTime)
        {
            if (progress + deltaTime > Duration && Duration != -1)
            {
                float deltaTimeToDuration = Duration - progress;
                progress = Duration;
                AnimationUpdate(deltaTimeToDuration);
                Stop();
                return deltaTime - deltaTimeToDuration;
            }
            else
            {
                progress += deltaTime;
                AnimationUpdate(deltaTime);
                return 0;
            }
        }

        public virtual void AnimationUpdate(float deltaTime) { }
    }
}