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

		Light[] lights = FindObjectsOfType(typeof(Light)) as Light[];
		foreach(Light light in lights)
		{
			if ( light.tag != null && light.tag != "" ) {
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
		
		index += Math.Max(0, serial * (Math.Min (light_map.Count, light_map.Count / SERIAL_MAX - 1)));
		return tag_list[index];
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

	public void Update() {
		if (changes.Count > 0) 
		{
			Change change = changes.Dequeue();

			foreach(Light light in light_map[change.Tag] ) {
				light.intensity = change.Intensity;
			}
		}
	}
}


