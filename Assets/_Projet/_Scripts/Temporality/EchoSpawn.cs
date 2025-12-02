using UnityEngine;

public class EchoSpawn : MonoBehaviour
{
    [SerializeField] GameObject echoPrefab;

    public void Start()
    {
        if(GameManager.instance.echoSpawn == null)
        {
            GameManager.instance.echoSpawn = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SpawnEcho(Transform transform, int numberPatern)
    {
        //Instantiate the echo prefab and set the numberPatern
        Instantiate(echoPrefab, transform.position, transform.rotation, this.transform).GetComponent<EchoBehaviour>().SetPatern(numberPatern);
    }
}
