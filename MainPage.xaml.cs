using System.Diagnostics;
using MauiApp1;
using System.Text;
using System.Collections.Concurrent;

namespace YourApp;

public class ProcessingResult
{
    public string ProcessedText { get; set; }
    public bool WasCanceled { get; set; }
    public int ThreadIndex { get; set; }
    public int ProcessedLength { get; set; }
    public int TotalLength { get; set; }

    public override string ToString()
    {
        if (WasCanceled)
        {
            return $"{ProcessedText}[Thread {ThreadIndex} canceled at position {ProcessedLength}/{TotalLength}]";
        }
        return ProcessedText;
    }
}

public partial class MainPage : ContentPage
{
    private static TaskFactory<ProcessingResult> TF = new TaskFactory<ProcessingResult>();
    private List<Task<ProcessingResult>> taskuri = new List<Task<ProcessingResult>>();
    private int completedTasks = 0;
    private CancellationTokenSource tokenSource;
    private TaskScheduler uiScheduler;
    private ConcurrentQueue<string> concurrentResults = new ConcurrentQueue<string>();
    private List<string> regularResults = new List<string>();

    public MainPage()
    {
        InitializeComponent();
        uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
    }

    private async void OnCriptareClicked(object sender, EventArgs e)
    {
        Debug.Print($"Thread ID principal: {Thread.CurrentThread.ManagedThreadId}");

        var text = OriginalMessage.Text ?? string.Empty;
        if (string.IsNullOrEmpty(text)) return;

        tokenSource = new CancellationTokenSource();
        var stopwatch = Stopwatch.StartNew();

        try
        {
            CriptareButton.IsEnabled = false;
            DecriptareButton.IsEnabled = false;
            CancelButton.IsEnabled = true;

            ProcessingProgress.Progress = 0;
            ProgressText.Text = "Progres: 0%";

            var lines = text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            var processedLines = new List<string>();
            var totalLines = lines.Length;
            var processedCount = 0;

            foreach (var line in lines)
            {
                if (string.IsNullOrEmpty(line)) continue;

                try
                {
                    var processedLine = await CriptareDecriptare.Criptare(line, tokenSource.Token);
                    processedLines.Add(processedLine);

                    processedCount++;
                    var progress = (double)processedCount / totalLines;
                    ProcessingProgress.Progress = progress;
                    ProgressText.Text = $"Progres: {progress:P0}";
                }
                catch (OperationCanceledException)
                {
                    processedLines.Add("[Operație anulată]");
                    throw;
                }
            }

            EncryptedMessage.Text = string.Join(Environment.NewLine, processedLines);
        }
        catch (OperationCanceledException)
        {
            EncryptedMessage.Text = "Operația a fost anulată de utilizator";
        }
        catch (Exception ex)
        {
            await DisplayAlert("Eroare", ex.Message, "OK");
        }
        finally
        {
            stopwatch.Stop();
            ExecutionTime.Text = $"Timp de execuție: {stopwatch.ElapsedMilliseconds}ms";

            CriptareButton.IsEnabled = true;
            DecriptareButton.IsEnabled = true;
            CancelButton.IsEnabled = false;
            tokenSource?.Dispose();
        }
    }

    private async void OnDecriptareClicked(object sender, EventArgs e)
    {
        Debug.Print($"Thread ID principal: {Thread.CurrentThread.ManagedThreadId}");

        var text = EncryptedMessage.Text ?? string.Empty;
        if (string.IsNullOrEmpty(text)) return;

        tokenSource = new CancellationTokenSource();
        var stopwatch = Stopwatch.StartNew();

        try
        {
            CriptareButton.IsEnabled = false;
            DecriptareButton.IsEnabled = false;
            CancelButton.IsEnabled = true;

            ProcessingProgress.Progress = 0;
            ProgressText.Text = "Progres: 0%";

            var lines = text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            var processedLines = new List<string>();
            var totalLines = lines.Length;
            var processedCount = 0;

            foreach (var line in lines)
            {
                if (string.IsNullOrEmpty(line)) continue;

                try
                {
                    var processedLine = await CriptareDecriptare.Decriptare(line, tokenSource.Token);
                    processedLines.Add(processedLine);

                    processedCount++;
                    var progress = (double)processedCount / totalLines;
                    ProcessingProgress.Progress = progress;
                    ProgressText.Text = $"Progres: {progress:P0}";
                }
                catch (OperationCanceledException)
                {
                    processedLines.Add("[Operație anulată]");
                    throw;
                }
            }

            OriginalMessage.Text = string.Join(Environment.NewLine, processedLines);
        }
        catch (OperationCanceledException)
        {
            OriginalMessage.Text = "Operația a fost anulată de utilizator";
        }
        catch (Exception ex)
        {
            await DisplayAlert("Eroare", ex.Message, "OK");
        }
        finally
        {
            stopwatch.Stop();
            ExecutionTime.Text = $"Timp de execuție: {stopwatch.ElapsedMilliseconds}ms";

            CriptareButton.IsEnabled = true;
            DecriptareButton.IsEnabled = true;
            CancelButton.IsEnabled = false;
            tokenSource?.Dispose();
        }
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
                int threadIndex = i;

                var task = Task.Factory.StartNew<Task<ProcessingResult>>(async () =>
                {
                    Debug.Print($"În thread-ul {Thread.CurrentThread.ManagedThreadId}");

                    StringBuilder result = new StringBuilder();
                    var processingResult = new ProcessingResult
                    {
                        ThreadIndex = threadIndex,
                        TotalLength = segmentPentruTask.Length
                    };

                    try
                    {
                        for (int j = 0; j < segmentPentruTask.Length; j++)
                        {
                            tokenSource.Token.ThrowIfCancellationRequested();

                            Thread.Sleep(100);

                            if (isCriptare)
                            {
                                var encryptedChar = await CriptareDecriptare.Criptare(segmentPentruTask[j].ToString(), tokenSource.Token);
                                result.Append(encryptedChar);
                            }
                            else
                            {
                                var decryptedChar = await CriptareDecriptare.Decriptare(segmentPentruTask[j].ToString(), tokenSource.Token);
                                result.Append(decryptedChar);
                            }

                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                completedTasks++;
                                double progressPercent = (double)completedTasks / text.Length;
                                ProcessingProgress.Progress = progressPercent;
                                ProgressText.Text = $"Progres: {progressPercent:P0}";
                            });
                        }

                        processingResult.ProcessedText = result.ToString();
                        processingResult.ProcessedLength = result.Length;
                        return processingResult;
                    }
                    catch (OperationCanceledException)
                    {
                        processingResult.ProcessedText = result.ToString();
                        processingResult.ProcessedLength = result.Length;
                        processingResult.WasCanceled = true;
                        return processingResult;
                    }
                }, tokenSource.Token).Unwrap();

                taskuri.Add(task);
            }

            try
            {
                var results = await Task.WhenAll(taskuri);

                StringBuilder finalResult = new StringBuilder();
                int canceledThreads = 0;

                foreach (var result in results)
                {
                    finalResult.AppendLine(result.ToString());
                    if (result.WasCanceled)
                    {
                        canceledThreads++;
                    }
                }

                if (canceledThreads > 0)
                {
                    finalResult.AppendLine($"\n{canceledThreads} thread-uri au fost anulate!");
                }

                EncryptedMessage.Text = finalResult.ToString();
            }
            catch (AggregateException ae)
            {
                if (!ae.InnerExceptions.All(e => e is OperationCanceledException))
                {
                    throw;
                }
            }

            stopwatch.Stop();
            ExecutionTime.Text = $"Timp de executie pentru {(isCriptare ? "criptare" : "decriptare")}: {stopwatch.ElapsedMilliseconds}ms";
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

    private async Task ProcessTextParallel(bool isCriptare)
    {
        var stopwatch = Stopwatch.StartNew();
        var text = OriginalMessage.Text ?? string.Empty;
        if (string.IsNullOrEmpty(text)) return;

        tokenSource = new CancellationTokenSource();
        concurrentResults.Clear();
        completedTasks = 0;

        ProcessingProgress.Progress = 0;
        ProgressText.Text = "Progres: 0%";

        CriptareParallelButton.IsEnabled = false;
        DecriptareParallelButton.IsEnabled = false;
        CancelButton.IsEnabled = true;

        try
        {
            int processorCount = Environment.ProcessorCount;
            int charsPerTask = (text.Length + processorCount - 1) / processorCount;
            var totalChars = text.Length;

            var tasks = new List<Task>();

            for (int i = 0; i < processorCount; i++)
            {
                int startIndex = i * charsPerTask;
                if (startIndex >= text.Length) break;

                int endIndex = Math.Min(startIndex + charsPerTask, text.Length);
                string segment = text.Substring(startIndex, endIndex - startIndex);
                int threadIndex = i;

                var task = Task.Run(async () =>
                {
                    Debug.Print($"În thread-ul {Thread.CurrentThread.ManagedThreadId}");
                    StringBuilder processedSegment = new StringBuilder();

                    for (int j = 0; j < segment.Length; j++)
                    {
                        tokenSource.Token.ThrowIfCancellationRequested();

                        Thread.Sleep(100);

                        if (isCriptare)
                        {
                            var encryptedChar = await CriptareDecriptare.Criptare(segment[j].ToString(), tokenSource.Token);
                            processedSegment.Append(encryptedChar);
                        }
                        else
                        {
                            var decryptedChar = await CriptareDecriptare.Decriptare(segment[j].ToString(), tokenSource.Token);
                            processedSegment.Append(decryptedChar);
                        }

                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            completedTasks++;
                            double progressPercent = (double)completedTasks / totalChars;
                            ProcessingProgress.Progress = progressPercent;
                            ProgressText.Text = $"Progres: {progressPercent:P0}";
                        });
                    }

                    concurrentResults.Enqueue($"{processedSegment}");
                }, tokenSource.Token);

                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            string result = string.Join(Environment.NewLine, concurrentResults);
            EncryptedMessage.Text = result;

            stopwatch.Stop();
            ExecutionTime.Text = $"Timp de executie pentru {(isCriptare ? "criptare" : "decriptare")} Task.WhenAll: {stopwatch.ElapsedMilliseconds}ms";
        }
        catch (OperationCanceledException)
        {
            HandleCancellation();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Eroare", ex.Message, "OK");
        }
        finally
        {
            HandleCompletion();
        }
    }
    private async void OnCriptareParallelClicked(object sender, EventArgs e)
    {
        await ProcessTextParallel(true);
    }

    private async void OnDecriptareParallelClicked(object sender, EventArgs e)
    {
        await ProcessTextParallel(false);
    }

    private void HandleCancellation()
    {
        EncryptedMessage.Text = "Operația a fost anulată de utilizator";
        ExecutionTime.Text = "Operație anulată";
        ProcessingProgress.Progress = 0;
        ProgressText.Text = "Progres: Anulat";
    }

    private void HandleCompletion()
    {
        CriptareButton.IsEnabled = true;
        DecriptareButton.IsEnabled = true;
        CriptareParallelButton.IsEnabled = true;
        DecriptareParallelButton.IsEnabled = true;
        CancelButton.IsEnabled = false;
        tokenSource?.Dispose();
    }
}