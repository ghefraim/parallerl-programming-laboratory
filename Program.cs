
using System.Diagnostics;

internal class Program
{
  private static void Main(string[] args)
  {
    string text = string.Empty;
    string encryptedText = string.Empty;
    string decryptedText = string.Empty;

    Console.WriteLine("Introduceti textul de criptat:");
    text = Console.ReadLine();

    ManualResetEvent encryptionDone = new ManualResetEvent(false);

    var threadParams = new ThreadParams
    {
      Text = text,
      EncryptedText = (result) => encryptedText = result,
      EncryptedTextData = encryptedText,
      DecryptedText = (result) => decryptedText = result,
      DecryptedTextData = decryptedText,
      Signal = encryptionDone
    };

    CancellationTokenSource ctokens = new CancellationTokenSource();
    threadParams.Token = ctokens.Token;

    ThreadPool.QueueUserWorkItem(CriptareDecriptare.CriptareThreadPool, threadParams);

    ManualResetEvent finalEvent = new ManualResetEvent(false);
    threadParams.FinalSignal = finalEvent;
    ThreadPool.QueueUserWorkItem(CriptareDecriptare.DecriptareThreadPool, threadParams);

    Console.WriteLine("Apasati o tasta pentru a întrerupe criptarea...");
    Console.ReadKey();
    ctokens.Cancel();

    finalEvent.WaitOne();
    Console.WriteLine($"\nEncrypted text: {encryptedText}");
    Console.WriteLine($"Decrypted text: {decryptedText}");
  }
}