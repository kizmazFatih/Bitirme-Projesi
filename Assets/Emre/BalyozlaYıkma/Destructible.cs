using UnityEngine;

public class Destructible : MonoBehaviour
{
    public GameObject fracturedPrefab;
    public float explosionForce = 300f;
    public float explosionRadius = 2f;
    public float explosionUpward = 0.5f;

    private bool isBroken = false;

    public void Break(Vector3 hitPoint)
    {
        if (InventoryController.instance.player_inventory.slots[Handle.instance.index].prefab == null) return;
        if (isBroken || InventoryController.instance.player_inventory.slots[Handle.instance.index].prefab.tag != "SledgeHammer") return;
        isBroken = true;

        if (fracturedPrefab == null)
        {
            Debug.LogWarning("Destructible: fracturedPrefab atanmadı!");
            return;
        }

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
        Clocks.instance.AddBrokenClock();
        Destroy(gameObject);
    }

    public void Break()
    {
        Break(transform.position);

    }
}
