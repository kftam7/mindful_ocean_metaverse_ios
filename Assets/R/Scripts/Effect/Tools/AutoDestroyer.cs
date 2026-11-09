using UnityEngine;
using System.Collections;

public class AutoDestroyer : MonoBehaviour
{
    [SerializeField] protected float time = 0.5f;

    IEnumerator Start()
    {
        yield return new WaitForSeconds(time);
        Destroy(this.gameObject);
    }
}