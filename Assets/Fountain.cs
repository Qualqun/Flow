using UnityEngine;

public class Fountain : MonoBehaviour
{
    public int HealAmount = 5;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerStay(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            other.gameObject.GetComponent<PlayerController>().Heal(HealAmount);
        }
    }
}
