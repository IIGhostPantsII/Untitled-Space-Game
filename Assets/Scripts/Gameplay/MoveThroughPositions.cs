using UnityEngine;

public class MoveThroughPositions : MonoBehaviour
{
    public Vector3[] targetPositions;
    public float speed = 5f;

    private int currentIndex = 0;

    void Update()
    {
        
        if (targetPositions.Length > 0)
        {
            MoveTowardsTarget(targetPositions[currentIndex]);

            if (Vector3.Distance(transform.position, targetPositions[currentIndex]) < 0.1f)
            {
                currentIndex = (currentIndex + 1) % targetPositions.Length;
            }
        }
        else
        {
            Debug.LogError("No target pos");
        }
    }

    void MoveTowardsTarget(Vector3 target)
    {
        float step = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, target, step);
    }
}
