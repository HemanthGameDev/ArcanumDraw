using UnityEngine;
using UnityEditor;

public class CompleteMatchSetupWizard : EditorWindow
{
    private enum SetupStep
    {
        Welcome,
        PlayerSetup,
        MatchManagerSetup,
        HUDSetup,
        Complete
    }
    
    private SetupStep currentStep = SetupStep.Welcome;
    private bool autoSetupPlayers = true;
    private bool autoSetupMatchManager = true;
    private bool autoSetupHUD = true;
    
    [MenuItem("Tools/Arcanum Draw/Complete Match Setup Wizard", priority = 1)]
    public static void ShowWizard()
    {
        CompleteMatchSetupWizard window = GetWindow<CompleteMatchSetupWizard>("Match Setup Wizard");
        window.minSize = new Vector2(500, 400);
        window.Show();
    }
    
    private void OnGUI()
    {
        DrawHeader();
        
        GUILayout.Space(20);
        
        switch (currentStep)
        {
            case SetupStep.Welcome:
                DrawWelcomeStep();
                break;
            case SetupStep.PlayerSetup:
                DrawPlayerSetupStep();
                break;
            case SetupStep.MatchManagerSetup:
                DrawMatchManagerSetupStep();
                break;
            case SetupStep.HUDSetup:
                DrawHUDSetupStep();
                break;
            case SetupStep.Complete:
                DrawCompleteStep();
                break;
        }
        
        GUILayout.FlexibleSpace();
        DrawNavigationButtons();
    }
    
    private void DrawHeader()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("🎮 Arcanum Draw - Match Setup Wizard", EditorStyles.boldLabel);
        GUILayout.Label($"Step {(int)currentStep + 1} of 5: {currentStep}", EditorStyles.miniLabel);
        EditorGUILayout.EndVertical();
    }
    
    private void DrawWelcomeStep()
    {
        GUILayout.Label("Welcome to the Match Setup Wizard!", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        EditorGUILayout.HelpBox(
            "This wizard will help you set up the complete match system for your game.\n\n" +
            "What will be configured:\n" +
            "• Player GameObjects with PlayerStats components\n" +
            "• MatchManager with state machine\n" +
            "• Match HUD with health bars and UI\n" +
            "• All references automatically linked\n\n" +
            "This will take about 1 minute to complete.",
            MessageType.Info
        );
        
        GUILayout.Space(20);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("Prerequisites:", EditorStyles.boldLabel);
        GUILayout.Label("✓ Unity project with URP");
        GUILayout.Label("✓ TextMeshPro imported");
        GUILayout.Label("✓ Player1 and Player2 GameObjects in scene (recommended)");
        EditorGUILayout.EndVertical();
        
        GUILayout.Space(20);
        
        autoSetupPlayers = EditorGUILayout.Toggle("Auto-setup Players", autoSetupPlayers);
        autoSetupMatchManager = EditorGUILayout.Toggle("Auto-setup Match Manager", autoSetupMatchManager);
        autoSetupHUD = EditorGUILayout.Toggle("Auto-setup Match HUD", autoSetupHUD);
    }
    
    private void DrawPlayerSetupStep()
    {
        GUILayout.Label("Player Setup", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        GameObject player1 = GameObject.Find("Player1");
        GameObject player2 = GameObject.Find("Player2");
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("Player Detection:", EditorStyles.boldLabel);
        
        DrawPlayerStatus("Player1", player1);
        DrawPlayerStatus("Player2", player2);
        
        EditorGUILayout.EndVertical();
        
        GUILayout.Space(10);
        
        if (autoSetupPlayers)
        {
            EditorGUILayout.HelpBox(
                "The wizard will automatically:\n" +
                "• Add PlayerStats components to both players\n" +
                "• Set player tags to 'Player'\n" +
                "• Configure default health values (100 HP)",
                MessageType.Info
            );
            
            if (player1 == null || player2 == null)
            {
                EditorGUILayout.HelpBox(
                    "⚠️ One or both players not found!\n" +
                    "Please create Player1 and Player2 GameObjects before continuing.",
                    MessageType.Warning
                );
            }
        }
        else
        {
            EditorGUILayout.HelpBox(
                "You've chosen to skip automatic player setup.\n" +
                "You'll need to manually add PlayerStats components.",
                MessageType.Info
            );
        }
    }
    
    private void DrawMatchManagerSetupStep()
    {
        GUILayout.Label("Match Manager Setup", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        MatchManager existingManager = FindObjectOfType<MatchManager>();
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        if (existingManager != null)
        {
            EditorGUILayout.HelpBox(
                $"✓ MatchManager found: {existingManager.gameObject.name}\n" +
                "The wizard will update existing configuration.",
                MessageType.Info
            );
        }
        else
        {
            EditorGUILayout.HelpBox(
                "No MatchManager found in scene.\n" +
                "A new MatchManager will be created.",
                MessageType.Info
            );
        }
        EditorGUILayout.EndVertical();
        
        GUILayout.Space(10);
        
        if (autoSetupMatchManager)
        {
            EditorGUILayout.HelpBox(
                "Match Manager Configuration:\n" +
                "• Match start delay: 3 seconds\n" +
                "• Time limit: 300 seconds (optional)\n" +
                "• Victory conditions: HP reaches 0\n" +
                "• Player references auto-linked",
                MessageType.Info
            );
        }
    }
    
    private void DrawHUDSetupStep()
    {
        GUILayout.Label("Match HUD Setup", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        Canvas existingCanvas = FindObjectOfType<Canvas>();
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        if (existingCanvas != null)
        {
            EditorGUILayout.HelpBox(
                $"✓ Canvas found: {existingCanvas.gameObject.name}\n" +
                "A new MatchHUDCanvas will be created separately.",
                MessageType.Info
            );
        }
        else
        {
            EditorGUILayout.HelpBox(
                "No Canvas found in scene.\n" +
                "A new Canvas will be created with proper settings.",
                MessageType.Info
            );
        }
        EditorGUILayout.EndVertical();
        
        GUILayout.Space(10);
        
        if (autoSetupHUD)
        {
            EditorGUILayout.HelpBox(
                "Match HUD will include:\n" +
                "• Match state display (Ready, Fight!, Victory)\n" +
                "• Match timer\n" +
                "• Player 1 health bar (top-left)\n" +
                "• Player 2 health bar (top-right)\n" +
                "• Victory panel with winner announcement\n" +
                "• All references auto-linked",
                MessageType.Info
            );
        }
    }
    
    private void DrawCompleteStep()
    {
        GUILayout.Label("✓ Setup Complete!", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        EditorGUILayout.HelpBox(
            "Your match system is now fully configured!\n\n" +
            "Next steps:\n" +
            "1. Press Play to test the match flow\n" +
            "2. Watch the console for match state changes\n" +
            "3. Cast spells to test damage and victory\n\n" +
            "Check the documentation files:\n" +
            "• PHASE_1_1_IMPLEMENTATION_GUIDE.md\n" +
            "• QUICK_SETUP_CHECKLIST.md",
            MessageType.Info
        );
        
        GUILayout.Space(20);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("Quick Test Instructions:", EditorStyles.boldLabel);
        GUILayout.Label("1. Enter Play Mode");
        GUILayout.Label("2. Wait for 'MATCH STARTED!' message");
        GUILayout.Label("3. Draw a gesture to cast a spell");
        GUILayout.Label("4. Watch health bars decrease");
        GUILayout.Label("5. Victory message appears at 0 HP");
        EditorGUILayout.EndVertical();
        
        GUILayout.Space(20);
        
        if (GUILayout.Button("Open Scene View", GUILayout.Height(30)))
        {
            EditorApplication.ExecuteMenuItem("Window/General/Scene");
        }
        
        if (GUILayout.Button("View MatchManager", GUILayout.Height(30)))
        {
            MatchManager manager = FindObjectOfType<MatchManager>();
            if (manager != null)
            {
                Selection.activeGameObject = manager.gameObject;
                EditorGUIUtility.PingObject(manager.gameObject);
            }
        }
    }
    
    private void DrawNavigationButtons()
    {
        EditorGUILayout.BeginHorizontal();
        
        GUI.enabled = currentStep > SetupStep.Welcome;
        if (GUILayout.Button("← Back", GUILayout.Height(30)))
        {
            currentStep--;
        }
        GUI.enabled = true;
        
        GUILayout.FlexibleSpace();
        
        if (currentStep < SetupStep.Complete)
        {
            if (GUILayout.Button("Next →", GUILayout.Height(30), GUILayout.Width(100)))
            {
                PerformSetupForCurrentStep();
                currentStep++;
            }
            
            if (GUILayout.Button("Complete Setup Now", GUILayout.Height(30), GUILayout.Width(150)))
            {
                PerformCompleteSetup();
            }
        }
        else
        {
            if (GUILayout.Button("Close", GUILayout.Height(30), GUILayout.Width(100)))
            {
                Close();
            }
        }
        
        EditorGUILayout.EndHorizontal();
    }
    
    private void PerformSetupForCurrentStep()
    {
        switch (currentStep)
        {
            case SetupStep.PlayerSetup:
                if (autoSetupPlayers) SetupPlayers();
                break;
            case SetupStep.MatchManagerSetup:
                if (autoSetupMatchManager) SetupMatchManager();
                break;
            case SetupStep.HUDSetup:
                if (autoSetupHUD) SetupMatchHUD();
                break;
        }
    }
    
    private void PerformCompleteSetup()
    {
        Debug.Log("<color=cyan>========== Starting Complete Match Setup ==========</color>");
        
        if (autoSetupPlayers) SetupPlayers();
        if (autoSetupMatchManager) SetupMatchManager();
        if (autoSetupHUD) SetupMatchHUD();
        
        currentStep = SetupStep.Complete;
        
        Debug.Log("<color=green>========== Match Setup Complete! ==========</color>");
        
        EditorUtility.DisplayDialog(
            "Setup Complete!",
            "Your match system is now fully configured!\n\n" +
            "Press Play to test the match flow.",
            "OK"
        );
    }
    
    private void SetupPlayers()
    {
        GameObject player1 = GameObject.Find("Player1");
        GameObject player2 = GameObject.Find("Player2");
        
        if (player1 == null || player2 == null)
        {
            Debug.LogError("Player1 and/or Player2 not found in scene! Please create them first.");
            EditorUtility.DisplayDialog(
                "Players Not Found",
                "Player1 and/or Player2 GameObjects were not found in the scene.\n\n" +
                "Please create them before running the setup.",
                "OK"
            );
            return;
        }
        
        SetupPlayer(player1, "Player1");
        SetupPlayer(player2, "Player2");
        
        Debug.Log("<color=green>✓ Player setup complete!</color>");
    }
    
    private void SetupPlayer(GameObject player, string playerName)
    {
        PlayerStats stats = player.GetComponent<PlayerStats>();
        if (stats == null)
        {
            stats = Undo.AddComponent<PlayerStats>(player);
            Debug.Log($"<color=cyan>✓ Added PlayerStats to {playerName}</color>");
        }
        
        if (!player.CompareTag("Player"))
        {
            Undo.RecordObject(player, "Set Player Tag");
            player.tag = "Player";
            Debug.Log($"<color=yellow>✓ Set {playerName} tag to 'Player'</color>");
        }
        
        PlayerUIController uiController = FindPlayerUIController(player);
        if (uiController != null)
        {
            SerializedObject uiSO = new SerializedObject(uiController);
            uiSO.FindProperty("playerStats").objectReferenceValue = stats;
            uiSO.ApplyModifiedProperties();
            EditorUtility.SetDirty(uiController);
        }
    }
    
    private void SetupMatchManager()
    {
        MatchManager manager = FindObjectOfType<MatchManager>();
        
        if (manager == null)
        {
            GameObject managerGO = new GameObject("MatchManager");
            manager = Undo.AddComponent<MatchManager>(managerGO);
            Debug.Log("<color=green>✓ Created new MatchManager</color>");
        }
        
        SerializedObject so = new SerializedObject(manager);
        
        so.FindProperty("matchStartDelay").floatValue = 3f;
        so.FindProperty("matchTimeLimit").floatValue = 300f;
        so.FindProperty("useTimeLimit").boolValue = false;
        
        GameObject player1 = GameObject.Find("Player1");
        GameObject player2 = GameObject.Find("Player2");
        
        if (player1 != null)
        {
            PlayerStats stats1 = player1.GetComponent<PlayerStats>();
            so.FindProperty("player1Stats").objectReferenceValue = stats1;
            
            PlayerUIController ui1 = FindPlayerUIController(player1);
            if (ui1 != null)
            {
                so.FindProperty("player1UI").objectReferenceValue = ui1;
            }
        }
        
        if (player2 != null)
        {
            PlayerStats stats2 = player2.GetComponent<PlayerStats>();
            so.FindProperty("player2Stats").objectReferenceValue = stats2;
            
            PlayerUIController ui2 = FindPlayerUIController(player2);
            if (ui2 != null)
            {
                so.FindProperty("player2UI").objectReferenceValue = ui2;
            }
        }
        
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(manager);
        
        Debug.Log("<color=green>✓ MatchManager configured!</color>");
    }
    
    private void SetupMatchHUD()
    {
        Debug.Log("<color=cyan>Creating Match HUD...</color>");
        EditorApplication.ExecuteMenuItem("GameObject/Arcanum Draw/Create Match HUD");
        Debug.Log("<color=green>✓ Match HUD created!</color>");
    }
    
    private void DrawPlayerStatus(string playerName, GameObject player)
    {
        EditorGUILayout.BeginHorizontal();
        
        if (player != null)
        {
            GUILayout.Label("✓", GUILayout.Width(20));
            GUILayout.Label(playerName, GUILayout.Width(80));
            
            PlayerStats stats = player.GetComponent<PlayerStats>();
            if (stats != null)
            {
                GUILayout.Label("(has PlayerStats)", EditorStyles.miniLabel);
            }
            else
            {
                GUILayout.Label("(missing PlayerStats)", EditorStyles.miniLabel);
            }
        }
        else
        {
            GUILayout.Label("✗", GUILayout.Width(20));
            GUILayout.Label(playerName, GUILayout.Width(80));
            GUILayout.Label("(not found)", EditorStyles.miniLabel);
        }
        
        EditorGUILayout.EndHorizontal();
    }
    
    private PlayerUIController FindPlayerUIController(GameObject player)
    {
        PlayerUIController ui = player.GetComponent<PlayerUIController>();
        if (ui != null) return ui;
        
        ui = player.GetComponentInChildren<PlayerUIController>();
        if (ui != null) return ui;
        
        ui = FindObjectOfType<PlayerUIController>();
        return ui;
    }
}
