using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySound : MonoBehaviour
{
    [SerializeField] protected string soundPath;

    public void Play(string sound)
    {
        ProGamer.SoundManager.Instance.Play(sound);
    }

    public void Play()
    {
        Play(soundPath);
    }
}