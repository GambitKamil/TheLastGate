
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class CompanionAgent : Agent
{
    [Header("Training Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float maxEpisodeTime = 60f;

    [Header("Game Mode")]
    [SerializeField] private bool trainingMode = true;
    [SerializeField] private bool startAutomatically = true;

    [Header("Reward Settings")]
    [SerializeField] private float successReward = 2f;
    [SerializeField] private float stepPenalty = -0.0002f;
    [SerializeField] private float progressRewardScale = 0f;

    [Header("Training Environment")]
    [SerializeField] private BridgeController trainingBridge;
    [SerializeField] private SwitchController trainingSwitch;
    [Header("Training Start")]
    [SerializeField] private bool randomizeTrainingStart = true;
    [SerializeField, Min(0f)] private float startOffsetX = 0.25f;

    private Rigidbody2D rb;
    private Vector2 startPosition;
    private float episodeTimer;
    private float previousDistance;

    private bool targetReached;
    private bool navigationActive;
    private bool episodeEnding;

    public bool TargetReached => targetReached;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody2D>();

        startPosition = rb.position;

        navigationActive =
            trainingMode || startAutomatically;
    }

    public override void OnEpisodeBegin()
    {
        targetReached = false;
        episodeEnding = false;
        episodeTimer = 0f;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        Vector2 episodeStart = startPosition;

        if (trainingMode && randomizeTrainingStart)
        {
            episodeStart.x += Random.Range(-startOffsetX, startOffsetX);
        }

        rb.position = episodeStart;

        navigationActive =
            trainingMode || startAutomatically;

        if (target != null)
        {
            previousDistance = Vector2.Distance(
                rb.position,
                target.position
            );
        }

        if (trainingMode)
        {
            ResetTrainingEnvironment();
        }
    }

    private void ResetTrainingEnvironment()
    {
        if (trainingSwitch != null)
        {
            trainingSwitch.ResetSwitch();
        }

        if (trainingBridge != null)
        {
            trainingBridge.ResetBridge();
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector2 direction = target != null
            ? (Vector2)target.position - rb.position
            : Vector2.zero;

        sensor.AddObservation(direction / 12f);
        sensor.AddObservation(rb.linearVelocity / moveSpeed);
    }

    public override void OnActionReceived(
        ActionBuffers actions)
    {
        if (episodeEnding)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (!trainingMode &&
            (!navigationActive ||
             targetReached ||
             target == null))
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        int horizontal =
            actions.DiscreteActions[0] - 1;

        int vertical =
            actions.DiscreteActions[1] - 1;

        Vector2 movement = new Vector2(
            horizontal,
            vertical
        ).normalized;

        rb.linearVelocity = movement * moveSpeed;

        if (!trainingMode)
            return;

        AddReward(stepPenalty);

        if (target != null)
        {
            float currentDistance =
                Vector2.Distance(
                    rb.position,
                    target.position
                );

            float improvement =
                previousDistance - currentDistance;

            AddReward(
                improvement * progressRewardScale
            );

            previousDistance = currentDistance;
        }

        episodeTimer += Time.fixedDeltaTime;

        if (episodeTimer >= maxEpisodeTime)
        {
            FinishTrainingEpisode(false);
        }
    }

    private void FinishTrainingEpisode(bool success)
    {
        if (!trainingMode || episodeEnding)
            return;

        episodeEnding = true;

        if (success)
        {
            AddReward(successReward);
        }

        rb.linearVelocity = Vector2.zero;

        // Сначала завершаем эпизод.
        // ML-Agents может вызвать OnEpisodeBegin
        // при следующем запросе решения.
        EndEpisode();

        // Не сбрасываем мост здесь:
        // это выполняется в OnEpisodeBegin.
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Finish"))
            return;

        if (targetReached || episodeEnding)
            return;

        if (!trainingMode && !navigationActive)
            return;

        targetReached = true;

        if (trainingMode)
        {
            FinishTrainingEpisode(true);
        }
        else
        {
            navigationActive = false;
            rb.linearVelocity = Vector2.zero;

            Debug.Log(
                "Companion reached switch in game mode!"
            );
        }
    }

    public void StartNavigation()
    {
        if (trainingMode)
        {
            Debug.LogWarning(
                "StartNavigation is only for game mode."
            );
            return;
        }

        if (targetReached || navigationActive)
            return;

        if (target == null)
        {
            Debug.LogWarning(
                "Companion target is not assigned!"
            );
            return;
        }

        navigationActive = true;

        Debug.Log(
            "Companion navigation started!"
        );
    }

    public override void Heuristic(
        in ActionBuffers actionsOut)
    {
        var actions = actionsOut.DiscreteActions;

        actions[0] = 1;
        actions[1] = 1;

        if (Input.GetKey(KeyCode.A))
            actions[0] = 0;

        if (Input.GetKey(KeyCode.D))
            actions[0] = 2;

        if (Input.GetKey(KeyCode.S))
            actions[1] = 0;

        if (Input.GetKey(KeyCode.W))
            actions[1] = 2;
    }
}
