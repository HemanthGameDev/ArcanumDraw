using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class GestureDrawingManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RunePadController runePadController;
    [SerializeField] private GestureLineRenderer lineRenderer;
    [SerializeField] private GestureRecognizer gestureRecognizer;
    [SerializeField] private GestureRecognizerNew gestureRecognizerNew;
    [SerializeField] private SpellCaster spellCaster;
    
    [Header("Recognizer Settings")]
    [Tooltip("Use new GestureRecognizerNew (recommended) or legacy GestureRecognizer")]
    [SerializeField] private bool useNewRecognizer = true;
    
    [Header("Touch Settings")]
    [SerializeField] private float minDistanceBetweenPoints = 1f;
    [SerializeField] private float doubleTapTimeWindow = 0.3f;
    [SerializeField] private float doubleTapMaxDistance = 50f;
    
    private InputSystem_Actions inputActions;
    
    private bool isDrawing = false;
    private bool hasStartedVisualLine = false;
    private List<GesturePoint> currentGesturePoints = new List<GesturePoint>();
    private Vector2 lastRecordedScreenPosition;
    private float gestureStartTime = 0f;
    
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
        Vector2 screenPosition = ReadScreenPositionFromInput();
        
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
        hasStartedVisualLine = false;
        currentGesturePoints.Clear();
        gestureStartTime = Time.time;
        
        Vector2 localPosition = runePadController.ScreenToLocalPosition(screenPosition);
        
        GesturePoint initialPoint = new GesturePoint(localPosition, Time.time);
        currentGesturePoints.Add(initialPoint);
        
        lastRecordedScreenPosition = screenPosition;
        
        Debug.Log($"Drawing Initiated at Screen: {screenPosition}, Local: {localPosition} (visual pending movement)");
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
            if (!hasStartedVisualLine)
            {
                StartVisualLine();
            }
            
            AddPointToCurrentGesture(currentScreenPosition);
        }
    }
    
    private void StartVisualLine()
    {
        hasStartedVisualLine = true;
        
        Vector2 initialLocalPosition = currentGesturePoints[0].position;
        lineRenderer.StartNewGestureLine(initialLocalPosition);
        
        Debug.Log($"Visual line started at: {initialLocalPosition}");
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
        
        if (currentGesturePoints.Count >= 2 && hasStartedVisualLine)
        {
            lineRenderer.FinalizeCurrentLine();
            ProcessCompletedGesture(currentGesturePoints);
        }
        else
        {
            if (hasStartedVisualLine)
            {
                lineRenderer.DiscardCurrentLine();
            }
            Debug.Log("Gesture too short - discarded (single tap or no drag)");
        }
        
        currentGesturePoints.Clear();
        hasStartedVisualLine = false;
    }
    
    private void ProcessCompletedGesture(List<GesturePoint> gesturePoints)
    {
        Debug.Log($"Gesture Completed: {gesturePoints.Count} points recorded");
        
        if (useNewRecognizer && gestureRecognizerNew == null)
        {
            Debug.LogWarning("GestureRecognizerNew is enabled but not assigned!");
            return;
        }
        
        if (!useNewRecognizer && gestureRecognizer == null)
        {
            Debug.LogWarning("GestureRecognizer is not assigned!");
            return;
        }
        
        List<Vector3> points3D = new List<Vector3>();
        foreach (GesturePoint gp in gesturePoints)
        {
            points3D.Add(new Vector3(gp.position.x, gp.position.y, 0f));
        }
        
        float totalDrawingTime = Time.time - gestureStartTime;
        
        GestureRecognitionResult result = useNewRecognizer
            ? gestureRecognizerNew.RecognizeGesture(points3D, totalDrawingTime)
            : gestureRecognizer.RecognizeGesture(points3D, totalDrawingTime);
        
        if (result.success)
        {
            Debug.Log($"<color=green>{result.message}</color>");
            Debug.Log($"Speed: {result.drawSpeed:F2} | Direction: {result.drawDirection}");
            
            if (spellCaster != null)
            {
                spellCaster.AttemptCastSpell(result.recognizedSpell);
            }
            else
            {
                Debug.LogWarning("SpellCaster reference is missing. Cannot cast spell.");
            }
        }
        else
        {
            Debug.Log($"<color=yellow>{result.message}</color>");
            if (result.confidence > 0)
            {
                Debug.Log($"Best match confidence: {result.confidence:P0} (threshold not met)");
            }
        }
    }
    
    private Vector2 ReadScreenPositionFromInput()
    {
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            return Touchscreen.current.primaryTouch.position.ReadValue();
        }
        else if (Mouse.current != null)
        {
            return Mouse.current.position.ReadValue();
        }
        
        return Vector2.zero;
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
    
    public void ClearAllDrawings()
    {
        if (lineRenderer != null)
        {
            lineRenderer.ClearAllLinesWithFade();
        }
    }
}
