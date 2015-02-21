using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

[System.Serializable]
public class Savefile {
	public static Savefile current; 
	public List<Vector2> mines;
	public int timePoints;

	public Savefile() {
		List<Vector2> mines = new List<Vector2>();
		int timePoints = 0;
	}
	public Savefile(List<Vector2> m, int tp) {
		mines = m;
		timePoints = tp;
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
		if(File.Exists(Application.persistentDataPath + "/"+fileName+"."+fileExtension)) {
			BinaryFormatter bf = new BinaryFormatter();
			FileStream file = File.Open(Application.persistentDataPath + "/"+fileName+"."+fileExtension, FileMode.Open);
			savedGame = (Savefile)bf.Deserialize(file);
			file.Close();
		}
	}
}


