using UnityEngine;

public class TopDownCamera : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;      
    public Vector3 offset = new(0, 10, -6);

    [Header("Follow Settings")]
    public float followSpeed = 5f;    
    public float rotationSmooth = 10f;
    public bool lookAtTarget = true;    

    [Header("Optional")]
    public bool lockRotationY = true;

    [Header("Angle")]
    public Vector3 angle = new(50f, 0f, 0f);
    private Vector3 velocity;         

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;

        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, 1f / followSpeed);

        if (lookAtTarget)
        {
            Vector3 dir = target.position - transform.position;
            dir.y = 0;
            if (dir.sqrMagnitude > 0.01f)
            {
                Quaternion desiredRot = Quaternion.LookRotation(dir);
                if (lockRotationY)
                    desiredRot = Quaternion.Euler(angle);
                transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, rotationSmooth * Time.deltaTime);
            }
        }
    }
}
