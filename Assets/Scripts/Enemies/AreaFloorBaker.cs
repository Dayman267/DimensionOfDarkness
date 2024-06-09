using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class AreaFloorBaker : MonoBehaviour
{
    [SerializeField] private NavMeshSurface[] Surfaces;
    [SerializeField] private PlayerController Player;
    [SerializeField] private float UpdateRate = 0.1f;
    [SerializeField] private float MovementThreshold = 3f;
    [SerializeField] private Vector3 NavMeshSize = new(20, 20, 20);
    [SerializeField] private bool CacheSources;

    public delegate void NavMeshUpdatedEvent(Bounds Bounds);

    public NavMeshUpdatedEvent OnNavMeshUpdate;
    
    private NavMeshData[] NavMeshDatas;

    private Dictionary<int, List<NavMeshBuildSource>> SourcesPerSurface = new();
    private Dictionary<int, List<NavMeshBuildMarkup>> MarkupsPerSurface = new();
    private Dictionary<int, List<NavMeshModifier>> ModifiersPerSurface = new();
    
    private Vector3 WorldAnchor;

    private void Awake()
    {
        NavMeshDatas = new NavMeshData[Surfaces.Length];
        for (var i = 0; i < Surfaces.Length; i++)
        {
            NavMeshDatas[i] = new NavMeshData();
            NavMesh.AddNavMeshData(NavMeshDatas[i]);
            SourcesPerSurface[i] = new List<NavMeshBuildSource>();
            MarkupsPerSurface[i] = new List<NavMeshBuildMarkup>();
            ModifiersPerSurface[i] = new List<NavMeshModifier>();
        }

        WorldAnchor = Player.transform.position;  // Установить начальную позицию игрока
        BuildNavMesh(false);
        StartCoroutine(CheckPlayerMovement());
    }

    private IEnumerator CheckPlayerMovement()
    {
        var wait = new WaitForSeconds(UpdateRate);
        float movementThresholdSquared = MovementThreshold * MovementThreshold;

        while (true)
        {
            if ((WorldAnchor - Player.transform.position).sqrMagnitude > movementThresholdSquared)
            {
                BuildNavMesh(true);
                WorldAnchor = Player.transform.position;
            }

            yield return wait;
        }
    }

    private void BuildNavMesh(bool async)
    {
        var navMeshBounds = new Bounds(Player.transform.position, NavMeshSize);

        for (var index = 0; index < Surfaces.Length; index++)
        {
            CollectModifiers(index);
            if (!CacheSources || SourcesPerSurface[index].Count == 0)
            {
                CollectSources(index, navMeshBounds);
            }

            if (async)
            {
                AsyncOperation navMeshUpdateOperation = NavMeshBuilder.UpdateNavMeshDataAsync(NavMeshDatas[index],
                    Surfaces[index].GetBuildSettings(), SourcesPerSurface[index], navMeshBounds);
                navMeshUpdateOperation.completed += HandleNavMeshUpdate;
            }
            else
            {
                NavMeshBuilder.UpdateNavMeshData(NavMeshDatas[index], Surfaces[index].GetBuildSettings(), SourcesPerSurface[index],
                    navMeshBounds);
                OnNavMeshUpdate?.Invoke(navMeshBounds);
            }
        }
    }


    private void CollectModifiers(int index)
    {
        if (ModifiersPerSurface[index].Count == 0)
        {
            if (Surfaces[index].collectObjects == CollectObjects.Children)
            {
                ModifiersPerSurface[index].AddRange(GetComponentsInChildren<NavMeshModifier>());
            }
            else
            {
                ModifiersPerSurface[index].AddRange(NavMeshModifier.activeModifiers);
            }
        }

        if (MarkupsPerSurface[index].Count == 0)
        {
            foreach (var modifier in ModifiersPerSurface[index])
            {
                if ((Surfaces[index].layerMask & (1 << modifier.gameObject.layer)) != 0 && 
                    modifier.AffectsAgentType(Surfaces[index].agentTypeID))
                {
                    MarkupsPerSurface[index].Add(new NavMeshBuildMarkup
                    {
                        root = modifier.transform,
                        overrideArea = modifier.overrideArea,
                        area = modifier.area,
                        ignoreFromBuild = modifier.ignoreFromBuild
                    });
                }
            }
        }
    }

    private void CollectSources(int index, Bounds navMeshBounds)
    {
        if (Surfaces[index].collectObjects == CollectObjects.Children)
        {
            NavMeshBuilder.CollectSources(transform, Surfaces[index].layerMask, Surfaces[index].useGeometry, Surfaces[index].defaultArea,
                MarkupsPerSurface[index], SourcesPerSurface[index]);
        }
        else
        {
            NavMeshBuilder.CollectSources(navMeshBounds, Surfaces[index].layerMask, Surfaces[index].useGeometry,
                Surfaces[index].defaultArea, MarkupsPerSurface[index], SourcesPerSurface[index]);
        }
    }

    private void HandleNavMeshUpdate(AsyncOperation operation)
    {
        OnNavMeshUpdate?.Invoke(new Bounds(WorldAnchor, NavMeshSize));
    }
}
