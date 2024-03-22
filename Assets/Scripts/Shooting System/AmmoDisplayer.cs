using System;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(TextMeshProUGUI))]
public class AmmoDisplayer : MonoBehaviour
{
    [SerializeField] private PlayerGunSelector GunSelector;
    private TextMeshProUGUI AmmoText;

    private void Awake()
    {
        AmmoText = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        AmmoText.SetText(
            $"{GunSelector.ActiveGun.AmmoConfig.CurrentClipAmmo} / " +
            $"{GunSelector.ActiveGun.AmmoConfig.ClipSize}");
    }
}