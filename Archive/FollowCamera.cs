using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    void Update()
    {
        if (GameObject.FindGameObjectWithTag("Player") == null)
        {
            return;
        }
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        Vector3 directionToPlayer = player.transform.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(-directionToPlayer);
        transform.rotation = Quaternion.Euler(0, lookRotation.eulerAngles.y, 0);
    }
}
