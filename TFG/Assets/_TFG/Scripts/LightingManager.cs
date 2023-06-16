using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LightingManager : MonoBehaviour
{
    public Slider slider;
    GameObject directionalLight;
    public GameObject instanciatedLight;
    private void Start()
    {
        directionalLight = GameObject.Find("Directional Light");
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
        GameObject hand = FindObjectOfType<SpawnCube>().gameObject;
        instanciatedLight = Instantiate(light, hand.transform);
    }

}
