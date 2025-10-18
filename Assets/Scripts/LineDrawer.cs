using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LineDrawer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform lineContainer;
    [SerializeField] private Sprite circleSprite;
    
    [Header("Line Settings")]
    [SerializeField] private float lineWidth = 10f;
    [SerializeField] private Color lineColor = new Color(0f, 1f, 1f, 1f);
    [SerializeField] private float clearFadeDuration = 0.5f;
    [SerializeField] private int minPointsForLine = 2;
    
    private GameObject currentLineObject;
    private List<GameObject> currentCircles = new List<GameObject>();
    private List<Vector2> currentLinePoints = new List<Vector2>();
    
    private List<GameObject> allCompletedLines = new List<GameObject>();
    private bool isClearingAll = false;
    private float clearFadeTimer = 0f;
    
    private void Update()
    {
        if (isClearingAll)
        {
            clearFadeTimer += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, clearFadeTimer / clearFadeDuration);
            
            Color fadedColor = lineColor;
            fadedColor.a = alpha;
            
            foreach (GameObject line in allCompletedLines)
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
            
            if (clearFadeTimer >= clearFadeDuration)
            {
                foreach (GameObject line in allCompletedLines)
                {
                    if (line != null)
                    {
                        Destroy(line);
                    }
                }
                allCompletedLines.Clear();
                isClearingAll = false;
                Debug.Log("All lines cleared");
            }
        }
    }
    
    public void StartNewLine()
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
        currentCircles.Clear();
    }
    
    public void AddPoint(Vector2 localPosition)
    {
        if (currentLineObject == null)
        {
            Debug.LogWarning("Trying to add point but no line exists!");
            return;
        }
        
        currentLinePoints.Add(localPosition);
        
        GameObject circle = new GameObject($"Point_{currentLinePoints.Count}");
        circle.transform.SetParent(currentLineObject.transform, false);
        
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
        
        currentCircles.Add(circle);
        
        if (currentLinePoints.Count <= 3)
        {
            Debug.Log($"LineDrawer: Added visual point at local position {localPosition}");
        }
        
        if (currentLinePoints.Count > 1)
        {
            CreateLineBetweenPoints(currentLinePoints[currentLinePoints.Count - 2], localPosition);
        }
    }
    
    private void CreateLineBetweenPoints(Vector2 pointA, Vector2 pointB)
    {
        Vector2 direction = pointB - pointA;
        float distance = direction.magnitude;
        
        if (distance < 0.1f) return;
        
        GameObject lineSegment = new GameObject($"Segment_{currentCircles.Count}");
        lineSegment.transform.SetParent(currentLineObject.transform, false);
        lineSegment.transform.SetAsFirstSibling();
        
        RectTransform rectTransform = lineSegment.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0, 0.5f);
        
        rectTransform.anchoredPosition = pointA;
        rectTransform.sizeDelta = new Vector2(distance, lineWidth);
        
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rectTransform.localRotation = Quaternion.Euler(0, 0, angle);
        
        Image image = lineSegment.AddComponent<Image>();
        image.color = lineColor;
        image.raycastTarget = false;
        
        currentCircles.Add(lineSegment);
    }
    
    public void FinishLine()
    {
        if (currentLineObject == null) return;
        
        if (currentLinePoints.Count >= minPointsForLine)
        {
            allCompletedLines.Add(currentLineObject);
            Debug.Log($"Line finished with {currentLinePoints.Count} points - Line persists");
        }
        else
        {
            Debug.Log($"Line too short ({currentLinePoints.Count} points), destroying immediately");
            Destroy(currentLineObject);
        }
        
        currentLineObject = null;
        currentLinePoints.Clear();
        currentCircles.Clear();
    }
    
    public void CancelCurrentLine()
    {
        if (currentLineObject != null)
        {
            Destroy(currentLineObject);
            currentLineObject = null;
        }
        
        currentLinePoints.Clear();
        currentCircles.Clear();
    }
    
    public void ClearAllLines()
    {
        if (allCompletedLines.Count == 0)
        {
            Debug.Log("No lines to clear");
            return;
        }
        
        isClearingAll = true;
        clearFadeTimer = 0f;
        
        Debug.Log($"Starting to clear {allCompletedLines.Count} lines");
    }
}
