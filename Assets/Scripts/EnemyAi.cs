using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    // 3. Durumu ekledik: Saldýrý
    public enum AIState { Wandering, Chasing, Attacking }
    public AIState currentState = AIState.Wandering;

    [Header("Hedef")]
    public Transform playerTarget;
    public float eyeHeight = 1f;

    [Header("Görüþ & Algýlama (Wander Modu)")]
    public float viewRadius = 15f;       // Görme mesafesi
    [Range(0, 360)]
    public float viewAngle = 90f;        // Görüþ açýsý
    public float closeAwarenessRadius = 3f; // Bu mesafeye girersen arkasý dönük olsa bile seni HÝSSEDER

    [Header("Kovalama Ayarlarý (Chase Modu)")]
    public float loseRadius = 25f;       // Peþini býrakmasý için arayý ne kadar açman gerektiði (viewRadius'tan büyük olmalý)

    [Header("Saldýrý Ayarlarý (Attack Modu)")]
    public float attackRadius = 2f;      // Sana vurmak için duracaðý mesafe

    [Header("Gezinme Ayarlarý")]
    public float wanderRadius = 20f;
    private Vector3 patrolCenter;

    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        patrolCenter = transform.position;

        if (playerTarget == null)
        {
            playerTarget = GameObject.FindGameObjectWithTag("Player").transform;
        }

        SetRandomWanderDestination();
    }

    void Update()
    {
        if (playerTarget == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        // --- 1. STATE GEÇÝÞ KONTROLLERÝ (Karar Verme Merkezi) ---
        switch (currentState)
        {
            case AIState.Wandering:
                // Seni görürse YADA çok dibine girip ses çýkarýrsan (açýya bakmaksýzýn) kovalamaya baþlar
                if (distanceToPlayer <= closeAwarenessRadius || CanSeePlayer(distanceToPlayer))
                {
                    currentState = AIState.Chasing;
                }
                break;

            case AIState.Chasing:
                // Kovalama modundayken artýk açýya (FOV) bakmýyoruz. Sadece arayý ne kadar açtýðýna bakýyoruz.
                if (distanceToPlayer > loseRadius)
                {
                    currentState = AIState.Wandering;
                    SetRandomWanderDestination();
                }
                // Dibine girdiyse saldýrý moduna geç
                else if (distanceToPlayer <= attackRadius)
                {
                    currentState = AIState.Attacking;
                }
                break;

            case AIState.Attacking:
                // Sen ondan uzaklaþýrsan tekrar kovalamaya baþlar
                if (distanceToPlayer > attackRadius)
                {
                    currentState = AIState.Chasing;
                    agent.isStopped = false; // Hareketi geri aç
                }
                break;
        }

        // --- 2. STATE EYLEMLERÝ (Fiziksel Hareketler) ---
        switch (currentState)
        {
            case AIState.Wandering:
                PerformWandering();
                break;
            case AIState.Chasing:
                PerformChasing();
                break;
            case AIState.Attacking:
                PerformAttacking();
                break;
        }
    }

    // --- GÖRÜÞ SÝSTEMÝ ---
    private bool CanSeePlayer(float distance)
    {
        if (distance > viewRadius) return false;

        Vector3 dirToPlayer = (playerTarget.position - transform.position).normalized;
        float angleToPlayer = Vector3.Angle(transform.forward, dirToPlayer);

        if (angleToPlayer < viewAngle / 2f)
        {
            return CheckLineOfSight();
        }
        return false;
    }

    private bool CheckLineOfSight()
    {
        RaycastHit hit;
        Vector3 startPos = transform.position + Vector3.up * eyeHeight;
        Vector3 targetPos = playerTarget.position + Vector3.up * eyeHeight;
        Vector3 rayDir = (targetPos - startPos).normalized;

        if (Physics.Raycast(startPos, rayDir, out hit, viewRadius))
        {
            if (hit.transform == playerTarget) return true;
        }
        return false;
    }

    // --- DAVRANIÞLAR ---
    private void PerformChasing()
    {
        agent.isStopped = false;
        agent.SetDestination(playerTarget.position);
    }

    private void PerformWandering()
    {
        agent.isStopped = false;
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            SetRandomWanderDestination();
        }
    }

    private void PerformAttacking()
    {
        // 1. Ajaný durdur (Seni ittirip üstüne çýkmasýný engeller)
        agent.isStopped = true;

        // 2. Daima yüzünü sana dönmesini saðla
        Vector3 lookDirection = (playerTarget.position - transform.position).normalized;
        lookDirection.y = 0; // Düþmanýn yukarý doðru yamulmasýný engeller
        Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);

        // ÝLERÝDE BURAYA ANÝMASYON KODUNU YAZACAKSIN
        // Örn: animator.SetTrigger("Attack");
    }

    private void SetRandomWanderDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += patrolCenter;

        NavMeshHit navHit;
        if (NavMesh.SamplePosition(randomDirection, out navHit, wanderRadius, -1))
        {
            agent.SetDestination(navHit.position);
        }
    }

    // Görselleþtirmeler (Gizmos)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRadius); // Görüþ Alaný

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, closeAwarenessRadius); // Yakýn Hissetme Alaný

        Vector3 fovLine1 = Quaternion.AngleAxis(viewAngle / 2, Vector3.up) * transform.forward;
        Vector3 fovLine2 = Quaternion.AngleAxis(-viewAngle / 2, Vector3.up) * transform.forward;

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position + Vector3.up * eyeHeight, fovLine1 * viewRadius);
        Gizmos.DrawRay(transform.position + Vector3.up * eyeHeight, fovLine2 * viewRadius);
    }
}