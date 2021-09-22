using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rand : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Function to put random values in the pie chart. placeholder for now
    public void RandomPieChart()
    {
        float[] randvalues = new float[4];

        for (int n = 0; n < randvalues.Length; n++)
        {
            randvalues[n] = Random.Range(0.0f, 100.0f);
        }

        Debug.Log("rand working");

        GetComponent<BoardManager>().o1pie(randvalues);
        
        GetComponent<BoardManager>().o2pie(randvalues);
    }
}

