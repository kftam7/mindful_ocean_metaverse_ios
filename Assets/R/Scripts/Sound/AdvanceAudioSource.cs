//  CustomAudioSource.cs
//  Musici
//
//  Created by henryy on 11/09/2015 14:12:17.

using UnityEngine;
using System.Collections;

namespace ProGamer
{
    public class AdvanceAudioSource : MonoBehaviour
    {
        public delegate void CustomAudioSourceEvent(AdvanceAudioSource s);

        public event CustomAudioSourceEvent AudioSourceWillStart;
        public event CustomAudioSourceEvent AudioSourceDidFinish;

        public AudioSource BaseAudioSource;

        public bool PlayOnAwake;
        public bool OneShot;
        public float OneShotVolumeScale = 1.0f;
        public float Delay;
        protected float StartTime;
        protected float DelayedTime;
        public double ScheduledTime = 0.0f;

        public bool IsBGM;
        public bool DestroyOnStop = false;

        void Awake()
        {
            if (PlayOnAwake)
            {
                SoundManager.Instance.Play(this);
            }
        }

        void OnDestroy()
        {
            if (BaseAudioSource != null)
            {
                Destroy(BaseAudioSource);
            }
        }

        public virtual void InitWithAudioClip(AudioClip clip)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.clip = clip;
            BaseAudioSource = source;
        }

        public virtual void Play()
        {
            if (Delay > 0)
            {
                StartTime = Time.realtimeSinceStartup;
                Invoke("PlayDelayed", Delay - DelayedTime);
            }
            else
            {
                PlayDelayed();
            }
        }

        public virtual void Pause()
        {
            BaseAudioSource.Pause();
            CancelInvoke("PlayDelayed");
            CancelInvoke("AudioSourceFinish");
            DelayedTime = Time.realtimeSinceStartup - StartTime;
        }

        public virtual void UnPause()
        {
            Play();
        }

        public virtual void Stop()
        {
            BaseAudioSource.Stop();
            CancelInvoke("PlayDelayed");
            CancelInvoke("AudioSourceFinish");
            if (DestroyOnStop)
            {
                Destroy(gameObject);
            }
        }

        protected virtual void PlayDelayed()
        {
            if (AudioSourceWillStart != null)
            {
                AudioSourceWillStart(this);
            }

            if (OneShot)
            {
                BaseAudioSource.PlayOneShot(BaseAudioSource.clip, OneShotVolumeScale);
            }
            else if (ScheduledTime > 0)
            {
                BaseAudioSource.PlayScheduled(ScheduledTime);
            }
            else
            {
                BaseAudioSource.Play();
            }

            if (BaseAudioSource.clip != null)
            {
                Invoke("AudioSourceFinish", BaseAudioSource.clip.length - BaseAudioSource.time);
            }
        }

        protected virtual void AudioSourceFinish()
        {
            if (AudioSourceDidFinish != null)
            {
                AudioSourceDidFinish(this);
            }
        }
    }
}