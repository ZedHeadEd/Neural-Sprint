//This script controlls the functionality of the buttons, what they do and loading the correct scene when doing so for the Scene that asks the user whether to run the neural Network from a new pool or an existing one, or go back to the previous Scene.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public class MainMenuManager : MonoBehaviour
{

	//Game objects for the buttons which have been assigned via the Unity Editor.
	public GameObject NeuralPlayButton;
	public GameObject FreePlayButton;
	public GameObject NeuralPlayInsButton;
	public GameObject FreePlayInsButton;
	public GameObject WhatIsNeuralPlayButton;

	//Runs before any runs start and set frame rate to 60fps.
	void Awake() {
		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = 60;
	}

	// Use this for initialization
	void Start ()
	{
		//On Scene creation tell the buttons to perform a method if pressed while the Scene is loaded.
		NeuralPlayButton.GetComponent<Button> ().onClick.AddListener (neuralButtonOnClick);
		FreePlayButton.GetComponent<Button> ().onClick.AddListener (freeButtonOnClick);
		WhatIsNeuralPlayButton.GetComponent<Button> ().onClick.AddListener (whatIsButtonOnClick);
		NeuralPlayInsButton.GetComponent<Button> ().onClick.AddListener (neuralInsButtonOnClick);
		FreePlayInsButton.GetComponent<Button> ().onClick.AddListener (freeInsButtonOnClick);
	}
	
	// Update is called once per frame
	void Update ()
	{

	}

	//A method that modifies the value in the config file to tell the Level Manager to run the program in Neural Mode and to actively run the Neural Network.
	void neuralButtonOnClick ()
	{
		//Check format of config file.
		if (configCorrectFormat () == 0) {

			//Read in all of the lines for the file, change the important value and write it back to the same file.
			string[] lines = (File.ReadAllLines ("InputValues.cfg"));
			lines [0] = "1";
			File.WriteAllLines ("InputValues.cfg", lines);

			//Load the load new pool Scene.
			SceneManager.LoadScene ("LoadNewNeuralScene", LoadSceneMode.Single);

		} else if(configCorrectFormat () == 1) {
			SceneManager.LoadScene ("IncorrectFormatMenu", LoadSceneMode.Single);
		} else {
			SceneManager.LoadScene ("NoConfigMenu", LoadSceneMode.Single);
		}
	}

	//A method that modifies the value in the config file to tell the Level Manager to not run the program in Neural Mode and Free Mode instead and to not actively run the Neural Network.
	void freeButtonOnClick ()
	{
		//Check format of config file.
		if (configCorrectFormat () == 0) {

			//Read in all of the lines for the file, change the important value and write it back to the same file.
			string[] lines = (File.ReadAllLines ("InputValues.cfg"));
			lines [0] = "0";
			File.WriteAllLines ("InputValues.cfg", lines);

			//Load the main Scene.
			SceneManager.LoadScene ("MainScene", LoadSceneMode.Single);

		} else if(configCorrectFormat () == 1) {
			SceneManager.LoadScene ("IncorrectFormatMenu", LoadSceneMode.Single);
		} else {
			SceneManager.LoadScene ("NoConfigMenu", LoadSceneMode.Single);
		}
			
	}

	//A Method to load the Scene containing the informnation on the program itself.
	void whatIsButtonOnClick ()
	{

		//Load the Scene.
		SceneManager.LoadScene ("WhatIsNeuralSprintScene", LoadSceneMode.Single);
	}

	//A Method to load the Scene containing the informnation on the Neural Mode.
	void neuralInsButtonOnClick ()
	{

		//Load the Scene.
		SceneManager.LoadScene ("NeuralPlayInsSceneA", LoadSceneMode.Single);
	}

	//A Method to load the Scene containing the informnation on the Free Mode.
	void freeInsButtonOnClick ()
	{

		//Load the Scene.
		SceneManager.LoadScene ("FreePlayInsScene", LoadSceneMode.Single);
	}

	//A method to see if the config file has been formatted correctly.
	int configCorrectFormat(){

		//Define the variables that will hold the data that will be read in line by line.
		string lineIn;
		string[] lineInParts;

		//Seeing if the dimensions are correct.
		int dimY = 0;
		int dimX = 0;
		int currentX = 0;
		int currentY = 0;
		int tempInt = 0;

		//Seeing if the number of players and enemies are valid.
		int numPlayers = 0;
		int numEnemies = 0;

		//If the file exists:
		if (File.Exists (Directory.GetCurrentDirectory () + "\\InputValues.cfg")) {

			using (TextReader reader = File.OpenText ("InputValues.cfg")) {

				//For the following values of 1 or 0 are correct.
				lineIn = reader.ReadLine ();
				lineInParts = lineIn.Split (' ');
				if(!(lineInParts[0].CompareTo("0") == 0 || lineInParts[0].CompareTo("1") == 0)){
					return 1;
				}
				lineIn = reader.ReadLine ();
				lineInParts = lineIn.Split (' ');
				if(!(lineInParts[0].CompareTo("0") == 0 || lineInParts[0].CompareTo("1") == 0)){
					return 1;
				}
				lineIn = reader.ReadLine ();
				lineInParts = lineIn.Split (' ');
				if(!(lineInParts[0].CompareTo("0") == 0 || lineInParts[0].CompareTo("1") == 0)){
					return 1;
				}

				//Get the defined level dimensions.
				lineIn = reader.ReadLine ();
				lineInParts = lineIn.Split (' ');
				if(!int.TryParse(lineInParts[0], out dimY)){
					return 1;
				}
				//dimY = int.Parse (lineInParts[0]);

				lineIn = reader.ReadLine ();
				lineInParts = lineIn.Split (' ');
				if(!int.TryParse(lineInParts[0], out dimX)){
					return 1;
				}
				//dimX = int.Parse (lineInParts[0]);

				////---

				//While there is still more to read from the file
				while(reader.Peek() != -1){

					//Keep track 
					lineIn = reader.ReadLine ();
					lineInParts = lineIn.Split (' ');
					currentY = currentY + 1;
					currentX = lineInParts.Length;

					//Check each value of the X dimension
					for(int counter = 0; counter < lineInParts.Length; counter++){

						//Check to see if the character is a number.
						if(!int.TryParse(lineInParts[counter], out tempInt)){
							return 1;
						}

						if (int.Parse (lineInParts[counter]) < 0 || int.Parse (lineInParts[counter]) > 6) {
							//Incorrect levelTile value.
							return 1;
						} else if (int.Parse (lineInParts[counter]) == 2) {
							//Keeping track of the number of player objects.
							numPlayers = numPlayers + 1;
						} else if (int.Parse (lineInParts[counter]) == 3){
							//Keeping track of the number of enemy objects.
							numEnemies = numEnemies + 1;
						}
					}

					if(currentX != dimX){
						//Incorrect X dimension found value.
						return 1;
					}
				}

				if(currentY != dimY){
					//Incorrect Y dimension found value.
					return 1;
				}

				if(numPlayers != 1){
					//Exactly 1 player should have been found.
					return 1;
				}

				if(numEnemies > 50){
					//A max of 50 enemies is allowed.
					return 1;
				}

			}

			//The config file is correctly formated.
			return 0;

		} else {

			//File does not exist return to main menu and create a dummy config file that is correctly formatted.
			using (TextWriter writer = File.CreateText ("InputValues.cfg")) {
				writer.WriteLine ("0");
				writer.WriteLine ("0");
				writer.WriteLine ("0");
				writer.WriteLine ("5");
				writer.WriteLine ("10");
				writer.WriteLine ("0 3 0 0 0 0 0 0 0 0");
				writer.WriteLine ("0 1 1 0 0 0 0 0 0 0");
				writer.WriteLine ("0 0 0 0 0 1 1 1 0 0");
				writer.WriteLine ("0 2 0 0 0 0 0 0 0 0");
				writer.WriteLine ("6 1 1 1 1 1 1 1 1 1");
				writer.Close ();
			}

			//Load the correct scene to inform the user.
			return 2;
		}
	}
}
