public class ConsoleSpinner : IDisposable {
    private bool _active;
    private readonly Thread _thread;
    public ConsoleSpinner() => _thread = new Thread(Spin);

    public void Start(string msg) {
        _active = true;
        System.Console.Write($"{msg} ");
        if (_thread.ThreadState == ThreadState.Unstarted) _thread.Start();
    }

    public void Stop() {
        _active = false;
        System.Console.Write("\b \b\n");
    }

    private void Spin() {
        string[] chars = ["/", "-", "\\", "|"];
        int i = 0;
        while (_active) {
            System.Console.Write(chars[i++ % 4]);
            Thread.Sleep(100);
            System.Console.Write("\b");
        }
    }
    public void Dispose() => Stop();
}