# sn76489arduino
Project and Supporting files to turn an arduino into a BBC Micro music player.

In this repository, you will find everything you need to create a BBC Micro Music player, using an Arduino, and a PC.

I'll provide

* Circuit Diagrams (To connect a Texas Instruments SN76489 *[The BBC Micro's hardware sound chip]* to your Arduino)
* The code/Arduino IDE project, to run in the Arduino IDE and program into your arduino
* A VGM Streamer application written in C# using dotnet core, to stream the music data from a PC to the Arduino

## A Few Notes

The project was built on and designed to work with an arduino mega.  It may take a little work to adapt it to work on other arduinos.

The main thing you will have to do to use it on different devices will be to remap the pin numbers you use for the connections to the sound chip.  Iv'e followed good arduino practice however, and provided easy to read pin number constants/defines at the top of the file, so it should be trivial with anyone who has a smattering of arduino experience to change it.

MOST diagrams for the SN76489 sound chip that are available online, are taken from the original Texas Instruments data sheet.  This data sheet **IS WRONG** in the original data sheet, data pin d0 was labeled as being on pin 3 of the chip, when in fact it is actually on pin 10.

Once you've set the pin numbers up, build the circuit, then use the Arduino IDE, load the arduino project, and upload it to your chosen arduino device.

The c# program has only been tested on a PC running under windows, however since it's dotnet core, then it should be possible to run it on ANY MACHINE that you have dot net core installed on.  Dot Net core currently runs on Windows, Linux AND MacOS, instructions to install dotnet core are outside the scope of this project, the place to look for instructions for your platform is as follows:

https://www.microsoft.com/net/learn/get-started/windows

Once you have dotnet core installed and running correctly, use the command line of your system, change to the folder called "VGMPlayer" in the place you cloned this project, and type

**dotnet run**

Running the player is simple, but you'll need to change a few things first, the name of the file to play is in the file 'Program.cs' on line 69, you'll need to change this line to the file you want to play, Iv'e deliberately not made any effort to make this windows player user aware, as it's really only a proof of concept, if you know C# and you want to go changing it to ask for the file name or anything like that, then feel free to do so.

In the file "SerialSender.cs", on line 10 is the name of the serial port your arduino is connected too, you will need to change this to whatever port your OS tells you your arduino is using in the arduino IDE tools menu, it will be something like "COMxx" for windows and "/dev/ttyxxxx" or "/dev/....." on linux.  DOn't ask me about MacOS I don't own one, and I ain't got a clue :-)

To hear the sound output, you **WILL NEED** an amplifed speaker.  Personally (As you can see in the YouTube video here: https://www.youtube.com/watch?v=rOjFRLOkblk ) I use a small USB Powered Xmi mobile phone speaker, but connecting a jack plug to the line in on your PC works just as well, and gives you the added bonus that you can record it too.

What ever you do, you cannot connect headphones directly to the output as it's too weak to drive them correctly so you'll not be able to hear what's being played.

Once these 2 changes are correct, and you dotnet run the app, then assuming your wiring is correct, and you got the arduino programmed correctly, and you have your sound output hooked up, you should hear the dulcet tones of a genuine 1980's era BBC Sound chip playing back actual song data from an actual 80's era BBC Micro.

### How I created the VGM files......
Quite easily as it happens.

Make sure you have Tom Sneddons "model-b" BBC Micro Emulator installed ( http://archive.retro-kit.co.uk/bbc.nvg.org/emulators.php3.html#Model-B ) it has to be this specific emulator as this is the only one I can find that records VGA, rather than recording the wave audio output which is what all the others do.

Once you have the emulator running, click "special" on the menu bar, then "Start Recording Sound", run the program who's music you want to record and let it play util you record everything you want to record.

Once done, click "special" again and "Stop recording sound", wait a few seconds and the emulator will ask you where you want to save the VGM file.

Iv'e provided a bunch of VGM test files that I recorded from the emulator in the "Example VGM Tunes" folder, please be aware though that some of them are well, awfull :-)  SImply beacuse some of them used some rather bizare tricks to try and get things like argeggios and complex harmonies going (Every trick in Ian Waughs excelent - "Making music on the BBC Micro Computer" book basically) and the emulator bless it's little socks did try to record things as faithfully as it could, but just didn't manage to get a good quality grab for some of them.  I'm sure with patience and time I'll be able to make some better ones.

You'll also find the soundtrack to my multipart BBC Model B demo "DreamScape" ( https://www.youtube.com/watch?v=_mrOAB4UBcM ) in there too, and that plays rather quite well :-)

**Why a VGM file**

When you record a VGM file, you physically record the instructions sent to the emulated BBC sound chip, these are the exact same instructions that the real SN76489 also understands, and obeys to make sounds.  If you download a copy of the BBC Advanced User GUide, it has an entire chapter on writing these instructions on an actual BBC Micro direct to the real chip in a BBC Micro (One of the first ever 6502 Machine Code Programs I wrote on a BBC all those years ago was a direct hardware music player), the VGM file is a standard emulator format used by a number of different emulators for recording this chip level music data containing chip commands.

The PC Player is in actual fact a mini VGM format file player, but it only looks at and uses the parts that are specific to the BBC Micro or SN76489 chip.  The SN chip was added to the spec as it was used in a number of consoles in the 1990's, the most famous of wich was the Sega Mega Drive, so it suits our purposes perfectly.

