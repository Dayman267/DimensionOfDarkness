using UnityEngine;

namespace Enemies
{
    [CreateAssetMenu(fileName = "Attack Configuration", menuName = "ScriptableObject/Attack Configuration")]
    public class AttackScriptableObject : ScriptableObject
    {
        public bool isRanged = false;
        public int Damage = 5;
        public float AttackRadius = 1.5f;
        public float AttackDelay = 1.5f;
    
        // Ranged Configs
        public EnemyBullet BulletPrefab;
        public Vector3 BulletSpawnOffset = new Vector3(0, 1, 0);
        public LayerMask LineOfSightLayers;

        /*public AttackScriptableObject ScaleUpForPoints(ScalingScriptableObject scaling, int darkEnergyPoints)
        {
            AttackScriptableObject scaledUpConfiguration = CreateInstance<AttackScriptableObject>();

            scaledUpConfiguration.isRanged = isRanged;
            scaledUpConfiguration.Damage = Mathf.FloorToInt(Damage * scaling.DamageCurve.Evaluate(darkEnergyPoints));
            scaledUpConfiguration.AttackRadius = AttackRadius;
            scaledUpConfiguration.AttackDelay = AttackDelay;

            scaledUpConfiguration.BulletPrefab = BulletPrefab;
            scaledUpConfiguration.BulletSpawnOffset = BulletSpawnOffset;
            scaledUpConfiguration.LineOfSightLayers = LineOfSightLayers;


            return scaledUpConfiguration;
        }*/
        
        public void SetupEnemy(Enemy enemy)
        {
            enemy.AttackRadius.Collider.radius = AttackRadius;
            enemy.AttackRadius.AttackDelay = AttackDelay;
            enemy.AttackRadius.Damage = Damage;

            if (isRanged)
            {
                RangedAttackRadius rangedAttackRadius = enemy.AttackRadius.GetComponent<RangedAttackRadius>();

                rangedAttackRadius.CreateBulletPool();
                rangedAttackRadius.BulletPrefab = BulletPrefab;
                rangedAttackRadius.BulletSpawnOffset = BulletSpawnOffset;
                rangedAttackRadius.Mask = LineOfSightLayers;
                
                
            }
        }
    }
}
