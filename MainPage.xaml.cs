using System.Diagnostics;
using MauiApp1;
using System.Text;

namespace YourApp;

public partial class MainPage : ContentPage
{
    private static TaskFactory<string> TF = new TaskFactory<string>();
    private List<Task<string>> taskuri = new List<Task<string>>();
    private int completedTasks = 0;
    private CancellationTokenSource tokenSource;
    private TaskScheduler uiScheduler;

    public MainPage()
    {
        InitializeComponent();
        uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
    }

    private async void OnCriptareClicked(object sender, EventArgs e)
    {
        await ProcessText(true);
    }

    private async void OnDecriptareClicked(object sender, EventArgs e)
    {
        await ProcessText(false);
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        tokenSource?.Cancel();
    }

    private async Task ProcessText(bool isCriptare)
    {
        var stopwatch = Stopwatch.StartNew();
        var text = OriginalMessage.Text ?? string.Empty;
        if (string.IsNullOrEmpty(text)) return;

        tokenSource = new CancellationTokenSource();
        taskuri.Clear();
        completedTasks = 0;

        ProcessingProgress.Progress = 0;
        ProgressText.Text = "Progres: 0%";

        CriptareButton.IsEnabled = false;
        DecriptareButton.IsEnabled = false;
        CancelButton.IsEnabled = true;

        try
        {
            int processorCount = Environment.ProcessorCount;
            int charsPerTask = (text.Length + processorCount - 1) / processorCount;

            for (int i = 0; i < processorCount; i++)
            {
                int startIndex = i * charsPerTask;
                if (startIndex >= text.Length) break;

                int endIndex = Math.Min(startIndex + charsPerTask, text.Length);
                string segmentPentruTask = text.Substring(startIndex, endIndex - startIndex);

                var task = TF.StartNew(() =>
                {
                    Debug.Print($"În thread-ul {Thread.CurrentThread.ManagedThreadId}");

                    string result = string.Empty;
                    for (int j = 0; j < segmentPentruTask.Length; j++)
                    {
                        tokenSource.Token.ThrowIfCancellationRequested();

                        Thread.Sleep(100);

                        if (isCriptare)
                        {
                            result += CriptareDecriptare.Criptare(segmentPentruTask[j].ToString());
                        }
                        else
                        {
                            result += CriptareDecriptare.Decriptare(segmentPentruTask[j].ToString());
                        }

                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            completedTasks++;
                            double progressPercent = (double)completedTasks / text.Length;
                            ProcessingProgress.Progress = progressPercent;
                            ProgressText.Text = $"Progres: {progressPercent:P0}";
                        });
                    }
                    return result;
                }, tokenSource.Token);

                taskuri.Add(task);
            }

            try
            {
                await Task.WhenAll(taskuri);
                string rezultat = string.Concat(taskuri.Select(t => t.Result));
                EncryptedMessage.Text = rezultat;
            }
            catch (AggregateException ae)
            {
                if (ae.InnerExceptions.Any(e => e is OperationCanceledException))
                {
                    throw new OperationCanceledException();
                }
                throw;
            }

            stopwatch.Stop();
            ExecutionTime.Text = $"Timp de executie pentru {(isCriptare ? "criptare" : "decriptare")}: {stopwatch.ElapsedMilliseconds}ms";
        }
        catch (OperationCanceledException)
        {
            EncryptedMessage.Text = "Operația a fost anulată de utilizator";
            ExecutionTime.Text = "Operație anulată";
            ProcessingProgress.Progress = 0;
            ProgressText.Text = "Progres: Anulat";
        }
        catch (Exception ex)
        {
            await DisplayAlert("Eroare", ex.Message, "OK");
        }
        finally
        {
            CriptareButton.IsEnabled = true;
            DecriptareButton.IsEnabled = true;
            CancelButton.IsEnabled = false;
            tokenSource?.Dispose();
        }
    }
}