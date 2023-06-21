using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;

public class MaterialManager : MonoBehaviour
{
    public XRRayInteractor rayInteractor;
    public Button colorButton;
    public GameObject sphere;
    public SpawnCube sc;

    private GameObject selectedObject;

    private void Start()
    {
        sc = GameObject.FindObjectOfType<SpawnCube>();
        selectedObject = sc.selectedObject;
    }

    public void SetMaterial()
    {
        if (rayInteractor.TryGetCurrentUIRaycastResult(out RaycastResult raycastResult))
        {
            Debug.Log(raycastResult.gameObject);
            Debug.Log(colorButton.gameObject);
            if (raycastResult.gameObject == colorButton.gameObject)
            {
                Debug.Log(raycastResult.gameObject);
                Debug.Log(colorButton.image.gameObject);
                Vector2 localPoint = GetLocalPoint(raycastResult);

                if (IsWithinImageBounds(localPoint))
                {
                    selectedObject = sc.selectedObject;
                    Color selectedColor = GetColorFromImage(localPoint);

                    // Use the selectedColor as desired (e.g., assign it to a material or apply it to an object).
                    //Vector3 newPos = raycastResult.worldPosition;
                   // newPos.z = sphere.transform.position.z;
                    //sphere.transform.position = newPos;
                    sphere.GetComponent<RawImage>().color = selectedColor;

                    if (selectedObject != null)
                        selectedObject.GetComponent<MeshRenderer>().material.color = selectedColor;
                
                }
            }
        }
    }
    public void SetIntensity()
    {
        selectedObject = sc.selectedObject;

        if (selectedObject != null)
        {
            Color color = selectedObject.GetComponent<MeshRenderer>().material.color;

            // Set the intensity by multiplying the RGB values of the color
            Debug.Log(color);
            Debug.Log(color);

            selectedObject.GetComponent<MeshRenderer>().material.color = color;
        }
    }

    private Vector2 GetLocalPoint(RaycastResult raycastResult)
    {
        RectTransform imageRect = colorButton.image.rectTransform;
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(colorButton.image.rectTransform, raycastResult.screenPosition, raycastResult.module.eventCamera, out localPoint);
        return new Vector2(localPoint.x + imageRect.pivot.x * imageRect.rect.width, localPoint.y + imageRect.pivot.y * imageRect.rect.height);
    }

    private bool IsWithinImageBounds(Vector2 localPoint)
    {
        Rect imageRect = colorButton.image.rectTransform.rect;
        return localPoint.x >= 0 && localPoint.y >= 0 && localPoint.x <= imageRect.width && localPoint.y <= imageRect.height;
    }

    private Color GetColorFromImage(Vector2 localPoint)
    {
        Vector2 normalizedPoint = new Vector2(localPoint.x / colorButton.image.rectTransform.rect.width, localPoint.y / colorButton.image.rectTransform.rect.height);
        return colorButton.image.sprite.texture.GetPixelBilinear(normalizedPoint.x,normalizedPoint.y);
        //return image.sprite.texture.GetPixelBilinear(normalizedPoint.x, normalizedPoint.y);
    }
}