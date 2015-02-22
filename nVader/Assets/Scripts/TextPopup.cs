using UnityEngine;
using System.Collections;

public class TextPopup : MonoBehaviour {

    private Vector2 scrollPosition = new Vector2(0,0);
    private bool isDisplaying = false;
	private bool isMineable = false;
    private string popupTitle;
    private string popupText;
    private int textureHeight;
    private int textureWidth;
    private int screenHeight;
    private int screenWidth;
    //private GUITexture myGUITexture = new GUITexture();
    private GUISkin uiSkinResource;
    private GameObject backdrop;

	// Use this for initialization
	void Start () {
        uiSkinResource = Resources.Load("TextPopup") as GUISkin;
	}

    public void DisplayTextAndTitle (string title, string text, bool minethis = false) {
        popupTitle = title;
        popupText = text;
        isDisplaying = true;
		isMineable = minethis;
    }

    public void DisplayText(string text) {
        popupTitle = "";
        popupText = text;
        isDisplaying = true;
    }

    void OnGUI() {
        GUI.skin = uiSkinResource;
        if (isDisplaying) {
            //backdrop.SetActive(true);

            GUILayout.BeginArea(new Rect(50, 50, Screen.width - 100, Screen.height - 50));
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(Screen.width - 100), GUILayout.Height(Screen.height - 120));

            GUI.skin.box.wordWrap = true;     // set the wordwrap on for box only.
            GUILayout.Box(popupTitle);       // just your message as parameter.
            GUILayout.Box(popupText);
			if (GUILayout.Button("Close")) {
                isDisplaying = false;
				GameObject go = gameObject;
				GameplayController tp = (GameplayController)go.GetComponent (typeof(GameplayController));
				tp.OnPopupClosed();
			}
			if(isMineable) {
				if(GUILayout.Button("Place Mine")) {
					isDisplaying = false;
					isMineable = false;
					GameObject go = gameObject;
					GameplayController tp = (GameplayController)go.GetComponent (typeof(GameplayController));
					tp.OnPopupClosed();
					Debug.Log ("Placing a mine");
					Debug.Log (go != null);
					tp.OnMineButtonDown();
				}
			}
            GUILayout.EndScrollView();

            

            GUILayout.EndArea();

        } else {
            //backdrop.SetActive(false);
        }
    }
    
	// Update is called once per frame
	void Update () {
        
	}
}
