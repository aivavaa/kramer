using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public bool isHunterMode = false;

    [Header("Sistem Baðlantýlarý")]
    public TerrorSystem terrorSystem;

    [Header("Oyuncu ve Kamera Baðlantýlarý")]
    public FirstPersonMovement playerMovement;
    public Camera playerCamera;

    [Header("Görsel Ayarlar (FOV & Exposure)")]
    public float hunterFOV = 100f;
    public float fovTransitionSpeed = 5f;

    public Volume globalVolume;
    public float phase1Exposure = 1f;
    public float phase2Exposure = -1f;
    private Exposure exposureOverride;

    [Header("Hedef Göstergeleri (Phase 2)")]
    public GameObject bedXRaySilhouette;

    [Header("Ekipmanlar")]
    public GameObject flashlightObj;
    public GameObject attackHandsObj;

    [Header("Phase 2 - Kill Tracker")]
    public int totalEnemies;
    public int killedEnemies = 0;
    public bool isPhaseClear = false;

    // --- YENÝ EKLENEN KISIM: RENK DEÐÝÞÝMÝ ---
    public Image sliderFillImage; // Slider'ýn içini boyayan obje
    public Color bloodRedColor = new Color(0.7f, 0f, 0f, 1f); // Koyu Kan Kýrmýzýsý (Inspector'dan da deðiþtirebilirsin)
    // -----------------------------------------

    void Awake()
    {
        if (Instance == null) Instance = this;

        if (playerMovement == null)
            playerMovement = Object.FindFirstObjectByType<FirstPersonMovement>();

        if (playerCamera == null)
            playerCamera = Camera.main;

        if (terrorSystem == null)
            terrorSystem = Object.FindFirstObjectByType<TerrorSystem>();

        playerMovement.canRun = true;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (bedXRaySilhouette != null) bedXRaySilhouette.SetActive(false);

        if (globalVolume != null)
        {
            globalVolume.profile.TryGet(out exposureOverride);
        }
    }

    public void ActivateOneMoreTime()
    {
        isHunterMode = true;
        Debug.Log("ÝLAÇ ALINDI! PHASE 2 (HUNTER MODE) BAÞLADI!");

        if (terrorSystem != null)
        {
            terrorSystem.TakePill();

            totalEnemies = 0;
            foreach (Transform enemyTransform in terrorSystem.enemies)
            {
                if (enemyTransform != null)
                {
                    totalEnemies++;

                    EnemyAI enemyAI = enemyTransform.GetComponent<EnemyAI>();
                    if (enemyAI != null) enemyAI.StartFleeing();
                }
            }

            if (terrorSystem.terrorMeter != null)
            {
                terrorSystem.terrorMeter.maxValue = totalEnemies;
                terrorSystem.terrorMeter.value = 0;
            }
        }
        else
        {
            Debug.LogError("TerrorSystem bulunamadý! Oyun döngüsü kilitlenebilir.");
        }

        // YENÝ: Ýlacý aldýðýmýzda Slider'ýn rengini Kan Kýrmýzýsý yap
        if (sliderFillImage != null)
        {
            sliderFillImage.color = bloodRedColor;
        }

        if (playerMovement != null) playerMovement.isManic = true;

        if (playerCamera != null) StartCoroutine(TransitionFOV());
        if (exposureOverride != null) StartCoroutine(TransitionExposure());

        if (bedXRaySilhouette != null) bedXRaySilhouette.SetActive(true);
        if (flashlightObj != null) flashlightObj.SetActive(false);
        if (attackHandsObj != null) attackHandsObj.SetActive(true);
    }

    public void EnemyDied()
    {
        if (!isHunterMode) return;

        killedEnemies++;

        if (terrorSystem != null && terrorSystem.terrorMeter != null)
        {
            terrorSystem.terrorMeter.value = killedEnemies;
        }

        if (killedEnemies >= totalEnemies)
        {
            isPhaseClear = true;
            Debug.Log("Bütün canavarlar temizlendi! Artýk yataða dönüp uyuyabilirsin.");
        }
    }

    private IEnumerator TransitionFOV()
    {
        while (Mathf.Abs(playerCamera.fieldOfView - hunterFOV) > 0.1f)
        {
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, hunterFOV, Time.deltaTime * fovTransitionSpeed);
            yield return null;
        }
        playerCamera.fieldOfView = hunterFOV;
    }

    private IEnumerator TransitionExposure()
    {
        while (Mathf.Abs(exposureOverride.fixedExposure.value - phase2Exposure) > 0.05f)
        {
            exposureOverride.fixedExposure.value = Mathf.Lerp(exposureOverride.fixedExposure.value, phase2Exposure, Time.deltaTime * fovTransitionSpeed);
            yield return null;
        }
        exposureOverride.fixedExposure.value = phase2Exposure;
    }
}