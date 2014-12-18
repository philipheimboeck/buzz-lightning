# Projekt: Buzz Lightning

## Idee

Die Idee für unser Projekt ist eine Lichtsteuerung, mit der man die Intensität mehrerer Lichter mithilfe eines Distanzsensors regeln kann.

Dabei soll zwischen zwei Modis unterschieden werden:

+ Steuerung der Intensität mit absoluten Werten
+ Steuerung der Intensität relativ zu aktuellem Wert

Außerdem wollen wir mithilfe eines Drehpotentiometer zwischen mehreren Lichtern auswählen können.

Als Szenario haben wir eine reale Situation gewählt: Die Beleuchtung einer Wohnung mit mehreren Lichter. Jedoch haben wir dieses Szenario etwas verändert, damit die Lichtsteurung besser wahrnehmbar ist: 

+ Überflüssige Texturen werden weggelassen
+ Es gibt keine Lichteinflüsse von außen
+ Die Lichter sind in einem rötlichen Farbton

### Kriterien
Wir haben uns folgende Kriterien gesetzt die wir mit Buzz Lightning erreichen und erfüllen wollen. Dies gibt auch einen kleinen Ausblick bzw. Begründung für die weiter folgenden Schritte während der Entwicklung. 

+ Einfachheit der Bedienung
	+ Anwender soll verschiedene Lichtquellen schnell anpassen und steuern können
	+ Ungewollte Angaben abfangen durch ein definiertes Start und Ende der Eingabe
+ Umsetzung der Lichtsteuerung
	+ keine Sprunghafte Änderung des Lichtes
	+ Natürliches Verhalten von Bewegung zu Steuerung des Lichtes
+ Verständlichkeit für Anwender 
	+ Ist das eigentliche Bedienkonzept verständlich?
	+ Wirkungsverhalten verständlich? Was passiert bei einem bestimmten Eingabeverhalten?
	+ Schnelle Änderungen durch schnellere Bewegungen
+ Praxisbezug
	+ Gibt es Anwendungsgebiete in der diese Steuerung klare Vorteile gegenüber einer herkömmlichen Steuerung hat? 

## Umsetzung

### Controlleraufbau

Zur Eingabe werden ein Distanzsensor und ein Drehpotentiometer verwendet.
Diese werden wie in folgenden Grafiken an einen Arduiono Leonardo angeschlossen, der die Steuerung übernimmt.

![Steckplatine](Plan_Steckplatine.png)

![Steckplatine](Plan_Schaltplan.png)

### Software

#### Arduino

Der Arduino regelt die Eingabewerte und übermittelt diese über den Seriellen Ausgang an unsere Software.

Dabei werden zuerst noch die Werte geglättet. Außerdem werden die Werte immer nur dann übermittelt, wenn sie einen bestimmten, vorher festgelegten Grenzwert überschreiten.

```c
//Todo Some Code...
```

Außerdem war es uns wichtig, dass wir erkennen können ob die Eingabe gewollt oder ungewollt erfolgt ist.

Um die Änderungen zu unterscheiden können, haben wir uns für ein Lock-Out System entschieden. Der AnwenderInnen können die Regelung beenden, sobald sie kurz in einer Position verharren. Das System sperrt dann diese Position für einige Sekunden bevor weitere Änderungen übernommen werden können.

```c
// Code
```

#### Java Serial2UDP

Aufgrund von Problemen mit Mono und der Seriellen Schnittstelle unter OSX haben wir ein Java Programm verwendet, dass die Seriellen Schnittstellen überwacht und sämtlichen Werte über UDP an unsere Unitysoftware weiterleitet.

#### Unity

Unser Unity Programm besteht aus zwei Tweilen: Einem UDP-Empfänger und einem Lichtcontroller.

##### UDP Empfänger
Der UDP Empfänger hört auf den UDP-Eingabeport und leitet die empfangenen Daten an den Lichtcontroller weiter.

```c#
private void ReceiveData() {
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
				
			String distValue = lastReceivedUDPPacket.Split('p')[0];
			int serial1 = Convert.ToInt32(distValue.Substring(1));
				
			String potValue = lastReceivedUDPPacket.Split('p')[1];
			int serial2 = Convert.ToInt32(potValue);
				
			String tag = lightController.serialToTag(serial2);
			float intensity = lightController.serialToIntensity(serial1);
			lightController.setLightIntensity(tag, intensity);
		}
		catch (Exception err)
		{
			print(err.ToString());
		}
	}
}
```

##### Light Controller

Der Light Controller kennt alle Lichter und wandelt die Seriellen Werte in verarbeitbaren Daten um.

Das bedeutet er übernimmt die Steuerung des Lichtes in dem er die Werte des Potentiometers umrechnet und ein Licht anwählt.

Die Lichtintensität wird ebenfalls anhand von Eingabedaten berechnet. Dabei wird der Sensorwert des Distanzsensors in Lichtintensität umgerechnet. Folgende Formel wird hierfür verwendet:

`Intensity = Sensor_Value * (INTENSITY_MAX / SENSOR_MAX)`

Für die Steuerung stehen daher vier Funktionen zur Verfügung:
+ **serialToTag** - rechnet den Sensorwert in einen Lichttag um für die Auswahl der Lichter
+ **serialToIntensity** - rechnet den Sensorwert in eine Intensität um
+ **setLightIntensity** - setzt die Intensität der Lichter mit dem Tag
+ **setLightIntensityRelative** - setzt die Intensität der Lichter relativ


```c#
/**
 * Returns the Tag for the light by the serial port mapping
 */
public String serialToTag(int serial) {
	int index = 0;
	
	index += Math.Max(0,  Math.Min (light_map.Count - 1, serial * light_map.Count / SERIAL_MAX));

	// Get the tag
	String tag = tag_list [index];

	// Highlight the selected light
	changeLight (tag);

	// Return the tag
	return tag;
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
	change.Intensity = light_map^light_tag][0].intensity + delta_intensity;
	
	changes.Enqueue (change);
}
```

Wie oben zu sehen ist, werden die Änderungen nicht direkt übernommen sondern in eine Changes-Queue gespeichert.
Diese Queue wird dann innerhalb der Update Funktion abgeabeitet.

```c#
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
```

### 3D-Modelle

## Ergebnis
Wir haben im ersten Schritt unsere eigenen Kriterien bewertet und mit dem Ergebnis des Projektes verglichen und.

### Vor und Nachteile einer absoluten und relativen Steuerung
**Absolute Steuerung**  
+ es gibt pro Steuerungsvorgang nur ein Lock-Zyklus  
+ die Steuerung erlaubt ein leichtes Ein- und Ausschalten der Lichtquelle  
- nur ein begrenzte Genauigkeit beim Dimmen einer Lichtquelle

**Relative Steuerung**  
+ im Gegensatz zur absoluten Steuerung ist durch die Aufteilung in mehrere Schritte der Regulierungsbereich kleiner und somit ist ein genaueres adjustieren möglich   
- erzeugt dadurch aber mehrere Lock-Zyklen pro Steuerungsvorgang und somit längere Wartezeiten für den Anwender   
- Ein- und Ausschalten kann mitunter 3-4 Steuerungszyklen benötigen

### Benutzerakzeptanz
Um die gesetzten Kriterien repräsentativ zu bewerten haben wir einen Versuch mit 10 Personen durchgeführt. Wir haben dazu vor allem die Kriterien in Bezug auf Einfachheit und Verständlichkeit der Anwender betrachtet. Anhand von dessen Reaktionen und Rückmeldungen ist folgende Analyse enstanden. 

**Einfachheit der Bedienung**  
Die Geschwindigkeit der Schaltung ist vor allem im relativen Modus etwas schwerfällig und kann länger dauern als ein gewöhnlicher Dimmer-Taster. Im Fall des absoluten Modus sind die Warte-Zwischenschritte nicht nötig. 

**Verständlichkeit**   
Die Schaltung ist nach einer kurzen Erklärung für alle Probanden verständlich und die Logik "verdunkeln durch Nähe" und "erhellen durch Ferne" 
