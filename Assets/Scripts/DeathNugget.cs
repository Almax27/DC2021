
using UnityEngine;
using Utils;

[RequireComponent(typeof(Rigidbody))]
public class DeathNugget : MonoBehaviour
{
    public MinMaxFloat SpawnSpeedRange = new MinMaxFloat(1,20);
    public MinMaxFloat SpawnRotSpeedRange = new MinMaxFloat(-180, 180);

    private void Awake()
    {
        var body = GetComponent<Rigidbody>();

        Vector3 dir = Random.onUnitSphere;
        dir.y = Mathf.Abs(dir.y);
        body.velocity = dir * SpawnSpeedRange.Random;
        body.angularVelocity = new Vector3(SpawnRotSpeedRange.Random, SpawnRotSpeedRange.Random, SpawnRotSpeedRange.Random);

        transform.rotation = Random.rotation;
        transform.position += (Random.value > 0.5f ? 1.0f : -1.0f) * Random.insideUnitSphere * 0.1f;

    }
}