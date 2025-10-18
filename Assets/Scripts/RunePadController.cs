using UnityEngine;
using UnityEngine.EventSystems;

public class RunePadController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("References")]
    [SerializeField] private RectTransform runePadRect;
    [SerializeField] private RectTransform lineContainer;
    
    private Canvas parentCanvas;
    private bool isPointerInside = false;
    
    private void Awake()
    {
        if (runePadRect == null)
        {
            runePadRect = GetComponent<RectTransform>();
        }
        
        if (lineContainer == null)
        {
            Debug.LogError("Line Container reference is missing! Please assign it in the Inspector.");
        }
        
        parentCanvas = GetComponentInParent<Canvas>();
        
        if (parentCanvas == null)
        {
            Debug.LogError("RunePadController must be a child of a Canvas!");
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerInside = true;
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerInside = false;
    }
    
    public bool IsPointerInside()
    {
        return isPointerInside;
    }
    
    public bool IsPositionInsideRunePad(Vector2 screenPosition)
    {
        if (runePadRect == null || parentCanvas == null)
        {
            return false;
        }
        
        Camera cam = GetCanvasCamera();
        return RectTransformUtility.RectangleContainsScreenPoint(runePadRect, screenPosition, cam);
    }
    
    public Vector2 ScreenToLocalPosition(Vector2 screenPosition)
    {
        if (lineContainer == null || parentCanvas == null)
        {
            Debug.LogError("Cannot convert position: Missing references!");
            return Vector2.zero;
        }
        
        Camera cam = GetCanvasCamera();
        
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            lineContainer,
            screenPosition,
            cam,
            out Vector2 localPosition
        );
        
        return localPosition;
    }
    
    private Camera GetCanvasCamera()
    {
        if (parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            return null;
        }
        
        return parentCanvas.worldCamera;
    }
    
    public RectTransform GetRunePadRect()
    {
        return runePadRect;
    }
    
    public RectTransform GetLineContainer()
    {
        return lineContainer;
    }
}

