namespace VGMPlayer
{
  public class VgmFileHeader
  {
    public string VgmMagic { get; set; }
    public uint EofOffset { get; set; }
    public uint Version { get; set; }
    public uint Sn76489Clock { get; set; }
    public uint Ym2413Clock { get; set; }
    public uint Gd3Offset { get; set; }
    public uint TotalSamples { get; set; }
    public uint LoopOffset { get; set; }
    public uint LoopSamples { get; set; }
    public uint Rate { get; set; }
    public ushort SnFb { get; set; }
    public byte Snw { get; set; }
    public byte Reserved { get; set; }
    public uint Ym2612Clock { get; set; }
    public uint Ym2151Clock { get; set; }
    public uint VgmDataOffset { get; set; }

  }
}
