/*
 
    -----------------------
    UDP-Receive (send to)
    -----------------------
    // [url]http://msdn.microsoft.com/de-de/library/bb979228.aspx#ID0E3BAC[/url]
   
   
    // > receive
    // 127.0.0.1 : 8051
   
    // send
    // nc -u 127.0.0.1 8051
 
*/
using UnityEngine;
using System.Collections;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class UDPReceive : MonoBehaviour {

	// Light Controller
	LightController lightController;

	// Receiving Thread
	Thread receiveThread;
	
	// UDPClient object
	UdpClient client;

	public int port = 30000; // define > init
	
	// Infos
	public string lastReceivedUDPPacket="";

	public void Start()
	{
		init();
	}

	void OnGUI()
	{
		Rect rectObj=new Rect(40,10,200,400);
		GUIStyle style = new GUIStyle();
		style.alignment = TextAnchor.UpperLeft;
		GUI.Box(rectObj,"# UDPReceive\n127.0.0.1 "+port+" #\n"
		        + "shell> nc -u 127.0.0.1 : "+port+" \n"
		        + "\nLast Packet: \n"+ lastReceivedUDPPacket
		        ,style);
	}

	private void init()
	{
		// Endpunkt definieren, von dem die Nachrichten gesendet werden.
		print("UDPSend.init()");
		
		// define port
		//port = 30000;
		
		// status
		print("Sending to 127.0.0.1 : "+port);
		print("Test-Sending to this Port: nc -u 127.0.0.1  "+port+"");

		lightController = FindObjectOfType(typeof(LightController)) as LightController;
		
		// ----------------------------
		// Abhören
		// ----------------------------
		// Lokalen Endpunkt definieren (wo Nachrichten empfangen werden).
		// Einen neuen Thread für den Empfang eingehender Nachrichten erstellen.
		receiveThread = new Thread(
			new ThreadStart(ReceiveData));
		receiveThread.IsBackground = true;
		receiveThread.Start();
	}

	private void ReceiveData()
	{
		client = new UdpClient(port);
		while (true)
		{
			try
			{
				// Bytes empfangen.
				IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
				byte[] data = client.Receive(ref anyIP);
				
				// Bytes mit der UTF8-Kodierung in das Textformat kodieren.
				string text = Encoding.UTF8.GetString(data);

				// Do nothing if it starts with '#' 
				if (text.Trim()[0] == '#') {
					return;
				}

				// Den abgerufenen Text anzeigen.
				//print(">> " + text);
				
				// latest UDPpacket
				lastReceivedUDPPacket=text;

				// Dirty: Set Light Intensity, Better: Observer Pattern or something...
				//int serial = BitConverter.ToInt32(data, 0);
				String distValue = lastReceivedUDPPacket.Split('p')[0];
				int serial1 = Convert.ToInt32(distValue.Substring(1));
				print(">> DistValue: " + serial1);

				String potValue = lastReceivedUDPPacket.Split('p')[1];
				int serial2 = Convert.ToInt32(potValue);
				print(">> PotValue: " + serial2);

				String tag = lightController.serialToTag(serial2);
				print (tag);
				float intensity = lightController.serialToIntensity(serial1);
				lightController.setLightIntensity(tag, intensity);
			}
			catch (Exception err)
			{
				print(err.ToString());
			}
		}
	}
}