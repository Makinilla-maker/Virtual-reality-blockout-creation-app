using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;

public class MaterialManager : MonoBehaviour
{
    public XRRayInteractor rayInteractor;
    public Button button;
    public GameObject sphere;
    public SpawnCube sc;
    private void Start()
    {
        sc = GameObject.FindObjectOfType<SpawnCube>();
    }

    public void SetMaterial()
    {
        if (rayInteractor.TryGetCurrentUIRaycastResult(out RaycastResult raycastResult))
        {
            Debug.Log(raycastResult.gameObject);
            Debug.Log(button.gameObject);
            if (raycastResult.gameObject == button.gameObject)
            {
                Debug.Log(raycastResult.gameObject);
                Debug.Log(button.image.gameObject);
                Vector2 localPoint = GetLocalPoint(raycastResult);

                if (IsWithinImageBounds(localPoint))
                {
                    Color selectedColor = GetColorFromImage(localPoint);
                    // Use the selectedColor as desired (e.g., assign it to a material or apply it to an object).
                    sphere.transform.position = raycastResult.worldPosition;
                    sphere.GetComponent<RawImage>().color = selectedColor;
                    sc.selectedObject.GetComponent<MeshRenderer>().material.color = selectedColor;
                }
            }
        }
    }

    private Vector2 GetLocalPoint(RaycastResult raycastResult)
    {
        RectTransform imageRect = button.image.rectTransform;
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(button.image.rectTransform, raycastResult.screenPosition, raycastResult.module.eventCamera, out localPoint);
        return new Vector2(localPoint.x + imageRect.pivot.x * imageRect.rect.width, localPoint.y + imageRect.pivot.y * imageRect.rect.height);
    }

    private bool IsWithinImageBounds(Vector2 localPoint)
    {
        Rect imageRect = button.image.rectTransform.rect;
        return localPoint.x >= 0 && localPoint.y >= 0 && localPoint.x <= imageRect.width && localPoint.y <= imageRect.height;
    }

    private Color GetColorFromImage(Vector2 localPoint)
    {
        Vector2 normalizedPoint = new Vector2(localPoint.x / button.image.rectTransform.rect.width, localPoint.y / button.image.rectTransform.rect.height);
        return button.image.sprite.texture.GetPixelBilinear(normalizedPoint.x,normalizedPoint.y);
        //return image.sprite.texture.GetPixelBilinear(normalizedPoint.x, normalizedPoint.y);
    }
}