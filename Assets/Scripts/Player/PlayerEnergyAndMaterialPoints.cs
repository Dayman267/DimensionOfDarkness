using System;
using TMPro;
using UnityEngine;

public class PlayerEnergyAndMaterialPoints : MonoBehaviour
{
    public TextMeshProUGUI solidMaterialText;
    private PointsManager manager;
    private int solidMaterial;

    private void Start()
    {
        manager = GameObject.FindWithTag("PointsManager").GetComponent<PointsManager>();
        solidMaterialText = GameObject.FindGameObjectWithTag("SolidMaterial").GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        manager.darkEnergyText.text = $"Score: {manager.darkEnergyPoints}";
        solidMaterialText.text = $"{solidMaterial}";
    }

    public void AddSolidMaterial(int points)
    {
        solidMaterial += points;
    }
}