using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "Shoot Config", menuName = "Guns/Shoot Configuration", order = 2)]
public class ShootConfigurationSO : ScriptableObject, ICloneable
{
    public bool IsHitScan = true;
    public Bullet BulletPrefab;
    public float BulletSpawnForce = 1000;
    public LayerMask HitMask;
    public float FireRate = 0.25f;
    public int BulletPerShot = 1;

    public ShootType ShootType = ShootType.FromGun;

    public bool IsPreparedShot;
    public float chargeTime = 1.0f;


    public float RecoilRecoverySpeed = 1f;
    public float MaxSpreadTime = 1f;
    public BulletSpreadType SpreadType = BulletSpreadType.Simple;

    [Header("Simple Spread")] public Vector3 Spread = new(0.1f, 0.1f, 0.1f);

    public Vector3 MinSpread = Vector3.zero;

    [Header("Texture-Based Spread")] [Range(0.001f, 5f)]
    public float SpreadMultiplier = 0.1f;

    public Texture2D SpreadTexture;

    public object Clone()
    {
        var config = CreateInstance<ShootConfigurationSO>();

        Utilities.CopyValues(this, config);
        return config;
    }

    public Vector3 GetSpread(float ShootTime = 0)
    {
        var spread = Vector3.zero;

        if (SpreadType == BulletSpreadType.Simple)
        {
            spread = Vector3.Lerp(
                new Vector3(
                    Random.Range(-MinSpread.x, MinSpread.x),
                    Random.Range(-MinSpread.y, MinSpread.y),
                    Random.Range(-MinSpread.z, MinSpread.z)
                ),
                new Vector3(
                    Random.Range(-Spread.x, Spread.x),
                    Random.Range(-Spread.y, Spread.y),
                    Random.Range(-Spread.z, Spread.z)
                ),
                Mathf.Clamp01(ShootTime / MaxSpreadTime) //Чем дольше спрей тем больше разброс
            );
        }
        else if (SpreadType == BulletSpreadType.TextureBased)
        {
            spread = GetTextureDirection(ShootTime);
            spread *= SpreadMultiplier;
        }

        return spread;
    }

    private Vector3 GetTextureDirection(float ShootTime)
    {
        var halfSize = new Vector2(SpreadTexture.width / 2f, SpreadTexture.height / 2f);
        var halfSquareExtends = Mathf.CeilToInt(
            Mathf.Lerp(
                0.01f,
                halfSize.x,
                Mathf.Clamp01(ShootTime / MaxSpreadTime)
            )
        );

        var minX = Mathf.FloorToInt(halfSize.x) - halfSquareExtends;
        var minY = Mathf.FloorToInt(halfSize.y) - halfSquareExtends;

        var sampleColors = SpreadTexture.GetPixels(
            minX,
            minY,
            halfSquareExtends * 2,
            halfSquareExtends * 2
        );

        var colorsAsGrey = Array.ConvertAll(sampleColors, color => color.grayscale);
        var totalGreyValue = colorsAsGrey.Sum();

        var gray = Random.Range(0, totalGreyValue);
        var i = 0;
        for (; i < colorsAsGrey.Length; i++)
        {
            gray -= colorsAsGrey[i];
            if (gray <= 0) break;
        }

        var x = minX + i % (halfSquareExtends * 2);
        var y = minY + i / (halfSquareExtends * 2);
        var targetPosition = new Vector2(x, y);
        var direction = (targetPosition - halfSize) / halfSize.x;

        return direction;
    }
}