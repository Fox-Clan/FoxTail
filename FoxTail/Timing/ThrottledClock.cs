// This code is adapted from osu-framework.
// 
// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file at https://github.com/ppy/osu-framework/blob/master/LICENCE for full licence text.
//
// See the original code here on GitHub:
// https://github.com/ppy/osu-framework/blob/527d836d37009ce0b84521ad4b74914acbe89927/osu.Framework/Timing/ThrottledFrameClock.cs

namespace FoxTail.Timing;

public sealed class ThrottledClock : Clock
{
   public double TargetUpdateHz = 60.0d;
   public bool ShouldThrottle = true;
   public double TimeSlept { get; private set; }

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
      
      TimeSpan timeSpan = TimeSpan.FromMilliseconds(milliseconds);

      // The actual function that waits. Adjust as necessary.
      Thread.Sleep(timeSpan);

      return (CurrentTime = SourceTime) - before;
   }
}