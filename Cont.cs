using System;
using System.Threading;

class Cont
{
  public long Sold { get; set; }

  public int NumarDeRetrageri { get; set; }

  private readonly object lockObj = new object();

  private Random r = new Random();

  public Cont(long soldInitial)
  {
    Sold = soldInitial;
  }

  public long Retrage(long suma)
  {
    lock (lockObj)
    {
      if (Sold == 0)
      {
        Console.WriteLine("Soldul este zero. Nu se pot efectua retrageri.");
        return 0;
      }
      if (Sold >= suma)
      {
        Sold -= suma;
        NumarDeRetrageri++;
        Console.WriteLine($"Retragere de {suma} RON reusita. Sold ramas: {Sold} RON.");
        return suma;
      }
      else
      {
        Console.WriteLine($"Fonduri insuficiente pentru retragerea de {suma} RON. Sold actual: {Sold} RON.");
        return 0;
      }
    }
  }

  public void Tranzactii()
  {
    for (int i = 0; i < 20; i++)
    {
      long sumaDeRetras = r.Next(1, 100);
      Retrage(sumaDeRetras);
      Thread.Sleep(100);
    }
  }
}