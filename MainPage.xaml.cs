using System.Diagnostics;
using MauiApp1;
using System.Text;

namespace YourApp;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private void OnExecutaClicked(object sender, EventArgs e)
    {
        var stopwatch = Stopwatch.StartNew();

        string originalText = OriginalMessage.Text ?? string.Empty;
        string encryptedText = CriptareDecriptare.Criptare(originalText);

        stopwatch.Stop();

        EncryptedMessage.Text = encryptedText;
        ExecutionTime.Text = $"Timp de executie: {stopwatch.ElapsedMilliseconds}ms";
    }

    private async void OnExecutaThreadClicked(object sender, EventArgs e)
    {
        var stopwatch = Stopwatch.StartNew();
        string originalText = OriginalMessage.Text ?? string.Empty; 
        
        int numberOfCores = Environment.ProcessorCount;

        string encryptedText = await Task.Run(() =>
        {

            if (string.IsNullOrEmpty(originalText))
                return string.Empty;

            int segmentSize = (originalText.Length + numberOfCores - 1) / numberOfCores;
            var threads = new List<Thread>();
            var results = new string[numberOfCores];

            for (int i = 0; i < numberOfCores; i++)
            {
                int threadIndex = i;
                int startIndex = i * segmentSize;
                int endIndex = Math.Min(startIndex + segmentSize, originalText.Length);

                if (startIndex >= originalText.Length)
                    break;

                string segment = originalText.Substring(startIndex, endIndex - startIndex);

                var thread = new Thread(() =>
                {
                    results[threadIndex] = CriptareDecriptare.Criptare(segment);
                });

                threads.Add(thread);
                thread.Start();
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }

            return string.Concat(results.Where(r => r != null));
        });

        stopwatch.Stop();

        EncryptedMessage.Text = encryptedText;
        ExecutionTime.Text = $"Timp de executie: {stopwatch.ElapsedMilliseconds}ms pe {numberOfCores} thread-uri ";
    }
}