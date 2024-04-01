using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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

    public ShootType ShootType = ShootType.FromGun;
    
    public float RecoilRecoverySpeed = 1f;
    public float MaxSpreadTime = 1f;
    public BulletSpreadType SpreadType = BulletSpreadType.Simple;
    [Header("Simple Spread")] 
    public Vector3 Spread = new Vector3(0.1f, 0.1f, 0.1f);

    [Header("Texture-Based Spread")] [Range(0.001f, 5f)]
    public float SpreadMultiplier = 0.1f;

    public Texture2D SpreadTexture;

    public Vector3 GetSpread(float ShootTime = 0)
    {
        Vector3 spread = Vector3.zero;

        if (SpreadType == BulletSpreadType.Simple)
        {
            spread = Vector3.Lerp(
                Vector3.zero,
                new Vector3(
                    Random.Range(
                        -Spread.x,
                        Spread.x
                    ),
                    Random.Range(
                        -Spread.y,
                        Spread.y
                    ),
                    Random.Range(
                        -Spread.z,
                        Spread.z
                    )
                ),
                Mathf.Clamp01(ShootTime / MaxSpreadTime) //Чем дольше спрей тем дольше разброс
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
        Vector2 halfSize = new Vector2(SpreadTexture.width / 2f, SpreadTexture.height / 2f);
        int halfSquareExtends = Mathf.CeilToInt(
            Mathf.Lerp(
                0.01f,
                halfSize.x,
                Mathf.Clamp01(ShootTime / MaxSpreadTime)
            )
        );

        int minX = Mathf.FloorToInt(halfSize.x) - halfSquareExtends;
        int minY = Mathf.FloorToInt(halfSize.y) - halfSquareExtends;

        Color[] sampleColors = SpreadTexture.GetPixels(
            minX,
            minY,
            halfSquareExtends * 2,
            halfSquareExtends * 2
        );

        float[] colorsAsGrey = System.Array.ConvertAll(sampleColors, (color) => color.grayscale);
        float totalGreyValue = colorsAsGrey.Sum();

        float gray = Random.Range(0, totalGreyValue);
        int i = 0;
        for (; i < colorsAsGrey.Length; i++)
        {
            gray -= colorsAsGrey[i];
            if (gray <= 0)
            {
                break;
            }
        }

        int x = minX + i % (halfSquareExtends * 2);
        int y = minY + i / (halfSquareExtends * 2);
        Vector2 targetPosition = new Vector2(x, y);
        Vector2 direction = (targetPosition - halfSize) / halfSize.x;

        return direction;
    }

    public object Clone()
    {
        ShootConfigurationSO config = CreateInstance<ShootConfigurationSO>();

        Utilities.CopyValues(this, config);
        return config;
    }
}