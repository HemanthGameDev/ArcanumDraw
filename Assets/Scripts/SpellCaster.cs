using UnityEngine;
using System.Collections.Generic;

public class SpellCaster : MonoBehaviour
{
    [Header("Mana Settings")]
    [SerializeField] private float currentMana = 100f;
    [SerializeField] private float maxMana = 100f;
    [SerializeField] private float manaRegenRate = 5f;
    
    [Header("Spell Transform References")]
    [SerializeField] private Transform spellSpawnPoint;
    [SerializeField] private Transform targetOpponent;
    
    [Header("Projectile Settings")]
    [SerializeField] private float projectileForce = 10f;
    
    [Header("Shield Settings")]
    [SerializeField] private float shieldSpawnDistance = 1.5f;
    
    [Header("Optional References")]
    [SerializeField] private GestureDrawingManager gestureDrawingManager;
    [SerializeField] private PlayerUIController playerUIController;
    
    private Dictionary<string, float> spellCooldowns = new Dictionary<string, float>();
    
    private void Update()
    {
        RegenerateMana();
    }
    
    private void RegenerateMana()
    {
        if (currentMana < maxMana)
        {
            currentMana += manaRegenRate * Time.deltaTime;
            currentMana = Mathf.Min(currentMana, maxMana);
        }
    }
    
    public bool AttemptCastSpell(SpellData spell)
    {
        if (spell == null)
        {
            Debug.LogWarning("Attempted to cast null spell");
            return false;
        }
        
        if (!HasEnoughMana(spell))
        {
            Debug.Log($"Not enough mana to cast {spell.spellName}. Need {spell.manaCost}, have {currentMana:F0}");
            
            if (playerUIController != null)
            {
                playerUIController.ShowLowManaFeedback();
            }
            
            return false;
        }
        
        if (IsSpellOnCooldown(spell))
        {
            float timeRemaining = GetCooldownTimeRemaining(spell);
            Debug.Log($"{spell.spellName} is on cooldown. Wait {timeRemaining:F1}s");
            
            if (playerUIController != null)
            {
                playerUIController.ShowOnCooldownFeedback();
            }
            
            return false;
        }
        
        DeductMana(spell.manaCost);
        StartCooldown(spell);
        SpawnSpellEffect(spell);
        ClearDrawingVisuals();
        
        if (playerUIController != null)
        {
            playerUIController.UpdateHealthMana();
            playerUIController.StartSpellCooldown(spell.spellID, spell.cooldownTime);
        }
        
        Debug.Log($"<color=green>Cast {spell.spellName}! Mana: {currentMana:F0}/{maxMana:F0}</color>");
        
        return true;
    }
    
    private bool HasEnoughMana(SpellData spell)
    {
        return currentMana >= spell.manaCost;
    }
    
    private bool IsSpellOnCooldown(SpellData spell)
    {
        if (spellCooldowns.ContainsKey(spell.spellID))
        {
            return Time.time < spellCooldowns[spell.spellID];
        }
        return false;
    }
    
    private float GetCooldownTimeRemaining(SpellData spell)
    {
        if (spellCooldowns.ContainsKey(spell.spellID))
        {
            return Mathf.Max(0f, spellCooldowns[spell.spellID] - Time.time);
        }
        return 0f;
    }
    
    private void DeductMana(float amount)
    {
        currentMana -= amount;
        currentMana = Mathf.Max(0f, currentMana);
    }
    
    private void StartCooldown(SpellData spell)
    {
        spellCooldowns[spell.spellID] = Time.time + spell.cooldownTime;
    }
    
    private void SpawnSpellEffect(SpellData spell)
    {
        if (spell.spellEffectPrefab == null)
        {
            Debug.LogError($"<color=red>ERROR: No effect prefab assigned for {spell.spellName}! Check the SpellData asset.</color>");
            return;
        }
        
        Vector3 spawnPosition = spellSpawnPoint != null ? spellSpawnPoint.position : transform.position;
        Quaternion spawnRotation = spellSpawnPoint != null ? spellSpawnPoint.rotation : Quaternion.identity;
        
        GameObject spellEffect = Instantiate(spell.spellEffectPrefab, spawnPosition, spawnRotation);
        
        Debug.Log($"<color=cyan>✓ Spawned {spell.spellName} effect at {spawnPosition}</color>");
        
        InitializeSpellEffect(spellEffect, spell);
    }
    
    private void InitializeSpellEffect(GameObject spellEffect, SpellData spell)
    {
        SpellProjectile projectile = spellEffect.GetComponent<SpellProjectile>();
        LightningEffect lightning = spellEffect.GetComponent<LightningEffect>();
        ShieldEffect shield = spellEffect.GetComponent<ShieldEffect>();
        
        if (projectile != null)
        {
            ApplyProjectileLogic(spellEffect, spell);
        }
        else if (lightning != null)
        {
            if (targetOpponent != null)
            {
                lightning.SetTarget(targetOpponent);
            }
        }
        else if (shield != null)
        {
            Vector3 directionToOpponent = targetOpponent != null 
                ? (targetOpponent.position - transform.position).normalized 
                : transform.forward;
            
            Vector3 shieldPosition = transform.position + directionToOpponent * shieldSpawnDistance;
            spellEffect.transform.position = shieldPosition;
            
            Quaternion lookRotation = Quaternion.LookRotation(directionToOpponent);
            spellEffect.transform.rotation = lookRotation;
            
            shield.SetTargetToFollow(transform);
            shield.SetFacingDirection(directionToOpponent);
            
            Debug.Log($"Shield spawned at {shieldPosition}, facing opponent");
        }
        else
        {
            ApplyProjectileLogic(spellEffect, spell);
        }
    }
    
    private void ApplyProjectileLogic(GameObject spellEffect, SpellData spell)
    {
        Rigidbody rb = spellEffect.GetComponent<Rigidbody>();
        
        if (rb != null && targetOpponent != null)
        {
            Vector3 direction = (targetOpponent.position - spellEffect.transform.position).normalized;
            spellEffect.transform.rotation = Quaternion.LookRotation(direction);
            rb.AddForce(direction * projectileForce, ForceMode.Impulse);
            
            Debug.Log($"Applied force to {spell.spellName} towards target");
        }
        else if (rb != null && spellSpawnPoint != null)
        {
            rb.AddForce(spellSpawnPoint.forward * projectileForce, ForceMode.Impulse);
        }
    }
    
    private void ClearDrawingVisuals()
    {
        if (gestureDrawingManager != null)
        {
            gestureDrawingManager.ClearAllDrawings();
        }
    }
    
    public float GetCurrentMana()
    {
        return currentMana;
    }
    
    public float GetMaxMana()
    {
        return maxMana;
    }
    
    public float GetCooldownProgress(SpellData spell)
    {
        if (!spellCooldowns.ContainsKey(spell.spellID))
        {
            return 1f;
        }
        
        float cooldownEndTime = spellCooldowns[spell.spellID];
        float cooldownStartTime = cooldownEndTime - spell.cooldownTime;
        float elapsed = Time.time - cooldownStartTime;
        
        return Mathf.Clamp01(elapsed / spell.cooldownTime);
    }
}
