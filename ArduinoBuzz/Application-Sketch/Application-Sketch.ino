#include <FiniteStateMachine.h>

const int anzahlMessungen = 20; // Mittelwert wird aus 10 Werten gebildet
const int intervall = 10; // Messungen alle 10 mS, ggf. anpassen
int messungen[anzahlMessungen]; // Array fuer Messwerte
int zeiger = 0; // Zeiger des aktuellen Messwerts
int gesamtSumme = 0; // aktueller Gesamtwert
int durchschnitt = 0; // Mittelwert
int dieseMessung = 0; // aktueller Messwert
boolean firstSet = 0; // Flag fuer ersten Durchlauf
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

// States
State Wait = State(Waiting);
State Ready = State(Readying);
State Lock = State(Locking);
FSM lightStateMachine = FSM(Wait); 


// the setup routine runs once when you press reset:
void setup() {
  // initialize serial communication at 9600 bits per second:
  Serial.begin(9600); 
}

int lastValidValue = 20;
// Waiting for User Input
void Waiting() {
  if(firstSet == 1) {
    if(abs(lastValidValue - durchschnitt) > 20) {
     //Serial.println("go readying state");
     lightStateMachine.transitionTo(Ready);
    }
  }
}

int readyCount = 0;
// Read user input until the same for n seconds
void Readying() {
  String logs ="count ";
  logs.concat(readyCount);
  //Serial.println(logs);
  
 //Serial.println("Ready State");
 if(abs(distAvgPrev - durchschnitt) < 10 && (durchschnitt > 30)) {
   readyCount++;
   if(readyCount > 200) {
     readyCount = 0;
     
     String debug = "go lock yourself with: ";
     debug.concat(durchschnitt);
     //Serial.println(debug);
     
     lightStateMachine.transitionTo(Lock);
   }
 }
}

// Do nothing for a few seconds
int waitLock = 0;
void Locking() {
    //delay(5000);
    waitLock++;
    if(waitLock > 200) {
      waitLock = 0;
      lightStateMachine.transitionTo(Wait);
    }
}

// the loop routine runs over and over again forever:
void loop() {
  
  if(!lightStateMachine.isInState(Lock)) {
    // read the input on distance sensor and potentiometer
    gesamtSumme = gesamtSumme - messungen[zeiger]; // substrahiere letzte Messung
    messungen[zeiger] = analogRead(distSensor); 
    gesamtSumme = gesamtSumme + messungen[zeiger]; // addiere Wert zur Summe   
    zeiger = zeiger + 1; // zur naechsten Position im Array                
    if (zeiger >= anzahlMessungen) // wenn Ende des Arrays erreicht ... zurueck zum Anfang
    {
      /*if (firstSet == 1) // wenn Array erstmalig aufgefuellt ...
      {
        if(abs(distAvgPrev - durchschnitt) > 10) {
          sendSerial = true;
        } else {
          //Serial.println("locking");
          //lightStateMachine.transitionTo(Lock);
        }
      }*/
      if (firstSet == 1) {
        lastValidValue = durchschnitt;
      }
      
      distAvgPrev = durchschnitt;
      firstSet = 1;
      zeiger = 0;
    }
  }
  
  if(lightStateMachine.isInState(Wait)) 
  {
    //Serial.println("in waiting state");
  }
  else if(lightStateMachine.isInState(Ready)) 
  {
    //Serial.println("in ready state");
  }
  else 
  {
    //Serial.println("In locking state");
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
  
  
  // Update Statemachine
  lightStateMachine.update();
  
  // delay in between reads for stability
  delay(intervall);
}
 
