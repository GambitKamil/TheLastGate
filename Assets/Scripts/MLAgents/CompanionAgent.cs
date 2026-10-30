
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class CompanionAgent : Agent
{
    [Header("Training Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float maxEpisodeTime = 30f;

    private Rigidbody2D rb;
    private Vector2 startPosition;
    private float episodeTimer;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position;
    }

    public override void OnEpisodeBegin()
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        transform.position = startPosition;
        episodeTimer = 0f;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Hedefe göre agent konumu
        Vector2 direction = target.position - transform.position;
        sensor.AddObservation(direction / 12f);

        
        sensor.AddObservation(rb.linearVelocity / moveSpeed);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int horizontal = actions.DiscreteActions[0] - 1;
        int vertical = actions.DiscreteActions[1] - 1;

        Vector2 movement = new Vector2(horizontal, vertical).normalized;
        rb.linearVelocity = movement * moveSpeed;

        // Küçük bir ceza, hedefe hızlı bir şekilde ulaşılmasını teşvik eder
        AddReward(-0.001f);

        episodeTimer += Time.fixedDeltaTime;

        if (episodeTimer >= maxEpisodeTime)
        {
            EndEpisode();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Finish"))
        {
            AddReward(1f);
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;

        discreteActions[0] = 1;
        discreteActions[1] = 1;

        if (Input.GetKey(KeyCode.A)) discreteActions[0] = 0;
        if (Input.GetKey(KeyCode.D)) discreteActions[0] = 2;
        if (Input.GetKey(KeyCode.S)) discreteActions[1] = 0;
        if (Input.GetKey(KeyCode.W)) discreteActions[1] = 2;
    }
}
