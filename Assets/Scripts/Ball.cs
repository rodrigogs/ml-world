using UnityEngine;

public class Ball : MonoBehaviour
{
  public GameObject Environment;
  public GameObject WallLeft;
  public GameObject WallRight;
  private GameManager GameManager;
  private Rigidbody Rb;
  private Color OriginalWallColor;

  void Start()
  {
    GameManager = GameManager.GetInstance(Environment);
    Rb = GetComponent<Rigidbody>();
    OriginalWallColor = WallLeft.GetComponent<Renderer>().material.color;
  }

  void Update()
  {
    var wallTopPosition = GameManager.WallTop.transform.position.y - GameManager.WallTop.transform.localScale.z / 2;
    // Debug.DrawRay(new Vector3(-5f, wallTopPosition, 0), Vector3.right * 10, Color.red, 1f);
    var wallBottomPosition = GameManager.WallBottom.transform.position.y + GameManager.WallBottom.transform.localScale.z / 2;
    // Debug.DrawRay(new Vector3(-5, wallBottomPosition, 0), Vector3.right * 10, Color.red, 1f);
    var wallLeftPosition = GameManager.WallLeft.transform.position.x + GameManager.WallLeft.transform.localScale.x / 2;
    // Debug.DrawRay(new Vector3(wallLeftPosition, -2.5f, 0), Vector3.up * 5, Color.red, 1f);
    var wallRightPosition = GameManager.WallRight.transform.position.x - GameManager.WallRight.transform.localScale.x / 2;
    // Debug.DrawRay(new Vector3(wallRightPosition, -2.5f, 0), Vector3.up * 5, Color.red, 1f);
    // Draw a crosshair for the ball
    // Debug.DrawRay(transform.position, Vector3.left * 0.2f, Color.green, 1f);
    // Debug.DrawRay(transform.position, Vector3.right * 0.2f, Color.green, 1f);
    // Debug.DrawRay(transform.position, Vector3.up * 0.2f, Color.green, 1f);
    // Debug.DrawRay(transform.position, Vector3.down * 0.2f, Color.green, 1f);

    // If the ball is out of bounds, end the episode
    if (transform.position.y > wallTopPosition || transform.position.y < wallBottomPosition ||
        transform.position.x < wallLeftPosition || transform.position.x > wallRightPosition)
    {
      Debug.Log("Ball out of bounds!");
      GameManager.EndEpisode();
    }
  }

  private void OnCollisionEnter(Collision collision)
  {
    // If the ball hits the left wall
    if (collision.gameObject == WallLeft)
    {
      // Increment the right player's score
      GameManager.IncrementScoreRight();
      collision.gameObject.GetComponent<Renderer>().material.color = Color.red;
      Invoke(nameof(ResetLeftWallColor), .5f);
    }

    // If the ball hits the right wall
    if (collision.gameObject == WallRight)
    {
      // Increment the left player's score
      GameManager.IncrementScoreLeft();
      collision.gameObject.GetComponent<Renderer>().material.color = Color.red;
      Invoke(nameof(ResetRightWallColor), .5f);
    }
  }

  private void ResetLeftWallColor()
  {
    WallLeft.GetComponent<Renderer>().material.color = OriginalWallColor;
  }

  private void ResetRightWallColor()
  {
    WallRight.GetComponent<Renderer>().material.color = OriginalWallColor;
  }
}
