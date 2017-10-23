//A script attached to the Player object to control it's functions, interactions and movement.

using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{

	//Varibles for movement and such.
	public float moveSpeed = 5;
	public float jumpSpeed = 20;
	public float moveVelocity = 0;
	public float scale = 1;

	//A set of bools to determine the state of the player.
	public bool faster = false;
	public bool jumped = false;
	public bool ducked = false;
	public bool combojump = false;
	public bool isWalking = false;
	public bool grounded = false;

	//Variables to help determine if the player is standing on the ground or not.
	public LayerMask whatisGround;
	public float groundCheckRadius = 0.1f;
	public Transform groundCheck;

	//The players Animator object to changes it sprite and animation depending on it's state.
	public Animator anim;

	public bool[] buttonsPressedP;

	/*
	 * Buttons should act accordingly:
	 * W = Nothing. When pressed with K, Big jump. [comboJump]
	 * A = Move Left, does nothing when pressed with D.
	 * S = Duck, unable to move left or right.
	 * D = Move right, does nothing when pressed with A.
	 * J = Increase movement speed and jump height
	 * K = Normal Jump
	 */

	// Use this for initialization
	void Start ()
	{
		//Buttons should be W,A,S,D + J,K
		buttonsPressedP = new bool[6];

		//Find and assign the objects Animator object to anim.
		anim = GetComponent<Animator> ();
	}
	
	// Update is called once per frame
	void Update ()
	{
		//Slows the player down gradually
		moveVelocity = moveVelocity * 0.90F;

		//Check if the position of groundCheck is within a ground object and if so set to true.
		grounded = Physics2D.OverlapCircle (groundCheck.position, groundCheckRadius, whatisGround);

		//Depending on the state of grounded reflect it in jumped and the animator.
		if (grounded) {
			jumped = false;
			anim.SetBool ("Jumped", false);
		} else {
			anim.SetBool ("Jumped", true);
		}

		//If J is pressed then allow the player to move faster and jump higher.
		if (Input.GetKey (KeyCode.J) || buttonsPressedP [4] == true) {
			faster = true;
		} else {
			faster = false;
		}

		//If S is pressed then reflect it in ducked and the animator.
		if (Input.GetKey (KeyCode.S) || buttonsPressedP [2] == true) {
			ducked = true;
			anim.SetBool ("Ducking", true);
		} else {
			ducked = false;
			anim.SetBool ("Ducking", false);
		}

		//If W is pressed allow the player to jump higher.
		if ((Input.GetKey (KeyCode.W) || buttonsPressedP [0] == true)) {
			combojump = true;
		} else {
			combojump = false;
		}


		//If the player is moving slow enough, reduce movement to 0 and reflect thusly in the animator.
		if (moveVelocity < 0.5 && moveVelocity > -0.5) {
			moveVelocity = 0;
			anim.SetBool ("IsWalking", false);
		} else {
			anim.SetBool ("IsWalking", true);
		}
			
		//If K is pressed and the player hasn't alreadly jump allow the player to jump.
		if ((Input.GetKeyDown (KeyCode.K) || buttonsPressedP [5] == true) && !jumped) {
			Jump ();
		}

		//If the player tries to move right and left at the same time do neither.
		//If the player has ducked the he is unable to move.
		//If the player presses the 'D' key, the player faces right and add the approiate force in that direction. Vice Versa for A.
		if (!ducked) {
			if ((Input.GetKey (KeyCode.D) || buttonsPressedP [3] == true) && !(Input.GetKey (KeyCode.A) || buttonsPressedP [1] == true)) {
				transform.localScale = new Vector3 (scale, scale, 1f);

				if (faster) {
					moveVelocity = -moveSpeed * 1.5f;
				} else {
					moveVelocity = -moveSpeed;
				}
			}

			if ((Input.GetKey (KeyCode.A) || buttonsPressedP [1] == true) && !((Input.GetKey (KeyCode.D) || buttonsPressedP [3] == true))) {
				transform.localScale = new Vector3 (-scale, scale, 1f);

				if (faster) {
					moveVelocity = moveSpeed * 1.5f;
				} else {
					moveVelocity = moveSpeed;
				}
			}
		}

		//Update the players physics velocity.
		GetComponent<Rigidbody2D> ().velocity = new Vector2 (-moveVelocity, GetComponent<Rigidbody2D> ().velocity.y);

		//Resets the rotation of the player that may have changed due to local forces applied or ones external.
		Quaternion temp = new Quaternion (0f, 0f, 0f, 0f);
		GetComponent<Transform> ().rotation = temp;
	}

	//A method that can be called to receive what buttons are being pressed by the NN.
	public void getButtonsPressedPl (bool[] incomingButtons)
	{
		buttonsPressedP = incomingButtons;
	}

	//A method that applies a vertical force to the player causing them to rise or "Jump".
	public void Jump ()
	{
		//Set a temporary variable to the default value for the force of a jump.
		float tempFloat = jumpSpeed;

		//If the player comboJumped increase said force.
		//If the player is moving faster increase the jump's force and either way apply said force top the player.
		if (faster && combojump) {
			GetComponent<Rigidbody2D> ().velocity = new Vector2 (GetComponent<Rigidbody2D> ().velocity.x, tempFloat * 1.3f);
		} else if (faster ^ combojump) {
			GetComponent<Rigidbody2D> ().velocity = new Vector2 (GetComponent<Rigidbody2D> ().velocity.x, tempFloat * 1.2f);
		} else {
			GetComponent<Rigidbody2D> ().velocity = new Vector2 (GetComponent<Rigidbody2D> ().velocity.x, tempFloat);
		}

		//Update the state of the player here and in the animator.
		jumped = true;
		anim.SetBool ("Jumped", true);
	}
}