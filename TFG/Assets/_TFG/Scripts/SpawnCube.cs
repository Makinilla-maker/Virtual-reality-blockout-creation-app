using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
//using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public enum State
{
    NONE,
    SELECTED,
    PLACING,
    CREATING,
}

public class SpawnCube : MonoBehaviour
{
    //Inputs
    public InputActionProperty buttonClicked;
    public InputActionProperty joystickUp;
    public InputActionProperty trigger;

    public GameObject cube;
    public Transform placeTransform;

    public float objectOffset;

    public LineRenderer lineRenderer;

    public State actionState;

    public List<GameObject> cubes;

    //Provisional

    GameObject selectedObject;
    GameObject objectToPlace;
    public GameObject voxelPrfab;

    // Start is called before the first frame update
    void Start()
    {
        cubes = new List<GameObject>();
        lineRenderer.enabled = false;
        actionState = State.NONE;

    }

    // Update is called once per frame
    void Update()
    {
        float triggerValue = buttonClicked.action.ReadValue<float>();
        var joystickUpValue = joystickUp.action?.ReadValue<Vector2>() ?? Vector2.zero;
        float export = trigger.action.ReadValue<float>();

        if(triggerValue > .0f && actionState == State.NONE)
        {
            selectedObject = Instantiate(cube, placeTransform);
            selectedObject.gameObject.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);

            actionState = State.SELECTED;
        }
        if((actionState == State.SELECTED || actionState == State.PLACING))
        {
            lineRenderer.enabled = true;
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity))
            {
                Debug.DrawLine(transform.position, hit.point);
                Debug.Log(hit.transform.tag);


                Vector3 pos;
                pos = hit.point;
                pos.x = Mathf.RoundToInt(pos.x / 1f) * 1f;
                pos.y = (Mathf.RoundToInt(pos.y / 1f) * 1f) + 0.5f;
                pos.z = Mathf.RoundToInt(pos.z / 1f) * 1f;
                //transform.position = pos;


                if (!hit.transform.CompareTag("ObjectToExport") && export > .0f)
                {
                    Debug.Log("Intanciating cuboooooooooooooooooooooooooooooooooooooooooooooooooooooooo");
                    objectToPlace = CreateObject(cube, pos);
                    objectToPlace.gameObject.transform.position = pos;
                    objectToPlace.gameObject.transform.localScale = new Vector3(1, 1, 1);
                    cubes.Add(objectToPlace.gameObject);

                    actionState = State.PLACING;
                }

            }
            else
            {
                Debug.Log("No collision detected.");
            }

        }
        if(actionState == State.PLACING && export < .1f)
        {
            actionState = State.CREATING;
            List<Vector3> cubePosition = new List<Vector3>();

            GameObject voxel = CreateObject(voxelPrfab, new Vector3(0,0,0));

            foreach (GameObject cube in cubes)
            {
                cubePosition.Add(cube.transform.position);
                Destroy(cube);
            }

            Debug.Log("Creating Voxel");
            voxel.GetComponent<VoxelRender>().GenerateVoxelMesh(cubePosition);

            cubePosition.Clear();
            cubes.Clear();

            actionState = State.NONE;

        }
        if (selectedObject != null)
        {
            selectedObject.gameObject.transform.position = placeTransform.position;
            selectedObject.gameObject.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
        }
    }
    GameObject CreateObject(GameObject go, Vector3 pos)
    {
        GameObject inst = Instantiate(go, pos, Quaternion.identity);
        inst.transform.tag = "ObjectToExport";
        return inst;
    }
    bool CheckIfExists(RaycastHit hit)
    {
        if(hit.transform.tag == "ObjectToExport")
            return true;
        return false;
    }
}
