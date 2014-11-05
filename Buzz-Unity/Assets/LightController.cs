using UnityEngine;
using System;
using System.Collections.Generic;

public class LightController : MonoBehaviour {
	Dictionary<String, Light> light_map;

		// Start from Unity 3D
		public void Start()
		{
			init();
		}

		
		// init
		private void init()
		{
			light_map = new Dictionary<String, Light>();
			Light[] lights = FindObjectsOfType(typeof(Light)) as Light[];
			foreach(Light light in lights)
			{
				light_map.Add(light.tag, light);
			}
		}

		/**
		 * Returns the Tag for the light by the serial port mapping
		 */
		public void getTag(int serial) {

		}

		public void sendCommand(String light_tag, int intensity) {
			light_map[light_tag].intensity = intensity;
		}
}


