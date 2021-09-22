using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using System.Linq;
using Random = UnityEngine.Random;

public class o1options : MonoBehaviour
{
    // creating text objects for each state
    public string o1state1;

	// Creating array to store the options for each trial
	//public o1states[];

	// Need to add bit where I load options from input
	//Input and Outout Folders with respect to the Application.dataPath;
	private static string inputFolder = "/DATAinf/Input/";
	private static string inputFolderTSPInstances = "/DATAinf/Input/TSPInstances/";
	private static string outputFolder = "/DATAinf/Output/";

	//An array of all the instances to be uploaded from .txt files, i.e importing everything using the structure from above 
	public static TSPInstance[] tspinstances;// = new TSPInstance[numberOfInstances];

	//we would need a vector of literals, corresponding to 3 literals for each clause in that instance
	//also need a vector of variables for each instance, to see where the literals are being drawn from
	//changes needed further along due to changes in struct
	//A structure that contains the parameters of each instance
	public struct TSPInstance
	{
		public int[] rewards;
		public float[] probs;
		public int[] days;

		//public int ncities;
		//public int maxdistance;

		public string id;
		public string type;
		
		//public int solution;
	}

	//This is also taken from input files, so in reality 63 instance files are loaded, not 3
	//Number of instance files to be considered. From i1.txt to i_.txt..
	public static int numberOfInstances = 3;

	// Start is called before the first frame update
	void Start()
    {
		loadTSPInstance();
		Debug.Log("o1 working");

	}

    // Update is called once per frame
    void Update()
    {
        
    }

	public static void loadTSPInstance()
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
			string rewardsS;
			string probsS;
			string daysS;

			//grab all of those parameters as strings
			dict.TryGetValue("rewards", out rewardsS);
			dict.TryGetValue("probs", out probsS);
			dict.TryGetValue("days", out daysS);

			//convert (most of them) to integers, with variables and literals being arrays and the others single literals
			tspinstances[k - 1].rewards = Array.ConvertAll(rewardsS.Substring(1, rewardsS.Length - 2).Split(','), int.Parse);
			tspinstances[k - 1].probs = Array.ConvertAll(probsS.Substring(1, probsS.Length - 2).Split(','), float.Parse);
			tspinstances[k - 1].days = Array.ConvertAll(daysS.Substring(1, daysS.Length - 2).Split(','), int.Parse);

			//tspinstances[k - 1].ncities = int.Parse(ncitiesS);
			//tspinstances[k - 1].maxdistance = int.Parse(maxdistanceS);
			//tspinstances[k - 1].solution = int.Parse(solutionS);

			dict.TryGetValue("problemID", out tspinstances[k - 1].id);
			dict.TryGetValue("instanceType", out tspinstances[k - 1].type);
		}
	}
}
