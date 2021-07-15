using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;

public class CapsuleAgent : Agent
{
  private Rigidbody rigidBody;

  public override void Initialize()
  {
    rigidBody = GetComponent<Rigidbody>();
  }

  public override void OnEpisodeBegin()
  {
    transform.localRotation = Quaternion.identity;
    transform.localPosition = new Vector3(0, 1, 0);
  }

  public override void Heuristic(in ActionBuffers actionsOut)
  {
    ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
    continuousActions[0] = Input.GetAxisRaw("Horizontal");
    continuousActions[1] = Input.GetAxisRaw("Vertical");
    continuousActions[2] = 1f;
  }

  private float lastDistanceFromCenter = 0f;
  public override void OnActionReceived(ActionBuffers actions)
  {
    float speed = actions.ContinuousActions[2];
    float x = actions.ContinuousActions[0] * speed;
    float z = actions.ContinuousActions[1] * speed;
    rigidBody.AddForce(x, 0, z, ForceMode.Impulse);
    float distanceFromCenter = Quaternion.Angle(transform.localRotation, Quaternion.identity);
    if (distanceFromCenter > lastDistanceFromCenter) {
      AddReward(-.01f);
    } else {
      AddReward(+.01f);
    }
  }
}
