using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Unity.VisualScripting;
//using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UIElements;

public enum State
{
    NONE,
    SELECTED,
    PLACING,
    RISING,
    CREATING,
}

public class SpawnCube : MonoBehaviour
{
    public GameObject mainHand;
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
    GameObject voxel;
    bool scalling;

    GameObject lastCube;
    bool firstCubePlaced;

    //Configuration
    public bool cubeCheck;
    public bool customMeshCheck;
    public bool materialMode;


    // Start is called before the first frame update
    void Start()
    {
        cubes = new List<GameObject>();
        lineRenderer.enabled = false;
        actionState = State.NONE;
        scalling = false;
        firstCubePlaced = false;
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
        if ((actionState == State.SELECTED || actionState == State.PLACING))
        {
            lineRenderer.enabled = true;
            //CreatingCustomMesh(export);
            CreatingBigCube(export);

        }
        if (actionState == State.PLACING && export < .1f)
        {
            actionState = State.CREATING;

            //CreatingVoxel1();
            CreatingVoxel2();

            actionState = State.SELECTED;
        }
        if(actionState == State.RISING)
        {
            scalling = true;

            Vector3 newScale = new Vector3(0, 1, 0);
            //StartCoroutine(ScaleUp());
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity))
            {
                Debug.DrawLine(transform.position, hit.point);

                if (!hit.transform.CompareTag("ObjectToExport"))
                    voxel.transform.localScale += newScale;
                else
                    Debug.Log("noHit");
            }
            Debug.Log("LIGMAIKASJDIAJIDJASIDJHAUSDHUASDHJIASHDIASJ");
            //actionState = State.NONE;
        }
        if (selectedObject != null)
        {
            selectedObject.gameObject.transform.position = placeTransform.position;
            selectedObject.gameObject.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
        }
    }
    IEnumerator ScaleUp()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity) && !scalling)
        {
            Vector3 newScale = new Vector3(0, 1, 0);
            while (!hit.transform.CompareTag("ObjectToExport"))
            {
                voxel.transform.localScale += newScale;
            }
        }
        scalling = false;
        yield return new WaitForSeconds(0f);
    }

    void CreatingCustomMesh(float export)
    {
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
    void CreatingBigCube(float export)
    {
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
                if(!firstCubePlaced)
                {
                    firstCubePlaced = true;
                    Debug.Log("Intanciating cuboooooooooooooooooooooooooooooooooooooooooooooooooooooooo");
                    objectToPlace = CreateObject(cube, pos);
                    objectToPlace.gameObject.transform.position = pos;
                    objectToPlace.gameObject.transform.localScale = new Vector3(1, 1, 1);
                    cubes.Add(objectToPlace.gameObject);
                    lastCube = CreateObject(cube, pos);
                    cubes.Add(lastCube.gameObject);
                }
                else
                {
                    lastCube.gameObject.transform.position = pos;
                    objectToPlace.gameObject.transform.localScale = new Vector3(1, 1, 1);
                }
                

                actionState = State.PLACING;
            }

        }
        else
        {
            Debug.Log("No collision detected.");
        }
    }
    void CreatingVoxel1()
    {
        List<Vector3> cubePosition = new List<Vector3>();

        voxel = CreateObject(voxelPrfab, new Vector3(0, 0, 0));

        foreach (GameObject cube in cubes)
        {
            cubePosition.Add(cube.transform.position);
            Destroy(cube);
        }

        Debug.Log("Creating Voxel");
        voxel.GetComponent<VoxelRender>().GenerateVoxelMesh(cubePosition);

        cubePosition.Clear();
        cubes.Clear();
    }
    void CreatingVoxel2()
    {
        List<Vector3> cubePosition = new List<Vector3>();

        voxel = CreateObject(voxelPrfab, new Vector3(0, 0, 0));

        Debug.Log(cubes[1].transform.position);
        int mx = 0;
        int mz = 0;

        if (cubes[1].transform.position.x > cubes[0].transform.position.x)  mx = 1;
        else if (cubes[1].transform.position.x < cubes[0].transform.position.x) mx = -1;


        if (cubes[1].transform.position.z > cubes[0].transform.position.z) mz = 1;
        else if (cubes[1].transform.position.z < cubes[0].transform.position.z) mz = -1;

        cubePosition = CreateMeshVoxel(mx,mz);
        
        Debug.Log("Creating Voxel");
        voxel.GetComponent<VoxelRender>().GenerateVoxelMesh(cubePosition);

        foreach (GameObject cube in cubes)
        {
            Destroy(cube);
        }

        firstCubePlaced = false;
        cubePosition.Clear();
        cubes.Clear();
    }
    List<Vector3> CreateMeshVoxel(int mx, int mz)
    {
        List<Vector3> cubePosition = new List<Vector3>();
        if(mx > 0)
        {
            for (int x = 0; x <= (Mathf.Abs(cubes[1].transform.position.x) - Mathf.Abs(cubes[0].transform.position.x)) * mx; x += mx)
            {
                if(mz > 0)
                {
                    for (int z = 0; z <= (Mathf.Abs(cubes[1].transform.position.z) - Mathf.Abs(cubes[0].transform.position.z)) * mz; z += mz)
                    {
                        cubePosition.Add(new Vector3(cubes[0].transform.position.x + x, 0.5f, cubes[0].transform.position.z + z));
                    }
                }
                else
                {
                    for (int z = 0; z >= (Mathf.Abs(cubes[1].transform.position.z) - Mathf.Abs(cubes[0].transform.position.z)) * mz; z += mz)
                    {
                        cubePosition.Add(new Vector3(cubes[0].transform.position.x + x, 0.5f, cubes[0].transform.position.z + z));
                    }
                }
                
            }

        }
        else
        {
            for (int x = 0; x >= (Mathf.Abs(cubes[1].transform.position.x) - Mathf.Abs(cubes[0].transform.position.x)) * mx; x += mx)
            {
                if (mz > 0)
                {
                
                    for (int z = 0; z <= (Mathf.Abs(cubes[1].transform.position.z) - Mathf.Abs(cubes[0].transform.position.z)) * mz; z += mz)
                    {
                        cubePosition.Add(new Vector3(cubes[0].transform.position.x + x, 0.5f, cubes[0].transform.position.z + z));
                    }
                }
                else
                {
                    for (int z = 0; z >= (Mathf.Abs(cubes[1].transform.position.z) - Mathf.Abs(cubes[0].transform.position.z)) * mz; z += mz)
                    {
                        cubePosition.Add(new Vector3(cubes[0].transform.position.x + x, 0.5f, cubes[0].transform.position.z + z));
                    }
                    
                }
            }
            
        }
        return cubePosition;
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
