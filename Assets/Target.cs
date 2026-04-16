using UnityEngine;

public class Target : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<LaserProjectile>() != null)
        {
            Destroy(other.gameObject);
            GameManager.instance.LevelComplete();
        }
    }
}