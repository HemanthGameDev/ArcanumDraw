using UnityEngine;
using System.Collections.Generic;

public static class SpellTemplateCreator
{
    public static List<Vector2> CreateCircleTemplate(int points = 32, float radius = 100f)
    {
        List<Vector2> template = new List<Vector2>();
        
        for (int i = 0; i < points; i++)
        {
            float angle = (i / (float)points) * Mathf.PI * 2f;
            Vector2 point = new Vector2(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius
            );
            template.Add(point);
        }
        
        return template;
    }
    
    public static List<Vector2> CreateSpiralTemplate(int points = 64, float maxRadius = 100f, float rotations = 2f)
    {
        List<Vector2> template = new List<Vector2>();
        
        for (int i = 0; i < points; i++)
        {
            float t = i / (float)(points - 1);
            float angle = t * rotations * Mathf.PI * 2f;
            float radius = t * maxRadius;
            
            Vector2 point = new Vector2(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius
            );
            template.Add(point);
        }
        
        return template;
    }
    
    public static List<Vector2> CreateVShapeTemplate(float width = 100f, float height = 100f)
    {
        List<Vector2> template = new List<Vector2>();
        
        int pointsPerSide = 16;
        
        for (int i = 0; i <= pointsPerSide; i++)
        {
            float t = i / (float)pointsPerSide;
            Vector2 point = new Vector2(
                -width / 2f + t * width / 2f,
                t * height
            );
            template.Add(point);
        }
        
        for (int i = 1; i <= pointsPerSide; i++)
        {
            float t = i / (float)pointsPerSide;
            Vector2 point = new Vector2(
                t * width / 2f,
                height - t * height
            );
            template.Add(point);
        }
        
        return template;
    }
    
    public static List<Vector2> CreateZigzagTemplate(int peaks = 3, float width = 100f, float height = 50f)
    {
        List<Vector2> template = new List<Vector2>();
        
        int pointsPerPeak = 8;
        
        for (int i = 0; i <= peaks * 2; i++)
        {
            for (int j = 0; j < pointsPerPeak; j++)
            {
                float x = (i * pointsPerPeak + j) / (float)(peaks * 2 * pointsPerPeak) * width;
                float y = (i % 2 == 0) ? 0f : height;
                
                if (j > 0)
                {
                    float t = j / (float)pointsPerPeak;
                    float prevY = (i % 2 == 0) ? height : 0f;
                    y = Mathf.Lerp(prevY, y, t);
                }
                
                template.Add(new Vector2(x - width / 2f, y - height / 2f));
            }
        }
        
        return template;
    }
    
    public static List<Vector2> CreateLineTemplate(Vector2 direction, float length = 100f)
    {
        List<Vector2> template = new List<Vector2>();
        
        direction = direction.normalized;
        
        template.Add(Vector2.zero);
        template.Add(direction * length);
        
        return template;
    }
    
    public static List<Vector2> CreateSquareTemplate(float size = 100f)
    {
        List<Vector2> template = new List<Vector2>();
        
        int pointsPerSide = 8;
        float half = size / 2f;
        
        for (int i = 0; i < pointsPerSide; i++)
        {
            float t = i / (float)pointsPerSide;
            template.Add(new Vector2(-half + t * size, -half));
        }
        
        for (int i = 0; i < pointsPerSide; i++)
        {
            float t = i / (float)pointsPerSide;
            template.Add(new Vector2(half, -half + t * size));
        }
        
        for (int i = 0; i < pointsPerSide; i++)
        {
            float t = i / (float)pointsPerSide;
            template.Add(new Vector2(half - t * size, half));
        }
        
        for (int i = 0; i < pointsPerSide; i++)
        {
            float t = i / (float)pointsPerSide;
            template.Add(new Vector2(-half, half - t * size));
        }
        
        return template;
    }
    
    public static List<Vector2> CreateTriangleTemplate(float size = 100f)
    {
        List<Vector2> template = new List<Vector2>();
        
        int pointsPerSide = 11;
        float half = size / 2f;
        float height = size * Mathf.Sqrt(3f) / 2f;
        
        for (int i = 0; i < pointsPerSide; i++)
        {
            float t = i / (float)(pointsPerSide - 1);
            template.Add(new Vector2(-half + t * size, -height / 3f));
        }
        
        for (int i = 0; i < pointsPerSide; i++)
        {
            float t = i / (float)(pointsPerSide - 1);
            template.Add(new Vector2(half - t * half, -height / 3f + t * height));
        }
        
        for (int i = 0; i < pointsPerSide; i++)
        {
            float t = i / (float)(pointsPerSide - 1);
            template.Add(new Vector2(-t * half, height * 2f / 3f - t * height));
        }
        
        return template;
    }
}
