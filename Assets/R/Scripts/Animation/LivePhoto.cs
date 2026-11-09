using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LivePhoto : MonoBehaviour
{
    public int FrameRate = 24;
    public List<Sprite> Frames;
    public bool IsLoop = false;

    [SerializeField] private Image TargetImage;

    protected bool isPlaying = false;
    protected int currentFrame = 0;

    protected float Duration
    {
        get
        {
            if (Frames != null && Frames.Count > 0)
            {
                return (float)Frames.Count / FrameRate;
            }

            return 0;
        }
    }

    public int CurrentFrame
    {
        get { return currentFrame; }
    }

    public bool IsPlaying
    {
        get { return isPlaying; }
    }

    public void Play()
    {
        if (Duration > 0)
        {
            isPlaying = true;
            timeCount = 0;
            currentFrame = 0;
        }
    }

    public void Resume()
    {
        if (Duration > 0)
        {
            isPlaying = true;
        }
    }

    public void Stop()
    {
        isPlaying = false;
    }

    public void Play(int frameRate, List<Sprite> frames, bool isLoop)
    {
        SetLivePhoto(frameRate, frames, isLoop);
        Play();
    }

    public void SetLivePhoto(int frameRate, List<Sprite> frames, bool isLoop)
    {
        this.FrameRate = frameRate;
        this.Frames = frames;
        this.IsLoop = isLoop;
    }

    protected float timeCount = 0;

    // Update is called once per frame
    protected void Update()
    {
        if (isPlaying)
        {
            timeCount += Time.deltaTime;
            UpdateFrame(Mathf.CeilToInt(timeCount * FrameRate) % Frames.Count);
        }
    }

    public virtual void UpdateFrame(int frame)
    {
        if (currentFrame != frame)
        {
            currentFrame = frame;
            TargetImage.sprite = Frames[frame];
        }
    }
}