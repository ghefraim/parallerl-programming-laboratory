class ThreadParams
{
  public string Text { get; set; }
  public Func<object, string> EncryptedText { get; set; }
  public ManualResetEvent Signal { get; set; }
}