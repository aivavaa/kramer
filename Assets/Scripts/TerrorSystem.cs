using UnityEngine;
using UnityEngine.UI;

public class TerrorSystem : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public Transform player;
    public Transform[] enemies;

    [Header("UI Elements")]
    public Slider terrorMeter;
    public GameObject gameOverScreen;

    [Header("Expressions")]
    public Image expressionUI;
    public Sprite[] expressionSprites;

    // İlaç ve Manik Mod Ayarları
    [Header("Manic Mode Settings")]
    public Sprite manicSprite;         // İlacı içince görünecek özel sprite
    public bool isManicMode = false;   // Karakter şu an avcı modunda mı?

    [Header("Terror Settings")]
    public float currentTerror = 0f;
    public float maxTerror = 100f;
    public float terrorRadius = 15f;

    public float baseIncreaseRate = 5f;
    public float decreaseRate = 3f;

    private bool isFainted = false;

    // --- YENİ EKLENEN KISIM ---
    void Start()
    {
        // Oyun başladığında barın görsel sınırını terör sınırına (100) eşitle
        if (terrorMeter != null)
        {
            terrorMeter.maxValue = maxTerror;
            terrorMeter.value = currentTerror;
        }
    }
    // --------------------------

    void Update()
    {
        if (isFainted) return;

        // Eğer manik moddaysak (ilaç içildiyse), terör artmasın ve değeri sıfırlansın
        if (isManicMode)
        {
            currentTerror = 0f;
        }
        else
        {
            // Normal Terör Hesaplaması (Sadece manik modda değilsek çalışır)
            CalculateTerror();
        }

        currentTerror = Mathf.Clamp(currentTerror, 0f, maxTerror);

        // --- KRİTİK DEĞİŞİKLİK ---
        // Eğer manik modda DEĞİLSEK (Phase 1) bu script slider'ı kontrol eder.
        // Eğer manik moddaysak (Phase 2) bu script barı salar ve kontrol GameManager'ın Kill Bar'ına geçer.
        if (terrorMeter != null && !isManicMode)
        {
            terrorMeter.value = currentTerror;
        }

        UpdateExpression();

        if (currentTerror >= maxTerror && !isManicMode)
        {
            TriggerFaintState();
        }
    }

    void CalculateTerror()
    {
        bool enemyIsNear = false;
        float closestDistance = Mathf.Infinity;

        foreach (Transform enemy in enemies)
        {
            if (enemy != null)
            {
                float distance = Vector3.Distance(player.position, enemy.position);
                if (distance < terrorRadius)
                {
                    enemyIsNear = true;
                    if (distance < closestDistance) closestDistance = distance;
                }
            }
        }

        if (enemyIsNear)
        {
            float distanceMultiplier = 1f - (closestDistance / terrorRadius);
            currentTerror += (baseIncreaseRate + (distanceMultiplier * 10f)) * Time.deltaTime;
        }
        else
        {
            currentTerror -= decreaseRate * Time.deltaTime;
        }
    }

    void UpdateExpression()
    {
        if (expressionUI == null) return;

        // Eğer ilaç içildiyse, sadece avcı sprite'ını göster ve fonksiyondan çık
        if (isManicMode)
        {
            if (manicSprite != null)
            {
                expressionUI.sprite = manicSprite;
            }
            return;
        }

        // Normal Terör Yüzleri (İlaç içilmediyse çalışır)
        if (expressionSprites == null || expressionSprites.Length == 0) return;

        float terrorPercent = currentTerror / maxTerror;
        int spriteIndex = Mathf.FloorToInt(terrorPercent * expressionSprites.Length);
        spriteIndex = Mathf.Clamp(spriteIndex, 0, expressionSprites.Length - 1);

        expressionUI.sprite = expressionSprites[spriteIndex];
    }

    // İlacı aldığında bu fonksiyonu çağıracaksın
    public void TakePill()
    {
        isManicMode = true;
        Debug.Log("İLAÇ ALINDI! ROLLER DEĞİŞTİ, AVCI MODU AKTİF!");

        // Sprite'ın anında güncellenmesi için çağırıyoruz
        UpdateExpression();
    }

    void TriggerFaintState()
    {
        isFainted = true;
        Debug.Log("100% TERROR REACHED: BAYILMA EKRANI TETIKLENDI!");

        if (gameOverScreen != null) gameOverScreen.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}