using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PageShower : MonoBehaviour
{
    public GameObject Footer;

    public void TurnToPage(int targetPage)
    {
        Footer.SetActive(targetPage > 0);
    }

    public void Back()
    {
        SceneTransitHandler.BackMainMenu();
    }
}