using UnityEngine;
using System;
using System.Collections.Generic;
using AssemblyCSharp;

public class LightController : MonoBehaviour {
	const float INTENSITY_MIN = 0;
	const float INTENSITY_MAX = 8;

	const int SERIAL_MAX = 1023;
	const int SERIAL_MIN = 0;

	Dictionary<String, Light> light_map;
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
		light_map = new Dictionary<String, Light>();
		tag_list = new List<String>();
		changes = new ConcurrentQueue<Change>();

		Light[] lights = FindObjectsOfType(typeof(Light)) as Light[];
		foreach(Light light in lights)
		{
			if ( light.tag != null && light.tag != "" ) {
				light_map.Add(light.tag, light);
				tag_list.Add(light.tag);
			}
		}
	}

	/**
	 * Returns the Tag for the light by the serial port mapping
	 */
	public String serialToTag(int serial) {
		return tag_list[0];
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

			light_map[change.Tag].intensity = change.Intensity;
		}
	}
}


