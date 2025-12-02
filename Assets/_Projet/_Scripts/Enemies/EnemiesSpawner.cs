using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemiesSpawner : MonoBehaviour
{
    [SerializeField] float spawnFrequency = 1f;
    [SerializeField] float rangeSpawn = 10f;

    [SerializeField] int maxEnemiesPerTemporality = 100;

    [SerializeField] Enemy[] enemiesPrefab = new Enemy[3];

    public List<Enemy>[] temporalityEnemies = new List<Enemy>[3];

    void Start()
    {
        for (int i = 0; i < temporalityEnemies.Length; i++)
        {
            temporalityEnemies[i] = new List<Enemy>();
        }

        if(GameManager.instance.enemiesSpawner == null)
        {
            GameManager.instance.enemiesSpawner = this;
        }

        StartCoroutine(SpawnEnemies());
    }

    IEnumerator SpawnEnemies()
    {
        yield return new WaitForSeconds(spawnFrequency);

        while (true)
        {
            int temporalityIndex = Random.Range(0, 3);

            Enemy enemy;

            Debug.Log("Spawn enemy for temporality " + temporalityIndex);

            if (enemiesPrefab[temporalityIndex] == null)
            {
                Debug.LogError("Enemy prefab for " + temporalityIndex + " is not assigned.");
                yield break;
            }

            enemy = Instantiate(enemiesPrefab[temporalityIndex], GetPointOnCircle(transform.position, rangeSpawn, Random.Range(0, 360)), Quaternion.identity, transform);
            temporalityEnemies[temporalityIndex].Add(enemy);

            yield return new WaitForSeconds(spawnFrequency);
        }
    }

    public Vector3 GetPointOnCircle(Vector3 center, float radius, float angleDegrees)
    {
        float angleRad = angleDegrees * Mathf.Deg2Rad;

        // Cercle sur le plan XZ 
        return new Vector3(center.x + Mathf.Cos(angleRad) * radius, center.y, center.z + Mathf.Sin(angleRad) * radius);
    }

    private void OnDrawGizmos()
    {
        if(GameManager.instance != null)
        {
            Gizmos.DrawWireSphere(transform.position, GameManager.instance.distanceTimelessZone);


        }
    }
}
