using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

[System.Serializable]
public class Savefile {
	public static Savefile current; 
	public int hi = 0;
	public List<Mine> mines;
	public List<Resource> resources;

	public Savefile() {
		hi = 1;
		List<Mine> mines = new List<Mine>();
		List<Resource> resources = new List<Resource>();
	}
}
//From http://gamedevelopment.tutsplus.com/tutorials/how-to-save-and-load-your-players-progress-in-unity--cms-20934
public class GameSave {
	private static string fileName = "saved"; 
	private static string fileExtension = "nvader";
	public static Savefile savedGame;
	//it's static so we can call it from anywhere
	public static void Save() {
		BinaryFormatter bf = new BinaryFormatter ();
		Debug.Log ("Creating binary fomratter");
		FileStream file = File.Create (Application.persistentDataPath + "/"+fileName+"."+fileExtension);
		Debug.Log ("Created file " + Application.persistentDataPath + "/" + fileName + "." + fileExtension);
		bf.Serialize (file, savedGame);
		Debug.Log ("Serialized the data");
		file.Close();
		Debug.Log ("Closed the file.");
	}
	public static void Load() {
		Debug.Log ("Trying to load");
		if (File.Exists (Application.persistentDataPath + "/" + fileName + "." + fileExtension)) {
			ReadFile();
		} else {
			RecreateSave();
		}
		if (savedGame.mines == null || savedGame.resources == null)
			RecreateSave();

		Debug.Log ("Mines: HI:");// + savedGame.mines.Count + "; Resources: " + savedGame.resources.Count
		Debug.Log (savedGame.mines == null);
		Debug.Log (savedGame.hi);
	}
	public static void ReadFile() {
		Debug.Log("Loading - V6+");
		Debug.Log (Application.persistentDataPath + "/" + fileName + "." + fileExtension);

		BinaryFormatter bf = new BinaryFormatter ();
		FileStream file = File.Open (Application.persistentDataPath + "/" + fileName + "." + fileExtension, FileMode.Open);
		//try {
			savedGame = (Savefile)bf.Deserialize (file);
			file.Close ();
		//} catch(IOException e) {
		//	Debug.Log ("Error loading: "+e.Message);
		//	RecreateSave();
		//}

	}
	public static void RecreateSave() {
		Debug.Log ("Creating new save file");
		savedGame = new Savefile();
		Debug.Log ("New File Created and assigned");
		Save ();
		Debug.Log ("Save written");
	}
}