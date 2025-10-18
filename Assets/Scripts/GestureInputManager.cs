using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class GestureInputManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RunePadController runePad;
    [SerializeField] private LineDrawer lineDrawer;
    
    [Header("Input Settings")]
    [SerializeField] private float minDistanceBetweenPoints = 0.5f;
    [SerializeField] private float doubleTapTimeWindow = 0.3f;
    [SerializeField] private float doubleTapMaxDistance = 50f;
    [SerializeField] private int aggressiveFrameCount = 10;
    
    private InputSystem_Actions inputActions;
    private bool isDrawing = false;
    private List<GesturePoint> currentGesturePoints = new List<GesturePoint>();
    private Vector2 lastRecordedPosition;
    private int framesDrawing = 0;
    
    private float lastTapTime = 0f;
    private Vector2 lastTapPosition;
    private bool waitingForSecondTap = false;
    
    private void Awake()
    {
        inputActions = new InputSystem_Actions();
    }
    
    private void OnEnable()
    {
        inputActions.Enable();
        inputActions.Gesture.Touch.started += OnTouchStarted;
        inputActions.Gesture.Touch.canceled += OnTouchEnded;
    }
    
    private void OnDisable()
    {
        inputActions.Gesture.Touch.started -= OnTouchStarted;
        inputActions.Gesture.Touch.canceled -= OnTouchEnded;
        inputActions.Disable();
    }
    
    private void Update()
    {
        if (waitingForSecondTap && Time.time - lastTapTime > doubleTapTimeWindow)
        {
            waitingForSecondTap = false;
        }
        
        if (isDrawing)
        {
            framesDrawing++;
            Vector2 currentPosition = GetCurrentInputPosition();
            
            if (!runePad.IsPositionInsideRunePad(currentPosition))
            {
                EndDrawing();
                return;
            }
            
            float distance = Vector2.Distance(currentPosition, lastRecordedPosition);
            
            if (framesDrawing <= aggressiveFrameCount || distance >= minDistanceBetweenPoints)
            {
                RecordGesturePoint(currentPosition);
                lastRecordedPosition = currentPosition;
            }
        }
    }
    
    private void OnTouchStarted(InputAction.CallbackContext context)
    {
        Vector2 touchPosition = GetCurrentInputPosition();
        
        if (!runePad.IsPositionInsideRunePad(touchPosition))
        {
            return;
        }
        
        if (waitingForSecondTap)
        {
            float distance = Vector2.Distance(touchPosition, lastTapPosition);
            
            if (distance <= doubleTapMaxDistance)
            {
                lineDrawer.ClearAllLines();
                waitingForSecondTap = false;
                Debug.Log("Double-tap detected - Clearing all lines");
                return;
            }
        }
        
        waitingForSecondTap = true;
        lastTapTime = Time.time;
        lastTapPosition = touchPosition;
        
        StartDrawing(touchPosition);
    }
    
    private void OnTouchEnded(InputAction.CallbackContext context)
    {
        if (isDrawing)
        {
            EndDrawing();
        }
    }
    
    private void StartDrawing(Vector2 position)
    {
        isDrawing = true;
        framesDrawing = 0;
        currentGesturePoints.Clear();
        
        lineDrawer.StartNewLine();
        
        RecordGesturePoint(position);
        RecordGesturePoint(position);
        RecordGesturePoint(position);
        
        lastRecordedPosition = position;
        
        Debug.Log($"Drawing started at screen position: {position}");
    }
    
    private void RecordGesturePoint(Vector2 screenPosition)
    {
        Vector2 localPosition = runePad.ScreenToLocalPosition(screenPosition);
        GesturePoint point = new GesturePoint(localPosition, Time.time);
        
        currentGesturePoints.Add(point);
        lineDrawer.AddPoint(localPosition);
        
        if (currentGesturePoints.Count <= 3)
        {
            Debug.Log($"Point {currentGesturePoints.Count}: Screen={screenPosition}, Local={localPosition}");
        }
    }
    
    private void EndDrawing()
    {
        isDrawing = false;
        
        if (currentGesturePoints.Count > 0)
        {
            lineDrawer.FinishLine();
            ProcessGesture(currentGesturePoints);
        }
        else
        {
            lineDrawer.CancelCurrentLine();
        }
    }
    
    private void ProcessGesture(List<GesturePoint> points)
    {
        Debug.Log($"Gesture recorded with {points.Count} points");
    }
    
    private Vector2 GetCurrentInputPosition()
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
