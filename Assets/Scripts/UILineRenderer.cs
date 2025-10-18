using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class UILineRenderer : Graphic
{
    [SerializeField] private Vector2[] _points;
    [SerializeField] private float _lineThickness = 10f;
    [SerializeField] private bool _relativeSize = false;
    
    public float lineThickness
    {
        get { return _lineThickness; }
        set { _lineThickness = value; SetVerticesDirty(); }
    }
    
    public Vector2[] points
    {
        get { return _points; }
        set { _points = value; SetVerticesDirty(); }
    }
    
    public bool relativeSize
    {
        get { return _relativeSize; }
        set { _relativeSize = value; SetVerticesDirty(); }
    }
    
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        
        if (_points == null || _points.Length < 2)
            return;
        
        float thickness = _lineThickness;
        
        if (_relativeSize)
        {
            thickness = rectTransform.rect.width * thickness;
        }
        
        for (int i = 0; i < _points.Length - 1; i++)
        {
            Vector2 point1 = _points[i];
            Vector2 point2 = _points[i + 1];
            
            DrawLineSegment(point1, point2, thickness, vh);
        }
    }
    
    private void DrawLineSegment(Vector2 point1, Vector2 point2, float thickness, VertexHelper vh)
    {
        Vector2 direction = (point2 - point1).normalized;
        Vector2 perpendicular = new Vector2(-direction.y, direction.x) * (thickness * 0.5f);
        
        int vertexIndex = vh.currentVertCount;
        
        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;
        
        vertex.position = point1 - perpendicular;
        vh.AddVert(vertex);
        
        vertex.position = point1 + perpendicular;
        vh.AddVert(vertex);
        
        vertex.position = point2 + perpendicular;
        vh.AddVert(vertex);
        
        vertex.position = point2 - perpendicular;
        vh.AddVert(vertex);
        
        vh.AddTriangle(vertexIndex, vertexIndex + 1, vertexIndex + 2);
        vh.AddTriangle(vertexIndex + 2, vertexIndex + 3, vertexIndex);
    }
}
