//A script attached to objects that have a triggerable 2D collider that when collides into the player object in the LM will tell the player has died.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class KillPlayerOnContact : MonoBehaviour
{

	//A reference to the LM object in the scene.
	public LevelManager LM;

	// Use this for initialization
	void Start ()
	{
		//Find the LM object in the scene and assign it to the local LM.
		LM = FindObjectOfType<LevelManager> ();
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}

	//If the trigger collider hits another collider do the following:
	void OnTriggerEnter2D (Collider2D other)
	{
		//If the other collider belongs to an object with the name of "Player", tell the LM the player died.
		if (other.name == "Player") {
			LM.playerHasDied ();
		}
	}
}


