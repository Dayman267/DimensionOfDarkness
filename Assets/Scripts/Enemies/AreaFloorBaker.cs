using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class AreaFloorBaker : MonoBehaviour
{
    [SerializeField] private List<NavMeshSurface> Surfaces;

    [SerializeField] private PlayerController Player;

    [SerializeField] private float UpdateRate = 0.1f;

    [SerializeField] private float MovementThreshold = 3f;

    [SerializeField] private Vector3 NavMeshSize = new(50, 50, 50);
    private readonly List<NavMeshBuildSource> Sources = new();

    private List<NavMeshData> NavMeshDataList;

    private Vector3 WorldAnchor;

    private void Start()
    {
        NavMeshDataList = new List<NavMeshData>();

        foreach (var surface in Surfaces)
        {
            var navMeshData = new NavMeshData();
            NavMesh.AddNavMeshData(navMeshData);
            NavMeshDataList.Add(navMeshData);
        }

        BuildNavMeshes(false);
        StartCoroutine(CheckPlayerMovement());
    }

    private IEnumerator CheckPlayerMovement()
    {
        var Wait = new WaitForSeconds(UpdateRate);

        while (true)
        {
            if (Vector3.Distance(WorldAnchor, Player.transform.position) > MovementThreshold)
            {
                BuildNavMeshes(true);
                WorldAnchor = Player.transform.position;
            }

            yield return Wait;
        }
    }

    private void BuildNavMeshes(bool Async)
    {
        for (var i = 0; i < Surfaces.Count; i++)
        {
            var surface = Surfaces[i];
            var navMeshData = NavMeshDataList[i];

            var navMeshBounds = new Bounds(Player.transform.position, NavMeshSize);

            IEnumerable<NavMeshModifier> modifiers = surface.collectObjects == CollectObjects.Children
                ? GetComponentsInChildren<NavMeshModifier>()
                : NavMeshModifier.activeModifiers;

            var markups = modifiers
                .Where(t => (surface.layerMask & (1 << t.gameObject.layer)) != 0 &&
                            t.AffectsAgentType(surface.agentTypeID))
                .Select(t => new NavMeshBuildMarkup
                {
                    root = t.transform,
                    overrideArea = t.overrideArea,
                    area = t.area,
                    ignoreFromBuild = t.ignoreFromBuild
                }).ToList();

            if (surface.collectObjects == CollectObjects.Children)
                NavMeshBuilder.CollectSources(transform, surface.layerMask, surface.useGeometry, surface.defaultArea,
                    markups, Sources);
            else
                NavMeshBuilder.CollectSources(navMeshBounds, surface.layerMask, surface.useGeometry,
                    surface.defaultArea, markups, Sources);

            Sources.RemoveAll(source =>
                source.component != null && source.component.gameObject.GetComponent<NavMeshAgent>() != null);

            if (Async)
                NavMeshBuilder.UpdateNavMeshDataAsync(navMeshData, surface.GetBuildSettings(), Sources,
                    new Bounds(Player.transform.position, NavMeshSize));
            else
                NavMeshBuilder.UpdateNavMeshData(navMeshData, surface.GetBuildSettings(), Sources,
                    new Bounds(Player.transform.position, NavMeshSize));
        }
    }
}