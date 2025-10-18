using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GestureLineRenderer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform lineContainer;
    [SerializeField] private Sprite circleSprite;
    
    [Header("Visual Settings")]
    [SerializeField] private float lineWidth = 10f;
    [SerializeField] private Color lineColor = new Color(0f, 1f, 1f, 1f);
    [SerializeField] private float clearFadeDuration = 0.5f;
    [SerializeField] private int minPointsToDisplay = 2;
    
    private GameObject currentLineObject;
    private List<GameObject> currentLineVisuals = new List<GameObject>();
    private List<Vector2> currentLinePoints = new List<Vector2>();
    
    private List<GameObject> persistentLines = new List<GameObject>();
    
    private void Awake()
    {
        if (lineContainer == null)
        {
            Debug.LogError("Line Container is not assigned!");
        }
        
        if (circleSprite == null)
        {
            Debug.LogWarning("Circle Sprite is not assigned! Lines may not render properly.");
        }
    }
    
    public void StartNewGestureLine(Vector2 initialLocalPosition)
    {
        if (currentLineObject != null)
        {
            Destroy(currentLineObject);
        }
        
        currentLineObject = new GameObject("GestureLine");
        currentLineObject.transform.SetParent(lineContainer, false);
        
        RectTransform lineRect = currentLineObject.AddComponent<RectTransform>();
        lineRect.anchorMin = Vector2.zero;
        lineRect.anchorMax = Vector2.one;
        lineRect.offsetMin = Vector2.zero;
        lineRect.offsetMax = Vector2.zero;
        
        currentLinePoints.Clear();
        currentLineVisuals.Clear();
        
        currentLinePoints.Add(initialLocalPosition);
        CreateCircleAtPosition(initialLocalPosition);
        
        Debug.Log($"New line started at local position: {initialLocalPosition}");
    }
    
    public void AddPointToCurrentLine(Vector2 localPosition)
    {
        if (currentLineObject == null)
        {
            Debug.LogWarning("Cannot add point: No active line!");
            return;
        }
        
        int previousIndex = currentLinePoints.Count - 1;
        Vector2 previousPosition = currentLinePoints[previousIndex];
        
        currentLinePoints.Add(localPosition);
        
        CreateLineBetweenPoints(previousPosition, localPosition);
        CreateCircleAtPosition(localPosition);
    }
    
    private void CreateCircleAtPosition(Vector2 localPosition)
    {
        GameObject circle = new GameObject($"Point_{currentLinePoints.Count}");
        circle.transform.SetParent(currentLineObject.transform, false);
        circle.transform.SetAsLastSibling();
        
        RectTransform rectTransform = circle.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = localPosition;
        rectTransform.sizeDelta = new Vector2(lineWidth, lineWidth);
        
        Image image = circle.AddComponent<Image>();
        image.sprite = circleSprite;
        image.color = lineColor;
        image.raycastTarget = false;
        
        currentLineVisuals.Add(circle);
    }
    
    private void CreateLineBetweenPoints(Vector2 startPosition, Vector2 endPosition)
    {
        Vector2 direction = endPosition - startPosition;
        float distance = direction.magnitude;
        
        if (distance < 0.1f)
        {
            return;
        }
        
        GameObject segment = new GameObject($"Segment_{currentLineVisuals.Count}");
        segment.transform.SetParent(currentLineObject.transform, false);
        segment.transform.SetAsFirstSibling();
        
        RectTransform rectTransform = segment.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0f, 0.5f);
        rectTransform.anchoredPosition = startPosition;
        rectTransform.sizeDelta = new Vector2(distance, lineWidth);
        
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rectTransform.localRotation = Quaternion.Euler(0, 0, angle);
        
        Image image = segment.AddComponent<Image>();
        image.color = lineColor;
        image.raycastTarget = false;
        
        currentLineVisuals.Add(segment);
    }
    
    public void FinalizeCurrentLine()
    {
        if (currentLineObject == null)
        {
            return;
        }
        
        if (currentLinePoints.Count >= minPointsToDisplay)
        {
            persistentLines.Add(currentLineObject);
            Debug.Log($"Line finalized with {currentLinePoints.Count} points - Line persists");
        }
        else
        {
            Debug.Log($"Line discarded: Only {currentLinePoints.Count} points");
            Destroy(currentLineObject);
        }
        
        currentLineObject = null;
        currentLinePoints.Clear();
        currentLineVisuals.Clear();
    }
    
    public void DiscardCurrentLine()
    {
        if (currentLineObject != null)
        {
            Destroy(currentLineObject);
            currentLineObject = null;
        }
        
        currentLinePoints.Clear();
        currentLineVisuals.Clear();
    }
    
    public void ClearAllLinesWithFade()
    {
        if (persistentLines.Count == 0)
        {
            Debug.Log("No lines to clear");
            return;
        }
        
        StartCoroutine(FadeOutAndDestroyLines());
    }
    
    private IEnumerator FadeOutAndDestroyLines()
    {
        List<GameObject> linesToClear = new List<GameObject>(persistentLines);
        persistentLines.Clear();
        
        float elapsed = 0f;
        
        while (elapsed < clearFadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / clearFadeDuration);
            
            Color fadedColor = lineColor;
            fadedColor.a = alpha;
            
            foreach (GameObject line in linesToClear)
            {
                if (line != null)
                {
                    Image[] images = line.GetComponentsInChildren<Image>();
                    foreach (Image img in images)
                    {
                        img.color = fadedColor;
                    }
                }
            }
            
            yield return null;
        }
        
        foreach (GameObject line in linesToClear)
        {
            if (line != null)
            {
                Destroy(line);
            }
        }
        
        Debug.Log($"Cleared {linesToClear.Count} lines with fade effect");
    }
    
    public int GetPersistentLineCount()
    {
        return persistentLines.Count;
    }
}
