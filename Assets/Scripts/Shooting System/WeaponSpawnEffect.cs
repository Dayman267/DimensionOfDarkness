﻿using UnityEngine;

public class WeaponSpawnEffect : MonoBehaviour
{
    public float spawnEffectTime = 2;
    public float pause = 1;
    public AnimationCurve fadeIn;
    private readonly float timer = 0;
    private Renderer _renderer;

    private ParticleSystem ps;

    private int shaderProperty;

    private void Start()
    {
        shaderProperty = Shader.PropertyToID("_cutoff");
        _renderer = GetComponent<Renderer>();
        ps = GetComponentInChildren<ParticleSystem>();

        var main = ps.main;
        main.duration = spawnEffectTime;

        ps.Play();
    }

    private void Update()
    {
        _renderer.material.SetFloat(shaderProperty, fadeIn.Evaluate(Mathf.InverseLerp(0, spawnEffectTime, timer)));
    }
}