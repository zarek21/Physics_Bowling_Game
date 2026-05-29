using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject DefaultBallPrefab;

    private void Start()
    {
        GameObject prefabToSpawn = BallSelector.SelectedBallPrefab;
        if (prefabToSpawn == null)
        {
            prefabToSpawn = DefaultBallPrefab;
        }

        if (prefabToSpawn != null)
        {
            BallPhysics[] existingBalls = FindObjectsByType<BallPhysics>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var oldBall in existingBalls)
            {
                Destroy(oldBall.gameObject);
            }

            GameObject spawnedBall = Instantiate(prefabToSpawn, transform.position, transform.rotation);
            BallPhysics bp = spawnedBall.GetComponent<BallPhysics>();
            
            if (bp != null)
            {
                UIManager ui = FindFirstObjectByType<UIManager>();
                if (ui != null)
                {
                    ui.SetupNewBall(bp);
                }
            }
        }
    }
}
