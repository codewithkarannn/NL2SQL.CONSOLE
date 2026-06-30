namespace NL2SQL.POC.Console.Services;

public class ConsoleSpinner : IDisposable
{
    private readonly int _delay;
    private bool _active;
    private readonly Thread _thread;
    private int _counter;

    public ConsoleSpinner(int delay = 100)
    {
        _delay = delay;
        _thread = new Thread(Spin);
    }

    public void Start(string message = "Processing")
    {
        _active = true;
        System.Console.Write($"{message} ");
        if (_thread.ThreadState == ThreadState.Unstarted) 
        {
            _thread.Start();
        }
    }

    public void Stop()
    {
        _active = false;
        System.Console.Write("\b \b"); // Clean up last char
        System.Console.WriteLine();
    }

    private void Spin()
    {
        while (_active)
        {
            _counter++;
            switch (_counter % 4)
            {
                case 0: System.Console.Write("/"); break;
                case 1: System.Console.Write("-"); break;
                case 2: System.Console.Write("\\"); break;
                case 3: System.Console.Write("|"); break;
            }
            Thread.Sleep(_delay);
            System.Console.Write("\b");
        }
    }

    public void Dispose() => Stop();
}