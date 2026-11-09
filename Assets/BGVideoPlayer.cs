using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class BGVideoPlayer : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
        VideoPlayer player = GetComponent<VideoPlayer>();

        // player.Prepare();

        if (!SystemExtension.isIPhoneXScreen())
        {
            player.Pause();
        }

        player.prepareCompleted += (VideoPlayer source) =>
        {
            if (SystemExtension.isIPhoneXScreen())
            {
                source.Play();
            }
        };
    }
}