using UnityEngine;
using UnityEditor;

public class MatchManagerSetupTool : EditorWindow
{
    [MenuItem("GameObject/Arcanum Draw/Create Match Manager", false, 10)]
    public static void CreateMatchManagerFromMenu()
    {
        CreateMatchManager();
    }
    
    [MenuItem("Tools/Arcanum Draw/Setup Match Manager")]
    public static void ShowWindow()
    {
        GetWindow<MatchManagerSetupTool>("Match Manager Setup");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Match Manager Auto Setup", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        EditorGUILayout.HelpBox(
            "This tool will automatically create and configure a MatchManager with:\n" +
            "• Match state machine\n" +
            "• Player references (auto-detected)\n" +
            "• Default match settings\n" +
            "• Event system integration",
            MessageType.Info
        );
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Create Match Manager", GUILayout.Height(40)))
        {
            CreateMatchManager();
        }
        
        GUILayout.Space(10);
        
        EditorGUILayout.HelpBox(
            "After creation, the tool will automatically:\n" +
            "• Find Player1 and Player2 GameObjects\n" +
            "• Add PlayerStats components if missing\n" +
            "• Link player references\n" +
            "• Configure default settings",
            MessageType.Info
        );
    }
    
    private static void CreateMatchManager()
    {
        MatchManager existingManager = FindObjectOfType<MatchManager>();
        if (existingManager != null)
        {
            if (!EditorUtility.DisplayDialog(
                "Match Manager Exists",
                "A MatchManager already exists in the scene. Create another one anyway?\n\n" +
                "Note: Only one MatchManager should exist at a time.",
                "Yes, Create New",
                "No, Cancel"))
            {
                Selection.activeGameObject = existingManager.gameObject;
                EditorGUIUtility.PingObject(existingManager.gameObject);
                return;
            }
        }
        
        GameObject managerGO = new GameObject("MatchManager");
        MatchManager manager = managerGO.AddComponent<MatchManager>();
        
        Undo.RegisterCreatedObjectUndo(managerGO, "Create Match Manager");
        
        AutoConfigureMatchManager(manager);
        
        Selection.activeGameObject = managerGO;
        EditorGUIUtility.PingObject(managerGO);
        
        Debug.Log("<color=green>✓ MatchManager created successfully!</color>");
    }
    
    private static void AutoConfigureMatchManager(MatchManager manager)
    {
        SerializedObject so = new SerializedObject(manager);
        
        so.FindProperty("matchStartDelay").floatValue = 3f;
        so.FindProperty("matchTimeLimit").floatValue = 300f;
        so.FindProperty("useTimeLimit").boolValue = false;
        
        GameObject player1 = GameObject.Find("Player1");
        GameObject player2 = GameObject.Find("Player2");
        
        if (player1 != null)
        {
            PlayerStats stats1 = player1.GetComponent<PlayerStats>();
            if (stats1 == null)
            {
                stats1 = player1.AddComponent<PlayerStats>();
                Debug.Log($"<color=cyan>✓ Added PlayerStats to {player1.name}</color>");
            }
            
            so.FindProperty("player1Stats").objectReferenceValue = stats1;
            
            PlayerUIController ui1 = FindPlayerUIController(player1);
            if (ui1 != null)
            {
                so.FindProperty("player1UI").objectReferenceValue = ui1;
                
                SerializedObject uiSO = new SerializedObject(ui1);
                uiSO.FindProperty("playerStats").objectReferenceValue = stats1;
                uiSO.ApplyModifiedProperties();
                EditorUtility.SetDirty(ui1);
            }
            
            if (!player1.CompareTag("Player"))
            {
                player1.tag = "Player";
                Debug.Log($"<color=yellow>✓ Set {player1.name} tag to 'Player'</color>");
            }
        }
        else
        {
            Debug.LogWarning("Player1 not found in scene. Please create it or assign manually.");
        }
        
        if (player2 != null)
        {
            PlayerStats stats2 = player2.GetComponent<PlayerStats>();
            if (stats2 == null)
            {
                stats2 = player2.AddComponent<PlayerStats>();
                Debug.Log($"<color=cyan>✓ Added PlayerStats to {player2.name}</color>");
            }
            
            so.FindProperty("player2Stats").objectReferenceValue = stats2;
            
            PlayerUIController ui2 = FindPlayerUIController(player2);
            if (ui2 != null)
            {
                so.FindProperty("player2UI").objectReferenceValue = ui2;
                
                SerializedObject uiSO = new SerializedObject(ui2);
                uiSO.FindProperty("playerStats").objectReferenceValue = stats2;
                uiSO.ApplyModifiedProperties();
                EditorUtility.SetDirty(ui2);
            }
            
            if (!player2.CompareTag("Player"))
            {
                player2.tag = "Player";
                Debug.Log($"<color=yellow>✓ Set {player2.name} tag to 'Player'</color>");
            }
        }
        else
        {
            Debug.LogWarning("Player2 not found in scene. Please create it or assign manually.");
        }
        
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(manager);
        
        Debug.Log("<color=green>✓ MatchManager configured with default settings!</color>");
        
        if (player1 != null && player2 != null)
        {
            Debug.Log("<color=green>✓ Player references automatically assigned!</color>");
        }
    }
    
    private static PlayerUIController FindPlayerUIController(GameObject player)
    {
        PlayerUIController ui = player.GetComponent<PlayerUIController>();
        if (ui != null) return ui;
        
        ui = player.GetComponentInChildren<PlayerUIController>();
        if (ui != null) return ui;
        
        ui = FindObjectOfType<PlayerUIController>();
        return ui;
    }
}
