using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Experimental.XR;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(ARSessionOrigin))]
public class PlaceOnPlane : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Instantiates this prefab on a plane at the touch location.")]
    GameObject m_PlacedPrefab;

    /// <summary>
    /// The prefab to instantiate on touch.
    /// </summary>
    public GameObject placedPrefab
    {
        get { return m_PlacedPrefab; }
        set { m_PlacedPrefab = value; }
    }

    /// <summary>
    /// The object instantiated as a result of a successful raycast intersection with a plane.
    /// </summary>
    public GameObject spawnedObject { get; private set; }

    ARSessionOrigin m_SessionOrigin;

    static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

    void Awake()
    {
        m_SessionOrigin = GetComponent<ARSessionOrigin>();
    }

    float distance = 0;
    bool isPlacedAquarium;


    void Update()
    {
        if (!isPlacedAquarium)
        {
            if (spawnedObject != null)
            {
                //check the distance
                distance = Vector3.Distance(m_SessionOrigin.camera.transform.position.ClearY(), spawnedObject.transform.position.ClearY());
                if (distance < 1)
                {
                    isPlacedAquarium = true;
                    OpenBook();
                }
            }
            else if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                // if (m_SessionOrigin.Raycast(touch.position, s_Hits, TrackableType.PlaneWithinPolygon))
                // {
                //     Pose hitPose = s_Hits[0].pose;
                //
                //     if (spawnedObject == null)
                //     {
                //         spawnedObject = Instantiate(m_PlacedPrefab, hitPose.position, hitPose.rotation);
                //         spawnedObject.transform.localScale = Vector3.one * 0.3f;
                //     }
                //     //else
                //     //{
                //     //    spawnedObject.transform.position = hitPose.position;
                //     //}
                // }
            }
        }
    }

    public void OpenBook()
    {
        Animator an = spawnedObject.GetComponent<Animator>();
        if (an != null)
        {
            an.CrossFade("flippage", 0);
            StartCoroutine(OpeningBook());
        }
    }

    IEnumerator OpeningBook()
    {
        yield return new WaitForSeconds(1.1f);
        Destroy(spawnedObject);
        ProGamer.SoundManager.Instance.SetBGBListAndPlay(new string[] { "underwater" });
        GenerateAquarium();
    }

    public GameObject GenerateAquarium()
    {
        AquariumData aData = DataCenter.Instance.GetAquarium(GameSave.SelectedAquarium);
        GameObject tankModel = Instantiate(Resources.Load("Models/Tanks/" + aData.Data.ARGround) as GameObject, spawnedObject.transform.parent);

        //manually init
        FishTank tank = tankModel.GetComponentInChildren<FishTank>();
        tank.InitFishTank();
        tank.CreateFishes(aData.DefaultCreature, aData.DefaultSkin);
        tank.CreateFishes(ImageDataManager.Instance.GetInAquariumImageDataList(aData.AquariumId), 0, Vector3.zero);

        //set default size
        tankModel.transform.localScale = Vector3.one * 3;
        tankModel.transform.localPosition = spawnedObject.transform.localPosition;
        tankModel.transform.localRotation = spawnedObject.transform.localRotation;

        tank.SetTankSize(tankModel.transform.localScale.x);

        return tankModel;
    }


    public void Back()
    {
        SceneTransitHandler.LoadScene("Aquarium");
    }
}