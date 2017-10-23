//This is the script for the level manager (LM) for the main scene of the application and by extension controls just about everything that happens within the application.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{

	//Variables that determine the state of the LM and what it does.
	public bool savePool = false;
	public bool playTop = false;
	public bool playChampion = false;
	public bool neuralPlay = false;
	public bool NNRestartLevel = false;
	public bool debug = false;
	public bool playerDied = false;
	public bool[] buttonsBeingPressed = new bool[6];

	//Varoiables for the creation of the level itself.
	public int levelDimY = -1;
	public int levelDimX = -1;
	public int loadPool = 0;
	int[,] levelArray = new int [1, 1];
		
	/*
	 * 0 = Background Tile.
	 * 1 = Platform (Generic)
	 * 2 = Player Spawn + Empty Tile
	 * 3 = Enemy Spawn + Empty Tile.
	 * 4 = Edge of level tile, like platform but primarially used for padding.
	 * 5 = EmptyTile, used for padding.
	 * 6 = Death Tile, kills on contact.
	 */

	//Prefabs used.
	public GameObject ground_tile;
	public GameObject border_tile;
	public GameObject empty_tile;
	public GameObject background_tile;
	public GameObject death_tile;
	public GameObject Player;
	public GameObject Enemy;
	public GameObject NeuralNetworkPrefab;
	public GameObject TilesPreFab;

	//Variables used for UpdatePlayerPosition()
	public float tempY1;
	public float tempY2;
	public float tempX1;
	public float tempX2;
	public int pastX;
	public int pastY;
	public int currentX;
	public int currentY;
	public int startingX;
	public int startingY;
	public int endOfLevelInX;
	Vector3 tempVec;

	//Array for the generated level tiles
	GameObject[,] tileArray = new GameObject[1, 1];
	GameObject[,] tileArrayExtra = new GameObject[1, 1];
	int[,] tempArray = new int[1, 1];

	//Array to store the spawned in enemies and to keep track how many enemies currently exist.
	GameObject[] enemyArray = new GameObject[50];

	//Variables used for UpdateEnemyPositions();
	int[] currentEnemyPosX = new int[50];
	int[] currentEnemyPosY = new int[50];
	int[] pastEnemyPosX = new int[50];
	int[] pastEnemyPosY = new int[50];
	public int enemiesGenerated = 0;

	//Temp Gameobject mostly used for generation method.
	GameObject gOTemp;

	//Reference to the players character after generation.
	GameObject PlayerCharacter;

	//Reference to the Neural Network once created.
	GameObject NeuralNetwork0;

	//Reference to the Tiles object once created.
	GameObject Tiles;

	// Use this for initialization
	void Start ()
	{
		//Read in all of the values from the config file.
		readConfig ();

		//Pad the incoming level, this does take some time.
		padlevel ();

		//Create the Tiles pbject to make a parent of all tile objects.
		Tiles = Instantiate (TilesPreFab, new Vector3 (0, 0, 0), Quaternion.identity);

		//Call the generate level method.
		Generatelevel ();

		//Tell the NN whether it is running or not.
		if (neuralPlay) {
			//Generate the NN and assign it to NeuralNetwork0.
			NeuralNetwork0 = Instantiate (NeuralNetworkPrefab, new Vector3 (0, 0, 0), Quaternion.identity);
			NeuralNetwork0.GetComponent<NeuralNetwork> ().runningNN = true;
		} else {
			NeuralNetwork0 = Instantiate (NeuralNetworkPrefab, new Vector3 (0, 0, 0), Quaternion.identity);
			NeuralNetwork0.GetComponent<NeuralNetwork> ().runningNN = false;
		}

		//If the NN is running; tell it whether it should a previously saved pool or start fresh.
		if (neuralPlay && loadPool == 0) {
			NeuralNetwork0.GetComponent<NeuralNetwork> ().initializePool ();
		} else if (neuralPlay && loadPool == 1) {
			NeuralNetwork0.GetComponent<NeuralNetwork> ().readInPool ();
		}

	}

	// Update is called once per frame
	void Update ()
	{

		//If the user presses the Esc key return to the main Menu.
		if (Input.GetKeyDown (KeyCode.Escape)) {
			SceneManager.LoadScene ("MainMenu", LoadSceneMode.Single);
		}

		//If the user pressed the 'R' key call the restart level method.
		if (Input.GetKeyDown (KeyCode.R)) {
			RestartLevel ();
		}

		//Call this method to update the players position.
		UpdatePlayerPosition ();

		//Call this method to update the enemy positions.
		UpdateEnemyPositions ();

		//If the NN is running:
		if (neuralPlay) {
			
			//Send inputs to the NN, The players position and the level itself.
			NeuralNetwork0.GetComponent<NeuralNetwork> ().sendInputs (tileArray, currentX, currentY);

			//If the NN is starting a new run tell it the players starting position for fitness evaluation.
			if (NeuralNetwork0.GetComponent<NeuralNetwork> ().startOfRunPos == true) {
				NeuralNetwork0.GetComponent<NeuralNetwork> ().getPlayerStartingPos (PlayerCharacter.GetComponent<Transform> ().position.x, PlayerCharacter.GetComponent<Transform> ().position.y);
			}

			//If the user presses the T key tell the NN to play the Pools's current top Genome.
			if (Input.GetKeyDown (KeyCode.T)) {
				playTop = true;
			}
			if (playTop) {
				NeuralNetwork0.GetComponent<NeuralNetwork> ().nowPlayTop ();
				playTop = false;
			}

			//If the user presses the Y key tell the NN to play the Pools's champion Genome.
			if ((Input.GetKeyDown (KeyCode.Y))) {
				playChampion = true;
			}
			if (playChampion) {
				NeuralNetwork0.GetComponent<NeuralNetwork> ().nowPlayChampion ();
				playChampion = false;
			}
				
			//If the user presses the U key tell the NN manually save the Pool in it's current state.
			if ((Input.GetKeyDown (KeyCode.U))) {
				savePool = true;
			}
			if (savePool) {
				NeuralNetwork0.GetComponent<NeuralNetwork> ().savePool ();
				savePool = false;
			}

			//If the player has died inform the NN that he has. Reset the value after informing it.
			if (playerDied) {
				NeuralNetwork0.GetComponent<NeuralNetwork> ().playerDeath (playerDied);
				playerDied = false;
			}

			//If the player has reached the end of the level inform the NN that he has.
			if (endOfLevelInX == currentX) {
				NeuralNetwork0.GetComponent<NeuralNetwork> ().endOfLevelReached ();
			}

			//Tell the NN where the player is currently.
			NeuralNetwork0.GetComponent<NeuralNetwork> ().getPlayerCurrentPos (PlayerCharacter.GetComponent<Transform> ().position.x, PlayerCharacter.GetComponent<Transform> ().position.y);

			//Tell the NN to perform it's main function and it's main Loop.
			NeuralNetwork0.GetComponent<NeuralNetwork> ().mainLoop ();

			//Getting information from the NN.
			NNRestartLevel = NeuralNetwork0.GetComponent<NeuralNetwork> ().getRestartOutputs ();
			buttonsBeingPressed = NeuralNetwork0.GetComponent<NeuralNetwork> ().getButtonOutputs ();
			PlayerCharacter.GetComponent<PlayerController> ().getButtonsPressedPl (buttonsBeingPressed);
				
		} else {

			//If the player died restart the level.
			if (playerDied) {
				RestartLevel ();
				playerDied = false;
			}

			//If the player has reached the end of the level restart thr level.
			if (endOfLevelInX == currentX) {
				RestartLevel ();
			}
		}

		//NN telling the LM to restart the level.
		if (neuralPlay && NNRestartLevel) {
			RestartLevel ();
			NNRestartLevel = false;
			NeuralNetwork0.GetComponent<NeuralNetwork> ().restartLMLevel = false;
		}
	}
		
	//A method that sets the state of the player to dead.
	public void playerHasDied ()
	{
		playerDied = true;
	}

	//Method to read in all required values from the config file.
	void readConfig ()
	{
		//An array to read in the values of the file of the level.
		int[,] readInLevel = new int[1, 1];

		//Use TextReader to read in the values of the config file.
		using (TextReader reader = File.OpenText ("InputValues.cfg")) {

			//Start in NeuralMode or FreeMode
			string lineIn = reader.ReadLine ();
			string[] lineInParts = lineIn.Split (' ');
			int tempInt = int.Parse (lineInParts [0]);

			if (tempInt == 0) {
				neuralPlay = false;
			} else {
				neuralPlay = true;
			}

			//Read in whether or not to load the previously saved pool or start fresh.
			lineIn = reader.ReadLine ();
			lineInParts = lineIn.Split (' ');
			loadPool = int.Parse (lineInParts [0]);

			//Read in whether or not to run in debug mode.
			lineIn = reader.ReadLine ();
			lineInParts = lineIn.Split (' ');
			tempInt = int.Parse (lineInParts [0]);

			if (tempInt == 0) {
				debug = false;
			} else {
				debug = true;
			}

			//Read in the level dimensions.
			lineIn = reader.ReadLine ();
			lineInParts = lineIn.Split (' ');
			levelDimY = int.Parse (lineInParts [0]);
			lineIn = reader.ReadLine ();
			lineInParts = lineIn.Split (' ');
			levelDimX = int.Parse (lineInParts [0]);

			//Create the array according to the dimensions.
			readInLevel = new int[levelDimY, levelDimX];

			//Fill in the array with values read in from file.
			for (int counter1 = 0; counter1 < levelDimY; counter1++) {
				lineIn = reader.ReadLine ();
				lineInParts = lineIn.Split (' ');
				for (int counter2 = 0; counter2 < levelDimX; counter2++) {
					readInLevel [counter1, counter2] = int.Parse (lineInParts [counter2]); 
				}
			}

			//Assign new values to proper places.
			levelArray = readInLevel;
		}
	}

	//This method shall pad the level out with extra tiles to prevent any possible out of bounds errors.
	void padlevel ()
	{
		//Getting the dimensions of the original array that will be converted.
		int arrayHeight = levelArray.GetLength (0);
		int arrayWidth = levelArray.GetLength (1);
		
		//Padding of white space
		int padding = 13;

		//Creating a temporary new array with the new dimensions.
		//Padding happens on both sides as does the 1 tile thick border wall.
		tempArray = new int[arrayHeight + (padding * 2) + 2, arrayWidth + (padding * 2) + 2];

		//Nested for loop which will go through every space in the new array and assign the correct number in each.
		for (int counter1 = 0; counter1 < tempArray.GetLength (0); counter1++) {
			for (int counter2 = 0; counter2 < tempArray.GetLength (1); counter2++) {
				
				if (counter1 < padding) {
					tempArray [counter1, counter2] = 5;
				} else if (counter1 > padding + arrayHeight + 1) {
					tempArray [counter1, counter2] = 5;
				} else if (counter1 == padding || counter1 == padding + arrayHeight + 1) {
					if (counter2 < padding || counter2 > padding + arrayWidth + 1) {
						tempArray [counter1, counter2] = 5;
					} else {
						tempArray [counter1, counter2] = 4;
					}
				} else {
					if (counter2 < padding || counter2 > padding + arrayWidth + 1) {
						tempArray [counter1, counter2] = 5;
					} else if (counter2 == padding || counter2 == padding + arrayWidth + 1) {
						tempArray [counter1, counter2] = 4;
					} else {
						tempArray [counter1, counter2] = levelArray [counter1 - padding - 1, counter2 - padding - 1];
					}
				}
			}
		}

		//Define the end of the level.
		endOfLevelInX = levelArray.GetLength (1) + padding;

		//Reassigning the new padded array back to levelArray.
		levelArray = tempArray;

		//Defining the dimensions of the tileArray to match the new ones of the levelArray.
		tileArray = new GameObject[tempArray.GetLength (0), tempArray.GetLength (1)];	
		tileArrayExtra = new GameObject[tempArray.GetLength (0), tempArray.GetLength (1)];
	}

	//Generates level from prefabs and levelArray information.
	void Generatelevel ()
	{
		//Counter1 for the Y position and Counter2 for the X position.
		for (int counter1 = 0; counter1 < tileArray.GetLength (0); counter1++) {
			for (int counter2 = 0; counter2 < tileArray.GetLength (1); counter2++) {

				//Based on the value from levelArray do one of the following:
				if (levelArray [counter1, counter2] == 0) {
					
					//Creates prefab pbject at counters location and temp stores it in a temp gameobject.
					gOTemp = Instantiate (background_tile, new Vector3 ((float)counter2, -(float)counter1, 0), Quaternion.identity);

					//Sets its parent object to a an empty object called Tiles, this is just for organisation while testing.
					gOTemp.transform.parent = Tiles.transform;

					//Sets the objects internal position to its placed position.
					gOTemp.GetComponent<TileController> ().posX = counter2;
					gOTemp.GetComponent<TileController> ().posY = counter1;
					gOTemp.GetComponent<TileController> ().debugTile = debug;

					//Sets that object within the tile array.
					tileArray [counter1, counter2] = gOTemp;

				} else if (levelArray [counter1, counter2] == 1) {
					
					gOTemp = Instantiate (ground_tile, new Vector3 ((float)counter2, -(float)counter1, 0), Quaternion.identity);
					gOTemp.transform.parent = Tiles.transform;
					gOTemp.GetComponent<TileController> ().posX = counter2;
					gOTemp.GetComponent<TileController> ().posY = counter1;
					gOTemp.GetComponent<TileController> ().debugTile = debug;

					//Sets the tiles internal setting for positive to true.
					gOTemp.GetComponent<TileController> ().positive = true;
					gOTemp.GetComponent<TileController> ().posConst = true;
					tileArray [counter1, counter2] = gOTemp;

				} else if (levelArray [counter1, counter2] == 2) {
					
					gOTemp = Instantiate (background_tile, new Vector3 ((float)counter2, -(float)counter1, 0), Quaternion.identity);
					gOTemp.transform.parent = Tiles.transform;
					gOTemp.GetComponent<TileController> ().posX = counter2;
					gOTemp.GetComponent<TileController> ().posY = counter1;
					gOTemp.GetComponent<TileController> ().debugTile = debug;
					tileArray [counter1, counter2] = gOTemp;

					//Creates the player at the counters location.
					PlayerCharacter = Instantiate (Player, new Vector3 ((float)counter2, -(float)counter1, -2), Quaternion.identity);
					PlayerCharacter.GetComponent<Transform> ().name = "Player";

					//The following is used for player tracking.
					pastX = counter2;
					pastY = counter1;
					currentX = counter2;
					currentY = counter1;
					startingX = counter2;
					startingY = counter1;

				} else if (levelArray [counter1, counter2] == 3) {
					
					gOTemp = Instantiate (background_tile, new Vector3 ((float)counter2, -(float)counter1, 0), Quaternion.identity);
					gOTemp.transform.parent = Tiles.transform;
					gOTemp.GetComponent<TileController> ().posX = counter2;
					gOTemp.GetComponent<TileController> ().posY = counter1;
					gOTemp.GetComponent<TileController> ().debugTile = debug;
					tileArray [counter1, counter2] = gOTemp;

					gOTemp = Instantiate (Enemy, new Vector3 ((float)counter2, -(float)counter1, -2), Quaternion.identity);
					enemyArray [enemiesGenerated] = gOTemp;

					//Used for tracking the generated enemy.
					pastEnemyPosX [enemiesGenerated] = counter2;
					pastEnemyPosY [enemiesGenerated] = counter1;
					currentEnemyPosX [enemiesGenerated] = counter2;
					currentEnemyPosY [enemiesGenerated] = counter1;

					//Update how many enemies currently exist.
					enemiesGenerated++;

				} else if (levelArray [counter1, counter2] == 4) {
					
					gOTemp = Instantiate (border_tile, new Vector3 ((float)counter2, -(float)counter1, 0), Quaternion.identity);
					gOTemp.transform.parent = Tiles.transform;
					gOTemp.GetComponent<TileController> ().posX = counter2;
					gOTemp.GetComponent<TileController> ().posY = counter1;
					gOTemp.GetComponent<TileController> ().debugTile = debug;
					tileArray [counter1, counter2] = gOTemp;

				} else if (levelArray [counter1, counter2] == 5) {
					
					gOTemp = Instantiate (empty_tile, new Vector3 ((float)counter2, -(float)counter1, 0), Quaternion.identity);
					gOTemp.transform.parent = Tiles.transform;
					gOTemp.GetComponent<TileController> ().posX = counter2;
					gOTemp.GetComponent<TileController> ().posY = counter1;
					gOTemp.GetComponent<TileController> ().debugTile = debug;

					if (debug) {
						gOTemp.GetComponent<SpriteRenderer> ().enabled = true;
					}
					tileArray [counter1, counter2] = gOTemp;

				} else if (levelArray [counter1, counter2] == 6) {
					
					gOTemp = Instantiate (death_tile, new Vector3 ((float)counter2, -(float)counter1, 0), Quaternion.identity);
					gOTemp.transform.parent = Tiles.transform;
					gOTemp.GetComponent<TileController> ().posX = counter2;
					gOTemp.GetComponent<TileController> ().posY = counter1;
					gOTemp.GetComponent<TileController> ().negative = true;
					gOTemp.GetComponent<TileController> ().negConst = true;
					tileArray [counter1, counter2] = gOTemp;

					//Creating a background tile to behind the DeathTile.
					gOTemp = Instantiate (background_tile, new Vector3 ((float)counter2, -(float)counter1, 2), Quaternion.identity);
					gOTemp.transform.parent = Tiles.transform;
					gOTemp.GetComponent<TileController> ().posX = counter2;
					gOTemp.GetComponent<TileController> ().posY = counter1;
					gOTemp.GetComponent<TileController> ().debugTile = debug;
					tileArrayExtra [counter1, counter2] = gOTemp;
				}
			}
		}
	}

	//Keeps track of the player's current position within the level array boundries.
	void UpdatePlayerPosition ()
	{
		//Storing the players position in a temp Vector3.
		tempVec = PlayerCharacter.GetComponent<Transform> ().position;

		//Inverting Y value to be positive as 0,0 starts top left not bottom left.
		tempVec.y = tempVec.y * -1;

		//Rounding off the players X and Y position into Ints to be used within the int array.
		tempY1 = tempVec.y % 1;
		tempY2 = tempVec.y / 1;
		if (tempY1 > 0.5) {
			tempY1 = 1;
		} else {
			tempY1 = 0;
		}
		tempVec.y = tempY1 + tempY2;

		tempX1 = tempVec.x % 1;
		tempX2 = tempVec.x / 1;
		if (tempX1 > 0.5) {
			tempX1 = 1;
		} else {
			tempX1 = 0;
		}
		tempVec.x = tempX1 + tempX2;

		//A rare occuring bug exists that the length of both deminsions of the TileArray are of length 1 which causes an out of bounds error.
		if ((int)tempVec.y < 0 || (int)tempVec.y >= tileArray.GetLength (0) || (int)tempVec.x < 0 || (int)tempVec.x >= tileArray.GetLength (1)) {
			Generatelevel ();
			RestartLevel ();
		} else {
			
			//Setting the tile that the player is on or closest to as being occupied.
			tileArray [(int)tempVec.y, (int)tempVec.x].GetComponent<TileController> ().playerOccupied = true;

			//For ease of use while coding storing these ints into variables more easily read at a glance.
			currentX = (int)tempVec.x;
			currentY = (int)tempVec.y;

			//Checking to see if the players occupied tile has changed and if it has, update the tiles playerOccupied value and the players past position to its current one.
			if (!(currentX == pastX && currentY == pastY)) {
				tileArray [pastY, pastX].GetComponent<TileController> ().playerOccupied = false;
				pastX = currentX;
				pastY = currentY;
			}
		}
	}

	//Keeps track of the enemies's current position within the level array boundries.
	void UpdateEnemyPositions ()
	{
		//Update each enemy position individually.
		for (int counter = 0; counter < enemiesGenerated; counter++) {

			//Storing the enemies position in a temp Vector3.
			tempVec = enemyArray [counter].GetComponent<Transform> ().position;

			//Inverting Y value to be positive as 0,0 starts top left not bottom left.
			tempVec.y = tempVec.y * -1;

			//Rounding off the players X and Y position into Ints to be used within the int array.
			tempY1 = tempVec.y % 1;
			tempY2 = tempVec.y / 1;
			if (tempY1 > 0.5) {
				tempY1 = 1;
			} else {
				tempY1 = 0;
			}
			tempVec.y = tempY1 + tempY2;

			tempX1 = tempVec.x % 1;
			tempX2 = tempVec.x / 1;
			if (tempX1 > 0.5) {
				tempX1 = 1;
			} else {
				tempX1 = 0;
			}
			tempVec.x = tempX1 + tempX2;

			//A rare occuring bug exists that the length of both deminsions of the TileArray are of length 1 which causes an out of bounds error.
			if ((int)tempVec.y < 0 || (int)tempVec.y >= tileArray.GetLength (0) || (int)tempVec.x < 0 || (int)tempVec.x >= tileArray.GetLength (1)) {
				Generatelevel ();
				RestartLevel ();
			} else {
				
				//Setting the tile that the enemy is on or closest to as being occupied.
				tileArray [(int)tempVec.y, (int)tempVec.x].GetComponent<TileController> ().dangerPresent = true;

				//Updating internal current enemy position.
				currentEnemyPosX [counter] = (int)tempVec.x;
				currentEnemyPosY [counter] = (int)tempVec.y;

				//Checking to see if the enemys occupied tile has changed and if it has, update the tiles dangerPresent value and the enemys past position to its current one.
				if (!(currentEnemyPosX [counter] == pastEnemyPosX [counter] && currentEnemyPosY [counter] == pastEnemyPosY [counter])) {
					tileArray [pastEnemyPosY [counter], pastEnemyPosX [counter]].GetComponent<TileController> ().dangerPresent = false;
					pastEnemyPosX [counter] = currentEnemyPosX [counter];
					pastEnemyPosY [counter] = currentEnemyPosY [counter];
				}
			}
		}
	}

	//Restarts the level by destroying all objects and running GenerateLevel() again.
	void RestartLevel ()
	{
		//Destroy the player's object.
		Destroy (PlayerCharacter);

		//Go through the enemyArray and destroy each object.
		for (int counter = 0; counter < enemiesGenerated; counter++) {
			Destroy (enemyArray [counter]);
			enemyArray [counter] = null;
		}

		//Sets all of the tiles that the enemies are currently on and set their dangerPresent value to false.
		for (int counter = 0; counter < enemiesGenerated; counter++) {
			tileArray [currentEnemyPosY [counter], currentEnemyPosX [counter]].GetComponent<TileController> ().dangerPresent = false;
		}

		//Resets the enemies generated counter.
		enemiesGenerated = 0;
		enemyArray = null;
		enemyArray = new GameObject[50];

		//Sets the player's last occupied tile's playerOccupied to false.
		tileArray [currentY, currentX].GetComponent<TileController> ().playerOccupied = false;

		//Set the playerCharacter object to null.
		PlayerCharacter = null;

		//Go through the tileArray and destroy and re-create certain objects.
		for (int counter1 = 0; counter1 < tileArray.GetLength (0); counter1++) {
			for (int counter2 = 0; counter2 < tileArray.GetLength (1); counter2++) {

				//Destroy the player's object and tile and re-create them.
				//Else destroy an enemy's object and tile and re-create them.
				if (levelArray [counter1, counter2] == 2) {

					Destroy (tileArray [counter1, counter2]);
					tileArray [counter1, counter2] = null;
					gOTemp = Instantiate (background_tile, new Vector3 ((float)counter2, -(float)counter1, 0), Quaternion.identity);
					gOTemp.transform.parent = Tiles.transform;
					gOTemp.GetComponent<TileController> ().posX = counter2;
					gOTemp.GetComponent<TileController> ().posY = counter1;
					tileArray [counter1, counter2] = gOTemp;

					//Creates the player at the counters location.
					PlayerCharacter = Instantiate (Player, new Vector3 ((float)counter2, -(float)counter1, -2), Quaternion.identity);
					PlayerCharacter.GetComponent<Transform> ().name = "Player";

					//The following is used for player tracking.
					pastX = counter2;
					pastY = counter1;
					currentX = counter2;
					currentY = counter1;
					startingX = counter2;
					startingY = counter1;

				} else if (levelArray [counter1, counter2] == 3) {

					Destroy (tileArray [counter1, counter2]);
					tileArray [counter1, counter2] = null;
					gOTemp = Instantiate (background_tile, new Vector3 ((float)counter2, -(float)counter1, 0), Quaternion.identity);
					gOTemp.transform.parent = Tiles.transform;
					gOTemp.GetComponent<TileController> ().posX = counter2;
					gOTemp.GetComponent<TileController> ().posY = counter1;
					tileArray [counter1, counter2] = gOTemp;

					gOTemp = Instantiate (Enemy, new Vector3 ((float)counter2, -(float)counter1, -2), Quaternion.identity);
					enemyArray [enemiesGenerated] = gOTemp;

					//Used for tracking the generated enemy.
					pastEnemyPosX [enemiesGenerated] = counter2;
					pastEnemyPosY [enemiesGenerated] = counter1;
					currentEnemyPosX [enemiesGenerated] = counter2;
					currentEnemyPosY [enemiesGenerated] = counter1;

					//Update how many enemies currently exist.
					enemiesGenerated++;
				}
			}
		}

		//Free up all unused unity objects from memory.
		Resources.UnloadUnusedAssets ();
	}
}
