using UnityEngine;
using System.Collections;

public class Calc : MonoBehaviour {

	float output = -29.5f;
	// Use this for initialization
	void Start () 
	{
		int i = 1;
		while (output <= 500f)
		{
			i++;
			output += 5f - 0.0909f;
			Debug.Log(output);
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
