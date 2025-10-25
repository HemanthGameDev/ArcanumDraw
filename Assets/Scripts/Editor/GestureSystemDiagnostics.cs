using UnityEngine;
using UnityEditor;

public class GestureSystemDiagnostics : EditorWindow
{
    [MenuItem("Arcanum Draw/Diagnose Gesture System")]
    public static void ShowWindow()
    {
        GetWindow<GestureSystemDiagnostics>("Gesture System Diagnostics");
    }

    private Vector2 scrollPosition;

    private void OnGUI()
    {
        GUILayout.Label("Gesture Recognition System Diagnostics", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("Run Full Diagnostic", GUILayout.Height(30)))
        {
            RunDiagnostics();
        }

        GUILayout.Space(10);
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        GUILayout.EndScrollView();
    }

    private void RunDiagnostics()
    {
        Debug.Log("=== GESTURE SYSTEM DIAGNOSTICS START ===");
        
        bool allGood = true;

        GestureDrawingManager drawingManager = FindObjectOfType<GestureDrawingManager>();
        if (drawingManager == null)
        {
            Debug.LogError("❌ CRITICAL: No GestureDrawingManager found in scene!");
            allGood = false;
        }
        else
        {
            Debug.Log("✅ GestureDrawingManager found");
            
            SerializedObject so = new SerializedObject(drawingManager);
            
            SerializedProperty runePadProp = so.FindProperty("runePadController");
            if (runePadProp.objectReferenceValue == null)
            {
                Debug.LogError("❌ GestureDrawingManager: RunePadController NOT assigned!");
                allGood = false;
            }
            else
            {
                Debug.Log($"✅ GestureDrawingManager: RunePadController assigned ({runePadProp.objectReferenceValue.name})");
            }
            
            SerializedProperty recognizerProp = so.FindProperty("gestureRecognizer");
            if (recognizerProp.objectReferenceValue == null)
            {
                Debug.LogError("❌ GestureDrawingManager: GestureRecognizer NOT assigned!");
                allGood = false;
            }
            else
            {
                Debug.Log($"✅ GestureDrawingManager: GestureRecognizer assigned");
            }
            
            SerializedProperty casterProp = so.FindProperty("spellCaster");
            if (casterProp.objectReferenceValue == null)
            {
                Debug.LogError("❌ GestureDrawingManager: SpellCaster NOT assigned!");
                allGood = false;
            }
            else
            {
                Debug.Log($"✅ GestureDrawingManager: SpellCaster assigned ({casterProp.objectReferenceValue.name})");
            }
        }

        GestureRecognizer recognizer = FindObjectOfType<GestureRecognizer>();
        if (recognizer == null)
        {
            Debug.LogError("❌ CRITICAL: No GestureRecognizer found in scene!");
            allGood = false;
        }
        else
        {
            Debug.Log("✅ GestureRecognizer found");
            
            SerializedObject so = new SerializedObject(recognizer);
            SerializedProperty spellsProp = so.FindProperty("availableSpells");
            
            if (spellsProp.arraySize == 0)
            {
                Debug.LogError("❌ GestureRecognizer: NO SPELLS assigned in availableSpells list!");
                Debug.LogWarning("   → Select GestureManager, set Available Spells size to 1+, drag SpellData assets");
                allGood = false;
            }
            else
            {
                Debug.Log($"✅ GestureRecognizer: {spellsProp.arraySize} spell(s) assigned");
                
                for (int i = 0; i < spellsProp.arraySize; i++)
                {
                    SerializedProperty spellProp = spellsProp.GetArrayElementAtIndex(i);
                    SpellData spell = spellProp.objectReferenceValue as SpellData;
                    
                    if (spell == null)
                    {
                        Debug.LogError($"❌ GestureRecognizer: Spell slot {i} is NULL!");
                        allGood = false;
                    }
                    else
                    {
                        Debug.Log($"  Checking Spell {i}: '{spell.spellName}' (ID: {spell.spellID})");
                        
                        if (spell.gestureTemplate == null || spell.gestureTemplate.Count == 0)
                        {
                            Debug.LogError($"    ❌ Spell '{spell.spellName}' has NO TEMPLATE! Generate one using the inspector.");
                            allGood = false;
                        }
                        else
                        {
                            Debug.Log($"    ✅ Template: {spell.gestureTemplate.Count} points");
                        }
                        
                        if (spell.spellEffectPrefab == null)
                        {
                            Debug.LogWarning($"    ⚠️ Spell '{spell.spellName}' has NO PREFAB assigned!");
                            allGood = false;
                        }
                        else
                        {
                            Debug.Log($"    ✅ Prefab: {spell.spellEffectPrefab.name}");
                        }
                        
                        Debug.Log($"    Tolerance: {spell.recognitionTolerance:F2}");
                        Debug.Log($"    Enforce Speed: {spell.enforceSpeed} {(spell.enforceSpeed ? $"[{spell.expectedSpeedRange.x}-{spell.expectedSpeedRange.y}]" : "")}");
                        Debug.Log($"    Enforce Direction: {spell.enforceDirection} {(spell.enforceDirection ? $"[{spell.expectedDirection}]" : "")}");
                    }
                }
            }
        }

        SpellCaster caster = FindObjectOfType<SpellCaster>();
        if (caster == null)
        {
            Debug.LogError("❌ CRITICAL: No SpellCaster found in scene!");
            Debug.LogWarning("   → Add SpellCaster component to Player1");
            allGood = false;
        }
        else
        {
            Debug.Log($"✅ SpellCaster found on '{caster.gameObject.name}'");
            
            SerializedObject so = new SerializedObject(caster);
            
            SerializedProperty spawnProp = so.FindProperty("spellSpawnPoint");
            if (spawnProp.objectReferenceValue == null)
            {
                Debug.LogError("❌ SpellCaster: SpellSpawnPoint NOT assigned!");
                Debug.LogWarning("   → Create empty child under Player1, name it 'SpellSpawnPoint', assign it");
                allGood = false;
            }
            else
            {
                Debug.Log($"✅ SpellCaster: SpellSpawnPoint assigned ({spawnProp.objectReferenceValue.name})");
            }
            
            SerializedProperty targetProp = so.FindProperty("targetOpponent");
            if (targetProp.objectReferenceValue == null)
            {
                Debug.LogWarning("⚠️ SpellCaster: TargetOpponent NOT assigned (projectiles won't aim)");
            }
            else
            {
                Debug.Log($"✅ SpellCaster: TargetOpponent assigned ({targetProp.objectReferenceValue.name})");
            }
            
            SerializedProperty managerProp = so.FindProperty("gestureDrawingManager");
            if (managerProp.objectReferenceValue == null)
            {
                Debug.LogWarning("⚠️ SpellCaster: GestureDrawingManager NOT assigned (drawings won't clear)");
            }
            else
            {
                Debug.Log($"✅ SpellCaster: GestureDrawingManager assigned");
            }
        }

        Debug.Log("=== GESTURE SYSTEM DIAGNOSTICS END ===");
        
        if (allGood)
        {
            Debug.Log("<color=green>🎉 ALL CHECKS PASSED! System should be working!</color>");
            Debug.Log("<color=yellow>NEXT: Press Play and draw a circle to test!</color>");
        }
        else
        {
            Debug.LogError("<color=red>❌ SETUP INCOMPLETE! Fix the errors above, then run diagnostics again.</color>");
            Debug.LogWarning("📖 See CRITICAL_SETUP_FIX.md in /Assets/Scripts/ for step-by-step instructions!");
        }
    }
}
