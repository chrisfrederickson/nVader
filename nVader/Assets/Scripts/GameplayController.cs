﻿using UnityEngine;

using System;

using UnitySlippyMap;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using ProjNet.Converters.WellKnownText;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine.UI;

public class GameplayController : MonoBehaviour {
	void Awake() {
		// Forces a different code path in the BinaryFormatter that doesn't rely on run-time code generation (which would break on iOS).
		Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
	}
	public int MineInventory = 3;
	public float LocationAccuracy = 0.01f; 
	public Button HarvestMine;
	private Marker BeaconMarker;
	private List<Marker> MineMarkers = null;
	private float MAP_CURRENT_ZOOM = 15.0f;
	//public AudioClip buttonBloop;
	//TODO One beacon at a time
	// Use this for initialization
	void Start () {
		StartCoroutine(StartMapManager());
		GameSave.Load ();
		GameSave.Save ();
		UpdateMineInventory ();
		HarvestMine.gameObject.SetActive (false);
		InsertMineMarkers ();
	}
	
	// Update is called once per frame
	void Update () {
		UpdateMap ();
//		Debug.Log (map.CurrentZoom+", "+map.MinZoom+", "+map.MaxZoom);
		if (!IsLocationEnabled()) 
			return;
		//Debug.Log (GetMinesLeft() + " mines left");
		if (Input.GetButtonDown ("Mine") && GetMinesLeft () != 0) {
			PlaceMine("Name");
			//TODO Get data about location
		//	Saved().mines.Add(new Mine(BOGUS));
			GameSave.Save();
		} else if (GetMinesLeft () == 0) {
			//TODO Spit error back to the user
		}

		if (Saved ().mines != null) {
			//Check if any mines are nearby to harvest 
			Saved ().mines.ForEach (delegate(Mine m) {
				if (LocationClose (m.GetCoordinatesPlaced (), MyLocation (), LocationAccuracy)) { //TODO AND Havest time
					//Harvest Mine Bttn appears
					HarvestMine.gameObject.SetActive (true);
					if (Input.GetButtonDown ("HarvestMine")) {
						//You obtain stuff!
						MergeResources (m.GetPairedLandmark ().GetResources ());
					}
				} else {
					HarvestMine.gameObject.SetActive(false); 
				}
			});
		}
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
	void UpdateMineInventory() {
		//MinesLeft.text = "Mines ("+GetMinesLeft()+")";
	}
	void GetBeaconMessage(Landmark l) {
		//TODO Display
		//TODO Give option to mine it
		if (false && GetMinesLeft() > 0) {
			SetMine (l);
		}
	}
	void SetMine(Landmark l) {
		Saved().mines.Add(new Mine(6, MyLocation(), l));
		//TODO Set a delayed notification for later
		GameSave.Save ();
	}
	void MergeResources(List<Resource> stuff) {
		Saved ().resources.ForEach (delegate(Resource r) {
			stuff.ForEach(delegate(Resource obj) {
				if(r.GetResourceType().Equals(obj.GetResourceType())) {
					//Merge newly harvested items
					r.AddResourcesOfThisType(obj.GetResourceValue());
					Debug.Log ("Adding "+obj.GetResourceValue()+" "+obj.GetResourceType()+"s");	
				}
			});
		});
		GameSave.Save ();
	}

	/**** MAP MANAGER CODE ****/
	public static Map		map;
	
	public Texture	LocationTexture;
	public Texture	MineTexture;
	public Texture  BeaconTexture;
	
	private float	guiXScale;
	private float	guiYScale;
	private Rect	guiRect;
	
	private bool 	isPerspectiveView = false;
	private float	perspectiveAngle = 30.0f;
	private float	destinationAngle = 0.0f;
	private float	currentAngle = 0.0f;
	private float	animationDuration = 0.5f;
	private float	animationStartTime = 0.0f;
	
	private List<Layer> layers;
	private int     currentLayerIndex = 0;

	private static int MINE = 314;
	private static int BEACON = 278;

	IEnumerator StartMapManager() {
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
			//yield WaitForSeconds (1);
			//Debug.Log (DateTime.Now.Ticks+", "+secondsElapsed+", "+maxWait+", "+(DateTime.Now.Ticks - secondsElapsed)+", "+((DateTime.Now.Ticks - secondsElapsed) < maxWait));

			//maxWait--;
		}
		// Service didn't initialize in 20 seconds
		if ((DateTime.Now.Ticks - secondsElapsed) > maxWait) {
			Debug.Log ("Timed out");
			return false;
		}
		// Connection has failed
		if (Input.location.status == LocationServiceStatus.Failed) {
			Debug.Log ("Unable to determine device location");
			return false ;
		}
		if(Input.location.status == LocationServiceStatus.Initializing) {
			Debug.Log ("Didn't wait long enough");
			return false;
		}
		if(Input.location.status == LocationServiceStatus.Stopped) {
			Debug.Log ("Stopped service");
			return false;
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

#if !UNITY_WEBPLAYER // FIXME: SQLite won't work in webplayer except if I find a full .NET 2.0 implementation (for free)
		// create an MBTiles tile layer
		bool error = false;
		// on iOS, you need to add the db file to the Xcode project using a directory reference
		string mbTilesDir = "MBTiles/";
		string filename = "UnitySlippyMap_World_0_8.mbtiles";
		string filepath = null;
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			filepath = Application.streamingAssetsPath + "/" + mbTilesDir + filename;
		}
		else if (Application.platform == RuntimePlatform.Android)
		{
			// Note: Android is a bit tricky, Unity produces APK files and those are never unzip on the device.
			// Place your MBTiles file in the StreamingAssets folder (http://docs.unity3d.com/Documentation/Manual/StreamingAssets.html).
			// Then you need to access the APK on the device with WWW and copy the file to persitentDataPath
			// to that it can be read by SqliteDatabase as an individual file
			string newfilepath = Application.temporaryCachePath + "/" + filename;
			if (File.Exists(newfilepath) == false)
			{
				Debug.Log("DEBUG: file doesn't exist: " + newfilepath);
				filepath = Application.streamingAssetsPath + "/" + mbTilesDir + filename;
				// TODO: read the file with WWW and write it to persitentDataPath
				WWW loader = new WWW(filepath);
				yield return loader;
				if (loader.error != null)
				{
					Debug.LogError("ERROR: " + loader.error);
					error = true;
				}
				else
				{
					Debug.Log("DEBUG: will write: '" + filepath + "' to: '" + newfilepath + "'");
					File.WriteAllBytes(newfilepath, loader.bytes);
				}
			}
			else
				Debug.Log("DEBUG: exists: " + newfilepath);
			filepath = newfilepath;
		}
        else
		{
			filepath = Application.streamingAssetsPath + "/" + mbTilesDir + filename;
		}
		
		if (error == false)
		{
            Debug.Log("DEBUG: using MBTiles file: " + filepath);
			MBTilesLayer mbTilesLayer = map.CreateLayer<MBTilesLayer>("MBTiles");
			mbTilesLayer.Filepath = filepath;
#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_3_6 || UNITY_3_7 || UNITY_3_8 || UNITY_3_9
            mbTilesLayer.gameObject.SetActiveRecursively(false);
#else
			mbTilesLayer.gameObject.SetActive(false);
#endif

            layers.Add(mbTilesLayer);
		}
        else
            Debug.LogError("ERROR: MBTiles file not found!");

#endif
		
		// create the location marker
		GameObject go = Tile.CreateTileTemplate().gameObject;
		go.renderer.material.mainTexture = LocationTexture;
		go.renderer.material.renderQueue = 4000;
		go.transform.localScale /= 27.0f;
		
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
		return PlaceMarker(name, MINE, coords);
	}
	public void BeaconDown() {
		Debug.Log ("BEACON BUTTON");
		if(BeaconMarker != null)
			RemoveMarker(BeaconMarker);
		BeaconMarker = PlaceBeacon("");
	}
	public Marker PlaceBeacon(String name) {
		return PlaceBeacon(name, map.CenterWGS84);
	}
	public Marker PlaceBeacon(String name, double[] coords) {

		Debug.Log ("Beacon "+name+" is being placed on the map at "+coords[0]+", "+coords[1]);
		return PlaceMarker(name, BEACON, coords);
	}
	public Marker PlaceMarker(String name, int type, double[] coords) {
		GameObject go = Tile.CreateTileTemplate(Tile.AnchorPoint.BottomCenter).gameObject;
		if(type == MINE)
			go.renderer.material.mainTexture = MineTexture;
		else 
			go.renderer.material.mainTexture = BeaconTexture;
		go.renderer.material.renderQueue = 4001;
		go.transform.localScale = new Vector3(0.70588235294118f, 1.0f, 1.0f);
		go.transform.localScale /= 7.0f;
        go.AddComponent<CameraFacingBillboard>().Axis = Vector3.up;

		GameObject markerGO;
		markerGO = Instantiate(go) as GameObject;
		return map.CreateMarker<Marker>(name, coords, markerGO);
	}
	public void RemoveMarker(Marker m) {
		map.RemoveMarker(m);
	}
	void InsertMineMarkers() {
		//Remove any mines that are on the map
		if(MineMarkers != null) {
			MineMarkers.ForEach(delegate(Marker obj) {
				RemoveMarker(obj);
			});
			MineMarkers.Clear();
		}
		//Get each mine, put it in a marker, add it to a list
		if(Saved ().mines != null) {
			Saved ().mines.ForEach(delegate(Mine obj) {
				MineMarkers.Add(PlaceMine("", obj.GetCoordinatesPlaced()));
			});
		}
	}
}
