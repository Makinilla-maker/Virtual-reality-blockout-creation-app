using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DirectionalLightManager : MonoBehaviour
{
    public Slider slider;
    private void Update()
    {
        Quaternion newRotation = Quaternion.Euler(slider.value, 270f,270f);
        transform.rotation = newRotation;
    }
}
