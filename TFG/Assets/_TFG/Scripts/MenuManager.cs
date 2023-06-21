using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    GameObject objectsMenu;
    GameObject materialMenu;
    GameObject lightMenu;
    GameObject facilityMenu;
    public List<GameObject> menus = new List<GameObject>();
    public List<SpriteRenderer> sprites = new List<SpriteRenderer>();
    public CanvasGroup cg;
    float oldAlpha;

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
    public void Update()
    {
        //if(cg.alpha != oldAlpha)
        //{
        //    cg.alpha = oldAlpha;
        //    SetAlpha(cg.alpha);
        //}
    }
    void SetAlpha(float alpha)
    {
        foreach(SpriteRenderer spriteRenderer in sprites)
        {
            Color color = spriteRenderer.material.color;
            color.a = alpha;
            spriteRenderer.material.SetColor("ChangedAlpha",color);
        }
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
