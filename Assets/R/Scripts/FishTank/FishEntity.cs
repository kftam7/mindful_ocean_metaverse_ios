using Auki.ConjureKit;
using UnityEngine;
using BezierSolution;
using UnityEngine.SceneManagement;

public class FishEntity : AukiObjectBase
{
    public CharacterData CharacterData { get; set; }
    public float MaxSpeed { get; set; }
    public float MinSpeed { get; set; }
    public BezierWalkerWithSpeed CompWalker { get; set; }
    public FishSpeedController CompSpeedController { get; set; }
    public FishMagicObjectChecker CompMagicObjectChecker { get; set; }
    public FishFollowHeart CompFollowHeart { get; set; }
    public FishCircleMotion CompCircleMotion { get; set; }
    public FishSpiralMotion CompSpiralMotion { get; set; }
    public FishGameMotion CompGameMotion { get; set; }
    public FishBody CompFishBody { get; set; }
    public FishPoseSyncer CompFishPoseSyncer { get; set; }
    public float CurrentSpeed => CompSpeedController.GetSpeed();
    public string Id { get; set; }
    public Entity AukiEntity { get; set; }
    public string OwnerName { get; set; }
    public string ImageUrl { get; set; }

    #region Unity Methods

    private void Awake()
    {
        CompFishBody = GetComponent<FishBody>();

        if (SceneManager.GetActiveScene().name == "ARMetaverse")
        {
            CompFollowHeart = gameObject.AddComponent<FishFollowHeart>();
            CompFollowHeart.Entity = this;
            var fishTank = FindObjectOfType<FishTank>();
            CompFollowHeart.MinDistance = fishTank.AdjustSize * 0.1f;
            CompFollowHeart.enabled = false;

            CompCircleMotion = gameObject.AddComponent<FishCircleMotion>();
            CompCircleMotion.Entity = this;
            CompCircleMotion.enabled = false;

            CompSpiralMotion = gameObject.AddComponent<FishSpiralMotion>();
            CompSpiralMotion.Entity = this;
            CompSpiralMotion.enabled = false;

            CompMagicObjectChecker = gameObject.AddComponent<FishMagicObjectChecker>();
            CompMagicObjectChecker.Entity = this;

            CompFishPoseSyncer = gameObject.AddComponent<FishPoseSyncer>();
            CompFishPoseSyncer.Entity = this;

            CompGameMotion = gameObject.AddComponent<FishGameMotion>();
            CompGameMotion.Entity = this;
        }
    }

    private void OnEnable()
    {
        CompWalker.enabled = true;
    }

    private void OnDestroy()
    {
        BezierSpline spline = CompWalker.Spline;
        if (spline != null)
        {
            Destroy(spline.gameObject);
        }
    }

    #endregion

    #region Public Methods

    public void SpeedBurst(float speedModifier)
    {
        CompSpeedController.SetSpeed(MaxSpeed * speedModifier);
    }

    public virtual void SetSkin(Texture skin)
    {
        if (skin != null)
        {
            GetComponentInChildren<Renderer>().material.SetTexture("_MainTex", skin);
        }
    }

    public void SetSkin(ImageData imageData)
    {
        if (!string.IsNullOrEmpty(imageData.Url))
        {
            SetSkinFromUrl(imageData.Url, true);
        }
        else
        {
            SetSkinFromImageFile(imageData.ImageFile, true);
        }
    }

    public void SetSkinFromImageFile(string fullFileName, bool small)
    {
        Texture2D skin = TextureCache.Instance.Load(fullFileName, small);
        GetComponentInChildren<Renderer>().material.SetTexture("_MainTex", skin);
    }

    public void SetSkinFromUrl(string imageUrl, bool small)
    {
        ServerCenter.Instance.GetImageFromCacheOrUrl(imageUrl, small, texture =>
        {
            GetComponentInChildren<Renderer>().material.SetTexture("_MainTex", texture);
        });
    }

    public void SetSpeedScale(float speedScale)
    {
        CompSpeedController.SetSpeedScale(speedScale);
    }

    public void Struggle()
    {
        CompWalker.enabled = false;
        PlayAnimation("struggle");
    }

    public virtual void SetTimeScale(float timeScale)
    {
        GetComponentInChildren<Animator>().speed = timeScale;
    }


    public virtual void PlayAnimation(string animation)
    {
        GetComponentInChildren<Animator>().CrossFade(animation, 0);
    }

    #endregion
}