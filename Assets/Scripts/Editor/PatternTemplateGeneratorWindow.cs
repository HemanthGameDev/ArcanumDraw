using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class PatternTemplateGeneratorWindow : EditorWindow
{
    private static SpellData targetSpell;
    private static PatternTemplateGeneratorWindow window;
    
    private List<List<Vector2>> recordedGestures = new List<List<Vector2>>();
    private GestureRecognizerNew recognizer;
    private bool isRecording = false;
    private int targetGestureCount = 5;
    private Vector2 scrollPosition;
    
    private const int RESAMPLE_POINT_COUNT = 64;
    
    public static void ShowWindow(SpellData spell)
    {
        targetSpell = spell;
        window = GetWindow<PatternTemplateGeneratorWindow>("Template Generator");
        window.minSize = new Vector2(400, 500);
        window.Show();
    }
    
    private void OnEnable()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }
    
    private void OnDisable()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
    }
    
    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            FindRecognizer();
        }
        else if (state == PlayModeStateChange.ExitingPlayMode)
        {
            recognizer = null;
        }
    }
    
    private void FindRecognizer()
    {
        recognizer = FindObjectOfType<GestureRecognizerNew>();
        
        if (recognizer == null)
        {
            Debug.LogWarning("GestureRecognizerNew not found in scene! Make sure your recognition system is set up.");
        }
    }
    
    private void OnGUI()
    {
        if (targetSpell == null)
        {
            EditorGUILayout.HelpBox("No target spell selected!", MessageType.Error);
            return;
        }
        
        EditorGUILayout.Space(10);
        
        EditorGUILayout.LabelField("Pattern Template Generator", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"Target Spell: {targetSpell.spellName}", EditorStyles.miniLabel);
        
        EditorGUILayout.Space(10);
        
        DrawInstructions();
        
        EditorGUILayout.Space(10);
        
        DrawSettings();
        
        EditorGUILayout.Space(10);
        
        DrawRecordingSection();
        
        EditorGUILayout.Space(10);
        
        DrawRecordedGestures();
        
        EditorGUILayout.Space(10);
        
        DrawGenerateButton();
    }
    
    private void DrawInstructions()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.LabelField("📋 Instructions", EditorStyles.boldLabel);
        
        EditorGUILayout.HelpBox(
            "1. Click 'Enter Play Mode' below\n" +
            "2. Draw the gesture multiple times on the rune pad\n" +
            "3. Each successful gesture will be recorded automatically\n" +
            "4. Once you have enough samples, click 'Generate Template'\n" +
            "5. Exit Play Mode - your template will be saved!",
            MessageType.Info
        );
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawSettings()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.LabelField("⚙ Settings", EditorStyles.boldLabel);
        
        targetGestureCount = EditorGUILayout.IntSlider(
            new GUIContent("Target Gesture Count", "How many gesture samples to collect"),
            targetGestureCount,
            3, 15
        );
        
        if (targetGestureCount < 5)
        {
            EditorGUILayout.HelpBox("⚠ Recommended: At least 5 gestures for quality template", MessageType.Warning);
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawRecordingSection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.LabelField("🎮 Recording", EditorStyles.boldLabel);
        
        if (!EditorApplication.isPlaying)
        {
            EditorGUILayout.HelpBox("Enter Play Mode to start recording gestures", MessageType.Warning);
            
            if (GUILayout.Button("▶ Enter Play Mode", GUILayout.Height(30)))
            {
                EditorApplication.isPlaying = true;
            }
        }
        else
        {
            if (recognizer == null)
            {
                EditorGUILayout.HelpBox("❌ GestureRecognizerNew not found in scene!", MessageType.Error);
                
                if (GUILayout.Button("🔍 Try Find Recognizer Again"))
                {
                    FindRecognizer();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("✅ Ready to record! Draw gestures in the Game view.", MessageType.Info);
                
                EditorGUILayout.BeginHorizontal();
                
                GUI.enabled = !isRecording;
                if (GUILayout.Button("🔴 Start Auto-Recording", GUILayout.Height(25)))
                {
                    StartRecording();
                }
                GUI.enabled = true;
                
                GUI.enabled = isRecording;
                if (GUILayout.Button("⏹ Stop Recording", GUILayout.Height(25)))
                {
                    StopRecording();
                }
                GUI.enabled = true;
                
                EditorGUILayout.EndHorizontal();
                
                if (isRecording)
                {
                    EditorGUILayout.HelpBox($"🎙 RECORDING... Draw {targetSpell.spellName} gesture now!", MessageType.None);
                }
            }
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawRecordedGestures()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.LabelField($"📊 Recorded Gestures: {recordedGestures.Count} / {targetGestureCount}", EditorStyles.boldLabel);
        
        if (recordedGestures.Count == 0)
        {
            EditorGUILayout.HelpBox("No gestures recorded yet", MessageType.Info);
        }
        else
        {
            float progress = (float)recordedGestures.Count / targetGestureCount;
            Rect rect = GUILayoutUtility.GetRect(18, 18, GUILayout.ExpandWidth(true));
            EditorGUI.ProgressBar(rect, progress, $"{recordedGestures.Count}/{targetGestureCount}");
            
            EditorGUILayout.Space(5);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.MaxHeight(150));
            
            for (int i = 0; i < recordedGestures.Count; i++)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                
                EditorGUILayout.LabelField($"Sample {i + 1}", GUILayout.Width(70));
                EditorGUILayout.LabelField($"{recordedGestures[i].Count} points", GUILayout.Width(80));
                
                GUILayout.FlexibleSpace();
                
                if (GUILayout.Button("❌", GUILayout.Width(30)))
                {
                    recordedGestures.RemoveAt(i);
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.Space(5);
            
            if (GUILayout.Button("Clear All Samples"))
            {
                if (EditorUtility.DisplayDialog("Clear Samples?",
                    "Remove all recorded gesture samples?",
                    "Yes, Clear", "Cancel"))
                {
                    recordedGestures.Clear();
                }
            }
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawGenerateButton()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        GUI.enabled = recordedGestures.Count >= 3;
        
        if (recordedGestures.Count < 3)
        {
            EditorGUILayout.HelpBox("❌ Need at least 3 gesture samples to generate template", MessageType.Warning);
        }
        else if (recordedGestures.Count < targetGestureCount)
        {
            EditorGUILayout.HelpBox($"⚠ You have {recordedGestures.Count}/{targetGestureCount} samples. More samples = better template!", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox($"✅ Ready to generate! You have {recordedGestures.Count} quality samples.", MessageType.Info);
        }
        
        if (GUILayout.Button("✨ Generate Template from Recorded Gestures", GUILayout.Height(35)))
        {
            GenerateTemplate();
        }
        
        GUI.enabled = true;
        
        EditorGUILayout.EndVertical();
    }
    
    private void StartRecording()
    {
        isRecording = true;
        Debug.Log($"<color=cyan>🎙 Started auto-recording for '{targetSpell.spellName}'</color>");
    }
    
    private void StopRecording()
    {
        isRecording = false;
        Debug.Log($"<color=yellow>⏹ Stopped recording. Captured {recordedGestures.Count} gestures.</color>");
    }
    
    private void Update()
    {
        if (isRecording && EditorApplication.isPlaying && recognizer != null)
        {
            Repaint();
        }
    }
    
    public void OnGestureCompleted(List<Vector2> gesturePoints)
    {
        if (!isRecording) return;
        
        if (gesturePoints == null || gesturePoints.Count < 10)
        {
            Debug.LogWarning("Gesture too short to record");
            return;
        }
        
        List<Vector2> processed = PreprocessGesture(gesturePoints);
        recordedGestures.Add(processed);
        
        Debug.Log($"<color=green>✓ Recorded gesture {recordedGestures.Count}/{targetGestureCount} ({processed.Count} points)</color>");
        
        if (recordedGestures.Count >= targetGestureCount)
        {
            StopRecording();
            Debug.Log($"<color=cyan>🎉 Target reached! {recordedGestures.Count} gestures recorded. Ready to generate template!</color>");
        }
    }
    
    private List<Vector2> PreprocessGesture(List<Vector2> points)
    {
        List<Vector2> resampled = Resample(points, RESAMPLE_POINT_COUNT);
        List<Vector2> normalized = Normalize(resampled);
        return normalized;
    }
    
    private List<Vector2> Resample(List<Vector2> points, int targetCount)
    {
        if (points.Count < 2) return new List<Vector2>(points);
        
        float pathLength = 0f;
        for (int i = 0; i < points.Count - 1; i++)
        {
            pathLength += Vector2.Distance(points[i], points[i + 1]);
        }
        
        float intervalLength = pathLength / (targetCount - 1);
        List<Vector2> resampled = new List<Vector2> { points[0] };
        float distance = 0f;
        
        for (int i = 1; i < points.Count; i++)
        {
            float segmentLength = Vector2.Distance(points[i - 1], points[i]);
            
            if (distance + segmentLength >= intervalLength)
            {
                Vector2 prev = points[i - 1];
                
                while (distance + segmentLength >= intervalLength && resampled.Count < targetCount)
                {
                    float t = (intervalLength - distance) / segmentLength;
                    Vector2 newPoint = Vector2.Lerp(prev, points[i], t);
                    resampled.Add(newPoint);
                    
                    segmentLength = Vector2.Distance(newPoint, points[i]);
                    prev = newPoint;
                    distance = 0f;
                }
                distance = segmentLength;
            }
            else
            {
                distance += segmentLength;
            }
        }
        
        if (resampled.Count < targetCount)
        {
            resampled.Add(points[points.Count - 1]);
        }
        
        return resampled;
    }
    
    private List<Vector2> Normalize(List<Vector2> points)
    {
        if (points.Count == 0) return points;
        
        Vector2 centroid = Vector2.zero;
        foreach (var p in points) centroid += p;
        centroid /= points.Count;
        
        List<Vector2> translated = new List<Vector2>();
        foreach (var p in points) translated.Add(p - centroid);
        
        float maxDist = 0f;
        foreach (var p in translated)
        {
            float dist = p.magnitude;
            if (dist > maxDist) maxDist = dist;
        }
        
        float scale = 100f / Mathf.Max(maxDist, 0.0001f);
        
        List<Vector2> normalized = new List<Vector2>();
        foreach (var p in translated) normalized.Add(p * scale);
        
        return normalized;
    }
    
    private void GenerateTemplate()
    {
        if (recordedGestures.Count < 3)
        {
            EditorUtility.DisplayDialog("Not Enough Samples",
                "Need at least 3 gesture samples to generate a template.",
                "OK");
            return;
        }
        
        List<Vector2> averageTemplate = ComputeAverageGesture(recordedGestures);
        
        targetSpell.gestureTemplate = averageTemplate;
        EditorUtility.SetDirty(targetSpell);
        AssetDatabase.SaveAssets();
        
        Debug.Log($"<color=green>✅ Successfully generated template for '{targetSpell.spellName}'!</color>\n" +
                  $"   Samples used: {recordedGestures.Count}\n" +
                  $"   Template points: {averageTemplate.Count}\n" +
                  $"   Template saved to asset.");
        
        EditorUtility.DisplayDialog("Template Generated!",
            $"Successfully created template for '{targetSpell.spellName}'\n\n" +
            $"Samples: {recordedGestures.Count}\n" +
            $"Points: {averageTemplate.Count}\n\n" +
            $"You can now exit Play Mode. The template is saved!",
            "Great!");
    }
    
    private List<Vector2> ComputeAverageGesture(List<List<Vector2>> gestures)
    {
        if (gestures.Count == 0) return new List<Vector2>();
        
        List<List<Vector2>> alignedGestures = AlignGestures(gestures);
        
        int pointCount = RESAMPLE_POINT_COUNT;
        List<Vector2> average = new List<Vector2>(pointCount);
        
        for (int i = 0; i < pointCount; i++)
        {
            List<Vector2> pointsAtIndex = new List<Vector2>();
            
            foreach (var gesture in alignedGestures)
            {
                if (i < gesture.Count)
                {
                    pointsAtIndex.Add(gesture[i]);
                }
            }
            
            if (pointsAtIndex.Count > 0)
            {
                Vector2 median = CalculateMedianPoint(pointsAtIndex);
                average.Add(median);
            }
        }
        
        List<Vector2> smoothed = SmoothGesture(average);
        return Normalize(smoothed);
    }
    
    private List<List<Vector2>> AlignGestures(List<List<Vector2>> gestures)
    {
        if (gestures.Count <= 1) return gestures;
        
        List<Vector2> referenceTemplate = gestures[0];
        List<List<Vector2>> aligned = new List<List<Vector2>> { referenceTemplate };
        
        for (int i = 1; i < gestures.Count; i++)
        {
            List<Vector2> bestAligned = gestures[i];
            float bestScore = float.MaxValue;
            
            for (int shift = 0; shift < gestures[i].Count / 4; shift++)
            {
                List<Vector2> shifted = ShiftGesture(gestures[i], shift);
                float score = CalculateGestureDistance(referenceTemplate, shifted);
                
                if (score < bestScore)
                {
                    bestScore = score;
                    bestAligned = shifted;
                }
            }
            
            aligned.Add(bestAligned);
        }
        
        return aligned;
    }
    
    private List<Vector2> ShiftGesture(List<Vector2> gesture, int shiftAmount)
    {
        if (shiftAmount == 0 || gesture.Count == 0) return gesture;
        
        List<Vector2> shifted = new List<Vector2>(gesture.Count);
        
        for (int i = 0; i < gesture.Count; i++)
        {
            int newIndex = (i + shiftAmount) % gesture.Count;
            shifted.Add(gesture[newIndex]);
        }
        
        return shifted;
    }
    
    private float CalculateGestureDistance(List<Vector2> g1, List<Vector2> g2)
    {
        if (g1.Count != g2.Count) return float.MaxValue;
        
        float totalDist = 0f;
        for (int i = 0; i < g1.Count; i++)
        {
            totalDist += Vector2.Distance(g1[i], g2[i]);
        }
        
        return totalDist / g1.Count;
    }
    
    private Vector2 CalculateMedianPoint(List<Vector2> points)
    {
        if (points.Count == 0) return Vector2.zero;
        if (points.Count == 1) return points[0];
        
        Vector2 centroid = Vector2.zero;
        foreach (var p in points) centroid += p;
        centroid /= points.Count;
        
        List<Vector2> sortedByDistToCentroid = points.OrderBy(p => Vector2.Distance(p, centroid)).ToList();
        
        int medianIndex = sortedByDistToCentroid.Count / 2;
        return sortedByDistToCentroid[medianIndex];
    }
    
    private List<Vector2> SmoothGesture(List<Vector2> points)
    {
        if (points.Count < 3) return points;
        
        List<Vector2> smoothed = new List<Vector2>(points.Count) { points[0] };
        
        for (int i = 1; i < points.Count - 1; i++)
        {
            Vector2 avg = (points[i - 1] + points[i] * 2f + points[i + 1]) * 0.25f;
            smoothed.Add(avg);
        }
        
        smoothed.Add(points[points.Count - 1]);
        
        return smoothed;
    }
}
