using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GunSwitcher : MonoBehaviour
{
    [SerializeField] private PlayerGunSelector GunSelector;
    
    [SerializeField] private GameObject activeGunSlot;
    [SerializeField] private GameObject nextGunSlot;
    [SerializeField] private GameObject previousGunSlot;

    private Image activeGunIcon;
    private Image nextGunIcon;
    private Image previousGunIcon;

    [SerializeField] private float shakeDuration = 0.2f;
    [SerializeField] private float shakeMagnitude = 1.5f;

    private Vector3 activeGunSlotOriginalPosition;
    private Vector3 nextGunSlotOriginalPosition;
    private Vector3 previousGunSlotOriginalPosition;

    private void Awake()
    {
        activeGunIcon = activeGunSlot.transform.GetChild(0).GetComponent<Image>();
        nextGunIcon = nextGunSlot.transform.GetChild(0).GetComponent<Image>();
        previousGunIcon = previousGunSlot.transform.GetChild(0).GetComponent<Image>();
        
        activeGunSlotOriginalPosition = activeGunSlot.transform.localPosition;
        nextGunSlotOriginalPosition = nextGunSlot.transform.localPosition;
        previousGunSlotOriginalPosition = previousGunSlot.transform.localPosition;
    }

    private void OnEnable()
    {
        PlayerGunSelector.OnGunChanged += HandleGunChanged;
    }

    private void OnDisable()
    {
        PlayerGunSelector.OnGunChanged -= HandleGunChanged;
    }
    
    private void HandleGunChanged()
    {
        UpdateSwitcherIcons();
        StartCoroutine(ShakeIcon(activeGunSlot, activeGunSlotOriginalPosition));
        StartCoroutine(ShakeIcon(nextGunSlot, nextGunSlotOriginalPosition));
        StartCoroutine(ShakeIcon(previousGunSlot, previousGunSlotOriginalPosition));
    }

    private void UpdateSwitcherIcons()
    {
        activeGunIcon.sprite = GunSelector.ActiveGun.GunIcon;
        nextGunIcon.sprite = GunSelector.GetNextGun().GunIcon;
        previousGunIcon.sprite = GunSelector.GetPreviousGun().GunIcon;
    }
    
    private IEnumerator ShakeIcon(GameObject icon, Vector3 originalPosition)
    {
        float duration = shakeDuration;
        float magnitude = shakeMagnitude; 

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float x = UnityEngine.Random.Range(-1f, 1f) * magnitude;
            float y = UnityEngine.Random.Range(-1f, 1f) * magnitude;

            icon.transform.localPosition = new Vector3(originalPosition.x + x, originalPosition.y + y, originalPosition.z);

            elapsed += Time.deltaTime;

            yield return null;
        }
        
        icon.transform.localPosition = originalPosition;
    }
}
