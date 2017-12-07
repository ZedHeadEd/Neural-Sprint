//This script controlls the functionality of the button, what it does and loading the correct scene when doing so.
//This script is for the instructional pages which contain revelant information and a next button to go to the next scene.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class InsSceneManagerNB : MonoBehaviour{
	//Game object for the button which have been assigned via the Unity Editor.
	public GameObject BackButton;

	// Use this for initialization
	void Start (){
		//On Scene creation tell the button to perform a method if pressed while the Scene is loaded.
		BackButton.GetComponent<Button> ().onClick.AddListener (nextButtonOnClick);
	}
	
	// Update is called once per frame
	void Update (){
		
	}

	//A method to return to the previous Scene.
	void nextButtonOnClick (){
		//Load the previous Scene which is the MainScene.
		SceneManager.LoadScene ("NeuralPlayInsSceneC", LoadSceneMode.Single);
	}
}
