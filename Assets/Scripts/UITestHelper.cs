using UnityEngine;
using UnityEngine.InputSystem;

public class UITestHelper : MonoBehaviour
{
    [Header("Test References")]
    [SerializeField] private PlayerUIController uiController;
    [SerializeField] private SpellCaster spellCaster;
    
    [Header("Test Controls")]
    [SerializeField] private Key testDamageKey = Key.D;
    [SerializeField] private Key testHealKey = Key.H;
    [SerializeField] private Key testManaKey = Key.M;
    [SerializeField] private Key testGestureHighlightKey = Key.G;
    
    [Header("Test Values")]
    [SerializeField] private float testDamageAmount = 10f;
    [SerializeField] private float testHealAmount = 15f;
    
    private Keyboard keyboard;
    
    private void Awake()
    {
        keyboard = Keyboard.current;
    }
    
    private void Update()
    {
        if (uiController == null)
        {
            Debug.LogWarning("UITestHelper: PlayerUIController reference is missing!");
            return;
        }
        
        if (keyboard == null)
        {
            keyboard = Keyboard.current;
            return;
        }
        
        if (keyboard[testDamageKey].wasPressedThisFrame)
        {
            TestDamage();
        }
        
        if (keyboard[testHealKey].wasPressedThisFrame)
        {
            TestHeal();
        }
        
        if (keyboard[testManaKey].wasPressedThisFrame)
        {
            TestLowManaFeedback();
        }
        
        if (keyboard[testGestureHighlightKey].wasPressedThisFrame)
        {
            TestGestureHighlight();
        }
    }
    
    private void TestDamage()
    {
        uiController.TakeDamage(testDamageAmount);
        Debug.Log($"<color=red>Test: Applied {testDamageAmount} damage</color>");
    }
    
    private void TestHeal()
    {
        uiController.Heal(testHealAmount);
        Debug.Log($"<color=green>Test: Healed {testHealAmount} HP</color>");
    }
    
    private void TestLowManaFeedback()
    {
        uiController.ShowLowManaFeedback();
        Debug.Log($"<color=yellow>Test: Low Mana Feedback</color>");
    }
    
    private void TestGestureHighlight()
    {
        uiController.DisplayGestureHighlight(null);
        Debug.Log($"<color=cyan>Test: Gesture Highlight</color>");
    }
}
