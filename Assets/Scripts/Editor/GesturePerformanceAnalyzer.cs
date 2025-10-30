using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Diagnostics;

public class GesturePerformanceAnalyzer : EditorWindow
{
    private GestureRecognizerNew recognizer;
    private List<SpellData> testSpells;
    private List<Vector2> testGesture;
    private int iterations = 100;
    private bool isAnalyzing = false;
    private string results = "";
    private Vector2 scrollPos;
    
    [MenuItem("Tools/Gesture Performance Analyzer")]
    public static void ShowWindow()
    {
        GetWindow<GesturePerformanceAnalyzer>("Performance Analyzer");
    }
    
    private void OnGUI()
    {
        EditorGUILayout.LabelField("Gesture Recognition Performance Analyzer", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);
        
        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Enter Play Mode to run performance analysis", MessageType.Info);
            
            if (GUILayout.Button("Enter Play Mode", GUILayout.Height(30)))
            {
                EditorApplication.isPlaying = true;
            }
            return;
        }
        
        if (recognizer == null)
        {
            recognizer = FindObjectOfType<GestureRecognizerNew>();
        }
        
        if (recognizer == null)
        {
            EditorGUILayout.HelpBox("GestureRecognizerNew not found in scene!", MessageType.Error);
            return;
        }
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
        iterations = EditorGUILayout.IntSlider("Test Iterations", iterations, 10, 1000);
        
        EditorGUILayout.Space(5);
        
        GUI.enabled = !isAnalyzing;
        
        if (GUILayout.Button("Run Performance Test", GUILayout.Height(35)))
        {
            RunPerformanceTest();
        }
        
        if (GUILayout.Button("Generate Test Gesture"))
        {
            GenerateTestGesture();
        }
        
        GUI.enabled = true;
        
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        if (!string.IsNullOrEmpty(results))
        {
            EditorGUILayout.LabelField("Results", EditorStyles.boldLabel);
            
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, EditorStyles.helpBox);
            EditorGUILayout.TextArea(results, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }
    }
    
    private void GenerateTestGesture()
    {
        testGesture = new List<Vector2>();
        
        int points = 50;
        float radius = 100f;
        
        for (int i = 0; i < points; i++)
        {
            float angle = (i / (float)points) * Mathf.PI * 2f;
            testGesture.Add(new Vector2(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius
            ));
        }
        
        UnityEngine.Debug.Log($"Generated test gesture with {testGesture.Count} points");
    }
    
    private void RunPerformanceTest()
    {
        if (testGesture == null || testGesture.Count == 0)
        {
            GenerateTestGesture();
        }
        
        results = "Running performance test...\n\n";
        Repaint();
        
        isAnalyzing = true;
        
        Stopwatch sw = new Stopwatch();
        List<long> timings = new List<long>();
        
        List<Vector3> points3D = new List<Vector3>();
        foreach (var p in testGesture)
        {
            points3D.Add(new Vector3(p.x, p.y, 0));
        }
        
        for (int i = 0; i < iterations; i++)
        {
            sw.Restart();
            recognizer.RecognizeGesture(points3D, 1.0f);
            sw.Stop();
            
            timings.Add(sw.ElapsedTicks);
        }
        
        timings.Sort();
        
        long totalTicks = 0;
        foreach (long t in timings) totalTicks += t;
        
        double avgMs = (totalTicks / (double)iterations) / (Stopwatch.Frequency / 1000.0);
        double minMs = timings[0] / (Stopwatch.Frequency / 1000.0);
        double maxMs = timings[timings.Count - 1] / (Stopwatch.Frequency / 1000.0);
        double medianMs = timings[timings.Count / 2] / (Stopwatch.Frequency / 1000.0);
        double p95Ms = timings[(int)(timings.Count * 0.95)] / (Stopwatch.Frequency / 1000.0);
        
        results = "=== PERFORMANCE TEST RESULTS ===\n\n";
        results += $"Iterations: {iterations}\n";
        results += $"Test Gesture Points: {testGesture.Count}\n\n";
        
        results += "--- Timings ---\n";
        results += $"Average: {avgMs:F3} ms\n";
        results += $"Median:  {medianMs:F3} ms\n";
        results += $"Min:     {minMs:F3} ms\n";
        results += $"Max:     {maxMs:F3} ms\n";
        results += $"95th %:  {p95Ms:F3} ms\n\n";
        
        results += "--- Frame Budget ---\n";
        results += $"60 FPS (16.67ms): {(avgMs < 16.67 ? "✓ PASS" : "✗ FAIL")}\n";
        results += $"30 FPS (33.33ms): {(avgMs < 33.33 ? "✓ PASS" : "✗ FAIL")}\n\n";
        
        results += "--- Recognition Rate ---\n";
        results += $"Gestures/sec: {1000.0 / avgMs:F1}\n\n";
        
        results += "--- Memory ---\n";
        results += "Check Profiler for GC allocations\n";
        results += "(Should be 0 with optimized code)\n\n";
        
        results += "=== RECOMMENDATIONS ===\n\n";
        
        if (avgMs < 1.0)
        {
            results += "✓ Excellent performance!\n";
            results += "  Recognition is very fast.\n";
        }
        else if (avgMs < 2.0)
        {
            results += "✓ Good performance\n";
            results += "  Suitable for real-time gameplay.\n";
        }
        else if (avgMs < 5.0)
        {
            results += "⚠ Acceptable performance\n";
            results += "  Consider reducing point count or features.\n";
        }
        else
        {
            results += "✗ Poor performance\n";
            results += "  Optimization needed:\n";
            results += "  - Reduce resample point count\n";
            results += "  - Disable start point invariance\n";
            results += "  - Check for excessive spell templates\n";
        }
        
        isAnalyzing = false;
        Repaint();
        
        UnityEngine.Debug.Log("Performance test complete!");
    }
}
