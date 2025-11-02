using UnityEngine;
using UnityEngine.Events; 

public class Health : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;


    public UnityEvent OnDie;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {

        if (currentHealth <= 0) return;

        currentHealth -= amount;
        Debug.Log(gameObject.name + " " + amount + " hasar ald�, Can�: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " �ld�.");


        OnDie.Invoke();


        if (gameObject.CompareTag("Guard"))
        {

            UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent) agent.enabled = false;


            Animator anim = GetComponentInChildren<Animator>();
            if (anim) anim.SetTrigger("Die");


            this.enabled = false;
            SecurityAI ai = GetComponent<SecurityAI>();
            if (ai) ai.enabled = false;


            Destroy(gameObject, 5f);
        }
        else if (gameObject.CompareTag("Player"))
        {

            Debug.Log("OYUN B�TT�");

        }
    }
}