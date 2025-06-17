using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class enemyAI : MonoBehaviour
{
    // -------------------- PUBLIC FIELDS --------------------
    [Header("NavMesh Agent")]
    public NavMeshAgent ai;

    [Header("Audio")]
    public AudioSource backgroundMusic;

    [Header("Patrol-Zielpunkte")]
    public List<Transform> destinations;

    [Header("Animationen")]
    public Animator aiAnim;
    public float walkSpeed = 2f;
    public float chaseSpeed = 5f;

    [Header("Idle-Timing")]
    public float minIdleTime = 1f;
    public float maxIdleTime = 3f;

    [Header("Spieler & Jagd")]
    public Transform player;
    public float catchDistance = 1.5f;
    public float chaseAbortDistance = 50f;     // Beginnt Puffer, wenn Spieler weiter weg ist
    public float lostSightThreshold = 1f;      // Sekunden, die Spieler verloren sein darf, bevor Chase abbricht
    public float fieldOfViewAngle = 110f;      // Sichtwinkel
    public float sightRayLength = 15f;         // Raycast-Länge
    public Vector3 rayCastOffset;              // Raycast-Offset

    [Header("Forced Repath")]
    public float maxAllowedDistance = 100f;    // Wechsel zu ForcedRepath, wenn weiter entfernt
    public float forcedRepathCooldown = 2f;    // Cooldown in Sekunden

    [Header("Tod & Szenenwechsel")]
    public string deathScene;
    public float deathDelay = 2f;

    [Header("UI")]
    public GameObject hotbarUI;

    // -------------------- PRIVATE FIELDS --------------------
    private enum State { Idle, Patrol, ForcedRepath, Chase }
    private State currentState;

    private int lastDestIndex = -1;
    private Transform currentDest;
    private bool isIdleRoutineRunning = false;
    private float forcedRepathTimer = 0f;
    private float aiDistance = 0f;
    private float lostSightTimer = 0f;

    // Stuck-Check
    private float stuckCheckTimer = 0f;
    private Vector3 lastPosition;
    public float stuckCheckInterval = 1f;  // alle 1 Sekunde prüfen
    public float stuckThreshold = 0.1f;    // minimale Bewegung innerhalb Interval

    // **************** P U B L I C   P R O P E R T I E S ****************
    public bool IsChasing => currentState == State.Chase;


    // -------------------- UNITY CALLBACKS --------------------

    void OnEnable()
    {
        // 1) State initialisieren
        currentState = State.Patrol;
        forcedRepathTimer = forcedRepathCooldown;
        lostSightTimer = 0f;
        isIdleRoutineRunning = false;

        // 2) Anfangsabstand berechnen (Debug)
        if (player != null)
        {
            aiDistance = Vector3.Distance(player.position, transform.position);
            Debug.Log($"[enemyAI|OnEnable] State={currentState}, Start-Distance to player: {aiDistance:F2} m, DestCount={destinations?.Count}");
        }
        else
        {
            Debug.LogError("[enemyAI|OnEnable] Player-Transform ist null!");
        }

        if (ai == null)
        {
            Debug.LogError("[enemyAI|OnEnable] NavMeshAgent (ai) ist null!");
        }

        // 3) Erstes Patrol-Ziel wählen
        if (destinations != null && destinations.Count > 0)
        {
            int r = Random.Range(0, destinations.Count);
            currentDest = destinations[r];
            lastDestIndex = r;
            if (ai != null)
            {
                ai.destination = currentDest.position;
                ai.speed = walkSpeed;
                ai.isStopped = false;
            }
            if (aiAnim != null)
            {
                aiAnim.SetTrigger("walk");
            }
            Debug.Log($"[enemyAI|OnEnable] Patrol start: ZielIndex={r}, ZielPos={currentDest.position}");
        }
        else
        {
            Debug.LogWarning("[enemyAI|OnEnable] Keine Ziele (destinations) zugewiesen!");
        }

        lastPosition = transform.position;
        stuckCheckTimer = 0f;
    }

    void Update()
    {
        if (player == null || ai == null)
            return;

        // 1) Aktuellen Abstand zum Spieler berechnen
        aiDistance = Vector3.Distance(player.position, transform.position);
        Debug.Log($"[enemyAI|Update] State={currentState}, Distance to player: {aiDistance:F2}");

        // 2) Wenn im Chase-State, verwende gepufferte Chase-Logik
        if (currentState == State.Chase)
        {
            RunChaseLogic();
            return;
        }

        // 3) Forced Repath prüfen (wenn nicht in Chase/ForcedRepath)
        forcedRepathTimer += Time.deltaTime;
        if (currentState != State.Chase
            && currentState != State.ForcedRepath
            && aiDistance > maxAllowedDistance
            && forcedRepathTimer >= forcedRepathCooldown)
        {
            StartForcedRepath();
            return;
        }

        // 4) Sichtfeld-Check zum Starten der Chase (nur, wenn nicht bereits im Chase)
        if (currentState != State.Chase && IsPlayerInSight())
        {
            StartChase();
            return;
        }

        // 5) Sonstige State-Logik: Patrol, ForcedRepath, Idle
        switch (currentState)
        {
            case State.Patrol:
                RunPatrolLogic();
                break;
            case State.ForcedRepath:
                RunForcedRepathLogic();
                break;
            case State.Idle:
                // Idle-Routine läuft → nichts weiter machen
                break;
        }

        // Update für stuckCheck auch außerhalb Chase, falls gewünscht
        // optional: implementieren auch in Patrol-Mode
    }


    // -------------------- STATE-METHODEN --------------------

    private void RunPatrolLogic()
    {
        if (currentDest == null) return;

        if (!ai.pathPending)
        {
            if (ai.remainingDistance <= ai.stoppingDistance)
            {
                Debug.Log($"[enemyAI] Patrol-Ziel erreicht: {currentDest.position}. Wechsel zu Idle.");
                EnterIdleState();
            }
            else
            {
                // Optional Debug: verbleibende Distanz
                // Debug.Log($"[enemyAI] Patrol: remainingDistance={ai.remainingDistance:F2}");
            }
        }
        else
        {
            // Optional Debug: Pfadberechnung noch nicht fertig
            // Debug.Log("[enemyAI] Patrol: pathPending...");
        }
    }

    private void RunForcedRepathLogic()
    {
        if (currentDest == null) return;

        if (!ai.pathPending)
        {
            if (ai.remainingDistance <= ai.stoppingDistance)
            {
                Debug.Log($"[enemyAI] ForcedRepath-Ziel erreicht: {currentDest.position}. Wechsel zu Idle.");
                forcedRepathTimer = 0f;
                EnterIdleState();
            }
        }
        else
        {
            // Optional Debug: Pfadberechnung noch nicht fertig
            // Debug.Log("[enemyAI] ForcedRepath: pathPending...");
        }
    }

    private void RunChaseLogic()
    {
        // 1) Abbruchbedingungen prüfen
        bool tooFar = aiDistance > chaseAbortDistance;
        bool notInSight = !IsPlayerInSight();
        Debug.Log($"[enemyAI|RunChaseLogic] lostSightTimer={lostSightTimer:F2}, tooFar={tooFar}, notInSight={notInSight}");

        if (tooFar || notInSight)
        {
            lostSightTimer += Time.deltaTime;
            if (!tooFar && !notInSight)
            {
                lostSightTimer = 0f;
            }
            if (lostSightTimer >= lostSightThreshold)
            {
                Debug.Log($"[enemyAI] Abbruch Chase nach {lostSightTimer:F2}s (zu weit/nicht in Sicht).");
                CancelChase();
                return;
            }
        }
        else
        {
            lostSightTimer = 0f;
        }

        // 2) Setze Ziel auf den Spieler
        if (ai.isStopped)
            ai.isStopped = false;
        ai.destination = player.position;

        // DEBUG: NavMeshAgent-Infos
        Debug.LogFormat("[enemyAI|RunChaseLogic] Setting destination. isOnNavMesh={0}, pathPending={1}, hasPath={2}, pathStatus={3}, remainingDistance={4:F2}, velocity={5:F2}",
                        ai.isOnNavMesh, ai.pathPending, ai.hasPath, ai.pathStatus, ai.remainingDistance, ai.velocity.magnitude);

        // 2a) Zusätzlicher Debug: Pfadecken ausgeben
        if (ai.hasPath && ai.pathStatus == NavMeshPathStatus.PathComplete)
        {
            Vector3[] corners = ai.path.corners;
            Debug.Log($"[enemyAI] Path corners count={corners.Length}");
            for (int i = 0; i < corners.Length; i++)
            {
                Debug.Log($"[enemyAI] Corner[{i}]: {corners[i]}");
                if (i > 0)
                {
                    Debug.DrawLine(corners[i - 1], corners[i], Color.cyan, 1f);
                }
            }
        }
        else
        {
            Debug.LogWarning($"[enemyAI] Kein kompletter Pfad: hasPath={ai.hasPath}, status={ai.pathStatus}");
        }

        // 3) Stuck-Check: velocity nahe 0 aber noch Distanz zum Ziel
        stuckCheckTimer += Time.deltaTime;
        if (stuckCheckTimer >= stuckCheckInterval)
        {
            float distMoved = Vector3.Distance(transform.position, lastPosition);
            if (!ai.pathPending && ai.hasPath && ai.pathStatus == NavMeshPathStatus.PathComplete
                && ai.remainingDistance > ai.stoppingDistance && ai.velocity.magnitude < 0.1f)
            {
                Debug.LogWarning($"[enemyAI] Stecken bei Pos {transform.position}, Ziel {player.position}, remainingDistance={ai.remainingDistance:F2}. Versuch ResetPath.");
                ai.ResetPath();
                ai.SetDestination(player.position);
            }
            lastPosition = transform.position;
            stuckCheckTimer = 0f;
        }

        // 4) Jumpscare prüfen
        if (aiDistance <= catchDistance)
        {
            Debug.Log("[enemyAI] Spieler gefangen. Starte Jumpscare/DeathRoutine.");
            player.gameObject.SetActive(false);
            aiAnim.ResetTrigger("walk");
            aiAnim.ResetTrigger("idle");
            aiAnim.ResetTrigger("sprint");
            aiAnim.SetTrigger("jumpscare");
            StartCoroutine(DeathRoutine());
            currentState = State.Idle;
        }
    }


    // -------------------- STATE-WECHSEL & HILFSMETHODEN --------------------

    private void StartChase()
    {
        currentState = State.Chase;
        ai.speed = chaseSpeed;
        ai.isStopped = false;
        aiAnim.SetTrigger("sprint");
        aiAnim.ResetTrigger("walk");
        aiAnim.ResetTrigger("idle");
        if (backgroundMusic != null && backgroundMusic.isPlaying)
            backgroundMusic.Stop();

        lostSightTimer = 0f;
        StopIdleRoutine();

        Debug.Log($"[enemyAI] Wechsel zu Chase-Mode. PlayerDistance={aiDistance:F2}");
    }

    private void StartForcedRepath()
    {
        ChooseNearestDestinationToPlayer();
        currentState = State.ForcedRepath;
        ai.destination = currentDest.position;
        ai.speed = walkSpeed;
        ai.isStopped = false;
        aiAnim.SetTrigger("walk");
        aiAnim.ResetTrigger("idle");
        forcedRepathTimer = 0f;

        StopIdleRoutine();
        Debug.Log($"[enemyAI] ForcedRepath: ZielIndex={lastDestIndex}, ZielPos={currentDest.position}, PlayerDistance={aiDistance:F2}");
    }

    private void StartPatrol()
    {
        if (destinations.Count == 0) return;

        int newIndex = lastDestIndex;
        if (destinations.Count > 1)
        {
            int attempts = 0;
            while (newIndex == lastDestIndex && attempts < 10)
            {
                newIndex = Random.Range(0, destinations.Count);
                attempts++;
            }
        }
        lastDestIndex = newIndex;
        currentDest = destinations[newIndex];
        currentState = State.Patrol;
        ai.destination = currentDest.position;
        ai.speed = walkSpeed;
        ai.isStopped = false;
        aiAnim.SetTrigger("walk");
        aiAnim.ResetTrigger("idle");

        Debug.Log($"[enemyAI] Start Patrol: ZielIndex={newIndex}, ZielPos={currentDest.position}");
    }

    private void EnterIdleState()
    {
        currentState = State.Idle;
        ai.isStopped = true;
        aiAnim.ResetTrigger("walk");
        aiAnim.ResetTrigger("sprint");
        aiAnim.SetTrigger("idle");

        lostSightTimer = 0f;
        StartIdleRoutine();
    }

    private void ChooseNearestDestinationToPlayer()
    {
        if (destinations.Count == 0) return;

        float bestDist = float.MaxValue;
        int bestIdx = lastDestIndex;
        for (int i = 0; i < destinations.Count; i++)
        {
            float d = Vector3.Distance(player.position, destinations[i].position);
            if (d < bestDist)
            {
                bestDist = d;
                bestIdx = i;
            }
        }
        currentDest = destinations[bestIdx];
        lastDestIndex = bestIdx;
    }


    // -------------------- IDLE-ROUTINE --------------------

    private void StartIdleRoutine()
    {
        if (!isIdleRoutineRunning)
        {
            isIdleRoutineRunning = true;
            Debug.Log($"[enemyAI] Enter Idle-State. Warte zwischen {minIdleTime:F1}s und {maxIdleTime:F1}s.");
            StartCoroutine(IdleCoroutine());
        }
    }

    private void StopIdleRoutine()
    {
        if (isIdleRoutineRunning)
        {
            StopCoroutine(IdleCoroutine());
            Debug.Log("[enemyAI] Idle-Routine gestoppt.");
            isIdleRoutineRunning = false;
        }
    }

    IEnumerator IdleCoroutine()
    {
        float wait = Random.Range(minIdleTime, maxIdleTime);
        yield return new WaitForSeconds(wait);

        if (currentState == State.Idle)
        {
            Debug.Log("[enemyAI] Idle-Zeit vorbei, wechsle zu Patrol.");
            StartPatrol();
        }
        isIdleRoutineRunning = false;
    }


    // -------------------- SIGHT & DEATH --------------------

    bool IsPlayerInSight()
    {
        if (player == null)
            return false;

        Vector3 origin = transform.position + rayCastOffset;
        Vector3 dirToPlayer = (player.position - transform.position).normalized;

        // DEBUG: Zeige Raycast-Origin und Richtung
        Debug.DrawRay(origin, dirToPlayer * sightRayLength, Color.green);
        Debug.Log($"[enemyAI|IsPlayerInSight] Raycast Origin={origin}, Direction={dirToPlayer}, Length={sightRayLength}");

        float angle = Vector3.Angle(transform.forward, dirToPlayer);
        Debug.Log($"[enemyAI|IsPlayerInSight] Angle to player: {angle:F1}° (Threshold {fieldOfViewAngle * 0.5f}°)");
        if (angle < fieldOfViewAngle * 0.5f)
        {
            if (Physics.Raycast(origin, dirToPlayer, out RaycastHit hit, sightRayLength))
            {
                Debug.DrawRay(origin, dirToPlayer * hit.distance, Color.red);
                Debug.Log($"[enemyAI|IsPlayerInSight] Raycast hit: {hit.collider.name}");
                if (hit.collider.CompareTag("Player"))
                {
                    Debug.Log("[enemyAI|IsPlayerInSight] Spieler erkannt!");
                    return true;
                }
                else
                {
                    Debug.Log($"[enemyAI|IsPlayerInSight] Raycast hat anderes getroffen: {hit.collider.name}");
                }
            }
            else
            {
                Debug.Log("[enemyAI|IsPlayerInSight] Raycast hat nichts getroffen.");
            }
        }
        else
        {
            Debug.Log("[enemyAI|IsPlayerInSight] Spieler außerhalb Sichtwinkel.");
        }
        return false;
    }

    IEnumerator DeathRoutine()
    {
        if (hotbarUI != null)
            hotbarUI.SetActive(false);
        yield return new WaitForSeconds(deathDelay);
        if (DeathScreenController.Instance != null)
        {
            DeathScreenController.Instance.ShowDeathScreen();
        }
        else
        {
            Debug.LogWarning("DeathScreenController.Instance ist null! Fallback: lade Todesszene direkt.");
            if (!string.IsNullOrEmpty(deathScene))
                SceneManager.LoadScene(deathScene);
        }
    }


    // ***************** ÖFFENTLICHE METHODE ZUM ABBRECHEN DER JAGD *****************

    public void CancelChase()
    {
        if (currentState == State.Chase)
        {
            currentState = State.Idle;
            ai.isStopped = true;
            aiAnim.ResetTrigger("sprint");
            aiAnim.ResetTrigger("walk");
            aiAnim.SetTrigger("idle");
            if (backgroundMusic != null)
                backgroundMusic.Play();
            StartIdleRoutine();
            lostSightTimer = 0f;

            Debug.Log($"[enemyAI] CancelChase aufgerufen. Wechsel zu Idle. PlayerDistance={aiDistance:F2}");
        }
    }
}
