using UnityEngine;
using System;

public class PlayerStats : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth = 100f;
    
    [Header("References")]
    [SerializeField] private PlayerUIController uiController;
    
    public event Action<float, float> OnHealthChanged;
    public event Action OnPlayerDied;
    
    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public bool IsAlive => currentHealth > 0f;
    public float HealthPercent => currentHealth / maxHealth;
    
    private void Start()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    public void TakeDamage(float damage)
    {
        if (!IsAlive || damage <= 0f) return;
        
        currentHealth = Mathf.Max(0f, currentHealth - damage);
        
        Debug.Log($"<color=red>{gameObject.name} took {damage} damage! HP: {currentHealth:F0}/{maxHealth:F0}</color>");
        
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        if (uiController != null)
        {
            uiController.TakeDamage(damage);
        }
        
        if (currentHealth <= 0f)
        {
            Die();
        }
    }
    
    public void Heal(float amount)
    {
        if (!IsAlive || amount <= 0f) return;
        
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        
        Debug.Log($"<color=green>{gameObject.name} healed {amount}! HP: {currentHealth:F0}/{maxHealth:F0}</color>");
        
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        if (uiController != null)
        {
            uiController.Heal(amount);
        }
    }
    
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        if (uiController != null)
        {
            uiController.UpdateHealthMana();
        }
        
        Debug.Log($"{gameObject.name} health reset to {maxHealth}");
    }
    
    private void Die()
    {
        Debug.Log($"<color=red>💀 {gameObject.name} has been defeated!</color>");
        OnPlayerDied?.Invoke();
    }
    
    public void SetMaxHealth(float newMaxHealth)
    {
        maxHealth = Mathf.Max(1f, newMaxHealth);
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}
