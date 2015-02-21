using UnityEngine;
using System;
using System.IO;

[System.Serializable]
public class Mine {
	private int TimeRate;
	private int Year = 2015;
	private int YEAR_START = 3015;
	private long TimePlaced;
	private long HarvestTime;
	private double[] CoordinatesPlaced;
	private Landmark Location;

	public Mine(int points, int harvest, double[] d, Landmark l) {
		TimeRate = YEAR_START - points;
		TimePlaced = DateTime.Now.Ticks;
		HarvestTime = TimePlaced + harvest*10000/*ms*/*1000/*s*/*60*60;
		Location = l;
		CoordinatesPlaced = d;
	}
	public int GetTimeRate() {
		return TimeRate;
	}
	public double[] GetCoordinatesPlaced() {
		return CoordinatesPlaced;
	}
	public long GetHarvestTime() {
		return HarvestTime;
	}
	public long GetTimePlaced() {
		return TimePlaced;
	}
	public Landmark GetPairedLandmark() {
		return Location;
	}
}
