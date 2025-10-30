using UnityEngine;
using UnityEngine.UI;
using System.Reflection;

public class GestureRecognitionTuner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GestureRecognizerNew recognizer;
    
    [Header("Runtime Tuning (Keyboard Shortcuts)")]
    [Tooltip("Press + or = to increase tolerance by 0.05")]
    [SerializeField] private KeyCode increaseToleranceKey = KeyCode.Equals;
    
    [Tooltip("Press - to decrease tolerance by 0.05")]
    [SerializeField] private KeyCode decreaseToleranceKey = KeyCode.Minus;
    
    [Tooltip("Press R to reset to recommended 0.40")]
    [SerializeField] private KeyCode resetToleranceKey = KeyCode.R;
    
    [Header("Display")]
    [SerializeField] private bool showOnScreenDisplay = true;
    [SerializeField] private Text displayText;
    
    private float currentTolerance;
    private FieldInfo toleranceField;
    private const float MIN_TOLERANCE = 0.15f;
    private const float MAX_TOLERANCE = 0.70f;
    private const float TOLERANCE_STEP = 0.05f;
    private const float RECOMMENDED_TOLERANCE = 0.40f;
    
    private GUIStyle guiStyle;
    private bool showHelpMessage = true;
    
    private void Start()
    {
        if (recognizer == null)
        {
            recognizer = GetComponent<GestureRecognizerNew>();
        }
        
        if (recognizer != null)
        {
            toleranceField = typeof(GestureRecognizerNew).GetField("recognitionTolerance", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            
            if (toleranceField != null)
            {
                currentTolerance = (float)toleranceField.GetValue(recognizer);
            }
            else
            {
                currentTolerance = RECOMMENDED_TOLERANCE;
                Debug.LogWarning("Could not find recognitionTolerance field via reflection");
            }
        }
        
        guiStyle = new GUIStyle();
        guiStyle.fontSize = 20;
        guiStyle.normal.textColor = Color.white;
        guiStyle.alignment = TextAnchor.UpperLeft;
        guiStyle.padding = new RectOffset(10, 10, 10, 10);
        
        Debug.Log($"<color=cyan>Gesture Tuner Active! Current tolerance: {currentTolerance:F2}</color>");
        Debug.Log($"<color=cyan>Press [{increaseToleranceKey}] to increase, [{decreaseToleranceKey}] to decrease, [{resetToleranceKey}] to reset</color>");
    }
    
    private void Update()
    {
        if (recognizer == null) return;
        
        if (Input.GetKeyDown(increaseToleranceKey))
        {
            IncreaseTolerance();
        }
        
        if (Input.GetKeyDown(decreaseToleranceKey))
        {
            DecreaseTolerance();
        }
        
        if (Input.GetKeyDown(resetToleranceKey))
        {
            ResetToRecommended();
        }
        
        if (Input.GetKeyDown(KeyCode.H))
        {
            showHelpMessage = !showHelpMessage;
        }
        
        if (displayText != null)
        {
            UpdateDisplayText();
        }
    }
    
    private void OnGUI()
    {
        if (!showOnScreenDisplay) return;
        
        string display = GetStatusString();
        
        if (showHelpMessage)
        {
            display += GetHelpString();
        }
        
        GUI.Label(new Rect(10, 10, 500, 300), display, guiStyle);
    }
    
    private void IncreaseTolerance()
    {
        currentTolerance = Mathf.Min(currentTolerance + TOLERANCE_STEP, MAX_TOLERANCE);
        ApplyTolerance();
        Debug.Log($"<color=yellow>Tolerance INCREASED to {currentTolerance:F2} (more lenient - easier to recognize)</color>");
    }
    
    private void DecreaseTolerance()
    {
        currentTolerance = Mathf.Max(currentTolerance - TOLERANCE_STEP, MIN_TOLERANCE);
        ApplyTolerance();
        Debug.Log($"<color=yellow>Tolerance DECREASED to {currentTolerance:F2} (more strict - harder to recognize)</color>");
    }
    
    private void ResetToRecommended()
    {
        currentTolerance = RECOMMENDED_TOLERANCE;
        ApplyTolerance();
        Debug.Log($"<color=green>Tolerance RESET to recommended {RECOMMENDED_TOLERANCE:F2} (War of Wizards level!)</color>");
    }
    
    private void ApplyTolerance()
    {
        if (recognizer == null || toleranceField == null) return;
        
        toleranceField.SetValue(recognizer, currentTolerance);
    }
    
    private void UpdateDisplayText()
    {
        if (displayText == null) return;
        
        displayText.text = $"Tolerance: {currentTolerance:F2}";
        
        if (currentTolerance < 0.30f)
        {
            displayText.color = Color.red;
        }
        else if (currentTolerance > 0.50f)
        {
            displayText.color = Color.yellow;
        }
        else
        {
            displayText.color = Color.green;
        }
    }
    
    private string GetStatusString()
    {
        string status = $"<b>Recognition Tolerance: {currentTolerance:F2}</b>\n";
        
        if (currentTolerance < 0.25f)
        {
            status += "<color=red>⚠ TOO STRICT - May miss valid gestures!</color>\n";
        }
        else if (currentTolerance < 0.35f)
        {
            status += "<color=yellow>⚠ STRICT - Good for experienced users</color>\n";
        }
        else if (currentTolerance <= 0.45f)
        {
            status += "<color=green>✓ RECOMMENDED - War of Wizards level!</color>\n";
        }
        else if (currentTolerance <= 0.55f)
        {
            status += "<color=yellow>⚠ LENIENT - May have false positives</color>\n";
        }
        else
        {
            status += "<color=red>⚠ TOO LENIENT - Will match everything!</color>\n";
        }
        
        return status;
    }
    
    private string GetHelpString()
    {
        return "\n<size=14><b>Controls:</b>\n" +
               $"[{increaseToleranceKey}] Increase (+{TOLERANCE_STEP:F2})\n" +
               $"[{decreaseToleranceKey}] Decrease (-{TOLERANCE_STEP:F2})\n" +
               $"[{resetToleranceKey}] Reset to {RECOMMENDED_TOLERANCE:F2}\n" +
               "[H] Toggle Help\n\n" +
               "<b>Quick Guide:</b>\n" +
               "• Shield not working? Press [=]\n" +
               "• Too many false matches? Press [-]\n" +
               "• Start fresh? Press [R]\n\n" +
               "<b>How Tolerance Works:</b>\n" +
               "• LOWER = More strict (harder)\n" +
               "• HIGHER = More lenient (easier)\n" +
               "• War of Wizards uses ~0.40</size>";
    }
}
