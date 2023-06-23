using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;
using Unity.MLAgents.Sensors;

public class CapsuleAgent : Agent
{
  private Rigidbody rigidBody;
  private Vector3 balancePoint;
  private Vector3 pokePoint1;
  private Vector3 pokePoint2;
  private Vector3 episodeStartPosition;

  public override void Initialize()
  {
    rigidBody = GetComponent<Rigidbody>();
    balancePoint = transform.localPosition + new Vector3(0, transform.localScale.y - 0.1f, 0);
    pokePoint1 = transform.localPosition + new Vector3(0.1f, transform.localScale.y - 0.1f, 0);
    pokePoint2 = transform.localPosition + new Vector3(0, transform.localScale.y - 0.1f, 0.1f);
  }

  public override void OnEpisodeBegin()
  {
    transform.localRotation = Quaternion.identity;
    episodeStartPosition = transform.localPosition; // Store the episode start position
    transform.localPosition = new Vector3(Random.Range(-0.4f, 0.4f), 0.5f, Random.Range(-0.4f, 0.4f)); // Set the initial position to a random position on the platform
    rigidBody.angularVelocity = Vector3.zero;
    rigidBody.velocity = Vector3.zero;

    Renderer renderer = GetComponent<Renderer>();
    renderer.material.color = new Color(Random.value, Random.value, Random.value);
  }

  public override void OnActionReceived(ActionBuffers actions)
  {
    float speed = actions.ContinuousActions[2];

    float x = actions.ContinuousActions[0] * speed;
    float z = actions.ContinuousActions[1] * speed;

    Vector3 forceDirection = new Vector3(x, 0, z);
    Vector3 balanceDirection = balancePoint - transform.localPosition;
    float balanceDistance = balanceDirection.magnitude;
    balanceDirection.Normalize();
    float forceMagnitude = balanceDistance * 0.1f;
    Vector3 balanceForce = balanceDirection * forceMagnitude;

    // Apply forces to the capsule at two points on the upper part
    Vector3 pokeForce1 = new Vector3(-x, 0, 0) * 0.1f;
    Vector3 pokeForce2 = new Vector3(0, 0, -z) * 0.1f;
    rigidBody.AddForceAtPosition(pokeForce1, pokePoint1, ForceMode.Impulse);
    rigidBody.AddForceAtPosition(pokeForce2, pokePoint2, ForceMode.Impulse);

    float distanceFromStart = Vector3.Distance(transform.localPosition, episodeStartPosition); // Calculate the distance from the episode start position
    float penalty = 1f - Mathf.Abs(transform.up.y) - (distanceFromStart / 10f); // Subtract the distance from the episode start position from the penalty
    float reward = 1f - (balanceDistance / 10f);

    AddReward(reward - penalty);
  }

  public override void Heuristic(in ActionBuffers actionsOut)
  {
    var continuousActionsOut = actionsOut.ContinuousActions;
    continuousActionsOut[0] = Input.GetAxis("Horizontal");
    continuousActionsOut[1] = Input.GetAxis("Vertical");
    continuousActionsOut[2] = Input.GetKey(KeyCode.Space) ? 1.0f : 0.0f;
  }

  public override void CollectObservations(VectorSensor sensor)
  {
    sensor.AddObservation(transform.localPosition);
    sensor.AddObservation(transform.localRotation);
    sensor.AddObservation(rigidBody.velocity);
    sensor.AddObservation(rigidBody.angularVelocity);
  }
}
