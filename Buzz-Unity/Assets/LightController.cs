using UnityEngine;
using System;
using System.Collections.Generic;
using AssemblyCSharp;

public class LightController : MonoBehaviour {
	const float INTENSITY_MIN = 0;
	const float INTENSITY_MAX = 8;

	const int SERIAL_MAX = 100;
	const int SERIAL_MIN = 1;

	Dictionary<String, List<Light>> light_map;
	List<string> tag_list;
	ConcurrentQueue<Change> changes;

	// Start from Unity 3D
	public void Start()
	{
		init();
	}

	
	// init
	private void init()
	{
		light_map = new Dictionary<String, List<Light>>();
		tag_list = new List<String>();
		changes = new ConcurrentQueue<Change>();

		// Find all values
		Light[] lights = FindObjectsOfType(typeof(Light)) as Light[];
		foreach(Light light in lights)
		{
			if ( light.tag != null && light.tag != "" && light.tag != "Untagged" ) {
				if ( !light_map.ContainsKey(light.tag) ) {
					List<Light> list = new List<Light>();
					light_map.Add(light.tag, list);
					tag_list.Add(light.tag);
				}

				light_map[light.tag].Add(light);
			}
		}

	}

	/**
	 * Returns the Tag for the light by the serial port mapping
	 */
	public String serialToTag(int serial) {
		int index = 0;
		
		index += Math.Max(0,  Math.Min (light_map.Count - 1, serial * light_map.Count / SERIAL_MAX));

		// Get the tag
		String tag = tag_list [index];

		changeLight (tag);

		// Return the tag
		return tag;
	}

	private void changeLight(String tag) {
		// Change the color of the lights
		foreach (String t in tag_list) {
			disableLight(t);
		}
		enableLight (tag);
		print ("Selected light: " + tag);
	}

	private void enableLight(String tag) {
		Change change = new Change();
		change.Tag = tag;
		change.Intensity = 4; // Todo Get old intensity value
		changes.Enqueue (change);
	}

	private void disableLight(String tag) {
		Change change = new Change();
		change.Tag = tag;
		change.Intensity = 0;
		changes.Enqueue (change);
	}

	/**
	 * Returns the intensity for the serial value
	 */
	public float serialToIntensity(int serial) {
		serial = Math.Max (SERIAL_MIN, Math.Min (serial, SERIAL_MAX));

		float intensity = serial * (INTENSITY_MAX / SERIAL_MAX);

		return Math.Max (INTENSITY_MIN, Math.Min (intensity, INTENSITY_MAX));
	}
	
	/**
	 * Set the light intensity
	 */
	public void setLightIntensity(String light_tag, float intensity) {
		Change change = new Change ();
		change.Tag = light_tag;
		change.Intensity = intensity;

		changes.Enqueue (change);
	}

	/*
	 * Set the light intensity relative
	 */
	public void setLightIntensityRelative(String light_tag, float delta_intensity) {
		Change change = new Change ();
		change.Tag = light_tag;
		change.Intensity = light_map[light_tag][0].intensity + delta_intensity;
		
		changes.Enqueue (change);
	}

	public void Update() {
		// If there are changes to do, do it..
		if (changes.Count > 0) 
		{
			Change change = changes.Dequeue();

			// Change the light intensities
			foreach(Light light in light_map[change.Tag] ) {
				light.intensity = Math.Max(INTENSITY_MIN, Math.Min(INTENSITY_MAX, change.Intensity));
			}
		}
	}

	// Input
	private String selected_tag = ""; 
	public void OnGUI() {
		if (Event.current != null && Event.current.type == EventType.KeyDown) {

			if ( Event.current.keyCode == KeyCode.Plus ) {
				if ( selected_tag != "" ) setLightIntensityRelative(selected_tag, 0.5f);
			} else if ( Event.current.keyCode == KeyCode.Minus ) {
				if ( selected_tag != "" ) setLightIntensityRelative(selected_tag, -0.5f);
			} else {

				// Convert to numeric value for convenience :)
				int num = Event.current.keyCode - KeyCode.Alpha1;
				if ( num >= 0 && num < tag_list.Count ) {
					changeLight(tag_list[num]);
					selected_tag = tag_list[num];
				} else if ( num == -1 ) {
					// Enable all lights
					foreach (String tag in light_map.Keys) {
						enableLight(tag);
					}
				}
			}
		}
	}
}


