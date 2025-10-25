using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LightningEffect : MonoBehaviour
{
    [Header("Lightning Settings")]
    [SerializeField] private float lifetime = 2f;
    [SerializeField] private float strikeDelay = 0.2f;
    [SerializeField] private int damage = 25;
    [SerializeField] private float chainRange = 5f;
    [SerializeField] private int maxChainTargets = 3;
    
    [Header("Visual Settings")]
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private Color lightningColor = new Color(0.5f, 0.5f, 1f, 1f);
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Light lightningLight;
    
    [Header("Audio")]
    [SerializeField] private AudioClip strikeSound;
    
    private Transform target;
    private List<GameObject> hitTargets = new List<GameObject>();
    private MeshRenderer meshRenderer;
    private Material lightningMaterial;
    
    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        
        if (meshRenderer != null && meshRenderer.material != null)
        {
            lightningMaterial = meshRenderer.material;
        }
        
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
        
        if (lightningLight != null)
        {
            lightningLight.enabled = false;
        }
    }
    
    private void Start()
    {
        StartCoroutine(LightningStrikeRoutine());
    }
    
    private IEnumerator LightningStrikeRoutine()
    {
        yield return new WaitForSeconds(strikeDelay);
        
        StrikeTarget();
        
        yield return new WaitForSeconds(lifetime - strikeDelay - fadeOutDuration);
        
        yield return StartCoroutine(FadeOutRoutine());
        
        Destroy(gameObject);
    }
    
    private void StrikeTarget()
    {
        if (target != null)
        {
            ApplyDamage(target.gameObject);
            hitTargets.Add(target.gameObject);
            
            StartCoroutine(FlashEffectRoutine());
            
            if (strikeSound != null)
            {
                AudioSource.PlayClipAtPoint(strikeSound, transform.position);
            }
            
            ChainToNearbyTargets(target.position, 1);
        }
    }
    
    private void ChainToNearbyTargets(Vector3 fromPosition, int chainCount)
    {
        if (chainCount >= maxChainTargets) return;
        
        Collider[] nearbyColliders = Physics.OverlapSphere(fromPosition, chainRange);
        
        foreach (Collider col in nearbyColliders)
        {
            if (col.CompareTag("Player") && !hitTargets.Contains(col.gameObject))
            {
                hitTargets.Add(col.gameObject);
                ApplyDamage(col.gameObject);
                
                if (lineRenderer != null)
                {
                    StartCoroutine(DrawLightningArcRoutine(fromPosition, col.transform.position));
                }
                
                ChainToNearbyTargets(col.transform.position, chainCount + 1);
                break;
            }
        }
    }
    
    private IEnumerator DrawLightningArcRoutine(Vector3 start, Vector3 end)
    {
        if (lineRenderer == null) yield break;
        
        lineRenderer.enabled = true;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
        
        yield return new WaitForSeconds(flashDuration);
        
        lineRenderer.enabled = false;
    }
    
    private IEnumerator FlashEffectRoutine()
    {
        if (lightningLight != null)
        {
            lightningLight.enabled = true;
            yield return new WaitForSeconds(flashDuration);
            lightningLight.enabled = false;
        }
        
        if (meshRenderer != null)
        {
            meshRenderer.enabled = true;
            yield return new WaitForSeconds(flashDuration * 2);
            meshRenderer.enabled = false;
        }
    }
    
    private IEnumerator FadeOutRoutine()
    {
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;
        
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeOutDuration;
            
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            
            if (lightningMaterial != null)
            {
                Color color = lightningMaterial.color;
                color.a = Mathf.Lerp(1f, 0f, t);
                lightningMaterial.color = color;
            }
            
            yield return null;
        }
    }
    
    private void ApplyDamage(GameObject target)
    {
        Debug.Log($"Lightning struck {target.name} for {damage} damage!");
    }
    
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        
        if (target != null)
        {
            transform.position = target.position;
        }
    }
    
    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }
}
