using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform target; // drag your player in here
    private Vector3 offset;

    void Start()
    {
        offset = transform.position - target.position;
    }

    void LateUpdate()
    {
        transform.position = target.position + offset;
        // rotation is never touched, so it stays locked
    }
}
