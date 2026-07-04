using UnityEngine;

public class WeaponSway : MonoBehaviour
{
    [Header("Fare Gecikmesi (Mouse Sway)")]
    public float swayAmount = 0.03f;
    public float maxSwayAmount = 0.08f;
    public float smoothAmount = 6f;

    [Header("Yürüme Yalpalamasý (Doom Bobbing)")]
    public FirstPersonMovement playerMovement;
    public Rigidbody playerRigidbody;
    public float bobSpeed = 10f;          // Sekme hýzý
    public float bobAmountX = 0.05f;      // Saða sola yatma miktarý
    public float bobAmountY = 0.05f;      // Yukarý aþaðý sekme miktarý

    private Vector3 initialPosition;
    private float timer;

    void Start()
    {
        initialPosition = transform.localPosition;

        // Sen Inspector'da uðraþma diye karakterin ana hareket kodlarýný otomatik bulur
        if (playerMovement == null) playerMovement = GetComponentInParent<FirstPersonMovement>();
        if (playerRigidbody == null) playerRigidbody = GetComponentInParent<Rigidbody>();
    }

    void Update()
    {
        // --- 1. FARE GECÝKMESÝ ---
        float moveX = -Input.GetAxis("Mouse X") * swayAmount;
        float moveY = -Input.GetAxis("Mouse Y") * swayAmount;

        moveX = Mathf.Clamp(moveX, -maxSwayAmount, maxSwayAmount);
        moveY = Mathf.Clamp(moveY, -maxSwayAmount, maxSwayAmount);

        // --- 2. YÜRÜME YALPALAMASI ---
        float bobX = 0f;
        float bobY = 0f;

        // Eðer karakter yerdeyse ve hareket ediyorsa bobbing yap
        if (playerMovement != null && playerRigidbody != null && playerMovement.IsGrounded && playerRigidbody.linearVelocity.magnitude > 0.1f)
        {
            // Koþarken fenerin daha hýzlý sekmesi için hýzý artýr
            float currentSpeed = playerMovement.IsRunning ? bobSpeed * 1.5f : bobSpeed;
            timer += Time.deltaTime * currentSpeed;

            // Doom tarzý sekiz (sonsuzluk) çizme formülü
            bobX = Mathf.Cos(timer) * bobAmountX;
            bobY = Mathf.Sin(timer * 2) * bobAmountY; // *2 olmasý her adýmda fenerin aþaðý vurmasýný saðlar
        }
        else
        {
            // Durduðumuzda fenerin yalpalamasýný sýfýrla ki merkeze dönsün
            timer = 0f;
        }

        // --- 3. HAREKETLERÝ BÝRLEÞTÝR ---
        // Farenin gecikmesi ile adýmýn yalpalamasýný topluyoruz
        Vector3 finalPosition = new Vector3(moveX + bobX, moveY + bobY, 0);

        // Feneri eski pürüzsüzlükte (Lerp) yeni yerine doðru kaydýr
        transform.localPosition = Vector3.Lerp(transform.localPosition, finalPosition + initialPosition, Time.deltaTime * smoothAmount);
    }
}