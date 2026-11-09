using System.Collections.Generic;
using UnityEngine;
using ProGamer;
using BezierSolution;
using Unity.AI.Navigation;

public abstract class FishTank : MonoBehaviour
{
    #region Fields

    public const string ModelPath = "Models/Characters/";
    public float SwimMaxSpeed = 5f;
    public float SwimMinSpeed = 3f;

    [Range(5, 15)] private const int pathPointCount = 10;

    public float AdjustSize = 1f; //adjust model size and speed
    public bool AutoAdjustAfterContainerChanged = true; //adjust volume

    private static List<FishEntity> tankFishes = new();
    private PathGenerator generator;

    public static List<FishEntity> TankFishes => tankFishes;

    #endregion

    #region Tank

    protected abstract Volume GetVolume();

    public void InitFishTank(bool independentPath = false)
    {
        generator = new PathGenerator(GetVolume(), independentPath ? null : transform);
        tankFishes = new List<FishEntity>();
    }

    public void SetTankSize(float size)
    {
        foreach (FishEntity fishEntity in tankFishes)
        {
            //Tank size is the speed modifier
            fishEntity.SetSpeedScale(size);
        }
    }

    #endregion

    #region Generate Random Fishes

    public void CreateRandomFishes(List<string> fishNames, List<string> skins, int count, float pathScaleStep, int startScale)
    {
        TextureReference skin = (Resources.Load("SpriteDic/SkinTexture") as GameObject).GetComponent<TextureReference>();
        for (int j = startScale; j < count; j++)
        {
            var i = new System.Random().Next(fishNames.Count);

            if (DataCenter.Instance.CharacterDataDic.TryGetValue(fishNames[i], out var characterData))
            {
                FishEntity fishEntity = DoCreateFish(characterData,
                    GetBezierPath(Vector3.one + (j + 1) * new Vector3(pathScaleStep, 0, pathScaleStep),
                        new Vector3(0, new System.Random().Next(11) / 10 - 0.0f, 0), null),
                    SwimMaxSpeed * AdjustSize, SwimMinSpeed * AdjustSize);
                fishEntity.SetSkin(skin.GetTextureByName(skins[i]));
                tankFishes.Add(fishEntity);
            }
        }
    }

    public void CreateRandomFishes(List<ImageData> imageDataList, int count, float pathScaleStep, int startScale)
    {
        for (int j = startScale; j < count; j++)
        {
            var i = new System.Random().Next(imageDataList.Count);

            if (DataCenter.Instance.CharacterDataDic.TryGetValue(imageDataList[i].GetModelName(), out var characterData))
            {
                FishEntity fishEntity = DoCreateFish(characterData,
                    GetBezierPath(Vector3.one + (j + 1) * new Vector3(pathScaleStep, 0, pathScaleStep),
                        new Vector3(0, new System.Random().Next(11) / 10.0f - 0.0f, 0), null),
                    SwimMaxSpeed * AdjustSize, SwimMinSpeed * AdjustSize);
                fishEntity.Id = imageDataList[i].Id;
                fishEntity.SetSkin(imageDataList[i]);
                tankFishes.Add(fishEntity);
            }
            else
            {
                Debug.LogError("Cannot find model: " + imageDataList[i].GetModelName());
            }
        }
    }

    #endregion

    #region Generate Fishes

    public int CreateFishes(List<string> fishNames, List<string> skins)
    {
        TextureReference skin = (Resources.Load("SpriteDic/SkinTexture") as GameObject).GetComponent<TextureReference>();
        for (int i = 0; i < fishNames.Count; i++)
        {
            if (DataCenter.Instance.CharacterDataDic.TryGetValue(fishNames[i], out var characterData))
            {
                FishEntity fishEntity = DoCreateFish(characterData, GetBezierPath(Vector3.one, Vector3.zero, null), SwimMaxSpeed * AdjustSize, SwimMinSpeed * AdjustSize);
                fishEntity.SetSkin(skin.GetTextureByName(skins[i]));
                tankFishes.Add(fishEntity);
            }
        }

        return fishNames.Count;
    }

    public int CreateFishes(List<ImageData> imageDataList, float pathScaleStep, Vector3 offset)
    {
        int i = 0;
        foreach (ImageData data in imageDataList)
        {
            CreateFish(data, pathScaleStep, offset, i);
            i++;
        }

        return imageDataList.Count;
    }

    #endregion

    #region Generate Fish

    public FishEntity CreateFish(ImageData imageData, float pathScaleStep, Vector3 offset, int i, Vector3? startPosition = null)
    {
        if (DataCenter.Instance.CharacterDataDic.TryGetValue(imageData.GetModelName(), out var characterData))
        {
            var path = GetBezierPath(Vector3.one + i * new Vector3(pathScaleStep, 0, pathScaleStep), offset, startPosition);
            FishEntity fishEntity = DoCreateFish(characterData, path, SwimMaxSpeed * AdjustSize, SwimMinSpeed * AdjustSize);
            fishEntity.Id = imageData.Id;
            fishEntity.SetSkin(imageData);
            tankFishes.Add(fishEntity);
            return fishEntity;
        }
        else
        {
            Debug.LogError("Cannot find model: " + imageData.GetModelName());
            return null;
        }
    }

    public FishEntity CreateFish(string imageUrl, float pathScaleStep, Vector3 offset, int i, Vector3 startPosition)
    {
        string imageFile = string.IsNullOrEmpty(imageUrl)
            ? GameSetting.DefaultFish
            : ImageFileNameUtils.GetFullFileNameFromUrl(imageUrl);
        var imageData = new ImageData
        {
            ImageFile = imageFile,
            Url = imageUrl
        };

        return CreateFish(imageData, pathScaleStep, offset, i, startPosition);
    }

    public FishEntity GenerateCircularFish(string fishName, string skin, float distance, Vector3 offset)
    {
        TextureReference skins = (Resources.Load("SpriteDic/SkinTexture") as GameObject).GetComponent<TextureReference>();
        if (DataCenter.Instance.CharacterDataDic.TryGetValue(fishName, out var characterData))
        {
            FishEntity fishEntity = DoCreateFish(characterData, GetCircularBezierPath(distance, offset),
                SwimMaxSpeed * AdjustSize, SwimMinSpeed * AdjustSize);
            fishEntity.SetSkin(skins.GetTextureByName(skin)); // actually what is retrieved here is null
            tankFishes.Add(fishEntity);
            return fishEntity;
        }

        return null;
    }

    #endregion

    #region Find and Remove Fish

    public FishEntity FindFish(ImageData imageData)
    {
        foreach (FishEntity fishEntity in tankFishes)
        {
            if (fishEntity.Id == imageData.Id)
            {
                return fishEntity;
            }
        }

        return null;
    }

    public void RemoveFish(FishEntity fishEntity)
    {
        if (tankFishes.Contains(fishEntity))
        {
            if (fishEntity != null)
            {
                Destroy(fishEntity.gameObject);
            }

            tankFishes.Remove(fishEntity);
        }
    }

    public void DestroyAllFishes()
    {
        for (int i = 0; i < tankFishes.Count; i++)
        {
            Destroy(tankFishes[i].gameObject);
        }

        tankFishes = new List<FishEntity>();
    }

    #endregion

    #region Get Path

    public BezierSpline GetBezierPathForNavMesh(Vector3 startPosition, AukiSdkWrapper aukiSdkWrapper)
    {
        return generator.GetBezierPathForNavMesh("NavFishPath", startPosition, aukiSdkWrapper);
    }

    public BezierSpline GetBezierPath(Vector3 scale, Vector3 offset, Vector3? startPosition)
    {
        Logger.Log("*8*******");
        return generator.GetBezierPath("FishPath", pathPointCount, true, scale, offset, startPosition);
    }

    public BezierSpline GetCircularBezierPath(float distance, Vector3 offset)
    {
        float startAngle = Random.Range(0f, 360f);
        int direction = Random.Range(0, 100) % 2 == 0 ? 1 : -1;
        return generator.GetCircularBezierPath("FishCircularPath",
            pathPointCount, direction, true, Vector3.one, offset, startAngle, null, distance);
    }

    // Move and ResumeCreature
    public BezierSpline GetCircularBezierPath(Vector3 startPosition)
    {
        int direction = Random.Range(0, 100) % 2 == 0 ? 1 : -1;

        return generator.GetCircularBezierPath("FishCircularPath", pathPointCount, direction, true, Vector3.one, Vector3.zero, 0, startPosition, null);
    }

    // BezierWalkerWithSpeed.CreateArPathFromPosition
    public BezierSpline GetCircularBezierPath(Vector3 startPosition, Vector3 pathScale, Vector3 offset)
    {
        int direction = Random.Range(0, 100) % 2 == 0 ? 1 : -1;

        return generator.GetCircularBezierPath("FishCircularPath", pathPointCount, direction, true, pathScale, offset, 0, startPosition, null);
    }

    #endregion

    #region Private Methods

    private FishEntity DoCreateFish(CharacterData characterData, BezierSpline curve, float maxSpeed, float minSpeed)
    {
        GameObject model = CreateFishModel(characterData.FishTankModelName, characterData.DisplayAmount);
        model.transform.SetParent(transform);
        model.transform.localScale = Vector3.one * AdjustSize * characterData.AquariumSize;

        Collider sphereCollider = model.GetComponent<Collider>();
        if (sphereCollider == null)
        {
            sphereCollider = model.AddComponent<SphereCollider>();
        }

        sphereCollider.isTrigger = true;
        model.SetActive(false);

        FishGroupEntity entity = model.AddComponent<FishGroupEntity>();
        entity.CharacterData = characterData;
        entity.MaxSpeed = maxSpeed * characterData.AquariumSpeed;
        entity.MinSpeed = minSpeed * characterData.AquariumSpeed;

        BezierWalkerWithSpeed compWalker = model.AddComponent<BezierWalkerWithSpeed>();
        compWalker.Entity = entity;
        compWalker.Spline = curve;
        compWalker.relaxationAtEndPoints = 0;
        compWalker.TravelMode = BezierWalkerWithSpeed.TravelModeEnum.Loop;
        compWalker.lookForward = true;
        entity.CompWalker = compWalker;

        FishSpeedController compSpeedController = model.AddComponent<FishSpeedController>();
        compSpeedController.Entity = entity;
        entity.CompSpeedController = compSpeedController;

        model.SetActive(true);
        return entity;
    }

    private GameObject CreateFishModel(string modelName, int amount)
    {
        if (amount == 1)
        {
            return ReuseableObjectManager.Instantiate(ModelPath + modelName, Vector3.zero, Quaternion.identity);
        }
        else
        {
            GameObject fishGroup = new GameObject("Group");

            List<Vector3> positionList = GenerateRandomPositionInSphere(amount, 1f);

            for (int i = 0; i < positionList.Count; i++)
            {
                GameObject element = ReuseableObjectManager.Instantiate(ModelPath + modelName, fishGroup.transform);
                element.transform.localPosition = positionList[i];
            }

            return fishGroup;
        }
    }

    //For Fish Group
    private List<Vector3> GenerateRandomPositionInSphere(int amount, float radius)
    {
        List<Vector3> positionList = new List<Vector3>();

        for (int i = 0; i < amount; i++)
        {
            positionList.Add(Random.insideUnitSphere * radius);
        }

        return positionList;
    }

    #endregion
}