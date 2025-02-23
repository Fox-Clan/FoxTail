using System.Diagnostics;

namespace FoxTail.Common.Timing;

public abstract class Clock
{
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
    
    // private readonly double[] _betweenFrameTimes = new double[128];
    
    public float FramesPerSecond { get; private set; }
    // public double Jitter { get; private set; }
    public virtual double CurrentTime { get; protected set; }
    protected virtual double LastFrameTime { get; set; }
    
    protected double SourceTime => _stopwatch.ElapsedTicks / (double) Stopwatch.Frequency * 1000.0;
    public double ElapsedFrameTime => CurrentTime - LastFrameTime;
    
    private const int FpsCalculationInterval = 250;
    
    private double _timeUntilNextCalculation;
    private double _timeSinceLastCalculation;
    private int _framesSinceLastCalculation;
    
    protected void ProcessFrame()
    {
        // _betweenFrameTimes[_totalFramesProcessed % _betweenFrameTimes.Length] = CurrentTime - LastFrameTime;
        // _totalFramesProcessed++;

        // if (processSource && Source is IFrameBasedClock framedSource)
            // framedSource.ProcessFrame();

        if (_timeUntilNextCalculation <= 0)
        {
            _timeUntilNextCalculation += FpsCalculationInterval;

            if (_framesSinceLastCalculation == 0)
            {
                FramesPerSecond = 0;
                // Jitter = 0;
            }
            else
            {
                FramesPerSecond = _framesSinceLastCalculation * 1000f / (float)_timeSinceLastCalculation;

                // simple stddev
                // double sum = 0;
                // double sumOfSquares = 0;
                //
                // foreach (double v in _betweenFrameTimes)
                // {
                //     sum += v;
                //     sumOfSquares += v * v;
                // }

                // double avg = sum / _betweenFrameTimes.Length;
                // double variance = (sumOfSquares / _betweenFrameTimes.Length) - (avg * avg);
                // Jitter = Math.Sqrt(variance);
            }

            _timeSinceLastCalculation = _framesSinceLastCalculation = 0;
        }

        _framesSinceLastCalculation++;
        _timeUntilNextCalculation -= ElapsedFrameTime;
        _timeSinceLastCalculation += ElapsedFrameTime;

        LastFrameTime = CurrentTime;
        CurrentTime = SourceTime;
    }
}