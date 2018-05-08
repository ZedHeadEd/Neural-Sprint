
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class NeuralNetwork : MonoBehaviour
{
	//Variables for the receiving and sending of information to and from the NN and LM.
	int[,] inputs = new int[11, 11];
	int inputDimensions = 11;
	int inputSize = 11 * 11;
	bool[] buttonsPressed = new bool[6];
	int outputSize = 6;

	//Variables that the NN uses to control the UI.
	public GameObject UIPrefab;
	public GameObject NodePrefab;
	public GameObject GenePrefab;
	public GameObject NodesPrefab;
	public GameObject GenesPrefab;
	public GameObject[] NodeArray = new GameObject[1];
	public GameObject[] GeneArray = new GameObject[1];
	public GameObject tempGOGO;
	public Color32 whiteTrans2 = new Color32 (255, 255, 255, 30);
	public Color32 whiteTrans = new Color32 (255, 255, 255, 100);
	public Color32 white = new Color32 (255, 255, 255, 255);
	public Color32 blackTrans = new Color32 (0, 0, 0, 100);
	public Color32 black = new Color32 (0, 0, 0, 255);
	public Color32 redTrans = new Color32 (255, 0, 0, 100);
	public Color32 red = new Color32 (255, 0, 0, 255);
	public Color32 greenTrans = new Color32 (0, 255, 0, 100);
	public Color32 green = new Color32 (0, 255, 0, 255);
	public Color32 blueTrans = new Color32 (0, 255, 255, 100);
	public Color32 blue = new Color32 (0, 255, 255, 255);
	public bool restartGenomeDisplay = true;
	GameObject Nodes;
	GameObject Genes;
	GameObject UI;

	//Used for the UI and as a variable for the reading in from a saved pool.
	public int geneCount = 0;

	//For fitness evaluation
	public float playerProgress = 0f;
	public float timeoutBonus = 0f;
	public float timeout = 0f;
	public float playerStartingX = 0f;
	public float playerStartingY = 0f;
	public float playerCurrentX = 0f;
	public float playerCurrentY = 0f;
	public int tempFitness = 0;
	public bool reTestChampionGenome = false;

	//This Pool will be main pool of the entire NN.
	public Pool mainPool;

	//Bools that decide the state opf the NN and what it does at any given time.
	public bool runningNN = false;
	public bool restartLMLevel = false;
	public bool playerHasDied = false;
	public bool endOfLevel = false;
	public bool playTop = false;
	public bool playTopRevert = false;
	public bool playChampion = false;
	public bool playChampionRevert = false;
	public bool startOfRunPos = false;

	//Global Variables to be used in other parts of the program.
	public float deltaDisjoint = 1f;
	public float deltaWeight = 0.3f;
	public float deltaExcess = 0.8f;
	public float deltaThreshold = 4f;
	public float population = 300;
	public float staleSpecies = 15f;
	public float timeoutConst = 30f;
	public float crossoverChance = 0.75f;
	public float perturbChance = 0.80f;
	public float fitnessMultiplier = 100f;
	public int maxNodes = 780;

	// Use this for initialization
	void Start ()
	{

		//If the game is running in Neural Mode then generate the UI.
		if (runningNN) {
			generateDisplay ();
		}
	}

	// Update is called once per frame
	void Update ()
	{

	}

	//A method to be called to tell the NN that the player has died.
	public void playerDeath (bool hasHeDied)
	{
		playerHasDied = hasHeDied;
	}

	//A method to be called to tell the NN that the player has reached the end of the stage.
	public void endOfLevelReached ()
	{
		endOfLevel = true;
	}

	//A method to be called to tell the NN what the player's position was when the level/run was started.
	public void getPlayerStartingPos (float incomingX, float incomingY)
	{
		playerStartingX = incomingX;
		playerStartingY = incomingY;
	}

	//A method to be called to tell the NN what the player's position at that point in time.
	public void getPlayerCurrentPos (float incomingX, float incomingY)
	{
		playerCurrentX = incomingX;
		playerCurrentY = incomingY;
	}

	//A method to be called to tell the LM what buttons the NN is pressing.
	public bool[] getButtonOutputs ()
	{
		return buttonsPressed;
	}

	//A method to be called to tell the LM if the level should be restarted.
	public bool getRestartOutputs ()
	{
		return restartLMLevel;
	}

	//A method that generates the UI display.
	public void generateDisplay ()
	{

		//Generating the default UI prefab and the empty Nodes object that the new nodes will be allocated to for tidyness.
		UI = Instantiate (UIPrefab, new Vector3 (0, 0, 0), Quaternion.identity);
		Nodes = Instantiate (NodesPrefab, new Vector3 (0, 0, 0), Quaternion.identity);

		Nodes.transform.parent = UI.transform;
		Genes = Instantiate (GenesPrefab, new Vector3 (0, 0, 0), Quaternion.identity);
		Genes.transform.parent = UI.transform;

		//A variable that descerns how far apart the node object should be away from each other.
		int distanceBetweenNodes = 15;

		//Make the array the correct size.
		NodeArray = new GameObject[maxNodes + outputSize];

		//A temporary variable to keep track of which node in the array is being used.
		int nodeNum = 0;

		//Generate the input neurons and make them childs of Nodes.
		for (int counter1 = 0; counter1 < inputDimensions; counter1++) {
			for (int counter2 = 0; counter2 < inputDimensions; counter2++) {

				//Generate the Node object, make it a child of Nodes, assign it to the array and increment NodeNum.
				tempGOGO = Instantiate (NodePrefab, new Vector3 (0,0,0), Quaternion.identity);
				tempGOGO.transform.position = new Vector3 (((float)counter2 * distanceBetweenNodes) - 595f, -((float)counter1 * distanceBetweenNodes) + 295f, 0);

				Debug.Log ("Generated node at:" + (((float)counter2 * distanceBetweenNodes) - 595f) + " " + -(((float)counter1 * distanceBetweenNodes) + 295f));
				tempGOGO.transform.SetParent (Nodes.transform);
				NodeArray [nodeNum] = tempGOGO;
				nodeNum = nodeNum + 1;
			}
		}

		//Generating bias Node and make it a child of Nodes.
		tempGOGO = Instantiate (NodePrefab, new Vector3 (0,0,0), Quaternion.identity);
		tempGOGO.transform.position = new Vector3 (-445f, 108f, 0);
		tempGOGO.transform.SetParent (Nodes.transform);
		NodeArray [nodeNum] = tempGOGO;
		nodeNum = nodeNum + 1;

		//This block of code generates Node objects and arrages them in a row until the row is full then starts placing them in the next row.
		//The rows are visual only and determines where they appear on the screen during runtime.
		//These Nodes represent the Hidden Layer.
		int rowNumber = 0;
		while (nodeNum < maxNodes) {
			if (!(nodeNum < maxNodes)) {
				break;
			}
			for (int counter = 0; counter < 11; counter++) {
				if (!(nodeNum < maxNodes)) {
					break;
				}
				tempGOGO = Instantiate (NodePrefab, new Vector3 (0,0,0), Quaternion.identity);
				tempGOGO.transform.position = new Vector3 (((float)rowNumber * distanceBetweenNodes) - 400f, 295f - (float)(counter * distanceBetweenNodes), 0);

				tempGOGO.transform.SetParent (Nodes.transform);
				NodeArray [nodeNum] = tempGOGO;
				nodeNum = nodeNum + 1;
			}
			rowNumber = rowNumber + 1;
		}

		//As before but for the Out Nodes.
		for (int counter = 0; counter < outputSize; counter++) {
			tempGOGO = Instantiate (NodePrefab, new Vector3 (0,0,0), Quaternion.identity);
			tempGOGO.transform.position = new Vector3 (520f, 287f - (float)(distanceBetweenNodes * 2 * counter), 0);

			tempGOGO.transform.SetParent (Nodes.transform);
			NodeArray [maxNodes + counter] = tempGOGO;
		}
		Nodes.transform.position = new Vector3 (640, 360, 0);
	}

	//A method to update the display information (UI) whenever called.
	public void updateDisplay (List<Neuron> incomingNetwork)
	{
		
		//A variable to remember which neuron is currently being looking at.
		int nodeNum = 0;

		//Resets the variable that remembers how genomeGenes are present for UpdateGenomeDisplay().
		geneCount = 0;

		//To address a known bug where the function would sometimes be called when the NodeArray does not yet fully exist.
		if (NodeArray.Length == 1) {
			Debug.Log ("updateDisplay(): Prevented a bug when NodeArray.Length is 1");
		} else {

			//Go through all neurons and depending on their existance and value, enable and color them accordingly.
			for (int counter = 0; counter < NodeArray.Length; counter++) {

				if (incomingNetwork [counter] == null) {
					NodeArray [nodeNum].GetComponent<Image> ().enabled = false;
				} else {

					//Update the amount of genomeGenes present.
					geneCount = geneCount + incomingNetwork [counter].incomingGenes.Count;

					//If a Node exists then enable it and color it according to its value.
					if (NodeArray [nodeNum] != null) {

						NodeArray [nodeNum].GetComponent<Image> ().enabled = true;

						//60 is the players position so always color it green.
						if (nodeNum == 60) {
							NodeArray [nodeNum].GetComponent<Image> ().color = blue;
						} else if (incomingNetwork [nodeNum].value <= -1) {
							NodeArray [nodeNum].GetComponent<Image> ().color = red;
						} else if (incomingNetwork [nodeNum].value > 0) {
							NodeArray [nodeNum].GetComponent<Image> ().color = green;
						} else {
							if (nodeNum < inputSize) {
								NodeArray [nodeNum].GetComponent<Image> ().color = whiteTrans2;
							} else {
								NodeArray [nodeNum].GetComponent<Image> ().color = white;
							}
						}
					}
				}
				nodeNum = nodeNum + 1;
			}


			//This part is to update the top-bar information of the UI.
			UI.transform.Find ("GenerationsNumber").GetComponent<Text> ().text = (mainPool.generation + 1).ToString ();
			UI.transform.Find ("SpeciesNumber").GetComponent<Text> ().text = (mainPool.currentSpecies + 1).ToString ();
			UI.transform.Find ("GenomeNumber").GetComponent<Text> ().text = (mainPool.currentGenome + 1).ToString ();
			UI.transform.Find ("TopGenomeFitnessNumber").GetComponent<Text> ().text = mainPool.poolTopFitness.ToString ();
			UI.transform.Find ("StalenessNumber").GetComponent<Text> ().text = (mainPool.poolSpecies [mainPool.currentSpecies].staleness).ToString ();

			//Calculating the fitness and updating the UI.
			int tempint = Mathf.RoundToInt (playerProgress * fitnessMultiplier);
			UI.transform.Find ("CurrentFitnessNumber").GetComponent<Text> ().text = tempint.ToString ();

			//Calculating the Timeout/time left in the run.
			UI.transform.Find ("TimeoutLeftNumber").GetComponent<Text> ().text = (timeout + timeoutBonus).ToString ();

			//Calculating the Timeout/time left in the run.
			UI.transform.Find ("EndOfLevelBonusNumber").GetComponent<Text> ().text = (5000 - mainPool.currentLoop / 2).ToString ();

			//Updating the fitness of the Champion Genome stored in the pool.
			UI.transform.Find ("ChampionGenomeFitnessBoxNumber").GetComponent<Text> ().text = (mainPool.championGenome.fitness).ToString ();

		}
	}

	//A method to update the Genome part of the UI when called.
	public void updateGenomeDisplay (List<Neuron> incomingNetwork)
	{

		//To address a known bug where the function would sometimes be called when the NodeArray does not yet fully exist.
		if (NodeArray.Length == 1) {
			Debug.Log ("updateGenomeDisplay(): Prevented bug when NodeArray.Length is 1");
		} else {

			//If a new run has started then wipe the current display and generate the new one.
			if (restartGenomeDisplay) {

				//Destroy all objects in the array, set the array to null and generate a new array as big as genecount defined in UpdateDisplay().
				for (int counter = 0; counter < GeneArray.Length; counter++) {
					DestroyImmediate (GeneArray [counter]);
				}
				GeneArray = null;
				GeneArray = new GameObject[geneCount];

				//Temporary variables to keep track of the current Node and Gene.
				int nodeNum = 0;
				int currentGene = 0;

				//A Loop to generate all of the genomeGenes.
				for (int counter = 0; counter < maxNodes + outputSize; counter++) {

					//Ignore if the Neuron does not exist or if it has no Genes incoming.
					if (incomingNetwork [counter] != null) {
						if (incomingNetwork [counter].incomingGenes.Count != 0) {

							//Go through all of the neurons Genes.
							for (int counter2 = 0; counter2 < incomingNetwork [counter].incomingGenes.Count; counter2++) {

								//Generate the Gene UI object.
								tempGOGO = Instantiate (GenePrefab, new Vector3 (0f, 0f, 0), Quaternion.identity);

								//This block of code defines the size, proportions, rotation and position of the GeneUI object to become a rectangle from the Genes in and out neurons.
								Vector3 fromPoint = NodeArray [incomingNetwork [counter].incomingGenes [counter2].neuronOut].GetComponent<RectTransform> ().position;
								Vector3 toPoint = NodeArray [incomingNetwork [counter].incomingGenes [counter2].neuronIn].GetComponent<RectTransform> ().position;
								;
								Vector3 differencePoints = toPoint - fromPoint;
								tempGOGO.GetComponent<RectTransform> ().sizeDelta = new Vector2 (differencePoints.magnitude, 5f);
								tempGOGO.GetComponent<RectTransform> ().pivot = new Vector2 (0f, 0.5f);
								tempGOGO.GetComponent<RectTransform> ().position = fromPoint;
								float angle = Mathf.Atan2 (differencePoints.y, differencePoints.x) * Mathf.Rad2Deg;
								tempGOGO.GetComponent<RectTransform> ().rotation = Quaternion.Euler (0, 0, angle);

								//Sets this GeneUI Object the child of Genes.
								tempGOGO.transform.SetParent (Genes.transform);

								//Assign this object to the array and increment the amount of Genes present.
								GeneArray [currentGene] = tempGOGO;
								currentGene = currentGene + 1;
							}
						}
					}
					//Increment the amount of Nodes present.
					nodeNum = nodeNum + 1;
				}
				//Debug.Log ("Genome display restarted.");
				restartGenomeDisplay = false;
			} else {

				//If the UI has alreadly been generated then just update it.
				//Temporary variable to keep track of the Gene currently being looked at.
				int geneNum = 0;

				//A loop to go through all of the Genes.
				for (int counter = 0; counter < maxNodes + outputSize; counter++) {

					//If the Neuron is Null or has no Genes then ignore.
					if (incomingNetwork [counter] != null) {
						if (incomingNetwork [counter].incomingGenes.Count != 0) {

							//For each Gene, update their color according to their weight
							for (int counter2 = 0; counter2 < incomingNetwork [counter].incomingGenes.Count; counter2++) {

								if (incomingNetwork [counter].incomingGenes [counter2].weight > 0) {
									GeneArray [geneNum].GetComponent<Image> ().color = greenTrans;
								} else if (incomingNetwork [counter].incomingGenes [counter2].weight <= -1) {
									GeneArray [geneNum].GetComponent<Image> ().color = redTrans;
								} else {
									GeneArray [geneNum].GetComponent<Image> ().color = whiteTrans;
								}
								geneNum = geneNum + 1;

							}
						}
					}

				}
			}
		}
	}



	//A method to read in the data from a file that contains a previously saved Pool and assign that data to the NN's Pool.
	public void readInPool ()
	{

		//Create and assign a new Pool the NN's main Pool.
		mainPool = basicPool ();

		//Define the variables that will hold the data that will be read in line by line.
		string lineIn;
		string[] lineInParts;

		//Variables that will hold and define how far to read and what value to assign as the file is being read.
		int speciesCount = 1;
		int genomeCount = 1;
		int geneCount = 1;
		int enabledNum = 1;

		//Attempt to read in the file and if the file cannot be found then generate a new Pool.
		try {

			//Open the .pool file using TextReader.
			using (TextReader reader = File.OpenText ("savedPool.pool")) {

				//These 3 lines of code repeat through out the method and do the following:
				//Read in the next line in the file, split the string into multiple strings using ' ' as the seperator, assign a part of that string array to a variable in the Pool.
				lineIn = reader.ReadLine ();
				lineInParts = lineIn.Split (' ');
				mainPool.generation = int.Parse (lineInParts [0]);

				lineIn = reader.ReadLine ();
				lineInParts = lineIn.Split (' ');
				mainPool.innovation = int.Parse (lineInParts [0]);

				lineIn = reader.ReadLine ();
				lineInParts = lineIn.Split (' ');
				mainPool.poolTopFitness = int.Parse (lineInParts [0]);

				lineIn = reader.ReadLine ();
				lineInParts = lineIn.Split (' ');
				speciesCount = int.Parse (lineInParts [0]);

				//Create a number of poolSpecies as is specified in the file.
				for (int counter = 0; counter < speciesCount; counter++) {
					mainPool.poolSpecies.Add (new Species ());
				}

				//For each poolSpecies that exist do the following:
				for (int counter1 = 0; counter1 < speciesCount; counter1++) {

					lineIn = reader.ReadLine ();
					lineInParts = lineIn.Split (' ');
					mainPool.poolSpecies [counter1].topFitness = int.Parse (lineInParts [0]);

					lineIn = reader.ReadLine ();
					lineInParts = lineIn.Split (' ');
					mainPool.poolSpecies [counter1].averageFitness = int.Parse (lineInParts [0]);

					lineIn = reader.ReadLine ();
					lineInParts = lineIn.Split (' ');
					mainPool.poolSpecies [counter1].staleness = int.Parse (lineInParts [0]);

					lineIn = reader.ReadLine ();
					lineInParts = lineIn.Split (' ');
					genomeCount = int.Parse (lineInParts [0]);

					//Create a number of Genomes that is specified in the file for the Species in Question.
					for (int counterA = 0; counterA < genomeCount; counterA++) {
						mainPool.poolSpecies [counter1].speciesGenomes.Add (new Genome ());
					}

					//For each Genome that exists do the following:
					for (int counter2 = 0; counter2 < genomeCount; counter2++) {

						lineIn = reader.ReadLine ();
						lineInParts = lineIn.Split (' ');
						mainPool.poolSpecies [counter1].speciesGenomes [counter2].fitness = int.Parse (lineInParts [0]);

						lineIn = reader.ReadLine ();
						lineInParts = lineIn.Split (' ');
						mainPool.poolSpecies [counter1].speciesGenomes [counter2].adjustedFitness = int.Parse (lineInParts [0]);

						lineIn = reader.ReadLine ();
						lineInParts = lineIn.Split (' ');
						mainPool.poolSpecies [counter1].speciesGenomes [counter2].globalRank = int.Parse (lineInParts [0]);

						lineIn = reader.ReadLine ();
						lineInParts = lineIn.Split (' ');
						mainPool.poolSpecies [counter1].speciesGenomes [counter2].genomeBiasChance = float.Parse (lineInParts [0]);

						lineIn = reader.ReadLine ();
						lineInParts = lineIn.Split (' ');
						mainPool.poolSpecies [counter1].speciesGenomes [counter2].genomeGeneWeightChance = float.Parse (lineInParts [0]);

						lineIn = reader.ReadLine ();
						lineInParts = lineIn.Split (' ');
						mainPool.poolSpecies [counter1].speciesGenomes [counter2].genomeDisableChance = float.Parse (lineInParts [0]);

						lineIn = reader.ReadLine ();
						lineInParts = lineIn.Split (' ');
						mainPool.poolSpecies [counter1].speciesGenomes [counter2].genomeEnableChance = float.Parse (lineInParts [0]);

						lineIn = reader.ReadLine ();
						lineInParts = lineIn.Split (' ');
						mainPool.poolSpecies [counter1].speciesGenomes [counter2].genomeLinkChance = float.Parse (lineInParts [0]);

						lineIn = reader.ReadLine ();
						lineInParts = lineIn.Split (' ');
						mainPool.poolSpecies [counter1].speciesGenomes [counter2].genomeNodeChance = float.Parse (lineInParts [0]);

						lineIn = reader.ReadLine ();
						lineInParts = lineIn.Split (' ');
						mainPool.poolSpecies [counter1].speciesGenomes [counter2].genomeStepSize = float.Parse (lineInParts [0]);

						lineIn = reader.ReadLine ();
						lineInParts = lineIn.Split (' ');
						geneCount = int.Parse (lineInParts [0]);

						//Create a number of Genes specified in the file for the Genome in Question
						for (int counterB = 0; counterB < geneCount; counterB++) {
							mainPool.poolSpecies [counter1].speciesGenomes [counter2].genomeGenes.Add (new Gene ());
						}

						//For each Gene do the following:
						for (int counter3 = 0; counter3 < geneCount; counter3++) {
							lineIn = reader.ReadLine ();
							lineInParts = lineIn.Split (' ');
							mainPool.poolSpecies [counter1].speciesGenomes [counter2].genomeGenes [counter3].neuronIn = int.Parse (lineInParts [0]);

							lineIn = reader.ReadLine ();
							lineInParts = lineIn.Split (' ');
							mainPool.poolSpecies [counter1].speciesGenomes [counter2].genomeGenes [counter3].neuronOut = int.Parse (lineInParts [0]);

							lineIn = reader.ReadLine ();
							lineInParts = lineIn.Split (' ');
							mainPool.poolSpecies [counter1].speciesGenomes [counter2].genomeGenes [counter3].weight = float.Parse (lineInParts [0]);

							lineIn = reader.ReadLine ();
							lineInParts = lineIn.Split (' ');
							mainPool.poolSpecies [counter1].speciesGenomes [counter2].genomeGenes [counter3].innovation = int.Parse (lineInParts [0]);

							lineIn = reader.ReadLine ();
							lineInParts = lineIn.Split (' ');
							enabledNum = int.Parse (lineInParts [0]);

							if (enabledNum == 1) {
								mainPool.poolSpecies [counter1].speciesGenomes [counter2].genomeGenes [counter3].enabled = true;
							} else {
								mainPool.poolSpecies [counter1].speciesGenomes [counter2].genomeGenes [counter3].enabled = false;
							}
						}
					}
				}

				//Repeat the above process for just the ChampionGenome.
				//This step does not need to happen for the SwappedOutGenome.
				lineIn = reader.ReadLine ();
				lineInParts = lineIn.Split (' ');
				mainPool.championGeneration = int.Parse (lineInParts [0]);

				lineIn = reader.ReadLine ();
				lineInParts = lineIn.Split (' ');
				mainPool.championGenome.fitness = int.Parse (lineInParts [0]);

				lineIn = reader.ReadLine ();
				lineInParts = lineIn.Split (' ');
				mainPool.championGenome.adjustedFitness = int.Parse (lineInParts [0]);

				lineIn = reader.ReadLine ();
				lineInParts = lineIn.Split (' ');
				mainPool.championGenome.globalRank = int.Parse (lineInParts [0]);

				lineIn = reader.ReadLine ();
				lineInParts = lineIn.Split (' ');
				mainPool.championGenome.genomeBiasChance = float.Parse (lineInParts [0]);

				lineIn = reader.ReadLine ();
				lineInParts = lineIn.Split (' ');
				mainPool.championGenome.genomeGeneWeightChance = float.Parse (lineInParts [0]);

				lineIn = reader.ReadLine ();
				lineInParts = lineIn.Split (' ');
				mainPool.championGenome.genomeDisableChance = float.Parse (lineInParts [0]);

				lineIn = reader.ReadLine ();
				lineInParts = lineIn.Split (' ');
				mainPool.championGenome.genomeEnableChance = float.Parse (lineInParts [0]);

				lineIn = reader.ReadLine ();
				lineInParts = lineIn.Split (' ');
				mainPool.championGenome.genomeLinkChance = float.Parse (lineInParts [0]);

				lineIn = reader.ReadLine ();
				lineInParts = lineIn.Split (' ');
				mainPool.championGenome.genomeNodeChance = float.Parse (lineInParts [0]);

				lineIn = reader.ReadLine ();
				lineInParts = lineIn.Split (' ');
				mainPool.championGenome.genomeStepSize = float.Parse (lineInParts [0]);

				lineIn = reader.ReadLine ();
				lineInParts = lineIn.Split (' ');
				geneCount = int.Parse (lineInParts [0]);

				for (int counterB = 0; counterB < geneCount; counterB++) {
					mainPool.championGenome.genomeGenes.Add (new Gene ());
				}

				for (int counter3 = 0; counter3 < geneCount; counter3++) {
					lineIn = reader.ReadLine ();
					lineInParts = lineIn.Split (' ');
					mainPool.championGenome.genomeGenes [counter3].neuronIn = int.Parse (lineInParts [0]);

					lineIn = reader.ReadLine ();
					lineInParts = lineIn.Split (' ');
					mainPool.championGenome.genomeGenes [counter3].neuronOut = int.Parse (lineInParts [0]);

					lineIn = reader.ReadLine ();
					lineInParts = lineIn.Split (' ');
					mainPool.championGenome.genomeGenes [counter3].weight = float.Parse (lineInParts [0]);

					lineIn = reader.ReadLine ();
					lineInParts = lineIn.Split (' ');
					mainPool.championGenome.genomeGenes [counter3].innovation = int.Parse (lineInParts [0]);

					lineIn = reader.ReadLine ();
					lineInParts = lineIn.Split (' ');
					enabledNum = int.Parse (lineInParts [0]);

					if (enabledNum == 1) {
						mainPool.championGenome.genomeGenes [counter3].enabled = true;
					} else {
						mainPool.championGenome.genomeGenes [counter3].enabled = false;
					}
				}

			}

			//Figure out which Genome to run next.
			while (genomeAlreadlyTested ()) {
				nextGenome ();
			}

			//Start a new run with the next Genome to run.
			initializeRun ();

			Debug.Log ("The pool has been loaded.");

		} catch (FileNotFoundException e) {
			Debug.Log ("The file was not found to be read in. Generating a new Pool in it's place." + e);
			initializePool ();
		}

	}

	//A method to save the current state of the Pool to a file to be loaded for later uses of teh application.
	public void savePool ()
	{

		//Using TextWriter create a new "savedPool.pool" file or overwrite one that alreadly exists.
		using (TextWriter writer = File.CreateText ("savedPool.pool")) {

			//The method writes a value to a line of the .pool file and then moves along to the next line and repeats the process.
			writer.WriteLine (mainPool.generation);
			writer.WriteLine (mainPool.innovation);
			writer.WriteLine (mainPool.poolTopFitness);
			writer.WriteLine (mainPool.poolSpecies.Count);

			//For each Species that exists within the Pool.
			for (int counter1 = 0; counter1 < mainPool.poolSpecies.Count; counter1++) {
				writer.WriteLine (mainPool.poolSpecies [counter1].topFitness);
				writer.WriteLine (mainPool.poolSpecies [counter1].averageFitness);
				writer.WriteLine (mainPool.poolSpecies [counter1].staleness);
				writer.WriteLine (mainPool.poolSpecies [counter1].speciesGenomes.Count);

				//For each Genome that exists within said Species.
				for (int counter2 = 0; counter2 < mainPool.poolSpecies [counter1].speciesGenomes.Count; counter2++) {
					writer.WriteLine (mainPool.poolSpecies [counter1].speciesGenomes [counter2].fitness);
					writer.WriteLine (mainPool.poolSpecies [counter1].speciesGenomes [counter2].adjustedFitness);
					writer.WriteLine (mainPool.poolSpecies [counter1].speciesGenomes [counter2].globalRank);
					writer.WriteLine (mainPool.poolSpecies [counter1].speciesGenomes [counter2].genomeBiasChance);
					writer.WriteLine (mainPool.poolSpecies [counter1].speciesGenomes [counter2].genomeGeneWeightChance);
					writer.WriteLine (mainPool.poolSpecies [counter1].speciesGenomes [counter2].genomeDisableChance);
					writer.WriteLine (mainPool.poolSpecies [counter1].speciesGenomes [counter2].genomeEnableChance);
					writer.WriteLine (mainPool.poolSpecies [counter1].speciesGenomes [counter2].genomeLinkChance);
					writer.WriteLine (mainPool.poolSpecies [counter1].speciesGenomes [counter2].genomeNodeChance);
					writer.WriteLine (mainPool.poolSpecies [counter1].speciesGenomes [counter2].genomeStepSize);

					writer.WriteLine (mainPool.poolSpecies [counter1].speciesGenomes [counter2].genomeGenes.Count);

					//For each Gene that exists with said Genome.
					for (int counter3 = 0; counter3 < mainPool.poolSpecies [counter1].speciesGenomes [counter2].genomeGenes.Count; counter3++) {
						writer.WriteLine (mainPool.poolSpecies [counter1].speciesGenomes [counter2].genomeGenes [counter3].neuronIn);
						writer.WriteLine (mainPool.poolSpecies [counter1].speciesGenomes [counter2].genomeGenes [counter3].neuronOut);
						writer.WriteLine (mainPool.poolSpecies [counter1].speciesGenomes [counter2].genomeGenes [counter3].weight);
						writer.WriteLine (mainPool.poolSpecies [counter1].speciesGenomes [counter2].genomeGenes [counter3].innovation);

						if (mainPool.poolSpecies [counter1].speciesGenomes [counter2].genomeGenes [counter3].enabled) {
							writer.WriteLine (1);
						} else {
							writer.WriteLine (0);
						}
					}

					//The Genome's genomeNetwork does not need to be saved as it is generated upon InitializeRun().
				}
			}

			writer.WriteLine (mainPool.championGeneration);

			//Writing Champion Genome to the file.
			//We don't write the SwappedGenome to the file as there is no point.
			writer.WriteLine (mainPool.championGenome.fitness);
			writer.WriteLine (mainPool.championGenome.adjustedFitness);
			writer.WriteLine (mainPool.championGenome.globalRank);
			writer.WriteLine (mainPool.championGenome.genomeBiasChance);
			writer.WriteLine (mainPool.championGenome.genomeGeneWeightChance);
			writer.WriteLine (mainPool.championGenome.genomeDisableChance);
			writer.WriteLine (mainPool.championGenome.genomeEnableChance);
			writer.WriteLine (mainPool.championGenome.genomeLinkChance);
			writer.WriteLine (mainPool.championGenome.genomeNodeChance);
			writer.WriteLine (mainPool.championGenome.genomeStepSize);
			writer.WriteLine (mainPool.championGenome.genomeGenes.Count);

			for (int counter = 0; counter < mainPool.championGenome.genomeGenes.Count; counter++) {
				writer.WriteLine (mainPool.championGenome.genomeGenes [counter].neuronIn);
				writer.WriteLine (mainPool.championGenome.genomeGenes [counter].neuronOut);
				writer.WriteLine (mainPool.championGenome.genomeGenes [counter].weight);
				writer.WriteLine (mainPool.championGenome.genomeGenes [counter].innovation);

				if (mainPool.championGenome.genomeGenes [counter].enabled) {
					writer.WriteLine (1);
				} else {
					writer.WriteLine (0);
				}
			}
		}

		Debug.Log ("The pool has been saved.");
	}

	//Retreive the inputs from what is around the player from the LM and store them in inputs.
	public void sendInputs (GameObject[,] incomingArray, int playerX, int playerY)
	{

		//Temporary variables to keep track of where in the incomingArray we are looking at.
		int tempIntA = 0;
		int tempIntB = 0;

		//Go through the array one dimension at a time.
		for (int counter1 = -5; counter1 < 6; counter1++) {
			for (int counter2 = -5; counter2 < 6; counter2++) {

				//Depending on the value of the objects internal values (The tiles around the player) assign different values to our input array.
				if (incomingArray [playerY + counter2, playerX + counter1].GetComponent<TileController> ().negative == true) {
					tempIntA = counter1 + 5;
					tempIntB = counter2 + 5;
					inputs [tempIntA, tempIntB] = -1;

				} else if (incomingArray [playerY + counter2, playerX + counter1].GetComponent<TileController> ().positive == true) {
					tempIntA = counter1 + 5;
					tempIntB = counter2 + 5;
					inputs [tempIntA, tempIntB] = 1;

				} else {
					tempIntA = counter1 + 5;
					tempIntB = counter2 + 5;
					inputs [tempIntA, tempIntB] = 0;
				}
			}
		}
	}

	//A method to genetically crossover the genomeGenes of two speciesGenomes with genome 1 better the one of higher fitness.
	public Genome genomeCrossover (Genome genome1, Genome genome2)
	{

		//New genome that will be returned.
		Genome childGenome = new Genome ();

		//Genes by their innovation number.
		Gene[] g1ByInnovation;
		Gene[] g2ByInnovation;

		//Highest innovation number.
		int gHigh = 0;

		//Making sure genome1 is the higher fitness genome.
		if (genome1.fitness < genome2.fitness) {

			Genome temp1 = new Genome ();
			temp1.genomeFullCopy (genome1);
			Genome temp2 = new Genome ();
			temp2.genomeFullCopy (genome2);

			genome1.genomeFullCopy (temp2);
			genome2.genomeFullCopy (temp1);
		}

		//Find the highest innovation number to make the array said size.
		for (int counter = 0; counter < genome1.genomeGenes.Count; counter++) {
			if (genome1.genomeGenes [counter].innovation > gHigh) {
				gHigh = genome1.genomeGenes [counter].innovation;
			}
		}
		for (int counter = 0; counter < genome2.genomeGenes.Count; counter++) {
			if (genome2.genomeGenes [counter].innovation > gHigh) {
				gHigh = genome2.genomeGenes [counter].innovation;
			}
		}

		//Making the arrays the same length of the maximium.
		g1ByInnovation = new Gene[gHigh + 1];
		g2ByInnovation = new Gene[gHigh + 1];

		//Manually setting the values of the arrays to Null.
		for (int counter = 0; counter < gHigh; counter++) {
			g1ByInnovation [counter] = null;
			g2ByInnovation [counter] = null;
		}

		//Adding in all of the genomeGenes according to their innovation number.
		for (int counter = 0; counter < genome1.genomeGenes.Count; counter++) {
			g1ByInnovation [genome1.genomeGenes [counter].innovation] = new Gene ();
			g1ByInnovation [genome1.genomeGenes [counter].innovation].geneFullCopy (genome1.genomeGenes [counter]);
		}
		for (int counter = 0; counter < genome2.genomeGenes.Count; counter++) {
			g2ByInnovation [genome2.genomeGenes [counter].innovation] = new Gene ();
			g2ByInnovation [genome2.genomeGenes [counter].innovation].geneFullCopy (genome2.genomeGenes [counter]);
		}

		//Comparing genomeGenes and adding them to the new child genome.
		//If only one exists add that one, both null do nothing, if the 2nd is enabled and a 50-50 chance that gene will be added, otherwise add the 1st.
		for (int counter = 0; counter < g1ByInnovation.Length; counter++) {

			if (g2ByInnovation [counter] == null) {

				if (g1ByInnovation [counter] == null) {
					//Both are null, do nothing.
				} else {
					Gene tempGene = new Gene ();
					tempGene.geneFullCopy (g1ByInnovation [counter]);
					childGenome.genomeGenes.Add (tempGene);
				}

			} else {

				if (g1ByInnovation [counter] == null) {
					Gene tempGene = new Gene ();
					tempGene.geneFullCopy (g2ByInnovation [counter]);
					childGenome.genomeGenes.Add (tempGene);
				} else if (g2ByInnovation [counter].enabled && Random.Range (0, 2) == 1) {
					Gene tempGene = new Gene ();
					tempGene.geneFullCopy (g2ByInnovation [counter]);
					childGenome.genomeGenes.Add (tempGene);
				} else {
					Gene tempGene = new Gene ();
					tempGene.geneFullCopy (g1ByInnovation [counter]);
					childGenome.genomeGenes.Add (tempGene);
				}
			}
		}

		//Setting the new speciesGenomes mutation rates to the better genome's.
		childGenome.genomeGeneWeightChance = genome1.genomeGeneWeightChance;
		childGenome.genomeLinkChance = genome1.genomeLinkChance;
		childGenome.genomeBiasChance = genome1.genomeBiasChance;
		childGenome.genomeEnableChance = genome1.genomeEnableChance;
		childGenome.genomeDisableChance = genome1.genomeDisableChance;
		childGenome.genomeNodeChance = genome1.genomeNodeChance;
		childGenome.genomeStepSize = genome1.genomeStepSize;

		//Returning new genome.
		return childGenome;
	}

	//Creates and returns a basic starting genome. (Brand new and mutated.)
	public Genome basicGenome ()
	{
		Genome tempG = new Genome ();
		mutate (ref tempG);
		return tempG;
	}

	//Creates and sets up a brand new Pool Object.
	public Pool basicPool ()
	{
		Pool tempP = new Pool ();
		tempP.innovation = outputSize;
		return tempP;
	}

	//Generate the genomeNetwork for the given Genome
	public void generateNetwork (ref Genome genome0)
	{

		//Creating a new Neuron List to represent the newly generated genomeNetwork.
		List<Neuron> tempNetwork = new List<Neuron> ();

		//Create input neurons
		//Last one generated is the bias neuron
		for (int counter = 0; counter <= inputSize; counter++) {
			tempNetwork.Add (new Neuron ());
		}

		//Filling out the availible space with null for neurons to be adding in later.
		for (int counter = inputSize + 1; counter < maxNodes; counter++) {
			tempNetwork.Add (null);
		}

		//Adding output neurons at the end.
		for (int counter = 0; counter < outputSize; counter++) {
			tempNetwork.Add (new Neuron ());
		}

		//Sorting the genomeGenes in the genome based on their neuronOut value using a bubble sort.
		bool swapped = true;
		Gene tempGene1 = new Gene ();
		Gene tempGene2 = new Gene ();
		while (swapped) {
			swapped = false;
			for (int counter = 0; counter < genome0.genomeGenes.Count - 1; counter++) {

				int tempIntN = counter + 1;

				if (genome0.genomeGenes [counter].neuronOut > genome0.genomeGenes [tempIntN].neuronOut) {
					tempGene1 = new Gene ();
					tempGene2 = new Gene ();
					tempGene1.geneFullCopy (genome0.genomeGenes [counter]);
					tempGene2.geneFullCopy (genome0.genomeGenes [tempIntN]);
					genome0.genomeGenes [counter].geneFullCopy (tempGene2);
					genome0.genomeGenes [tempIntN].geneFullCopy (tempGene1);

					swapped = true;
				}
			}
		}

		//Generating the genomeNetwork.
		for (int counter = 0; counter < genome0.genomeGenes.Count; counter++) {

			//Check to see if the gene is enabled, if not ignore.
			if (genome0.genomeGenes [counter].enabled) {

				//If there is no Neuron that a gene is trying to come from, generate a Neuron.
				if (tempNetwork [genome0.genomeGenes [counter].neuronOut] == null) {
					tempNetwork [genome0.genomeGenes [counter].neuronOut] = new Neuron ();
				}

				//Add the Gene in question to the Neuron that alreadly exists or was just created.
				tempGene1 = new Gene ();
				tempGene1.geneFullCopy (genome0.genomeGenes [counter]);
				tempNetwork [genome0.genomeGenes [counter].neuronOut].incomingGenes.Add (tempGene1);

				//If there is no Neuron that a gene is trying to get to, generate a Neuron.
				if (tempNetwork [genome0.genomeGenes [counter].neuronIn] == null) {
					tempNetwork [genome0.genomeGenes [counter].neuronIn] = new Neuron ();
				}
			}
		}

		//Assign the newly created genomeNetwork to the Genome.
		genome0.genomeNetwork = tempNetwork;
	}

	//Returns the number representing a random neuron from the passed through gene list.
	public int randomNeuron (List<Gene> incomingGenes, bool inputNode)
	{

		//Bool array for all possible nodes including the input nodes and output nodes.
		bool[] neuronBoolList = new bool[outputSize + maxNodes];

		//Temp int for counting.
		int tempCount = 0;

		//If inputnode is false then set nodes equal to inputsize to true.
		if (!inputNode) {
			for (int counter = 0; counter < inputSize; counter++) {
				neuronBoolList [counter] = true;
				tempCount = tempCount + 1;
			}
		}

		//Set the nodes at the end which are the output nodes to true.
		for (int counter = 0; counter < outputSize; counter++) {
			neuronBoolList [counter + maxNodes] = true;
			tempCount = tempCount + 1;
		}

		//If the incoming genomeGenes in or out value is not a input node set to true
		for (int counter = 0; counter < incomingGenes.Count; counter++) {

			if (!inputNode || incomingGenes [counter].neuronIn > inputSize) {
				neuronBoolList [incomingGenes [counter].neuronIn] = true;
				tempCount = tempCount + 1;
			}

			if (!inputNode || incomingGenes [counter].neuronOut > inputSize) {
				neuronBoolList [incomingGenes [counter].neuronOut] = true;
				tempCount = tempCount + 1;
			}

		}

		//Temp in to hold which neuron to return gained randomly between 0 and the amount of true values.
		int tempHigh = Random.Range (0, tempCount);

		//Running through the list to get said number to return.
		for (int counter = 0; counter < neuronBoolList.Length; counter++) {
			if (neuronBoolList [counter] == true) {
				tempHigh = tempHigh - 1;
				if (tempHigh == 0) {
					return counter;
				}
			}
		}

		//Other wise return 0.
		return 0;

	}

	//A method to check if there is a link between a gene and a set of genomeGenes (Both genomeGenes having the same neuronIn and neuronOut values).
	public bool containsLink (List<Gene> incomingGenes, Gene possibleLink)
	{

		for (int counter = 0; counter < incomingGenes.Count; counter++) {

			if (incomingGenes [counter].neuronIn == possibleLink.neuronIn && incomingGenes [counter].neuronOut == possibleLink.neuronOut) {
				return true;
			}

		}
		return false;
	}

	//Go through a speciesGenomes genomeGenes and alter their weights depending on the value of perturbChance and a random value.
	public void geneWeightMutate (ref Genome incomingGenome5)
	{

		//A variable to more conviently store the Genome genomeStepSize value.
		float tempStep = incomingGenome5.genomeStepSize;

		//For each Gene in the Genome.
		for (int counter = 0; counter < incomingGenome5.genomeGenes.Count; counter++) {

			if (Random.Range (0f, 1f) < perturbChance) {
				incomingGenome5.genomeGenes [counter].weight = incomingGenome5.genomeGenes [counter].weight + Random.Range (0f, 1f) * (tempStep * 2) - tempStep;
			} else {
				incomingGenome5.genomeGenes [counter].weight = Random.Range (0f, 1f) * 4 - 2;
			}
		}
	}

	//A method to mutate a genome to create another link (A gene connecting two neurons).
	public void geneLinkMutate (ref Genome incomingGenome4, bool bias)
	{

		//Select two random neurons, the second having no chance of being a inputNode.
		int tempNeuron1 = randomNeuron (incomingGenome4.genomeGenes, false);
		int tempNeuron2 = randomNeuron (incomingGenome4.genomeGenes, true);

		//If both Neurons are input Neurons or both are output Neurons, re-roll.
		while ((tempNeuron1 < inputSize && tempNeuron2 < inputSize) || (tempNeuron1 >= maxNodes && tempNeuron2 >= maxNodes)) {
			tempNeuron1 = randomNeuron (incomingGenome4.genomeGenes, false);
			tempNeuron2 = randomNeuron (incomingGenome4.genomeGenes, true);
		}

		//New Temp Gene.
		Gene tempGene = new Gene ();

		//Swap input and output neurons.
		if (tempNeuron1 > tempNeuron2) {
			int tempint = tempNeuron1;
			tempNeuron1 = tempNeuron2;
			tempNeuron2 = tempint;
		}

		//Assign the new gene's in and out neurons.
		tempGene.neuronIn = tempNeuron1;
		tempGene.neuronOut = tempNeuron2;

		//If bias is true, reassign the input gene to this neuron.
		if (bias) {
			tempGene.neuronIn = inputSize;
		}

		//If no link currently exists, create it.
		if (!containsLink (incomingGenome4.genomeGenes, tempGene)) {
			mainPool.increaseInnovation ();
			tempGene.innovation = mainPool.innovation;
			tempGene.weight = Random.Range (0f, 1f) * 4 - 2;
			incomingGenome4.genomeGenes.Add (tempGene);
		}
	}

	//A method to disable an existing gene and replace it with a new node and two genomeGenes still keeping the incoming and outgoing direction as before.
	public void nodeMutate (ref Genome incomingGenome3)
	{

		//If there are no genomeGenes ignore.
		if (incomingGenome3.genomeGenes.Count != 0) {

			//Variable to remember if there is a gene availible to disable.
			int genesToSwitch = 0;
			for (int counter = 0; counter < incomingGenome3.genomeGenes.Count; counter++) {
				if (incomingGenome3.genomeGenes [counter].enabled) {
					genesToSwitch = genesToSwitch + 1;
				}
			}

			//If there are genes that could be disabled:
			if (genesToSwitch > 0) {

				//Select a random Node to create the 2 new genomeGenes to and from.
				int tempNodeNum = Random.Range (inputSize + 1, maxNodes);

				//Select a random gene to disable and create the new genomeGenes and node from.

				//Which enabled gene from the start to disable.
				int tempNumA = Random.Range (1, genesToSwitch + 1);

				//The position to check for an enabled gene.
				int tempNumB = 0;

				//The place an enabled gene was last found at.
				int tempNumC = 0;

				//Find the tempNumA-th gene to disable.
				while (tempNumA != 0) {

					if (incomingGenome3.genomeGenes [tempNumB].enabled == true) {
						tempNumA = tempNumA - 1;
						tempNumC = tempNumB;
					}
					tempNumB = tempNumB + 1;
				}

				//Disable gene and copy values to new gene.
				incomingGenome3.genomeGenes [tempNumC].enabled = false;

				//Create the first new Gene.
				Gene tempGene2 = new Gene ();
				tempGene2.geneFullCopy (incomingGenome3.genomeGenes [tempNumC]);

				//Modify new gene.
				tempGene2.neuronOut = tempNodeNum;
				tempGene2.weight = 1f;
				mainPool.increaseInnovation ();
				tempGene2.innovation = mainPool.innovation;
				tempGene2.enabled = true;
				incomingGenome3.genomeGenes.Add (tempGene2);

				//Create the second new Gene.
				Gene tempGene3 = new Gene ();
				tempGene3.geneFullCopy (incomingGenome3.genomeGenes [tempNumC]);

				//Modify new gene.
				tempGene3.neuronIn = tempNodeNum;
				mainPool.increaseInnovation ();
				tempGene3.innovation = mainPool.innovation;
				tempGene3.enabled = true;
				incomingGenome3.genomeGenes.Add (tempGene3);


			} else {
				//No genes that are enabled.
			}
		}
	}

	//A method to find a single gene in the incoming genome to flip to what toEnable is.
	public void disableEnableMutate (ref Genome incomingGenome1, bool toEnable)
	{

		//Variables that will hold which genomeGenes can switch.
		int numOfGenesToSwitch = 0;
		int geneToSwitch = 0;
		int listPos = 0;

		//Ignore if Genome has no genomeGenes.
		if (incomingGenome1.genomeGenes.Count != 0) {

			//See how mant genomeGenes are availible to switch.
			for (int counter = 0; counter < incomingGenome1.genomeGenes.Count; counter++) {
				if (incomingGenome1.genomeGenes [counter].enabled != toEnable) {
					numOfGenesToSwitch = numOfGenesToSwitch + 1;
				}
			}

			//Ignore if there are no suitable genomeGenes to switch.
			if (numOfGenesToSwitch != 0) {

				//Select a random gene to flip
				geneToSwitch = Random.Range (0, numOfGenesToSwitch);

				//Loop through the genomeGenes until the correct gene is found.
				while (geneToSwitch > 0) {

					if (incomingGenome1.genomeGenes [listPos].enabled != toEnable) {
						geneToSwitch = geneToSwitch - 1;
					}
					listPos = listPos + 1;
				}

				//Flip the enabled value of the gene.
				incomingGenome1.genomeGenes [listPos].enabled = !incomingGenome1.genomeGenes [listPos].enabled;
			}
		}
	}

	//A method that calls the other mutate functions and alters values and mutate chances.
	public void mutate (ref Genome incomingGenome2)
	{

		//A variable to hold the amount of times a certain mutation method is called.
		float numOfGenomeMutations = 0;

		//Alter the mutation rates of the genome for all of its rates.
		if (Random.Range (0, 2) == 0) {
			incomingGenome2.genomeBiasChance = incomingGenome2.genomeBiasChance * 0.9f;
		} else {
			incomingGenome2.genomeBiasChance = incomingGenome2.genomeBiasChance * 1.1f;
		}

		if (Random.Range (0, 2) == 0) {
			incomingGenome2.genomeLinkChance = incomingGenome2.genomeLinkChance * 0.9f;
		} else {
			incomingGenome2.genomeLinkChance = incomingGenome2.genomeLinkChance * 1.1f;
		}

		if (Random.Range (0, 2) == 0) {
			incomingGenome2.genomeGeneWeightChance = incomingGenome2.genomeGeneWeightChance * 0.9f;
		} else {
			incomingGenome2.genomeGeneWeightChance = incomingGenome2.genomeGeneWeightChance * 1.1f;
		}

		if (Random.Range (0, 2) == 0) {
			incomingGenome2.genomeDisableChance = incomingGenome2.genomeDisableChance * 0.9f;
		} else {
			incomingGenome2.genomeDisableChance = incomingGenome2.genomeDisableChance * 1.1f;
		}

		if (Random.Range (0, 2) == 0) {
			incomingGenome2.genomeEnableChance = incomingGenome2.genomeEnableChance * 0.9f;
		} else {
			incomingGenome2.genomeEnableChance = incomingGenome2.genomeEnableChance * 1.1f;
		}

		if (Random.Range (0, 2) == 0) {
			incomingGenome2.genomeNodeChance = incomingGenome2.genomeNodeChance * 0.9f;
		} else {
			incomingGenome2.genomeNodeChance = incomingGenome2.genomeNodeChance * 1.1f;
		}

		//For each outside of geneWeightMutate, called each function each time Random.Range(0f,1f) is less than it's mutationRate
		//For each attempt reduce numOfGenomeMutations until its 0 or less.

		if (Random.Range (0f, 1f) < incomingGenome2.genomeGeneWeightChance) {
			geneWeightMutate (ref incomingGenome2);
		}

		numOfGenomeMutations = incomingGenome2.genomeLinkChance;
		while (numOfGenomeMutations > 0) {
			if (Random.Range (0f, 1f) < numOfGenomeMutations) {
				geneLinkMutate (ref incomingGenome2, false);
			}
			numOfGenomeMutations = numOfGenomeMutations - 1f;
		}

		numOfGenomeMutations = incomingGenome2.genomeBiasChance;
		while (numOfGenomeMutations > 0) {
			if (Random.Range (0f, 1f) < numOfGenomeMutations) {
				geneLinkMutate (ref incomingGenome2, true);
			}
			numOfGenomeMutations = numOfGenomeMutations - 1f;
		}

		numOfGenomeMutations = incomingGenome2.genomeNodeChance;
		while (numOfGenomeMutations > 0) {
			if (Random.Range (0f, 1f) < numOfGenomeMutations) {
				nodeMutate (ref incomingGenome2);
			}
			numOfGenomeMutations = numOfGenomeMutations - 1f;
		}

		numOfGenomeMutations = incomingGenome2.genomeEnableChance;
		while (numOfGenomeMutations > 0) {
			if (Random.Range (0f, 1f) < numOfGenomeMutations) {
				disableEnableMutate (ref incomingGenome2, true);
			}
			numOfGenomeMutations = numOfGenomeMutations - 1f;
		}

		numOfGenomeMutations = incomingGenome2.genomeDisableChance;
		while (numOfGenomeMutations > 0) {
			if (Random.Range (0f, 1f) < numOfGenomeMutations) {
				disableEnableMutate (ref incomingGenome2, false);
			}
			numOfGenomeMutations = numOfGenomeMutations - 1f;
		}

	}

	//A method to show the number of disjointed genomeGenes between two sets of genomeGenes.
	public float disjointGenes (List<Gene> genes1, List<Gene> genes2)
	{

		//Getting the correct length to make the bool arrays later based on the bigger
		int arrayLength = 0;

		//Disjoint genomeGenes don't go beyond excess so this is important.
		int genes1Length = 0;
		int genes2Length = 0;
		genes1Length = genes1.Count;
		genes2Length = genes2.Count;

		//Find the length to make the arrays.
		for (int counter = 0; counter < genes1.Count; counter++) {
			if (genes1 [counter].innovation > arrayLength) {
				arrayLength = genes1 [counter].innovation;
			}
		}
		for (int counter = 0; counter < genes2.Count; counter++) {
			if (genes2 [counter].innovation > arrayLength) {
				arrayLength = genes2 [counter].innovation;
			}
		}

		//Two bool arrays to show which gene sets have which genomeGenes, true means the gene is present.
		bool[] disjointedGenes1 = new bool[arrayLength + 1];
		bool[] disjointedGenes2 = new bool[arrayLength + 1];

		//Setting the values of the two bool arrays to true for the innovation numbers of the incoming gene lists.
		for (int counter = 0; counter < genes1.Count; counter++) {
			disjointedGenes1 [genes1 [counter].innovation] = true;
		}
		for (int counter = 0; counter < genes2.Count; counter++) {
			disjointedGenes2 [genes2 [counter].innovation] = true;
		}

		//The number of disjoints.
		int disjointsNum = 0;

		//Use the shorter value to not take into account excessGenes
		if (genes1Length > genes2Length) {
			genes1Length = genes2Length;
		}

		//Go through the incoming Gene lists again and compare them to the opposing bool array for anything not true and increment disjointsNum if so to show disjoints present.
		for (int counter = 0; counter < disjointedGenes1.Length; counter++) {

			if (disjointedGenes1 [counter] != disjointedGenes2 [counter]) {
				disjointsNum = disjointsNum + 1;
			}
		}

		//Get the max length of the longer list and return the disjoint number divided by the length.
		int maxLength = Mathf.Max (genes1.Count, genes2.Count);

		//Return the value of the disjointed genomeGenes by deltaDisjoint.
		//Don't divide by maxLength if it's 0 or less than 20.
		if (maxLength == 0) {
			return 0;
		} else {

			if (maxLength < 20) {
				return disjointsNum * deltaDisjoint;
			} else {
				return (disjointsNum * deltaDisjoint) / maxLength;
			}
		}
	}

	//A method to show the overall difference in weight between two lists of genomeGenes.
	public float weightDifference (List<Gene> genes1, List<Gene> genes2)
	{

		//Getting the correct length to make the Gene array later.
		int arrayLength = 0;
		for (int counter = 0; counter < genes2.Count; counter++) {
			if (genes2 [counter].innovation > arrayLength) {
				arrayLength = genes2 [counter].innovation;
			}
		}
		for (int counter = 0; counter < genes1.Count; counter++) {
			if (genes1 [counter].innovation > arrayLength) {
				arrayLength = genes1 [counter].innovation;
			}
		}

		//The gene array for genomeGenes from the second incoming gene list
		Gene[] geneArray = new Gene[arrayLength + 1];

		//Temporary Gene object to be used later.
		Gene tempGene = new Gene ();

		//Filling it out with null values to be sure.
		for (int counter = 0; counter < geneArray.Length; counter++) {
			geneArray [counter] = null;
		}

		//Placing the genomeGenes in their correct place according to their innovation number.
		for (int counter = 0; counter < genes2.Count; counter++) {
			tempGene = new Gene ();
			tempGene.geneFullCopy (genes2 [counter]);
			geneArray [genes2 [counter].innovation] = tempGene;
		}

		//Two numbers to store the weight value and the num of similiar genomeGenes.
		float totalNum = 0f;
		float similiarNum = 0f;

		//A loop to add the absoulte value difference between two gene with the same innovation numbner.
		for (int counter = 0; counter < genes1.Count; counter++) {
			if (geneArray [genes1 [counter].innovation] != null) {

				totalNum = totalNum + Mathf.Abs (genes1 [counter].weight - geneArray [genes1 [counter].innovation].weight);
				similiarNum = similiarNum + 1;
			}
		}

		//Returning the value/similarties.
		if (totalNum == 0) {
			return 0;
		} else {
			return totalNum / similiarNum;
		}


	}

	//A method to show whether two speciesGenomes would be considered to be from the same poolSpecies.
	public bool sameSpecies (Genome genome1, Genome genome2)
	{

		//Gather the values of disjoints, weight difference and excesses by their delta values and see if together they are less than the threshold.
		float disjoints0 = disjointGenes (genome1.genomeGenes, genome2.genomeGenes);
		float weights0 = deltaWeight * weightDifference (genome1.genomeGenes, genome2.genomeGenes);
		float excess0 = excessGenes (genome1.genomeGenes, genome2.genomeGenes);

		return disjoints0 + weights0 + excess0 < deltaThreshold;
	}

	//A method to calculate all the genome's adjusted fitness based againist who else is in their poolSpecies. This is the Explict fitness sharing.
	public void calculateAdjustedFitness ()
	{

		//Variables to hold a temporary value for fitness and the current genome being evaluated.
		int genomeBeingEvaluated = 0;
		int tempFitnessNum = 0;

		//For each poolSpecies:
		for (int counter1 = 0; counter1 < mainPool.poolSpecies.Count; counter1++) {

			//For each genome:
			for (int counter2 = 0; counter2 < mainPool.poolSpecies [counter1].speciesGenomes.Count; counter2++) {

				//Set which genome is being evaluated.
				genomeBeingEvaluated = counter2;

				if (mainPool.poolSpecies [counter1].speciesGenomes.Count == 0) {
					//This case shopuld not happen, just in case this check is here.
				} else if (mainPool.poolSpecies [counter1].speciesGenomes.Count == 1) {
					//No other comparsion to be made, AF and F are same.
					mainPool.poolSpecies [counter1].speciesGenomes [genomeBeingEvaluated].adjustedFitness = mainPool.poolSpecies [counter1].speciesGenomes [genomeBeingEvaluated].fitness;
				} else {

					//Run through all speciesGenomes in poolSpecies to compare.
					for (int counter3 = 0; counter3 < mainPool.poolSpecies [counter1].speciesGenomes.Count; counter3++) {

						//Do not compare againist self
						if (counter3 != genomeBeingEvaluated) {

							//If not same poolSpecies then increase tempFitnessNum by one.
							if (!sameSpecies (mainPool.poolSpecies [counter1].speciesGenomes [genomeBeingEvaluated], mainPool.poolSpecies [counter1].speciesGenomes [counter3])) {
								tempFitnessNum = tempFitnessNum + 1;
							}
						}
					}

					//Cannot divide by zero and all speciesGenomes are under the threshold, else set the genome AF to F/tempFitnessNum.
					if (tempFitnessNum == 0) {
						mainPool.poolSpecies [counter1].speciesGenomes [genomeBeingEvaluated].adjustedFitness = mainPool.poolSpecies [counter1].speciesGenomes [genomeBeingEvaluated].fitness;
					} else {
						mainPool.poolSpecies [counter1].speciesGenomes [genomeBeingEvaluated].adjustedFitness = mainPool.poolSpecies [counter1].speciesGenomes [genomeBeingEvaluated].fitness / tempFitnessNum;
					}
				}
			}
		}
	}

	//A method to rank speciesGenomes againist each other from all poolSpecies from the incoming pool.
	public void rankGenomes ()
	{

		//The number of speciesGenomes that exist and to be used for creating further arrays.
		int numOfGenomes = 0;

		//A loop to count how many speciesGenomes actually exist.
		for (int counter1 = 0; counter1 < mainPool.poolSpecies.Count; counter1++) {
			numOfGenomes = numOfGenomes + mainPool.poolSpecies [counter1].speciesGenomes.Count;
		}

		//Some arrays to keep track of what speciesGenomes come from what poolSpecies as they are sorted and ranked.
		int[] genomeNumber = new int[numOfGenomes];
		int[] speciesNumber = new int[numOfGenomes];
		int[] fitnessNumber = new int[numOfGenomes];
		int genomeNum = 0;

		//A loop to gether all of the speciesGenomes that exist's place and fitness score.
		for (int counter1 = 0; counter1 < mainPool.poolSpecies.Count; counter1++) {
			for (int counter2 = 0; counter2 < mainPool.poolSpecies [counter1].speciesGenomes.Count; counter2++) {

				genomeNumber [genomeNum] = counter2;
				speciesNumber [genomeNum] = counter1;
				fitnessNumber [genomeNum] = mainPool.poolSpecies [counter1].speciesGenomes [counter2].adjustedFitness;

				genomeNum = genomeNum + 1;
			}
		}

		//A bubble sort to sort all of the speciesGenomes according to their fitness.
		bool swapped = true;
		int tempInt = 0;
		while (swapped) {

			swapped = false;

			for (int counter = 0; counter < fitnessNumber.Length - 1; counter++) {

				int tempIntY = counter + 1;

				if (fitnessNumber [counter] < fitnessNumber [tempIntY]) {


					tempInt = fitnessNumber [counter];
					fitnessNumber [counter] = fitnessNumber [tempIntY];
					fitnessNumber [tempIntY] = tempInt;

					tempInt = genomeNumber [counter];
					genomeNumber [counter] = genomeNumber [tempIntY];
					genomeNumber [tempIntY] = tempInt;

					tempInt = speciesNumber [counter];
					speciesNumber [counter] = speciesNumber [tempIntY];
					speciesNumber [tempIntY] = tempInt;

					swapped = true;
				}
			}
		}

		//Setting the Highest rank to assign to be 1 less than the population number.
		int globalRankScore = (int)population - 1;

		//One last loop to assign all of the speciesGenomes their new rank.
		for (int counter = 0; counter < fitnessNumber.Length; counter++) {
			mainPool.poolSpecies [speciesNumber [counter]].speciesGenomes [genomeNumber [counter]].globalRank = globalRankScore;
			globalRankScore = globalRankScore - 1;
		}

	}

	//A method to calculate the average fitness of a poolSpecies by getting the global rank value of all speciesGenomes divided by the number of speciesGenomes.
	public void calculateAverageFitness (ref Species incomingSpecies)
	{

		//Temp float to store the calculated average fitness.
		int tempInt = 0;

		//Loop to add all of the globalRank scores.
		for (int counter = 0; counter < incomingSpecies.speciesGenomes.Count; counter++) {
			tempInt = tempInt + incomingSpecies.speciesGenomes [counter].globalRank;
		}

		//Setting the poolSpecies averageFitness.
		incomingSpecies.averageFitness = tempInt / incomingSpecies.speciesGenomes.Count;
	}

	//A method to calculate the average fitness of a Pool.
	public float totalAverageFitness ()
	{

		//Temp float to store the calculated average fitness.
		float tempFloat = 0f;

		//A loop to get and add together all of the poolSpecies's averageFitness.
		for (int counter = 0; counter < mainPool.poolSpecies.Count; counter++) {
			tempFloat = tempFloat + mainPool.poolSpecies [counter].averageFitness;
		}

		//Return the average.
		return tempFloat;

	}

	//A method to remove speciesGenomes of their poolSpecies to half their size or down to only one depending on the value of the passed in bool.
	public void cullGenomesInSpecies (bool downToOne)
	{

		//Temporary Genomes to be used in the bubble sort.
		Genome tempGenome1 = new Genome ();
		Genome tempGenome2 = new Genome ();

		//A loop that will go through every poolSpecies in turn.
		for (int counter1 = 0; counter1 < mainPool.poolSpecies.Count; counter1++) {

			//Sort speciesGenomes according to their fitness by bubblesort.
			bool swapped = true;

			while (swapped) {

				swapped = false;

				for (int counter2 = 0; counter2 < mainPool.poolSpecies [counter1].speciesGenomes.Count - 1; counter2++) {

					int tempIntX = counter2 + 1;

					if (mainPool.poolSpecies [counter1].speciesGenomes [counter2].adjustedFitness < mainPool.poolSpecies [counter1].speciesGenomes [tempIntX].adjustedFitness) {

						tempGenome1 = new Genome ();
						tempGenome2 = new Genome ();

						tempGenome1.genomeFullCopy (mainPool.poolSpecies [counter1].speciesGenomes [counter2]);
						tempGenome2.genomeFullCopy (mainPool.poolSpecies [counter1].speciesGenomes [tempIntX]);

						mainPool.poolSpecies [counter1].speciesGenomes [tempIntX].genomeFullCopy (tempGenome1);
						mainPool.poolSpecies [counter1].speciesGenomes [counter2].genomeFullCopy (tempGenome2);

						swapped = true;
					}
				}
			}

			//Get the value for half or greater of the current speciesGenomes in the poolSpecies as on int.
			int genomesLeft = (int)Mathf.Ceil ((float)mainPool.poolSpecies [counter1].speciesGenomes.Count / 2);

			//If bool is true reduce to one.
			if (downToOne) {
				genomesLeft = 1;
			}

			//Keep removing speciesGenomes from the end of the list until the correct number are left.
			while (mainPool.poolSpecies [counter1].speciesGenomes.Count > genomesLeft) {
				mainPool.poolSpecies [counter1].speciesGenomes.RemoveAt (mainPool.poolSpecies [counter1].speciesGenomes.Count - 1);
			}
		}
	}

	//A method to generate a new genome from an referenced Species by either crossover or duplication and then mutating further and returning the new genome.
	public Genome breedChildGenome (Species incomingSpecies)
	{

		//Temporary genome variables.
		Genome child = new Genome ();
		Genome tempG1 = new Genome ();
		Genome tempG2 = new Genome ();

		//If else statement to see whether the new genome will be generated by crossover or duplication by the crossoverChance variable.
		if (Random.Range (0f, 1f) < crossoverChance) {
			tempG1 = incomingSpecies.speciesGenomes [Random.Range (0, incomingSpecies.speciesGenomes.Count)];
			tempG2 = incomingSpecies.speciesGenomes [Random.Range (0, incomingSpecies.speciesGenomes.Count)];
			child = genomeCrossover (tempG1, tempG2);

		} else {
			tempG1 = incomingSpecies.speciesGenomes [Random.Range (0, incomingSpecies.speciesGenomes.Count)];
			child.genomePartialCopy (tempG1);
		}

		//Mutate the new child genome and return it.
		mutate (ref child);
		return child;
	}

	//A method to remove poolSpecies from a pool that fail to improve enough over an alotted period of time.
	public void removeStaleSpecies ()
	{

		//A new list of poolSpecies that will become the pool's poolSpecies list after revising the current list.
		List<Species> revisedSpecies = new List<Species> ();

		//Temporary Genome and Species objects.
		Genome tempGenome1 = new Genome ();
		Genome tempGenome2 = new Genome ();
		Species tempSpecies = new Species ();

		//The loop that will go through every poolSpecies in turn.
		for (int counter1 = 0; counter1 < mainPool.poolSpecies.Count; counter1++) {

			//Sort speciesGenomes according to their fitness by bubblesort.
			bool swapped = true;
			while (swapped) {

				swapped = false;

				for (int counter2 = 0; counter2 < mainPool.poolSpecies [counter1].speciesGenomes.Count - 1; counter2++) {

					int tempIntJ = counter2 + 1;

					if (mainPool.poolSpecies [counter1].speciesGenomes [counter2].adjustedFitness < mainPool.poolSpecies [counter1].speciesGenomes [tempIntJ].adjustedFitness) {

						tempGenome1 = new Genome ();
						tempGenome2 = new Genome ();

						tempGenome1.genomeFullCopy (mainPool.poolSpecies [counter1].speciesGenomes [counter2]);
						tempGenome2.genomeFullCopy (mainPool.poolSpecies [counter1].speciesGenomes [tempIntJ]);

						mainPool.poolSpecies [counter1].speciesGenomes [tempIntJ].genomeFullCopy (tempGenome1);
						mainPool.poolSpecies [counter1].speciesGenomes [counter2].genomeFullCopy (tempGenome2);

						swapped = true;
					}
				}
			}

			//If a poolSpecies best scoring genome has beaten the previous best score then update the top score and reduce the staleness to 0;
			//If not then increment the staleness value.
			if (mainPool.poolSpecies [counter1].speciesGenomes [0].adjustedFitness > mainPool.poolSpecies [counter1].topFitness) {

				mainPool.poolSpecies [counter1].topFitness = mainPool.poolSpecies [counter1].speciesGenomes [0].adjustedFitness;
				mainPool.poolSpecies [counter1].staleness = 0;
			} else {
				mainPool.poolSpecies [counter1].staleness = mainPool.poolSpecies [counter1].staleness + 1;
			}

			//If a poolSpecies staleness has not passed the threshold or if the poolSpecies's fitness is higher than the pool's overall max fitness then add the poolSpecies back to the revised list to keep.
			//Else remove it by not addiding it back in.
			if (mainPool.poolSpecies [counter1].staleness < staleSpecies || mainPool.poolSpecies [counter1].topFitness >= mainPool.poolTopFitness) {

				tempSpecies = new Species ();
				tempSpecies.speciesFullCopy (mainPool.poolSpecies [counter1]);
				revisedSpecies.Add (tempSpecies);
			}

		}

		//Assign the revised list of poolSpecies to the pool.
		mainPool.poolSpecies = revisedSpecies;
	}

	//A method to remove poolSpecies from the pool that's average fitness that is less than the average of the rest of the poolSpecies in the pool.
	public void removeWeakSpecies ()
	{

		//A new list of poolSpecies that will become the pool's poolSpecies list after revising the current list.
		List<Species> revisedSpecies = new List<Species> ();
		Species tempSpecies = new Species ();

		//Two variables that hold the values determining whether or not a poolSpecies carries on: the average fitness of the pool and the breeding score of a poolSpecies.
		float tempFloat = totalAverageFitness ();
		int toBreed = 0;

		//A loop that goes through every poolSpecies and calculates whether or not the poolSpecies is strong enough to survive and if it is it is added to the revises list of poolSpecies.
		for (int counter1 = 0; counter1 < mainPool.poolSpecies.Count; counter1++) {

			toBreed = Mathf.FloorToInt ((float)mainPool.poolSpecies [counter1].averageFitness / tempFloat * (float)population);

			if (toBreed >= 1) {
				tempSpecies = new Species ();
				tempSpecies.speciesFullCopy (mainPool.poolSpecies [counter1]);
				revisedSpecies.Add (tempSpecies);
			}
		}

		//Reassigning the new list to the pool's list.
		mainPool.poolSpecies = revisedSpecies;
	}

	//A method to take a genome and add it to a fitting poolSpecies and if one does not exist then create a poolSpecies for that new genome.
	public void addToSpecies (Genome incomingGenome)
	{

		//Variable to check whether a suitable poolSpecies has been found.
		bool speciesFound = false;

		//Variable to remember which random genome represent's their poolSpecies.
		int randomGenomeRep = 0;

		//Temporary Genome Objects.
		Genome tempGenome = new Genome ();
		tempGenome.genomeFullCopy (incomingGenome);

		//Search for a suitable poolSpecies to add the genome to.
		for (int counter = 0; counter < mainPool.poolSpecies.Count; counter++) {

			//Pick a random genome in the poolSpecies to represent it.
			randomGenomeRep = Random.Range (0, mainPool.poolSpecies [counter].speciesGenomes.Count);

			//If a suitable poolSpecies is found add that genome to that poolSpecies.
			if (!speciesFound && sameSpecies (incomingGenome, mainPool.poolSpecies [counter].speciesGenomes [randomGenomeRep])) {
				mainPool.poolSpecies [counter].speciesGenomes.Add (tempGenome);
				speciesFound = true;
				break;
			}
		}

		//If a suitable poolSpecies was not found then create a poolSpecies to add the genome to.
		if (!speciesFound) {
			Species newSpecies = new Species ();
			newSpecies.speciesGenomes.Add (tempGenome);
			mainPool.poolSpecies.Add (newSpecies);
		}
	}

	//A method to create the next generation of a Pool.
	public void newGeneration ()
	{

		//Get the adjusted fitness for all of the speciesGenomes that exist within the pool.
		calculateAdjustedFitness ();

		//Remove the bottom half of each poolSpecies.
		cullGenomesInSpecies (false);

		//Assign the global rank to all speciesGenomes.
		rankGenomes ();

		//Remove any poolSpecies have become too stale.
		removeStaleSpecies ();

		//Calculate the average fitness of all remaining poolSpecies.
		for (int counter = 0; counter < mainPool.poolSpecies.Count; counter++) {

			Species tempSpecies1 = new Species ();
			tempSpecies1.speciesFullCopy (mainPool.poolSpecies [counter]);
			calculateAverageFitness (ref tempSpecies1);
			mainPool.poolSpecies [counter].speciesFullCopy (tempSpecies1);
		}

		//Assign the global rank to all speciesGenomes.
		rankGenomes ();

		//Remove any poolSpecies that is weak compared to the other poolSpecies that exist.
		removeWeakSpecies ();

		//Temporary variable to store the average fitness of the pool.
		float tAFNum = totalAverageFitness ();

		//A list of all the newly generated speciesGenomes.
		List<Genome> newChildrenGenomes = new List<Genome> ();

		//Tempory variable to store how many times a poolSpecies gets to breed new speciesGenomes.
		int breedScore = 0;

		//A loop to go through each poolSpecies and depending on their breed score generate new speciesGenomes.
		for (int counter1 = 0; counter1 < mainPool.poolSpecies.Count; counter1++) {

			breedScore = ((int)(Mathf.Floor (mainPool.poolSpecies [counter1].averageFitness / tAFNum * population))) - 1;

			for (int counter2 = 0; counter2 < breedScore; counter2++) {
				Genome tempGenome = new Genome ();
				tempGenome = breedChildGenome (mainPool.poolSpecies [counter1]);
				newChildrenGenomes.Add (tempGenome);
			}
		}

		//Remove all but the top scoring Genome from each poolSpecies.
		cullGenomesInSpecies (true);

		//While there are less speciesGenomes than the population value, generate new speciesGenomes.
		while (newChildrenGenomes.Count + mainPool.poolSpecies.Count < population) {

			//Select a random poolSpecies to generate a new genome form.
			Species tempSpecies = new Species ();
			Genome tempGenome = new Genome ();
			tempSpecies = mainPool.poolSpecies [Random.Range (0, mainPool.poolSpecies.Count - 1)];
			tempGenome = breedChildGenome (tempSpecies);
			newChildrenGenomes.Add (tempGenome);
		}

		//Add the newly generated speciesGenomes to the Pool.
		for (int counter = 0; counter < newChildrenGenomes.Count; counter++) {
			addToSpecies (newChildrenGenomes [counter]);
		}

		//Increment the generation value of the pool to show a new generation have been made.
		mainPool.generation = mainPool.generation + 1;

		//Find the Pool's new top fitness.
		findNewPoolTopFitness ();

		//Save the current state of the poll to a file.
		savePool ();

		//Re-test the Champion Genome at the end of Generation.
		reTestChampionGenome = true;
	}

	//Evaluates the genomeNetwork, passing through the input values, altering them and outputting the resulting output button presses.
	public void evaluateNetwork (ref List<Neuron> incomingNetwork)
	{

		//Temporary Int variable to keep track of where in the incomingNetwork we are.
		int nodeNum = 0;

		//Assign the incoming values to the neurons from the inputs.
		for (int counter1 = 0; counter1 < inputDimensions; counter1++) {
			for (int counter2 = 0; counter2 < inputDimensions; counter2++) {

				incomingNetwork [nodeNum].value = inputs [counter2, counter1];
				nodeNum = nodeNum + 1;
			}

		}

		//Giving the Bias node it value manually.
		incomingNetwork [inputSize].value = 1;

		//Calculating values for all of the values for the Neurons.
		for (int counter1 = 0; counter1 < incomingNetwork.Count; counter1++) {

			//Value for the Neuron in question.
			float sum = 0f;

			//Ignore if the Neuron is null.
			if (incomingNetwork [counter1] != null) {

				//Go through all of the neurons incoming genomeGenes and calculate their value.
				for (int counter2 = 0; counter2 < incomingNetwork [counter1].incomingGenes.Count; counter2++) {

					//The incoming Gene.
					Gene incomingTemp = incomingNetwork [counter1].incomingGenes [counter2];

					//The neuron the gene is coming form
					Neuron otherTemp = incomingNetwork [incomingTemp.neuronIn];

					//Adding the values together by their weight.
					sum = sum + (incomingTemp.weight * otherTemp.value);
				}

				//If the Neuron had more than 1 incoming gene perform the sigmoid function on sum and set the result to the neurons value.
				if (incomingNetwork [counter1].incomingGenes.Count > 0) {
					incomingNetwork [counter1].value = sigmoid (sum);
				}
			}


		}

		//Looking at the output nodes and if the value is greater than 0 then set that button to true.
		for (int counter = 0; counter < outputSize; counter++) {
			if (incomingNetwork [counter + maxNodes].value > 0) {
				buttonsPressed [counter] = true;
			} else {
				buttonsPressed [counter] = false;
			}
		}
	}

	//A method to generate a new pool and populate it with new speciesGenomes and start a new run.
	public void initializePool ()
	{

		//Creating a new pool.
		mainPool = basicPool ();

		//Fill the pool with new Genomes.
		for (int counter = 0; counter < population; counter++) {
			addToSpecies (basicGenome ());
		}

		//Start a run.
		initializeRun ();
	}

	//A method to start a new run of teh NN with the current genome.
	public void initializeRun ()
	{

		//Reset variables that determine the start of a run, player progress, currentLoop, the level's timeout, and buttons pressed.
		startOfRunPos = true;
		playerProgress = 0;
		mainPool.currentLoop = 0;
		timeout = timeoutConst;
		resetButtonsPressed ();

		//Generate the genomeNetwork of the current genome to be run.
		Genome tempGG = new Genome ();
		tempGG.genomeFullCopy (mainPool.poolSpecies [mainPool.currentSpecies].speciesGenomes [mainPool.currentGenome]);
		generateNetwork (ref tempGG);
		mainPool.poolSpecies [mainPool.currentSpecies].speciesGenomes [mainPool.currentGenome].genomeFullCopy (tempGG);

		//Evaluate the current state of the genome's genomeNetwork.
		evaluateCurrent ();
	}

	//A method to evaluate the current state of the inputs through the NN and send back the outputs via button presses.
	public void evaluateCurrent ()
	{

		Genome TempGGG = mainPool.poolSpecies [mainPool.currentSpecies].speciesGenomes [mainPool.currentGenome];
		evaluateNetwork (ref TempGGG.genomeNetwork);
	}

	//Find's what the Pool's topfitness is for the Pool when called.
	public void findNewPoolTopFitness ()
	{

		for (int counter1 = 0; counter1 < mainPool.poolSpecies.Count; counter1++) {
			for (int counter2 = 0; counter2 < mainPool.poolSpecies [counter1].speciesGenomes.Count; counter2++) {
				if (mainPool.poolSpecies [counter1].speciesGenomes [counter2].fitness > mainPool.poolTopFitness) {
					mainPool.poolTopFitness = mainPool.poolSpecies [counter1].speciesGenomes [counter2].fitness;
				}
			}
		}

	}

	//A method to move onto the next genome in the pool and if reaches the end make the next generation.
	public void nextGenome ()
	{

		//Set the current genome to the next one in the list.
		mainPool.currentGenome = mainPool.currentGenome + 1;

		//If all of the speciesGenomes in a poolSpecies has been exhausted then move onto the next poolSpecies.
		if (mainPool.currentGenome >= mainPool.poolSpecies [mainPool.currentSpecies].speciesGenomes.Count) {
			mainPool.currentGenome = 0;
			mainPool.currentSpecies = mainPool.currentSpecies + 1;

			//If all of the poolSpecies have been exhausted then make a new generation of speciesGenomes.
			if (mainPool.currentSpecies >= mainPool.poolSpecies.Count) {
				newGeneration ();
				mainPool.currentSpecies = 0;
			}
		}

		//Set the variable to restart the level to true.
		restartLMLevel = true;
	}

	//A simple method to check whether the current genome has alreadly been tested.
	public bool genomeAlreadlyTested ()
	{
		return mainPool.poolSpecies [mainPool.currentSpecies].speciesGenomes [mainPool.currentGenome].fitness != 0;
	}

	//A simple method to reset the bool array for the controller inputs back to nothing being pressed.
	public void resetButtonsPressed ()
	{
		for (int counter = 0; counter < buttonsPressed.Length; counter++) {
			buttonsPressed [counter] = false;
		}
	}

	//The method the comprises the main loop and operation of the NN.
	public void mainLoop ()
	{

		//Reset tempFitness to 0 in case of a new run.
		tempFitness = 0;

		//Every run of mainLoop evaluate the current state of the NN.
		evaluateCurrent ();

		//Update UI to show its current state.
		updateDisplay (mainPool.poolSpecies [mainPool.currentSpecies].speciesGenomes [mainPool.currentGenome].genomeNetwork);

		//As before but for the genomeNetwork. Called twice to generate and color before frameUpdate.
		updateGenomeDisplay (mainPool.poolSpecies [mainPool.currentSpecies].speciesGenomes [mainPool.currentGenome].genomeNetwork);
		updateGenomeDisplay (mainPool.poolSpecies [mainPool.currentSpecies].speciesGenomes [mainPool.currentGenome].genomeNetwork);

		//Update the current progress the player has made towards the goal if he has progressed at all.
		//The +0.1 is to address a bug where the player would not move and this condition would always be true, halting the program.
		if ((playerCurrentX - playerStartingX) > (playerProgress + 0.1f)) {
			playerProgress = playerCurrentX - playerStartingX;
			timeout = timeoutConst;
		}

		//Update the values for timeout and timeoutBonus (which gets bigger the longer the player progresses.)
		timeout = timeout - 1;
		timeoutBonus = mainPool.currentLoop / 4;

		//Stop the run if any of the following reasons/bool are true.
		if (playerHasDied || endOfLevel || playTop || playChampion) {
			timeout = -1;
			timeoutBonus = -1;
		}

		//If the run has ended due to timeout or an external reason do the following:
		if (timeout + timeoutBonus <= 0) {

			//Reset the value of this bool in case the player did die.
			playerHasDied = false;

			//Calculate the fitness of the player for this run.
			tempFitness = Mathf.RoundToInt (playerProgress * fitnessMultiplier);

			//If the end of the level was reached grant a bonus to the fitness score which is bigger the faster the player did so.
			if (endOfLevel) {
				tempFitness = tempFitness + 5000 - mainPool.currentLoop / 2;
				endOfLevel = false;
			}

			//If the player did not move/progress at all set it's fitness accordingly.
			if (tempFitness <= 0) {
				tempFitness = -10;
			}

			//If the user requested to play the Pool's current top genome, set the current genome's fitness to 0 so it can be tested again and run playTopScoringGenome.
			//If the user requested to play the Pool's champion genome, set the current genome's fitness to 0 so it can be tested again and runplayChampionGenome.
			if (playTop) {
				tempFitness = 0;
				mainPool.poolSpecies [mainPool.currentSpecies].speciesGenomes [mainPool.currentGenome].fitness = 0;
				playTopScoringGenome ();

			} else if (playChampion) {

				tempFitness = 0;
				mainPool.poolSpecies [mainPool.currentSpecies].speciesGenomes [mainPool.currentGenome].fitness = 0;
				playChampionGenome ();

			} else {

				//If playTopScoringGenome was previously run reset playTopRevert.
				if (playTopRevert) {
					playTopRevert = false;
				}

				//If playTopScoringGenome was previously run swap the reset playChampionRevert and swap back swappedGenome into it's place and the generation number.
				if (playChampionRevert) {

					Genome tempGenome1 = new Genome ();
					Genome tempGenome2 = new Genome ();

					tempGenome1.genomeFullCopy (mainPool.swappedOutGenome);
					tempGenome2.genomeFullCopy (mainPool.poolSpecies [0].speciesGenomes [0]);

					mainPool.swappedOutGenome.genomeFullCopy (tempGenome2);
					mainPool.poolSpecies [0].speciesGenomes [0].genomeFullCopy (tempGenome1);

					int tempInt4 = 0;
					tempInt4 = mainPool.generation;
					mainPool.generation = mainPool.championGeneration;
					mainPool.championGeneration = tempInt4;

					//Re-evaluate the Champion Genome fitness due to a slight chance Unity Physics discrepancy.
					mainPool.championGenome.fitness = tempFitness;
					//mainPool.poolTopFitness = tempFitness;

					playChampionRevert = false;

				} else {

					//Assign new fitness score to genome / Re-evaluate the Top Genome fitness due to a slight chance Unity Physics discrepancy.
					mainPool.poolSpecies [mainPool.currentSpecies].speciesGenomes [mainPool.currentGenome].fitness = tempFitness;

					//Should the newly preformed genome be better than the Pool's best in score that Genome becomes the champion Genome.
					if (tempFitness > mainPool.championGenome.fitness) {

						Genome tempGenome2 = new Genome ();
						tempGenome2.genomeFullCopy (mainPool.poolSpecies [mainPool.currentSpecies].speciesGenomes [mainPool.currentGenome]);
						mainPool.championGenome.genomeFullCopy (tempGenome2);

						//mainPool.poolTopFitness = tempFitness;
						mainPool.championGeneration = mainPool.generation;

					}
				}

				//Set the pool's currentGenome and currentSpecies to 0 so it can find the next untested Genome.
				mainPool.currentGenome = 0;
				mainPool.currentSpecies = 0;

				//Find the next untested Genome and possibly make a new generation.
				while (genomeAlreadlyTested ()) {
					nextGenome ();
				}

				//Force a garbage collection.
				System.GC.Collect ();

				//Start a new run with the next Genome to test.
				initializeRun ();

				//Set this to true to wipe and replace the GenomeUI.
				restartGenomeDisplay = true;
			}

		}

		if (reTestChampionGenome) {
			//Re-evaluate the Champion Genome fitness due to a slight chance Unity Physics discrepancy.
			Debug.Log ("Re-testing Champion Genome");
			playChampionGenome ();
			reTestChampionGenome = false;
		}

		//Set this to false to show it is not the start of a run.
		startOfRunPos = false;

		//Increase the pool's currentLoop of running.
		mainPool.currentLoop = mainPool.currentLoop + 1;
	}

	//A method to force a run of the pool's current best performed Genome.
	public void playTopScoringGenome ()
	{

		//Variables to keep track of the best genome found thus far in the pool.
		int topScoringSpecies = 0;
		int topScoringGenome = 0;
		int topScoringFitness = -20;

		//Go through all of the Genomes in the pool and keep track of the one with the best fitness.
		for (int counter1 = 0; counter1 < mainPool.poolSpecies.Count; counter1++) {
			for (int counter2 = 0; counter2 < mainPool.poolSpecies [counter1].speciesGenomes.Count; counter2++) {

				if (mainPool.poolSpecies [counter1].speciesGenomes [counter2].fitness > topScoringFitness) {
					topScoringFitness = mainPool.poolSpecies [counter1].speciesGenomes [counter2].fitness;
					topScoringGenome = counter2;
					topScoringSpecies = counter1;
				}
			}
		}

		//Set the pool's current genome to test to the top performed Genome found.
		mainPool.currentGenome = topScoringGenome;
		mainPool.currentSpecies = topScoringSpecies;

		//Tell the LM to restart the Level.
		restartLMLevel = true;

		//Start a new run.
		initializeRun ();

		//Reset the Genome display.
		restartGenomeDisplay = true;

		//Set these variables accordingly so that the run can't be inturrupted.
		playTop = false;
		playTopRevert = true;
	}

	//A method to force a run of the pool's Champion Genome.
	public void playChampionGenome ()
	{

		//Temporary Genome objects to be used for swapping.
		Genome tempGenome1 = new Genome ();
		Genome tempGenome2 = new Genome ();

		//Set the swappedOutGenome to the first genome in the pool.
		tempGenome1.genomeFullCopy (mainPool.poolSpecies [0].speciesGenomes [0]);
		mainPool.swappedOutGenome.genomeFullCopy (tempGenome1);

		//Set the first genome in the Pool to the Champion Genome.
		tempGenome2.genomeFullCopy (mainPool.championGenome);
		mainPool.poolSpecies [0].speciesGenomes [0].genomeFullCopy (tempGenome2);

		//Swap the current generation and the champions genome to show where it came from.
		int tempInt = 0;
		tempInt = mainPool.generation;
		mainPool.generation = mainPool.championGeneration;
		mainPool.championGeneration = tempInt;

		//Set the Pool's current Genome to where the champion genome was placed.
		mainPool.currentGenome = 0;
		mainPool.currentSpecies = 0;

		//Tell the LM to restart the Level.
		restartLMLevel = true;

		//Start a new run.
		initializeRun ();

		//Reset the Genome display.
		restartGenomeDisplay = true;

		//Set these variables accordingly so that the run can't be inturrupted and can be reverted.
		playChampion = false;
		playChampionRevert = true;

	}

	//A method to be called that will force a run of the Pool's top Genome and prevent it from being interrupted.
	public void nowPlayTop ()
	{
		if (!playTopRevert && !playChampionRevert) {
			timeout = -1;
			timeoutBonus = -1;
			playTop = true;
		}

	}

	//A method to be called that will force a run of the Pool's champion Genome and prevent it from being interrupted.
	public void nowPlayChampion ()
	{
		if (!playChampionRevert && !playTopRevert) {
			timeout = -1;
			timeoutBonus = -1;
			playChampion = true;
		}
	}


	//A method to perform NEAT's sigmoid calculation.
	public float sigmoid (float x)
	{
		float temp1 = -4.9f;
		float expTemp = Mathf.Exp (temp1 * x);
		return 2 / (1 + expTemp) - 1;
	}

	//A method to return the value for excess genomeGenes by it's delta weight.
	public float excessGenes (List<Gene> genes1, List<Gene> genes2)
	{
		//Variables to keep track of the length of both Gene lists.
		int longNum = 0;
		int shortNum = 0;

		//Set the long and short variables where appropriate.
		if (genes1.Count > genes2.Count) {
			longNum = genes1.Count;
			shortNum = genes2.Count;
		} else {
			longNum = genes2.Count;
			shortNum = genes1.Count;
		}

		//IF both are the same length then return 0 otherwise return the difference by the weight and if longer than 20 by the longer of the two gene lists.
		if (longNum == shortNum) {
			return 0;
		} else {
			if (shortNum < 20) {
				return (longNum - shortNum) * deltaExcess;
			} else {
				return ((longNum - shortNum) * deltaExcess) / longNum;
			}
		}
	}
}

//A custom class object to represent a Gene.
public class Gene
{
	//neuronIn AND neuronOut represent the Neuron going into the gene and out from the gene.
	public int neuronIn = 0;
	public int neuronOut = 0;
	public float weight = 0.0f;
	public int innovation = 0;
	public bool enabled = true;

	public Gene ()
	{

	}

	//A method that copies all the values of another Gene into itself.
	public void geneFullCopy (Gene gene0)
	{
		neuronIn = gene0.neuronIn;
		neuronOut = gene0.neuronOut;
		weight = gene0.weight;
		enabled = gene0.enabled;
		innovation = gene0.innovation;
	}
}

//A custom class object to represent a Neuron.
public class Neuron
{
	public float value = 0.0f;

	//A list of all of the gene that are passing it's values to it.
	public List<Gene> incomingGenes = new List<Gene> ();

	public Neuron ()
	{

	}

	//A method that copies all the values of another Neuron into itself.
	public void neuronCopyFull (Neuron incomingNeuron)
	{
		value = incomingNeuron.value;

		incomingGenes.Clear ();
		for (int counter = 0; counter < incomingNeuron.incomingGenes.Count; counter++) {
			Gene tempGene = new Gene ();
			tempGene.geneFullCopy (incomingNeuron.incomingGenes [counter]);
			incomingGenes.Add (tempGene);
		}
	}
}

//A custom class object to represent a Genome.
public class Genome
{
	public int fitness = 0;
	public int adjustedFitness = 0;

	//Best rank is the value for population; (1st) and decreases from there.
	public int globalRank = 0;

	//The chance rates of a genome's mutation in any given area.
	public float genomeGeneWeightChance = 0.35f;
	public float genomeLinkChance = 2.0f;
	public float genomeBiasChance = 0.4f;
	public float genomeNodeChance = 0.85f;
	public float genomeEnableChance = 0.2f;
	public float genomeDisableChance = 0.4f;
	public float genomeStepSize = 0.1f;

	//The Genes the Genome contains.
	public List<Gene> genomeGenes = new List<Gene> ();

	//The Genomes network.
	public List<Neuron> genomeNetwork = new List<Neuron> ();

	public Genome ()
	{

	}

	//A method that copies some of the values of another Neuron into itself.
	public void genomePartialCopy (Genome genome0)
	{
		genomeGeneWeightChance = genome0.genomeGeneWeightChance;
		genomeLinkChance = genome0.genomeLinkChance;
		genomeBiasChance = genome0.genomeBiasChance;
		genomeEnableChance = genome0.genomeEnableChance;
		genomeDisableChance = genome0.genomeDisableChance;
		genomeGenes.Clear ();
		for (int counter = 0; counter < genome0.genomeGenes.Count; counter++) {
			Gene tempGene = new Gene ();
			tempGene.geneFullCopy (genome0.genomeGenes [counter]);
			genomeGenes.Add (tempGene);
		}
	}

	//A method that copies all the values of another Genome into itself.
	public void genomeFullCopy (Genome genome0)
	{

		genomeGeneWeightChance = genome0.genomeGeneWeightChance;
		genomeLinkChance = genome0.genomeLinkChance;
		genomeBiasChance = genome0.genomeBiasChance;
		genomeEnableChance = genome0.genomeEnableChance;
		genomeDisableChance = genome0.genomeDisableChance;

		genomeGenes.Clear ();
		for (int counter = 0; counter < genome0.genomeGenes.Count; counter++) {
			Gene tempGene = new Gene ();
			tempGene.geneFullCopy (genome0.genomeGenes [counter]);
			genomeGenes.Add (tempGene);
		}

		genomeStepSize = genome0.genomeStepSize;
		fitness = genome0.fitness;
		adjustedFitness = genome0.adjustedFitness;
		globalRank = genome0.globalRank;

		genomeNetwork.Clear ();

		if (genome0.genomeNetwork.Count != 0) {
			for (int counter = 0; counter < genome0.genomeNetwork.Count; counter++) {

				if (genome0.genomeNetwork [counter] == null) {
					genomeNetwork.Add (null);
				} else {
					Neuron tempNeuron = new Neuron ();
					tempNeuron.neuronCopyFull (genome0.genomeNetwork [counter]);
					genomeNetwork.Add (tempNeuron);
				}
			}
		}
	}
}

//A custom class object to represent a Species.
public class Species
{

	public int topFitness = 0;
	public int averageFitness = 0;
	public int staleness = 0;

	//The Genomes the species contains.
	public List<Genome> speciesGenomes = new List<Genome> ();

	public Species ()
	{

	}

	//A method that copies all the values of another Species into itself.
	public void speciesFullCopy (Species incomingSpecies)
	{

		topFitness = incomingSpecies.topFitness;
		averageFitness = incomingSpecies.averageFitness;
		staleness = incomingSpecies.staleness;

		speciesGenomes.Clear ();
		for (int counter = 0; counter < incomingSpecies.speciesGenomes.Count; counter++) {
			Genome tempGenome = new Genome ();
			tempGenome.genomeFullCopy (incomingSpecies.speciesGenomes [counter]);
			speciesGenomes.Add (tempGenome);
		}
	}
}

//A custom class object to represent a Pool.
public class Pool
{

	public int generation = 0;
	public int innovation = 0;
	public int currentSpecies = 0;
	public int currentGenome = 0;
	public int currentLoop = 0;
	public int poolTopFitness = 0;

	//The species the pool contains.
	public List<Species> poolSpecies = new List<Species> ();

	public int championGeneration = 0;
	public Genome championGenome = new Genome ();
	public Genome swappedOutGenome = new Genome ();

	public Pool ()
	{

	}

	public void increaseInnovation ()
	{
		innovation = innovation + 1;
	}
}