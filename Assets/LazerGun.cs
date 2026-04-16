using UnityEngine;

public class LaserGun : MonoBehaviour
{
    public Transform firePoint;
    public GameObject laserPrefab;

    private float nextFireTime;

    void Update()
    {
        RotateToMouse();

        if (Input.GetMouseButtonDown(0) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + 1f;
        }
    }

    void RotateToMouse()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        Vector2 direction = mousePos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void Shoot()
    {
        if (laserPrefab == null || firePoint == null) return;

        GameObject laser = Instantiate(laserPrefab, firePoint.position, firePoint.rotation);

        Physics2D.IgnoreCollision(
            laser.GetComponent<Collider2D>(),
            GetComponent<Collider2D>()
        );
    }
}