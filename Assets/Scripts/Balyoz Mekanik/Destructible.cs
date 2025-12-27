using UnityEngine;

public class Destructible : MonoBehaviour
{
    [Header("Saat Ayarları")]
    public bool isClock = true; 

    [Header("Parçalanma Efekti")]
    public GameObject fracturedPrefab;

    [Header("Fizik Ayarları")]
    public float explosionForce = 400f;
    public float explosionRadius = 3f;
    public float explosionUpward = 0.5f;

    private bool isBroken = false;

    public void Break(Vector3 hitPoint)
    {
        if (isBroken) return;
        
        if (Clocks.instance != null && Clocks.instance.gameOver) return;

        isBroken = true;

        if (fracturedPrefab != null)
        {
            GameObject fractured = Instantiate(
                fracturedPrefab, 
                transform.position, 
                transform.rotation
            );

            fractured.transform.localScale = transform.localScale;

            Rigidbody[] rbs = fractured.GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody rb in rbs)
            {
                rb.AddExplosionForce(explosionForce, hitPoint, explosionRadius, explosionUpward);
            }
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: Fractured Prefab atanmadı!");
        }

        if (isClock && Clocks.instance != null)
        {
            Clocks.instance.ClockBroken();
        }

        Destroy(gameObject);
    }

    public void Break()
    {
        Break(transform.position);
    }
}