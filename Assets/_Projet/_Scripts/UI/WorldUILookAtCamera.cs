using UnityEngine;

[ExecuteAlways]
public class WorldUIFollow : MonoBehaviour
{
    [Tooltip("the target")]
    public Transform target;

    [Tooltip("off-set on top of the player")]
    public Vector3 offset = new(0, 2.2f, 0);

    [Tooltip("Smoothing Follow")]
    [Range(0f, 10f)] public float followSmooth = 0f;

    private Transform cam;
    private Vector3 velocity;

    void Start()
    {
        if (Camera.main != null)
            cam = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        if (cam == null && Camera.main != null)
            cam = Camera.main.transform;

        Vector3 targetPos = target.position + offset;

        if (followSmooth > 0f)
            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, 1f / followSmooth);
        else
            transform.position = targetPos;

        if (cam != null)
            transform.forward = -cam.forward;
    }
}
