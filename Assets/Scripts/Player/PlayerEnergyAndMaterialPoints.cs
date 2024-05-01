using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerEnergyAndMaterialPoints : MonoBehaviour
{
    private PointsManager manager;
    private int solidMaterial;
    public TextMeshProUGUI solidMaterialText;
    
    private void Start()
    {
        manager = GameObject.FindWithTag("PointsManager").GetComponent<PointsManager>();
        solidMaterialText = GameObject.FindGameObjectWithTag("SolidMaterial").GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        manager.darkEnergyText.text = $"{manager.darkEnergyPoints}/{manager.darkEnergyPointsToAccess}";
        solidMaterialText.text = $"{solidMaterial}";
    }
    
    public void AddSolidMaterial(int points)
    {
        solidMaterial += points;
    }
}
