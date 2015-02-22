using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
[System.Serializable]
public class Landmark {
	private string Title;
	private string Description;
	private List<Resource> Resources;
	private int BuildYear;

	public Landmark(String t, String d, List<Resource> r, int by) {
		Title = t;
		Description = d;
		Resources = r;
		BuildYear = by; 
	}
	public String GetTitle() {
		return Title;
	}
	public String GetDescription() {
		return Description;
	}
	public List<Resource> GetResources () {
		return Resources;
	}
	public int GetBuildYear () {
		return BuildYear;
	}
}