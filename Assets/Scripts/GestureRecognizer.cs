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
    [Tooltip("If true, ignores speed and direction constraints for more lenient recognition")]
    [SerializeField] private bool lenientMode = true;
    [Tooltip("Maximum score to accept any spell (higher = more lenient)")]
    [SerializeField] private float globalToleranceOverride = 200f;
    
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
        
        Debug.Log($"Analyzing gesture: Speed={drawSpeed:F2}, Direction={drawDirection}, PathLength={pathLength:F2}");
        
        List<Vector2> processedGesture = PreprocessGesture(points2D, false);
        
        SpellData bestMatch = null;
        float bestScore = float.MaxValue;
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
            
            bool failedConstraint = false;
            string constraintFailReason = "";
            
            if (!lenientMode)
            {
                if (spell.enforceSpeed)
                {
                    if (drawSpeed < spell.expectedSpeedRange.x || drawSpeed > spell.expectedSpeedRange.y)
                    {
                        constraintFailReason = $"Speed {drawSpeed:F2} outside range [{spell.expectedSpeedRange.x}-{spell.expectedSpeedRange.y}]";
                        failedConstraint = true;
                    }
                }
                
                if (spell.enforceDirection && !failedConstraint)
                {
                    if (spell.expectedDirection != GestureDirection.None && 
                        drawDirection != spell.expectedDirection)
                    {
                        constraintFailReason = $"Direction {drawDirection} != {spell.expectedDirection}";
                        failedConstraint = true;
                    }
                }
            }
            
            List<Vector2> processedTemplate = PreprocessGesture(spell.gestureTemplate, spell.allowRotation);
            
            float score = CalculatePathDistance(processedGesture, processedTemplate);
            
            Debug.Log($"  Spell '{spell.spellName}': Score={score:F3}, LenientMode={lenientMode}, Failed={failedConstraint}, Reason={constraintFailReason}");
            
            if (!failedConstraint && score < bestScore)
            {
                bestScore = score;
                bestMatch = spell;
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
        
        float effectiveTolerance = lenientMode ? globalToleranceOverride : (bestMatch != null ? bestMatch.recognitionTolerance : 0f);
        
        if (bestMatch != null && bestScore <= effectiveTolerance)
        {
            float confidence = 1f - Mathf.Clamp01(bestScore / effectiveTolerance);
            
            if (playerUIController != null)
            {
                playerUIController.DisplayGestureHighlight(null);
            }
            
            Debug.Log($"<color=green>✓ RECOGNIZED: {bestMatch.spellName} (Score: {bestScore:F1}, Confidence: {confidence:P0})</color>");
            return new GestureRecognitionResult
            {
                success = true,
                recognizedSpell = bestMatch,
                confidence = confidence,
                message = $"Recognized: {bestMatch.spellName} ({confidence:P0})",
                drawSpeed = drawSpeed,
                drawDirection = drawDirection
            };
        }
        
        if (playerUIController != null)
        {
            playerUIController.ShowUnrecognizedGestureFeedback();
        }
        
        Debug.LogWarning($"Best match was '{(bestMatch != null ? bestMatch.spellName : "none")}' with score {bestScore:F3}, but exceeded tolerance {effectiveTolerance:F1}.");
        
        return new GestureRecognitionResult
        {
            success = false,
            confidence = bestScore < float.MaxValue ? (1f - bestScore) : 0f,
            message = "No matching spell found",
            drawSpeed = drawSpeed,
            drawDirection = drawDirection
        };
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
        
        return totalDistance / n;
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
