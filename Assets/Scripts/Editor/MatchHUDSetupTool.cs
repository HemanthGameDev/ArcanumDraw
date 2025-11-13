using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

public class MatchHUDSetupTool : EditorWindow
{
    private MatchManager matchManager;
    private bool createMatchManager = true;
    private bool autoAssignPlayers = true;
    
    [MenuItem("Tools/Arcanum Draw/Setup Match HUD")]
    public static void ShowWindow()
    {
        MatchHUDSetupTool window = GetWindow<MatchHUDSetupTool>("Match HUD Setup");
        window.minSize = new Vector2(400, 250);
        window.Show();
    }
    
    private void OnGUI()
    {
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Match HUD Auto-Setup Tool", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("This tool will automatically create a complete Match HUD Canvas with all UI elements configured and wired up.", MessageType.Info);
        
        GUILayout.Space(10);
        
        EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);
        
        createMatchManager = EditorGUILayout.Toggle("Create MatchManager", createMatchManager);
        if (!createMatchManager)
        {
            matchManager = (MatchManager)EditorGUILayout.ObjectField("Match Manager", matchManager, typeof(MatchManager), true);
        }
        
        autoAssignPlayers = EditorGUILayout.Toggle("Auto-Assign Players", autoAssignPlayers);
        
        GUILayout.Space(20);
        
        if (GUILayout.Button("Create Match HUD", GUILayout.Height(40)))
        {
            CreateMatchHUD();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Setup Player Stats Only", GUILayout.Height(30)))
        {
            SetupPlayerStats();
        }
    }
    
    private void CreateMatchHUD()
    {
        GameObject canvasObj = CreateCanvas();
        GameObject hudObj = CreateHUDStructure(canvasObj.transform);
        MatchHUD matchHUD = SetupMatchHUDComponent(hudObj, canvasObj.transform);
        
        if (createMatchManager || matchManager == null)
        {
            matchManager = CreateMatchManagerObject();
        }
        
        if (autoAssignPlayers)
        {
            AutoAssignPlayers();
        }
        
        ConnectMatchHUDToManager(matchHUD);
        
        Selection.activeGameObject = canvasObj;
        
        EditorUtility.DisplayDialog("Success!", 
            "Match HUD created successfully!\n\n" +
            "✓ Canvas with UI elements\n" +
            "✓ MatchHUD component configured\n" +
            (createMatchManager ? "✓ MatchManager created\n" : "") +
            (autoAssignPlayers ? "✓ Players auto-assigned\n" : "") +
            "\nCheck the Inspector to verify all references.", 
            "OK");
        
        Debug.Log("<color=green>✅ Match HUD setup complete!</color>");
    }
    
    private GameObject CreateCanvas()
    {
        GameObject canvasObj = new GameObject("MatchHUDCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
        
        Undo.RegisterCreatedObjectUndo(canvasObj, "Create Match HUD Canvas");
        
        return canvasObj;
    }
    
    private GameObject CreateHUDStructure(Transform canvasTransform)
    {
        GameObject hudRoot = new GameObject("MatchHUD");
        hudRoot.transform.SetParent(canvasTransform, false);
        
        RectTransform hudRect = hudRoot.AddComponent<RectTransform>();
        hudRect.anchorMin = Vector2.zero;
        hudRect.anchorMax = Vector2.one;
        hudRect.sizeDelta = Vector2.zero;
        
        CreateTopBar(hudRoot.transform);
        CreatePlayerHealthBars(hudRoot.transform);
        CreateVictoryPanel(hudRoot.transform);
        
        return hudRoot;
    }
    
    private void CreateTopBar(Transform parent)
    {
        GameObject topBar = new GameObject("TopBar");
        topBar.transform.SetParent(parent, false);
        RectTransform rect = topBar.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(0.5f, 1);
        rect.sizeDelta = new Vector2(0, 80);
        rect.anchoredPosition = Vector2.zero;
        
        Image bg = topBar.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.5f);
        
        GameObject matchStateText = CreateTextTMP("MatchStateText", topBar.transform);
        RectTransform stateRect = matchStateText.GetComponent<RectTransform>();
        stateRect.anchorMin = new Vector2(0.5f, 0.5f);
        stateRect.anchorMax = new Vector2(0.5f, 0.5f);
        stateRect.sizeDelta = new Vector2(400, 60);
        stateRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI stateTMP = matchStateText.GetComponent<TextMeshProUGUI>();
        stateTMP.text = "MATCH READY";
        stateTMP.fontSize = 48;
        stateTMP.fontStyle = FontStyles.Bold;
        stateTMP.alignment = TextAlignmentOptions.Center;
        stateTMP.color = Color.white;
        
        GameObject timerText = CreateTextTMP("MatchTimerText", topBar.transform);
        RectTransform timerRect = timerText.GetComponent<RectTransform>();
        timerRect.anchorMin = new Vector2(1, 0.5f);
        timerRect.anchorMax = new Vector2(1, 0.5f);
        timerRect.pivot = new Vector2(1, 0.5f);
        timerRect.sizeDelta = new Vector2(200, 50);
        timerRect.anchoredPosition = new Vector2(-20, 0);
        
        TextMeshProUGUI timerTMP = timerText.GetComponent<TextMeshProUGUI>();
        timerTMP.text = "00:00";
        timerTMP.fontSize = 36;
        timerTMP.alignment = TextAlignmentOptions.Right;
        timerTMP.color = Color.yellow;
    }
    
    private void CreatePlayerHealthBars(Transform parent)
    {
        GameObject healthBarsContainer = new GameObject("HealthBarsContainer");
        healthBarsContainer.transform.SetParent(parent, false);
        RectTransform containerRect = healthBarsContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0, 1);
        containerRect.anchorMax = new Vector2(1, 1);
        containerRect.pivot = new Vector2(0.5f, 1);
        containerRect.sizeDelta = new Vector2(0, 100);
        containerRect.anchoredPosition = new Vector2(0, -90);
        
        CreatePlayerHealthBar("Player1HealthBar", healthBarsContainer.transform, true);
        CreatePlayerHealthBar("Player2HealthBar", healthBarsContainer.transform, false);
    }
    
    private void CreatePlayerHealthBar(string name, Transform parent, bool isLeftSide)
    {
        GameObject healthBarObj = new GameObject(name);
        healthBarObj.transform.SetParent(parent, false);
        RectTransform rect = healthBarObj.AddComponent<RectTransform>();
        
        float xPos = isLeftSide ? 50 : -50;
        float anchorX = isLeftSide ? 0 : 1;
        float pivotX = isLeftSide ? 0 : 1;
        
        rect.anchorMin = new Vector2(anchorX, 0.5f);
        rect.anchorMax = new Vector2(anchorX, 0.5f);
        rect.pivot = new Vector2(pivotX, 0.5f);
        rect.sizeDelta = new Vector2(400, 80);
        rect.anchoredPosition = new Vector2(xPos, 0);
        
        GameObject nameTextObj = CreateTextTMP(name + "_Name", healthBarObj.transform);
        RectTransform nameRect = nameTextObj.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 1);
        nameRect.anchorMax = new Vector2(1, 1);
        nameRect.pivot = new Vector2(0.5f, 1);
        nameRect.sizeDelta = new Vector2(0, 30);
        nameRect.anchoredPosition = new Vector2(0, 0);
        
        TextMeshProUGUI nameTMP = nameTextObj.GetComponent<TextMeshProUGUI>();
        nameTMP.text = isLeftSide ? "Player 1" : "Player 2";
        nameTMP.fontSize = 24;
        nameTMP.fontStyle = FontStyles.Bold;
        nameTMP.alignment = isLeftSide ? TextAlignmentOptions.Left : TextAlignmentOptions.Right;
        nameTMP.color = isLeftSide ? new Color(0.3f, 0.6f, 1f) : new Color(1f, 0.4f, 0.4f);
        
        GameObject sliderObj = new GameObject(name + "_Slider");
        sliderObj.transform.SetParent(healthBarObj.transform, false);
        RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0, 0);
        sliderRect.anchorMax = new Vector2(1, 0);
        sliderRect.pivot = new Vector2(0.5f, 0);
        sliderRect.sizeDelta = new Vector2(0, 40);
        sliderRect.anchoredPosition = new Vector2(0, 5);
        
        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = 0;
        slider.maxValue = 100;
        slider.value = 100;
        slider.transition = Selectable.Transition.None;
        
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(sliderObj.transform, false);
        RectTransform bgRect = bg.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        Image bgImage = bg.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.sizeDelta = new Vector2(-10, -10);
        
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRect = fill.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = isLeftSide ? new Color(0.2f, 0.8f, 0.3f) : new Color(1f, 0.3f, 0.3f);
        
        slider.fillRect = fillRect;
        slider.targetGraphic = fillImage;
        
        GameObject healthTextObj = CreateTextTMP(name + "_Text", healthBarObj.transform);
        RectTransform healthTextRect = healthTextObj.GetComponent<RectTransform>();
        healthTextRect.anchorMin = new Vector2(0, 0);
        healthTextRect.anchorMax = new Vector2(1, 0);
        healthTextRect.pivot = new Vector2(0.5f, 0);
        healthTextRect.sizeDelta = new Vector2(0, 30);
        healthTextRect.anchoredPosition = new Vector2(0, 10);
        
        TextMeshProUGUI healthTMP = healthTextObj.GetComponent<TextMeshProUGUI>();
        healthTMP.text = "100/100";
        healthTMP.fontSize = 20;
        healthTMP.alignment = TextAlignmentOptions.Center;
        healthTMP.color = Color.white;
    }
    
    private void CreateVictoryPanel(Transform parent)
    {
        GameObject victoryPanel = new GameObject("VictoryPanel");
        victoryPanel.transform.SetParent(parent, false);
        RectTransform rect = victoryPanel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        
        Image panelImage = victoryPanel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.8f);
        
        victoryPanel.SetActive(false);
        
        GameObject victoryText = CreateTextTMP("VictoryText", victoryPanel.transform);
        RectTransform textRect = victoryText.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.sizeDelta = new Vector2(800, 200);
        textRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI victoryTMP = victoryText.GetComponent<TextMeshProUGUI>();
        victoryTMP.text = "VICTORY!";
        victoryTMP.fontSize = 100;
        victoryTMP.fontStyle = FontStyles.Bold;
        victoryTMP.alignment = TextAlignmentOptions.Center;
        victoryTMP.color = Color.yellow;
        
        Outline outline = victoryText.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(5, -5);
    }
    
    private GameObject CreateTextTMP(string name, Transform parent)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.raycastTarget = false;
        return textObj;
    }
    
    private MatchHUD SetupMatchHUDComponent(GameObject hudObj, Transform canvasTransform)
    {
        MatchHUD matchHUD = hudObj.AddComponent<MatchHUD>();
        
        SerializedObject so = new SerializedObject(matchHUD);
        
        so.FindProperty("matchStateText").objectReferenceValue = 
            hudObj.transform.Find("TopBar/MatchStateText")?.GetComponent<TextMeshProUGUI>();
        
        so.FindProperty("matchTimerText").objectReferenceValue = 
            hudObj.transform.Find("TopBar/MatchTimerText")?.GetComponent<TextMeshProUGUI>();
        
        so.FindProperty("victoryText").objectReferenceValue = 
            hudObj.transform.Find("VictoryPanel/VictoryText")?.GetComponent<TextMeshProUGUI>();
        
        so.FindProperty("victoryPanel").objectReferenceValue = 
            hudObj.transform.Find("VictoryPanel")?.gameObject;
        
        so.FindProperty("player1HealthBar").objectReferenceValue = 
            hudObj.transform.Find("HealthBarsContainer/Player1HealthBar/Player1HealthBar_Slider")?.GetComponent<Slider>();
        
        so.FindProperty("player2HealthBar").objectReferenceValue = 
            hudObj.transform.Find("HealthBarsContainer/Player2HealthBar/Player2HealthBar_Slider")?.GetComponent<Slider>();
        
        so.FindProperty("player1HealthText").objectReferenceValue = 
            hudObj.transform.Find("HealthBarsContainer/Player1HealthBar/Player1HealthBar_Text")?.GetComponent<TextMeshProUGUI>();
        
        so.FindProperty("player2HealthText").objectReferenceValue = 
            hudObj.transform.Find("HealthBarsContainer/Player2HealthBar/Player2HealthBar_Text")?.GetComponent<TextMeshProUGUI>();
        
        so.FindProperty("player1NameText").objectReferenceValue = 
            hudObj.transform.Find("HealthBarsContainer/Player1HealthBar/Player1HealthBar_Name")?.GetComponent<TextMeshProUGUI>();
        
        so.FindProperty("player2NameText").objectReferenceValue = 
            hudObj.transform.Find("HealthBarsContainer/Player2HealthBar/Player2HealthBar_Name")?.GetComponent<TextMeshProUGUI>();
        
        so.ApplyModifiedProperties();
        
        EditorUtility.SetDirty(matchHUD);
        
        return matchHUD;
    }
    
    private MatchManager CreateMatchManagerObject()
    {
        GameObject managerObj = new GameObject("MatchManager");
        MatchManager manager = managerObj.AddComponent<MatchManager>();
        
        Undo.RegisterCreatedObjectUndo(managerObj, "Create Match Manager");
        
        SerializedObject so = new SerializedObject(manager);
        so.FindProperty("matchStartDelay").floatValue = 3f;
        so.FindProperty("matchTimeLimit").floatValue = 300f;
        so.FindProperty("useTimeLimit").boolValue = false;
        so.ApplyModifiedProperties();
        
        EditorUtility.SetDirty(manager);
        
        Debug.Log("<color=cyan>✓ MatchManager created</color>");
        
        return manager;
    }
    
    private void AutoAssignPlayers()
    {
        GameObject player1 = GameObject.Find("Player1");
        GameObject player2 = GameObject.Find("Player2");
        
        if (player1 != null)
        {
            PlayerStats stats1 = player1.GetComponent<PlayerStats>();
            if (stats1 == null)
            {
                stats1 = Undo.AddComponent<PlayerStats>(player1);
                SerializedObject so = new SerializedObject(stats1);
                so.FindProperty("maxHealth").floatValue = 100f;
                so.FindProperty("currentHealth").floatValue = 100f;
                so.ApplyModifiedProperties();
                
                Debug.Log("<color=green>✓ Added PlayerStats to Player1</color>");
            }
            
            if (matchManager != null)
            {
                SerializedObject managerSO = new SerializedObject(matchManager);
                managerSO.FindProperty("player1Stats").objectReferenceValue = stats1;
                managerSO.ApplyModifiedProperties();
            }
        }
        
        if (player2 != null)
        {
            PlayerStats stats2 = player2.GetComponent<PlayerStats>();
            if (stats2 == null)
            {
                stats2 = Undo.AddComponent<PlayerStats>(player2);
                SerializedObject so = new SerializedObject(stats2);
                so.FindProperty("maxHealth").floatValue = 100f;
                so.FindProperty("currentHealth").floatValue = 100f;
                so.ApplyModifiedProperties();
                
                Debug.Log("<color=green>✓ Added PlayerStats to Player2</color>");
            }
            
            if (matchManager != null)
            {
                SerializedObject managerSO = new SerializedObject(matchManager);
                managerSO.FindProperty("player2Stats").objectReferenceValue = stats2;
                managerSO.ApplyModifiedProperties();
            }
        }
        
        PlayerUIController uiController = FindFirstObjectByType<PlayerUIController>();
        if (uiController != null && player1 != null)
        {
            PlayerStats stats1 = player1.GetComponent<PlayerStats>();
            if (stats1 != null)
            {
                SerializedObject uiSO = new SerializedObject(uiController);
                uiSO.FindProperty("playerStats").objectReferenceValue = stats1;
                uiSO.ApplyModifiedProperties();
                
                SerializedObject stats1SO = new SerializedObject(stats1);
                stats1SO.FindProperty("uiController").objectReferenceValue = uiController;
                stats1SO.ApplyModifiedProperties();
                
                Debug.Log("<color=green>✓ Connected Player1 stats to UI Controller</color>");
            }
        }
    }
    
    private void ConnectMatchHUDToManager(MatchHUD matchHUD)
    {
        if (matchManager != null && matchHUD != null)
        {
            SerializedObject hudSO = new SerializedObject(matchHUD);
            hudSO.FindProperty("matchManager").objectReferenceValue = matchManager;
            hudSO.ApplyModifiedProperties();
            
            EditorUtility.SetDirty(matchHUD);
            
            Debug.Log("<color=green>✓ Connected MatchHUD to MatchManager</color>");
        }
    }
    
    private void SetupPlayerStats()
    {
        GameObject player1 = GameObject.Find("Player1");
        GameObject player2 = GameObject.Find("Player2");
        
        int count = 0;
        
        if (player1 != null && player1.GetComponent<PlayerStats>() == null)
        {
            PlayerStats stats = Undo.AddComponent<PlayerStats>(player1);
            SerializedObject so = new SerializedObject(stats);
            so.FindProperty("maxHealth").floatValue = 100f;
            so.FindProperty("currentHealth").floatValue = 100f;
            so.ApplyModifiedProperties();
            count++;
        }
        
        if (player2 != null && player2.GetComponent<PlayerStats>() == null)
        {
            PlayerStats stats = Undo.AddComponent<PlayerStats>(player2);
            SerializedObject so = new SerializedObject(stats);
            so.FindProperty("maxHealth").floatValue = 100f;
            so.FindProperty("currentHealth").floatValue = 100f;
            so.ApplyModifiedProperties();
            count++;
        }
        
        if (count > 0)
        {
            EditorUtility.DisplayDialog("Success!", 
                $"Added PlayerStats to {count} player(s)!", 
                "OK");
            Debug.Log($"<color=green>✅ Added PlayerStats to {count} player(s)</color>");
        }
        else
        {
            EditorUtility.DisplayDialog("Info", 
                "Both players already have PlayerStats components.", 
                "OK");
        }
    }
}
