using UnityEngine;
using TMPro;
using System.Collections;
using System.Text.RegularExpressions;
using System.Text;
using System.Linq;

public class GameManagerRewardSystem
{
  private readonly float ScoreIncreaseAmount = 5f;

  public void AwardScore(Bumper scoringBumper, Bumper opponentBumper)
  {
    // Increase the scoring bumper's reward
    scoringBumper.AddReward(ScoreIncreaseAmount);

    // Decrease the opponent bumper's reward
    opponentBumper.AddReward(-ScoreIncreaseAmount);
  }
}

public class GameManager : MonoBehaviour
{
  [Header("Cameras")]
  public Camera Camera2D;
  public Camera Camera3D;

  [Header("Balls")]
  public GameObject Ball;

  [Header("Bumpers")]
  public GameObject BumperLeft;
  public GameObject BumperRight;

  [Header("Info")]
  public TextMeshPro LeftInfoText;
  public TextMeshPro RightInfoText;
  public TextMeshPro InfoText;

  [Header("Walls")]
  public GameObject WallLeft;
  public GameObject WallRight;
  public GameObject WallTop;
  public GameObject WallBottom;

  [HideInInspector]
  private Rigidbody BallRb;
  private Bumper BumperScriptRight;
  private Bumper BumperScriptLeft;

  [Header("Configs")]
  public float Speed = 3f;
  [HideInInspector]
  public readonly float BallMaxSpeed = 10f;

  private int LeftScore = 0;
  private int RightScore = 0;
  private Coroutine GameTimerCoroutine;

  // Store initial positions
  private Vector3 BallInitialPosition;
  private Vector3 BumperLeftInitialPosition;
  private Vector3 BumperRightInitialPosition;
  private int BumperHitCount = 0;
  // {fieldName:maxSize}
  private readonly string AgentInfoTextTemplate = "Score: {score:11}\nReward: {reward:11}";
  private readonly string InfoTextTemplate = "Speed: {speed:2}\nBumper hits: {bumper_hits:5}";
  private readonly Regex TemplateRegex = new(@"\{(\w+):(\d+)\}", RegexOptions.Compiled); // Notice the compiled option; this is a good choice if the regex doesn't change and is used often.
  private readonly GameManagerRewardSystem RewardSystem = new();

  public int GetLeftScore() { return LeftScore; }
  public int GetRightScore() { return RightScore; }

  IEnumerator GameTimer()
  {
    // Wait for 120 seconds (3 minutes)
    yield return new WaitForSeconds(180f);

    Debug.Log("Game Over!");

    // End the episode
    EndEpisode();
  }

  void Start()
  {
    BallRb = Ball.GetComponent<Rigidbody>();
    BumperScriptLeft = BumperLeft.GetComponent<Bumper>();
    BumperScriptRight = BumperRight.GetComponent<Bumper>();

    // Store initial positions
    BallInitialPosition = Ball.transform.position;
    BumperLeftInitialPosition = BumperLeft.transform.position;
    BumperRightInitialPosition = BumperRight.transform.position;

    GameTimerCoroutine = StartCoroutine(GameTimer());
  }

  private string ReplaceInfoText(string template, TextMeshPro infoText)
  {
    var matches = TemplateRegex.Matches(template);
    var textBuilder = new StringBuilder(template);

    foreach (var match in matches.Cast<Match>())
    {
      var fieldName = match.Groups[1].Value;
      var fieldValue = GetValueForField(fieldName, infoText);
      textBuilder.Replace(match.Value, fieldValue);
    }

    return textBuilder.ToString();
  }

  private string GetValueForField(string fieldName, TextMeshPro infoText)
  {
    return fieldName switch
    {
      "score" => infoText == LeftInfoText ? LeftScore.ToString() : RightScore.ToString(),
      "reward" => infoText == LeftInfoText ? BumperScriptLeft.GetCumulativeReward().ToString() : BumperScriptRight.GetCumulativeReward().ToString(),
      "speed" => BallRb.velocity.magnitude.ToString("0.00"),
      "bumper_hits" => BumperHitCount.ToString(),
      _ => throw new System.InvalidOperationException($"Unsupported field name: {fieldName}"),
    };
  }

  private void UpdateInfoText()
  {
    // Update side info texts
    LeftInfoText.text = ReplaceInfoText(AgentInfoTextTemplate, LeftInfoText);
    RightInfoText.text = ReplaceInfoText(AgentInfoTextTemplate, RightInfoText);

    // Update the central info text
    InfoText.text = ReplaceInfoText(InfoTextTemplate, null); // Assuming null here as the method can handle the null value
  }

  void Update()
  {
    AlternateCameras();
    UpdateInfoText();
  }

  private void AlternateCameras()
  {
    if (Input.GetKeyDown(KeyCode.C))
    {
      Camera2D.enabled = !Camera2D.enabled;
      Camera3D.enabled = !Camera3D.enabled;
    }
  }

  public void IncrementScoreLeft()
  {
    LeftScore++;
    RewardSystem.AwardScore(BumperScriptLeft, BumperScriptRight);
    ResetBall();
  }

  public void IncrementScoreRight()
  {
    RightScore++;
    RewardSystem.AwardScore(BumperScriptRight, BumperScriptLeft);
    ResetBall();
  }

  public void Reset()
  {
    LeftScore = 0;
    RightScore = 0;

    ResetBall();

    // Restart the game timer
    if (GameTimerCoroutine != null)
    {
      StopCoroutine(GameTimerCoroutine);
    }
    GameTimerCoroutine = StartCoroutine(GameTimer());
  }

  private float CalculateCurrentSpeed()
  {
    // Calculate current speed based on the base speed and the number of times the ball was hit
    var computedSpeed = Speed + BumperHitCount * 0.1f;
    return Mathf.Min(computedSpeed, BallMaxSpeed);
  }

  void UpdateBallSpeed(Vector3 velocity)
  {
    var currentSpeed = CalculateCurrentSpeed();
    BallRb.velocity = velocity.normalized * currentSpeed;
  }

  void LaunchBall()
  {
    // Flip a coin to decide the direction on the X axis
    float sx = Random.Range(0, 2) == 0 ? -1 : 1;

    // Flip a coin to decide the direction on the Z axis
    float sz = Random.Range(0, 2) == 0 ? -1 : 1;

    // Calculate additional speed based on the number of times the ball was hit
    // Calculate a random angle within the specified margin:
    var angleMargin = 120f; // Angle margin in degrees from the center line
    var minAngle = -angleMargin / 2;
    var maxAngle = angleMargin / 2;
    var angle = Random.Range(minAngle, maxAngle);

    // Convert the angle to radians for Mathf.Sin and Mathf.Cos functions
    var angleInRadians = angle * Mathf.Deg2Rad;

    // Calculate the velocity vector within the angle margin
    var velocity = new Vector3(
        Speed * Mathf.Cos(angleInRadians) * sx,
        Speed * Mathf.Sin(angleInRadians) * sz,
        0f
    );

    UpdateBallSpeed(velocity);

    // Debug.Log("Ball launched with speed: " + currentSpeed);
  }

  public void IncrementBumperHitCount()
  {
    BumperHitCount++;
    // Debug.Log("Bumper hit count: " + BumperHitCount);

    UpdateBallSpeed(BallRb.velocity);

    // Debug.Log("Ball speed after bumper hit: " + currentSpeed);
  }

  public void ResetBall()
  {
    // Give ball a random color
    var renderer = Ball.GetComponent<Renderer>();
    renderer.material.color = new Color(Random.value, Random.value, Random.value);

    // Reset positions and velocities
    Ball.transform.position = BallInitialPosition;
    BumperLeft.transform.position = BumperLeftInitialPosition;
    BumperRight.transform.position = BumperRightInitialPosition;

    LaunchBall();
  }

  public static GameManager GetInstance(GameObject environment)
  {
    return environment.GetComponent<GameManager>();
  }

  public void EndEpisode()
  {
    // Reset bumper hit count
    BumperHitCount = 0;

    // Stop the game timer coroutine if it's still running
    if (GameTimerCoroutine != null)
    {
      StopCoroutine(GameTimerCoroutine);
      GameTimerCoroutine = null;
    }

    BumperScriptLeft.EndEpisode();
    BumperScriptRight.EndEpisode();
  }
}
