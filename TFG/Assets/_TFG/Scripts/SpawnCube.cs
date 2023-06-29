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
//using static UnityEditor.Experimental.GraphView.GraphView;

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
    MESH,
    CILLINDER,
    RAMP,
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
    public GameObject ramp;
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
    bool secondCubePlaced;
    bool firstCillinder;
    bool deleting;

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
        secondCubePlaced = false;
        firstCillinder = false;
        canvasLeft = GameObject.Find("HandMenuLeft");
        canvasRight = GameObject.Find("HandMenuRight");
        canvasRight.SetActive(false);
        gravCanvas = GameObject.Find("CanvasAraSi").GetComponent<CanvasGroup>();
        pressAgain = true;
        deleting = false;

        SetPlayerSpeed(2);
        SwitchMovement("LINEAL");
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
                lineRenderer.enabled = false;
                break;
            case "CUBE":
                modeState = Mode.CUBE;
                lineRenderer.enabled = true;
                break;
            case "CILLINDER":
                modeState = Mode.CILLINDER;
                lineRenderer.enabled = true;
                break;
            case "MESH":
                modeState = Mode.MESH;
                lineRenderer.enabled = true;
                break;
            case "MATERIALS":
                modeState = Mode.MATERIALS;
                lineRenderer.enabled = true;
                break;
            case "LIGHT":
                modeState = Mode.LIGHT;
                lineRenderer.enabled = false;
                break;
            case "DELETE":
                modeState = Mode.DELETE;
                lineRenderer.enabled = true;
                break;
            case "RAMP":
                modeState = Mode.RAMP;
                lineRenderer.enabled = true;
                break;
        }

        actionState = State.NONE;
        Destroy(objectToPlace);
        Destroy(objectToInstantiate);
        DestroyListGO(cubes);

        firstCubePlaced = false;
        secondCubePlaced = false;
    }
    // Update is called once per frame
    void Update()
    {
        Vector2 joystickUpValue;
        float trigger;
        float grav = colorButtonClicked.action.ReadValue<float>();
        if (mainHand.name == "RightHand Controller")
        {
            joystickUpValue = joystickUpRight.action?.ReadValue<Vector2>() ?? Vector2.zero;
            trigger = triggerRight.action.ReadValue<float>();
        }
        else
        {
            joystickUpValue = joystickUpLeft.action?.ReadValue<Vector2>() ?? Vector2.zero;
            trigger = triggerLeft.action.ReadValue<float>();
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
        if (modeState == Mode.CUBE || modeState == Mode.MESH || modeState == Mode.CILLINDER|| modeState == Mode.RAMP)
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
                else if(modeState == Mode.RAMP)
                {
                    objectToInstantiate = Instantiate(ramp, mainHand.GetNamedChild("ObjectPoint").transform);
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
                        CreatingBigCube(trigger);
                        break;
                    case Mode.MESH:
                        CreatingCustomMesh(trigger);
                        break;
                    case Mode.CILLINDER:
                        SettingCillinder(trigger);
                        break;
                    case Mode.RAMP:
                        Debug.Log(trigger);
                        SettingRamp(trigger);
                        break;
                }
            }
            if (actionState == State.PLACING && trigger < .1f)
            {
                //actionState = State.CREATING;

                switch (modeState)
                {
                    case Mode.CUBE:
                        CreatingVoxelBigCube();
                        actionState = State.RISING;
                        break;
                    case Mode.MESH:
                        CreatingVoxelCustomMesh();
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
            if (actionState == State.RISING && trigger > .1f)
            {
                actionState = State.OBJECT_CREATED;
            }
            if (actionState == State.OBJECT_CREATED && trigger < .1f)
            {
                actionState = State.SELECTED;
            }
        }
        if (joystickUpValue.y > .1f)
        {
            Debug.Log(joystickUpValue);
            GetComponent<CharacterController>().Move(joystickUpValue/3);
        }

        else if (modeState == Mode.LIGHT)
        {
            if (trigger > .1f)
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
                if(trigger > .1f && hit.transform.tag != "Floor")
                {
                    selectedObject = hit.transform.gameObject;
                    SetOutlineShader(selectedObject);
                }
            }
        }
        else if(modeState == Mode.DELETE)
        {
            RaycastHit hit;
            if (Physics.Raycast(mainHand.transform.position, mainHand.transform.forward, out hit, Mathf.Infinity) && !deleting)
            {
                if(hit.transform.tag != "Floor")
                {
                    SetOutlineShader(hit.transform.gameObject);
                    if (trigger > .1)
                    {
                        deleting = true;
                        Debug.Log("_____________________________________________________________________");
                        allObjects.Remove(hit.transform.gameObject);
                        Destroy(hit.transform.gameObject);
                    }
                }
            }
            if(trigger < .1 && deleting) deleting = false;
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
    void SettingRamp(float export)
    {
        RaycastHit hit;
        if (Physics.Raycast(mainHand.transform.position, mainHand.transform.forward, out hit, Mathf.Infinity))
        {
            Debug.DrawLine(mainHand.transform.position, hit.point);

            Vector3 pos;
            pos = hit.point;

            //if(hit.normal.x < 0)    pos += hit.normal;
            //if(hit.normal.z < 0)    pos += hit.normal;

            pos.x = RoundFloat(pos.x, gridSize.gridSize);
            pos.y = RoundFloat(pos.y, 1f);
            pos.z = RoundFloat(pos.z, gridSize.gridSize) - 0.5f;

            Debug.Log(pos);
            Debug.Log(hit.normal);
            if (!hit.transform.CompareTag("ObjectPlacing"))
            {
                if (!firstCubePlaced)
                {
                    firstCubePlaced = true;
                    objectToPlace = CreateObject(ramp, pos, "ObjectPlacing");

                }
                else if (!secondCubePlaced)
                {
                    objectToPlace.gameObject.transform.position = pos;
                    objectToPlace.gameObject.transform.localScale = new Vector3(50f, 50f, 50f);
                }
            }
            if (export > .0f && !secondCubePlaced)
            {
                actionState = State.PLACING;
                secondCubePlaced = true;
            }
            else if(secondCubePlaced)
            {
                RotingRamp(export);
            }

        }
        else
        {
            Debug.Log("No collision detected.");
        }
    }
    void RotingRamp(float export)
    {
        RaycastHit hit;
        if (Physics.Raycast(mainHand.transform.position, mainHand.transform.forward, out hit, Mathf.Infinity))
        {
             Transform objectTransform = objectToPlace.transform;

            if(hit.transform.gameObject.tag != "ObjectPlacing")
            {
                Debug.Log(hit.point);
                Vector3 pos;
                pos = hit.point;
                pos.x = RoundFloat(pos.x, gridSize.gridSize);
                pos.y = RoundFloat(pos.y, 1f);
                pos.z = RoundFloat(pos.z, gridSize.gridSize) - 0.5f;

                Vector3 vectorDist = pos - objectToPlace.transform.position;
                float distanceZ = Mathf.Abs(vectorDist.z);
                float distanceY = Mathf.Abs(vectorDist.y);
                float distanceX = Mathf.Abs(vectorDist.x);

                Debug.Log("Esta es la ditnacia_______________________: " + distanceX + " " + distanceY + " " + distanceZ);


                // Obtener la rotación hacia el objetivo
                Quaternion targetRotation = Quaternion.LookRotation(pos - objectTransform.position);

                // Crear una rotación final con los valores deseados en los ejes X y Z
                Quaternion finalRotation = Quaternion.Euler(objectTransform.rotation.eulerAngles.x, targetRotation.eulerAngles.y, objectTransform.rotation.eulerAngles.z);

                // Asignar la rotación final al objeto
                objectTransform.rotation = finalRotation;
                Vector3 newScale;

                distanceX *= 50;
                distanceY *= 50;
                distanceZ *= 50;

                if (distanceX * 50 < 50) distanceX = 50;
                if (distanceY * 50 < 50) distanceY = 50;
                if (distanceZ * 50 < 50) distanceZ = 50;

                if(distanceX > distanceZ) distanceZ = distanceX;

                newScale = new Vector3(objectTransform.localScale.x, distanceZ, distanceY);
                Debug.Log(newScale);
                objectTransform.localScale = newScale;
            }
            if (export < .1f)
            {
                actionState = State.SELECTED;
                objectTransform.GetComponent<Renderer>().material = materialOnceCreated;
                firstCubePlaced = false;
                secondCubePlaced = false;
            }
            
        }

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

            if (!hit.transform.CompareTag("ObjectPlacing"))
            {
                if (!firstCubePlaced)
                {
                    firstCubePlaced = true;
                    secondCubePlaced = false;
                    objectToPlace = CreateObject(cube, pos, "ObjectPlacing");
                    cubes.Add(objectToPlace.gameObject);
                }
                else
                {
                    if (export > .0f && !secondCubePlaced)
                    {
                        firstCubePlaced = false;
                        secondCubePlaced = true;

                        actionState = State.PLACING;
                    }
                    objectToPlace.gameObject.transform.position = pos;
                    objectToPlace.gameObject.transform.localScale = new Vector3(50f, 50f, 50f);
                }
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
    void CreatingVoxelCustomMesh()
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
        firstCubePlaced = false;
        secondCubePlaced = false;
        cubePosition.Clear();
        cubes.Clear();
    }
    void CreatingBigCube(float trigger)
    {
        RaycastHit hit;
        if (Physics.Raycast(mainHand.transform.position, mainHand.transform.forward, out hit, Mathf.Infinity))
        {
            Debug.DrawLine(mainHand.transform.position, hit.point);

            Vector3 pos;
            pos = hit.point;

            //if(hit.normal.x < 0)    pos += hit.normal;
            //if(hit.normal.z < 0)    pos += hit.normal;

            pos.x = RoundFloat(pos.x, gridSize.gridSize);
            pos.y = RoundFloat(pos.y, 1f);
            pos.z = RoundFloat(pos.z, gridSize.gridSize);
            if(!hit.transform.CompareTag("ObjectPlacing"))
            {
                if (!firstCubePlaced)
                {
                    firstCubePlaced = true;
                    objectToPlace = CreateObject(cube, pos, "ObjectPlacing");
                    cubes.Add(objectToPlace.gameObject);

                }
                else
                {
                    if (trigger > .0f && !secondCubePlaced)
                    {
                        secondCubePlaced = true;
                        objectToPlace = CreateObject(cube, pos, "ObjectPlacing");
                        cubes.Add(objectToPlace.gameObject);
                    }
                    objectToPlace.gameObject.transform.position = pos;
                    objectToPlace.gameObject.transform.localScale = new Vector3(50f, 50f, 50f);
                    
                }
            }
            
            if(trigger > .0f && firstCubePlaced)    actionState = State.PLACING;

        }
        else
        {
            Debug.Log("No collision detected.");
        }
    }
    public void SetPlayerSpeed(int number)
    {

        float speed = GetComponent<ContinuousMoveProviderBase>().moveSpeed + number;
        GetComponent<ContinuousMoveProviderBase>().moveSpeed = speed;
    }    
    public void Exit()
    {
        Application.Quit();
    }
    void CreatingVoxelBigCube()
    {
        List<Vector3> cubePosition = new List<Vector3>();

        if(cubes.Count > 1)
        {
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
        }
        else
        {
            voxel = CreateObject(voxelPrfab, cubes[0].transform.position, "ObjectToExport");
            cubePosition.Add(new Vector3(cubes[0].transform.position.x - voxel.transform.position.x, cubes[0].transform.position.y + 0.5f - voxel.transform.position.y, cubes[0].transform.position.z - voxel.transform.position.z));
        }

        Debug.Log("Creating Voxel");
        voxel.GetComponent<VoxelRender>().GenerateVoxelMesh(cubePosition);
        voxel.GetComponent<MeshRenderer>().material = materialOnceCreated;
        voxel.AddComponent<BoxCollider>();
        allObjects.Add(voxel);

        DestroyListGO(cubes);

        firstCubePlaced = false;
        secondCubePlaced = false;
        cubePosition.Clear();
    }
    void DestroyListGO(List<GameObject> list)
    {
        foreach (GameObject cube in list)
        {
            Destroy(cube);
        }
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
    public void SwitchMovement(string type)
    {
        switch(type)
        {
            case "TP":
                GetComponent<TeleportationProvider>().enabled = true;
                GetComponent<ContinuousMoveProviderBase>().enabled = false;
                break;
            case "LINEAL":
                GetComponent<ContinuousMoveProviderBase>().enabled = true;
                GetComponent<TeleportationProvider>().enabled = false;
                break;
            default:
                Debug.Log("Non movement selected");
                break;

        }
    }
}
