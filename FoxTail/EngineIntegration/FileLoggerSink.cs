using NotEnoughLogs;
using NotEnoughLogs.Sinks;

namespace FoxTail.EngineIntegration;

public class FileLoggerSink : ILoggerSink, IDisposable
{
    private readonly FileStream _stream;
    private readonly StreamWriter _writer;

    public FileLoggerSink()
    {
        string filename = $"FoxTail - {DateTime.Now:yyyy-MM-dd HH_mm_ss}.log";
        string path = Path.Combine(Environment.CurrentDirectory, "Logs", filename);

        this._stream = File.OpenWrite(path);
        this._writer = new StreamWriter(this._stream);
        
        this._writer.WriteLine("FoxTail for Resonite");
    }

    public void Log(LogLevel level, ReadOnlySpan<char> category, ReadOnlySpan<char> content)
    {
        DateTime time = DateTime.Now;
        
        WriteBracketed($"{time:MM/dd/yy} {time:HH:mm:ss}");
        this._writer.Write(' ');
        this.WriteBracketed(level.ToString());
        this._writer.Write(' ');
        WriteBracketed(category);
        this._writer.Write(' ');
        
        this._writer.WriteLine(content);
    }

    public void Log(LogLevel level, ReadOnlySpan<char> category, ReadOnlySpan<char> format, params object[] args)
    {
        this.Log(level, category, string.Format(format.ToString(), args));
    }
    
    private void WriteBracketed(string str)
    {
        this._writer.Write('[');
        this._writer.Write(str);
        this._writer.Write(']');
    }

    private void WriteBracketed(ReadOnlySpan<char> span)
    {
        this._writer.Write('[');
        this._writer.Write(span);
        this._writer.Write(']');
    }

    public void Dispose()
    {
        _stream.Flush();
        _writer.Flush();
        _stream.Dispose();
        _writer.Dispose();
    }
}