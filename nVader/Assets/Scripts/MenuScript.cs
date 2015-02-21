using UnityEngine;
using System.Collections;

public class MenuScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.touchCount > 0 || Input.GetMouseButtonDown(0) == true)
        {
            Application.LoadLevel("Game");
        }
	}
}
