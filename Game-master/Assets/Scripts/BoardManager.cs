//coordinates are fixed, but does that mean we're going to have problems with distances?
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

// This Script (a component of Game Manager) Initializes the Borad (i.e. screen).
public class BoardManager : MonoBehaviour
{
	// Creating itemclicks variables so 'changetonextscene' works
	public static List<Vector3> itemClicks = new List<Vector3>();

	//Resoultion width and Height
	//CAUTION! Modifying this does not modify the Screen resolution. This is related to the unit grid on Unity.
	public static int resolutionWidth = 1024;
	public static int resolutionHeight = 768;

	//A canvas where all the board is going to be placed
	private GameObject canvas;

	//The answer Input by the player
	//0:No / 1:Yes / 2:None
	public static int answer;

	// creating strings to store questions that show up in trial answer
	private String question;
	private String o1_s1q;
	private String o1_s2q;
	private String o1_s3q;
	private String o1_s4q;
	private String o1_delay;
	private String o2_s1q;
	private String o2_s2q;
	private String o2_s3q;
	private String o2_s4q;
	private String o2_delay;

	// variables to create pie charts
	public Image[] o1PieChart;
	public float[] o1PieVal;
	public Image[] o2PieChart;
	public float[] o2PieVal;

	// Creating global variables that can be called in gamemanger
	[System.NonSerialized]
	public static int randq;
	public static string i_name;

	// array to store pie values
	public float[] o1pie_array;
	public float[] o2pie_array;

	//Should the key be working?
	public static bool keysON = false;

	//If randomization of buttons:
	// Keeping randomyes so changetonextscene works
	//1: No/Yes 0: Yes/No
	public static int randomYes;//=Random.Range(0,2);

	//Functions to create pie charts
	//Function to fill in o1 pie chart
	public void o1pie(float[] valuestoset)
	{
		float totalValues = 0;
		for (int i = 0; i < o1PieChart.Length; i++)
		{
			totalValues += FindPercentage(valuestoset, i);
			o1PieChart[i].fillAmount = totalValues;
		}
	}

	public void o2pie(float[] valuestoset)
	{
		float totalValues = 0;
		for (int i = 0; i < o2PieChart.Length; i++)
		{
			totalValues += FindPercentage(valuestoset, i);
			o2PieChart[i].fillAmount = totalValues;
		}
	}

	// converts pie chart values to percentages
	private float FindPercentage(float[] valuetoset, int index)
	{
		float totalamount = 0;

		for (int i = 0; i < valuetoset.Length; i++)
		{
			totalamount += valuetoset[i];
		}
		return valuetoset[index] / totalamount;
	}

	// IDK what this does but i need it for the game to work.
	public List<Vector2> unitycoord = new List<Vector2>();

	// Function to create list of instances to be used
	public List<int> instance_list = new List<int>();

	void pick_instances()
    {
		// adding 9 base instances
		for (int i = 0; i < 9; i++)
        {
			instance_list.Add(i);
        }

		// Adding 36 delay instances (18L, 18R)
		int counter = 0;
		while (counter < 18)
		{
			// Right delay instance
			int R1 = Random.Range(9, 44);
			if (!instance_list.Contains(R1))
			{
				instance_list.Add(R1);
				counter = counter + 1;
			}
		}

		counter = 0;
		while (counter < 18)
		{
			// Left delay instance
			int R2 = Random.Range(45, 80);
			//the -36 amkes sure the corresponding instance with left manipulation isnt in the instance list already
			if (!instance_list.Contains(R2) || !instance_list.Contains(R2 - 36))
			{
				instance_list.Add(R2);
				counter = counter + 1;
			}
		}

		// Adding 45 3 state instances (23L, 22R)
		counter = 0;
		while (counter < 23)
		{
			// Right delay instance
			int R3 = Random.Range(81, 161);
			if (!instance_list.Contains(R3))
			{
				instance_list.Add(R3);
				counter = counter + 1;
			}
		}

		counter = 0;
		while(counter < 22)
		{
			// Right delay instance
			int R4 = Random.Range(243, 323);
			//the -162 amkes sure the corresponding instance with left manipulation isnt in the instance list already
			if (!instance_list.Contains(R4) || !instance_list.Contains(R4 - 162))
			{
				instance_list.Add(R4);
				counter = counter + 1;
			}
		}

		// Adding 45 4 state instances (22L, 23R)
		counter = 0;
		while (counter < 22)
		{
			// Right delay instance
			int R5 = Random.Range(162, 242);
			if (!instance_list.Contains(R5))
			{
				instance_list.Add(R5);
				counter = counter + 1;
			}
		}

		counter = 0;
		while (counter < 23)
		{
			// Right delay instance
			int R6 = Random.Range(324, 404);
			//the -162 amkes sure the corresponding instance with left manipulation isnt in the instance list already
			if (!instance_list.Contains(R6) || !instance_list.Contains(R6 - 162)) 
			{
				instance_list.Add(R6);
				counter = counter + 1;
			}
		}
	}

	// Function to draw random instances (no repeats)
	List<int> used = new List<int>();
	public int randquest()
	{
		int Rand = Random.Range(0, instance_list.Count);
		if (!used.Contains(Rand))
		{
			used.Add(Rand);
			return instance_list[Rand];
		}

		return randquest();
	}

	//Initializes the instance for this trial:
	//1. Sets the question string using the instance (from the .txt files)
	//2. The weight and value vectors are uploaded
	//3. The instance prefab is uploaded

	public void setTSPInstance()
	{
		// Pick random instance from instance_list
		randq = randquest();

		// Saving lottery name as variable
		i_name = GameManager.tspinstances[randq].name;

		// Creating lottery states
		o1_s1q = 100 * GameManager.tspinstances[randq].o1probs[0] + "% chance for $" + GameManager.tspinstances[randq].o1rewards[0];
		o1_s2q = 100 * GameManager.tspinstances[randq].o1probs[1] + "% chance for $" + GameManager.tspinstances[randq].o1rewards[1];
		o1_s3q = 100 * GameManager.tspinstances[randq].o1probs[2] + "% chance for $" + GameManager.tspinstances[randq].o1rewards[2];
		o1_s4q = 100 * GameManager.tspinstances[randq].o1probs[3] + "% chance for $" + GameManager.tspinstances[randq].o1rewards[3];
		o1_delay = "Rewards will be given in " + GameManager.tspinstances[randq].o1days[0] + " days";

		o2_s1q = 100 * GameManager.tspinstances[randq].o2probs[0] + "% chance for $" + GameManager.tspinstances[randq].o2rewards[0];
		o2_s2q = 100 * GameManager.tspinstances[randq].o2probs[1] + "% chance for $" + GameManager.tspinstances[randq].o2rewards[1];
		o2_s3q = 100 * GameManager.tspinstances[randq].o2probs[2] + "% chance for $" + GameManager.tspinstances[randq].o2rewards[2];
		o2_s4q = 100 * GameManager.tspinstances[randq].o2probs[3] + "% chance for $" + GameManager.tspinstances[randq].o2rewards[3];
		o2_delay = "Rewards will be given in " + GameManager.tspinstances[randq].o2days[0] + " days";

		// CVahnging delay to today if delay = 0
		if (GameManager.tspinstances[randq].o1days[0] == 0)
		{
			o1_delay = "Rewards are given today";
		}
		if (GameManager.tspinstances[randq].o2days[0] == 0)
		{
			o2_delay = "Rewards are given today";
		}

		// Finding the placeholder gameobjects
		Text o1s1 = GameObject.Find("o1 state 1 txt").GetComponent<Text>();
		Text o1s2 = GameObject.Find("o1 state 2 txt").GetComponent<Text>();
		Text o1s3 = GameObject.Find("o1 state 3 txt").GetComponent<Text>();
		Text o1s4 = GameObject.Find("o1 state 4 txt").GetComponent<Text>();
		Text o1delay = GameObject.Find("o1 delay").GetComponent<Text>();

		Text o2s1 = GameObject.Find("o2 state 1 txt").GetComponent<Text>();
		Text o2s2 = GameObject.Find("o2 state 2 txt").GetComponent<Text>();
		Text o2s3 = GameObject.Find("o2 state 3 txt").GetComponent<Text>();
		Text o2s4 = GameObject.Find("o2 state 4 txt").GetComponent<Text>();
		Text o2delay = GameObject.Find("o2 delay").GetComponent<Text>();

		// Assigning text to the placeholders
		o1s1.text = o1_s1q;
		o1s2.text = o1_s2q;
		o1s3.text = o1_s3q;
		o1s4.text = o1_s4q;
		o1delay.text = o1_delay;
		o2s1.text = o2_s1q;
		o2s2.text = o2_s2q;
		o2s3.text = o2_s3q;
		o2s4.text = o2_s4q;
		o2delay.text = o2_delay;

		// Making text not active if rewards =0
		if (GameManager.tspinstances[randq].o1rewards[2] == 0)
		{
			GameObject.Find("o1 state 3 txt").SetActive(false);
			GameObject.Find("o1 state3 colour").SetActive(false);
		}

		if (GameManager.tspinstances[randq].o1rewards[3] == 0)
		{
			GameObject.Find("o1 state 4 txt").SetActive(false);
			GameObject.Find("o1 state4 colour").SetActive(false);
		}
		if (GameManager.tspinstances[randq].o2rewards[2] == 0)
		{
			GameObject.Find("o2 state 3 txt").SetActive(false);
			GameObject.Find("o2 state3 colour").SetActive(false);
		}
		if (GameManager.tspinstances[randq].o2rewards[3] == 0)
		{
			GameObject.Find("o2 state 4 txt").SetActive(false);
			GameObject.Find("o2 state4 colour").SetActive(false);
		}
	}

	// Creating pie charts, hope it works this time
	void piecharts()
	{
		// Feeding values to pie charts
		o1pie(o1PieVal);
		o2pie(o2PieVal);

		float[] o1pie_array = { GameManager.tspinstances[randq].o1probs[0], GameManager.tspinstances[randq].o1probs[1],
			GameManager.tspinstances[randq].o1probs[2], GameManager.tspinstances[randq].o1probs[3] };

		float[] o2pie_array = { GameManager.tspinstances[randq].o2probs[0], GameManager.tspinstances[randq].o2probs[1],
			GameManager.tspinstances[randq].o2probs[2], GameManager.tspinstances[randq].o2probs[3] };

		o1pie(o1pie_array);
		o2pie(o2pie_array);
	}

	/// Macro function that initializes the Board
	public void SetupScene(string sceneToSetup)
	{
		if (sceneToSetup == "LotteryTrial")
		{
			keysON = true;
			answer = 0;
			//Debug.Log("answer reset");

			// load TSPinstances
			setTSPInstance();
			Debug.Log("setTSPInstance");

			//			InitialiseList ();
			//			seeGrid();
		}

	}

	//Updates the timer rectangle size accoriding to the remaining time.
	public void updateTimer()
	{
		// timer = GameObject.Find ("Timer").GetComponent<RectTransform> ();
		// timer.sizeDelta = new Vector2 (timerWidth * (GameManager.tiempo / GameManager.totalTime), timer.rect.height);
		if (GameManager.escena != "SetUp" || GameManager.escena == "InterTrialRest" || GameManager.escena == "End")
		{
			Image timer = GameObject.Find("Timer").GetComponent<Image>();
			timer.fillAmount = GameManager.tiempo / GameManager.totalTime;
		}
	}

	//Sets the triggers for pressing the corresponding keys
	//123: Perhaps a good practice thing to do would be to create a "close scene" function that takes as parameter the answer and closes everything (including keysON=false) and then forwards to 
	//changeToNextScene(answer) on game manager
	//necessary: this was imported from decision version
	private void setKeyInput()
	{
		if (GameManager.escena == "LotteryTrial")
		{
			//1: No/Yes 0: Yes/No
			if (Input.GetKeyDown(KeyCode.LeftArrow))
			{
				//Left = option 1
				//GameManager.changeToNextScene (0, randomYes);
				keysON = false;
				answer = 1;
				GameObject boto = GameObject.Find("LEFTbutton") as GameObject;
				highlightButton(boto);
				GameManager.setTimeStamp();
				GameManager.changeToNextScene(itemClicks, 0, 1);
			}
			else if (Input.GetKeyDown(KeyCode.RightArrow))
			{
				//Right = option 2
				//GameManager.changeToNextScene (1, randomYes);
				keysON = false;
				answer = 2;
				GameObject boto = GameObject.Find("RIGHTbutton") as GameObject;
				highlightButton(boto);
				GameManager.setTimeStamp();
				GameManager.changeToNextScene(itemClicks, 1, 1);
			}

		}
		else if (GameManager.escena == "SetUp")
		{
			if (Input.GetKeyDown(KeyCode.Space))
			{
				//GameManager.setTimeStamp();
				//GameManager.changeToNextScene(itemClicks, 0, 0);
			}
		}
	}

	private void highlightButton(GameObject butt)
	{
		Text texto = butt.GetComponentInChildren<Text>();
		texto.color = Color.gray;
	}

	// This is the setup screen
	public void setupInitialScreen()
	{
		//Button 
		GameObject start = GameObject.Find("Start") as GameObject;
		start.SetActive(false);

		//start.btnLeft.GetComponentInChildren<Text>().text = "No";
		InputField pID = GameObject.Find("ParticipantID").GetComponent<InputField>();

		InputField.SubmitEvent se = new InputField.SubmitEvent();
		//se.AddListener(submitPID(start));

		// This adds something so that afer filling in input, the start button is pressed.
		se.AddListener((value) => submitPID(value, start));
		pID.onEndEdit = se;
	}

	// crating fucntion for start button
	public void start_button()
	{
		// Create instance list when start button is clicked
		pick_instances();
		Debug.Log("instance len " + instance_list.Count());

		/*foreach (var x in instance_list)
		{ 
			Debug.Log("ii_list" + x.ToString());
		}*/

		// after clicking start button, move to game
		GameManager.setTimeStamp();
		GameManager.changeToNextScene(itemClicks, 0, 0);
	}

	// creating function to ask for participant ID before start button appears
	private void submitPID(string pIDs, GameObject start)
	{
		GameObject pID = GameObject.Find("ParticipantID");
		GameObject pIDT = GameObject.Find("Participant ID Text");
		pID.SetActive(true);
		pIDT.SetActive(true);

		//Set Participant ID
		GameManager.participantID = pIDs;

		//Activate Start Button and listener
		start.SetActive(true);
		keysON = true;

	}

	// Use this for initialization
	void Start()
	{
		// Updating pie charts in start(), hope this works.
		piecharts();
	}

	// Update is called once per frame
	void Update()
	{
		if (keysON)
		{
			setKeyInput();
		}

	}
}