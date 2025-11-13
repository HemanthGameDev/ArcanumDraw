// This file has been moved to /Assets/Scripts/Editor/MatchHUDSetupTool.cs
// Please delete this file to avoid compilation errors.
// The correct implementation is in /Assets/Scripts/Editor/

#if false
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

public class MatchHUDSetupTool_DUPLICATE_DELETE_ME : EditorWindow
{
    [MenuItem("Tools/DELETE THIS DUPLICATE")]
    public static void SetupMatchHUD()
    {
        if (EditorUtility.DisplayDialog("Setup Match HUD", 
            "This will create a new Canvas with Match HUD UI elements.\n\nProceed?", 
            "Yes, Create", "Cancel"))
        {
            CreateMatchHUD();
        }
    }
    
    [MenuItem("GameObject/UI/DELETE DUPLICATE", false, 10)]
    public static void CreateMatchHUDFromGameObjectMenu()
    {
        CreateMatchHUD();
    }
    
    private static void CreateMatchHUD()
    {
        Canvas existingCanvas = FindObjectOfType<Canvas>();
        Canvas canvas;
        
        if (existingCanvas != null)
        {
            bool useExisting = EditorUtility.DisplayDialog("Canvas Found", 
                "A Canvas already exists in the scene. Use existing Canvas?", 
                "Use Existing", "Create New");
            
            if (useExisting)
            {
                canvas = existingCanvas;
            }
            else
            {
                canvas = CreateNewCanvas("MatchHUD Canvas");
            }
        }
        else
        {
            canvas = CreateNewCanvas("MatchHUD Canvas");
        }
        
        GameObject matchHUDRoot = new GameObject("MatchHUD");
        matchHUDRoot.transform.SetParent(canvas.transform, false);
        
        RectTransform rootRect = matchHUDRoot.AddComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.sizeDelta = Vector2.zero;
        rootRect.anchoredPosition = Vector2.zero;
        
        GameObject matchStateText = CreateText(matchHUDRoot, "MatchStateText", "PRE-MATCH", 
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -30), new Vector2(300, 60), 36);
        matchStateText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        matchStateText.GetComponent<TextMeshProUGUI>().color = Color.yellow;
        matchStateText.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
        
        GameObject matchTimerText = CreateText(matchHUDRoot, "MatchTimerText", "00:00", 
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-150, -30), new Vector2(140, 50), 32);
        matchTimerText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        matchTimerText.GetComponent<TextMeshProUGUI>().color = Color.white;
        
        GameObject playerHealthContainer = new GameObject("PlayerHealthBars");
        playerHealthContainer.transform.SetParent(matchHUDRoot.transform, false);
        RectTransform healthContainerRect = playerHealthContainer.AddComponent<RectTransform>();
        healthContainerRect.anchorMin = new Vector2(0f, 1f);
        healthContainerRect.anchorMax = new Vector2(1f, 1f);
        healthContainerRect.sizeDelta = new Vector2(0, 60);
        healthContainerRect.anchoredPosition = new Vector2(0, -80);
        
        GameObject player1Health = CreatePlayerHealthBar(playerHealthContainer, "Player1Health", 
            new Vector2(0f, 0.5f), new Vector2(0.45f, 0.5f), new Vector2(20, 0), "Player 1");
        
        GameObject player2Health = CreatePlayerHealthBar(playerHealthContainer, "Player2Health", 
            new Vector2(0.55f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-20, 0), "Player 2");
        
        GameObject victoryPanel = CreateVictoryPanel(matchHUDRoot);
        
        MatchHUD matchHUD = matchHUDRoot.AddComponent<MatchHUD>();
        
        SerializedObject so = new SerializedObject(matchHUD);
        so.FindProperty("matchStateText").objectReferenceValue = matchStateText.GetComponent<TextMeshProUGUI>();
        so.FindProperty("matchTimerText").objectReferenceValue = matchTimerText.GetComponent<TextMeshProUGUI>();
        so.FindProperty("victoryPanel").objectReferenceValue = victoryPanel;
        so.FindProperty("victoryText").objectReferenceValue = victoryPanel.transform.Find("VictoryText").GetComponent<TextMeshProUGUI>();
        
        so.FindProperty("player1HealthBar").objectReferenceValue = player1Health.transform.Find("HealthSlider").GetComponent<Slider>();
        so.FindProperty("player1HealthText").objectReferenceValue = player1Health.transform.Find("HealthText").GetComponent<TextMeshProUGUI>();
        so.FindProperty("player1NameText").objectReferenceValue = player1Health.transform.Find("PlayerName").GetComponent<TextMeshProUGUI>();
        
        so.FindProperty("player2HealthBar").objectReferenceValue = player2Health.transform.Find("HealthSlider").GetComponent<Slider>();
        so.FindProperty("player2HealthText").objectReferenceValue = player2Health.transform.Find("HealthText").GetComponent<TextMeshProUGUI>();
        so.FindProperty("player2NameText").objectReferenceValue = player2Health.transform.Find("PlayerName").GetComponent<TextMeshProUGUI>();
        
        so.ApplyModifiedProperties();
        
        Selection.activeGameObject = matchHUDRoot;
        EditorGUIUtility.PingObject(matchHUDRoot);
        
        Debug.Log("<color=green>✅ Match HUD created successfully!</color>\n" +
                  "Next steps:\n" +
                  "1. Assign MatchManager reference in MatchHUD component\n" +
                  "2. MatchManager will auto-wire player stats when scene starts");
        
        EditorUtility.DisplayDialog("Match HUD Created!", 
            "Match HUD UI has been created successfully!\n\n" +
            "Don't forget to:\n" +
            "1. Assign the MatchManager reference in the MatchHUD component\n" +
            "2. The MatchManager will automatically connect player health bars", 
            "OK");
    }
    
    private static Canvas CreateNewCanvas(string name)
    {
        GameObject canvasGO = new GameObject(name);
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();
        
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
        
        return canvas;
    }
    
    private static GameObject CreateText(GameObject parent, string name, string text, 
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta, int fontSize)
    {
        GameObject textGO = new GameObject(name);
        textGO.transform.SetParent(parent.transform, false);
        
        RectTransform rect = textGO.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = sizeDelta;
        
        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        
        return textGO;
    }
    
    private static GameObject CreatePlayerHealthBar(GameObject parent, string name, 
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, string playerName)
    {
        GameObject container = new GameObject(name);
        container.transform.SetParent(parent.transform, false);
        
        RectTransform containerRect = container.AddComponent<RectTransform>();
        containerRect.anchorMin = anchorMin;
        containerRect.anchorMax = anchorMax;
        containerRect.sizeDelta = new Vector2(0, 50);
        containerRect.anchoredPosition = anchoredPos;
        
        GameObject nameLabel = CreateText(container, "PlayerName", playerName, 
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0, 0), new Vector2(0, 20), 16);
        nameLabel.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
        nameLabel.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
        
        GameObject sliderGO = new GameObject("HealthSlider");
        sliderGO.transform.SetParent(container.transform, false);
        
        RectTransform sliderRect = sliderGO.AddComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0f, 0f);
        sliderRect.anchorMax = new Vector2(1f, 1f);
        sliderRect.sizeDelta = new Vector2(0, -25);
        sliderRect.anchoredPosition = new Vector2(0, -2);
        
        Slider slider = sliderGO.AddComponent<Slider>();
        slider.minValue = 0;
        slider.maxValue = 100;
        slider.value = 100;
        
        GameObject background = new GameObject("Background");
        background.transform.SetParent(sliderGO.transform, false);
        RectTransform bgRect = background.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderGO.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.sizeDelta = new Vector2(-5, -5);
        
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRect = fill.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = new Color(0f, 1f, 0.3f, 1f);
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        
        slider.fillRect = fillRect;
        
        GameObject healthText = CreateText(container, "HealthText", "100/100", 
            new Vector2(0.5f, 0f), new Vector2(0.5f, 1f), new Vector2(0, 0), new Vector2(100, 20), 14);
        healthText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        healthText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 5);
        
        return container;
    }
    
    private static GameObject CreateVictoryPanel(GameObject parent)
    {
        GameObject panel = new GameObject("VictoryPanel");
        panel.transform.SetParent(parent.transform, false);
        
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        panelRect.anchoredPosition = Vector2.zero;
        
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.8f);
        
        GameObject victoryText = CreateText(panel, "VictoryText", "PLAYER 1 WINS!", 
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(600, 100), 48);
        victoryText.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
        victoryText.GetComponent<TextMeshProUGUI>().color = Color.yellow;
        
        GameObject subtitle = CreateText(panel, "Subtitle", "Press R to restart", 
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -60), new Vector2(400, 40), 24);
        subtitle.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.8f, 0.8f, 1f);
        
        panel.SetActive(false);
        
        return panel;
    }
}
#endif
