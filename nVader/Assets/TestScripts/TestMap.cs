// 
//  TestMap.cs
//  
//  Author:
//       Jonathan Derrough <jonathan.derrough@gmail.com>
//  
//  Copyright (c) 2012 Jonathan Derrough
// 
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
// 
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using UnityEngine;

using System;

using UnitySlippyMap;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using ProjNet.Converters.WellKnownText;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class TestMap : MonoBehaviour
{
	private Map		map;
	
	public Texture	LocationTexture;
	public Texture	MarkerTexture;
	
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
	
	bool Toolbar(Map map)
	{
		GUI.matrix = Matrix4x4.Scale(new Vector3(guiXScale, guiXScale, 1.0f));
		
		GUILayout.BeginArea(guiRect);
		
		GUILayout.BeginHorizontal();
		
		//GUILayout.Label("Zoom: " + map.CurrentZoom);
		
		bool pressed = false;
        if (GUILayout.RepeatButton("+", GUILayout.ExpandHeight(true)))
		{
			map.Zoom(1.0f);
			pressed = true;
		}
        if (Event.current.type == EventType.Repaint)
        {
            Rect rect = GUILayoutUtility.GetLastRect();
            if (rect.Contains(Event.current.mousePosition))
                pressed = true;
        }

        if (GUILayout.Button("2D/3D", GUILayout.ExpandHeight(true)))
		{
			if (isPerspectiveView)
			{
				destinationAngle = -perspectiveAngle;
			}
			else
			{
				destinationAngle = perspectiveAngle;
			}
			
			animationStartTime = Time.time;
			
			isPerspectiveView = !isPerspectiveView;
		}
        if (Event.current.type == EventType.Repaint)
        {
            Rect rect = GUILayoutUtility.GetLastRect();
            if (rect.Contains(Event.current.mousePosition))
                pressed = true;
        }

        if (GUILayout.Button("Center", GUILayout.ExpandHeight(true)))
        {
            map.CenterOnLocation();
        }
        if (Event.current.type == EventType.Repaint)
        {
            Rect rect = GUILayoutUtility.GetLastRect();
            if (rect.Contains(Event.current.mousePosition))
                pressed = true;
        }

        string layerMessage = String.Empty;
        if (map.CurrentZoom > layers[currentLayerIndex].MaxZoom)
            layerMessage = "\nZoom out!";
        else if (map.CurrentZoom < layers[currentLayerIndex].MinZoom)
            layerMessage = "\nZoom in!";
        if (GUILayout.Button(((layers != null && currentLayerIndex < layers.Count) ? layers[currentLayerIndex].name + layerMessage : "Layer"), GUILayout.ExpandHeight(true)))
        {
#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_3_6 || UNITY_3_7 || UNITY_3_8 || UNITY_3_9
            layers[currentLayerIndex].gameObject.SetActiveRecursively(false);
#else
			layers[currentLayerIndex].gameObject.SetActive(false);
#endif
            ++currentLayerIndex;
            if (currentLayerIndex >= layers.Count)
                currentLayerIndex = 0;
#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_3_6 || UNITY_3_7 || UNITY_3_8 || UNITY_3_9
            layers[currentLayerIndex].gameObject.SetActiveRecursively(true);
#else
			layers[currentLayerIndex].gameObject.SetActive(true);
#endif
            map.IsDirty = true;
        }

        if (GUILayout.RepeatButton("-", GUILayout.ExpandHeight(true)))
		{
			map.Zoom(-1.0f);
			pressed = true;
		}
        if (Event.current.type == EventType.Repaint)
        {
            Rect rect = GUILayoutUtility.GetLastRect();
            if (rect.Contains(Event.current.mousePosition))
                pressed = true;
        }
		
		GUILayout.EndHorizontal();
					
		GUILayout.EndArea();
		
		return pressed;
	}
	
	private
#if !UNITY_WEBPLAYER
        IEnumerator
#else
        void
#endif
        Start()
	{
        // setup the gui scale according to the screen resolution
        guiXScale = (Screen.orientation == ScreenOrientation.Landscape ? Screen.width : Screen.height) / 480.0f;
        guiYScale = (Screen.orientation == ScreenOrientation.Landscape ? Screen.height : Screen.width) / 640.0f;
		// setup the gui area
		guiRect = new Rect(16.0f * guiXScale, 4.0f * guiXScale, Screen.width / guiXScale - 32.0f * guiXScale, 32.0f * guiYScale);

		// create the map singleton
		map = Map.Instance;
		map.CurrentCamera = Camera.main;
		map.InputDelegate += UnitySlippyMap.Input.MapInput.BasicTouchAndKeyboard;
		map.CurrentZoom = 15.0f;
		// 9 rue Gentil, Lyon
		//map.CenterWGS84 = new double[2] { 4.83527, 45.76487 };
		//Now use GPS

		map.UseLocation = true;
		map.InputsEnabled = true;
		map.ShowGUIControls = true;
		map.IsDirty = true;
		map.UpdateCenterWithLocation = true;
<<<<<<< HEAD
		Debug.Log ("Use location? " + map.UseLocation);
		UnityEngine.Input.location.Start ();
		//while(UnityEngine.Input.location.status != LocationServiceStatus.Running)
			Debug.Log("Waiting for location");
=======
		// Start service before querying location
		Input.location.Start ();
		Debug.Log ("Is location enabled by user?" + Input.location.isEnabledByUser);
		Debug.Log ("Use location? " + map.UseLocation);
		Debug.Log ("Is location running? " + (UnityEngine.Input.location.status == LocationServiceStatus.Running));


		// Wait until service initializes
		int maxWait = 5 * 100 * 1000/*ms*/* 1000;

		Debug.Log ("Init? " + (Input.location.status
			== LocationServiceStatus.Initializing));
		
		long secondsElapsed = DateTime.Now.Ticks;
		Debug.Log (DateTime.Now.Ticks+", "+secondsElapsed+", "+maxWait+", "+(DateTime.Now.Ticks - secondsElapsed)+", "+((DateTime.Now.Ticks - secondsElapsed) < maxWait));
		while (Input.location.status
		       == LocationServiceStatus.Initializing && (DateTime.Now.Ticks - secondsElapsed) < maxWait) {
			//yield WaitForSeconds (1);
			Debug.Log (DateTime.Now.Ticks+", "+secondsElapsed+", "+maxWait+", "+(DateTime.Now.Ticks - secondsElapsed)+", "+((DateTime.Now.Ticks - secondsElapsed) < maxWait));

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
>>>>>>> dfae518ff062e076caea4a0fd8d9156b70cb392a
		Debug.Log ("Is location running? " + (UnityEngine.Input.location.status == LocationServiceStatus.Running));
		Debug.Log ("Updating location? " + map.UpdateCenterWithLocation);
		map.CenterOnLocation ();
		map.CenterWGS84 = new double[2] { UnityEngine.Input.location.lastData.longitude, UnityEngine.Input.location.lastData.latitude };
		Debug.Log ("Getting location " + UnityEngine.Input.location.lastData.longitude + ", " + UnityEngine.Input.location.lastData.latitude);
		Debug.Log ("Found WGS84 location " + map.CenterWGS84[0]+", "+map.CenterWGS84[1]);


		map.GUIDelegate += Toolbar;

        layers = new List<Layer>();

		// create an OSM tile layer
        OSMTileLayer osmLayer = map.CreateLayer<OSMTileLayer>("OSM");
        osmLayer.BaseURL = "http://a.tile.openstreetmap.org/";
		
        layers.Add(osmLayer);

		// create a WMS tile layer
        WMSTileLayer wmsLayer = map.CreateLayer<WMSTileLayer>("WMS");
        //wmsLayer.BaseURL = "http://129.206.228.72/cached/osm?"; // http://www.osm-wms.de : seems to be of very limited use
        //wmsLayer.Layers = "osm_auto:all";
        wmsLayer.BaseURL = "http://vmap0.tiles.osgeo.org/wms/vmap0";
        wmsLayer.Layers = "basic";
#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_3_6 || UNITY_3_7 || UNITY_3_8 || UNITY_3_9
        wmsLayer.gameObject.SetActiveRecursively(false);
#else
		wmsLayer.gameObject.SetActive(false);
#endif

        layers.Add(wmsLayer);

		// create a VirtualEarth tile layer
        VirtualEarthTileLayer virtualEarthLayer = map.CreateLayer<VirtualEarthTileLayer>("VirtualEarth");
        // Note: this is the key UnitySlippyMap, DO NOT use it for any other purpose than testing
        virtualEarthLayer.Key = "ArgkafZs0o_PGBuyg468RaapkeIQce996gkyCe8JN30MjY92zC_2hcgBU_rHVUwT";
#if UNITY_WEBPLAYER
        virtualEarthLayer.ProxyURL = "http://reallyreallyreal.com/UnitySlippyMap/demo/veproxy.php";
#endif
#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_3_6 || UNITY_3_7 || UNITY_3_8 || UNITY_3_9
        virtualEarthLayer.gameObject.SetActiveRecursively(false);
#else
		virtualEarthLayer.gameObject.SetActive(false);
#endif

        layers.Add(virtualEarthLayer);

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

        // create some test 2D markers
		GameObject go = Tile.CreateTileTemplate(Tile.AnchorPoint.BottomCenter).gameObject;
		go.renderer.material.mainTexture = MarkerTexture;
		go.renderer.material.renderQueue = 4001;
		go.transform.localScale = new Vector3(0.70588235294118f, 1.0f, 1.0f);
		go.transform.localScale /= 7.0f;
        go.AddComponent<CameraFacingBillboard>().Axis = Vector3.up;
		
		GameObject markerGO;
		markerGO = Instantiate(go) as GameObject;
		map.CreateMarker<Marker>("test marker - 9 rue Gentil, Lyon", new double[2] { 4.83527, 45.76487 }, markerGO);

		markerGO = Instantiate(go) as GameObject;
		map.CreateMarker<Marker>("test marker - 31 rue de la Bourse, Lyon", new double[2] { 4.83699, 45.76535 }, markerGO);
		
		markerGO = Instantiate(go) as GameObject;
		map.CreateMarker<Marker>("test marker - 1 place St Nizier, Lyon", new double[2] { 4.83295, 45.76468 }, markerGO);

		DestroyImmediate(go);
		
		// create the location marker
		go = Tile.CreateTileTemplate().gameObject;
		go.renderer.material.mainTexture = LocationTexture;
		go.renderer.material.renderQueue = 4000;
		go.transform.localScale /= 27.0f;
		
		markerGO = Instantiate(go) as GameObject;
		map.SetLocationMarker<LocationMarker>(markerGO);

		DestroyImmediate(go);
	}
	
	void OnApplicationQuit()
	{
		map = null;
	}
	
	void Update()
	{
		if (destinationAngle != 0.0f)
		{
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
	
#if DEBUG_PROFILE
	void LateUpdate()
	{
		Debug.Log("PROFILE:\n" + UnitySlippyMap.Profiler.Dump());
		UnitySlippyMap.Profiler.Reset();
	}
#endif
}
