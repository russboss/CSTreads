// MonitorSample.cs
// This example shows use of the following methods of the C# lock keyword
// and the Monitor class 
// in threads:
//      Monitor.Pulse(Object)
//      Monitor.Wait(Object)
using System;
using System.Threading;

public class MonitorSample
{
    public static void Main(String[] args)
    {
        int result = 0;   // Result initialized to say there is no error
        Cell cell = new Cell();

        CellProd prod = new CellProd(cell, 50, 2, "Production");  // Use cell for storage, 
        // produce 20 items
        CellCons cons = new CellCons(cell, 21, 3, "Consumer 1");  // Use cell for storage, 
        // consume 20 items
        CellCons cons2 = new CellCons(cell, 10, 1, "Consumer 2");  // Use cell for storage, 
        // consume 20 items


        Thread producer = new Thread(new ThreadStart(prod.ThreadRun));
        Thread consumer = new Thread(new ThreadStart(cons.ThreadRun));
        Thread consumer2 = new Thread(new ThreadStart(cons2.ThreadRun));
        // Threads producer and consumer have been created, 
        // but not started at this point.

        try
        {
            producer.Start();
            consumer.Start();
            consumer2.Start();

            producer.Join();   // Join both threads with no timeout
            // Run both until done.
            consumer.Join();
            consumer2.Join();
            // threads producer and consumer have finished at this point.
        }
        catch (ThreadStateException e)
        {
            Console.WriteLine(e);  // Display text of exception
            result = 1;            // Result says there was an error
        }
        catch (ThreadInterruptedException e)
        {
            Console.WriteLine(e);  // This exception means that the thread
            // was interrupted during a Wait
            result = 1;            // Result says there was an error
        }
        // Even though Main returns void, this provides a return code to 
        // the parent process.
        Environment.ExitCode = result;
        Console.ReadLine();

    }
}

public class CellProd
{
    Cell cell;         // Field to hold cell object to be used
    int quantity = 1;  // Field for how many items to produce in cell
    int rate = 1;
    String name = "";
    public CellProd(Cell box, int request, int step, String inName)
    {
        cell = box;          // Pass in what cell object to be used
        quantity = request;  // Pass in how many items to produce in cell
        rate = step; // how many to produce at a time
        name = inName;
    }
    public void ThreadRun()
    {
        int val=0;
        for (int looper = 1; looper <= quantity; looper++)
        {
            val = cell.WriteToCell(rate, name);  // "producing"
            
        }

    }
}

public class CellCons
{
    Cell cell;         // Field to hold cell object to be used
    int quantity = 1;  // Field for how many items to consume from cell
    int rate=1;
    String name = "";
    public CellCons(Cell box, int request, int step, String inName)
    {
        cell = box;          // Pass in what cell object to be used
        quantity = request;  // Pass in how many items to consume from cell
        rate = step;    // how many to remove at a time
        name = inName;
    }
    public void ThreadRun()
    {
        int valReturned=0;
        for (int looper = 1; looper <= quantity; looper++){
            // Consume the result by placing it in valReturned.
            valReturned = cell.ReadFromCell(rate,name); // "consuming"

        }

    }
}

public class Cell
{
    int cellContents;         // Cell contents
    bool readerFlag = false;  // State flag

    public int ReadFromCell(int n, String name)
    {
        String str = "";
        lock (this)   // Enter synchronization block
        {
            if (!readerFlag)
            {            // Wait until Cell.WriteToCell is done producing
                try
                {
                    // Waits for the Monitor.Pulse in WriteToCell
                    Monitor.Wait(this);
                }
                catch (SynchronizationLockException e)
                {
                    Console.WriteLine(e);
                }
                catch (ThreadInterruptedException e)
                {
                    Console.WriteLine(e);
                }
            }

            if (n > cellContents)
            {
                str = name +" Cannot Consume: "+n+" CellContents: "+cellContents;
            }
            else
            {
                cellContents -= n;
                //Console.WriteLine("Consume: {0}", cellContents);
                str = name +" Consume: "+n+" CellContents: " +cellContents;
            }
            readerFlag = false;    // Reset the state flag to say consuming is done.
            Monitor.Pulse(this);   // Pulse tells Cell.WriteToCell that Cell.ReadFromCell is done.
            Console.WriteLine(str);
        }   // Exit synchronization block
        return n;
    }

    public int WriteToCell(int n, String name)
    {
        String str = "";
        lock (this)  // Enter synchronization block
        {
            if (readerFlag)
            {      // Wait until Cell.ReadFromCell is done consuming.
                try
                {
                    Monitor.Wait(this);   // Wait for the Monitor.Pulse in ReadFromCell
                }
                catch (SynchronizationLockException e)
                {
                    Console.WriteLine(e);
                }
                catch (ThreadInterruptedException e)
                {
                    Console.WriteLine(e);
                }
            }
            cellContents += n;

            readerFlag = true;    // Reset the state flag to say producing is done
            Monitor.Pulse(this);  // Pulse tells Cell.ReadFromCell that Cell.WriteToCell is done.
            str = name +" Produce: " + n + " CellContents: " + cellContents;
            Console.WriteLine(str); 
            return n;
        }   // Exit synchronization block
    }
}