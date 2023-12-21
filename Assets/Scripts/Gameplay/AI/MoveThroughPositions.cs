using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveThroughPositions : MonoBehaviour
{
    public List<Vector3> targetPositions;
    public float speed = 10f;
    public float rotationSpeed = 8f; // Adjust the rotation speed

    private int currentIndex = 0;

    void Update()
    {
        if (targetPositions.Count > 0)
        {
            MoveTowardsTarget(targetPositions[currentIndex]);

            if (Vector3.Distance(transform.position, targetPositions[currentIndex]) < 0.1f)
            {
                currentIndex = (currentIndex + 1) % targetPositions.Count;
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

        Vector3 direction = (target - transform.position).normalized;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}
