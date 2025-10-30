using UnityEngine;
using UnityEngine.AI;

public class NewMonoBehaviourScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    NavMeshAgent agent;

    void Start()
    {
        Debug.Log("Ben Doğdum");

        agent = GetComponent<NavMeshAgent>();

        

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
