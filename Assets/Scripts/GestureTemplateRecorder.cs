using UnityEngine;
using System.Collections.Generic;

public class GestureTemplateRecorder : MonoBehaviour
{
    public static GestureTemplateRecorder Instance { get; private set; }
    
    private bool isRecording = false;
    private List<List<Vector2>> recordedGestures = new List<List<Vector2>>();
    
    public bool IsRecording => isRecording;
    public int RecordedGestureCount => recordedGestures.Count;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void StartRecording()
    {
        isRecording = true;
        recordedGestures.Clear();
        Debug.Log("<color=cyan>🎙 Started recording gestures for template generation</color>");
    }
    
    public void StopRecording()
    {
        isRecording = false;
        Debug.Log($"<color=yellow>⏹ Stopped recording. Captured {recordedGestures.Count} gestures</color>");
    }
    
    public void RecordGesture(List<Vector2> gesturePoints)
    {
        if (!isRecording) return;
        
        if (gesturePoints == null || gesturePoints.Count < 10)
        {
            Debug.LogWarning("Gesture too short to record");
            return;
        }
        
        List<Vector2> copy = new List<Vector2>(gesturePoints);
        recordedGestures.Add(copy);
        
        Debug.Log($"<color=green>✓ Recorded gesture {recordedGestures.Count} ({copy.Count} points)</color>");
        
#if UNITY_EDITOR
        NotifyEditorWindow(copy);
#endif
    }
    
    public List<List<Vector2>> GetRecordedGestures()
    {
        return new List<List<Vector2>>(recordedGestures);
    }
    
    public void ClearRecordings()
    {
        recordedGestures.Clear();
        Debug.Log("Cleared all recorded gestures");
    }
    
#if UNITY_EDITOR
    private void NotifyEditorWindow(List<Vector2> gesture)
    {
        var windowType = System.Type.GetType("PatternTemplateGeneratorWindow");
        if (windowType != null)
        {
            var window = UnityEditor.EditorWindow.GetWindow(windowType, false, null, false);
            if (window != null)
            {
                var method = windowType.GetMethod("OnGestureCompleted");
                if (method != null)
                {
                    method.Invoke(window, new object[] { gesture });
                }
            }
        }
    }
#endif
}
