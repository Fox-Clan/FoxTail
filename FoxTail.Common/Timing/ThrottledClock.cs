// This code is adapted from osu-framework.
// 
// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file at https://github.com/ppy/osu-framework/blob/master/LICENCE for full licence text.
//
// See the original code here on GitHub:
// https://github.com/ppy/osu-framework/blob/527d836d37009ce0b84521ad4b74914acbe89927/osu.Framework/Timing/ThrottledFrameClock.cs

using FoxTail.Common.Timing.ThrottleMethods;

namespace FoxTail.Common.Timing;

public sealed class ThrottledClock : Clock
{
   public double TargetUpdateHz = 60.0d;
   public bool ShouldThrottle = true;
   public double TimeSlept { get; private set; }

   private readonly IThrottleMethod _throttleMethod;
   
   public ThrottledClock(bool allowPreciseThrottle = true)
   {
      if (!allowPreciseThrottle)
         goto fallback;

      try
      {
         if (OperatingSystem.IsWindows())
            this._throttleMethod = new WindowsThrottleMethod();
         else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS() || OperatingSystem.IsFreeBSD())
            this._throttleMethod = new UnixThrottleMethod();
         else goto fallback;

         // test the throttling method
         if (!this._throttleMethod.Sleep(0))
            goto fallback;
      }
      catch
      {
         goto fallback;
      }

      return;
      fallback:
      this._throttleMethod = new ImpreciseThrottleMethod();
   }

   public void Wait()
   {
      if (!ShouldThrottle) return;

      ProcessFrame();

      // ReSharper disable once MergeIntoPattern
      if (TargetUpdateHz > 0 && TargetUpdateHz < double.MaxValue)
      {
         Throttle();
      }
      else
      {
         // Even when running at unlimited frame-rate, we should call the scheduler
         // to give lower-priority background processes a chance to do work.
         this.TimeSlept = SleepAndUpdate(0);
      }
   }

   private double _accumulatedSleepError;

   private void Throttle()
   {
      double excessFrameTime = 1000d / TargetUpdateHz - ElapsedFrameTime;
      this.TimeSlept = SleepAndUpdate(Math.Max(0, excessFrameTime + this._accumulatedSleepError));

      _accumulatedSleepError += excessFrameTime - TimeSlept;
      
      // Never allow the sleep error to become too negative and induce too many catch-up frames
      this._accumulatedSleepError = Math.Max(-1000 / 30.0, this._accumulatedSleepError);
   }

   private double SleepAndUpdate(double milliseconds)
   {
      // By returning here, in cases where the game is not keeping up, we don't yield.
      // Not 100% sure if we want to do this, but let's give it a try.
      if (milliseconds <= 0)
         return 0;
      
      double before = CurrentTime;

      if (milliseconds == 0)
         this._throttleMethod.Yield();
      else
         this._throttleMethod.Sleep(milliseconds);

      return (CurrentTime = SourceTime) - before;
   }

   public string ThrottleMethodName => this._throttleMethod.GetType().Name;

   public override string ToString()
      => $"{this.GetType().Name}({this.ThrottleMethodName}) UPS: {this.FramesPerSecond:N2}, Jitter: {this.Jitter:N2}ms";
}