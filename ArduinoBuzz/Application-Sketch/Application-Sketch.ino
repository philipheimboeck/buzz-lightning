#include <FiniteStateMachine.h>

const int anzahlMessungen = 20; // Mittelwert wird aus 10 Werten gebildet
const int intervall = 10; // Messungen alle 10 mS, ggf. anpassen
int messungen[anzahlMessungen]; // Array fuer Messwerte
int zeiger = 0; // Zeiger des aktuellen Messwerts
int gesamtSumme = 0; // aktueller Gesamtwert
int currentValue = 0; // Mittelwert
int dieseMessung = 0; // aktueller Messwert
boolean firstSet = 0; // Flag fuer ersten Durchlauf
const int sensorPin = A0; // Sensor an Pin 0 analog
int prevValue = -1;
int distSensor = A0;
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

// States
State Wait = State(Waiting);
State Send = State(Sending);
State Lock = State(Locking);
FSM lightStateMachine = FSM(Wait); 

// neuer code
const int countThreshold = 200;
const int threshold = 5;
const int thresholdWait = 20;
int lastTotalValues = 0;
int countLock = 0;
int led = 2;

void SendData();
// end neuer code

// the setup routine runs once when you press reset:
void setup() {
  // initialize serial communication at 9600 bits per second:
  Serial.begin(9600); 
  
  pinMode(led, OUTPUT);
}

// Waiting for User Input
void Waiting() {
  //Serial.println("# Wait");
  
  if(prevValue != -1 && abs(prevValue - currentValue) > thresholdWait) {
    // go to ready state
    lightStateMachine.transitionTo(Send);
  }
}

// Do nothing for a few seconds
void Locking() {
  digitalWrite(led, HIGH);
  Serial.println("# Lock");
  delay(4000);
  
  // Reset all data
  prevValue = -1;
  currentValue = 0;
  countLock = 0;
  
  digitalWrite(led, LOW);
  lightStateMachine.transitionTo(Wait);
}

void Sending() {
  //Serial.println("# Send");
  // only send changes if there are any new
  if(abs(prevValue - currentValue) > threshold) {
    // reset counter
    countLock = 0;
    SendData();
  } else {
    countLock++;
    
    // wait until user stayed for 2s in same position
    if(countLock > countThreshold) {
      lightStateMachine.transitionTo(Lock);
    }
  }
}

// the loop routine runs over and over again forever:
void loop() {  
  if(!lightStateMachine.isInState(Lock)) {
    // read the input on distance sensor and potentiometer
    gesamtSumme = gesamtSumme - messungen[zeiger]; // substrahiere letzte Messung
    int readDistValue = analogRead(distSensor);
    if(readDistValue < 350) readDistValue = 350;
    if(readDistValue > 650) readDistValue = 650;

    messungen[zeiger] = map(readDistValue, 350, 650, 0, 100); 
    gesamtSumme = gesamtSumme + messungen[zeiger]; // addiere Wert zur Summe   
    zeiger = zeiger + 1; // zur naechsten Position im Array                
    if (zeiger >= anzahlMessungen) // wenn Ende des Arrays erreicht ... zurueck zum Anfang
    {
      // Save last value
      prevValue = currentValue;
      //Serial.println(currentValue);
      zeiger = 0;
    }
    
    currentValue = gesamtSumme / anzahlMessungen;
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
        SendDataPot();
      }
    }
    
    potAvgPrev = potAvg;
    potStatus = 1;
    potPointer = 0;
  }
  
  // Calculate average for distance sensor
  potAvg = potSum / anzahlMessungen;
  
  // Update Statemachine
  lightStateMachine.update();
  
  // delay in between reads for stability
  delay(intervall);
}
 
// Send Stuff to unity
void SendData() {
  // Print all values
  String str ="d";
  str.concat(currentValue);
  Serial.println(str);
}

// Send Stuff to unity
void SendDataPot() {
  // Print all values
  String str ="p";
  str.concat(map(potAvg, 1, 1024, 1, 100));
  Serial.println(str);
}
