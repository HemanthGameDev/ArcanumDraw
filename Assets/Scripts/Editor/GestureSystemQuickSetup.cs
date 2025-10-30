using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class GestureSystemQuickSetup : EditorWindow
{
    [MenuItem("Arcanum Draw/🎯 Quick Setup & Calibration")]
    public static void ShowWindow()
    {
        var window = GetWindow<GestureSystemQuickSetup>("Gesture System Setup");
        window.minSize = new Vector2(450, 600);
        window.Show();
    }
    
    private Vector2 scrollPosition;
    
    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        EditorGUILayout.Space(10);
        
        DrawHeader();
        
        EditorGUILayout.Space(10);
        
        DrawPhase1();
        
        EditorGUILayout.Space(10);
        
        DrawPhase2();
        
        EditorGUILayout.Space(10);
        
        DrawQuickActions();
        
        EditorGUILayout.EndScrollView();
    }
    
    private void DrawHeader()
    {
        EditorGUILayout.LabelField("🎯 Gesture Recognition System", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Quick Setup & Calibration Tool", EditorStyles.miniLabel);
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.HelpBox(
            "This tool helps you achieve 99% recognition accuracy like War of Wizards.\n\n" +
            "Follow the 2-phase approach:\n" +
            "Phase 1: Global calibration (guaranteed match)\n" +
            "Phase 2: Template refinement & final tuning",
            MessageType.Info
        );
    }
    
    private void DrawPhase1()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.LabelField("📈 PHASE 1: Global System Calibration", EditorStyles.boldLabel);
        
        EditorGUILayout.HelpBox(
            "Goal: Temporarily loosen all constraints to confirm the system works.\n" +
            "This proves any failures are due to strict settings, not broken code.",
            MessageType.Info
        );
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.LabelField("Step 1: Set Global Tolerance to 0.85 (Very Lenient)", EditorStyles.boldLabel);
        
        if (GUILayout.Button("🔧 Set Global Tolerance to 0.85", GUILayout.Height(30)))
        {
            SetGlobalTolerance(0.85f);
        }
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.LabelField("Step 2: Disable All Spell Constraints", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Disables speed and direction checks for all spells", MessageType.None);
        
        if (GUILayout.Button("🔓 Disable All Constraints (Speed & Direction)", GUILayout.Height(30)))
        {
            DisableAllConstraints();
        }
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.LabelField("Step 3: Test in Play Mode", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "1. Enter Play Mode\n" +
            "2. Draw your shield gesture\n" +
            "3. It SHOULD recognize now (if not, template is broken)\n" +
            "4. Check Console for actual score (e.g., [shield] Score: 0.32)",
            MessageType.None
        );
        
        if (!EditorApplication.isPlaying)
        {
            if (GUILayout.Button("▶ Enter Play Mode", GUILayout.Height(30)))
            {
                EditorApplication.isPlaying = true;
            }
        }
        else
        {
            EditorGUILayout.HelpBox("✅ Play Mode Active - Draw gestures and check Console!", MessageType.Info);
            
            if (GUILayout.Button("⏹ Exit Play Mode", GUILayout.Height(30)))
            {
                EditorApplication.isPlaying = false;
            }
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawPhase2()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.LabelField("🎨 PHASE 2: Template Refinement & Final Tuning", EditorStyles.boldLabel);
        
        EditorGUILayout.HelpBox(
            "Goal: Create perfect templates and set strict thresholds.\n" +
            "This achieves 99% accuracy by optimizing both template quality and tolerance.",
            MessageType.Info
        );
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.LabelField("Step 1: Re-Record Templates", EditorStyles.boldLabel);
        
        SpellData[] allSpells = FindAllSpellData();
        
        if (allSpells.Length == 0)
        {
            EditorGUILayout.HelpBox("⚠ No SpellData assets found!", MessageType.Warning);
        }
        else
        {
            EditorGUILayout.HelpBox($"Found {allSpells.Length} spells. Click each to re-record its template:", MessageType.None);
            
            foreach (var spell in allSpells)
            {
                EditorGUILayout.BeginHorizontal();
                
                EditorGUILayout.LabelField(spell.spellName, GUILayout.Width(100));
                
                string status = spell.gestureTemplate != null && spell.gestureTemplate.Count > 0
                    ? $"✓ {spell.gestureTemplate.Count} pts"
                    : "❌ No template";
                
                EditorGUILayout.LabelField(status, GUILayout.Width(100));
                
                if (GUILayout.Button("🎨 Re-Record", GUILayout.Width(100)))
                {
                    PatternTemplateGeneratorWindow.ShowWindow(spell);
                }
                
                if (GUILayout.Button("📊 View", GUILayout.Width(60)))
                {
                    Selection.activeObject = spell;
                }
                
                EditorGUILayout.EndHorizontal();
            }
        }
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.LabelField("Step 2: Find Optimal Tolerance Per Spell", EditorStyles.boldLabel);
        
        EditorGUILayout.HelpBox(
            "After re-recording templates:\n" +
            "1. Set tolerance back to testing (0.85)\n" +
            "2. Draw each spell perfectly 3 times\n" +
            "3. Note the average score from Console\n" +
            "4. Set tolerance = average score + 0.10\n\n" +
            "Example: Shield score = 0.32 → Set to 0.42",
            MessageType.None
        );
        
        if (GUILayout.Button("🔬 Set All to Testing Mode (0.85)", GUILayout.Height(30)))
        {
            SetAllSpellsToTolerance(0.85f);
        }
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.LabelField("Step 3: Apply Recommended Final Tolerances", EditorStyles.boldLabel);
        
        EditorGUILayout.HelpBox(
            "Based on War of Wizards standards:\n" +
            "• Shield (closed shape): 0.40-0.45\n" +
            "• Fireball (complex): 0.35-0.40\n" +
            "• Lightning (simple zig-zag): 0.30-0.35",
            MessageType.None
        );
        
        if (GUILayout.Button("✨ Apply War of Wizards Settings", GUILayout.Height(30)))
        {
            ApplyWarOfWizardsSettings();
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawQuickActions()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.LabelField("⚡ Quick Actions", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Reset All to 0.40\n(Recommended)", GUILayout.Height(40)))
        {
            SetAllSpellsToTolerance(0.40f);
            SetGlobalTolerance(0.40f);
        }
        
        if (GUILayout.Button("Enable All Constraints", GUILayout.Height(40)))
        {
            EnableAllConstraints();
        }
        
        if (GUILayout.Button("Disable All Constraints", GUILayout.Height(40)))
        {
            DisableAllConstraints();
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        if (GUILayout.Button("📖 Open Full Guide", GUILayout.Height(30)))
        {
            var guide = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Scripts/GESTURE_RECOGNITION_99PERCENT_GUIDE.md");
            if (guide != null)
            {
                Selection.activeObject = guide;
            }
            else
            {
                Debug.LogWarning("Guide not found at Assets/Scripts/GESTURE_RECOGNITION_99PERCENT_GUIDE.md");
            }
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void SetGlobalTolerance(float tolerance)
    {
        var recognizer = FindObjectOfType<GestureRecognizerNew>();
        
        if (recognizer == null)
        {
            EditorUtility.DisplayDialog("Not Found",
                "GestureRecognizerNew component not found in scene!\n\n" +
                "Make sure your scene is open and has the GestureManager with GestureRecognizerNew.",
                "OK");
            return;
        }
        
        SerializedObject so = new SerializedObject(recognizer);
        SerializedProperty prop = so.FindProperty("recognitionTolerance");
        
        if (prop != null)
        {
            prop.floatValue = tolerance;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(recognizer);
            
            Debug.Log($"<color=green>✓ Set global tolerance to {tolerance:F2}</color>");
        }
    }
    
    private void SetAllSpellsToTolerance(float tolerance)
    {
        SpellData[] allSpells = FindAllSpellData();
        
        if (allSpells.Length == 0)
        {
            EditorUtility.DisplayDialog("No Spells", "No SpellData assets found!", "OK");
            return;
        }
        
        foreach (var spell in allSpells)
        {
            Undo.RecordObject(spell, "Set Tolerance");
            spell.recognitionTolerance = tolerance;
            EditorUtility.SetDirty(spell);
        }
        
        AssetDatabase.SaveAssets();
        
        Debug.Log($"<color=green>✓ Set {allSpells.Length} spells to tolerance {tolerance:F2}</color>");
    }
    
    private void DisableAllConstraints()
    {
        SpellData[] allSpells = FindAllSpellData();
        
        if (allSpells.Length == 0)
        {
            EditorUtility.DisplayDialog("No Spells", "No SpellData assets found!", "OK");
            return;
        }
        
        foreach (var spell in allSpells)
        {
            Undo.RecordObject(spell, "Disable Constraints");
            spell.enforceSpeed = false;
            spell.enforceDirection = false;
            EditorUtility.SetDirty(spell);
        }
        
        AssetDatabase.SaveAssets();
        
        Debug.Log($"<color=green>✓ Disabled constraints for {allSpells.Length} spells</color>");
    }
    
    private void EnableAllConstraints()
    {
        SpellData[] allSpells = FindAllSpellData();
        
        if (allSpells.Length == 0)
        {
            EditorUtility.DisplayDialog("No Spells", "No SpellData assets found!", "OK");
            return;
        }
        
        foreach (var spell in allSpells)
        {
            Undo.RecordObject(spell, "Enable Constraints");
            spell.enforceSpeed = true;
            spell.enforceDirection = true;
            EditorUtility.SetDirty(spell);
        }
        
        AssetDatabase.SaveAssets();
        
        Debug.Log($"<color=yellow>⚠ Enabled constraints for {allSpells.Length} spells</color>");
    }
    
    private void ApplyWarOfWizardsSettings()
    {
        SpellData[] allSpells = FindAllSpellData();
        
        if (allSpells.Length == 0)
        {
            EditorUtility.DisplayDialog("No Spells", "No SpellData assets found!", "OK");
            return;
        }
        
        foreach (var spell in allSpells)
        {
            Undo.RecordObject(spell, "Apply War of Wizards Settings");
            
            spell.enforceSpeed = false;
            spell.enforceDirection = false;
            spell.allowRotation = true;
            
            string spellLower = spell.spellName.ToLower();
            
            if (spellLower.Contains("shield"))
            {
                spell.recognitionTolerance = 0.42f;
                Debug.Log($"Shield: Set to 0.42 (closed shape)");
            }
            else if (spellLower.Contains("fire") || spellLower.Contains("ball"))
            {
                spell.recognitionTolerance = 0.38f;
                Debug.Log($"Fireball: Set to 0.38 (complex shape)");
            }
            else if (spellLower.Contains("light") || spellLower.Contains("thunder"))
            {
                spell.recognitionTolerance = 0.32f;
                spell.allowRotation = false;
                Debug.Log($"Lightning: Set to 0.32 (zig-zag, no rotation)");
            }
            else
            {
                spell.recognitionTolerance = 0.40f;
                Debug.Log($"{spell.spellName}: Set to 0.40 (default)");
            }
            
            EditorUtility.SetDirty(spell);
        }
        
        SetGlobalTolerance(0.40f);
        
        AssetDatabase.SaveAssets();
        
        Debug.Log($"<color=green>✅ Applied War of Wizards settings to {allSpells.Length} spells!</color>");
    }
    
    private SpellData[] FindAllSpellData()
    {
        string[] guids = AssetDatabase.FindAssets("t:SpellData");
        return guids.Select(guid => AssetDatabase.LoadAssetAtPath<SpellData>(AssetDatabase.GUIDToAssetPath(guid)))
                    .Where(spell => spell != null)
                    .ToArray();
    }
}
