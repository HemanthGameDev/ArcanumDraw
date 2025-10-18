using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class GestureDrawingManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RunePadController runePadController;
    [SerializeField] private GestureLineRenderer lineRenderer;
    
    [Header("Touch Settings")]
    [SerializeField] private float minDistanceBetweenPoints = 1f;
    [SerializeField] private float doubleTapTimeWindow = 0.3f;
    [SerializeField] private float doubleTapMaxDistance = 50f;
    
    private InputSystem_Actions inputActions;
    
    private bool isDrawing = false;
    private List<GesturePoint> currentGesturePoints = new List<GesturePoint>();
    private Vector2 lastRecordedScreenPosition;
    
    private float lastTapTime = -1f;
    private Vector2 lastTapScreenPosition;
    private bool preventDrawingAfterDoubleTap = false;
    
    private void Awake()
    {
        inputActions = new InputSystem_Actions();
        
        if (runePadController == null)
        {
            Debug.LogError("RunePadController reference is missing!");
        }
        
        if (lineRenderer == null)
        {
            Debug.LogError("GestureLineRenderer reference is missing!");
        }
    }
    
    private void OnEnable()
    {
        inputActions.Enable();
        inputActions.Gesture.Touch.started += OnTouchBegan;
        inputActions.Gesture.Touch.canceled += OnTouchEnded;
    }
    
    private void OnDisable()
    {
        inputActions.Gesture.Touch.started -= OnTouchBegan;
        inputActions.Gesture.Touch.canceled -= OnTouchEnded;
        inputActions.Disable();
    }
    
    private void Update()
    {
        if (isDrawing)
        {
            UpdateDrawing();
        }
    }
    
    private void OnTouchBegan(InputAction.CallbackContext context)
    {
        Vector2 screenPosition = GetCurrentScreenPosition();
        
        if (!runePadController.IsPositionInsideRunePad(screenPosition))
        {
            return;
        }
        
        if (CheckForDoubleTap(screenPosition))
        {
            HandleDoubleTap();
            return;
        }
        
        RecordTapForDoubleTapDetection(screenPosition);
        
        if (!preventDrawingAfterDoubleTap)
        {
            InitiateDrawing(screenPosition);
        }
    }
    
    private bool CheckForDoubleTap(Vector2 currentScreenPosition)
    {
        if (lastTapTime < 0)
        {
            return false;
        }
        
        float timeSinceLastTap = Time.time - lastTapTime;
        
        if (timeSinceLastTap > doubleTapTimeWindow)
        {
            return false;
        }
        
        float distance = Vector2.Distance(currentScreenPosition, lastTapScreenPosition);
        
        return distance <= doubleTapMaxDistance;
    }
    
    private void HandleDoubleTap()
    {
        preventDrawingAfterDoubleTap = true;
        lastTapTime = -1f;
        
        lineRenderer.ClearAllLinesWithFade();
        
        Debug.Log("Double-Tap Detected: Clearing all gesture lines");
    }
    
    private void RecordTapForDoubleTapDetection(Vector2 screenPosition)
    {
        lastTapTime = Time.time;
        lastTapScreenPosition = screenPosition;
    }
    
    private void InitiateDrawing(Vector2 screenPosition)
    {
        isDrawing = true;
        currentGesturePoints.Clear();
        
        Vector2 localPosition = runePadController.ScreenToLocalPosition(screenPosition);
        
        lineRenderer.StartNewGestureLine(localPosition);
        
        GesturePoint initialPoint = new GesturePoint(localPosition, Time.time);
        currentGesturePoints.Add(initialPoint);
        
        lastRecordedScreenPosition = screenPosition;
        
        Debug.Log($"Drawing Initiated at Screen: {screenPosition}, Local: {localPosition}");
    }
    
    private void UpdateDrawing()
    {
        Vector2 currentScreenPosition = GetCurrentScreenPosition();
        
        if (!runePadController.IsPositionInsideRunePad(currentScreenPosition))
        {
            CompleteDrawing();
            return;
        }
        
        float distance = Vector2.Distance(currentScreenPosition, lastRecordedScreenPosition);
        
        if (distance >= minDistanceBetweenPoints)
        {
            AddPointToCurrentGesture(currentScreenPosition);
        }
    }
    
    private void AddPointToCurrentGesture(Vector2 screenPosition)
    {
        Vector2 localPosition = runePadController.ScreenToLocalPosition(screenPosition);
        
        GesturePoint point = new GesturePoint(localPosition, Time.time);
        currentGesturePoints.Add(point);
        
        lineRenderer.AddPointToCurrentLine(localPosition);
        
        lastRecordedScreenPosition = screenPosition;
    }
    
    private void OnTouchEnded(InputAction.CallbackContext context)
    {
        if (preventDrawingAfterDoubleTap)
        {
            preventDrawingAfterDoubleTap = false;
            return;
        }
        
        if (isDrawing)
        {
            CompleteDrawing();
        }
    }
    
    private void CompleteDrawing()
    {
        isDrawing = false;
        
        if (currentGesturePoints.Count >= 2)
        {
            lineRenderer.FinalizeCurrentLine();
            ProcessCompletedGesture(currentGesturePoints);
        }
        else
        {
            lineRenderer.DiscardCurrentLine();
            Debug.Log("Gesture too short - discarded (single tap, no drag)");
        }
        
        currentGesturePoints.Clear();
    }
    
    private void ProcessCompletedGesture(List<GesturePoint> gesturePoints)
    {
        Debug.Log($"Gesture Completed: {gesturePoints.Count} points recorded");
        Debug.Log("Ready for gesture recognition system (Phase 2.2)");
    }
    
    private Vector2 GetCurrentScreenPosition()
    {
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            return inputActions.Gesture.Position.ReadValue<Vector2>();
        }
        else if (Mouse.current != null)
        {
            return inputActions.Gesture.MousePosition.ReadValue<Vector2>();
        }
        
        return Vector2.zero;
    }
    
    public List<GesturePoint> GetCurrentGesturePoints()
    {
        return new List<GesturePoint>(currentGesturePoints);
    }
}
