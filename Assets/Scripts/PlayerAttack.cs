using UnityEngine;
using System.Collections;

public class PlayerAttack : MonoBehaviour
{
    [Header("Baðlantýlar")]
    public Camera playerCamera;
    public Transform leftHandPivot;
    public Transform rightHandPivot;

    [Header("Saldýrý Ayarlarý (Raycast)")]
    public float attackRange = 3.5f;

    [Header("Aþama 1: Hazýrlýk (Kollarý Açma)")]
    public float stage1Duration = 0.2f;
    // Sol elin sola ve geriye açýlmasý (Örn: X: -0.2, Y: 0, Z: -0.1)
    public Vector3 openPosOffset = new Vector3(-0.15f, 0f, -0.1f);
    // Sol elin dýþa doðru hafif dönmesi (Y ekseninde -20 derece)
    public Vector3 openRotOffset = new Vector3(0f, -20f, 0f);

    [Header("Aþama 2: Saldýrý (Kavrama / Sarýlma)")]
    public float stage2Duration = 0.3f;
    // Sol elin merkeze (saða) ve ileri atýlmasý (Örn: X: 0.3, Z: 0.5)
    public Vector3 grabPosOffset = new Vector3(0.25f, 0f, 0.4f);
    // Sol elin içe (saða) sertçe dönerek kavramasý (Y ekseninde 45 derece)
    public Vector3 grabRotOffset = new Vector3(0f, 40f, 0f);

    [Header("Aþama 3: Geri Çekilme")]
    public float returnDuration = 0.2f;

    private bool isAttacking = false;
    private Vector3 leftOriginalPos, rightOriginalPos;
    private Quaternion leftOriginalRot, rightOriginalRot;

    void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main;

        if (leftHandPivot != null)
        {
            leftOriginalPos = leftHandPivot.localPosition;
            leftOriginalRot = leftHandPivot.localRotation;
        }
        if (rightHandPivot != null)
        {
            rightOriginalPos = rightHandPivot.localPosition;
            rightOriginalRot = rightHandPivot.localRotation;
        }
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isHunterMode)
        {
            if (Input.GetMouseButtonDown(0) && !isAttacking)
            {
                StartCoroutine(GrabAttackRoutine());
                PerformRaycastAttack();
            }
        }
    }

    private void PerformRaycastAttack()
    {
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, attackRange))
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                EnemyAI enemy = hit.collider.GetComponent<EnemyAI>();
                if (enemy != null) enemy.TakeDamage();
            }
        }
    }

    private IEnumerator GrabAttackRoutine()
    {
        isAttacking = true;

        // --- Sað el için simetrik (aynalanmýþ) hedefleri hesapla ---
        // Pozisyon: X eksenini ters çevir
        Vector3 rightOpenPos = new Vector3(-openPosOffset.x, openPosOffset.y, openPosOffset.z);
        Vector3 rightGrabPos = new Vector3(-grabPosOffset.x, grabPosOffset.y, grabPosOffset.z);
        // Rotasyon: Y ve Z eksenini ters çevir
        Vector3 rightOpenRot = new Vector3(openRotOffset.x, -openRotOffset.y, -openRotOffset.z);
        Vector3 rightGrabRot = new Vector3(grabRotOffset.x, -grabRotOffset.y, -grabRotOffset.z);

        // --- AÞAMA 1: KOLLARI AÇMA ---
        float t = 0f;
        while (t < stage1Duration)
        {
            t += Time.deltaTime;
            float percent = t / stage1Duration;

            // Pozisyon Lerp
            leftHandPivot.localPosition = Vector3.Lerp(leftOriginalPos, leftOriginalPos + openPosOffset, Mathf.SmoothStep(0, 1, percent));
            rightHandPivot.localPosition = Vector3.Lerp(rightOriginalPos, rightOriginalPos + rightOpenPos, Mathf.SmoothStep(0, 1, percent));

            // Rotasyon Slerp (Yumuþak dönüþ)
            leftHandPivot.localRotation = Quaternion.Slerp(leftOriginalRot, leftOriginalRot * Quaternion.Euler(openRotOffset), Mathf.SmoothStep(0, 1, percent));
            rightHandPivot.localRotation = Quaternion.Slerp(rightOriginalRot, rightOriginalRot * Quaternion.Euler(rightOpenRot), Mathf.SmoothStep(0, 1, percent));

            yield return null;
        }

        // --- AÞAMA 2: KAVRAMA / ÝÇE KAPANMA ---
        t = 0f;
        Vector3 stage1LeftPos = leftHandPivot.localPosition;
        Vector3 stage1RightPos = rightHandPivot.localPosition;
        Quaternion stage1LeftRot = leftHandPivot.localRotation;
        Quaternion stage1RightRot = rightHandPivot.localRotation;

        while (t < stage2Duration)
        {
            t += Time.deltaTime;
            float percent = t / stage2Duration;

            leftHandPivot.localPosition = Vector3.Lerp(stage1LeftPos, leftOriginalPos + grabPosOffset, Mathf.SmoothStep(0, 1, percent));
            rightHandPivot.localPosition = Vector3.Lerp(stage1RightPos, rightOriginalPos + rightGrabPos, Mathf.SmoothStep(0, 1, percent));

            leftHandPivot.localRotation = Quaternion.Slerp(stage1LeftRot, leftOriginalRot * Quaternion.Euler(grabRotOffset), Mathf.SmoothStep(0, 1, percent));
            rightHandPivot.localRotation = Quaternion.Slerp(stage1RightRot, rightOriginalRot * Quaternion.Euler(rightGrabRot), Mathf.SmoothStep(0, 1, percent));

            yield return null;
        }

        // --- AÞAMA 3: GERÝ ÇEKÝLME ---
        t = 0f;
        Vector3 finalLeftPos = leftHandPivot.localPosition;
        Vector3 finalRightPos = rightHandPivot.localPosition;
        Quaternion finalLeftRot = leftHandPivot.localRotation;
        Quaternion finalRightRot = rightHandPivot.localRotation;

        while (t < returnDuration)
        {
            t += Time.deltaTime;
            float percent = t / returnDuration;

            leftHandPivot.localPosition = Vector3.Lerp(finalLeftPos, leftOriginalPos, percent);
            rightHandPivot.localPosition = Vector3.Lerp(finalRightPos, rightOriginalPos, percent);

            leftHandPivot.localRotation = Quaternion.Slerp(finalLeftRot, leftOriginalRot, percent);
            rightHandPivot.localRotation = Quaternion.Slerp(finalRightRot, rightOriginalRot, percent);

            yield return null;
        }

        // Kusursuz sýfýrlama
        leftHandPivot.localPosition = leftOriginalPos;
        rightHandPivot.localPosition = rightOriginalPos;
        leftHandPivot.localRotation = leftOriginalRot;
        rightHandPivot.localRotation = rightOriginalRot;

        isAttacking = false;
    }
}