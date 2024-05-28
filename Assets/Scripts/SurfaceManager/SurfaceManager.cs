using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace LlamAcademy.ImpactSystem
{
    public class SurfaceManager : MonoBehaviour
    {
        [SerializeField] private Transform objectParent;

        [SerializeField] private List<SurfaceType> Surfaces = new();
        [SerializeField] private Surface DefaultSurface;
        private readonly Dictionary<GameObject, ObjectPool<GameObject>> ObjectPools = new();

        public static SurfaceManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError("More than one SurfaceManager active in the scene! Destroying latest one: " + name);
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void HandleImpact(GameObject HitObject, Vector3 HitPoint, Vector3 HitNormal, ImpactType Impact,
            int TriangleIndex)
        {
            if (HitObject.TryGetComponent(out Terrain terrain))
            {
                var activeTextures = GetActiveTexturesFromTerrain(terrain, HitPoint);
                foreach (var activeTexture in activeTextures)
                {
                    var surfaceType = Surfaces.Find(surface => surface.Albedo == activeTexture.Texture);
                    if (surfaceType != null)
                    {
                        foreach (var typeEffect in surfaceType.Surface.ImpactTypeEffects)
                            if (typeEffect.ImpactType == Impact)
                                PlayEffects(HitPoint, HitNormal, typeEffect.SurfaceEffect, activeTexture.Alpha);
                    }
                    else
                    {
                        foreach (var typeEffect in DefaultSurface.ImpactTypeEffects)
                            if (typeEffect.ImpactType == Impact)
                                PlayEffects(HitPoint, HitNormal, typeEffect.SurfaceEffect, 1);
                    }
                }
            }
            else if (HitObject.TryGetComponent(out Renderer renderer))
            {
                var activeTexture = GetActiveTextureFromRenderer(renderer, TriangleIndex);

                var surfaceType = Surfaces.Find(surface => surface.Albedo == activeTexture);
                if (surfaceType != null)
                {
                    foreach (var typeEffect in surfaceType.Surface.ImpactTypeEffects)
                        if (typeEffect.ImpactType == Impact)
                            PlayEffects(HitPoint, HitNormal, typeEffect.SurfaceEffect, 1);
                }
                else
                {
                    foreach (var typeEffect in DefaultSurface.ImpactTypeEffects)
                        if (typeEffect.ImpactType == Impact)
                            PlayEffects(HitPoint, HitNormal, typeEffect.SurfaceEffect, 1);
                }
            }
        }

        private List<TextureAlpha> GetActiveTexturesFromTerrain(Terrain Terrain, Vector3 HitPoint)
        {
            var terrainPosition = HitPoint - Terrain.transform.position;
            var splatMapPosition = new Vector3(
                terrainPosition.x / Terrain.terrainData.size.x,
                0,
                terrainPosition.z / Terrain.terrainData.size.z
            );

            var x = Mathf.FloorToInt(splatMapPosition.x * Terrain.terrainData.alphamapWidth);
            var z = Mathf.FloorToInt(splatMapPosition.z * Terrain.terrainData.alphamapHeight);

            var alphaMap = Terrain.terrainData.GetAlphamaps(x, z, 1, 1);

            var activeTextures = new List<TextureAlpha>();
            for (var i = 0; i < alphaMap.Length; i++)
                if (alphaMap[0, 0, i] > 0)
                    activeTextures.Add(new TextureAlpha
                    {
                        Texture = Terrain.terrainData.terrainLayers[i].diffuseTexture,
                        Alpha = alphaMap[0, 0, i]
                    });

            return activeTextures;
        }

        private Texture GetActiveTextureFromRenderer(Renderer Renderer, int TriangleIndex)
        {
            if (Renderer.TryGetComponent(out MeshFilter meshFilter))
            {
                var mesh = meshFilter.mesh;

                return GetTextureFromMesh(mesh, TriangleIndex, Renderer.sharedMaterials);
            }

            if (Renderer is SkinnedMeshRenderer)
            {
                var smr = (SkinnedMeshRenderer)Renderer;
                var mesh = smr.sharedMesh;

                return GetTextureFromMesh(mesh, TriangleIndex, Renderer.sharedMaterials);
            }

            Debug.LogError(
                $"{Renderer.name} has no MeshFilter or SkinnedMeshRenderer! Using default impact effect instead of texture-specific one because we'll be unable to find the correct texture!");
            return null;
        }

        private Texture GetTextureFromMesh(Mesh Mesh, int TriangleIndex, Material[] Materials)
        {
            if (Mesh.subMeshCount > 1)
            {
                int[] hitTriangleIndices =
                {
                    Mesh.triangles[TriangleIndex * 3],
                    Mesh.triangles[TriangleIndex * 3 + 1],
                    Mesh.triangles[TriangleIndex * 3 + 2]
                };

                for (var i = 0; i < Mesh.subMeshCount; i++)
                {
                    var submeshTriangles = Mesh.GetTriangles(i);
                    for (var j = 0; j < submeshTriangles.Length; j += 3)
                        if (submeshTriangles[j] == hitTriangleIndices[0]
                            && submeshTriangles[j + 1] == hitTriangleIndices[1]
                            && submeshTriangles[j + 2] == hitTriangleIndices[2])
                            return Materials[i].mainTexture;
                }
            }

            return Materials[0].mainTexture;
        }

        private void PlayEffects(Vector3 HitPoint, Vector3 HitNormal, SurfaceEffect SurfaceEffect, float SoundOffset)
        {
            foreach (var spawnObjectEffect in SurfaceEffect.SpawnObjectEffects)
                if (spawnObjectEffect.Probability > Random.value)
                {
                    if (!ObjectPools.ContainsKey(spawnObjectEffect.Prefab))
                        ObjectPools.Add(spawnObjectEffect.Prefab, new ObjectPool<GameObject>(() =>
                        {
                            // Создаем объект с правильным родителем
                            var spawnedObject = Instantiate(spawnObjectEffect.Prefab, objectParent);
                            return spawnedObject;
                        }));

                    var instance = ObjectPools[spawnObjectEffect.Prefab].Get();

                    if (instance.TryGetComponent(out PoolableObject poolable))
                        poolable.Parent = ObjectPools[spawnObjectEffect.Prefab];

                    instance.SetActive(true);
                    instance.transform.position = HitPoint + HitNormal * 0.001f;
                    instance.transform.forward = HitNormal;

                    if (spawnObjectEffect.RandomizeRotation)
                    {
                        var offset = new Vector3(
                            Random.Range(0, 180 * spawnObjectEffect.RandomizedRotationMultiplier.x),
                            Random.Range(0, 180 * spawnObjectEffect.RandomizedRotationMultiplier.y),
                            Random.Range(0, 180 * spawnObjectEffect.RandomizedRotationMultiplier.z)
                        );

                        instance.transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + offset);
                    }
                }

            foreach (var playAudioEffect in SurfaceEffect.PlayAudioEffects)
            {
                if (!ObjectPools.ContainsKey(playAudioEffect.AudioSourcePrefab.gameObject))
                    ObjectPools.Add(playAudioEffect.AudioSourcePrefab.gameObject, new ObjectPool<GameObject>(() =>
                    {
                        var audioObject =
                            Instantiate(playAudioEffect.AudioSourcePrefab.gameObject, objectParent);
                        return audioObject;
                    }));

                var clip = playAudioEffect.AudioClips[Random.Range(0, playAudioEffect.AudioClips.Count)];
                if (clip != null)
                {
                    var instance = ObjectPools[playAudioEffect.AudioSourcePrefab.gameObject].Get();
                    instance.SetActive(true);
                    var audioSource = instance.GetComponent<AudioSource>();

                    audioSource.transform.position = HitPoint;
                    audioSource.PlayOneShot(clip,
                        SoundOffset * Random.Range(playAudioEffect.VolumeRange.x, playAudioEffect.VolumeRange.y));
                    StartCoroutine(DisableAudioSource(ObjectPools[playAudioEffect.AudioSourcePrefab.gameObject],
                        audioSource, clip.length));
                }
            }
        }

        private IEnumerator DisableAudioSource(ObjectPool<GameObject> Pool, AudioSource AudioSource, float Time)
        {
            yield return new WaitForSeconds(Time);

            AudioSource.gameObject.SetActive(false);
            Pool.Release(AudioSource.gameObject);
        }


        private class TextureAlpha
        {
            public float Alpha;
            public Texture Texture;
        }
    }
}