using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(AudioSource))]
public class Health : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    [Tooltip("Bu değeri değiştirmene gerek yok, 'Awake'te ayarlanır.")]
    [SerializeField] private float currentHealth;

    // --- HATA ÇÖZÜMÜ: EKSİK OLAN KISIM BURASI ---
    // 'SecurityAI' script'inin canı OKUMASINI sağlayan public özellik
    public float CurrentHealth
    {
        get { return currentHealth; }
    }
    // --- ÇÖZÜM BİTTİ ---

    [Header("FX")]
    public AudioClip deathSound;
    private AudioSource audioSource;
    private bool isDead = false;

    public UnityEvent OnDie;

    void Awake()
    {
        currentHealth = maxHealth;
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        isDead = false;
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        Debug.Log(gameObject.name + " " + amount + " hasar aldı, Canı: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log(gameObject.name + " öldü.");

        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        OnDie.Invoke();

        if (gameObject.CompareTag("Guard"))
        {
            UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent) agent.enabled = false;

            Animator anim = GetComponentInChildren<Animator>();
            if (anim)
            {
                anim.SetBool("isDead", true);
            }

            SecurityAI ai = GetComponent<SecurityAI>();
            if (ai) ai.enabled = false;

            Destroy(gameObject, 5f);
        }
        else if (gameObject.CompareTag("Player"))
        {
            Debug.Log("OYUN BİTTİ");
        }
    }
}