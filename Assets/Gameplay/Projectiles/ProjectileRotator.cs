using UnityEngine;


public class ProjectileRotator : MonoBehaviour
{
    [SerializeField]
    float speed = 10;
    private void Update()
    {
        transform.Rotate(new Vector3(0, 0, speed * Time.deltaTime));
    }
}

