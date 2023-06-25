using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LightingManager : MonoBehaviour
{
    public Slider slider;
    GameObject directionalLight;
    public GameObject instanciatedLight;

    public SpawnCube sc;
    private void Start()
    {
        directionalLight = GameObject.Find("Directional Light");
        sc = GameObject.FindObjectOfType<SpawnCube>();
    }

    public void SetSkybox(Material skybox)
    {
        //Material levelMat = new Material();
        RenderSettings.skybox = skybox;
    }
    private void Update()
    {
        Quaternion newRotation = Quaternion.Euler(slider.value, 270f, 270f);
        directionalLight.transform.rotation = newRotation;
    }
    public void SpawnLight(GameObject light)
    {
        GameObject hand = FindObjectOfType<SpawnCube>().mainHand;

        instanciatedLight = Instantiate(light, hand.transform);

        sc.SetMode("LIGHT");
        sc.SetLight(instanciatedLight);
    }

}
