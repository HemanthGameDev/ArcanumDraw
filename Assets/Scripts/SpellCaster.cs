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
    
    [Header("Spawn Settings")]
    [SerializeField] private float projectileForce = 15f;
    [SerializeField] private float arcHeight = 3f;
    [SerializeField] private float shieldSpawnDistance = 1.5f;
    [SerializeField] private bool autoScaleToPlayerSize = true;
    [SerializeField] private float spellScaleMultiplier = 1f;
    
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
        
        Vector3 spawnPosition = CalculateSpawnPosition(spell);
        Quaternion spawnRotation = CalculateSpawnRotation(spell);
        
        GameObject spellEffect = Instantiate(spell.spellEffectPrefab, spawnPosition, spawnRotation);
        
        if (autoScaleToPlayerSize)
        {
            ApplyScaleBasedOnPlayer(spellEffect);
        }
        else if (spellScaleMultiplier != 1f)
        {
            spellEffect.transform.localScale *= spellScaleMultiplier;
        }
        
        Debug.Log($"<color=cyan>✓ Spawned {spell.spellName} effect at {spawnPosition}</color>");
        
        InitializeSpellEffect(spellEffect, spell);
    }
    
    private Vector3 CalculateSpawnPosition(SpellData spell)
    {
        if (spell.spellID.Contains("Shield") || spell.spellID.Contains("shield"))
        {
            Vector3 directionToOpponent = targetOpponent != null 
                ? (targetOpponent.position - transform.position).normalized 
                : transform.forward;
            
            return transform.position + directionToOpponent * shieldSpawnDistance;
        }
        
        return spellSpawnPoint != null ? spellSpawnPoint.position : transform.position;
    }
    
    private Quaternion CalculateSpawnRotation(SpellData spell)
    {
        if (spell.spellID.Contains("Shield") || spell.spellID.Contains("shield"))
        {
            Vector3 directionToOpponent = targetOpponent != null 
                ? (targetOpponent.position - transform.position).normalized 
                : transform.forward;
            
            return Quaternion.LookRotation(directionToOpponent);
        }
        
        if (targetOpponent != null)
        {
            Vector3 directionToTarget = (targetOpponent.position - (spellSpawnPoint != null ? spellSpawnPoint.position : transform.position)).normalized;
            return Quaternion.LookRotation(directionToTarget);
        }
        
        return spellSpawnPoint != null ? spellSpawnPoint.rotation : transform.rotation;
    }
    
    private void ApplyScaleBasedOnPlayer(GameObject spellEffect)
    {
        Vector3 playerScale = transform.localScale;
        float averagePlayerScale = (playerScale.x + playerScale.y + playerScale.z) / 3f;
        
        Vector3 baseScale = spellEffect.transform.localScale;
        spellEffect.transform.localScale = baseScale * averagePlayerScale * spellScaleMultiplier;
        
        Debug.Log($"Auto-scaled spell: Player avg scale = {averagePlayerScale:F3}, Spell scale = {spellEffect.transform.localScale}");
    }
    
    private void InitializeSpellEffect(GameObject spellEffect, SpellData spell)
    {
        SpellProjectile projectile = spellEffect.GetComponent<SpellProjectile>();
        LightningEffect lightning = spellEffect.GetComponent<LightningEffect>();
        ShieldEffect shield = spellEffect.GetComponent<ShieldEffect>();
        
        if (lightning != null)
        {
            if (targetOpponent != null)
            {
                lightning.SetTarget(targetOpponent);
                Debug.Log($"Lightning spell initialized with target: {targetOpponent.name}");
            }
            else
            {
                Debug.LogWarning("Lightning spell spawned but no opponent target assigned!");
            }
        }
        else if (shield != null)
        {
            Vector3 directionToOpponent = targetOpponent != null 
                ? (targetOpponent.position - transform.position).normalized 
                : transform.forward;
            
            shield.SetTargetToFollow(transform);
            shield.SetFacingDirection(directionToOpponent);
            
            Debug.Log($"Shield spell initialized at position {spellEffect.transform.position}");
        }
        else if (projectile != null)
        {
            ApplyProjectileLogic(spellEffect, spell);
            Debug.Log($"Projectile spell ({spell.spellName}) initialized");
        }
        else
        {
            ApplyProjectileLogic(spellEffect, spell);
            Debug.LogWarning($"Spell effect for {spell.spellName} has no recognized component (SpellProjectile, LightningEffect, or ShieldEffect). Applied default projectile behavior.");
        }
    }
    
    private void ApplyProjectileLogic(GameObject spellEffect, SpellData spell)
    {
        Rigidbody rb = spellEffect.GetComponent<Rigidbody>();
        
        if (rb != null && targetOpponent != null)
        {
            Vector3 startPos = spellEffect.transform.position;
            Vector3 targetPos = targetOpponent.position;
            
            Vector3 direction = (targetPos - startPos).normalized;
            float distance = Vector3.Distance(startPos, targetPos);
            
            Vector3 horizontalDir = new Vector3(direction.x, 0, direction.z).normalized;
            float horizontalDist = Vector3.Distance(
                new Vector3(startPos.x, 0, startPos.z),
                new Vector3(targetPos.x, 0, targetPos.z)
            );
            
            float time = horizontalDist / (projectileForce * 0.5f);
            float upwardVelocity = (arcHeight + 0.5f * Mathf.Abs(Physics.gravity.y) * time * time) / time;
            
            Vector3 velocity = horizontalDir * projectileForce + Vector3.up * upwardVelocity;
            
            spellEffect.transform.rotation = Quaternion.LookRotation(direction);
            rb.linearVelocity = velocity;
            
            Debug.Log($"Applied arc trajectory to {spell.spellName} (Force: {projectileForce}, Arc: {arcHeight}, Time: {time:F2}s)");
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
