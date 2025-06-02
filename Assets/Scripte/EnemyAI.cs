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
    public float fieldOfViewAngle = 110f;    // Sichtwinkel
    public float sightRayLength = 15f;     // Raycast-Länge
    public Vector3 rayCastOffset;             // Raycast-Offset

    [Header("Forced Repath")]
    public float maxAllowedDistance = 100f;  // Wechsel zu ForcedRepath, wenn weiter entfernt
    public float forcedRepathCooldown = 2f;    // Cooldown in Sekunden

    [Header("Tod & Szenenwechsel")]
    public string deathScene;
    public float deathDelay = 2f;


    // -------------------- PRIVATE FIELDS --------------------
    private enum State { Idle, Patrol, ForcedRepath, Chase }
    private State currentState;

    private int lastDestIndex = -1;
    private Transform currentDest;
    private bool isIdleRoutineRunning = false;
    private float forcedRepathTimer = 0f;
    private float aiDistance = 0f;
    private float lostSightTimer = 0f;

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
        aiDistance = Vector3.Distance(player.position, transform.position);
        //Debug.Log($"[enemyAI|OnEnable] Start-Distance to player: {aiDistance:F2} m.");

        // 3) Erstes Patrol-Ziel wählen
        if (destinations != null && destinations.Count > 0)
        {
            int r = Random.Range(0, destinations.Count);
            currentDest = destinations[r];
            lastDestIndex = r;
            ai.destination = currentDest.position;
            ai.speed = walkSpeed;
            ai.isStopped = false;
            aiAnim.SetTrigger("walk");
        }
        else
        {
            Debug.LogWarning("[enemyAI] Keine Ziele (destinations) zugewiesen!");
        }
    }

    void Update()
    {
        // 1) Aktuellen Abstand zum Spieler berechnen
        aiDistance = Vector3.Distance(player.position, transform.position);
        // Debug-Ausgabe
        //Debug.Log($"[enemyAI|Update] Distance to player: {aiDistance:F2} m (State: {currentState})");

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
    }


    // -------------------- STATE-METHODEN --------------------

    private void RunPatrolLogic()
    {
        if (currentDest == null) return;

        // Wenn Ziel erreicht, direkt in Idle
        if (!ai.pathPending && ai.remainingDistance <= ai.stoppingDistance)
        {
            EnterIdleState();
        }
    }

    private void RunForcedRepathLogic()
    {
        if (currentDest == null) return;

        // Wenn Forced-Repath-Ziel erreicht, in Idle übergehen
        if (!ai.pathPending && ai.remainingDistance <= ai.stoppingDistance)
        {
            forcedRepathTimer = 0f;
            EnterIdleState();
        }
    }

    private void RunChaseLogic()
    {
        bool tooFar = aiDistance > chaseAbortDistance;
        bool notInSight = !IsPlayerInSight();

        // 1) Wenn zu weit weg ODER nicht im Sichtfeld, starte lostSight-Timer
        if (tooFar || notInSight)
        {
            lostSightTimer += Time.deltaTime;
            // Reset, sobald wieder beides OK ist
            if (!tooFar && !notInSight)
            {
                lostSightTimer = 0f;
            }
            // Nur nach Überschreiten des Thresholds abbrechen
            if (lostSightTimer >= lostSightThreshold)
            {
                Debug.Log($"[enemyAI] Abbruch Chase nach {lostSightTimer:F2}s (zu weit oder nicht im Sichtfeld).");
                CancelChase();
                return;
            }
        }
        else
        {
            // beides OK → Timer zurücksetzen
            lostSightTimer = 0f;
        }

        // 2) Setze Ziel auf den Spieler (flüssiges Verfolgen)
        if (ai.isStopped)
            ai.isStopped = false;

        ai.destination = player.position;
        // Geschwindigkeit und Animation brauchen wir jetzt nicht jeden Frame neu setzen:
        // Nur beim Einstieg in Chase wurde ai.speed = chaseSpeed und Animator 'sprint' getriggert.

        // 3) Wenn sehr nahe genug, Jumpscare auslösen
        if (aiDistance <= catchDistance)
        {
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
        ai.isStopped = false;          // Agent darf laufen
        aiAnim.SetTrigger("sprint");   // Nur EINMAL beim Zustandswechsel
        aiAnim.ResetTrigger("walk");
        aiAnim.ResetTrigger("idle");
        if (backgroundMusic != null && backgroundMusic.isPlaying)
            backgroundMusic.Stop();

        // Reset des Lost‐Sight‐Timers
        lostSightTimer = 0f;
        // Stopp Idle‐Routine, falls sie gerade lief
        StopIdleRoutine();
    }

    private void StartForcedRepath()
    {
        // Wähle Ziel, das dem Spieler am nächsten ist
        ChooseNearestDestinationToPlayer();
        currentState = State.ForcedRepath;
        ai.destination = currentDest.position;
        ai.speed = walkSpeed;
        ai.isStopped = false;
        aiAnim.SetTrigger("walk");   // Nur EINMAL beim Zustandswechsel
        aiAnim.ResetTrigger("idle");
        forcedRepathTimer = 0f;

        StopIdleRoutine();
    }

    private void StartPatrol()
    {
        // Wähle ein zufälliges Ziel, das sich vom letzten unterscheidet
        if (destinations.Count == 0) return;

        int newIndex = lastDestIndex;
        if (destinations.Count > 1)
        {
            while (newIndex == lastDestIndex)
            {
                newIndex = Random.Range(0, destinations.Count);
            }
        }
        lastDestIndex = newIndex;
        currentDest = destinations[newIndex];
        currentState = State.Patrol;
        ai.destination = currentDest.position;
        ai.speed = walkSpeed;
        ai.isStopped = false;
        aiAnim.SetTrigger("walk");   // Nur EINMAL beim Zustandswechsel
        aiAnim.ResetTrigger("idle");
    }

    private void EnterIdleState()
    {
        currentState = State.Idle;
        ai.isStopped = true;         // Laufen stoppen
        aiAnim.ResetTrigger("walk");
        aiAnim.ResetTrigger("sprint");
        aiAnim.SetTrigger("idle");   // Nur EINMAL beim Zustandswechsel

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
            StartCoroutine(IdleCoroutine());
        }
    }

    private void StopIdleRoutine()
    {
        if (isIdleRoutineRunning)
        {
            StopCoroutine(IdleCoroutine());
            isIdleRoutineRunning = false;
        }
    }

    IEnumerator IdleCoroutine()
    {
        float wait = Random.Range(minIdleTime, maxIdleTime);
        yield return new WaitForSeconds(wait);

        // Wenn nach Wartezeit immer noch Idle, wechsle zu Patrol
        if (currentState == State.Idle)
        {
            StartPatrol();
        }
        isIdleRoutineRunning = false;
    }


    // -------------------- SIGHT & DEATH --------------------

    bool IsPlayerInSight()
    {
        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        Debug.DrawRay(transform.position + rayCastOffset, dirToPlayer * sightRayLength, Color.green);

        float angle = Vector3.Angle(transform.forward, dirToPlayer);
        if (angle < fieldOfViewAngle * 0.5f)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position + rayCastOffset, dirToPlayer, out hit, sightRayLength))
            {
                Debug.DrawRay(transform.position + rayCastOffset, dirToPlayer * hit.distance, Color.red);
                if (hit.collider.CompareTag("Player"))
                {
                    return true;
                }
            }
        }
        return false;
    }

    IEnumerator DeathRoutine()
    {
        yield return new WaitForSeconds(deathDelay);
        SceneManager.LoadScene(deathScene);
    }


    // ***************** ÖFFENTLICHE METHODE ZUM ABBRECHEN DER JAGD *****************

    /// <summary>
    /// Bricht die aktuelle Jagd (Chase) ab und wechselt in Idle.
    /// </summary>
    public void CancelChase()
    {
        if (currentState == State.Chase)
        { 
            

            currentState = State.Idle;
            ai.isStopped = true;          // Agent anhalten
            aiAnim.ResetTrigger("sprint");
            aiAnim.ResetTrigger("walk");
            aiAnim.SetTrigger("idle");    // Nur EINMAL beim Zustandswechsel

            if (backgroundMusic != null)
                backgroundMusic.Play();

            StartIdleRoutine();
            lostSightTimer = 0f;
        }
    }
}
