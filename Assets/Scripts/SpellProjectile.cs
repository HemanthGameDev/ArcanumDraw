using UnityEngine;

public class SpellProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private float speed = 10f;
    [SerializeField] private int damage = 10;
    [SerializeField] private bool useRigidbodyForce = true;
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject impactEffectPrefab;
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private ParticleSystem particleSystem;
    
    [Header("Audio")]
    [SerializeField] private AudioClip launchSound;
    [SerializeField] private AudioClip impactSound;
    
    private Rigidbody rb;
    private float spawnTime;
    private bool hasHit = false;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        spawnTime = Time.time;
    }
    
    private void Start()
    {
        if (!useRigidbodyForce && rb != null)
        {
            rb.linearVelocity = transform.forward * speed;
        }
        
        if (particleSystem != null)
        {
            particleSystem.Play();
        }
        
        if (launchSound != null)
        {
            AudioSource.PlayClipAtPoint(launchSound, transform.position);
        }
    }
    
    private void Update()
    {
        if (Time.time - spawnTime >= lifetime)
        {
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
        if (collision.gameObject.CompareTag("Player"))
        {
            ApplyDamage(collision.gameObject);
        }
        
        SpawnImpactEffect(collision.contacts[0].point, collision.contacts[0].normal);
        PlayImpactSound();
        DestroyProjectile();
    }
    
    private void HandleImpact(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ApplyDamage(other.gameObject);
        }
        
        SpawnImpactEffect(transform.position, -transform.forward);
        PlayImpactSound();
        DestroyProjectile();
    }
    
    private void ApplyDamage(GameObject target)
    {
        Debug.Log($"Hit {target.name} for {damage} damage!");
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
}
