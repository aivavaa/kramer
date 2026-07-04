using UnityEngine;
using System.Collections;

public class PillAlarm : MonoBehaviour
{
    [Header("Görsel Ayarlar")]
    public Renderer meshRenderer;
    public Color alarmColor = Color.red;
    public float maxIntensity = 5f;
    public float flashDuration = 1f;
    public float waitTime = 6f;

    private Material xrayMaterial;

    void Start()
    {
        if (meshRenderer == null) meshRenderer = GetComponent<Renderer>();

        // ÖNEMLÝ: Modelcilerin kaplamasýný bozmamak için 2. materyali (X-Ray) alýyoruz
        if (meshRenderer.materials.Length > 1)
        {
            xrayMaterial = meshRenderer.materials[1];
        }
        else
        {
            xrayMaterial = meshRenderer.material; // Yanlýþlýkla tek materyal konduysa çökmesin
        }

        // Oyun baþlar baþlamaz GÖRÜNMEZ yap (Additive modda siyah = görünmezdir)
        if (xrayMaterial != null) xrayMaterial.SetColor("_BaseColor", Color.black);

        StartCoroutine(AlarmLoop());
    }

    private IEnumerator AlarmLoop()
    {
        while (true)
        {
            // 1. AÞAMA: Aniden Parlama
            float t = 0;
            while (t < flashDuration / 2)
            {
                t += Time.deltaTime;
                float intensity = Mathf.Lerp(0, maxIntensity, t / (flashDuration / 2));
                xrayMaterial.SetColor("_BaseColor", alarmColor * intensity);
                yield return null;
            }

            // 2. AÞAMA: Yavaþça Sönme
            t = 0;
            while (t < flashDuration / 2)
            {
                t += Time.deltaTime;
                float intensity = Mathf.Lerp(maxIntensity, 0, t / (flashDuration / 2));
                xrayMaterial.SetColor("_BaseColor", alarmColor * intensity);
                yield return null;
            }

            // Tamamen kapat (Siyah yap. Additive olduðu için DUVAR ARKASINDAN SÝLÝNÝR)
            xrayMaterial.SetColor("_BaseColor", Color.black);

            // 3. AÞAMA: 6-7 saniye görünmez bekle
            yield return new WaitForSeconds(waitTime);
        }
    }
}