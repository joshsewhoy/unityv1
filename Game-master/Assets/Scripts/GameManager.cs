using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using System.Linq;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour 
{

	//Game Manager: It is a singleton (i.e. it is always one and the same it is nor destroyed nor duplicated)	
	public static GameManager instance=null;

	//The reference to the script managing the board (interface/canvas).
	private BoardManager boardScript;

	//Current Scene
	public static string escena;

	//Time spent so far on this scene
	public static float tiempo;

	//Total time for this scene
	public static float totalTime;

	//Current trial initialization
	public static int trial = 0;

	//The total number of trials across all blocks
	public static int TotalTrial = 135;

	//Current block initialization
	public static int block = 0;

	private static bool showTimer;

	//Intertrial rest time
	public static float timeRest1;

	//The times listed are default settings. They are over-ridden by input files, so you need to change the input files to change times
	//InterBlock rest time
	public static float timeRest2;

	//Time given for each trial (The total time the items are shown -With and without the question-)
	public static float timeTrial;

	//Time for seeing the TSP question 
	public static float timeQuestion;

	//Time given for answering 
	public static float timeAnswer;

	//Total number of trials in each block
	private static int numberOfTrials;

	//Total number of blocks
	private static int numberOfBlocks;

	//This is also taken from input files, so in reality 63 instance files are loaded, not 3
	//Number of instance files to be considered. From i1.txt to i_.txt..
	public static int numberOfInstances;

	//The order of the instances to be presented
	public static int[] instanceRandomization;

	//Setting up the variable participantID
	//This is the string that will be used as the file name where the data is stored. DeCurrently the date-time is used.
	public static string participantID = "Empty";

	public static string dateID = @System.DateTime.Now.ToString("dd MMMM, yyyy, HH-mm");

	private static string identifierName;

	//Input and Outout Folders with respect to the Application.dataPath;
	private static string inputFolder = "/DATAinf/Input/";
	private static string outputFolder = "/DATAinf/Output/";
	private static string inputFolderTSPInstances = "/DATAinf/Input/TSPInstances/";

	private static string inputFolderBaseInstances = "/DATAinf/Input/BaseInstances/";
	private static string inputFolderDelayInstances = "/DATAinf/Input/DelayInstances/";
	private static string inputFolderStateInstances = "/DATAinf/Input/StateInstances/";

	//Can copy this code if time stamps are needed (likely) Stopwatch to calculate time of events.
	private static System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
	// Time at which the stopwatch started. Time of each event is calculated according to this moment.
	private static string initialTimeStamp;

	//The order of the left/right No/Yes randomization
	public static int[] buttonRandomization;

	//we would need a vector of literals, corresponding to 3 literals for each clause in that instance
	//also need a vector of variables for each instance, to see where the literals are being drawn from
	//changes needed further along due to changes in struct
	//A structure that contains the parameters of each instance
	public struct TSPInstance
	{
		public string name;
		public int[] o1rewards;
		public float[] o1probs;
		public int[] o1days;
		public int[] o2rewards;
		public float[] o2probs;
		public int[] o2days;

		public string id;
		public string type;
	}

	// Variables to record participants answer time
	public static string ans_start;
	public static string ans_end;

	//distance travelled
	public static int Distancetravelled;


	//An array of all the instances to be uploaded from .txt files, i.e importing everything using the structure from above 
	public static TSPInstance[] tspinstances;// = new TSPInstance[numberOfInstances];

	/// <summary>
	/// Picking instances.
	/// </summary>
	public List<string> base_i_list = new List<string>();
	public List<string> delay_i_list = new List<string>();
	public List<string> state_i_list = new List<string>();
	public List<string> instances_list = new List<string>();

	// Use this for initialization (this occurs at beginning of game)
	void Awake () 
	{
		//Makes the Game manager a Singleton
		if (instance == null) 
		{
			instance = this;
		}
		else if (instance != this)
			Destroy (gameObject);

		DontDestroyOnLoad (gameObject);

		//Initializes the game
		boardScript = instance.GetComponent<BoardManager> ();

		InitGame();
		if (escena != "SetUp") 
		{
			// Saves timestamp when lottery trial first appears
			//saveTimeStamp(escena);
		}
	}

	//Initializes the scene. One scene is setup, other is trial, other is Break
	void InitGame()
	{
		/*
		Scene Order: escena
		0=setup
		1=trial game
		2=trial game answer
		3= intertrial rest
		4= interblock rest
		5= end
		*/
		//Creates the variable scene? and selects the active scene
		Scene scene = SceneManager.GetActiveScene();
		escena = scene.name;
		//Name the scene "Scene #"
		Debug.Log ("escena" + escena);
		//change numbers to names
		//the loop which runs the game, and drives you from one scene to another
		//If it's the first scene, upload parameters and instances (this happens only once), randomise instances and move incrememntally through >blocks< 1 at a time
		if (escena == "SetUp") 
		{
			block++; 
			loadParameters ();
			loadTSPInstance ();
			Debug.Log("TSPinstances loaded");

			boardScript.setupInitialScreen ();
			//If it's the second scene, move incrementally through trials one at a time, set up the question with items only scene from the boardmanager, show the timer and 
			//run it for the time the items should be there by themselves, do not show the question
		} 

		else if (escena == "LotteryTrial") 
		{
			// adding to trial values so interblockrest works
			trial++;
			TotalTrial++;

			showTimer = true;
			boardScript.SetupScene ("LotteryTrial");
			tiempo = timeAnswer;
			totalTime = tiempo;

			ans_start = timeStamp();

			//If it's the fourth scene, don't show the timer and run it for the time between trials
		} 
		else if (escena == "InterTrialRest") 
		{
			showTimer = false;
			tiempo = timeRest1;
			totalTime = tiempo;

			//If it's the fifth scene, show the timer and run it for the time between blocks, then proceed to the next block
		} 
		else if (escena == "InterBlockRest") 
		{
			trial = 0;
			block++;
			showTimer = true;
			tiempo = timeRest2;
			totalTime = tiempo;
		}
	}

	// Update is called once per frame
	void Update () 
	{
		if (escena != "SetUp") 
		{
			startTimer ();
		}
	}

	/// <summary>
	/// Saves the headers for both files (Trial Info and Time Stamps)
	/// In the trial file it saves:  1. The participant ID. 2. Instance details.
	/// In the TimeStamp file it saves: 1. The participant ID. 2.The time onset of the stopwatch from which the time stamps are measured. 3. the event types description.
	/// </summary>
	private static void saveHeaders()
	{
		//name of the file and where to save it
		identifierName = participantID + "_" + dateID + "_" + "TSP" + "_";
		string folderPathSave = Application.dataPath + outputFolder;

		// Time Stamps file headers
		string[] lines1 = new string[4];
		lines1[0]="PartcipantID:" + participantID;
		lines1[1] = "InitialTimeStamp:" + initialTimeStamp;
		lines1[2] = "no answer = 0; lottery1 = 1(left); lottery2 = 2(right)";
		lines1[3]="block;trial;eventType;elapsedTime(ms)";
		using (StreamWriter outputFile = new StreamWriter(folderPathSave + identifierName + "TimeStamps.txt",true)) 
		{
			foreach (string line in lines1)
				outputFile.WriteLine(line);
		}
	}

	//Saves the data of a trial to a .txt file with the participants ID as filename using StreamWriter.
	//If the file doesn't exist it creates it. Otherwise it adds on lines to the existing file.
	//Each line in the File has the following structure: "trial;answer;timeSpent".
	public static void save(string itemsSelected, int answer, float timeSpent, int randomYes, string error) 
	{
	}

	/// <summary>
	/// Saves the time stamp for a particular event type to the "TimeStamps" File
	/// All these saves take place in the Data folder, where you can create an output folder
	/// </summary>
	/// Event type: 1=ItemsNoQuestion;11=ItemsWithQuestion;2=AnswerScreen;21=ParticipantsAnswer;3=InterTrialScreen;4=InterBlockScreen;5=EndScreen
	public static void saveTimeStamp(string eventType) 
	{
		// Creating time taken to answer
		int ans_time = int.Parse(ans_end) - int.Parse(ans_start);

		// Outputting times to save file
		string dataTrialText = "name " + BoardManager.i_name + "; Block" + block + "; Instance" + BoardManager.randq + "; Answer:" + BoardManager.answer + "; " + eventType + ":" + timeStamp() + "; time taken:" + ans_time;

		string[] lines = {dataTrialText};
		string folderPathSave = Application.dataPath + outputFolder;

		//This location can be used by unity to save a file if u open the game in any platform/computer:      Application.persistentDataPath;
		using (StreamWriter outputFile = new StreamWriter(folderPathSave + identifierName + "TimeStamps.txt",true)) 
		{
			foreach (string line in lines)
				outputFile.WriteLine(line);
		}

	}

	/*
	 * Loads all of the instances to be uploaded form .txt files. Example of input file:
	 * Name of the file: i3.txt
	 * Structure of each file is the following:
	 * variables:[1,2,3,4,5]
	 * literals:[X,Y,D,G,W]
	 *
	 * The instances are stored as tspinstances structures in the array of structures: tspinstances
	 * */
	public void loadTSPInstance()
	{
		//string folderPathLoad = Application.dataPath.Replace("Assets","") + "DATA/Input/KPInstances/";
		string folderPathLoad = Application.dataPath + inputFolderTSPInstances;
		//	int linesInEachKPInstance = 4;
		tspinstances = new TSPInstance[numberOfInstances];

		for (int k = 1; k <= numberOfInstances; k++)
		{
			//create a dictionary where all the variables and definitions are strings
			var dict = new Dictionary<string, string>();
			//Use streamreader to read the input files if there are lines to read
			//string[] KPInstanceText = new string[linesInEachKPInstance];
			try
			{   // Open the text file using a stream reader.
				using (StreamReader sr = new StreamReader(folderPathLoad + "i" + k + ".txt"))
				{
					string line;
					while (!string.IsNullOrEmpty((line = sr.ReadLine())))
					{
						string[] tmp = line.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
						// Add the key-value pair to the dictionary:
						dict.Add(tmp[0], tmp[1]); //int.Parse(dict[tmp[1]]);
					}
				}
				//if there is a problem, then report the following error message
			}
			catch (Exception e)
			{
				Debug.Log("The file could not be read:");
				Debug.Log(e.Message);
			}

			//the following are all recorded as string (hence the S at the end) 
			string nameS;
			string o1rewardsS;
			string o1probsS;
			string o1daysS;
			string o2rewardsS;
			string o2probsS;
			string o2daysS;

			//grab all of those parameters as strings
			dict.TryGetValue("name", out nameS);
			dict.TryGetValue("o1rewards", out o1rewardsS);
			dict.TryGetValue("o1probs", out o1probsS);
			dict.TryGetValue("o1days", out o1daysS);
			dict.TryGetValue("o2rewards", out o2rewardsS);
			dict.TryGetValue("o2probs", out o2probsS);
			dict.TryGetValue("o2days", out o2daysS);

			//convert (most of them) to integers, with variables and literals being arrays and the others single literals
			tspinstances[k - 1].name = nameS;
			tspinstances[k - 1].o1rewards = Array.ConvertAll(o1rewardsS.Substring(1, o1rewardsS.Length - 2).Split(','), int.Parse);
			tspinstances[k - 1].o1probs = Array.ConvertAll(o1probsS.Substring(1, o1probsS.Length - 2).Split(','), float.Parse);
			tspinstances[k - 1].o1days = Array.ConvertAll(o1daysS.Substring(1, o1daysS.Length - 2).Split(','), int.Parse);
			tspinstances[k - 1].o2rewards = Array.ConvertAll(o2rewardsS.Substring(1, o2rewardsS.Length - 2).Split(','), int.Parse);
			tspinstances[k - 1].o2probs = Array.ConvertAll(o2probsS.Substring(1, o2probsS.Length - 2).Split(','), float.Parse);
			tspinstances[k - 1].o2days = Array.ConvertAll(o2daysS.Substring(1, o2daysS.Length - 2).Split(','), int.Parse);

			dict.TryGetValue("problemID", out tspinstances[k - 1].id);
			dict.TryGetValue("instanceType", out tspinstances[k - 1].type);
		}
	}

	static int[,] StringToMatrix(string matrixS)
	{
		int[] convertor = Array.ConvertAll (matrixS.Substring (1, matrixS.Length - 2).Split (','), int.Parse);
		try {
			int vectorheight = Convert.ToInt32(Math.Sqrt (convertor.Length));
			int[,] arr = new int[vectorheight,vectorheight]; // note the swap
			int x = 0;
			int y = 0;
			for (int i = 0; i < convertor.Length; ++i)
			{
				arr[y, x] = convertor[i]; // note the swap
				x++;
				if (x == vectorheight)
				{
					x = 0;
					y++;
				}
			}
			return arr;
		}
			catch (Exception e) {
			Debug.Log ("We don't have an int");
			Debug.Log (e.Message);
			return new int[1, 1]; 
		}
	}

	//Loads the parameters from the text files: param.txt and layoutParam.txt
	void loadParameters()
	{
		//string folderPathLoad = Application.dataPath.Replace("Assets","") + "DATA/Input/";
		string folderPathLoad = Application.dataPath + inputFolder;
		var dict = new Dictionary<string, string>();

		try 
		{   // Open the text file using a stream reader.
			using (StreamReader sr = new StreamReader (folderPathLoad + "layoutParam.txt")) 
			{

				// (This loop reads every line until EOF or the first blank line.)
				string line;
				while (!string.IsNullOrEmpty((line = sr.ReadLine())))
				{
					// Split each line around ':'
					string[] tmp = line.Split(new char[] {':'}, StringSplitOptions.RemoveEmptyEntries);
					// Add the key-value pair to the dictionary:
					dict.Add(tmp[0], tmp[1]);//int.Parse(dict[tmp[1]]);
				}
			}

			using (StreamReader sr1 = new StreamReader (folderPathLoad + "param.txt")) 
			{

				// (This loop reads every line until EOF or the first blank line.)
				string line1;
				while (!string.IsNullOrEmpty((line1 = sr1.ReadLine())))
				{
					//Debug.Log (1);
					// Split each line around ':'
					string[] tmp = line1.Split(new char[] {':'}, StringSplitOptions.RemoveEmptyEntries);
					// Add the key-value pair to the dictionary:
					dict.Add(tmp[0], tmp[1]);//int.Parse(dict[tmp[1]]);
				}
			}

			using (StreamReader sr2 = new StreamReader (folderPathLoad + "param2.txt"))
			{

				// (This loop reads every line until EOF or the first blank line.)
				string line2;
				while (!string.IsNullOrEmpty((line2 = sr2.ReadLine())))
				{
					//Debug.Log (1);
					// Split each line around ':'
					string[] tmp = line2.Split(new char[] {':'}, StringSplitOptions.RemoveEmptyEntries);
					// Add the key-value pair to the dictionary:
					dict.Add(tmp[0], tmp[1]);//int.Parse(dict[tmp[1]]);
				}
			}

		} 
		catch (Exception e) 
		{
			Debug.Log ("The file could not be read:");
			Debug.Log (e.Message);
		}

		assignVariables(dict);

	}

	//Assigns the parameters in the dictionary to variables
	void assignVariables(Dictionary<string,string> dictionary)
	{
		//Assigns Parameters - these are all going to be imported from input files
		// input file is called param
		// Changed rest time (timerest1) to 0, so no delay between trials
		// I changed the values, rest = 0, time to answer (timeanswer) = 20
		string timeRest1S;
		string timeRest2S;
		string timeQuestionS;
		string timeAnswerS;
		string numberOfTrialsS;
		string numberOfBlocksS;
		string numberOfInstancesS;
		string instanceRandomizationS;

		dictionary.TryGetValue ("timeRest1", out timeRest1S);
		dictionary.TryGetValue ("timeRest2", out timeRest2S);

		dictionary.TryGetValue ("timeQuestion", out timeQuestionS);

		dictionary.TryGetValue ("timeAnswer", out timeAnswerS);

		dictionary.TryGetValue ("numberOfTrials", out numberOfTrialsS);

		dictionary.TryGetValue ("numberOfBlocks", out numberOfBlocksS);

		dictionary.TryGetValue ("numberOfInstances", out numberOfInstancesS);

		timeRest1=Convert.ToSingle (timeRest1S);
		timeRest2=Convert.ToSingle (timeRest2S);
		timeQuestion=Int32.Parse(timeQuestionS);
		timeAnswer=Int32.Parse(timeAnswerS);
		numberOfTrials=Int32.Parse(numberOfTrialsS);
		numberOfBlocks=Int32.Parse(numberOfBlocksS);
		numberOfInstances=Int32.Parse(numberOfInstancesS);

		dictionary.TryGetValue ("instanceRandomization", out instanceRandomizationS);
		
		 int[] instanceRandomizationNo0 = Array.ConvertAll(instanceRandomizationS.Substring (1, instanceRandomizationS.Length - 2).Split (','), int.Parse);
		instanceRandomization = new int[instanceRandomizationNo0.Length];

		//foreach (int i in instanceRandomizationNo0)
		for (int i = 0; i < instanceRandomizationNo0.Length; i++)
		{
			instanceRandomization[i] = instanceRandomizationNo0 [i] - 1;
		}
	}

	//Takes care of changing the Scene to the next one (Except for when in the setup scene)
	public static void changeToNextScene(List <Vector3> itemClicks, int answer, int randomYes)
	{
		BoardManager.keysON = false;
		if (escena == "SetUp"){
			saveHeaders();
			SceneManager.LoadScene("LotteryTrial");
		}else if (escena == "LotteryTrial")
		{
			string itemsSelected = extractItemsSelected(itemClicks);
			if (answer == 3)
			{
				save(itemsSelected, answer, timeQuestion, randomYes, "");
			}
			else
			{
				save(itemsSelected, answer, timeAnswer - tiempo, randomYes, "");
				ans_end = timeStamp();
				saveTimeStamp("ParticipantAnswer");
			}
			//saveClicks (itemClicks);
			SceneManager.LoadScene("InterTrialRest");
		}
		else if (escena == "InterTrialRest")
		{
			changeToNextTrial();
		}
		else if (escena == "InterBlockRest")
		{
			SceneManager.LoadScene("LotteryTrial");
		}

	}
	//Redirects to the next scene depending if the trials or blocks are over.
	private static void changeToNextTrial()
	{
		//Checks if trials are over
		Debug.Log("trial val" + trial);
		if (trial < numberOfTrials) {
			SceneManager.LoadScene ("LotteryTrial");
		} else if (block < numberOfBlocks) {
			SceneManager.LoadScene ("InterBlockRest");
		}else {
			SceneManager.LoadScene ("End");
		}
	}

	/// <summary>
	/// Extracts the items that were finally selected based on the sequence of clicks.
	/// </summary>
	/// <returns>The items selected.</returns>
	/// <param name="itemClicks"> Sequence of clicks on the items.</param>
	private static string extractItemsSelected (List <Vector3> itemClicks)
	{
		List<int> itemsIn = new List<int>();
		foreach(Vector3 clickito in itemClicks){
			if (clickito.z == 1) {
				itemsIn.Add (Convert.ToInt32 (clickito.x));
			} else if (clickito.z == 0) {
				itemsIn.Remove (Convert.ToInt32 (clickito.x));
			} else if (clickito.z == 3) {
				itemsIn.Clear ();
			}
		}

		string itemsInS = "";
		foreach (int i in itemsIn)
		{
			itemsInS = itemsInS + i + ",";
		}
		if(itemsInS.Length>0)
			itemsInS = itemsInS.Remove (itemsInS.Length - 1);

		return itemsInS;
	}

	/// <summary>
	/// Starts the stopwatch. Time of each event is calculated according to this moment.
	/// Sets "initialTimeStamp" to the time at which the stopwatch started.
	/// </summary>
	public static void setTimeStamp()
	{
		initialTimeStamp=@System.DateTime.Now.ToString("HH-mm-ss-fff");
		stopWatch.Start ();
		Debug.Log ("Stat timestamp" + initialTimeStamp);
	}

	/// <summary>
	/// Calculates time elapsed
	/// </summary>
	/// <returns>The time elapsed in milliseconds since the "setTimeStamp()".</returns>
	private static string timeStamp()
	{
		//		TimeSpan ts = stopWatch.Elapsed;
		//		string stamp = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
		//			ts.Hours, ts.Minutes, ts.Seconds,
		//			ts.Milliseconds / 10);
		long milliSec = stopWatch.ElapsedMilliseconds;
		string stamp = milliSec.ToString();
		return stamp;
	}

	//Updates the timer (including the graphical representation)
	//If time runs out in the trial or the break scene. It switches to the next scene.
	void startTimer()
	{
		tiempo -= Time.deltaTime;
		//Debug.Log (tiempo);
		if (showTimer) 
		{
			boardScript.updateTimer();
		}

		//When the time runs out:
		if(tiempo < 0)
		{
			//changeToNextScene(2,BoardManager.randomYes);
			changeToNextScene(BoardManager.itemClicks,BoardManager.answer,BoardManager.randomYes);
		}
	}

	/// <summary>
	/// In case of an error: Skip trial and go to next one.
	/// Example of error: Not enough space to place all items
	/// </summary>
	/// Receives as input a string with the errorDetails which is saved in the output file.
	public static void errorInScene(string errorDetails){
		Debug.Log (errorDetails);

		BoardManager.keysON = false;
		int answer = 4;
		int randomYes = -1;
		save ("", answer, timeTrial, randomYes, errorDetails);
		changeToNextTrial ();
	}

}