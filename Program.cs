
using System.Diagnostics;

internal class Program
{
  public static void CriptareThread(object? param)
  {
    var parameters = (ThreadParams)param;
    Console.WriteLine("Aceasta este thread-ul de criptare!");

    string result = CriptareDecriptare.Criptare(parameters.Text);

    parameters.EncryptedText(result);
    parameters.Signal.Set();

    Debug.Print($"[Encryption Thread] Text criptat: {result}");
  }

  private static void Main(string[] args)
  {
    string text = string.Empty;
    string encryptedText = string.Empty;
    string decryptedText = string.Empty;

    Console.WriteLine("Introduceti textul de criptat:");
    text = Console.ReadLine();

    ManualResetEvent encryptionDone = new ManualResetEvent(false);

    Thread encryptionThread = new Thread(new ParameterizedThreadStart(CriptareThread));
    encryptionThread.Name = "EncryptionThread";
    encryptionThread.Priority = ThreadPriority.Normal;

    Debug.Print($"[Encryption Thread] Nume: {encryptionThread.Name}, Prioritate: {encryptionThread.Priority}, Stare: {encryptionThread.ThreadState}");

    Thread decryptionThread = new Thread(() =>
    {
      encryptionDone.WaitOne();

      Console.WriteLine("Acesta este thread-ul de decriptare!");

      string result = CriptareDecriptare.Decriptare(encryptedText);

      Debug.Print($"[Decryption Thread] Text decriptat: {result}");
    });

    decryptionThread.Name = "DecryptionThread";
    decryptionThread.Priority = ThreadPriority.Normal;

    Debug.Print($"[Decryption Thread] Nume: {decryptionThread.Name}, Prioritate: {decryptionThread.Priority}, Stare: {decryptionThread.ThreadState}, IsBackground = {decryptionThread.IsBackground}");

    decryptionThread.Start();
    encryptionThread.Start(new ThreadParams { Text = text, EncryptedText = (result) => encryptedText = (string)result, Signal = encryptionDone });
  }
}