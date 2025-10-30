using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GestureRecognizer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private List<SpellData> availableSpells = new List<SpellData>();
    [SerializeField] private SpellCaster spellCaster;
    [SerializeField] private PlayerUIController playerUIController;
    
    [Header("Recognition Settings")]
    [SerializeField] private int resamplePointCount = 64;
    [SerializeField] private float normalizedSquareSize = 250f;
    
    [Header("Advanced Recognition")]
    [Tooltip("Combines multiple recognition algorithms for better accuracy")]
    [SerializeField] private bool useAdvancedRecognition = true;
    
    [Tooltip("Weight for path distance matching (0-1)")]
    [SerializeField] private float pathDistanceWeight = 0.35f;
    
    [Tooltip("Weight for shape features matching (0-1)")]
    [SerializeField] private float shapeFeaturesWeight = 0.35f;
    
    [Tooltip("Weight for directional matching (0-1)")]
    [SerializeField] private float directionalWeight = 0.3f;
    
    [Tooltip("Maximum score threshold to accept a spell match")]
    [SerializeField] private float matchingThreshold = 0.40f;
    
    [Tooltip("Minimum confidence difference between best and second-best match")]
    [SerializeField] private float minimumConfidenceGap = 0.08f;
    
    private const float DirectionThreshold = 30f;
    
    public GestureRecognitionResult RecognizeGesture(List<Vector3> drawnPoints, float totalDrawingTime)
    {
        if (drawnPoints == null || drawnPoints.Count < 2)
        {
            return new GestureRecognitionResult 
            { 
                success = false, 
                message = "Invalid gesture: Too few points" 
            };
        }
        
        if (availableSpells == null || availableSpells.Count == 0)
        {
            Debug.LogWarning("GestureRecognizer: No spells are assigned in availableSpells list!");
            return new GestureRecognitionResult 
            { 
                success = false, 
                message = "No spells configured in recognizer" 
            };
        }
        
        List<Vector2> points2D = drawnPoints.Select(p => new Vector2(p.x, p.y)).ToList();
        
        float pathLength = CalculatePathLength(points2D);
        float drawSpeed = totalDrawingTime > 0 ? pathLength / totalDrawingTime : 0f;
        GestureDirection drawDirection = CalculateDrawDirection(points2D);
        
        Debug.Log($"<color=cyan>Analyzing gesture: Speed={drawSpeed:F2}, Direction={drawDirection}, PathLength={pathLength:F2}, Points={drawnPoints.Count}</color>");
        
        List<Vector2> processedGesture = PreprocessGesture(points2D, false);
        GestureFeatures drawnFeatures = ExtractGestureFeatures(processedGesture);
        
        Debug.Log($"<color=yellow>Drawn Gesture Features:</color>\n" +
                 $"  Aspect Ratio: {drawnFeatures.aspectRatio:F2} | Closedness: {drawnFeatures.closedness:F2}\n" +
                 $"  Total Curvature: {drawnFeatures.totalCurvature:F1}° | Dir Changes: {drawnFeatures.directionChanges}\n" +
                 $"  Start Angle: {drawnFeatures.startAngle:F1}° | End Angle: {drawnFeatures.endAngle:F1}°");
        
        List<SpellMatchResult> matchResults = new List<SpellMatchResult>();
        int validSpellsChecked = 0;
        
        foreach (SpellData spell in availableSpells)
        {
            if (spell == null)
            {
                Debug.LogWarning("GestureRecognizer: Null spell found in availableSpells list!");
                continue;
            }
            
            if (spell.gestureTemplate == null || spell.gestureTemplate.Count == 0)
            {
                Debug.LogWarning($"GestureRecognizer: Spell '{spell.spellName}' has no template! Generate one using the custom inspector.");
                continue;
            }
            
            validSpellsChecked++;
            
            List<Vector2> processedTemplate = PreprocessGesture(spell.gestureTemplate, spell.allowRotation);
            
            float matchScore = useAdvancedRecognition 
                ? CalculateAdvancedMatchScore(processedGesture, processedTemplate, drawnFeatures, spell, drawSpeed, drawDirection)
                : CalculatePathDistance(processedGesture, processedTemplate);
            
            matchResults.Add(new SpellMatchResult
            {
                spell = spell,
                score = matchScore,
                passed = matchScore <= matchingThreshold
            });
            
            if (useAdvancedRecognition)
            {
                float pathScore = CalculatePathDistance(processedGesture, processedTemplate);
                GestureFeatures templateFeatures = ExtractGestureFeatures(processedTemplate);
                float shapeScore = CalculateShapeFeatureDistance(drawnFeatures, templateFeatures);
                float directionScore = CalculateDirectionalSimilarity(processedGesture, processedTemplate);
                
                Debug.Log($"  <b>[{spell.spellName}]</b> Final: {matchScore:F3} {(matchScore <= matchingThreshold ? "<color=green>✓ PASS</color>" : "<color=red>✗ FAIL</color>")}\n" +
                         $"    Components: Path={pathScore:F3} Shape={shapeScore:F3} Dir={directionScore:F3}\n" +
                         $"    Template: AR={templateFeatures.aspectRatio:F2} Close={templateFeatures.closedness:F2} Curv={templateFeatures.totalCurvature:F0}° DirCh={templateFeatures.directionChanges}");
            }
            else
            {
                Debug.Log($"  [{spell.spellName}] Score: {matchScore:F3} {(matchScore <= matchingThreshold ? "✓ PASS" : "✗ FAIL")}");
            }
        }
        
        if (validSpellsChecked == 0)
        {
            Debug.LogError("GestureRecognizer: No valid spells with templates found! Please create SpellData assets and generate templates.");
            return new GestureRecognitionResult
            {
                success = false,
                message = "No valid spell templates configured"
            };
        }
        
        matchResults = matchResults.OrderBy(r => r.score).ToList();
        
        SpellMatchResult bestMatch = matchResults[0];
        
        if (bestMatch.passed)
        {
            bool hasAmbiguousMatch = false;
            
            if (matchResults.Count > 1)
            {
                SpellMatchResult secondBest = matchResults[1];
                float confidenceGap = secondBest.score - bestMatch.score;
                
                if (confidenceGap < minimumConfidenceGap)
                {
                    hasAmbiguousMatch = true;
                    Debug.LogWarning($"<color=yellow>Ambiguous match! {bestMatch.spell.spellName} ({bestMatch.score:F3}) vs {secondBest.spell.spellName} ({secondBest.score:F3}). Gap: {confidenceGap:F3}</color>");
                }
            }
            
            float confidence = 1f - Mathf.Clamp01(bestMatch.score / matchingThreshold);
            
            if (playerUIController != null)
            {
                playerUIController.DisplayGestureHighlight(null);
            }
            
            Debug.Log($"<color=green>✓ RECOGNIZED: {bestMatch.spell.spellName} | Score: {bestMatch.score:F3} | Confidence: {confidence:P0} {(hasAmbiguousMatch ? "(Low certainty)" : "(High certainty)")}</color>");
            
            return new GestureRecognitionResult
            {
                success = true,
                recognizedSpell = bestMatch.spell,
                confidence = confidence,
                message = $"Recognized: {bestMatch.spell.spellName} ({confidence:P0})",
                drawSpeed = drawSpeed,
                drawDirection = drawDirection
            };
        }
        
        if (playerUIController != null)
        {
            playerUIController.ShowUnrecognizedGestureFeedback();
        }
        
        Debug.LogWarning($"<color=yellow>No match found. Best was '{bestMatch.spell.spellName}' with score {bestMatch.score:F3} (threshold: {matchingThreshold:F3})</color>");
        
        return new GestureRecognitionResult
        {
            success = false,
            confidence = 0f,
            message = $"No matching spell found. Closest: {bestMatch.spell.spellName}",
            drawSpeed = drawSpeed,
            drawDirection = drawDirection
        };
    }
    
    private float CalculateAdvancedMatchScore(List<Vector2> gesture, List<Vector2> template, GestureFeatures drawnFeatures, SpellData spell, float drawSpeed, GestureDirection drawDirection)
    {
        float pathScore = CalculatePathDistance(gesture, template);
        
        GestureFeatures templateFeatures = ExtractGestureFeatures(template);
        float shapeScore = CalculateShapeFeatureDistance(drawnFeatures, templateFeatures);
        
        float directionScore = CalculateDirectionalSimilarity(gesture, template);
        
        float combinedScore = (pathScore * pathDistanceWeight) + 
                             (shapeScore * shapeFeaturesWeight) + 
                             (directionScore * directionalWeight);
        
        float constraintPenalty = 0f;
        
        if (spell.enforceSpeed && (drawSpeed < spell.expectedSpeedRange.x || drawSpeed > spell.expectedSpeedRange.y))
        {
            float speedDeviation = drawSpeed < spell.expectedSpeedRange.x 
                ? (spell.expectedSpeedRange.x - drawSpeed) / spell.expectedSpeedRange.x
                : (drawSpeed - spell.expectedSpeedRange.y) / spell.expectedSpeedRange.y;
            constraintPenalty += speedDeviation * 0.2f;
        }
        
        if (spell.enforceDirection && spell.expectedDirection != GestureDirection.None && drawDirection != spell.expectedDirection)
        {
            constraintPenalty += 0.3f;
        }
        
        return Mathf.Clamp01(combinedScore + constraintPenalty);
    }
    
    private GestureFeatures ExtractGestureFeatures(List<Vector2> points)
    {
        if (points.Count < 2) return new GestureFeatures();
        
        Rect bounds = CalculateBoundingBox(points);
        float aspectRatio = bounds.height > 0 ? bounds.width / bounds.height : 1f;
        
        float totalAngle = 0f;
        List<float> angles = new List<float>();
        
        for (int i = 1; i < points.Count - 1; i++)
        {
            Vector2 v1 = (points[i] - points[i - 1]).normalized;
            Vector2 v2 = (points[i + 1] - points[i]).normalized;
            float angle = Vector2.SignedAngle(v1, v2);
            angles.Add(angle);
            totalAngle += Mathf.Abs(angle);
        }
        
        float totalCurvature = totalAngle;
        float avgCurvature = angles.Count > 0 ? totalAngle / angles.Count : 0f;
        
        int directionChanges = 0;
        for (int i = 1; i < angles.Count; i++)
        {
            if (Mathf.Sign(angles[i]) != Mathf.Sign(angles[i - 1]))
            {
                directionChanges++;
            }
        }
        
        float startToEndDistance = Vector2.Distance(points[0], points[points.Count - 1]);
        float pathLength = CalculatePathLength(points);
        float closedness = pathLength > 0 ? 1f - (startToEndDistance / pathLength) : 0f;
        
        Vector2 centroid = CalculateCentroid(points);
        float startAngle = Mathf.Atan2(points[0].y - centroid.y, points[0].x - centroid.x) * Mathf.Rad2Deg;
        float endAngle = Mathf.Atan2(points[points.Count - 1].y - centroid.y, points[points.Count - 1].x - centroid.x) * Mathf.Rad2Deg;
        
        return new GestureFeatures
        {
            aspectRatio = aspectRatio,
            totalCurvature = totalCurvature,
            averageCurvature = avgCurvature,
            directionChanges = directionChanges,
            closedness = closedness,
            startAngle = startAngle,
            endAngle = endAngle,
            boundingBoxArea = bounds.width * bounds.height
        };
    }
    
    private float CalculateShapeFeatureDistance(GestureFeatures f1, GestureFeatures f2)
    {
        float aspectRatioDiff = Mathf.Abs(f1.aspectRatio - f2.aspectRatio) / Mathf.Max(f1.aspectRatio, f2.aspectRatio);
        
        float curvatureDiff = Mathf.Abs(f1.totalCurvature - f2.totalCurvature) / Mathf.Max(f1.totalCurvature, f2.totalCurvature, 1f);
        
        float directionChangeDiff = Mathf.Abs(f1.directionChanges - f2.directionChanges) / Mathf.Max(f1.directionChanges, f2.directionChanges, 1f);
        
        float closednessDiff = Mathf.Abs(f1.closedness - f2.closedness);
        
        float angleStartDiff = Mathf.Abs(Mathf.DeltaAngle(f1.startAngle, f2.startAngle)) / 180f;
        float angleEndDiff = Mathf.Abs(Mathf.DeltaAngle(f1.endAngle, f2.endAngle)) / 180f;
        
        return (aspectRatioDiff * 0.2f + 
                curvatureDiff * 0.3f + 
                directionChangeDiff * 0.2f + 
                closednessDiff * 0.2f + 
                angleStartDiff * 0.05f + 
                angleEndDiff * 0.05f);
    }
    
    private float CalculateDirectionalSimilarity(List<Vector2> gesture, List<Vector2> template)
    {
        int n = Mathf.Min(gesture.Count, template.Count);
        if (n < 2) return 0f;
        
        float totalDifference = 0f;
        int comparisons = 0;
        
        for (int i = 1; i < n; i++)
        {
            Vector2 gestureDir = (gesture[i] - gesture[i - 1]).normalized;
            Vector2 templateDir = (template[i] - template[i - 1]).normalized;
            
            float dotProduct = Vector2.Dot(gestureDir, templateDir);
            float angleDifference = Mathf.Acos(Mathf.Clamp(dotProduct, -1f, 1f)) / Mathf.PI;
            
            totalDifference += angleDifference;
            comparisons++;
        }
        
        return comparisons > 0 ? totalDifference / comparisons : 1f;
    }
    
    private List<Vector2> PreprocessGesture(List<Vector2> points, bool normalizeRotation)
    {
        List<Vector2> processed = new List<Vector2>(points);
        
        processed = Resample(processed, resamplePointCount);
        
        if (normalizeRotation)
        {
            float angle = CalculateIndicativeAngle(processed);
            processed = RotateBy(processed, -angle);
        }
        
        processed = ScaleToSquare(processed, normalizedSquareSize);
        processed = TranslateToOrigin(processed);
        
        return processed;
    }
    
    private List<Vector2> Resample(List<Vector2> points, int n)
    {
        float pathLength = CalculatePathLength(points);
        float interval = pathLength / (n - 1);
        
        List<Vector2> resampled = new List<Vector2> { points[0] };
        float currentDist = 0f;
        
        for (int i = 1; i < points.Count; i++)
        {
            float segmentDist = Vector2.Distance(points[i - 1], points[i]);
            
            if (currentDist + segmentDist >= interval)
            {
                Vector2 direction = (points[i] - points[i - 1]).normalized;
                float neededDist = interval - currentDist;
                Vector2 newPoint = points[i - 1] + direction * neededDist;
                
                resampled.Add(newPoint);
                points.Insert(i, newPoint);
                currentDist = 0f;
            }
            else
            {
                currentDist += segmentDist;
            }
        }
        
        if (resampled.Count < n)
        {
            resampled.Add(points[points.Count - 1]);
        }
        
        while (resampled.Count > n)
        {
            resampled.RemoveAt(resampled.Count - 1);
        }
        
        return resampled;
    }
    
    private float CalculateIndicativeAngle(List<Vector2> points)
    {
        Vector2 centroid = CalculateCentroid(points);
        return Mathf.Atan2(centroid.y - points[0].y, centroid.x - points[0].x);
    }
    
    private List<Vector2> RotateBy(List<Vector2> points, float angle)
    {
        Vector2 centroid = CalculateCentroid(points);
        float cos = Mathf.Cos(angle);
        float sin = Mathf.Sin(angle);
        
        List<Vector2> rotated = new List<Vector2>();
        foreach (Vector2 p in points)
        {
            Vector2 translated = p - centroid;
            Vector2 rotatedPoint = new Vector2(
                translated.x * cos - translated.y * sin,
                translated.x * sin + translated.y * cos
            );
            rotated.Add(rotatedPoint + centroid);
        }
        
        return rotated;
    }
    
    private List<Vector2> ScaleToSquare(List<Vector2> points, float size)
    {
        Rect boundingBox = CalculateBoundingBox(points);
        
        float scaleX = size / boundingBox.width;
        float scaleY = size / boundingBox.height;
        float scale = Mathf.Min(scaleX, scaleY);
        
        List<Vector2> scaled = new List<Vector2>();
        foreach (Vector2 p in points)
        {
            scaled.Add(p * scale);
        }
        
        return scaled;
    }
    
    private List<Vector2> TranslateToOrigin(List<Vector2> points)
    {
        Vector2 centroid = CalculateCentroid(points);
        
        List<Vector2> translated = new List<Vector2>();
        foreach (Vector2 p in points)
        {
            translated.Add(p - centroid);
        }
        
        return translated;
    }
    
    private float CalculatePathDistance(List<Vector2> gesture, List<Vector2> template)
    {
        int n = Mathf.Min(gesture.Count, template.Count);
        float totalDistance = 0f;
        
        for (int i = 0; i < n; i++)
        {
            totalDistance += Vector2.Distance(gesture[i], template[i]);
        }
        
        float averageDistance = totalDistance / n;
        float normalizedDistance = averageDistance / normalizedSquareSize;
        
        return normalizedDistance;
    }
    
    private float CalculatePathLength(List<Vector2> points)
    {
        float length = 0f;
        for (int i = 1; i < points.Count; i++)
        {
            length += Vector2.Distance(points[i - 1], points[i]);
        }
        return length;
    }
    
    private Vector2 CalculateCentroid(List<Vector2> points)
    {
        Vector2 sum = Vector2.zero;
        foreach (Vector2 p in points)
        {
            sum += p;
        }
        return sum / points.Count;
    }
    
    private Rect CalculateBoundingBox(List<Vector2> points)
    {
        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;
        float maxY = float.MinValue;
        
        foreach (Vector2 p in points)
        {
            minX = Mathf.Min(minX, p.x);
            maxX = Mathf.Max(maxX, p.x);
            minY = Mathf.Min(minY, p.y);
            maxY = Mathf.Max(maxY, p.y);
        }
        
        return new Rect(minX, minY, maxX - minX, maxY - minY);
    }
    
    private GestureDirection CalculateDrawDirection(List<Vector2> points)
    {
        if (points.Count < 3)
        {
            return GestureDirection.None;
        }
        
        float totalAngle = 0f;
        
        for (int i = 1; i < points.Count - 1; i++)
        {
            Vector2 v1 = points[i] - points[i - 1];
            Vector2 v2 = points[i + 1] - points[i];
            
            float angle = Vector2.SignedAngle(v1, v2);
            totalAngle += angle;
        }
        
        if (Mathf.Abs(totalAngle) < DirectionThreshold)
        {
            return GestureDirection.None;
        }
        
        return totalAngle > 0 ? GestureDirection.CounterClockwise : GestureDirection.Clockwise;
    }
    
    public void AddSpell(SpellData spell)
    {
        if (!availableSpells.Contains(spell))
        {
            availableSpells.Add(spell);
        }
    }
    
    public void RemoveSpell(SpellData spell)
    {
        availableSpells.Remove(spell);
    }
    
    public void ClearSpells()
    {
        availableSpells.Clear();
    }
    
    public List<SpellData> GetAvailableSpells()
    {
        return new List<SpellData>(availableSpells);
    }
}

public struct GestureRecognitionResult
{
    public bool success;
    public SpellData recognizedSpell;
    public float confidence;
    public string message;
    public float drawSpeed;
    public GestureDirection drawDirection;
}

public struct GestureFeatures
{
    public float aspectRatio;
    public float totalCurvature;
    public float averageCurvature;
    public int directionChanges;
    public float closedness;
    public float startAngle;
    public float endAngle;
    public float boundingBoxArea;
}

public struct SpellMatchResult
{
    public SpellData spell;
    public float score;
    public bool passed;
}
