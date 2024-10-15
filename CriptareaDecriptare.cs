using System.Diagnostics;

class CriptareDecriptare
{
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
