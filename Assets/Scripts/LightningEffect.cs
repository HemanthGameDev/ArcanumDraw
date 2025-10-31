using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LightningEffect : MonoBehaviour
{
    [Header("Lightning Settings")]
    [SerializeField] private float lifetime = 1.5f;
    [SerializeField] private float strikeDelay = 0.2f;
    [SerializeField] private float fallSpeed = 10f;
    [SerializeField] private int damage = 25;
    [SerializeField] private float chainRange = 5f;
    [SerializeField] private int maxChainTargets = 3;
    
    [Header("Visual Settings - Main Bolt")]
    [SerializeField] private int boltSegments = 20;
    [SerializeField] private float boltJaggedness = 0.3f;
    [SerializeField] private float boltWidth = 0.2f;
    [SerializeField] private Color boltColor = new Color(0.6f, 0.8f, 1f, 1f);
    [SerializeField] private float boltIntensity = 3f;
    
    [Header("Visual Settings - Effects")]
    [SerializeField] private int flickerCount = 5;
    [SerializeField] private float flickerSpeed = 0.05f;
    [SerializeField] private float glowIntensity = 8f;
    [SerializeField] private Color glowColor = new Color(0.4f, 0.6f, 1f, 1f);
    
    [Header("Audio")]
    [SerializeField] private AudioClip strikeSound;
    
    private Transform target;
    private Transform caster;
    private Vector3 strikeStartPosition;
    private List<GameObject> hitTargets = new List<GameObject>();
    private LineRenderer mainBoltRenderer;
    private LineRenderer glowRenderer;
    private Light lightningLight;
    private bool hasStruck = false;
    
    private void Awake()
    {
        SetupLineRenderers();
        SetupLight();
    }
    
    private void SetupLineRenderers()
    {
        mainBoltRenderer = gameObject.AddComponent<LineRenderer>();
        mainBoltRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        mainBoltRenderer.startWidth = boltWidth;
        mainBoltRenderer.endWidth = boltWidth * 0.5f;
        mainBoltRenderer.startColor = boltColor;
        mainBoltRenderer.endColor = boltColor;
        mainBoltRenderer.material.SetColor("_BaseColor", boltColor * boltIntensity);
        mainBoltRenderer.material.EnableKeyword("_EMISSION");
        mainBoltRenderer.material.SetColor("_EmissionColor", boltColor * boltIntensity);
        mainBoltRenderer.numCapVertices = 5;
        mainBoltRenderer.numCornerVertices = 5;
        mainBoltRenderer.enabled = false;
        
        GameObject glowObj = new GameObject("LightningGlow");
        glowObj.transform.SetParent(transform);
        glowObj.transform.localPosition = Vector3.zero;
        glowRenderer = glowObj.AddComponent<LineRenderer>();
        glowRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        glowRenderer.startWidth = boltWidth * 3f;
        glowRenderer.endWidth = boltWidth * 2f;
        Color glowColorTransparent = glowColor;
        glowColorTransparent.a = 0.3f;
        glowRenderer.startColor = glowColorTransparent;
        glowRenderer.endColor = glowColorTransparent;
        glowRenderer.material.SetColor("_BaseColor", glowColor * glowIntensity);
        glowRenderer.material.EnableKeyword("_EMISSION");
        glowRenderer.material.SetColor("_EmissionColor", glowColor * glowIntensity);
        glowRenderer.numCapVertices = 10;
        glowRenderer.numCornerVertices = 10;
        glowRenderer.enabled = false;
    }
    
    private void SetupLight()
    {
        lightningLight = gameObject.AddComponent<Light>();
        lightningLight.type = LightType.Point;
        lightningLight.color = boltColor;
        lightningLight.range = 10f;
        lightningLight.intensity = 5f;
        lightningLight.enabled = false;
    }
    
    private void Start()
    {
        if (target != null)
        {
            strikeStartPosition = transform.position;
            Debug.Log($"<color=yellow>⚡ Lightning spawned at {strikeStartPosition}, will strike {target.name}</color>");
            StartCoroutine(LightningStrikeSequence());
        }
        else
        {
            Debug.LogError("⚡ Lightning has no target! Destroying...");
            Destroy(gameObject);
        }
    }
    
    private IEnumerator LightningStrikeSequence()
    {
        yield return new WaitForSeconds(strikeDelay);
        
        Vector3 targetPosition = target.position;
        
        Debug.Log($"<color=yellow>⚡ LIGHTNING STRIKE! From {strikeStartPosition} to {targetPosition}</color>");
        
        DrawJaggedBolt(strikeStartPosition, targetPosition);
        
        if (lightningLight != null)
        {
            lightningLight.enabled = true;
            lightningLight.transform.position = targetPosition;
        }
        
        mainBoltRenderer.enabled = true;
        glowRenderer.enabled = true;
        
        for (int i = 0; i < flickerCount; i++)
        {
            mainBoltRenderer.enabled = !mainBoltRenderer.enabled;
            glowRenderer.enabled = !glowRenderer.enabled;
            
            if (i % 2 == 0)
            {
                DrawJaggedBolt(strikeStartPosition, targetPosition);
            }
            
            yield return new WaitForSeconds(flickerSpeed);
        }
        
        mainBoltRenderer.enabled = true;
        glowRenderer.enabled = true;
        
        StrikeTarget();
        
        if (strikeSound != null)
        {
            AudioSource.PlayClipAtPoint(strikeSound, targetPosition, 1f);
        }
        
        yield return new WaitForSeconds(0.2f);
        
        yield return StartCoroutine(FadeOutBolt());
        
        Debug.Log($"<color=yellow>⚡ Lightning complete. Destroying effect.</color>");
        Destroy(gameObject);
    }
    
    private void DrawJaggedBolt(Vector3 start, Vector3 end)
    {
        Vector3[] boltPoints = GenerateJaggedLine(start, end, boltSegments, boltJaggedness);
        
        mainBoltRenderer.positionCount = boltPoints.Length;
        mainBoltRenderer.SetPositions(boltPoints);
        
        glowRenderer.positionCount = boltPoints.Length;
        glowRenderer.SetPositions(boltPoints);
    }
    
    private Vector3[] GenerateJaggedLine(Vector3 start, Vector3 end, int segments, float jaggedness)
    {
        Vector3[] points = new Vector3[segments + 1];
        points[0] = start;
        points[segments] = end;
        
        Vector3 direction = (end - start);
        Vector3 perpendicular = Vector3.Cross(direction.normalized, Vector3.up).normalized;
        if (perpendicular.magnitude < 0.1f)
        {
            perpendicular = Vector3.Cross(direction.normalized, Vector3.right).normalized;
        }
        
        for (int i = 1; i < segments; i++)
        {
            float t = (float)i / segments;
            Vector3 basePoint = Vector3.Lerp(start, end, t);
            
            float randomOffset = Random.Range(-jaggedness, jaggedness);
            float randomPerpOffset = Random.Range(-jaggedness * 0.5f, jaggedness * 0.5f);
            
            Vector3 offset = perpendicular * randomOffset + Vector3.Cross(direction.normalized, perpendicular) * randomPerpOffset;
            
            points[i] = basePoint + offset;
        }
        
        return points;
    }
    
    private void StrikeTarget()
    {
        if (hasStruck) return;
        hasStruck = true;
        
        if (target != null)
        {
            ApplyDamage(target.gameObject);
            hitTargets.Add(target.gameObject);
            
            Debug.Log($"<color=yellow>⚡ Lightning struck {target.name}! Checking for chain targets...</color>");
            
            ChainToNearbyTargets(target.position, 1);
        }
        else
        {
            Debug.LogWarning("Lightning has no target to strike!");
        }
    }
    
    private void ChainToNearbyTargets(Vector3 fromPosition, int chainCount)
    {
        if (chainCount >= maxChainTargets) return;
        
        Collider[] nearbyColliders = Physics.OverlapSphere(fromPosition, chainRange);
        
        Debug.Log($"<color=cyan>⚡ Lightning chain search: found {nearbyColliders.Length} colliders within {chainRange} units</color>");
        
        foreach (Collider col in nearbyColliders)
        {
            bool isPlayer = col.CompareTag("Player");
            bool notHit = !hitTargets.Contains(col.gameObject);
            bool notTarget = col.gameObject != target.gameObject;
            bool notCaster = caster == null || col.gameObject != caster.gameObject;
            
            Debug.Log($"<color=cyan>   Checking {col.name}: isPlayer={isPlayer}, notHit={notHit}, notTarget={notTarget}, notCaster={notCaster}</color>");
            
            if (isPlayer && notHit && notTarget && notCaster)
            {
                hitTargets.Add(col.gameObject);
                ApplyDamage(col.gameObject);
                
                Debug.Log($"<color=yellow>⚡ Lightning chained to {col.name}!</color>");
                
                StartCoroutine(DrawChainBolt(fromPosition, col.transform.position));
                
                ChainToNearbyTargets(col.transform.position, chainCount + 1);
                break;
            }
        }
        
        Debug.Log($"<color=cyan>⚡ Lightning chain search complete (depth={chainCount})</color>");
    }
    
    private IEnumerator DrawChainBolt(Vector3 start, Vector3 end)
    {
        GameObject chainObj = new GameObject("ChainLightning");
        chainObj.transform.position = start;
        
        LineRenderer chainRenderer = chainObj.AddComponent<LineRenderer>();
        chainRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        chainRenderer.startWidth = boltWidth * 0.5f;
        chainRenderer.endWidth = boltWidth * 0.3f;
        chainRenderer.startColor = boltColor;
        chainRenderer.endColor = boltColor;
        chainRenderer.material.SetColor("_BaseColor", boltColor * boltIntensity);
        chainRenderer.material.EnableKeyword("_EMISSION");
        chainRenderer.material.SetColor("_EmissionColor", boltColor * boltIntensity);
        
        Vector3[] chainPoints = GenerateJaggedLine(start, end, boltSegments / 2, boltJaggedness * 0.7f);
        chainRenderer.positionCount = chainPoints.Length;
        chainRenderer.SetPositions(chainPoints);
        
        for (int i = 0; i < 3; i++)
        {
            chainRenderer.enabled = !chainRenderer.enabled;
            yield return new WaitForSeconds(flickerSpeed);
        }
        
        Destroy(chainObj, 0.5f);
    }
    
    private IEnumerator FadeOutBolt()
    {
        float fadeDuration = 0.3f;
        float elapsed = 0f;
        
        Color startColor = mainBoltRenderer.startColor;
        Color glowStartColor = glowRenderer.startColor;
        float startLightIntensity = lightningLight != null ? lightningLight.intensity : 0f;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            
            Color fadeColor = Color.Lerp(startColor, Color.clear, t);
            mainBoltRenderer.startColor = fadeColor;
            mainBoltRenderer.endColor = fadeColor;
            
            Color fadeGlow = Color.Lerp(glowStartColor, Color.clear, t);
            glowRenderer.startColor = fadeGlow;
            glowRenderer.endColor = fadeGlow;
            
            if (lightningLight != null)
            {
                lightningLight.intensity = Mathf.Lerp(startLightIntensity, 0f, t);
            }
            
            yield return null;
        }
        
        mainBoltRenderer.enabled = false;
        glowRenderer.enabled = false;
        if (lightningLight != null) lightningLight.enabled = false;
    }
    
    private void ApplyDamage(GameObject target)
    {
        Debug.Log($"<color=yellow>⚡ Lightning struck {target.name} for {damage} damage!</color>");
    }
    
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        
        if (target != null)
        {
            Debug.Log($"<color=cyan>Lightning target set to {target.name} at position {target.position}</color>");
        }
    }
    
    public void SetCaster(Transform newCaster)
    {
        caster = newCaster;
        
        if (caster != null)
        {
            Debug.Log($"<color=cyan>Lightning caster set to {caster.name}</color>");
        }
    }
    
    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }
}
