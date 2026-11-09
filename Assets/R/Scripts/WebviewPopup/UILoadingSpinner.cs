using UnityEngine;
using System.Collections;

public class UILoadingSpinner : MonoBehaviour
{
    public float factor = 500;

    private void Update()
    {
        transform.Rotate(Vector3.back * Time.deltaTime * factor);
    }
}