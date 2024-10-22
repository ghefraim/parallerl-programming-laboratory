using System;
using System.Threading;

class Cont
{
  private long sold;
  private int numarDeRetrageri = 0;
  private Random r = new Random();

  public Cont(long soldInitial)
  {
    sold = soldInitial;
  }

  public long Retrage(long suma)
  {
    long soldCurent;
    do
    {
      soldCurent = Interlocked.Read(ref sold);

      if (soldCurent < suma)
      {
        Console.WriteLine($"Fonduri insuficiente pentru retragerea de {suma} RON. Sold actual: {soldCurent} RON.");
        return 0;
      }

    } while (Interlocked.CompareExchange(ref sold, soldCurent - suma, soldCurent) != soldCurent);

    Interlocked.Increment(ref numarDeRetrageri);
    Console.WriteLine($"Retragere de {suma} RON reușită. Sold rămas: {soldCurent - suma} RON.");
    return suma;
  }

  public void Tranzactii()
  {
    for (int i = 0; i < 20; i++)
    {
      long soldCurent = Interlocked.Read(ref sold);

      // Oprire dacă soldul este zero sau negativ
      if (soldCurent <= 0)
      {
        Console.WriteLine("Nu mai sunt fonduri disponibile. Oprire tranzacții.");
        break;
      }

      long sumaDeRetras = r.Next(1, 100);
      Retrage(sumaDeRetras);
      Thread.Sleep(r.Next(50, 200));
    }
  }

  public int GetNumarDeRetrageri()
  {
    return Interlocked.CompareExchange(ref numarDeRetrageri, 0, 0);
  }

  public long GetSold()
  {
    return Interlocked.Read(ref sold);
  }
}
