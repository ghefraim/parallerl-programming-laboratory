using System.Diagnostics;

class CriptareDecriptare
{

  public static void CriptareThreadPool(object? state)
  {
    var parameters = (ThreadParams)state;

    Console.WriteLine($"[ThreadPool] Hello from the encryption thread! Thread ID: {Thread.CurrentThread.ManagedThreadId}");

    try
    {
      string result = CriptareWithCancellation(parameters.Text, parameters.Token);
      parameters.EncryptedText(result);
      parameters.EncryptedTextData = result;
      parameters.Signal.Set();

      Debug.Print($"[ThreadPool - Criptare] Thread ID: {Thread.CurrentThread.ManagedThreadId}, Text criptat: {result}");
    }
    catch (OperationCanceledException)
    {
      Console.WriteLine("Criptarea a fost întreruptă.");
    }
  }

  public static void DecriptareThreadPool(object? state)
  {
    var parameters = (ThreadParams)state;

    parameters.Signal.WaitOne();
    Console.WriteLine($"[ThreadPool] Hello from the decryption thread! Thread ID: {Thread.CurrentThread.ManagedThreadId}");

    string result = Decriptare(parameters.EncryptedTextData);
    parameters.DecryptedText(result);
    parameters.FinalSignal.Set();

    Debug.Print($"[ThreadPool - Decriptare] Thread ID: {Thread.CurrentThread.ManagedThreadId}, Text decriptat: {result}");
  }

  public static string CriptareWithCancellation(string text, CancellationToken token)
  {
    string encryptedText = string.Empty;
    char randomChar;
    Random rand = new Random();

    for (int i = 0; i <= (text.Length - 1); i++)
    {
      if (token.IsCancellationRequested)
      {
        throw new OperationCanceledException(token);
      }
      Thread.Sleep(200);

      randomChar = (char)(rand.Next(128));
      encryptedText += ((char)(text[i] ^ randomChar)).ToString();
      encryptedText += ((char)(randomChar ^ (128 - i))).ToString();
    }

    return encryptedText;
  }

  public static string Criptare(string text)
  {
    string encryptedText = string.Empty;
    char randomChar;
    Random rand = new Random();

    for (int i = 0; i <= (text.Length - 1); i++)
    {
      randomChar = (char)(rand.Next(128));
      encryptedText += ((char)(text[i] ^ randomChar)).ToString();
      encryptedText += ((char)(randomChar ^ (128 - i))).ToString();
    }

    return encryptedText;
  }

  public static string Decriptare(string encryptedText)
  {
    string decryptedText = string.Empty;
    char originalChar;
    char decryptedRandomChar;

    for (int i = 0; i < encryptedText.Length - 1; i += 2)
    {
      decryptedRandomChar = (char)((encryptedText[i + 1]) ^ (128 - (i / 2)));
      originalChar = (char)(encryptedText[i] ^ decryptedRandomChar);
      decryptedText += originalChar;
    }

    return decryptedText;
  }
}
