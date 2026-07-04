using UnityEngine;
using System.Collections;

public class WakeUpSequence : MonoBehaviour
{
    [Header("Sahne Baðlantýlarý")]
    public GameObject playerController;  // Asýl karakterin kendisi (Ayaklarý yere basýyor olmalý)
    public Camera playerCamera;          // Asýl karakterin ÝÇÝNDEKÝ kendi kamerasý
    public Camera wakeUpCamera;          // Geçici sinematik kamera

    [Header("Animasyon Ayarlarý")]
    public float animationDuration = 2.5f;

    [Header("Yatýþ Pozisyonu (Yastýk)")]
    public Transform bedPillowTransform; // Yastýðýn üzerindeki boþ obje (Kalkýþýn baþlayacaðý yer)

    void Start()
    {
        // 1. Asýl karakteri baþlangýçta gizle (zaten haritada doðru yerde duruyor)
        playerController.SetActive(false);
        wakeUpCamera.gameObject.SetActive(true);

        // 2. Geçici kamerayý tam yastýðýn üzerine koy
        wakeUpCamera.transform.position = bedPillowTransform.position;
        wakeUpCamera.transform.rotation = bedPillowTransform.rotation;

        StartCoroutine(WakeUpRoutine());
    }

    private IEnumerator WakeUpRoutine()
    {
        yield return new WaitForSeconds(1f);

        float elapsedTime = 0f;

        // Hedefimiz matematiksel bir sayý deðil, asýl kameranýn dünyadaki TAM KENDÝ KONUMU
        Vector3 targetPos = playerCamera.transform.position;
        Quaternion targetRot = playerCamera.transform.rotation;

        Vector3 startPos = wakeUpCamera.transform.position;
        Quaternion startRot = wakeUpCamera.transform.rotation;

        while (elapsedTime < animationDuration)
        {
            float t = elapsedTime / animationDuration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            // Geçici kamera yastýktan kalkýp, asýl kameranýn durmasý gereken yere doðru uçar
            wakeUpCamera.transform.position = Vector3.Lerp(startPos, targetPos, smoothT);
            wakeUpCamera.transform.rotation = Quaternion.Slerp(startRot, targetRot, smoothT);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Küsüratlarý temizle ve iki kamerayý tam olarak %100 üst üste bindir
        wakeUpCamera.transform.position = targetPos;
        wakeUpCamera.transform.rotation = targetRot;

        // --- KUSURSUZ GEÇÝÞ ---
        // Kameralar ayný pikselde olduðu için geçiþi anlamazsýn. Karakter ýþýnlanmaz, sadece görünür olur.
        playerController.SetActive(true);
        wakeUpCamera.gameObject.SetActive(false);
    }
}