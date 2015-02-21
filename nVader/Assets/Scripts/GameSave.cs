using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

[System.Serializable]
public class Savefile {
	public static Savefile current; 
	public List<Mine> mines;
	public List<Resource> resources;

	public Savefile() {
		List<Mine> mines = new List<Mine>();
		List<Resource> resources = new List<Resource>();
	}
	public Savefile(List<Mine> m, List<Resource> tp) {
		mines = m;
		resources = tp;
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
		FileStream file = File.Create (Application.persistentDataPath + "/"+fileName+"."+fileExtension);
		bf.Serialize (file, Savefile.current);
	}
	public static void Load() {
		Debug.Log ("Trying to load");
		if (File.Exists (Application.persistentDataPath + "/" + fileName + "." + fileExtension)) {
			RecreateLoad();
		} else {
			RecreateSave();
		}
		if (savedGame.mines == null || savedGame.resources == null)
			RecreateSave();

		Debug.Log ("Mines: " + savedGame.mines.Count + "; Resources: " + savedGame.resources.Count);
	}
	public static void RecreateLoad() {
		Debug.Log("Loading");
		BinaryFormatter bf = new BinaryFormatter ();
		FileStream file = File.Open (Application.persistentDataPath + "/" + fileName + "." + fileExtension, FileMode.Open);
		savedGame = (Savefile)bf.Deserialize (file);
		file.Close ();
	}
	public static void RecreateSave() {
		Debug.Log ("Creating new save file");
		savedGame = new Savefile();
		Save ();
	}
}



