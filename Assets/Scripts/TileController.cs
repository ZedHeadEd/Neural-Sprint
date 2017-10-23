//This script is attached to all of the Tile objects within the program regardless of which object or prefab it is attached to.
//It contains variables for the Tiles conditional statements and a method to manually set the internal position of the tile as it exists in the array it resides in in the Level Manager.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileController : MonoBehaviour
{

	//Variables the tile contains such as position, whether it is a neutral, positive or negative Tile and if an enemy or player occupies that tile.
	public bool positive = false;
	public bool negative = false;
	public bool dangerPresent = false;
	public bool playerOccupied = false;
	public bool posConst = false;
	public bool negConst = false;
	public bool debugTile = false;

	//Used for early development testing.
	public int posX = -1;
	public int posY = -1;

	// Use this for initialization
	void Start ()
	{
		
	}
	
	// Update is called once per frame
	void Update ()
	{

		//If a dangerous element such as an enemy occupies this tile update this tile as Negative, if not, return to previous state.
		if (dangerPresent) {
			negative = true;
		} else {
			negative = negConst;
		}
			
		//For debugging purposes, if the player or an enemy occupies this tile then update the revelant variable accordingly.
		if (debugTile) {
			if (playerOccupied || dangerPresent) {
				GetComponent<SpriteRenderer> ().enabled = false;
			} else {
				GetComponent<SpriteRenderer> ().enabled = true;
			}
		}
		/*
		//If the player occupies the tile update positice to true and if not update to what it was previously.
		if (playerOccupied) {
			positive = true;
		} else {
			positive = posConst;
		}
		*/
	}

	//A method that can called to internally set the tile position.
	public void setPos (int x, int y)
	{
		posX = x;
		posY = y;
	}
}
