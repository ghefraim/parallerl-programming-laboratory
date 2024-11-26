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
                int threadIndex = i;

                var task = TF.StartNew(() =>
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
                                result.Append(CriptareDecriptare.Criptare(segmentPentruTask[j].ToString()));
                            }
                            else
                            {
                                result.Append(CriptareDecriptare.Decriptare(segmentPentruTask[j].ToString()));
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
                }, tokenSource.Token);

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

            await Task.Run(() =>
            {
                Parallel.For(0, processorCount, (i, state) =>
                {
                    Debug.Print($"În thread-ul Parallel.For: {Thread.CurrentThread.ManagedThreadId}");

                    int startIndex = i * charsPerTask;
                    if (startIndex >= text.Length) return;

                    int endIndex = Math.Min(startIndex + charsPerTask, text.Length);
                    string segment = text.Substring(startIndex, endIndex - startIndex);

                    StringBuilder processedSegment = new StringBuilder();
                    for (int j = 0; j < segment.Length; j++)
                    {
                        if (tokenSource.Token.IsCancellationRequested)
                        {
                            state.Stop();
                            HandleCancellation();
                            return;
                        }

                        Thread.Sleep(100);

                        if (isCriptare)
                        {
                            processedSegment.Append(CriptareDecriptare.Criptare(segment[j].ToString()));
                        }
                        else
                        {
                            processedSegment.Append(CriptareDecriptare.Decriptare(segment[j].ToString()));
                        }

                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            completedTasks++;
                            double progressPercent = (double)completedTasks / totalChars;
                            ProcessingProgress.Progress = progressPercent;
                            ProgressText.Text = $"Progres: {progressPercent:P0}";
                        });
                    }

                    concurrentResults.Enqueue(processedSegment.ToString());
                });
            });

            string result = string.Concat(concurrentResults);
            EncryptedMessage.Text = result;

            stopwatch.Stop();
            ExecutionTime.Text = $"Timp de executie pentru {(isCriptare ? "criptare" : "decriptare")} Parallel.For: {stopwatch.ElapsedMilliseconds}ms";
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

    // private async Task ProcessTextPLINQ(bool isCriptare)
    // {
    //     var stopwatch = Stopwatch.StartNew();
    //     var text = OriginalMessage.Text ?? string.Empty;
    //     if (string.IsNullOrEmpty(text)) return;
    //
    //     tokenSource = new CancellationTokenSource();
    //     regularResults.Clear();
    //     completedTasks = 0;
    //
    //     ProcessingProgress.Progress = 0;
    //     ProgressText.Text = "Progres: 0%";
    //
    //     CriptarePLINQButton.IsEnabled = false;
    //     DecriptarePLINQButton.IsEnabled = false;
    //     CancelButton.IsEnabled = true;
    //
    //     try
    //     {
    //         int processorCount = Environment.ProcessorCount;
    //         int charsPerTask = (text.Length + processorCount - 1) / processorCount;
    //         var totalChars = text.Length;
    //
    //         await Task.Run(() =>
    //         {
    //             var segments = Enumerable.Range(0, processorCount)
    //                 .Select(i =>
    //                 {
    //                     int startIndex = i * charsPerTask;
    //                     if (startIndex >= text.Length) return string.Empty;
    //                     int endIndex = Math.Min(startIndex + charsPerTask, text.Length);
    //                     return text.Substring(startIndex, endIndex - startIndex);
    //                 })
    //                 .Where(s => !string.IsNullOrEmpty(s));
    //
    //             var query = segments.AsParallel()
    //                               .WithDegreeOfParallelism(processorCount)
    //                               .WithCancellation(tokenSource.Token)
    //                               .Select(segment =>
    //                               {
    //                                   Debug.Print($"În thread-ul PLINQ: {Thread.CurrentThread.ManagedThreadId}");
    //                                   StringBuilder processedSegment = new StringBuilder();
    //                                   foreach (char c in segment)
    //                                   {
    //                                       Thread.Sleep(100);
    //                                       if (isCriptare)
    //                                       {
    //                                           processedSegment.Append(CriptareDecriptare.Criptare(c.ToString()));
    //                                       }
    //                                       else
    //                                       {
    //                                           processedSegment.Append(CriptareDecriptare.Decriptare(c.ToString()));
    //                                       }
    //
    //                                       MainThread.BeginInvokeOnMainThread(() =>
    //                                       {
    //                                           completedTasks++;
    //                                           double progressPercent = (double)completedTasks / totalChars;
    //                                           ProcessingProgress.Progress = progressPercent;
    //                                           ProgressText.Text = $"Progres: {progressPercent:P0}";
    //                                       });
    //                                   }
    //                                   return processedSegment.ToString();
    //                               });
    //
    //             regularResults.AddRange(query.ToList());
    //         });
    //
    //         string result = string.Concat(regularResults);
    //         EncryptedMessage.Text = result;
    //
    //         stopwatch.Stop();
    //         ExecutionTime.Text = $"Timp de executie pentru {(isCriptare ? "criptare" : "decriptare")} PLINQ: {stopwatch.ElapsedMilliseconds}ms";
    //     }
    //     catch (OperationCanceledException)
    //     {
    //         HandleCancellation();
    //     }
    //     catch (Exception ex)
    //     {
    //         await DisplayAlert("Eroare", ex.Message, "OK");
    //     }
    //     finally
    //     {
    //         HandleCompletion();
    //     }
    // }

    private async void OnCriptareParallelClicked(object sender, EventArgs e)
    {
        await ProcessTextParallel(true);
    }

    private async void OnDecriptareParallelClicked(object sender, EventArgs e)
    {
        await ProcessTextParallel(false);
    }

    // private async void OnCriptarePLINQClicked(object sender, EventArgs e)
    // {
    //     await ProcessTextPLINQ(true);
    // }
    //
    // private async void OnDecriptarePLINQClicked(object sender, EventArgs e)
    // {
    //     await ProcessTextPLINQ(false);
    // }

    private void HandleCancellation()
    {
        EncryptedMessage.Text = "Operația a fost anulată de utilizator";
        // ConcurrentResult.Text = "Operație anulată";
        // RegularResult.Text = "Operație anulată";
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
        // CriptarePLINQButton.IsEnabled = true;
        // DecriptarePLINQButton.IsEnabled = true;
        CancelButton.IsEnabled = false;
        tokenSource?.Dispose();
    }
}