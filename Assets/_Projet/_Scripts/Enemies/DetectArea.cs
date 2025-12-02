using UnityEngine;

public class DetectArea : MonoBehaviour
{
    [SerializeField] float detectRadius = 5f;
    [SerializeField] float loseRadius = 7f;

    SphereCollider areaCollider;
    GameObject currentTarget;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        areaCollider = GetComponent<SphereCollider>();

        if (areaCollider == null)
        {
            areaCollider = gameObject.AddComponent<SphereCollider>();
            areaCollider.isTrigger = true;
            areaCollider.radius = detectRadius;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (currentTarget != null)
        {
            if (Vector3.Distance(transform.position, currentTarget.transform.position) > loseRadius)
            {
                currentTarget = null;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            currentTarget = other.gameObject;
        }
    }

    public GameObject GetCurrentTarget()
    {
        return currentTarget;
    }
}
