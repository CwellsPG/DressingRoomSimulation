using System;
using System.Diagnostics;
using System.Threading;

public class DressingRooms
{
    private Semaphore _semaphore;
    private int _numberOfRooms;

    public DressingRooms(int numberOfRooms = 3)
    {
        _numberOfRooms = numberOfRooms;
        _semaphore = new Semaphore(_numberOfRooms, _numberOfRooms);
    }

    public void RequestRoom(int customerId, int numberOfItems)
    {
        Console.WriteLine($"Customer {customerId} is waiting for a room.");
        _semaphore.WaitOne();
        Console.WriteLine($"Customer {customerId} entered a room with {numberOfItems} items.");

        Random random = new Random();
        int timeToTryOn = random.Next(1, 4) * numberOfItems;
        Thread.Sleep(timeToTryOn * 1000);

        Console.WriteLine($"Customer {customerId} is done after {timeToTryOn} seconds.");
        _semaphore.Release();
    }
}

public class Customer
{
    private int _customerId;
    private int _numberOfItems;
    private DressingRooms _dressingRooms;

    public int NumberOfItems => _numberOfItems; 

    public Customer(int customerId, int numberOfItems, DressingRooms dressingRooms)
    {
        _customerId = customerId;
        _numberOfItems = numberOfItems;
        _dressingRooms = dressingRooms;
    }

    public void TryOnClothes()
    {
        _dressingRooms.RequestRoom(_customerId, _numberOfItems);
    }
}

public class Scenario
{
    private int _numberOfRooms;
    private int _numberOfCustomers;
    private DressingRooms _dressingRooms;
    private Customer[] _customers;
    private int _totalItems;
    private int _totalTime;
    private int _waitingTime;

    public Scenario(int numberOfRooms, int numberOfCustomers)
    {
        _numberOfRooms = numberOfRooms;
        _numberOfCustomers = numberOfCustomers;
        _dressingRooms = new DressingRooms(_numberOfRooms);
        _customers = new Customer[_numberOfCustomers];
    }

    public void RunScenario()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        Random random = new Random();

        for (int i = 0; i < _numberOfCustomers; i++)
        {
            int numberOfItems = random.Next(1, 7);
            _customers[i] = new Customer(i + 1, numberOfItems, _dressingRooms);
            _totalItems += numberOfItems;
        }

        Thread[] customerThreads = new Thread[_numberOfCustomers];
        for (int i = 0; i < _numberOfCustomers; i++)
        {
            int capture = i;
            customerThreads[i] = new Thread(() =>
            {
                Stopwatch customerStopwatch = new Stopwatch();
                customerStopwatch.Start();

                _customers[capture].TryOnClothes();

                customerStopwatch.Stop();
                _totalTime += customerStopwatch.Elapsed.Seconds;
                _waitingTime += customerStopwatch.Elapsed.Seconds - (random.Next(1, 4) * _customers[capture].NumberOfItems);
            });

            customerThreads[i].Start();
        }

        foreach (Thread t in customerThreads)
        {
            t.Join();
        }

        stopwatch.Stop();

        Console.WriteLine("\n--- Scenario Summary ---");
        Console.WriteLine($"Number of Rooms: {_numberOfRooms}");
        Console.WriteLine($"Number of Customers: {_numberOfCustomers}");
        Console.WriteLine($"Total Time Elapsed: {stopwatch.Elapsed.TotalSeconds} seconds");
        Console.WriteLine($"Average Number of Items: {(double)_totalItems / _numberOfCustomers}");
        Console.WriteLine($"Average Usage Time per Room: {(double)_totalTime / _numberOfCustomers} seconds");
        Console.WriteLine($"Average Waiting Time per Customer: {(double)_waitingTime / _numberOfCustomers} seconds\n");
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        Scenario scenario1 = new Scenario(3, 10);
        scenario1.RunScenario();

        Scenario scenario2 = new Scenario(5, 15);
        scenario2.RunScenario();

        Scenario scenario3 = new Scenario(7, 20);
        scenario3.RunScenario();
    }
}
