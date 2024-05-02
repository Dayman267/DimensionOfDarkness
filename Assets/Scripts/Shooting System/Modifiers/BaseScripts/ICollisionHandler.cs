using UnityEngine;

public interface ICollisionHandler
{
    void HandleImpact(
        Collider ImpactedObject,
        Vector3 HitPosition,
        Vector3 HitNormal,
        GunSO Gun
    );
}
