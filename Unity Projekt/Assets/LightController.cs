using UnityEngine;
using System;
using System.Collections.Generic;
using AssemblyCSharp;

public class LightController : MonoBehaviour {
	const float INTENSITY_MIN = 0;
	const float INTENSITY_MAX = 4;

	const int SERIAL_MAX = 100;
	const int SERIAL_MIN = 0;
	const int SERIAL_DELTA_MAX = 100;

	Dictionary<String, List<Light>> light_map;
	Dictionary<String, float> intensity_map;
	Dictionary<String, float> intensity_map_prev;
	List<string> tag_list;
	ConcurrentQueue<Change> changes;

	public bool RelativeMode = false;

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
		intensity_map = new Dictionary<String, float> ();
		intensity_map_prev = new Dictionary<String, float> ();
		changes = new ConcurrentQueue<Change>();

		// Find all values
		Light[] lights = FindObjectsOfType(typeof(Light)) as Light[];
		foreach(Light light in lights)
		{
			if ( light.tag != null && light.tag != "" && light.tag != "Untagged" ) {
				// Set light intensity
				light.intensity = INTENSITY_MAX;

				if ( !light_map.ContainsKey(light.tag) ) {
					List<Light> list = new List<Light>();
					light_map.Add(light.tag, list);
					tag_list.Add(light.tag);
					intensity_map.Add (light.tag, light.intensity);
					intensity_map_prev.Add (light.tag, light.intensity);
				}

				light_map[light.tag].Add(light);
			}
		}

	}

	/**
	 * Returns the Tag for the light by the serial port mapping
	 */
	private String last_tag = "";
	public String serialToTag(int serial) {
		int index = 0;
		
		index += Math.Max(0,  Math.Min (light_map.Count - 1, serial * light_map.Count / SERIAL_MAX));

		// Get the tag
		String tag = tag_list [index];

		if ( last_tag != tag) {
			changeLight (tag);	
		}
		last_tag = tag;
		

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
		change.Intensity = intensity_map_prev[tag];
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
		if (RelativeMode == false) {
			serial = Math.Max (SERIAL_MIN, Math.Min (serial, SERIAL_MAX));

			float intensity = serial * (INTENSITY_MAX / SERIAL_MAX);
			// Inverse dine mama (the intensity)
			intensity = INTENSITY_MAX - intensity;

			return Math.Max (INTENSITY_MIN, Math.Min (intensity, INTENSITY_MAX));
		} else {
			float maxDeltaIntensity = (INTENSITY_MAX - INTENSITY_MIN) / 4;
			
			float intensity = serial * (maxDeltaIntensity / SERIAL_DELTA_MAX);
			
			return Math.Max (-maxDeltaIntensity, Math.Min (intensity, maxDeltaIntensity));
		}
	}
	
	/**
	 * Set the light intensity
	 */
	public void setLightIntensity(String light_tag, float intensity) {
		if (light_tag != "") {
			if (RelativeMode == false) {
				Change change = new Change ();
				change.Tag = light_tag;
				change.Intensity = intensity;

				changes.Enqueue (change);
			} else {
				Change change = new Change ();
				change.Tag = light_tag;
				change.Intensity = Math.Max (INTENSITY_MIN, Math.Min (intensity_map [light_tag] + intensity, INTENSITY_MAX));

				changes.Enqueue (change);
			}
		}
	}

	public void Update() {
		// If there are changes to do, do it..
		if (changes.Count > 0) 
		{
			Change change = changes.Dequeue();

			print("Change: Tag = " + change.Tag + "; Intensity = " + change.Intensity);

			// Save previous intensity (only if there is really a change in the intensity)
			if ( change.Intensity != intensity_map[change.Tag] && intensity_map[change.Tag] != 0)
				intensity_map_prev[change.Tag] = intensity_map[change.Tag];

			// Change the light intensities
			foreach(Light light in light_map[change.Tag] ) {
				light.intensity = Math.Max(INTENSITY_MIN, Math.Min(INTENSITY_MAX, change.Intensity));
			}
			intensity_map[change.Tag] = change.Intensity;
		}
	}

	// Input
	public void OnGUI() {
		if (Event.current != null && Event.current.type == EventType.KeyDown) {
			if ( Event.current.keyCode == KeyCode.R ) { 
				RelativeMode = !RelativeMode;
			} else if ( Event.current.keyCode == KeyCode.Plus ) {
				//if ( selected_tag != "" ) setLightIntensityRelative(selected_tag, 0.5f);
			} else if ( Event.current.keyCode == KeyCode.Minus ) {
				//if ( selected_tag != "" ) setLightIntensityRelative(selected_tag, -0.5f);
			} else {

				// Convert to numeric value for convenience :)
				int num = Event.current.keyCode - KeyCode.Alpha1;
				if ( num >= 0 && num < tag_list.Count ) {
					changeLight(tag_list[num]);
					last_tag = tag_list[num];
				} else if ( num == -1 ) {
					// Enable all lights if some are disabled
					if ( last_tag != "" ) {
						last_tag = "";
						foreach (String tag in tag_list) {
							enableLight(tag);
						}
					}
				}
			}
		}

		Rect rectObj=new Rect(40,10,500,100);
		
		GUIStyle style = new GUIStyle();
		style.alignment = TextAnchor.UpperLeft;
		
		Texture2D texture = new Texture2D (1, 1);
		texture.SetPixel (0, 0, new Color(255,255,255, 0.5f));
		texture.Apply ();
		
		style.normal.background = texture;
		style.fontSize = 30;
//		style.normal.textColor = new Color(225, 223, 255);

		GUI.Box(rectObj,"Mode: " + (RelativeMode? " RELATIVE" : "ABSOLUTE") + "\n" +
			"Selected Tag: " + last_tag
		        ,style);

	}
}


