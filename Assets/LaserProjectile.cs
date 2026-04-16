using UnityEngine;

public class LaserProjectile : MonoBehaviour
{
    public float speed = 10f;
    public float lifeTime = 3f;
    public int maxBounces = 3;
    public LayerMask bounceLayerMask;

    private Vector2 direction;
    private int bounceCount = 0;

    void Start()
    {
        direction = transform.right;
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        float distanceThisFrame = speed * Time.deltaTime;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, distanceThisFrame, bounceLayerMask);

        if (hit.collider != null)
        {
            if (bounceCount < maxBounces)
            {
                transform.position = hit.point;
                direction = Vector2.Reflect(direction, hit.normal);
                transform.right = direction;
                bounceCount++;

                float remaining = distanceThisFrame - hit.distance;
                transform.position += (Vector3)(direction * remaining);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        else
        {
            transform.position += (Vector3)(direction * distanceThisFrame);
        }
    }
}