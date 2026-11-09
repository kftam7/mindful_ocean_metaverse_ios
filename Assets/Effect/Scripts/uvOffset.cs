using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class uvOffset : MonoBehaviour
{
    public float scrollSpeed = 0.5F;
    public Renderer rend;
    public bool IsTextureMove;

    void Start()
    {
        rend = this.GetComponent<Renderer>();
    }

    void Update()
    {
        float offset = Time.time * scrollSpeed;
        if (IsTextureMove)
            rend.material.SetTextureOffset("_Texture", new Vector2(0, offset));
        rend.material.SetTextureOffset("_Displace", new Vector2(0, offset));
    }
}