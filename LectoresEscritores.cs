using System;
using System.Threading;

class LectoresEscritores
{
    private ReaderWriterLockSlim lockSlim = new ReaderWriterLockSlim();
    private int lectores = 0;

    public void Leer()
    {
        lockSlim.EnterReadLock();
        try
        {
            Console.WriteLine($"Leyendo... (Lectores activos: {Interlocked.Increment(ref lectores)})");
            Thread.Sleep(100);
        }
        finally
        {
            Interlocked.Decrement(ref lectores);
            lockSlim.ExitReadLock();
        }
    }

    public void Escribir()
    {
        lockSlim.EnterWriteLock();
        try
        {
            Console.WriteLine("Escribiendo...");
            Thread.Sleep(100);
        }
        finally
        {
            lockSlim.ExitWriteLock();
        }
    }
}

class Program
{
    static void Main()
    {
        LectoresEscritores recurso = new LectoresEscritores();

        // Crear y ejecutar hilos de lectores
        Thread lector1 = new Thread(() => recurso.Leer());
        Thread lector2 = new Thread(() => recurso.Leer());
        Thread lector3 = new Thread(() => recurso.Leer());

        Thread escritor1 = new Thread(() => recurso.Escribir());
        Thread escritor2 = new Thread(() => recurso.Escribir());

        lector1.Start();
        lector2.Start();
        lector3.Start();

        escritor1.Start();
        escritor2.Start();
        
        lector1.Join();
        lector2.Join();
        lector3.Join();
        escritor1.Join();
        escritor2.Join();
    }
}
