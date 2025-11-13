using UnityEngine;

public class SpellProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private float speed = 10f;
    [SerializeField] private int damage = 10;
    [SerializeField] private bool useRigidbodyForce = true;
    
    [Header("Visual Settings")]
    [SerializeField] private float stretchMultiplier = 3f;
    [SerializeField] private bool stretchInDirectionOfTravel = true;
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject impactEffectPrefab;
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private new ParticleSystem particleSystem;
    
    [Header("Audio")]
    [SerializeField] private AudioClip launchSound;
    [SerializeField] private AudioClip impactSound;
    
    private Rigidbody rb;
    private float spawnTime;
    private bool hasHit = false;
    private Transform caster;
    private Vector3 baseScale;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        spawnTime = Time.time;
        baseScale = transform.localScale;
        
        gameObject.AddComponent<VisibilityDebugger>();
        
        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            Debug.Log($"<color=cyan>Fireball Awake: Rigidbody found, CollisionDetection set to Continuous</color>");
        }
        
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.enabled = true;
            
            Material mat = meshRenderer.material;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(1f, 0.3f, 0f, 1f) * 2f);
            mat.SetColor("_BaseColor", new Color(1f, 0.5f, 0f, 1f));
            
            Debug.Log($"<color=cyan>Fireball MeshRenderer: enabled={meshRenderer.enabled}, material={mat.name}</color>");
            Debug.Log($"<color=cyan>   Material BaseColor: {mat.GetColor("_BaseColor")}</color>");
            Debug.Log($"<color=cyan>   Material EmissionColor: {mat.GetColor("_EmissionColor")}</color>");
        }
        else
        {
            Debug.LogError($"<color=red>Fireball has NO MeshRenderer!</color>");
        }
        
        Light fireLight = GetComponent<Light>();
        if (fireLight == null)
        {
            fireLight = gameObject.AddComponent<Light>();
            fireLight.type = LightType.Point;
            fireLight.color = new Color(1f, 0.5f, 0f);
            fireLight.range = 5f;
            fireLight.intensity = 5f;
            Debug.Log($"<color=cyan>Added Point Light to Fireball: range=5, intensity=5</color>");
        }
        
        gameObject.layer = 0;
        Debug.Log($"<color=cyan>Fireball layer set to Default (0)</color>");
    }
    
    private void Start()
    {
        if (particleSystem != null)
        {
            particleSystem.Play();
        }
        
        if (launchSound != null)
        {
            AudioSource.PlayClipAtPoint(launchSound, transform.position);
        }
        
        if (stretchInDirectionOfTravel && rb != null && rb.linearVelocity.magnitude > 0.1f)
        {
            ApplyStretchedScale();
        }
        
        Debug.Log($"<color=cyan>═══════════════════════════════════════════════</color>");
        Debug.Log($"<color=cyan>🔥 FIREBALL START DIAGNOSTIC:</color>");
        Debug.Log($"<color=cyan>   Position: {transform.position}</color>");
        Debug.Log($"<color=cyan>   LocalScale: {transform.localScale}</color>");
        Debug.Log($"<color=cyan>   LossyScale: {transform.lossyScale}</color>");
        Debug.Log($"<color=cyan>   Rotation: {transform.rotation.eulerAngles}</color>");
        Debug.Log($"<color=cyan>   Active: {gameObject.activeSelf}</color>");
        Debug.Log($"<color=cyan>   Layer: {LayerMask.LayerToName(gameObject.layer)}</color>");
        Debug.Log($"<color=cyan>   Stretch Enabled: {stretchInDirectionOfTravel}</color>");
        
        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (mr != null)
        {
            Debug.Log($"<color=cyan>   MeshRenderer.enabled: {mr.enabled}</color>");
            Debug.Log($"<color=cyan>   Material: {mr.material.name}</color>");
            Debug.Log($"<color=cyan>   Material.color: {mr.material.color}</color>");
        }
        
        if (rb != null)
        {
            Debug.Log($"<color=cyan>   Rigidbody.velocity: {rb.linearVelocity} (magnitude: {rb.linearVelocity.magnitude:F2})</color>");
            Debug.Log($"<color=cyan>   Rigidbody.isKinematic: {rb.isKinematic}</color>");
            Debug.Log($"<color=cyan>   Rigidbody.useGravity: {rb.useGravity}</color>");
        }
        Debug.Log($"<color=cyan>═══════════════════════════════════════════════</color>");
    }
    
    private void ApplyStretchedScale()
    {
        Vector3 stretchedScale = baseScale;
        stretchedScale.z *= stretchMultiplier;
        transform.localScale = stretchedScale;
        
        Debug.Log($"<color=yellow>🔥 Fireball stretched: baseScale={baseScale} → stretchedScale={stretchedScale} (multiplier={stretchMultiplier})</color>");
    }
    
    private void Update()
    {
        if (rb != null && Time.frameCount % 30 == 0)
        {
            Camera mainCam = Camera.main;
            float distToCamera = mainCam != null ? Vector3.Distance(transform.position, mainCam.transform.position) : -1f;
            
            Debug.Log($"<color=green>🔥 Fireball Update:</color>");
            Debug.Log($"   Position: {transform.position}");
            Debug.Log($"   Velocity: {rb.linearVelocity.magnitude:F2}");
            Debug.Log($"   Distance to Camera: {distToCamera:F2}");
            Debug.Log($"   Active: {gameObject.activeSelf}");
            Debug.Log($"   Scale: {transform.localScale}");
            
            MeshRenderer mr = GetComponent<MeshRenderer>();
            if (mr != null)
            {
                Debug.Log($"   Renderer.enabled: {mr.enabled}");
                Debug.Log($"   Renderer.isVisible: {mr.isVisible}");
                Debug.Log($"   Bounds: {mr.bounds}");
            }
        }
        
        if (Time.time - spawnTime >= lifetime)
        {
            Debug.Log($"<color=yellow>Fireball lifetime expired ({lifetime}s)</color>");
            DestroyProjectile();
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (hasHit) return;
        
        hasHit = true;
        
        HandleImpact(collision);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;
        
        hasHit = true;
        
        HandleImpact(other);
    }
    
    private void HandleImpact(Collision collision)
    {
        Debug.Log($"<color=yellow>🔥 Fireball collision with {collision.gameObject.name} (Tag: {collision.gameObject.tag})</color>");
        
        if (!collision.gameObject.CompareTag("Player"))
        {
            Debug.Log($"<color=gray>Ignoring collision with {collision.gameObject.name} (not a Player)</color>");
            hasHit = false;
            return;
        }
        
        if (caster != null && (collision.transform == caster || collision.transform.IsChildOf(caster)))
        {
            Debug.Log($"<color=gray>Ignoring collision with caster {collision.gameObject.name}</color>");
            hasHit = false;
            return;
        }
        
        Debug.Log($"<color=orange>💥 FIREBALL HIT! Applying damage to {collision.gameObject.name} and destroying projectile</color>");
        
        ApplyDamage(collision.gameObject);
        SpawnImpactEffect(collision.contacts[0].point, collision.contacts[0].normal);
        PlayImpactSound();
        DestroyProjectile();
    }
    
    private void HandleImpact(Collider other)
    {
        Debug.Log($"<color=yellow>🔥 Fireball triggered collision with {other.name} (Tag: {other.tag})</color>");
        
        if (!other.CompareTag("Player"))
        {
            Debug.Log($"<color=gray>Ignoring collision with {other.name} (not a Player)</color>");
            hasHit = false;
            return;
        }
        
        if (caster != null && (other.transform == caster || other.transform.IsChildOf(caster)))
        {
            Debug.Log($"<color=gray>Ignoring collision with caster {other.name}</color>");
            hasHit = false;
            return;
        }
        
        Debug.Log($"<color=orange>💥 FIREBALL HIT! Applying damage to {other.name} and destroying projectile</color>");
        
        ApplyDamage(other.gameObject);
        SpawnImpactEffect(transform.position, -transform.forward);
        PlayImpactSound();
        DestroyProjectile();
    }
    
    private void ApplyDamage(GameObject target)
    {
        PlayerStats targetStats = target.GetComponent<PlayerStats>();
        if (targetStats != null)
        {
            targetStats.TakeDamage(damage);
            Debug.Log($"<color=orange>💥 Fireball hit {target.name} for {damage} damage!</color>");
        }
        else
        {
            Debug.LogWarning($"Fireball hit {target.name} but no PlayerStats component found!");
        }
    }
    
    private void SpawnImpactEffect(Vector3 position, Vector3 normal)
    {
        if (impactEffectPrefab != null)
        {
            GameObject impact = Instantiate(impactEffectPrefab, position, Quaternion.LookRotation(normal));
            Destroy(impact, 2f);
        }
    }
    
    private void PlayImpactSound()
    {
        if (impactSound != null)
        {
            AudioSource.PlayClipAtPoint(impactSound, transform.position);
        }
    }
    
    private void DestroyProjectile()
    {
        if (particleSystem != null)
        {
            particleSystem.Stop();
            particleSystem.transform.SetParent(null);
            Destroy(particleSystem.gameObject, particleSystem.main.duration);
        }
        
        Destroy(gameObject);
    }
    
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
        if (!useRigidbodyForce && rb != null)
        {
            rb.linearVelocity = transform.forward * speed;
        }
    }
    
    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }
    
    public void SetCaster(Transform casterTransform)
    {
        caster = casterTransform;
        Debug.Log($"Fireball caster set to {caster.name}");
    }
}
