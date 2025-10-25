using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SpellData))]
public class SpellDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        SpellData spellData = (SpellData)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Template Generation Tools", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Circle"))
        {
            spellData.gestureTemplate = SpellTemplateCreator.CreateCircleTemplate(32, 100f);
            EditorUtility.SetDirty(spellData);
            Debug.Log($"Generated Circle template for {spellData.spellName}");
        }
        
        if (GUILayout.Button("Spiral"))
        {
            spellData.gestureTemplate = SpellTemplateCreator.CreateSpiralTemplate(64, 100f, 2f);
            EditorUtility.SetDirty(spellData);
            Debug.Log($"Generated Spiral template for {spellData.spellName}");
        }
        
        if (GUILayout.Button("V-Shape"))
        {
            spellData.gestureTemplate = SpellTemplateCreator.CreateVShapeTemplate(100f, 100f);
            EditorUtility.SetDirty(spellData);
            Debug.Log($"Generated V-Shape template for {spellData.spellName}");
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Square"))
        {
            spellData.gestureTemplate = SpellTemplateCreator.CreateSquareTemplate(100f);
            EditorUtility.SetDirty(spellData);
            Debug.Log($"Generated Square template for {spellData.spellName}");
        }
        
        if (GUILayout.Button("Triangle"))
        {
            spellData.gestureTemplate = SpellTemplateCreator.CreateTriangleTemplate(100f);
            EditorUtility.SetDirty(spellData);
            Debug.Log($"Generated Triangle template for {spellData.spellName}");
        }
        
        if (GUILayout.Button("Zigzag"))
        {
            spellData.gestureTemplate = SpellTemplateCreator.CreateZigzagTemplate(3, 100f, 50f);
            EditorUtility.SetDirty(spellData);
            Debug.Log($"Generated Zigzag template for {spellData.spellName}");
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        if (spellData.gestureTemplate != null && spellData.gestureTemplate.Count > 0)
        {
            EditorGUILayout.LabelField($"Template Points: {spellData.gestureTemplate.Count}", EditorStyles.miniLabel);
            
            if (GUILayout.Button("Clear Template"))
            {
                spellData.gestureTemplate.Clear();
                EditorUtility.SetDirty(spellData);
                Debug.Log($"Cleared template for {spellData.spellName}");
            }
        }
    }
}
