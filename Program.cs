Cont cont = new Cont(1000);
Thread[] threads = new Thread[5];

for (int i = 0; i < threads.Length; i++)
{
  threads[i] = new Thread(cont.Tranzactii);
  threads[i].Start();
}

foreach (Thread t in threads)
{
  t.Join();
}

Console.WriteLine($"Sold final: {cont.Sold} RON");
Console.WriteLine($"Numar total de retrageri efectuate cu succes: {cont.NumarDeRetrageri}.");