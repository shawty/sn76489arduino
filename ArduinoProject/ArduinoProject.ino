
#define CLOCK  10
#define WE     2

#define PIN_D0 29
#define PIN_D1 28
#define PIN_D2 27
#define PIN_D3 26
#define PIN_D4 25
#define PIN_D5 24
#define PIN_D6 23
#define PIN_D7 22

int dataPtr = 0;
int delayCounter = 0;
int delayCounterTimer = 0;
int delayCounterTimerMax = 4;

void setup() {

  // This is only used if you want to attempt to let the Arduino generate the 4Mhz clock
  // that the SN76489 requires to operate.  NOTE: Beacuse this uses a custom PWM to generate it
  // It MUST use pin 10 as in the clock define above.  If you have a hardware 4mhz clock
  // device on the board however, you can leave the next line commented and not bother connecting a wire
  //setupFourMhzTimer(); 

  // Set up the R/W pin
  pinMode(WE, OUTPUT);
  digitalWrite(WE, HIGH);

  // Set up the Data I/O pins
  pinMode(PIN_D0, OUTPUT);
  pinMode(PIN_D1, OUTPUT);
  pinMode(PIN_D2, OUTPUT);
  pinMode(PIN_D3, OUTPUT);
  pinMode(PIN_D4, OUTPUT);
  pinMode(PIN_D5, OUTPUT);
  pinMode(PIN_D6, OUTPUT);
  pinMode(PIN_D7, OUTPUT);

  // Set them all to logic 0
  digitalWrite(PIN_D0, LOW);
  digitalWrite(PIN_D1, LOW);
  digitalWrite(PIN_D2, LOW);
  digitalWrite(PIN_D3, LOW);
  digitalWrite(PIN_D4, LOW);
  digitalWrite(PIN_D5, LOW);
  digitalWrite(PIN_D6, LOW);
  digitalWrite(PIN_D7, LOW);

  // Initialise the SN76489
  InitiliseSoundChip();

  // and the inbound default serial port
  Serial.begin(115200);
}

void setupFourMhzTimer()
{
  // Iv'e no idea how this works :-)  I grabbed it off the arduino forums
  // I made an attempt to understand it, but every tweak I made I killed it!!!
  // it works, it gives 4Mhz, so I'm just leaving it at that.
  pinMode(CLOCK, OUTPUT);
  TCCR2A = ((1 << WGM21) | (1 << COM2A0)); //0x23;
  TCCR2B = (1 << CS20); //0x09;
  
  OCR2A  = 0x02;
  TIMSK2 = 0x00;
  
  OCR2B  = 0x01;
}

void InitiliseSoundChip()
{
  // Do the well known BOOO-BEEP BBC Initialisation sound.
  // Yes, it does actually serve a purpose :-)

  // The low tone that all BBCs play on startup (Before the higher beep all owners are used to) is actually
  // The default tone on channel 2 of the SN76489 sound chip.  It produces this to let you know that it's working
  // and ready to recieve commands, and starts to produce it as soon as power is applied to the device.

  // We allow the default tone to be heard for half a second or so...
  delay(500);

  // Now we send the first batch of commands the BBC does on start up to the sound chip
  SendByteToSoundChip(0x82); // First byte of set Frequency on Tone 3 to decimal 1010
  SendByteToSoundChip(0x3F); // Second byte of set Frequency on Tone 3 to decimal 1010
  SendByteToSoundChip(0xBF); // Silence Tone Channel 2 (Volume is inverted on the beeb so 15 is silence, 0 is Loud)
  SendByteToSoundChip(0xA1); // First byte of set Frequency on Tone 2 to decimal 1009
  SendByteToSoundChip(0x3F); // Second byte of set Frequency on Tone 2 to decimal 1009
  SendByteToSoundChip(0xDF); // Silence Tone Channel 1 (Volume is inverted on the beeb so 15 is silence, 0 is Loud)
  SendByteToSoundChip(0xC0); // First byte of set Frequency on Tone 1 to decimal 1008
  SendByteToSoundChip(0x3F); // Second byte of set Frequency on Tone 1 to decimal 1008
  SendByteToSoundChip(0xFF); // Silence Noise Channel
  SendByteToSoundChip(0xE0); // Set noise channel to periodic noise with a low frequency

  // Now we wait a 50th of a second (I got these timings from the VGM file I recorded from a BBC Emulator)
  delay(20);

  // Next block of commands to the sound chip
  SendByteToSoundChip(0x92); // Set volume on tone channel 3 to 2
  SendByteToSoundChip(0x8F); // First byte of set Frequency on Tone 3 to decimal 239
  SendByteToSoundChip(0x0E); // Second byte of set Frequency on Tone 3 to decimal 239

  // Delay 15 50ths of a second (Approx 1 3rd of a sec)
  delay(300);

  // And finally the last sound chip command
  SendByteToSoundChip(0x9F); // Silence Tone 3
}

void SendByteToSoundChip(byte b)
{
  // First split up the byte to send onto the individual I/O pins of the 
  // chip's 8 bit data bus.
  digitalWrite(PIN_D0, (b&1)?HIGH:LOW);
  digitalWrite(PIN_D1, (b&2)?HIGH:LOW);
  digitalWrite(PIN_D2, (b&4)?HIGH:LOW);
  digitalWrite(PIN_D3, (b&8)?HIGH:LOW);
  digitalWrite(PIN_D4, (b&16)?HIGH:LOW);
  digitalWrite(PIN_D5, (b&32)?HIGH:LOW);
  digitalWrite(PIN_D6, (b&64)?HIGH:LOW);
  digitalWrite(PIN_D7, (b&128)?HIGH:LOW);
  
  // Then pulse the write enable line low for one millisecond.  The R/W line on the SN76489 is inverted
  // that is it commonly spoken as "Read NOT Write" which essentially means that to write the data on the Dx
  // input pins you briefly have to pull the write line low, then return it high for normal read operation.
  digitalWrite(WE, LOW);
  delay(1);
  digitalWrite(WE, HIGH);
  
}

// This is the main program runtime loop fo the Arduino firmware.
void loop() {

  // If there's anything coming down from the PC, just throw it
  // straight to the sound chip.  No ceremony here......
  if(Serial.available() > 0)
  {
    byte b = Serial.read();
    SendByteToSoundChip(b);
  }

}
