//  SoundManager.cs
//  Musici
//
//  Created by henryy on 11/09/2015 15:25:39.

using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

namespace ProGamer
{
    public class SoundManager : PrefabSingleton<SoundManager>
    {
        public const string BGMFolderPath = "Audio/BGM/";
        public const string SFXFolderPath = "Audio/SFX/";

        [SerializeField] protected AdvanceAudioSource bgmAudioSource;
        [SerializeField] protected AudioSource musicSource;
        [SerializeField] protected UnityEngine.Audio.AudioMixerGroup sfxMixer;

        protected float defaultMusicVolume = 1f;

        protected string[] mBGMList;
        protected int currentBGMIndex = 0;

        void Start()
        {
            bgmAudioSource.BaseAudioSource.volume = defaultMusicVolume;
        }

        void OnEnable()
        {
            bgmAudioSource.AudioSourceDidFinish += ChangeBGM;
            SceneManager.sceneLoaded += OnLevelFinishedLoading;
        }

        void OnDisable()
        {
            bgmAudioSource.AudioSourceDidFinish -= ChangeBGM;
            SceneManager.sceneLoaded -= OnLevelFinishedLoading;
        }

        protected override void Awake()
        {
            base.Awake();

            if (!GameSave.MusicOn)
            {
                StopBGM();
            }

            DontDestroyOnLoad(this);
        }

        public virtual AdvanceAudioSource Play(string audioClipName)
        {
            if (!GameSave.SfxOn || string.IsNullOrEmpty(audioClipName))
            {
                return null;
            }

            AudioClip clip = Resources.Load<AudioClip>(SFXFolderPath + audioClipName);

            if (clip == null)
            {
                Debug.Log("AudioClip not found: " + audioClipName);
                return null;
            }

            return Play(clip);
        }

        public virtual AdvanceAudioSource Play(AudioClip clip)
        {
            AdvanceAudioSource customSource = gameObject.AddComponent<AdvanceAudioSource>();

            customSource.InitWithAudioClip(clip);
            customSource.BaseAudioSource.outputAudioMixerGroup = sfxMixer;
            customSource.AudioSourceDidFinish += DestroyAudioSource;
            customSource.Play();

            return customSource;
        }

        protected virtual void DestroyAudioSource(AdvanceAudioSource audioSource)
        {
            Destroy(audioSource);
        }

        public virtual void Play(AdvanceAudioSource audioSource)
        {
            if (audioSource.IsBGM)
            {
                PlayBGM(audioSource);
            }
            else
            {
                PlaySFX(audioSource);
            }
        }

        protected virtual void PlayBGM(AdvanceAudioSource audioSource)
        {
            if (bgmAudioSource != null && bgmAudioSource != audioSource)
            {
                bgmAudioSource.Stop();
            }

            bgmAudioSource = audioSource;

            if (!GameSave.MusicOn)
            {
                return;
            }

            audioSource.Play();
        }


        protected virtual void PlaySFX(AdvanceAudioSource audioSource)
        {
            if (!GameSave.SfxOn)
            {
                return;
            }

            audioSource.Play();
        }

        public virtual void ResumeBGM()
        {
            if (bgmAudioSource)
            {
                Play(bgmAudioSource);
            }
        }

        public virtual void PauseBGM()
        {
            if (bgmAudioSource)
            {
                bgmAudioSource.Pause();
            }
        }

        public virtual void StopBGM()
        {
            if (bgmAudioSource)
            {
                bgmAudioSource.Stop();
            }
        }

        public virtual void ToggleMusic(bool isOn)
        {
            GameSave.MusicOn = isOn;
            if (isOn)
            {
                ResumeBGM();
            }
            else
            {
                StopBGM();
            }
        }


        public virtual void ToggleMusicVolume(bool isOn)
        {
            if (bgmAudioSource)
            {
                defaultMusicVolume = Mathf.Max(bgmAudioSource.BaseAudioSource.volume, defaultMusicVolume);

                bgmAudioSource.BaseAudioSource.volume = isOn ? defaultMusicVolume : 0;
            }
        }

        public virtual void ToggleSFX(bool isOn)
        {
            GameSave.SfxOn = isOn;
        }

        private void SwitchBGM(AdvanceAudioSource bgm)
        {
            if (mBGMList != null && mBGMList.Length > 0)
            {
                currentBGMIndex++;
                string bgmPath = mBGMList[currentBGMIndex % mBGMList.Length];

                bgm.BaseAudioSource.clip = Resources.Load<AudioClip>(BGMFolderPath + bgmPath);
                bgm.Play();
            }
            else
            {
                StopBGM();
            }
        }

        private void SwitchToDefaultBGM(AdvanceAudioSource bgm)
        {
            if (mBGMList != null && mBGMList.Length > 0)
            {
                currentBGMIndex = 0;

                bgm.BaseAudioSource.clip = Resources.Load<AudioClip>(BGMFolderPath + mBGMList[currentBGMIndex]);
                bgm.Play();
            }
            else
            {
                StopBGM();
            }
        }

        private void ChangeBGM(AdvanceAudioSource bgm)
        {
            SwitchBGM(bgm);
            if (!GameSave.MusicOn)
            {
                StopBGM();
            }
        }

        void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
        {
            //For different scene
            Debug.Log(string.Format("{0} loaded by {1} mode", scene.name, mode));

            switch (scene.name)
            {
                case "InitScene":
                    SetBGBListAndPlay(new string[]
                    {
                        "ocean_saver_main_title_v2"
                    });
                    break;
                case "AquariumManageScene":
                    SetBGBListAndPlay(new string[]
                    {
                        "ocean_saver_book_section_bgm_v1"
                    });
                    break;
                case "ARHatchery":
                    SetBGBListAndPlay(new string[]
                    {
                        "ocean_saver_fish_tank_bgm_v2"
                    });
                    break;
                case "ARMetaverse":
                    SetBGBListAndPlay(new string[]
                    {
                        "ocean_saver_fish_tank_bgm_v2"
                    });
                    break;
                case "Aquarium":
                case "ARAquarium":
                case "ColoringBook":
                case "PrintingScene":
                default:
                    if (SceneTransitHandler.PreviousScene != "ARAquarium"
                        && SceneTransitHandler.PreviousScene != "Aquarium"
                        && SceneTransitHandler.PreviousScene != "ColoringBook"
                        && SceneTransitHandler.PreviousScene != "PrintingScene")
                    {
                        SetBGBListAndPlay(new string[]
                        {
                            "ocean_saver_fish_tank_bgm_v2"
                        });
                    }

                    break;
                    break;
            }
        }

        public void SetBGBList(string[] list)
        {
            StopBGM();

            this.mBGMList = list;
            currentBGMIndex = 0;
        }

        public void SetBGBListAndPlay(string[] list)
        {
            SetBGBList(list);
            SwitchToDefaultBGM(bgmAudioSource);

            if (!GameSave.MusicOn)
            {
                StopBGM();
            }
        }
    }
}