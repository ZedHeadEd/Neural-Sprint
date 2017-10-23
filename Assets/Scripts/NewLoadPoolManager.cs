//This script controlls the functionality of the buttons, what they do and loading the correct scene when doing so for the Scene that asks the user whether to run the neural Network from a new pool or an existing one, or go back to the previous Scene.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public class NewLoadPoolManager : MonoBehaviour
{
	//Game objects for the buttons which have been assigned via the Unity Editor.
	public GameObject LoadPoolButton;
	public GameObject NewPoolButton;
	public GameObject BackButton;

	// Use this for initialization
	void Start ()
	{
		//On Scene creation tell the buttons to perform a method if pressed while the Scene is loaded.
		LoadPoolButton.GetComponent<Button> ().onClick.AddListener (loadButtonOnClick);
		NewPoolButton.GetComponent<Button> ().onClick.AddListener (newButtonOnClick);
		BackButton.GetComponent<Button> ().onClick.AddListener (backButtonOnClick);
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}

	//A method that modifies the value in the config file to tell the Neural Network to use and generate a new pool once generated.
	void newButtonOnClick ()
	{
		//Read in all of the lines for the file, change the important value and write it back to the same file.
		string[] lines = (File.ReadAllLines ("InputValues.cfg"));
		lines [1] = "0";
		File.WriteAllLines ("InputValues.cfg", lines);

		//Load the main Scene.
		SceneManager.LoadScene ("MainScene", LoadSceneMode.Single);
	}

	//A method that modifies the value in the config file to tell the Neural Network to use the pre-existing pool (if none exists make a new once via newButtonOnClick()) once generated.
	void loadButtonOnClick ()
	{

		//Check to see if a pool alreadly exists, if it does, tell the Neural Network to load it and if not run newButtonOnClick() to make one.
		if (File.Exists (Directory.GetCurrentDirectory () + "\\savedPool.pool")) {
			
			//Read in all of the lines for the file, change the important value and write it back to the same file.
			string[] lines = (File.ReadAllLines ("InputValues.cfg"));
			lines [1] = "1";
			File.WriteAllLines ("InputValues.cfg", lines);

			//Load the main Scene.
			SceneManager.LoadScene ("MainScene", LoadSceneMode.Single);
		} else {
			newButtonOnClick ();
		}
	}

	//A method to return to the previous Scene.
	void backButtonOnClick ()
	{
		//Load the previous Scene which is the MainScene.
		SceneManager.LoadScene ("MainMenu", LoadSceneMode.Single);
	}
}