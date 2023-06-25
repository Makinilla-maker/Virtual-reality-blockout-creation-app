using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using TMPro;
using Unity.VisualScripting;
using Unity.XR.CoreUtils;
//using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.XR.Haptics;
using UnityEngine.UIElements;
using UnityEngine.XR.Interaction.Toolkit;
using static UnityEditor.Experimental.GraphView.GraphView;

public enum State
{
    NONE,
    SELECTED,
    PLACING,
    RISING,
    CREATING,
    OBJECT_CREATED,
}
public enum Mode
{
    NONE,
    CUBE,
    CILLINDER,
    MESH,
    MATERIALS,
    LIGHT,
    DELETE,
}

public class SpawnCube : MonoBehaviour
{
    public GameObject mainHand;
    private XRInteractorLineVisual lineRenderer;
    //Inputs
    public InputActionProperty colorButtonClicked;
    public InputActionProperty joystickUpRight;
    public InputActionProperty triggerRight;
    
    public InputActionProperty joystickUpLeft;
    public InputActionProperty triggerLeft;

    public GameObject cube;
    public GameObject cillinder;
    public GameObject voxelPrfab;
    public Transform placeTransform;
    public Material materialOnceCreated;

    public float objectOffset;


    public State actionState;
    public Mode modeState;

    public List<GameObject> cubes;
    private GameObject lightPoint;

    //Provisional
    public GameObject selectedObject;
    public List<GameObject> allObjects;
    List<GameObject> destrollObjects;
    GameObject objectToInstantiate;
    GameObject objectToPlace;
    GameObject voxel;
    bool gravActive;
    CanvasGroup gravCanvas;
    bool pressAgain;

    GameObject lastCube;
    bool firstCubePlaced;
    bool firstCillinder;

    //Configuration
    public bool cubeCheck;
    public bool customMeshCheck;
    public bool materialMode;

    GameObject canvasRight;
    GameObject canvasLeft;
    public MenuManager gridSize;


    // Start is called before the first frame update
    void Start()
    {
        cubes = new List<GameObject>();
        lineRenderer = mainHand.GetComponent<XRInteractorLineVisual>();
        lineRenderer.enabled = false;
        actionState = State.NONE;
        gravActive = false;
        firstCubePlaced = false;
        firstCillinder = false;
        canvasLeft = GameObject.Find("HandMenuLeft");
        canvasRight = GameObject.Find("HandMenuRight");
        canvasRight.SetActive(false);
        gravCanvas = GameObject.Find("CanvasAraSi").GetComponent<CanvasGroup>();
        pressAgain = true;
    }
    public void SetMainHand(GameObject hand)
    {
        mainHand = hand;
        if (hand.name == "RightHand Controller")
        {
            canvasLeft.SetActive(true);
            canvasRight.SetActive(false);
        }
        else
        {
            canvasLeft.SetActive(false);
            canvasRight.SetActive(true);
        }
        lineRenderer = mainHand.GetComponent<XRInteractorLineVisual>();
    }
    public void SetMode(string modeName)
    {
        switch(modeName)
        {
            case "NONE":
                modeState = Mode.NONE;
                actionState = State.NONE;
                Destroy(objectToInstantiate);
                lineRenderer.enabled = false;
                break;
            case "CUBE":
                modeState = Mode.CUBE;
                actionState = State.NONE;
                Destroy(objectToInstantiate);
                lineRenderer.enabled = true;
                break;
            case "CILLINDER":
                modeState = Mode.CILLINDER;
                actionState = State.NONE;
                Destroy(objectToInstantiate);
                lineRenderer.enabled = true;
                break;
            case "MESH":
                modeState = Mode.MESH;
                actionState = State.NONE;
                Destroy(objectToInstantiate);
                lineRenderer.enabled = true;
                break;
            case "MATERIALS":
                modeState = Mode.MATERIALS;
                actionState = State.NONE;
                Destroy(objectToInstantiate);
                lineRenderer.enabled = true;
                break;
            case "LIGHT":
                modeState = Mode.LIGHT;
                actionState = State.NONE;
                Destroy(objectToInstantiate);
                lineRenderer.enabled = false;
                break;
            case "DELETE":
                modeState = Mode.DELETE;
                actionState = State.NONE;
                Destroy(objectToInstantiate);
                lineRenderer.enabled = true;
                break;
        }

    }
    // Update is called once per frame
    void Update()
    {
        Vector2 joystickUpValue;
        float export;
        float grav = colorButtonClicked.action.ReadValue<float>();
        if (mainHand.name == "RightHand Controller")
        {
            joystickUpValue = joystickUpRight.action?.ReadValue<Vector2>() ?? Vector2.zero;
            export = triggerRight.action.ReadValue<float>();
        }
        else
        {
            joystickUpValue = joystickUpLeft.action?.ReadValue<Vector2>() ?? Vector2.zero;
            export = triggerLeft.action.ReadValue<float>();
        }
        if (grav > .01f && pressAgain)
        {
            pressAgain = false;
            gravCanvas.alpha = 1;

            if (gravActive)
                gravCanvas.gameObject.GetNamedChild("GravText").GetComponent<TMP_Text>().text = "Gravity On";
            else
                gravCanvas.gameObject.GetNamedChild("GravText").GetComponent<TMP_Text>().text = "Gravity Off";
            
            gravActive = !gravActive;
            GetComponent<ContinuousMoveProviderBase>().enableFly = gravActive;
        }
        if (grav < 0.5f && !pressAgain)
        {
            gravCanvas.alpha = 0;
            pressAgain = true;
        }
        if (modeState == Mode.CUBE || modeState == Mode.MESH || modeState == Mode.CILLINDER)
        {
            if (actionState == State.NONE)
            {
                if (modeState == Mode.CUBE || modeState == Mode.MESH)
                {
                    
                    objectToInstantiate = Instantiate(cube, mainHand.GetNamedChild("ObjectPoint").transform);
                    objectToInstantiate.gameObject.transform.localScale = new Vector3(3f, 3f, 3f);

                }
                else if (modeState == Mode.CILLINDER)
                {
                    objectToInstantiate = Instantiate(cillinder, mainHand.GetNamedChild("ObjectPoint").transform);
                    objectToInstantiate.gameObject.transform.localScale = new Vector3(2f, 2f, 2f);
                    objectToInstantiate.gameObject.transform.rotation = cillinder.transform.rotation;
                }

                actionState = State.SELECTED;
            }
            if ((actionState == State.SELECTED || actionState == State.PLACING))
            {
                //lineRenderer.enabled = true;
                switch (modeState)
                {
                    case Mode.CUBE:
                        CreatingBigCube(export);
                        break;
                    case Mode.MESH:
                        CreatingCustomMesh(export);
                        break;
                    case Mode.CILLINDER:
                        Debug.Log(export);
                        SettingCillinder(export);
                        break;
                }
            }
            if (actionState == State.PLACING && export < .1f)
            {
                //actionState = State.CREATING;

                switch (modeState)
                {
                    case Mode.CUBE:
                        CreatingVoxel2();
                        actionState = State.RISING;
                        break;
                    case Mode.MESH:
                        CreatingVoxel1();
                        actionState = State.RISING;
                        break;
                    case Mode.CILLINDER:
                        CreatingCilider();
                        break;
                }

            }
            if (actionState == State.RISING)
            {
                float yScale = GetCubeHeight(mainHand.transform.position);

                Vector3 newScale = voxel.transform.localScale;
                newScale.y = RoundFloat(yScale, 1f);
                voxel.transform.localScale = newScale;
                //StartCoroutine(ScaleUp());

            }
            if (actionState == State.RISING && export > .1f)
            {
                actionState = State.OBJECT_CREATED;
            }
            if (actionState == State.OBJECT_CREATED && export < .1f)
            {
                actionState = State.SELECTED;
            }

            if (joystickUpValue.y > .1f)
            {
                Debug.Log(joystickUpValue.y);
            }
        }
        else if (modeState == Mode.LIGHT)
        {
            if (export > .1f)
            {
                lightPoint.transform.parent = null;
            }
        }
        else if (modeState == Mode.MATERIALS || modeState == Mode.NONE)
        {
            RaycastHit hit;
            if (Physics.Raycast(mainHand.transform.position, mainHand.transform.forward, out hit, Mathf.Infinity))
            {
                Debug.DrawLine(mainHand.transform.position, hit.point);
                if(export > .1f && hit.transform.tag != "Floor")
                {
                    selectedObject = hit.transform.gameObject;
                    SetOutlineShader(selectedObject);
                }
            }
        }
        else if(modeState == Mode.DELETE)
        {
            RaycastHit hit;
            if (Physics.Raycast(mainHand.transform.position, mainHand.transform.forward, out hit, Mathf.Infinity))
            {
                if(hit.transform.tag != "Floor")
                {
                    SetOutlineShader(hit.transform.gameObject);
                    if (export > .1)
                    {
                        Debug.Log("_____________________________________________________________________");
                        allObjects.Remove(hit.transform.gameObject);
                        Destroy(hit.transform.gameObject);
                    }
                }
            }
        }
        if (objectToInstantiate != null)
        {
            //objectToInstantiate.gameObject.transform.position = placeTransform.position;
            //objectToInstantiate.gameObject.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
        }
    }
    void SetOutlineShader(GameObject so)
    {
        foreach(GameObject obj in allObjects)
        {
            obj.layer = 0;
        }
        so.layer = 3;
    }
    public void SetLight(GameObject l)
    {
        lightPoint = l;
    }    
    void SettingCillinder(float export)
    {
        RaycastHit hit;
        if (Physics.Raycast(mainHand.transform.position, mainHand.transform.forward, out hit, Mathf.Infinity))
        {
            Debug.DrawLine(mainHand.transform.position, hit.point);
            Debug.Log(hit.transform.tag);


            Vector3 pos;
            pos = hit.point;
            pos.x = RoundFloat(pos.x, gridSize.gridSize);
            pos.y = RoundFloat(pos.y, 1f);
            pos.z = RoundFloat(pos.z, gridSize.gridSize);
            //transform.position = pos;


            if (!hit.transform.CompareTag("ObjectPlacing") && export > .0f)
            {
                if (!firstCillinder)
                {
                    firstCillinder = true;
                    Debug.Log("Intanciating cuboooooooooooooooooooooooooooooooooooooooooooooooooooooooo");
                    objectToPlace = CreateObject(cillinder, pos, "ObjectPlacing");
                    objectToPlace.gameObject.transform.position = pos;
                    objectToPlace.gameObject.transform.localScale = new Vector3(10, 10, 10);
                }
                else
                {
                    objectToPlace.gameObject.transform.position = pos;
                    objectToPlace.gameObject.transform.localScale = new Vector3(10, 10, 10);
                }
                actionState = State.PLACING;
            }

        }
        else
        {
            Debug.Log("No collision detected.");
        }
    }
    void CreatingCilider()
    {
        RaycastHit hit;
        if (Physics.Raycast(mainHand.transform.position, mainHand.transform.forward, out hit, Mathf.Infinity))
        {
            Debug.DrawLine(mainHand.transform.position, hit.point);
            Debug.Log(hit.transform.position);
            Debug.Log(objectToPlace.transform.position);
            Debug.Log((hit.transform.position - objectToPlace.transform.position).magnitude);
            float radius = Mathf.Abs((hit.transform.position - objectToPlace.transform.position).magnitude);
            Vector3 scale = objectToPlace.transform.localScale;
            scale.x = radius * 2; // Double the radius to set the diameter
            scale.y = radius * 2; // Double the radius to set the diameter
            objectToPlace.transform.localScale = scale;
        }
    }
    void CreatingCustomMesh(float export)
    {
        RaycastHit hit;
        if (Physics.Raycast(mainHand.transform.position, mainHand.transform.forward, out hit, Mathf.Infinity))
        {
            Debug.DrawLine(mainHand.transform.position, hit.point);
            Debug.Log(hit.transform.tag);


            Vector3 pos;
            pos = hit.point;
            pos.x = RoundFloat(pos.x, gridSize.gridSize);
            pos.y = RoundFloat(pos.y, 1f);
            pos.z = RoundFloat(pos.z, gridSize.gridSize);
            //pos.x = Mathf.RoundToInt(pos.x / 1f) * 1f;
            //pos.y = (Mathf.RoundToInt(pos.y / 1f) * 1f) + 0.5f;
            //pos.z = Mathf.RoundToInt(pos.z / 1f) * 1f;
            //transform.position = pos;


            if (!hit.transform.CompareTag("ObjectPlacing") && export > .0f)
            {
                Debug.Log("Intanciating cuboooooooooooooooooooooooooooooooooooooooooooooooooooooooo");
                objectToPlace = CreateObject(cube, pos, "ObjectPlacing");
                objectToPlace.gameObject.transform.position = pos;
                objectToPlace.gameObject.transform.localScale = new Vector3(50, 50, 50);
                cubes.Add(objectToPlace.gameObject);

                actionState = State.PLACING;
            }

        }
        else
        {
            Debug.Log("No collision detected.");
        }
    }
    float RoundFloat(float valueToRound, float valeRounded)
    {
        float value;
        return value = Mathf.RoundToInt(valueToRound / gridSize.gridSize) * valeRounded;
    }
    void CreatingVoxel1()
    {
        List<Vector3> cubePosition = new List<Vector3>();

        voxel = CreateObject(voxelPrfab, cubes[0].transform.position, "ObjectToExport");

        foreach (GameObject cube in cubes)
        {
            Vector3 posi = cube.transform.position - cubes[0].transform.position;
            posi.y += 0.5f;
            cubePosition.Add(posi);
            Destroy(cube);
        }

        Debug.Log("Creating Voxel");
        voxel.GetComponent<VoxelRender>().GenerateVoxelMesh(cubePosition);
        voxel.GetComponent<MeshRenderer>().material = materialOnceCreated;
        voxel.AddComponent<MeshCollider>();
        allObjects.Add(voxel);
        cubePosition.Clear();
        cubes.Clear();
    }
    void CreatingBigCube(float export)
    {
        RaycastHit hit;
        if (Physics.Raycast(mainHand.transform.position, mainHand.transform.forward, out hit, Mathf.Infinity))
        {
            Debug.DrawLine(mainHand.transform.position, hit.point);
            Debug.Log(hit.transform.tag);

            Debug.Log(hit.normal);

            Vector3 pos;

            pos = hit.point;
            pos.x = RoundFloat(pos.x, gridSize.gridSize);
            pos.y = RoundFloat(pos.y, 1f);
            pos.z = RoundFloat(pos.z, gridSize.gridSize);

            if(hit.normal.x < 0 || hit.normal.y < 0 || hit.normal.z < 0)    pos += hit.normal;

            if (!hit.transform.CompareTag("ObjectPlacing") && export > .0f)
            {
                if (!firstCubePlaced)
                {
                    firstCubePlaced = true;
                    Debug.Log("Intanciating cuboooooooooooooooooooooooooooooooooooooooooooooooooooooooo");
                    objectToPlace = CreateObject(cube, pos, "ObjectPlacing");
                    objectToPlace.gameObject.transform.position = pos;
                    objectToPlace.gameObject.transform.localScale = new Vector3(50f, 50f, 50f);
                    cubes.Add(objectToPlace.gameObject);
                    lastCube = CreateObject(cube, pos, "ObjectPlacing");
                    cubes.Add(lastCube.gameObject);
                }
                else
                {
                    lastCube.gameObject.transform.position = pos;
                    objectToPlace.gameObject.transform.localScale = new Vector3(50f, 50f, 50f);
                }
                actionState = State.PLACING;
            }

        }
        else
        {
            Debug.Log("No collision detected.");
        }
    }
    void CreatingVoxel2()
    {
        List<Vector3> cubePosition = new List<Vector3>();

        if (cubes[1].transform.position.y < cubes[0].transform.position.y)
        {
            GameObject go = cubes[0];
            cubes[0] = cubes[1];
            cubes[1] = go;
        }

        voxel = CreateObject(voxelPrfab, cubes[0].transform.position, "ObjectToExport");

        int mx = 0;
        int mz = 0;

        if (cubes[1].transform.position.x > cubes[0].transform.position.x)
            mx = 1;
        else if (cubes[1].transform.position.x < cubes[0].transform.position.x)
            mx = -1;

        if (cubes[1].transform.position.z > cubes[0].transform.position.z)
            mz = 1;
        else if (cubes[1].transform.position.z < cubes[0].transform.position.z)
            mz = -1;

        cubePosition = CreateMeshVoxel(mx, mz);

        if (cubePosition.Count == 0)
        {
            // Ambos cubos están en la misma posición, agregar una única posición
            cubePosition.Add(new Vector3(cubes[0].transform.position.x - voxel.transform.position.x, cubes[0].transform.position.y + 0.5f - voxel.transform.position.y, cubes[0].transform.position.z - voxel.transform.position.z));
        }

        Debug.Log("Creating Voxel");
        voxel.GetComponent<VoxelRender>().GenerateVoxelMesh(cubePosition);
        voxel.GetComponent<MeshRenderer>().material = materialOnceCreated;
        voxel.AddComponent<BoxCollider>();
        allObjects.Add(voxel);

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

        if (mx != 0 && mz != 0)
        {
            for (int x = 0; Mathf.Abs(x) <= Mathf.Abs(cubes[1].transform.position.x - cubes[0].transform.position.x); x += mx)
            {
                for (int z = 0; Mathf.Abs(z) <= Mathf.Abs(cubes[1].transform.position.z - cubes[0].transform.position.z); z += mz)
                {
                    cubePosition.Add(new Vector3(cubes[0].transform.position.x + x - voxel.transform.position.x, cubes[0].transform.position.y + 0.5f - voxel.transform.position.y, cubes[0].transform.position.z + z - voxel.transform.position.z));
                }
            }
        }
        else if (mx != 0)
        {
            for (int x = 0; Mathf.Abs(x) <= Mathf.Abs(cubes[1].transform.position.x - cubes[0].transform.position.x); x += mx)
            {
                cubePosition.Add(new Vector3(cubes[0].transform.position.x + x - voxel.transform.position.x, cubes[0].transform.position.y + 0.5f - voxel.transform.position.y, cubes[0].transform.position.z - voxel.transform.position.z));
            }
        }
        else if (mz != 0)
        {
            for (int z = 0; Mathf.Abs(z) <= Mathf.Abs(cubes[1].transform.position.z - cubes[0].transform.position.z); z += mz)
            {
                cubePosition.Add(new Vector3(cubes[0].transform.position.x - voxel.transform.position.x, cubes[0].transform.position.y + 0.5f - voxel.transform.position.y, cubes[0].transform.position.z + z - voxel.transform.position.z));
            }
        }

        return cubePosition;
    }
    float GetCubeHeight(Vector3 controllerPosition)
    {
        float desiredHeight = controllerPosition.y; // Obtener la altura deseada del controlador de realidad virtual
        float groundHeight = voxel.transform.position.y;// Altura del suelo predefinida si no hay colisión
        float finalHeigh = desiredHeight - groundHeight;// Sumar la altura deseada a la altura del suelo

        if (finalHeigh < 1)
            return 1; 
        else
            return finalHeigh;
    }
    GameObject CreateObject(GameObject go, Vector3 pos, string tag)
    {
        GameObject inst = Instantiate(go, pos, go.transform.rotation);
        inst.transform.tag = tag;
        return inst;
    }
}
