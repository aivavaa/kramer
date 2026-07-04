using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public bool isHunterMode = false;

    [Header("Oyuncu ve Kamera Ba�lant�lar�")]
    public FirstPersonMovement playerMovement;
    public Camera playerCamera;

    [Header("G�rsel Ayarlar (FOV)")]
    public float hunterFOV = 100f; // Limitlere �arpmamas� i�in 100-110 aras� idealdir
    public float fovTransitionSpeed = 5f;

    [Header("Hedef G�stergeleri (Phase 2)")]
    public GameObject bedXRaySilhouette; // Duvar arkas�ndan parlayacak yatak kopyas�

    void Awake()
    {
        if (Instance == null) Instance = this;

        // Kod e�er aray�zden atanmam��sa objeleri otomatik bulsun
        if (playerMovement == null)
            playerMovement = Object.FindFirstObjectByType<FirstPersonMovement>();

        if (playerCamera == null)
            playerCamera = Camera.main;

        playerMovement.canRun = true; // Oyuncunun ko�abilmesini sa�la   
    }

    // Hap al�nd���nda PillTrigger taraf�ndan �a�r�l�r
    public void ActivateOneMoreTime()
    {
        isHunterMode = true;
        Debug.Log("�LA� ALINDI! PHASE 2 (HUNTER MODE) BA�LADI!");

        // 1. D��MANLARI KA�IR
        EnemyAI[] allEnemies = Object.FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        foreach (EnemyAI enemy in allEnemies)
        {
            enemy.StartFleeing();
        }

        // 2. KARAKTER� "MANIC" MODA SOK (H�zland�r)
        if (playerMovement != null)
        {
            playerMovement.isManic = true;
        }

        // 3. FOV'U YUMU�AK�A ARTIR
        if (playerCamera != null)
        {
            StartCoroutine(TransitionFOV());
        }

        // 4. YATA�IN S�L�ET�N� (PHASE 2 HEDEF�N�) AKT�F ET
        if (bedXRaySilhouette != null)
        {
            bedXRaySilhouette.SetActive(true);
        }
    }

    private IEnumerator TransitionFOV()
    {
        // Ekran�n titrememesi i�in yumu�ak bir Lerp ge�i�i
        while (Mathf.Abs(playerCamera.fieldOfView - hunterFOV) > 0.1f)
        {
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, hunterFOV, Time.deltaTime * fovTransitionSpeed);
            yield return null;
        }
        playerCamera.fieldOfView = hunterFOV;
    }
}