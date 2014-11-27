const int anzahlMessungen = 20; // Mittelwert wird aus 10 Werten gebildet
const int intervall = 10; // Messungen alle 10 mS, ggf. anpassen
int messungen[anzahlMessungen]; // Array fuer Messwerte
int zeiger = 0; // Zeiger des aktuellen Messwerts
int gesamtSumme = 0; // aktueller Gesamtwert
int durchschnitt = 0; // Mittelwert
int dieseMessung = 0; // aktueller Messwert
boolean status = 0; // Flag fuer ersten Durchlauf
const int sensorPin = A0; // Sensor an Pin 0 analog
int distAvgPrev = 0;

int distSensor = A0;

boolean sendSerial = false;
int valPotentio = 0;

// Potentiometer
int potMeasurements[anzahlMessungen];
int potPointer = 0;
int potSum = 0;
int potAvg = 0;
int potValue = 0;
boolean potStatus = 0;
int potAvgPrev = 0;

int potSensor = A3;

// the setup routine runs once when you press reset:
void setup() {
  // initialize serial communication at 9600 bits per second:
  Serial.begin(9600); 
}


// the loop routine runs over and over again forever:
void loop() {
  // read the input on distance sensor and potentiometer
  
  gesamtSumme = gesamtSumme - messungen[zeiger]; // substrahiere letzte Messung
  messungen[zeiger] = analogRead(distSensor); // lese Sensor
  gesamtSumme = gesamtSumme + messungen[zeiger]; // addiere Wert zur Summe   
  zeiger = zeiger + 1; // zur naechsten Position im Array                
  if (zeiger >= anzahlMessungen) // wenn Ende des Arrays erreicht ... zurueck zum Anfang
  {
    if (status == 1) // wenn Array erstmalig aufgefuellt ...
    {
      //if((durchschnitt - durchschnittPrev) > 50) {
       //durchschnittPrev = durchschnitt;
       //sendSerial = true;
      //}
      
      if(abs(distAvgPrev - durchschnitt) > 30) {
        sendSerial = true;
      }
    }
    distAvgPrev = durchschnitt;
    status = 1;
    zeiger = 0;
  }
  
  // Potentiometer Equalization
  potSum = potSum - potMeasurements[potPointer];
  potMeasurements[potPointer] = analogRead(potSensor);
  potSum = potSum + potMeasurements[potPointer];
  potPointer = potPointer + 1;
  if(potPointer >= anzahlMessungen) 
  {
    if(potStatus == 1) {
      if(abs(potAvgPrev - potAvg) > 10) {
        sendSerial = true;
      }
    }
    
    potAvgPrev = potAvg;
    potStatus = 1;
    potPointer = 0;
  }
  
  if(sendSerial) {
    // Print all values
    String str ="d";
    str.concat(map(durchschnitt, 1, 1024, 1, 100));
    str.concat("p");
    str.concat(map(potAvg, 1, 1024, 1, 100));
    Serial.println(str);
    sendSerial = false;
  }
  
  // Calculate average for distance sensor
  durchschnitt = gesamtSumme / anzahlMessungen;
  potAvg = potSum / anzahlMessungen;
  
  // delay in between reads for stability
  delay(intervall);
}
 
