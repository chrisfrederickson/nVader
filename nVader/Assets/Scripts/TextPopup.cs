using UnityEngine;
using System.Collections;

public class TextPopup : MonoBehaviour {

    private Vector2 scrollPosition = new Vector2(0,0);
    private bool isDisplaying = false;
    private string popupTitle;
    private string popupText;

	// Use this for initialization
	void Start () {
	}

    void DisplayTextAndTitle (string title, string text) {
        popupTitle = title;
        popupText = text;
        isDisplaying = true;
    }

    void DisplayText(string text) {
        popupTitle = "";
        popupText = text;
        isDisplaying = true;
    }

    void OnGUI() {
        if (isDisplaying) {
            GUILayout.BeginArea(new Rect(50, 50, Screen.width - 100, Screen.height - 50));
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(Screen.width - 100), GUILayout.Height(Screen.height - 100));

            GUI.skin.box.wordWrap = true;     // set the wordwrap on for box only.
            GUILayout.Box(popupTitle);       // just your message as parameter.
            GUILayout.Box(popupText);

            GUILayout.EndScrollView();

            if (GUI.Button(new Rect(10, 70, 50, 30), "Close")) {
                isDisplaying = false;
            }

            GUILayout.EndArea();
        }
    }
	// Update is called once per frame
	void Update () {
        if (!isDisplaying && Input.GetMouseButtonDown(0) == true) {
            DisplayTextAndTitle("Test", "Very Long Test");
        }
	}
}
