using Enemies;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "Enemy Configuration", menuName = "ScriptableObject/Enemy Configuration")]
public class EnemyScriptableObject : ScriptableObject
{
    public Enemy Prefab;
    public AttackScriptableObject AttackConfiguration;
    
    // Enemy Stats
    public int Health = 100;
    public int PointsForKill = 10;
    
    // Movements Stats
    public EnemyState DefaultState;
    public float IdleLocationRadius = 4f;
    public float IdleMovespeedMultiplier = 0.6f;
    public float LineOfSightRange = 6f;
    public float FieldOfView = 90f;
    
    // NavMesh Configs
    public float AIUpdateInterval = 0.1f;
    public float Acceleration = 8;
    public float AngularSpeed = 200;
    public int AreaMask = -1;
    public int AvoidancePriority = 50;
    public float BaseOffset;
    public float Height = 2f;
    public ObstacleAvoidanceType ObstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
    public float Radius = 0.5f;
    public float Speed = 3f;
    public float StoppingDistance = 0.5f;
    
    /*public EnemyScriptableObject ScaleUpForPoints(ScalingScriptableObject scaling, int darkEnergyPoints)
    {
        EnemyScriptableObject scaledUpEnemy = CreateInstance<EnemyScriptableObject>();

        scaledUpEnemy.name = name;
        scaledUpEnemy.Prefab = Prefab;

        scaledUpEnemy.AttackConfiguration = AttackConfiguration.ScaleUpForPoints(scaling, darkEnergyPoints);

        scaledUpEnemy.Health = Mathf.FloorToInt(Health * scaling.HealthCurve.Evaluate(darkEnergyPoints));
        //scaledUpEnemy.Speed = Speed * scaling.SpeedCurve.Evaluate(darkEnergyPoints);
        scaledUpEnemy.Speed = Speed;
        scaledUpEnemy.DefaultState = DefaultState;
        scaledUpEnemy.IdleLocationRadius = IdleLocationRadius;
        scaledUpEnemy.IdleMovespeedMultiplier = IdleMovespeedMultiplier;
        scaledUpEnemy.LineOfSightRange = LineOfSightRange;
        scaledUpEnemy.FieldOfView = FieldOfView;

        scaledUpEnemy.AIUpdateInterval = AIUpdateInterval;
        scaledUpEnemy.Acceleration = Acceleration;
        scaledUpEnemy.AngularSpeed = AngularSpeed;

        scaledUpEnemy.AreaMask = AreaMask;
        scaledUpEnemy.AvoidancePriority = AvoidancePriority;

        scaledUpEnemy.BaseOffset = BaseOffset;
        scaledUpEnemy.Height = Height;
        scaledUpEnemy.ObstacleAvoidanceType = ObstacleAvoidanceType;
        scaledUpEnemy.Radius = Radius;
        scaledUpEnemy.StoppingDistance = StoppingDistance;

        return scaledUpEnemy;
    }*/

    public void SetupEnemy(Enemy enemy)
    {
        enemy.Agent.acceleration = Acceleration;
        enemy.Agent.angularSpeed = AngularSpeed;
        enemy.Agent.areaMask = AreaMask;
        enemy.Agent.avoidancePriority = AvoidancePriority;
        enemy.Agent.baseOffset = BaseOffset;
        enemy.Agent.height = Height;
        enemy.Agent.obstacleAvoidanceType = ObstacleAvoidanceType;
        enemy.Agent.radius = Radius;
        enemy.Agent.speed = Speed;
        enemy.Agent.stoppingDistance = StoppingDistance;

        enemy.Movement.UpdateRate = AIUpdateInterval;
        enemy.Movement.DefaultState = DefaultState;
        enemy.Movement.IdleLocationRadius = IdleLocationRadius;
        enemy.Movement.IdleMovespeedMultiplier = IdleMovespeedMultiplier;
        enemy.Movement.LineOfSightChecker.FieldOfView = FieldOfView;
        enemy.Movement.LineOfSightChecker.Collider.radius = LineOfSightRange;
        //enemy.Movement.LineOfSightChecker.LineOfSightLayers = AttackConfiguration.LineOfSightLayers;
        

        enemy.Health._MaxHealth = Health;
        enemy.PointsForKill = PointsForKill;

        AttackConfiguration.SetupEnemy(enemy);
    }
}