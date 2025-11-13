using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class PlayerUIController : MonoBehaviour
{
    [Header("Health & Mana References")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider manaSlider;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI manaText;
    
    [Header("Spell Icon Slots")]
    [SerializeField] private List<Image> spellIconSlots = new List<Image>();
    
    [Header("Gesture Feedback")]
    [SerializeField] private Image gestureHighlightImage;
    [SerializeField] private float highlightDuration = 0.3f;
    [SerializeField] private AnimationCurve highlightScaleCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 1.2f);
    
    [Header("Feedback Colors")]
    [SerializeField] private Color cooldownOverlayColor = new Color(0f, 0f, 0f, 0.5f);
    [SerializeField] private Color lowManaColor = Color.red;
    [SerializeField] private Color successColor = Color.green;
    
    [Header("Component References")]
    [SerializeField] private SpellCaster spellCaster;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private GestureDrawingManager drawingManager;
    [SerializeField] private GestureRecognizer gestureRecognizer;
    
    private Dictionary<string, Image> spellIconMap = new Dictionary<string, Image>();
    private Dictionary<string, Image> spellCooldownOverlays = new Dictionary<string, Image>();
    private Dictionary<string, Coroutine> activeCooldownCoroutines = new Dictionary<string, Coroutine>();
    
    private const float HEALTH_MAX = 100f;
    private float currentHealth = 100f;
    
    private void Start()
    {
        InitializeHealthMana();
        
        if (playerStats != null)
        {
            playerStats.OnHealthChanged += OnHealthChanged;
            currentHealth = playerStats.CurrentHealth;
        }
        
        if (gestureRecognizer != null)
        {
            InitializeSpellIcons(gestureRecognizer.GetAvailableSpells());
        }
        
        if (gestureHighlightImage != null)
        {
            gestureHighlightImage.gameObject.SetActive(false);
        }
    }
    
    private void OnDestroy()
    {
        if (playerStats != null)
        {
            playerStats.OnHealthChanged -= OnHealthChanged;
        }
    }
    
    private void OnHealthChanged(float newHealth, float maxHealth)
    {
        currentHealth = newHealth;
        UpdateHealthMana();
    }
    
    private void Update()
    {
        UpdateHealthMana();
    }
    
    private void InitializeHealthMana()
    {
        float maxHealth = playerStats != null ? playerStats.MaxHealth : HEALTH_MAX;
        
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
        
        if (spellCaster != null && manaSlider != null)
        {
            manaSlider.maxValue = spellCaster.GetMaxMana();
            manaSlider.value = spellCaster.GetCurrentMana();
        }
    }
    
    public void UpdateHealthMana()
    {
        float maxHealth = playerStats != null ? playerStats.MaxHealth : HEALTH_MAX;
        
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }
        
        if (healthText != null)
        {
            healthText.text = $"{currentHealth:F0}/{maxHealth:F0}";
        }
        
        if (spellCaster != null)
        {
            float currentMana = spellCaster.GetCurrentMana();
            float maxMana = spellCaster.GetMaxMana();
            
            if (manaSlider != null)
            {
                manaSlider.value = currentMana;
            }
            
            if (manaText != null)
            {
                manaText.text = $"{currentMana:F0}/{maxMana:F0}";
            }
        }
    }
    
    public void InitializeSpellIcons(List<SpellData> playerLoadout)
    {
        if (playerLoadout == null || playerLoadout.Count == 0)
        {
            Debug.LogWarning("PlayerUIController: No spells provided for loadout");
            return;
        }
        
        spellIconMap.Clear();
        spellCooldownOverlays.Clear();
        
        int slotsToFill = Mathf.Min(playerLoadout.Count, spellIconSlots.Count);
        
        for (int i = 0; i < slotsToFill; i++)
        {
            SpellData spell = playerLoadout[i];
            Image iconSlot = spellIconSlots[i];
            
            if (spell != null && iconSlot != null)
            {
                iconSlot.gameObject.SetActive(true);
                spellIconMap[spell.spellID] = iconSlot;
                
                CreateCooldownOverlay(spell.spellID, iconSlot);
                
                Debug.Log($"Initialized spell icon for: {spell.spellName}");
            }
        }
        
        for (int i = slotsToFill; i < spellIconSlots.Count; i++)
        {
            if (spellIconSlots[i] != null)
            {
                spellIconSlots[i].gameObject.SetActive(false);
            }
        }
    }
    
    private void CreateCooldownOverlay(string spellID, Image iconSlot)
    {
        GameObject overlayObject = new GameObject($"{spellID}_CooldownOverlay");
        overlayObject.transform.SetParent(iconSlot.transform, false);
        
        RectTransform overlayRect = overlayObject.AddComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        
        Image overlayImage = overlayObject.AddComponent<Image>();
        overlayImage.color = cooldownOverlayColor;
        overlayImage.fillMethod = Image.FillMethod.Radial360;
        overlayImage.fillOrigin = (int)Image.Origin360.Top;
        overlayImage.fillAmount = 0f;
        overlayImage.type = Image.Type.Filled;
        
        spellCooldownOverlays[spellID] = overlayImage;
    }
    
    public void StartSpellCooldown(string spellID, float cooldownTime)
    {
        if (!spellCooldownOverlays.ContainsKey(spellID))
        {
            Debug.LogWarning($"No cooldown overlay found for spell: {spellID}");
            return;
        }
        
        if (activeCooldownCoroutines.ContainsKey(spellID))
        {
            StopCoroutine(activeCooldownCoroutines[spellID]);
        }
        
        Coroutine cooldownCoroutine = StartCoroutine(CooldownAnimationRoutine(spellID, cooldownTime));
        activeCooldownCoroutines[spellID] = cooldownCoroutine;
    }
    
    private IEnumerator CooldownAnimationRoutine(string spellID, float duration)
    {
        Image overlayImage = spellCooldownOverlays[spellID];
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            overlayImage.fillAmount = 1f - progress;
            yield return null;
        }
        
        overlayImage.fillAmount = 0f;
        activeCooldownCoroutines.Remove(spellID);
    }
    
    public void DisplayGestureHighlight(Sprite gestureSprite)
    {
        if (gestureHighlightImage == null) return;
        
        StartCoroutine(GestureHighlightRoutine(gestureSprite));
    }
    
    private IEnumerator GestureHighlightRoutine(Sprite gestureSprite)
    {
        gestureHighlightImage.gameObject.SetActive(true);
        
        if (gestureSprite != null)
        {
            gestureHighlightImage.sprite = gestureSprite;
        }
        
        float elapsed = 0f;
        Vector3 originalScale = gestureHighlightImage.transform.localScale;
        Color originalColor = gestureHighlightImage.color;
        
        while (elapsed < highlightDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / highlightDuration;
            
            float scaleMultiplier = highlightScaleCurve.Evaluate(t);
            gestureHighlightImage.transform.localScale = originalScale * scaleMultiplier;
            
            Color currentColor = originalColor;
            currentColor.a = Mathf.Lerp(1f, 0f, t);
            gestureHighlightImage.color = currentColor;
            
            yield return null;
        }
        
        gestureHighlightImage.gameObject.SetActive(false);
        gestureHighlightImage.transform.localScale = originalScale;
        gestureHighlightImage.color = originalColor;
    }
    
    public void ShowLowManaFeedback()
    {
        StartCoroutine(FlashFeedbackRoutine(manaSlider, lowManaColor));
        Debug.Log("Low Mana Feedback!");
    }
    
    public void ShowOnCooldownFeedback()
    {
        Debug.Log("Spell on Cooldown!");
    }
    
    public void ShowUnrecognizedGestureFeedback()
    {
        Debug.Log("Gesture not recognized!");
    }
    
    private IEnumerator FlashFeedbackRoutine(Slider slider, Color flashColor)
    {
        if (slider == null) yield break;
        
        Image fillImage = slider.fillRect?.GetComponent<Image>();
        if (fillImage == null) yield break;
        
        Color originalColor = fillImage.color;
        float flashDuration = 0.2f;
        
        fillImage.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        fillImage.color = originalColor;
    }
    
    public void TakeDamage(float damage)
    {
        currentHealth = Mathf.Max(0f, currentHealth - damage);
        UpdateHealthMana();
    }
    
    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(HEALTH_MAX, currentHealth + amount);
        UpdateHealthMana();
    }
}
