using UnityEngine;


public class PaintingTrigger : MonoBehaviour
{
    public float timeToSteal = 4.0f;
    private float stealTimer = 0f;
    private bool playerIsInside = false;



    void Start()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player") && !SecurityAI.isAlarmTriggered)
        {
            playerIsInside = true;
            stealTimer = 0f;
        }
    }

    private void OnTriggerExit(Collider other)
    {

        if (other.CompareTag("Player"))
        {
            playerIsInside = false;
            stealTimer = 0f;

        }
    }

    void Update()
    {

        if (playerIsInside && !SecurityAI.isAlarmTriggered)
        {
            stealTimer += Time.deltaTime;



            if (stealTimer >= timeToSteal)
            {

                Debug.Log("ALARM! Tablo çalýndý!");


                SecurityAI.isAlarmTriggered = true;

                playerIsInside = false;

            }
        }
    }
}
