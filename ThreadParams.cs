class ThreadParams
{
  public string Text { get; set; }

  public Action<string> EncryptedText { get; set; }

  public string EncryptedTextData { get; set; }

  public Action<string> DecryptedText { get; set; }

  public string DecryptedTextData { get; set; }

  public ManualResetEvent Signal { get; set; }

  public ManualResetEvent FinalSignal { get; set; }

  public CancellationToken Token { get; set; }
}