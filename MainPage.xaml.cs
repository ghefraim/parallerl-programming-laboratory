using System.Diagnostics;
using MauiApp1;
using System.Text;

namespace YourApp;

public partial class MainPage : ContentPage
{
    private static TaskFactory<string> TF = new TaskFactory<string>();
    private List<Task<string>> taskuri = new List<Task<string>>();
    private int completedTasks = 0;

    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnCriptareClicked(object sender, EventArgs e)
    {
        await ProcessText(true);
    }

    private async void OnDecriptareClicked(object sender, EventArgs e)
    {
        await ProcessText(false);
    }

    private async Task ProcessText(bool isCriptare)
    {
        var stopwatch = Stopwatch.StartNew();
        var text = OriginalMessage.Text ?? string.Empty;
        if (string.IsNullOrEmpty(text)) return;

        taskuri.Clear();
        completedTasks = 0;

        ProcessingProgress.Progress = 0;
        ProgressText.Text = "Progres: 0%";

        CriptareButton.IsEnabled = false;
        DecriptareButton.IsEnabled = false;

        try
        {
            int processorCount = Environment.ProcessorCount;
            int charsPerTask = (text.Length + processorCount - 1) / processorCount; // Rotunjire în sus

            // Creăm câte un task pentru fiecare procesor
            for (int i = 0; i < processorCount; i++)
            {
                int startIndex = i * charsPerTask;
                if (startIndex >= text.Length) break;

                int endIndex = Math.Min(startIndex + charsPerTask, text.Length);
                string segmentPentruTask = text.Substring(startIndex, endIndex - startIndex);

                var task = TF.StartNew(() =>
                {
                    Debug.Print($"În thread-ul {Thread.CurrentThread.ManagedThreadId}");
                    Thread.Sleep(1000);

                    string result;
                    if (isCriptare)
                    {
                        string encrypted = CriptareDecriptare.Criptare(segmentPentruTask);
                        result = Convert.ToBase64String(Encoding.UTF8.GetBytes(encrypted));
                    }
                    else
                    {
                        try
                        {
                            byte[] decodedBytes = Convert.FromBase64String(segmentPentruTask);
                            string decodedText = Encoding.UTF8.GetString(decodedBytes);
                            result = CriptareDecriptare.Decriptare(decodedText);
                        }
                        catch (FormatException)
                        {
                            result = "Eroare: Text invalid pentru decriptare";
                        }
                    }

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        completedTasks += segmentPentruTask.Length;
                        double progressPercent = (double)completedTasks / text.Length;
                        ProcessingProgress.Progress = progressPercent;
                        ProgressText.Text = $"Progres: {progressPercent:P0}";
                    });

                    return result;
                });
                taskuri.Add(task);
            }

            await Task.WhenAll(taskuri);
            string rezultat = string.Concat(taskuri.Select(t => t.Result));
            EncryptedMessage.Text = rezultat;

            stopwatch.Stop();
            ExecutionTime.Text = $"Timp de executie pentru {(isCriptare ? "criptare" : "decriptare")}: {stopwatch.ElapsedMilliseconds}ms pe {taskuri.Count} thread-uri";
        }
        catch (Exception ex)
        {
            await DisplayAlert("Eroare", ex.Message, "OK");
        }
        finally
        {
            CriptareButton.IsEnabled = true;
            DecriptareButton.IsEnabled = true;
        }
    }
}