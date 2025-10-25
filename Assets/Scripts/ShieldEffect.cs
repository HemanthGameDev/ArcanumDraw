using UnityEngine;
using System.Collections;

public class ShieldEffect : MonoBehaviour
{
    [Header("Shield Settings")]
    [SerializeField] private float duration = 5f;
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private int damageAbsorption = 50;
    
    [Header("Visual Settings")]
    [SerializeField] private float maxScale = 2f;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseIntensity = 0.1f;
    [SerializeField] private Color shieldColor = new Color(0, 0.5f, 1f, 0.5f);
    
    [Header("References")]
    [SerializeField] private Transform targetToFollow;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private ParticleSystem particleSystem;
    
    private Material shieldMaterial;
    private Vector3 targetScale;
    private float spawnTime;
    private int currentAbsorption;
    private bool isActive = true;
    private Vector3 facingDirection = Vector3.forward;
    private Vector3 localOffset;
    
    private void Awake()
    {
        if (meshRenderer == null)
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }
        
        if (meshRenderer != null && meshRenderer.material != null)
        {
            shieldMaterial = meshRenderer.material;
            shieldMaterial.color = new Color(shieldColor.r, shieldColor.g, shieldColor.b, 0);
        }
        
        targetScale = transform.localScale * maxScale;
        currentAbsorption = damageAbsorption;
        spawnTime = Time.time;
    }
    
    private void Start()
    {
        if (particleSystem != null)
        {
            particleSystem.Play();
        }
        
        StartCoroutine(ShieldLifecycleRoutine());
    }
    
    private void Update()
    {
        if (targetToFollow != null)
        {
            transform.position = targetToFollow.position + targetToFollow.TransformDirection(localOffset);
            
            transform.rotation = Quaternion.LookRotation(facingDirection);
        }
        
        if (isActive && shieldMaterial != null)
        {
            float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseIntensity;
            float alphaMultiplier = 1f + pulse;
            
            Color currentColor = shieldMaterial.color;
            currentColor.a = Mathf.Clamp01(shieldColor.a * alphaMultiplier);
            shieldMaterial.color = currentColor;
        }
    }
    
    private IEnumerator ShieldLifecycleRoutine()
    {
        yield return StartCoroutine(FadeInRoutine());
        
        float activeTime = duration - fadeInDuration - fadeOutDuration;
        yield return new WaitForSeconds(activeTime);
        
        yield return StartCoroutine(FadeOutRoutine());
        
        Destroy(gameObject);
    }
    
    private IEnumerator FadeInRoutine()
    {
        float elapsed = 0f;
        Vector3 startScale = Vector3.zero;
        
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeInDuration;
            
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            
            if (shieldMaterial != null)
            {
                Color color = shieldMaterial.color;
                color.a = Mathf.Lerp(0, shieldColor.a, t);
                shieldMaterial.color = color;
            }
            
            yield return null;
        }
        
        transform.localScale = targetScale;
    }
    
    private IEnumerator FadeOutRoutine()
    {
        isActive = false;
        float elapsed = 0f;
        
        if (particleSystem != null)
        {
            particleSystem.Stop();
        }
        
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeOutDuration;
            
            transform.localScale = Vector3.Lerp(targetScale, Vector3.zero, t);
            
            if (shieldMaterial != null)
            {
                Color color = shieldMaterial.color;
                color.a = Mathf.Lerp(shieldColor.a, 0, t);
                shieldMaterial.color = color;
            }
            
            yield return null;
        }
    }
    
    public void AbsorbDamage(int damage)
    {
        currentAbsorption -= damage;
        
        if (shieldMaterial != null)
        {
            StartCoroutine(FlashOnHitRoutine());
        }
        
        if (currentAbsorption <= 0)
        {
            StopAllCoroutines();
            StartCoroutine(FadeOutRoutine());
        }
    }
    
    private IEnumerator FlashOnHitRoutine()
    {
        Color originalColor = shieldMaterial.color;
        shieldMaterial.color = Color.white;
        
        yield return new WaitForSeconds(0.1f);
        
        shieldMaterial.color = originalColor;
    }
    
    public void SetTargetToFollow(Transform target)
    {
        targetToFollow = target;
        
        if (targetToFollow != null)
        {
            localOffset = targetToFollow.InverseTransformDirection(transform.position - targetToFollow.position);
        }
    }
    
    public void SetFacingDirection(Vector3 direction)
    {
        facingDirection = direction.normalized;
        transform.rotation = Quaternion.LookRotation(facingDirection);
    }
}
