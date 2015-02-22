using UnityEngine;
using System.Collections;

public class TextPopup : MonoBehaviour {

    private Vector2 scrollPosition = new Vector2(0,0);
    private bool isDisplaying = false;
    private string popupTitle;
    private string popupText;
    private int textureHeight;
    private int textureWidth;
    private int screenHeight;
    private int screenWidth;
    private GUITexture myGUITexture = new GUITexture();
    private GUISkin uiSkinResource;
    private GameObject backdrop;

	// Use this for initialization
	void Start () {
        uiSkinResource = Resources.Load("TextPopup") as GUISkin;
        backdrop = GameObject.Find("Menu-Color");
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
        GUI.skin = uiSkinResource;
        if (isDisplaying) {
            backdrop.SetActive(true);

            GUILayout.BeginArea(new Rect(50, 50, Screen.width - 100, Screen.height - 50));
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(Screen.width - 100), GUILayout.Height(Screen.height - 120));

            GUI.skin.box.wordWrap = true;     // set the wordwrap on for box only.
            GUILayout.Box(popupTitle);       // just your message as parameter.
            GUILayout.Box(popupText);

            GUILayout.EndScrollView();

            if (GUILayout.Button("Close")) {
                isDisplaying = false;
            }

            GUILayout.EndArea();

        } else {
            backdrop.SetActive(false);
        }
    }
    
	// Update is called once per frame
	void Update () {
        if (!isDisplaying && Input.GetMouseButtonDown(0) == true) {
            DisplayTextAndTitle("Test", "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus est sapien, placerat sodales justo sit amet, venenatis interdum lectus. Ut congue metus at tempus pretium. Praesent sit amet imperdiet neque. Aliquam eget interdum velit. Nunc aliquet purus vel erat dapibus, eget pellentesque lectus finibus. Maecenas dignissim venenatis vulputate. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed venenatis urna in felis efficitur, sed congue neque tristique. Praesent imperdiet maximus elit, vitae egestas odio viverra ac. Mauris fringilla pellentesque nibh sed ullamcorper. Pellentesque turpis mi, consectetur vitae ipsum at, hendrerit convallis quam. Duis in lorem malesuada, rutrum turpis eu, hendrerit leo. Nulla at risus ut augue auctor aliquam eu eu nunc. Nam sed imperdiet dui. Duis id eros porttitor, imperdiet purus in, viverra purus. \nQuisque eget mauris condimentum, dignissim felis eget, ultricies odio. Cras tristique orci tellus, a vulputate quam semper id. Praesent massa eros, lobortis sed venenatis et, malesuada eget elit. Nulla egestas augue nibh, in tincidunt diam fermentum at. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Maecenas lobortis magna eu metus semper, et vestibulum dui tempus. Donec consectetur libero lacinia magna dapibus, id suscipit tortor ullamcorper. Vivamus pharetra placerat neque, at cursus ligula auctor sit amet.");
        }
	}
}
