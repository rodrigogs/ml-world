using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class BumperRewardSystem
{
  public GameObject Environment { get; private set; }
  public Bumper Bumper { get; private set; }
  public float HitBallReward { get; set; } = .1f;
  public float MovementPenalty { get; set; } = -.01f;

  public BumperRewardSystem(GameObject environment, Bumper bumper)
  {
    Environment = environment;
    Bumper = bumper;
  }

  public void HandleBallHit()
  {
    var ball = GameManager.GetInstance(Environment).Ball;
    var ballRb = ball.GetComponent<Rigidbody>();
    var bumperRb = Bumper.GetComponent<Rigidbody>();

    // Should influence ball's direction angle based on the bumper's movement at the time of the hit
    // If the bumper is moving up, the ball should slightly angle up in the y axis
    // If the bumper is moving down, the ball should slightly angle down in the y axis
    // If the bumper is not moving, the ball should go in its current angle
    // Calculate the y velocity based on bumper movement speed
    float bumperSpeedY = bumperRb.velocity.y;
    float influenceY = bumperSpeedY * 50f; // Adjust the 0.1f value to increase or decrease the influence

    // Calculate the direction and magnitude of the ball's reflection
    Vector3 ballDirection = Vector3.Reflect(ballRb.velocity.normalized, Vector3.forward);
    ballDirection.y += influenceY; // Apply influence based on the bumper's movement
    ballDirection.Normalize();

    // Set the new velocity of the ball with a constant speed, maintaining direction change
    float ballSpeed = 10f; // You can adjust the speed as needed
    ballRb.velocity = ballDirection * ballSpeed;

    // Should increase the reward relative to hits on the ball
    Bumper.AddReward(HitBallReward * ballRb.velocity.magnitude);
  }

  public void HandleMovement()
  {
    Bumper.AddReward(MovementPenalty); // Penalize for movement to encourage efficiency
  }

  public void ChangeBumperColorBasedOnScore()
  {
    var gameManager = GameManager.GetInstance(Environment);
    var myScore = Bumper.IsLeft ? gameManager.GetLeftScore() : gameManager.GetRightScore();

    if (myScore > 0)
    {
      var color = new Color(5f, 1f - (myScore / 10f), 1f - (myScore / 10f), 1f);
      Bumper.GetComponent<Renderer>().material.color = color;
    }
  }
}

public class Bumper : Agent
{
  public bool IsLeft;
  public float Speed = 5f;
  public int MaxScore = 5;
  public GameObject Environment;
  private GameManager GameManager;
  private string BumperName;

  // Flags for bumper movement restriction after hitting walls
  private bool CanMoveUp = true;
  private bool CanMoveDown = true;
  private Color BumperOriginalColor;
  private BumperRewardSystem RewardSystem;

  void Start()
  {
    GameManager = GameManager.GetInstance(Environment);

    BumperName = IsLeft ? "[Bumper Left]" : "[Bumper Right]";
    BumperOriginalColor = GetComponent<Renderer>().material.color;
    RewardSystem = new BumperRewardSystem(Environment, this);
  }

  private void OnCollisionEnter(Collision collision)
  {
    // If hit the ball
    if (collision.gameObject == GameManager.Ball)
    {
      // And if it's in the inner side, depending on the side of the bumper
      if ((IsLeft && GameManager.Ball.transform.position.x > transform.position.x) ||
          (!IsLeft && GameManager.Ball.transform.position.x < transform.position.x))
      {
        // Debug.Log($"{BumperName} Hit the ball in the inner side");
        RewardSystem.HandleBallHit();
        GameManager.IncrementBumperHitCount();
      }
    }
    // If hit the top wall
    if (collision.gameObject == GameManager.WallTop)
    {
      CanMoveUp = false;
    }
    // If hit the bottom wall
    else if (collision.gameObject == GameManager.WallBottom)
    {
      CanMoveDown = false;
    }
  }

  void Update()
  {
    var myScore = IsLeft ? GameManager.GetLeftScore() : GameManager.GetRightScore();

    // Debug.Log($"{BumperName} Score: {myScore} | Reward: {GetCumulativeReward()}");

    if (myScore >= MaxScore)
    {
      GameManager.EndEpisode();
    }
  }

  public override void OnEpisodeBegin()
  {
    CanMoveUp = true;
    CanMoveDown = true;
    GameManager.Reset();
    GetComponent<Renderer>().material.color = BumperOriginalColor;
  }

  private int NormalizeAction(float action)
  {
    if (action > 0)
    {
      return 1;
    }
    else if (action < 0)
    {
      return 2;
    }
    else
    {
      return 0;
    }
  }

  public override void Heuristic(in ActionBuffers actionsOut)
  {
    var discreteActionsOut = actionsOut.DiscreteActions;
    if (IsLeft)
    {
      discreteActionsOut[0] = NormalizeAction(Input.GetAxis("VerticalLeft"));
    }
    else
    {
      discreteActionsOut[0] = NormalizeAction(Input.GetAxis("VerticalRight"));
    }
  }

  public override void OnActionReceived(ActionBuffers actions)
  {
    // Debug.Log($"{BumperName} Received Action: {actions.DiscreteActions[0]}");

    var goUp = actions.DiscreteActions[0] == 1;
    var goDown = actions.DiscreteActions[0] == 2;

    // Debug.Log($"{BumperName} Go Up: {goUp} | Go Down: {goDown} | Can Move Up: {CanMoveUp} | Can Move Down: {CanMoveDown}");

    if (goUp && CanMoveUp)
    {
      transform.Translate(Speed * Time.deltaTime * Vector3.forward);
      CanMoveDown = true;
    }
    else if (goDown && CanMoveDown)
    {
      transform.Translate(Speed * Time.deltaTime * Vector3.back);
      CanMoveUp = true;
    }

    // Penalize the agent if it do move
    if (goUp || goDown)
    {
      RewardSystem.HandleMovement();
    }

    RewardSystem.ChangeBumperColorBasedOnScore();
  }

  private float ResolveNormalizedDeltaY()
  {
    var ball = GameManager.Ball;
    var normalizedDeltaY = (ball.transform.position.y - transform.position.y) / Environment.transform.localScale.y;

    // Drawing a line from the bumper to a position based on normalizedDeltaY
    Debug.DrawLine(transform.position, new Vector3(transform.position.x, transform.position.y + normalizedDeltaY, transform.position.z), Color.green);

    return normalizedDeltaY;
  }

  private float ResolveNormalizedDeltaX()
  {
    var ball = GameManager.Ball;
    var normalizedDeltaX = (ball.transform.position.x - transform.position.x) / Environment.transform.localScale.x;

    // Drawing a line from the bumper to a position based on normalizedDeltaX leaving a margin
    Debug.DrawLine(transform.position, new Vector3(transform.position.x + (normalizedDeltaX * .9f), transform.position.y, transform.position.z), Color.red);

    return normalizedDeltaX;
  }

  private float ResolveNormalizedVelocityX()
  {
    var ball = GameManager.Ball;
    var normalizedVelocityX = ball.GetComponent<Rigidbody>().velocity.x / GameManager.BallMaxSpeed;

    // Drawing a line from the ball to a position based on normalizedVelocityX
    Debug.DrawLine(ball.transform.position, new Vector3(ball.transform.position.x + normalizedVelocityX, ball.transform.position.y, ball.transform.position.z), Color.blue);

    return normalizedVelocityX;
  }

  private float ResolveNormalizedVelocityY()
  {
    var ball = GameManager.Ball;
    var normalizedVelocityY = ball.GetComponent<Rigidbody>().velocity.y / GameManager.BallMaxSpeed;

    // Drawing a line from the ball to a position based on normalizedVelocityY
    Debug.DrawLine(ball.transform.position, new Vector3(ball.transform.position.x, ball.transform.position.y + normalizedVelocityY, ball.transform.position.z), Color.yellow);

    return normalizedVelocityY;
  }

  void FixedUpdate()
  {
    ResolveNormalizedDeltaY();
    ResolveNormalizedDeltaX();
    ResolveNormalizedVelocityX();
    ResolveNormalizedVelocityY();
  }


  public override void CollectObservations(VectorSensor sensor)
  {
    // Distance from the bumper to the ball on the y axis (1 observation)
    sensor.AddObservation(ResolveNormalizedDeltaY());

    // Distance from the bumper to the ball on the x axis (1 observation)
    sensor.AddObservation(ResolveNormalizedDeltaX());

    // Speed of the ball on the x axis (1 observation)
    sensor.AddObservation(ResolveNormalizedVelocityX());

    // Speed of the ball on the y axis (1 observation)
    sensor.AddObservation(ResolveNormalizedVelocityY());

    // Total: 4 observations
    // This should be the number of neurons in the input layer of the neural network
  }
}
