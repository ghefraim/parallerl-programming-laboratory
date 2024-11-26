using System.Diagnostics;

namespace MauiApp1;

public static class CriptareDecriptare
{
    public static async Task<string> Criptare(string text, CancellationToken ct)
    {
        Debug.Print($"Thread ID pentru criptare: {Thread.CurrentThread.ManagedThreadId}");

        return await Task.Run(() =>
        {
            ct.ThrowIfCancellationRequested();

            // Simulăm o operație de durată
            Thread.Sleep(100); // Adăugăm o întârziere de 100ms

            string encryptedText = string.Empty;
            char randomChar;
            Random rand = new Random();

            for (int i = 0; i <= (text.Length - 1); i++)
            {
                ct.ThrowIfCancellationRequested();
                Thread.Sleep(10);
                randomChar = (char)(rand.Next(128));
                encryptedText += ((char)(text[i] ^ randomChar)).ToString();
                encryptedText += ((char)(randomChar ^ (128 - i))).ToString();
            }

            return encryptedText;
        }, ct);
    }

    public static async Task<string> Decriptare(string encryptedText, CancellationToken ct)
    {
        Debug.Print($"Thread ID pentru decriptare: {Thread.CurrentThread.ManagedThreadId}");

        return await Task.Run(() =>
        {
            ct.ThrowIfCancellationRequested();


            // Simulăm o operație de durată
            Thread.Sleep(100); // Adăugăm o întârziere de 100ms

            string decryptedText = string.Empty;
            char originalChar;
            char decryptedRandomChar;

            for (int i = 0; i < encryptedText.Length - 1; i += 2)
            {
                ct.ThrowIfCancellationRequested();
                Thread.Sleep(10);
                decryptedRandomChar = (char)((encryptedText[i + 1]) ^ (128 - (i / 2)));
                originalChar = (char)(encryptedText[i] ^ decryptedRandomChar);
                decryptedText += originalChar;
            }

            return decryptedText;
        }, ct);
    }
}