using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(SpellData))]
public class SpellDataEditor : Editor
{
    private const float EDGE_DETECTION_THRESHOLD = 0.5f;
    private const int TEMPLATE_POINT_COUNT = 64;
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        SpellData spellData = (SpellData)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Pattern Template Generator", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.HelpBox(
            "🎨 RECOMMENDED: Record perfect gesture in Play Mode\n" +
            "This creates high-quality templates by averaging your actual drawings.",
            MessageType.Info
        );
        
        if (GUILayout.Button("🎮 Open Live Gesture Recorder (Play Mode)", GUILayout.Height(35)))
        {
            PatternTemplateGeneratorWindow.ShowWindow(spellData);
        }
        
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(5);
        
        if (spellData.gesturePatternIcon != null)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.HelpBox("📐 ALTERNATIVE: Generate from Pattern Icon\nLess accurate but faster.", MessageType.Info);
            
            if (GUILayout.Button("Generate Template from Pattern Icon", GUILayout.Height(30)))
            {
                GenerateTemplateFromSprite(spellData);
            }
            
            EditorGUILayout.EndVertical();
        }
        
        EditorGUILayout.Space();
        
        if (spellData.gestureTemplate != null && spellData.gestureTemplate.Count > 0)
        {
            EditorGUILayout.LabelField($"Template Points: {spellData.gestureTemplate.Count}", EditorStyles.miniLabel);
            
            EditorGUILayout.Space(5);
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("🎯 Tolerance Calibration Guide", EditorStyles.boldLabel);
            
            float tolerance = spellData.recognitionTolerance;
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Current Tolerance:", GUILayout.Width(120));
            EditorGUILayout.LabelField($"{tolerance:F2}", EditorStyles.boldLabel, GUILayout.Width(40));
            
            if (tolerance < 0.25f)
            {
                EditorGUILayout.LabelField("🔴 VERY STRICT", GUILayout.Width(120));
            }
            else if (tolerance < 0.35f)
            {
                EditorGUILayout.LabelField("🟡 STRICT", GUILayout.Width(120));
            }
            else if (tolerance <= 0.50f)
            {
                EditorGUILayout.LabelField("🟢 RECOMMENDED", GUILayout.Width(120));
            }
            else
            {
                EditorGUILayout.LabelField("🔴 TOO LENIENT", GUILayout.Width(120));
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(3);
            
            EditorGUILayout.HelpBox(
                "Quick Calibration Steps:\n" +
                "1. Set to 0.85 temporarily (very lenient)\n" +
                "2. Enter Play Mode and test drawing\n" +
                "3. Check Console for actual score (e.g., 0.32)\n" +
                "4. Set tolerance to: score + 0.10\n" +
                "   Example: If score is 0.32 → set to 0.42",
                MessageType.Info
            );
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Set to 0.85\n(Testing)", GUILayout.Height(35)))
            {
                Undo.RecordObject(spellData, "Set Testing Tolerance");
                spellData.recognitionTolerance = 0.85f;
                EditorUtility.SetDirty(spellData);
                Debug.Log($"<color=yellow>Set {spellData.spellName} tolerance to 0.85 for testing</color>");
            }
            
            if (GUILayout.Button("Set to 0.40\n(Recommended)", GUILayout.Height(35)))
            {
                Undo.RecordObject(spellData, "Set Recommended Tolerance");
                spellData.recognitionTolerance = 0.40f;
                EditorUtility.SetDirty(spellData);
                Debug.Log($"<color=green>Set {spellData.spellName} tolerance to 0.40 (War of Wizards level)</color>");
            }
            
            if (GUILayout.Button("Set to 0.30\n(Strict)", GUILayout.Height(35)))
            {
                Undo.RecordObject(spellData, "Set Strict Tolerance");
                spellData.recognitionTolerance = 0.30f;
                EditorUtility.SetDirty(spellData);
                Debug.Log($"<color=orange>Set {spellData.spellName} tolerance to 0.30 (strict)</color>");
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(5);
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("⚙ Constraint Settings", EditorStyles.boldLabel);
            
            bool enforceSpeed = spellData.enforceSpeed;
            bool enforceDirection = spellData.enforceDirection;
            
            if (enforceSpeed || enforceDirection)
            {
                EditorGUILayout.HelpBox("⚠ Active constraints can cause false negatives!", MessageType.Warning);
            }
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Enforce Speed: {(enforceSpeed ? "✓ ON" : "✗ OFF")}", GUILayout.Width(150));
            EditorGUILayout.LabelField($"Enforce Direction: {(enforceDirection ? "✓ ON" : "✗ OFF")}", GUILayout.Width(150));
            EditorGUILayout.EndHorizontal();
            
            if (enforceSpeed || enforceDirection)
            {
                if (GUILayout.Button("🔓 Disable All Constraints (Recommended for Testing)", GUILayout.Height(30)))
                {
                    Undo.RecordObject(spellData, "Disable Constraints");
                    spellData.enforceSpeed = false;
                    spellData.enforceDirection = false;
                    EditorUtility.SetDirty(spellData);
                    Debug.Log($"<color=green>Disabled all constraints for {spellData.spellName}</color>");
                }
            }
            else
            {
                EditorGUILayout.HelpBox("✓ No constraints enabled - pure shape matching active", MessageType.Info);
            }
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(5);
            
            if (GUILayout.Button("Clear Template"))
            {
                if (EditorUtility.DisplayDialog("Clear Template", 
                    "Are you sure you want to clear the gesture template?", 
                    "Yes", "Cancel"))
                {
                    Undo.RecordObject(spellData, "Clear Template");
                    spellData.gestureTemplate.Clear();
                    EditorUtility.SetDirty(spellData);
                    Debug.Log($"Cleared template for {spellData.spellName}");
                }
            }
        }
    }
    
    private void GenerateTemplateFromSprite(SpellData spellData)
    {
        Sprite sprite = spellData.gesturePatternIcon;
        
        if (sprite == null)
        {
            Debug.LogError("No pattern icon assigned!");
            return;
        }
        
        Texture2D texture = GetReadableTexture(sprite);
        
        if (texture == null)
        {
            Debug.LogError("Could not read texture from sprite! Make sure the sprite's texture is set to Read/Write enabled in import settings.");
            return;
        }
        
        List<Vector2> edgePoints = ExtractEdgePoints(texture, sprite);
        
        if (edgePoints.Count == 0)
        {
            Debug.LogError("No edge points detected in the pattern image! Make sure your pattern has visible contrast.");
            return;
        }
        
        List<Vector2> orderedPoints = OrderPointsAlongPath(edgePoints);
        List<Vector2> resampledPoints = ResamplePoints(orderedPoints, TEMPLATE_POINT_COUNT);
        List<Vector2> normalizedPoints = NormalizeTemplate(resampledPoints);
        
        spellData.gestureTemplate = normalizedPoints;
        EditorUtility.SetDirty(spellData);
        
        Debug.Log($"<color=green>✓ Successfully generated template from pattern icon!</color>\n" +
                  $"Extracted {edgePoints.Count} edge points → Normalized to {normalizedPoints.Count} template points\n" +
                  $"Spell: {spellData.spellName}");
    }
    
    private Texture2D GetReadableTexture(Sprite sprite)
    {
        Texture2D originalTexture = sprite.texture;
        string assetPath = AssetDatabase.GetAssetPath(originalTexture);
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        
        if (importer != null && !importer.isReadable)
        {
            importer.isReadable = true;
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        }
        
        Rect spriteRect = sprite.rect;
        Texture2D croppedTexture = new Texture2D((int)spriteRect.width, (int)spriteRect.height);
        
        Color[] pixels = originalTexture.GetPixels(
            (int)spriteRect.x,
            (int)spriteRect.y,
            (int)spriteRect.width,
            (int)spriteRect.height
        );
        
        croppedTexture.SetPixels(pixels);
        croppedTexture.Apply();
        
        return croppedTexture;
    }
    
    private List<Vector2> ExtractEdgePoints(Texture2D texture, Sprite sprite)
    {
        List<Vector2> edgePoints = new List<Vector2>();
        int width = texture.width;
        int height = texture.height;
        
        for (int y = 1; y < height - 1; y++)
        {
            for (int x = 1; x < width - 1; x++)
            {
                float currentAlpha = texture.GetPixel(x, y).a;
                
                if (currentAlpha > EDGE_DETECTION_THRESHOLD)
                {
                    float left = texture.GetPixel(x - 1, y).a;
                    float right = texture.GetPixel(x + 1, y).a;
                    float top = texture.GetPixel(x, y + 1).a;
                    float bottom = texture.GetPixel(x, y - 1).a;
                    
                    bool isEdge = left < EDGE_DETECTION_THRESHOLD || 
                                  right < EDGE_DETECTION_THRESHOLD || 
                                  top < EDGE_DETECTION_THRESHOLD || 
                                  bottom < EDGE_DETECTION_THRESHOLD;
                    
                    if (isEdge)
                    {
                        edgePoints.Add(new Vector2(x, y));
                    }
                }
            }
        }
        
        return edgePoints;
    }
    
    private List<Vector2> OrderPointsAlongPath(List<Vector2> points)
    {
        if (points.Count == 0) return points;
        
        List<Vector2> ordered = new List<Vector2>();
        List<Vector2> remaining = new List<Vector2>(points);
        
        Vector2 current = remaining[0];
        ordered.Add(current);
        remaining.RemoveAt(0);
        
        while (remaining.Count > 0)
        {
            int nearestIndex = 0;
            float nearestDistance = Vector2.Distance(current, remaining[0]);
            
            for (int i = 1; i < remaining.Count; i++)
            {
                float distance = Vector2.Distance(current, remaining[i]);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestIndex = i;
                }
            }
            
            current = remaining[nearestIndex];
            ordered.Add(current);
            remaining.RemoveAt(nearestIndex);
        }
        
        return ordered;
    }
    
    private List<Vector2> ResamplePoints(List<Vector2> points, int targetCount)
    {
        if (points.Count < 2) return points;
        
        float totalLength = 0f;
        for (int i = 0; i < points.Count - 1; i++)
        {
            totalLength += Vector2.Distance(points[i], points[i + 1]);
        }
        
        float intervalLength = totalLength / (targetCount - 1);
        List<Vector2> resampled = new List<Vector2> { points[0] };
        
        float accumulatedDistance = 0f;
        
        for (int i = 1; i < points.Count; i++)
        {
            float segmentLength = Vector2.Distance(points[i - 1], points[i]);
            
            if (accumulatedDistance + segmentLength >= intervalLength)
            {
                while (accumulatedDistance + segmentLength >= intervalLength)
                {
                    float remainingDistance = intervalLength - accumulatedDistance;
                    float ratio = remainingDistance / segmentLength;
                    Vector2 newPoint = Vector2.Lerp(points[i - 1], points[i], ratio);
                    resampled.Add(newPoint);
                    
                    segmentLength -= remainingDistance;
                    accumulatedDistance = 0f;
                    points[i - 1] = newPoint;
                }
                accumulatedDistance = segmentLength;
            }
            else
            {
                accumulatedDistance += segmentLength;
            }
            
            if (resampled.Count >= targetCount)
                break;
        }
        
        if (resampled.Count < targetCount)
        {
            resampled.Add(points[points.Count - 1]);
        }
        
        return resampled;
    }
    
    private List<Vector2> NormalizeTemplate(List<Vector2> points)
    {
        if (points.Count == 0) return points;
        
        Vector2 centroid = Vector2.zero;
        foreach (Vector2 point in points)
        {
            centroid += point;
        }
        centroid /= points.Count;
        
        List<Vector2> centered = new List<Vector2>();
        foreach (Vector2 point in points)
        {
            centered.Add(point - centroid);
        }
        
        float maxDistance = 0f;
        foreach (Vector2 point in centered)
        {
            float distance = point.magnitude;
            if (distance > maxDistance)
            {
                maxDistance = distance;
            }
        }
        
        float scaleFactor = 100f / maxDistance;
        
        List<Vector2> normalized = new List<Vector2>();
        foreach (Vector2 point in centered)
        {
            normalized.Add(point * scaleFactor);
        }
        
        return normalized;
    }
}
