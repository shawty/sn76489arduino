using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

// VGM Player written by
// !Shawty!/DS in 2017
// NOTE: This player is designed only to playback BBC Model B VGM Files

namespace VGMPlayer
{
  public class VgmFile
  {
    // This class is based on the 1.50 VGM File format

    private SerialSender _serialSender = new SerialSender();

    private VgmFileHeader _header = new VgmFileHeader();
    private byte[] _chipData;

    // The player process is quite simple.
    // All we care about are VGM Commands 0x61, 0x63, 0x50 & 0x66
    // 61 means set a delay, if we see one of these we set the delay counter to that value, then each time the player is called
    // we decrement that delay until it reaches 0, we do not allow the vgm data reader to progress to the next byte if this
    // delay counter is not 0.  Command 63 is a shortcut for a delay of 1/50th of a second
    // Command 50, means the next byte is to be sent directly to the sound chip, so thats exactly what we do :-)
    // Command 66, means end of song data, so we set the variable to show that it's looped, then reset the song data pointer to 0

    private int _dataPointer = 0; // Current position in our chip data array
    private int _delayCounter = 0; // Counts down to 0, and is set by a delay command in the chip data
    //private int _delayCounterTimer = 0; // Used to control how fast the delay counter counts down
    //private int _delayCounterTimerMax = 500; // Used to set the speed on the delay counter timer. Higher is slower
    // NOTE: You WILL need to fiddle with the above max value to get the correct playback speed, remember the PC you
    // run this in is likley to be orders of magnitude faster than the 16Mhz arduino recieving the data to send to
    // the sound chip, which itself is only clocked at 4Mhz.
    // The serial connection in the Arduino code is set at 115200, and the Arduino's inbound buffer is only 128 bytes
    // so timing will need to be adjusted!!!

    private byte barMax = 32; // Maximum size a bar value can be
    private byte barSpeed = 0; // Counter to track bar speed
    private byte barSpeedMax = 192; // How many itterations of the play loop before we reduce a bar value;

    // These 3 vars are public (That is they can be accessed outside the player class) and are read only.
    public bool SongLooping { get; private set; }
    public int DelayCounter { get { return _delayCounter; } }
    public byte LastByteSent { get; private set; }

    public byte Tone3Volume { get; private set; }
    public byte Tone2Volume { get; private set; }
    public byte Tone1Volume { get; private set; }
    public byte NoiseVolume { get; private set; }

    public byte Tone3Bar { get; private set; }
    public byte Tone2Bar { get; private set; }
    public byte Tone1Bar { get; private set; }

    public void Load(string fileName)
    {
      Stream fileStream = new FileStream(fileName, FileMode.Open);
      BinaryReader reader = new BinaryReader(fileStream);

      LoadHeader(reader);
      LoadChipData(reader);

      fileStream.Close();

      SongLooping = false;
    }

    public void PlayNext()
    {
      // Plays the next available entry in the chip data array, this is designed to be called repeatedly
      // from a loop outside the class, so each call of it will decode one command only.

      // Adjust our tone counters
      if (barSpeed == 0)
      {
        Tone3Bar--; if (Tone3Bar < 1) { Tone3Bar = 1; }; if (Tone3Bar > barMax) { Tone3Bar = barMax; }
        Tone2Bar--; if (Tone2Bar < 1) { Tone2Bar = 1; }; if (Tone2Bar > barMax) { Tone2Bar = barMax; }
        Tone1Bar--; if (Tone1Bar < 1) { Tone1Bar = 1; }; if (Tone1Bar > barMax) { Tone1Bar = barMax; }
        barSpeed = barSpeedMax;
      }
      else
      {
        barSpeed--;
      }

      // If we have a delay set
      if (_delayCounter > 0)
      {
        // THIS WAS THE OLD WAY OF DOING THE TIMING UNTIL I MANAGED TO USE A HIGH RES TIMER
        // IVE NOT TRIED THIS ON ANYTHING OTHER THAN WINDOWS 7 YET, SO MAY HAVE TO Go
        // BACK TO USING THIS ON OTHER PLATFORMS
        // Increment our delay timer 
        //_delayCounterTimer++;
        // And if it's hit our max.....
        //if(_delayCounterTimer > _delayCounterTimerMax)
        //{
          // Decrement the actual delay and reset the timer
          _delayCounter--;
          //_delayCounterTimer = 0;
        //}
        return;
      }

      // if our delayCounter is 0 then we get here, ready to decode the next command

      byte currentDataByte = _chipData[_dataPointer];

      // Now we decode our VGM chip data commands.
      // In the case of the SN76489 in the BBC not all the commands are used, so we decode
      // only the one we are interested in.

      switch (currentDataByte)
      {
        case 0x61:
          // Set a delay counter value, low byte is in the next byte, high in the one following that
          int delayVal = (_chipData[_dataPointer + 2] << 8) + _chipData[_dataPointer + 1];
          _delayCounter = delayVal;
          _dataPointer = _dataPointer + 3; // Increase pointer to next data entry

          //Console.WriteLine("Set Delay {0}", _delayCounter);
          break;

        case 0x63:
          // Short cut for a 1/50th of a second delay
          _delayCounter = 0x7203; // See VGM spec for these values (These are 20ms on a PAL System running at 4Mhz)
          _dataPointer++; // Increase pointer to next data entry;

          //Console.WriteLine("Set 1/50th Second Delay");
          break;

        case 0x50:
          // This is an actual raw byte to send to the sound chip
          byte chipByte = _chipData[_dataPointer + 1];
          _dataPointer = _dataPointer + 2; // Increase pointer to the next entry

          _serialSender.Send(chipByte);

          // Extract some usefull info from the command
          // Where not going to do frequencies or anything like that, beacuse they are 2 byte
          // we risk slowing down the high presicion send loop too much.

          // Volumes however are simple.
          if ((chipByte & 0x90) == 0x90)
          {
            Tone3Volume = (byte)(chipByte & 0x0F);
          }

          if ((chipByte & 0xB0) == 0xB0)
          {
            Tone2Volume = (byte)(chipByte & 0x0F);
          }

          if ((chipByte & 0xD0) == 0xD0)
          {
            Tone1Volume = (byte)(chipByte & 0x0F);
          }

          if ((chipByte & 0xF0) == 0xF0)
          {
            NoiseVolume = (byte)(chipByte & 0x0F);
          }

          // and we can at least do some simple counters, that we set to max
          // when a frequency on a channel is triggered, and count down
          // so while not frequency related, we can at least do a simple
          // kind of VU :-)

          if ((chipByte & 0x80) == 0x80)
          {
            Tone3Bar = barMax;
          }

          if ((chipByte & 0xA0) == 0xA0)
          {
            Tone2Bar = barMax;
          }

          if ((chipByte & 0xC0) == 0xC0)
          {
            Tone1Bar = barMax;
          }

          //Console.WriteLine("Send Chip Byte {0}", chipByte);
          LastByteSent = chipByte;
          break;

        case 0x66:
          // End of current song data
          _dataPointer = 0; // Reset and loop our song
          SongLooping = true; // Set this so that calling program knows if song is looping

          //Console.WriteLine("Reset Data Pointer");
          break;

        default:
          // This shouldn't be needed, well at least the data pointer check shouldn't be needed anyway
          // beacuse in theory we should always hit a 0x66 at the end.  However if the 66 is not found, then
          // this stops a program crash due to trying to access data outside the array.
          // The increment is still needed however, as that allows us to skip over VGM commands that we don't yet implement.
          _dataPointer++;
          if(_dataPointer > (_chipData.Length - 1))
          {
            _dataPointer = 0;
          }

          //Console.WriteLine("Unknown VGM Command {0}", currentDataByte);
          break;
      }
    }

    private void LoadHeader(BinaryReader reader)
    {
      byte[] magicBytes = reader.ReadBytes(4);
      string magic = Encoding.Default.GetString(magicBytes).Trim();

      if(magic != "Vgm")
      {
        throw new ApplicationException("Specifed file is NOT a VGM file");
      }

      _header.VgmMagic = magic;

      _header.EofOffset = reader.ReadUInt32() + 4; // Eof val is 4 bytes in, so the actual offset is this + 4
      _header.Version = reader.ReadUInt32();
      _header.Sn76489Clock = reader.ReadUInt32();
      _header.Ym2413Clock = reader.ReadUInt32();
      _header.Gd3Offset = reader.ReadUInt32();
      _header.TotalSamples = reader.ReadUInt32();
      _header.LoopOffset = reader.ReadUInt32();
      _header.LoopOffset = reader.ReadUInt32();
      _header.Rate = reader.ReadUInt32();
      _header.SnFb = reader.ReadUInt16();
      _header.Snw = reader.ReadByte();
      _header.Reserved = reader.ReadByte();
      _header.Ym2612Clock = reader.ReadUInt32();
      _header.Ym2151Clock = reader.ReadUInt32();

      // From this point in the file, the following offset + the current file pointer is the ACTUAL location of the chip data
      // so we need to work out and record what that location actually is.
      // In most cases it will be 0x40 for v1.50 and prior, but it CAN be different, esp in the V1.50 VGM spec
      // yes this is a messy way of doing it, and if the data position is greater than 32 bits then where screwed
      // however, 32 bits tends to suggest a 4gb file, and Iv'e not seen one break the 10mb barrier yet, so I think where
      // fairly safe. :-)
      long currentFilePointer = reader.BaseStream.Position;
      _header.VgmDataOffset = reader.ReadUInt32();
      if (_header.VgmDataOffset == 0)
      {
        // No offset was specified, so we default to 0x40 which is the known start offset.
        _header.VgmDataOffset = 0x40;
      }
      else
      {
        // There is a positive offset, so lets take the file pos, add that on and store that.
        _header.VgmDataOffset = (uint)((int)currentFilePointer + _header.VgmDataOffset);
      }

      // We don't use the last 2 reserved words in the header
      var reserved = reader.ReadUInt32();
      reserved = reader.ReadUInt32();

    }

    private void LoadChipData(BinaryReader reader)
    {
      List<byte> result = new List<byte>();

      // Move the file stream to the start of our data
      reader.BaseStream.Seek(_header.VgmDataOffset, SeekOrigin.Begin);

      var dataSize = _header.EofOffset - _header.VgmDataOffset;

      result.AddRange(reader.ReadBytes((int)dataSize));
      _chipData = result.ToArray();
    }

  }
}
