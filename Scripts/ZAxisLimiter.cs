
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ZAxisLimiter : MonoBehaviour
{
    public float minZ = -2f;
    public float maxZ = 2f;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        Vector3 pos = rb.position;

        // 限制物体在指定的Z轴范围内
        pos.z = Mathf.Clamp(pos.z, minZ, maxZ);

        rb.position = pos;

        // 超出边界时，停止继续向外运动
        Vector3 velocity = rb.velocity;

        if (rb.position.z <= minZ && velocity.z < 0)
        {
            velocity.z = 0;
        }

        if (rb.position.z >= maxZ && velocity.z > 0)
        {
            velocity.z = 0;
        }

        rb.velocity = velocity;
    }
}
