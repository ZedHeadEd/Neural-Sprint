//This is a script attached to the Slime enemy of the game, which controls its movement, functions and collisions and what to do when touching and not touching certain objects.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeEnemyController : MonoBehaviour{

	//Variables use for the Slime such as how fast to move, what direction to move and whether what it looks out for and where it does so.
	public float moveSpeed = 2f;
	public bool moveRight = false;
	public bool hittingWall = false;
	public bool hittingBorder = false;
	public bool notAtEdge = false;
	public bool hittingEnemy = false;
	public float checkRadius = 0.1f;

	//These variables are assigned via the Unity editor.
	public Transform wallCheck;
	public Transform edgeCheck;
	public Transform enemyCheck;
	public Transform borderCheck;
	public LayerMask whatIsWall;
	public LayerMask whatIsEnemy;
	public LayerMask whatIsBorder;

	// Use this for initialization
	void Start (){
		
	}
	
	// Update is called once per frame
	void Update (){

		//Set these bools to whether or not the transforms (points in space relative to the Slime itself) are within a certain distance to an object with a certain LayerMask value.
		hittingWall = Physics2D.OverlapCircle (wallCheck.position, checkRadius, whatIsWall);
		notAtEdge = Physics2D.OverlapCircle (edgeCheck.position, checkRadius, whatIsWall);
		hittingEnemy = Physics2D.OverlapCircle (enemyCheck.position, checkRadius, whatIsEnemy);
		hittingBorder = Physics2D.OverlapCircle (borderCheck.position, checkRadius, whatIsBorder);

		//If the slime is about to go into a wall, another enemy, a border Tile or about to move off the Tile is on change direction.
		if (hittingWall || !notAtEdge || hittingEnemy || hittingBorder) {
			moveRight = !moveRight;
		}

		//Depending on the value of the bool move the Slime object in that direction by applying a force in said direction to the Slime's Rigidbody2D object.
		if (moveRight) {
			transform.localScale = new Vector3 (-2f, 2f, 1f);
			GetComponent<Rigidbody2D> ().velocity = new Vector2 (moveSpeed, GetComponent<Rigidbody2D> ().velocity.y);
		} else {
			transform.localScale = new Vector3 (2f, 2f, 1f);
			GetComponent<Rigidbody2D> ().velocity = new Vector2 (-moveSpeed, GetComponent<Rigidbody2D> ().velocity.y);
		}

		//Reset the object's rotation to the following default value to keep the object standing upright.
		Quaternion temp = new Quaternion (0f, 0f, 0f, 0f);
		GetComponent<Transform> ().rotation = temp;

	}
}
