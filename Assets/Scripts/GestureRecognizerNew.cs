using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GestureRecognizerNew : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private List<SpellData> availableSpells = new List<SpellData>();
    [SerializeField] private SpellCaster spellCaster;
    [SerializeField] private PlayerUIController playerUIController;
    
    [Header("Recognition Settings")]
    [SerializeField] private int resamplePointCount = 64;
    [SerializeField] private float normalizedSquareSize = 250f;
    
    [Header("Multi-Rotation Recognition")]
    [Tooltip("Test multiple rotation angles for better accuracy")]
    [SerializeField] private bool useMultiRotationMatching = true;
    
    [Tooltip("Use Golden Section Search for optimal rotation (faster than brute force)")]
    [SerializeField] private bool useGoldenSectionSearch = true;
    
    [Tooltip("Number of rotation steps if not using Golden Section")]
    [SerializeField] [Range(4, 12)] private int rotationSteps = 8;
    
    [Tooltip("Recognition threshold (0.35-0.45 recommended)")]
    [SerializeField] [Range(0.15f, 0.70f)] private float recognitionTolerance = 0.40f;
    
    [Header("Advanced Features")]
    [Tooltip("Use scale-invariant matching")]
    [SerializeField] private bool useScaleInvariance = true;
    
    [Tooltip("Use starting point invariance for closed shapes")]
    [SerializeField] private bool useStartPointInvariance = true;
    
    [Tooltip("Number of starting points to test")]
    [SerializeField] [Range(4, 12)] private int startPointTests = 8;
    
    [Tooltip("Show detailed debug logs in console")]
    [SerializeField] private bool debugMode = true;
    
    private const float DirectionThreshold = 30f;
    private const float MaxRotationAngle = 45f;
    private const float GoldenRatio = 0.618033989f;
    
    private List<Vector2> reusableGestureList = new List<Vector2>(64);
    private List<Vector2> reusableTemplateList = new List<Vector2>(64);
    private List<SpellMatchResult> reusableMatchResults = new List<SpellMatchResult>(10);
    
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
            Debug.LogWarning("GestureRecognizer: No spells assigned!");
            return new GestureRecognitionResult 
            { 
                success = false, 
                message = "No spells configured" 
            };
        }
        
        List<Vector2> points2D = drawnPoints.Select(p => new Vector2(p.x, p.y)).ToList();
        
        if (GestureTemplateRecorder.Instance != null && GestureTemplateRecorder.Instance.IsRecording)
        {
            GestureTemplateRecorder.Instance.RecordGesture(points2D);
        }
        
        float pathLength = CalculatePathLength(points2D);
        float drawSpeed = totalDrawingTime > 0 ? pathLength / totalDrawingTime : 0f;
        GestureDirection drawDirection = CalculateDrawDirection(points2D);
        
        if (debugMode)
        {
            Debug.Log($"<color=cyan>━━━ GESTURE ANALYSIS ━━━</color>");
            Debug.Log($"Points: {drawnPoints.Count} | Path Length: {pathLength:F1} | Speed: {drawSpeed:F1} | Direction: {drawDirection}");
        }
        
        List<Vector2> processedGesture = PreprocessGesture(points2D);
        
        reusableMatchResults.Clear();
        int validSpellCount = 0;
        
        foreach (SpellData spell in availableSpells)
        {
            if (spell == null || spell.gestureTemplate == null || spell.gestureTemplate.Count == 0)
            {
                if (spell != null && debugMode)
                {
                    Debug.LogWarning($"Spell '{spell.spellName}' has no template!");
                }
                continue;
            }
            
            validSpellCount++;
            
            List<Vector2> processedTemplate = PreprocessGesture(spell.gestureTemplate);
            
            float bestScore = float.MaxValue;
            
            if (useStartPointInvariance && IsClosedGesture(processedTemplate))
            {
                int step = Mathf.Max(1, processedTemplate.Count / startPointTests);
                
                for (int startIdx = 0; startIdx < startPointTests; startIdx++)
                {
                    List<Vector2> shiftedTemplate = ShiftStartingPoint(processedTemplate, startIdx * step);
                    float score = ComputeBestScore(processedGesture, shiftedTemplate);
                    
                    if (score < bestScore)
                    {
                        bestScore = score;
                    }
                }
            }
            else
            {
                bestScore = ComputeBestScore(processedGesture, processedTemplate);
            }
            
            float spellTolerance = spell.recognitionTolerance > 0 
                ? spell.recognitionTolerance 
                : recognitionTolerance;
            
            reusableMatchResults.Add(new SpellMatchResult
            {
                spell = spell,
                score = bestScore,
                passed = bestScore <= spellTolerance
            });
            
            if (debugMode)
            {
                string status = bestScore <= spellTolerance ? "<color=green>✓ PASS</color>" : "<color=red>✗ FAIL</color>";
                float margin = spellTolerance - bestScore;
                string marginStr = margin > 0 
                    ? $"<color=green>(margin: +{margin:F2})</color>" 
                    : $"<color=red>(over by {-margin:F2})</color>";
                    
                Debug.Log($"  [{spell.spellName}] Score: {bestScore:F4} vs Tolerance: {spellTolerance:F2} {status} {marginStr}");
            }
        }
        
        if (validSpellCount == 0)
        {
            Debug.LogError("No valid spell templates found! Generate templates for your spells.");
            return new GestureRecognitionResult
            {
                success = false,
                message = "No valid spell templates"
            };
        }
        
        reusableMatchResults.Sort((a, b) => a.score.CompareTo(b.score));
        
        SpellMatchResult bestMatch = reusableMatchResults[0];
        
        if (bestMatch.passed)
        {
            float spellTolerance = bestMatch.spell.recognitionTolerance > 0 
                ? bestMatch.spell.recognitionTolerance 
                : recognitionTolerance;
            
            float confidence = 1f - Mathf.Clamp01(bestMatch.score / spellTolerance);
            
            if (playerUIController != null)
            {
                playerUIController.DisplayGestureHighlight(null);
            }
            
            if (debugMode)
            {
                Debug.Log($"<color=green>✓✓✓ SPELL RECOGNIZED: {bestMatch.spell.spellName} ✓✓✓</color>");
                Debug.Log($"Score: {bestMatch.score:F4} | Confidence: {confidence:P0}");
            }
            
            return new GestureRecognitionResult
            {
                success = true,
                recognizedSpell = bestMatch.spell,
                confidence = confidence,
                message = $"Recognized: {bestMatch.spell.spellName}",
                drawSpeed = drawSpeed,
                drawDirection = drawDirection
            };
        }
        
        if (playerUIController != null)
        {
            playerUIController.ShowUnrecognizedGestureFeedback();
        }
        
        float bestTolerance = bestMatch.spell.recognitionTolerance > 0 
            ? bestMatch.spell.recognitionTolerance 
            : recognitionTolerance;
        
        if (debugMode)
        {
            Debug.LogWarning($"<color=yellow>✗ NO MATCH - Best: {bestMatch.spell.spellName} ({bestMatch.score:F4}) | Tolerance: {bestTolerance:F2}</color>");
        }
        
        return new GestureRecognitionResult
        {
            success = false,
            confidence = 0f,
            message = $"No match. Closest: {bestMatch.spell.spellName}",
            drawSpeed = drawSpeed,
            drawDirection = drawDirection
        };
    }
    
    private List<Vector2> PreprocessGesture(List<Vector2> points)
    {
        if (points == null || points.Count < 2)
            return new List<Vector2>();
        
        List<Vector2> processed = Resample(points, resamplePointCount);
        
        if (useScaleInvariance)
        {
            processed = ScaleToSquare(processed, normalizedSquareSize);
        }
        
        processed = TranslateToOrigin(processed);
        return processed;
    }
    
    private List<Vector2> Resample(List<Vector2> points, int n)
    {
        if (points.Count < 2) return new List<Vector2>(points);
        
        float pathLength = 0f;
        for (int i = 1; i < points.Count; i++)
        {
            pathLength += Vector2.Distance(points[i - 1], points[i]);
        }
        
        if (pathLength < 0.001f) return new List<Vector2> { points[0] };
        
        float interval = pathLength / (n - 1);
        List<Vector2> resampled = new List<Vector2>(n) { points[0] };
        float currentDist = 0f;
        
        Vector2 prevPoint = points[0];
        
        for (int i = 1; i < points.Count && resampled.Count < n; i++)
        {
            Vector2 currentPoint = points[i];
            float segmentDist = Vector2.Distance(prevPoint, currentPoint);
            
            if (currentDist + segmentDist >= interval)
            {
                while (currentDist + segmentDist >= interval && resampled.Count < n)
                {
                    float neededDist = interval - currentDist;
                    float ratio = neededDist / segmentDist;
                    Vector2 newPoint = Vector2.Lerp(prevPoint, currentPoint, ratio);
                    resampled.Add(newPoint);
                    
                    segmentDist -= neededDist;
                    prevPoint = newPoint;
                    currentDist = 0f;
                }
                currentDist = segmentDist;
            }
            else
            {
                currentDist += segmentDist;
            }
            
            prevPoint = currentPoint;
        }
        
        while (resampled.Count < n)
        {
            resampled.Add(points[points.Count - 1]);
        }
        
        if (resampled.Count > n)
            resampled.RemoveRange(n, resampled.Count - n);
        
        return resampled;
    }
    
    private List<Vector2> RotateBy(List<Vector2> points, float angleRad)
    {
        Vector2 centroid = CalculateCentroid(points);
        float cos = Mathf.Cos(angleRad);
        float sin = Mathf.Sin(angleRad);
        
        List<Vector2> rotated = new List<Vector2>(points.Count);
        for (int i = 0; i < points.Count; i++)
        {
            Vector2 translated = points[i] - centroid;
            rotated.Add(new Vector2(
                translated.x * cos - translated.y * sin + centroid.x,
                translated.x * sin + translated.y * cos + centroid.y
            ));
        }
        
        return rotated;
    }
    
    private List<Vector2> ScaleToSquare(List<Vector2> points, float size)
    {
        if (points.Count == 0) return points;
        
        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;
        
        for (int i = 0; i < points.Count; i++)
        {
            if (points[i].x < minX) minX = points[i].x;
            if (points[i].x > maxX) maxX = points[i].x;
            if (points[i].y < minY) minY = points[i].y;
            if (points[i].y > maxY) maxY = points[i].y;
        }
        
        float width = maxX - minX;
        float height = maxY - minY;
        
        if (width < 0.001f && height < 0.001f) return points;
        
        float scale = size / Mathf.Max(width, height);
        
        List<Vector2> scaled = new List<Vector2>(points.Count);
        for (int i = 0; i < points.Count; i++)
        {
            scaled.Add(points[i] * scale);
        }
        
        return scaled;
    }
    
    private List<Vector2> TranslateToOrigin(List<Vector2> points)
    {
        if (points.Count == 0) return points;
        
        Vector2 centroid = CalculateCentroid(points);
        List<Vector2> translated = new List<Vector2>(points.Count);
        
        for (int i = 0; i < points.Count; i++)
        {
            translated.Add(points[i] - centroid);
        }
        
        return translated;
    }
    
    private float CalculatePathDistance(List<Vector2> gesture, List<Vector2> template)
    {
        if (gesture.Count != template.Count) return float.MaxValue;
        
        float totalDistance = 0f;
        int count = gesture.Count;
        
        for (int i = 0; i < count; i++)
        {
            float dx = gesture[i].x - template[i].x;
            float dy = gesture[i].y - template[i].y;
            totalDistance += Mathf.Sqrt(dx * dx + dy * dy);
        }
        
        float halfDiagonal = 0.5f * normalizedSquareSize * 1.41421356f;
        return totalDistance / (count * halfDiagonal);
    }
    
    private float CalculatePathLength(List<Vector2> points)
    {
        if (points.Count < 2) return 0f;
        
        float length = 0f;
        for (int i = 1; i < points.Count; i++)
        {
            float dx = points[i].x - points[i - 1].x;
            float dy = points[i].y - points[i - 1].y;
            length += Mathf.Sqrt(dx * dx + dy * dy);
        }
        return length;
    }
    
    private Vector2 CalculateCentroid(List<Vector2> points)
    {
        if (points.Count == 0) return Vector2.zero;
        
        float sumX = 0f, sumY = 0f;
        for (int i = 0; i < points.Count; i++)
        {
            sumX += points[i].x;
            sumY += points[i].y;
        }
        
        return new Vector2(sumX / points.Count, sumY / points.Count);
    }
    
    private Rect CalculateBoundingBox(List<Vector2> points)
    {
        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;
        
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
        if (points.Count < 3) return GestureDirection.None;
        
        float totalAngle = 0f;
        for (int i = 1; i < points.Count - 1; i++)
        {
            Vector2 v1 = points[i] - points[i - 1];
            Vector2 v2 = points[i + 1] - points[i];
            totalAngle += Vector2.SignedAngle(v1, v2);
        }
        
        if (Mathf.Abs(totalAngle) < DirectionThreshold) return GestureDirection.None;
        return totalAngle > 0 ? GestureDirection.CounterClockwise : GestureDirection.Clockwise;
    }
    
    private float DistanceAtBestAngleGolden(List<Vector2> gesture, List<Vector2> template, float angleMin, float angleMax)
    {
        const float AnglePrecision = 2.0f;
        float phi = GoldenRatio;
        
        float x1 = phi * angleMin + (1 - phi) * angleMax;
        float f1 = DistanceAtAngle(gesture, template, x1);
        
        float x2 = (1 - phi) * angleMin + phi * angleMax;
        float f2 = DistanceAtAngle(gesture, template, x2);
        
        while (Mathf.Abs(angleMax - angleMin) > AnglePrecision)
        {
            if (f1 < f2)
            {
                angleMax = x2;
                x2 = x1;
                f2 = f1;
                x1 = phi * angleMin + (1 - phi) * angleMax;
                f1 = DistanceAtAngle(gesture, template, x1);
            }
            else
            {
                angleMin = x1;
                x1 = x2;
                f1 = f2;
                x2 = (1 - phi) * angleMin + phi * angleMax;
                f2 = DistanceAtAngle(gesture, template, x2);
            }
        }
        
        return Mathf.Min(f1, f2);
    }
    
    private float DistanceAtAngle(List<Vector2> gesture, List<Vector2> template, float angleDeg)
    {
        List<Vector2> rotatedGesture = RotateBy(gesture, angleDeg * Mathf.Deg2Rad);
        return CalculatePathDistance(rotatedGesture, template);
    }
    
    private float ComputeBestScore(List<Vector2> gesture, List<Vector2> template)
    {
        float bestScore;
        
        if (useMultiRotationMatching)
        {
            if (useGoldenSectionSearch)
            {
                bestScore = DistanceAtBestAngleGolden(gesture, template, -MaxRotationAngle, MaxRotationAngle);
            }
            else
            {
                bestScore = float.MaxValue;
                float angleStep = (MaxRotationAngle * 2f) / rotationSteps;
                
                for (int i = 0; i <= rotationSteps; i++)
                {
                    float testAngle = -MaxRotationAngle + (i * angleStep);
                    List<Vector2> rotatedGesture = RotateBy(gesture, testAngle * Mathf.Deg2Rad);
                    
                    float score = CalculatePathDistance(rotatedGesture, template);
                    
                    if (score < bestScore)
                    {
                        bestScore = score;
                    }
                }
            }
        }
        else
        {
            bestScore = CalculatePathDistance(gesture, template);
        }
        
        return bestScore;
    }
    
    private bool IsClosedGesture(List<Vector2> points)
    {
        if (points.Count < 10) return false;
        
        float distanceToClose = Vector2.Distance(points[0], points[points.Count - 1]);
        float averageSegmentLength = CalculatePathLength(points) / points.Count;
        
        return distanceToClose < (averageSegmentLength * 3f);
    }
    
    private List<Vector2> ShiftStartingPoint(List<Vector2> points, int newStartIndex)
    {
        if (newStartIndex <= 0 || newStartIndex >= points.Count)
            return new List<Vector2>(points);
        
        List<Vector2> shifted = new List<Vector2>();
        
        for (int i = 0; i < points.Count; i++)
        {
            shifted.Add(points[(newStartIndex + i) % points.Count]);
        }
        
        return shifted;
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
