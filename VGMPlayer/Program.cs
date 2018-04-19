using System;
using System.Threading;

// VGM Player written by
// !Shawty!/DS in 2017
// NOTE: This player is designed only to playback BBC Model B VGM Files

namespace VGMPlayer
{
  class Program
  {
    static VgmFile vgmFile = new VgmFile();
    static MicroTimer timer;
    static bool displayRunning = false;

    static void DisplayThread()
    {
      Console.Clear();
      while (displayRunning)
      {
        Console.SetCursorPosition(0, 0);
        Console.Write("BBC Micro VGM Player (Serial Port Arduino Version)");

        Console.SetCursorPosition(0, 2);
        Console.Write("Delay {0}     ", vgmFile.DelayCounter);

        Console.SetCursorPosition(0, 3);
        Console.Write("Last Byte Sent: 0x{0:X}     ", vgmFile.LastByteSent);

        Console.SetCursorPosition(0, 4);
        Console.Write("Tone 3 Volume: {0}".PadRight(60, ' '), vgmFile.Tone3Volume);

        Console.SetCursorPosition(0, 5);
        Console.Write("Tone 2 Volume: {0}".PadRight(60, ' '), vgmFile.Tone2Volume);

        Console.SetCursorPosition(0, 6);
        Console.Write("Tone 1 Volume: {0}".PadRight(60, ' '), vgmFile.Tone1Volume);

        Console.SetCursorPosition(0, 7);
        Console.Write(" Noise Volume: {0}".PadRight(60, ' '), vgmFile.NoiseVolume);

        Console.SetCursorPosition(0, 9);
        Console.WriteLine("3 {0}".PadRight(60, ' '), new String('*', vgmFile.Tone3Bar));

        Console.SetCursorPosition(0, 10);
        Console.WriteLine("2 {0}".PadRight(60, ' '), new String('*', vgmFile.Tone2Bar));

        Console.SetCursorPosition(0, 11);
        Console.WriteLine("1 {0}".PadRight(60, ' '), new String('*', vgmFile.Tone1Bar));
      }
      Console.SetCursorPosition(0, 14);
      Console.WriteLine("Song Finished, Please Press Return to Exit");
    }

    static void CallPlayer(object stateInfo, MicroTimerEventArgs e)
    {
      if(vgmFile.SongLooping)
      {
        displayRunning = false;
        timer.Enabled = false;
        timer.Stop();
      }

      vgmFile.PlayNext();
    }

    static void Main(string[] args)
    {
      vgmFile.Load(@"C:\BBC Micro Emulation\Embeded Stuff\dreamscape.vgm"); // CHANGE THIS TO THE FILE YOU WANT TO PLAY

      timer = new MicroTimer();
      timer.MicroTimerElapsed += new MicroTimer.MicroTimerElapsedEventHandler(CallPlayer);
      timer.Interval = 22; // NOTE: This is 22 MICROSECONDS, NOT MILLISECONDS as is usually the case in windows/dotnet
      timer.Enabled = true;

      Thread songInfoThread = new Thread(new ThreadStart(DisplayThread));
      displayRunning = true;
      songInfoThread.Start();

      Console.SetCursorPosition(0, 13);
      Console.WriteLine("Press return to exit.");

      // Wait for return to be pressed.....
      Console.ReadLine();
      displayRunning = false;
      timer.Enabled = false;
      timer.Stop();

    }

  }
}
