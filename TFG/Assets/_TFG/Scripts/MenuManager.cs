using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public GameObject objectsMenu;
    public GameObject materialMenu;
    public GameObject lightMenu;
    public GameObject facilityMenu;
    public List<GameObject> menus = new List<GameObject>();

    public TMP_Text gridText;
    public int gridSize;

    public void Start()
    {
        gridText.text = gridSize.ToString();
        SetScrrensToFalse();
    }
    void SetScrrensToFalse()
    {
        foreach (GameObject go in menus)
        {
            go.SetActive(false);
        }
        //objectsMenu.SetActive(false);
        //materialMenu.SetActive(false);
        //lightMenu.SetActive(false);
        //facilityMenu.SetActive(false);
    }
    public void SetScreens(GameObject screen)
    {
        SetScrrensToFalse();
        screen.SetActive(true);
    }
    public void SetGridSize(int number)
    {
        gridSize += number;
        gridText.text = gridSize.ToString();
    }
}
