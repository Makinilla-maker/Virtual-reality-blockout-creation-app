using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public enum State
{
    NONE,
    SELECTED,
    PLACING,
    PLACED,
}

public class SpawnCube : MonoBehaviour
{
    //Inputs
    public InputActionProperty buttonClicked;
    public InputActionProperty joystickUp;

    public GameObject cube;
    public Transform placeTransform;

    public float objectOffset;

    public LineRenderer lineRenderer;

    public State actionState;

    //Provisional

    GameObject selectedObject;
    GameObject objectToPlace;
    Vector3 pos;

    // Start is called before the first frame update
    void Start()
    {

        lineRenderer.enabled = false;
        actionState = State.NONE;
    }

    // Update is called once per frame
    void Update()
    {
        float triggerValue = buttonClicked.action.ReadValue<float>();
        var joystickUpValue = joystickUp.action?.ReadValue<Vector2>() ?? Vector2.zero;


        if(triggerValue > .0f && actionState == State.NONE)
        {
            selectedObject = Instantiate(cube, placeTransform);

            actionState = State.SELECTED;
        }
        if(actionState == State.PLACING && joystickUpValue.y < 0.1f)
        {
            actionState = State.SELECTED;

            objectToPlace.gameObject.layer = 0;
        }
        if((actionState == State.SELECTED || actionState == State.PLACING) && joystickUpValue.y > 0.8f)
        {
            lineRenderer.enabled = true;
            RaycastHit hit;
            Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity);
            Debug.DrawLine(transform.position, hit.point);

            pos = hit.point;
            pos.x = Mathf.RoundToInt(pos.x / 1f) * 1f;
            pos.y = (Mathf.RoundToInt(pos.y / 1f) * 1f) + 0.5f;
            pos.z = Mathf.RoundToInt(pos.z / 1f) * 1f;
            transform.position = pos;

            if(actionState == State.SELECTED)
                objectToPlace = CreateObject(cube);

            objectToPlace.gameObject.transform.position = pos;
            objectToPlace.gameObject.transform.localScale = new Vector3(1,1,1);

            actionState = State.PLACING;

        }
        if(selectedObject != null)
        {
            selectedObject.gameObject.transform.position = placeTransform.position;
            selectedObject.gameObject.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
        }
    }
    GameObject CreateObject(GameObject go)
    {
        return Instantiate(go, pos, Quaternion.identity);
    }
}
