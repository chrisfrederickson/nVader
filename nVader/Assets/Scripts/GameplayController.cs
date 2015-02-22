using UnityEngine;

using System;

using UnitySlippyMap;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using ProjNet.Converters.WellKnownText;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using System.Linq;

using UnityEngine.UI;

public class GameplayController : MonoBehaviour {
	public int MineInventory = 3;
	public float LocationAccuracy = 0.01f; 
	public Button SetBeacon;
	public Button HarvestMine;
	private Marker BeaconMarker;
	private List<Marker> MineMarkers = null;
	private float MAP_CURRENT_ZOOM = 18.0f;
	private Landmark CurrentLandmark;
	public ParticleSystem gear1;
	public ParticleSystem gear2;
	public ParticleSystem gear3;
	public AudioClip gearsGrinding;
	public AudioClip inventoryGet;
	//public AudioClip buttonBloop;
	//TODO One beacon at a time
	// Use this for initialization
	void Start () {
		//StartCoroutine(StartMapManager());
		StartMapManager ();
		GameSave.Load ();
		GameSave.Save ();
		HarvestMine.gameObject.SetActive (false);
		InsertMineMarkers ();
		GetAudioSourceReflection ().Stop ();
	}
	
	// Update is called once per frame
	void Update () {
		UpdateMap ();
//		Debug.Log (map.CurrentZoom+", "+map.MinZoom+", "+map.MaxZoom);
		if (!IsLocationEnabled ()) 
			return;
		//Debug.Log (GetMinesLeft() + " mines left");
		HarvestMine.gameObject.SetActive(false); 
		if (Saved ().mines != null) {
			//Check if any mines are nearby to harvest 
			Saved ().mines.ForEach (delegate(Mine m) {
				//Debug.Log("-For Mine @"+m.GetPairedLandmark().GetTitle());
				if (LocationClose (m.GetCoordinatesPlaced (), MyLocation (), LocationAccuracy) && (m.GetHarvestTime() < DateTime.Now.Ticks)) {
					//Harvest Mine Bttn appears
					HarvestMine.gameObject.SetActive (true);
				} else {
					HarvestMine.gameObject.SetActive(false); 
				}
			});
		} else 
			HarvestMine.gameObject.SetActive(false); 
	}
	public void OnHarvestMineDown() {
		Debug.Log ("Yay!");
		GetAudioSourceReflection ().clip = inventoryGet;
		GetAudioSourceReflection ().volume = 1f;
		GetAudioSourceReflection ().Play ();
		//TODO Stop after 3 seconds
		Saved ().mines.ForEach (delegate(Mine m) {
			Debug.Log("|For Mine @"+m.GetPairedLandmark ().GetTitle ());
			if (LocationClose (m.GetCoordinatesPlaced (), MyLocation (), LocationAccuracy) && m.GetHarvestTime() < DateTime.Now.Ticks) {
				//Harvest Mine Bttn appears
					//You obtain stuff!
					Saved().mines.Remove(m);
					MergeResources (m.GetPairedLandmark ().GetResources ());
					HarvestMine.gameObject.SetActive (false); 
				if(Saved().usedLandmarks == null)
					Saved ().usedLandmarks = new List<String> ();
				Saved ().usedLandmarks.Add(m.GetPairedLandmark ().GetTitle());
			} else {
				HarvestMine.gameObject.SetActive (false); 
			}
		});
		HarvestMine.gameObject.SetActive (false); 
		Saved ().resources.ForEach (delegate(Resource r) {
			if(r.GetResourceType ().Equals("TimeEnergy") && r.GetResourceValue () > 5000)
				AddPopup("Congrats!", "You have enough time energy to travel home, or you can stay on Earth. Thanks for playing!");

		});
	}
	double[] MyLocation() {
		return new double[2] { UnityEngine.Input.location.lastData.longitude, UnityEngine.Input.location.lastData.latitude };
	}
	bool IsLocationEnabled () {
		return Input.location.status == LocationServiceStatus.Running;
	}
	bool LocationClose(double[] c1, double[] c2, float accuracy) {
		return (Mathf.Abs((float) c1[0] - (float) c2[0]) < accuracy) && (Mathf.Abs((float) c1[1] - (float) c2[1]) < accuracy);
	}
	int GetMinesLeft() {
		if (Saved ().mines != null)
			return MineInventory - Saved ().mines.Count;
		else
			return MineInventory;
	}
	Savefile Saved() {
		return GameSave.savedGame;
	}
	String GetMinesLeftString() {
		return "Mines (" + GetMinesLeft () + ")";
	}
	void SetMine(Landmark l) {
		Saved().mines.Add(new Mine(6, MyLocation(), l));
		//TODO Set a delayed notification for later
		GameSave.Save ();
	}
	void MergeResources(List<Resource> stuff) {
		Debug.Log ("Found a lot of resources to have");
		if (Saved().resources == null) {
			Saved().resources = new List<Resource>();
			Saved().resources.Add(new TimeEnergy(0));
		}
		string output = "";
		Saved ().resources.ForEach (delegate(Resource r) {
			Debug.Log ("Saved."+r.GetResourceType());
			stuff.ForEach(delegate(Resource obj) {
				Debug.Log ("stuff."+r.GetResourceType());
				if(r.GetResourceType().Equals(obj.GetResourceType())) {
					//Merge newly harvested items
					output += r.GetResourceType()+"   x"+obj.GetResourceValue()+"\n";
					r.AddResourcesOfThisType(obj.GetResourceValue());
					Debug.Log ("Adding "+obj.GetResourceValue()+" "+obj.GetResourceType()+"s");	
				}
			});
		});
		AddPopup ("Resources Collected", output);
		GameSave.Save ();
		InsertMineMarkers();
	}
	public void OnInventoryButtonDown() {
		//CREATE A POPUP THAT DISPLAYS THE INVENTORY
		string inventory = "";
		inventory += "Mines   x"+GetMinesLeft ()+"\n";
		if (Saved ().resources != null) {
			Saved ().resources.ForEach(delegate(Resource obj) {
				inventory += obj.GetResourceType ()+"   x"+obj.GetResourceValue()+"\n";
			});
		}
		AddPopup ("Inventory", inventory);

	}
	/**
	 * Interface for TextPopup in the GameController
	 **/
	public void AddPopup(string title, string text, bool mine=false) {
		GameObject go = gameObject;
		TextPopup tp = (TextPopup)go.GetComponent (typeof(TextPopup));
		tp.DisplayTextAndTitle (title, text, mine);
		EnableGUI (false);
	}
	public void OnPopupClosed() {
		EnableGUI (true);
	}
	public void EnableGUI(bool en) {
		if (!en) {
			HarvestMine.gameObject.SetActive (en);
		}
		SetBeacon.gameObject.SetActive (en);
		
	}

	/**** MAP MANAGER CODE ****/
	public static Map		map;
	
	public Texture	LocationTexture;
	public Texture	MineTexture;
	public Texture  BeaconTexture;
	
	private float	guiXScale;
	private float	guiYScale;
	private Rect	guiRect;
	
	//private bool 	isPerspectiveView = false;
	//private float	perspectiveAngle = 30.0f;
	private float	destinationAngle = 0.0f;
	private float	currentAngle = 0.0f;
	private float	animationDuration = 0.5f;
	private float	animationStartTime = 0.0f;
	
	private List<Layer> layers;
	//private int     currentLayerIndex = 0;

	private static int MINE = 314;
	private static int BEACON = 278;

	void StartMapManager() {
		// create the map singleton
		map = Map.Instance;
		map.CurrentCamera = Camera.main;
		map.InputDelegate += UnitySlippyMap.Input.MapInput.BasicTouchAndKeyboard;
		map.CurrentZoom = MAP_CURRENT_ZOOM;
		// 9 rue Gentil, Lyon
		//map.CenterWGS84 = new double[2] { 4.83527, 45.76487 };
		//Now use GPS
		
		map.UseLocation = true;
		map.InputsEnabled = false;
		map.ShowGUIControls = true;
		map.IsDirty = true;
		map.UpdateCenterWithLocation = true;
		// Start service before querying location
		Input.location.Start ();
		Debug.Log ("Is location enabled by user?" + Input.location.isEnabledByUser);
		Debug.Log ("Use location? " + map.UseLocation);
		Debug.Log ("Is location running? " + (UnityEngine.Input.location.status == LocationServiceStatus.Running));
		
		
		// Wait until service initializes
		int maxWait = 5 * 100 * 1000/*ms*/* 1000;

		Debug.Log ("Init? " + (Input.location.status == LocationServiceStatus.Initializing));
		
		long secondsElapsed = DateTime.Now.Ticks;
		Debug.Log (DateTime.Now.Ticks+", "+secondsElapsed+", "+maxWait+", "+(DateTime.Now.Ticks - secondsElapsed)+", "+((DateTime.Now.Ticks - secondsElapsed) < maxWait));
		while (Input.location.status
		       == LocationServiceStatus.Initializing && (DateTime.Now.Ticks - secondsElapsed) < maxWait) {
			//return yield WaitForSeconds (1);
			//Debug.Log (DateTime.Now.Ticks+", "+secondsElapsed+", "+maxWait+", "+(DateTime.Now.Ticks - secondsElapsed)+", "+((DateTime.Now.Ticks - secondsElapsed) < maxWait));

			//maxWait--;
		}
		// Service didn't initialize in 20 seconds
		if ((DateTime.Now.Ticks - secondsElapsed) > maxWait) {
			Debug.Log ("Timed out");
			return ;
		}
		// Connection has failed
		if (Input.location.status == LocationServiceStatus.Failed) {
			Debug.Log ("Unable to determine device location");
			return ;
		}
		if(Input.location.status == LocationServiceStatus.Initializing) {
			Debug.Log ("Didn't wait long enough");
			return ;
		}
		if(Input.location.status == LocationServiceStatus.Stopped) {
			Debug.Log ("Stopped service");
			return ;
		}
		if(Input.location.status == LocationServiceStatus.Running) {
			Debug.Log ("Location is active");
		}
		Debug.Log ("Is location running? " + (UnityEngine.Input.location.status == LocationServiceStatus.Running));
		Debug.Log ("Updating location? " + map.UpdateCenterWithLocation);
		map.CenterOnLocation ();
		map.CenterWGS84 = new double[2] { UnityEngine.Input.location.lastData.longitude, UnityEngine.Input.location.lastData.latitude };
		Debug.Log ("Getting location " + UnityEngine.Input.location.lastData.longitude + ", " + UnityEngine.Input.location.lastData.latitude);
		Debug.Log ("Found WGS84 location " + map.CenterWGS84[0]+", "+map.CenterWGS84[1]);

        layers = new List<Layer>();

		// create an OSM tile layer
        OSMTileLayer osmLayer = map.CreateLayer<OSMTileLayer>("OSM");
        osmLayer.BaseURL = "http://a.tile.openstreetmap.org/";
		
        layers.Add(osmLayer);
		
		// create the location marker
		GameObject go = Tile.CreateTileTemplate().gameObject;
		go.renderer.material.mainTexture = LocationTexture;
		go.renderer.material.renderQueue = 4000;
		go.transform.localScale /= 13.0f;
		
		GameObject markerGO = Instantiate(go) as GameObject;
		map.SetLocationMarker<LocationMarker>(markerGO);

		DestroyImmediate(go);
	}
	void OnApplicationQuit()
	{
		map = null;
	}
	void UpdateMap() {
		if (destinationAngle != 0.0f) {
			Vector3 cameraLeft = Quaternion.AngleAxis(-90.0f, Camera.main.transform.up) * Camera.main.transform.forward;
			if ((Time.time - animationStartTime) < animationDuration)
			{
				float angle = Mathf.LerpAngle(0.0f, destinationAngle, (Time.time - animationStartTime) / animationDuration);
				Camera.main.transform.RotateAround(Vector3.zero, cameraLeft, angle - currentAngle);
				currentAngle = angle;
			}
			else
			{
				Camera.main.transform.RotateAround(Vector3.zero, cameraLeft, destinationAngle - currentAngle);
				destinationAngle = 0.0f;
				currentAngle = 0.0f;
				map.IsDirty = true;
			}
			
			map.HasMoved = true;
		}
	}
	public Marker PlaceMine(String name) {
		return PlaceMine(name, map.CenterWGS84);
	}
	public Marker PlaceMine(String name, double[] coords) {
		
		Debug.Log ("Placing a mine at "+coords[0]+", "+coords[1]);
		Debug.Log ("You have "+GetMinesLeft()+" mines left.");
		if(MineMarkers == null)
			MineMarkers = new List<Marker>();	
		Marker m = PlaceMarker(name, MINE, coords);
		MineMarkers.Add(m);
		return m;
	}
	public void BeaconDown() {
		Debug.Log ("BEACON BUTTON");
		if(BeaconMarker != null)
			RemoveMarker(BeaconMarker);
		BeaconMarker = PlaceBeacon("");
		FindClosestLandmark();
		
	}
	public void OnMineButtonDown() {

		Debug.Log("Mine this: x"+GetMinesLeft());
		//First, check if this area is depleted:
		if(Saved().usedLandmarks == null)
			Saved().usedLandmarks = new List<String>();
		if(Saved().usedLandmarks.IndexOf(CurrentLandmark.GetTitle()) > -1) {
			AddPopup("Cannot Place Mine", "You have visited this place before, and all its energy is depleted.");
			return;
		}

		if (GetMinesLeft () > 0 && CurrentLandmark != null) {
			if(Saved ().mines == null)
				Saved ().mines = new List<Mine> ();
			Debug.Log ("There are "+GetMinesLeft ()+" mines left, near "+CurrentLandmark.GetTitle ());
			//Check for dupes
			List<String> names = new List<string>();
			bool okayToMine = true;
			Saved ().mines.ForEach(delegate(Mine obj) {
				Debug.Log (obj.GetPairedLandmark ().GetTitle ());
				Debug.Log (names.ToString ());
				if(names.IndexOf(obj.GetPairedLandmark().GetTitle ()) == -1) {//Didn't find dupe! 
					names.Add(obj.GetPairedLandmark ().GetTitle ());

				} else {
					okayToMine = false;
					AddPopup("Cannot Place Mine", "You already have a mine here. Please wait a few more hours to retrieve it.");
				}
			});
			Debug.Log ("The thing you are seeking is "+names.IndexOf(CurrentLandmark.GetTitle ())+", "+okayToMine);
			if(okayToMine && names.IndexOf(CurrentLandmark.GetTitle ()) == -1){
				Saved().mines.Add(new Mine(6, MyLocation (), CurrentLandmark));
				InsertMineMarkers ();
				PlaceMine("");
				GameSave.Save ();
			} else {
				okayToMine = false;
				AddPopup("Cannot Place Mine", "You already have a mine here. Please wait a few more hours to retrieve it.");
			}
		} else if (GetMinesLeft() <= 0) {
			AddPopup("Cannot Place Mine", "You don't have any mines left! Go back to previous locations to retrieve them.");
		}
	}
	public Marker PlaceBeacon(String name) {
		return PlaceBeacon(name, map.CenterWGS84);
	}
	public Marker PlaceBeacon(String name, double[] coords) {

		Debug.Log ("Beacon "+name+" is being placed on the map at "+coords[0]+", "+coords[1]);
		return PlaceMarker (name, BEACON, coords);
	}
	public Marker PlaceMarker(String name, int type, double[] coords) {
		GameObject go = Tile.CreateTileTemplate (Tile.AnchorPoint.BottomCenter).gameObject;
		if(type == MINE)
			go.renderer.material.mainTexture = MineTexture;
		else 
			go.renderer.material.mainTexture = BeaconTexture;
		go.renderer.material.renderQueue = 4001;
		go.transform.localScale = new Vector3(0.70588235294118f, 1.0f, 1.0f);
		go.transform.localScale /= 2.0f;
        go.AddComponent<CameraFacingBillboard>().Axis = Vector3.up;

		GameObject markerGO;
		markerGO = Instantiate (go) as GameObject;
		if(type == BEACON) {
			gear1.Play();
			gear2.Play();
			gear3.Play();
		    GetAudioSourceReflection().clip = gearsGrinding;
		    GetAudioSourceReflection().Play ();
	    }
		Destroy(go);
		return map.CreateMarker<Marker>(name, coords, markerGO);
	}
	public AudioSource GetAudioSourceReflection() {
		GameObject go = gameObject;
		return (AudioSource)go.GetComponent (typeof(AudioSource));
	}
	public void RemoveMarker(Marker m) {
		map.RemoveMarker(m);
	}
	void InsertMineMarkers() {
		//Remove any beacons on the map
		//TODO Fixme later
		if(BeaconMarker != null)
			RemoveMarker(BeaconMarker);
		//Remove any mines that are on the map
		Debug.Log("Clearing the map of mines");
		if(map.Markers != null) {
			map.Markers.ForEach(delegate(Marker obj) {
				if(obj != null) {
					//RemoveMarker(obj);
					map.RemoveMarker(obj);
				} else
					Debug.Log ("It was a map marker that was undefined");
			});
		}
		if(MineMarkers != null) {
			MineMarkers.ForEach(delegate(Marker obj) {
				if(obj != null) {
					//RemoveMarker(obj);
					RemoveMarker(obj);
				} else
					Debug.Log ("It was a mine marker that was undefined");
			});
		}
		Debug.Log ("Now redrawing the mines");
		//Get each mine, put it in a marker, add it to a list
		if(Saved ().mines != null) {
			Debug.Log("Draw "+Saved().mines.Count+" mines on the map");
			Saved ().mines.ForEach(delegate(Mine obj) {
				Debug.Log ("Next mine obj");
				//if(obj != null) {
				//} else 
				//	Debug.Log ("It was a mine that was undefined");
			});
		}
	}
	
	/** SERVER API **/
	private string URL_ENDPOINT = "http://nvader.azurewebsites.net/goodies";
	void FindClosestLandmark() {
		string url = URL_ENDPOINT+"?lat="+MyLocation()[1]+"&lon="+MyLocation()[0];
		Debug.Log("Sending GET to " + url);
		//HandleClosestLandmark("{\"dist\":252.96047122030745,\"name\":\"Powelton Historic District\",\"description\":\"Powelton Village is a neighborhood of mostly Victorian, mostly twin homes in the West Philadelphia section of the United States city of Philadelphia, Pennsylvania. It is a national historic district which is part of University City. It extends north from Market Street to Spring Garden Street, east to 32nd Street, west to 40th and Spring Garden Streets, and to 44th and Market Streets.\",\"wikiArticle\":\"Powelton_Village,_Philadelphia\",\"buildDate\":\"1902\"}");
		WWW www = new WWW(url);
		StartCoroutine(WaitForGETRequest(www));
	}
	IEnumerator WaitForGETRequest(WWW www)
	{
		yield return www;
		
		// check for errors
		if (www.error == null)
		{
			Debug.Log("WWW Ok!: " + www.text);
			CurrentLandmark = HandleClosestLandmark(www.text);
		} else {
				FindClosestLandmark();
			Debug.Log("WWW Error: "+ www.error);
		}    
	}
	Landmark HandleClosestLandmark(String WebData) {
			gear1.Pause ();
			gear2.Pause ();
			gear3.Pause ();
			GetAudioSourceReflection ().Stop ();
		//TODO Track dist as a last chance to prevent errors
		//This comes from a Beacon ping. 
		//Parse Data
			Debug.Log (WebData);
			var response = JSON.Parse (WebData);

		List<Resource> r = new List<Resource>();
			r.Add(new TimeEnergy(DateTime.Now.Year - 1900));
		Landmark ClosestLandmark = new Landmark (
				response["name"], 
				response["description"], 
				r, 
				int.Parse(response["buildDate"]));
		//Set beacon to this landmark.
		//Open popup.
		
		AddPopup (ClosestLandmark.GetTitle (), ClosestLandmark.GetDescription (), true);
		//Show Mine button.
		return ClosestLandmark;
	}
}
	