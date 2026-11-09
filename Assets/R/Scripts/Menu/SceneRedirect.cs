using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneRedirect : MonoBehaviour
{
    [SerializeField] protected float waitingTime = 3f;
    [SerializeField] protected string redirectScene;

    // Use this for initialization
    IEnumerator Start()
    {
        yield return new WaitForSeconds(3f);
        SceneTransitHandler.LoadScene(redirectScene);
    }
}