using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public bool isHunterMode = false;

    [Header("Oyuncu ve Kamera Baðlantýlarý")]
    public FirstPersonMovement playerMovement;
    public Camera playerCamera;

    [Header("Görsel Ayarlar (FOV)")]
    public float hunterFOV = 100f; // Limitlere çarpmamasý için 100-110 arasý idealdir
    public float fovTransitionSpeed = 5f;

    [Header("Hedef Göstergeleri (Phase 2)")]
    public GameObject bedXRaySilhouette; // Duvar arkasýndan parlayacak yatak kopyasý

    [Header("Phase 1 Ekipmanlarý")]
    public GameObject flashlightObj; // Fenerin kendisi (Çakýþmada silinmiþti, geri eklendi)

    void Awake()
    {
        if (Instance == null) Instance = this;

        // Kod eðer arayüzden atanmamýþsa objeleri otomatik bulsun
        if (playerMovement == null)
            playerMovement = Object.FindFirstObjectByType<FirstPersonMovement>();

        if (playerCamera == null)
            playerCamera = Camera.main;

        playerMovement.canRun = true; // Oyuncunun koþabilmesini saðla   
    }

    void Start()
    {
        // 1. Ýþletim sisteminin fare imlecini ekranýn ortasýna kilitler
        Cursor.lockState = CursorLockMode.Locked;
        // 2. Fare imlecini görünmez yapar
        Cursor.visible = false;

        // Oyun baþladýðýnda yatak silüeti yanlýþlýkla açýk unutulmuþsa bile ZORLA KAPAT.
        if (bedXRaySilhouette != null)
        {
            bedXRaySilhouette.SetActive(false);
        }
    }

    // Hap alýndýðýnda PillTrigger tarafýndan çaðrýlýr
    public void ActivateOneMoreTime()
    {
        isHunterMode = true;
        Debug.Log("ÝLAÇ ALINDI! PHASE 2 (HUNTER MODE) BAÞLADI!");

        // 1. DÜÞMANLARI KAÇIR
        EnemyAI[] allEnemies = Object.FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        foreach (EnemyAI enemy in allEnemies)
        {
            enemy.StartFleeing();
        }

        // 2. KARAKTERÝ "MANIC" MODA SOK (Hýzlandýr)
        if (playerMovement != null)
        {
            playerMovement.isManic = true;
        }

        // 3. FOV'U YUMUÞAKÇA ARTIR
        if (playerCamera != null)
        {
            StartCoroutine(TransitionFOV());
        }

        // 4. YATAÐIN SÝLÜETÝNÝ (PHASE 2 HEDEFÝNÝ) AKTÝF ET
        if (bedXRaySilhouette != null)
        {
            bedXRaySilhouette.SetActive(true);
        }

        // 5. PHASE 1 BÝTTÝ: FENERÝ KAPAT
        if (flashlightObj != null)
        {
            flashlightObj.SetActive(false);
        }
    }

    private IEnumerator TransitionFOV()
    {
        // Ekranýn titrememesi için yumuþak bir Lerp geçiþi
        while (Mathf.Abs(playerCamera.fieldOfView - hunterFOV) > 0.1f)
        {
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, hunterFOV, Time.deltaTime * fovTransitionSpeed);
            yield return null;
        }
        playerCamera.fieldOfView = hunterFOV;
    }
}