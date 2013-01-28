using System;
using System.Collections.Generic;
using System.Timers;

namespace Microsoft.PowerShell.Workflow
{
	internal sealed class PSTimer : IDisposable
	{
		private bool disposed;

		private Timer timer;

		private readonly object syncLock;

		private bool IsRecurring;

		private bool IsOneTimeTimer;

		private TimeSpan Interval;

		private TimeSpan RemainingTime;

		private DateTime? StartedAtForFirstTime;

		private DateTime? StartedTime;

		private bool IsRunning;

		internal bool TimerReachedAlready;

		internal WorkflowTimerType TimerType
		{
			get;
			private set;
		}

		internal PSTimer(WorkflowTimerType type, bool isRecurring, bool isOneTimeTimer, TimeSpan interval, WorkflowTimerElapsedHandler handler)
		{
			this.syncLock = new object();
			this.TimerType = type;
			this.IsRecurring = isRecurring;
			this.IsOneTimeTimer = isOneTimeTimer;
			this.Interval = interval;
			this.RemainingTime = interval;
			this.StartedAtForFirstTime = null;
			this.StartedTime = null;
			this.IsRunning = false;
			this.Handler = handler;
		}

		internal PSTimer(Dictionary<string, object> data, WorkflowTimerElapsedHandler handler)
		{
			this.syncLock = new object();
			this.TimerType = (WorkflowTimerType)data["TimerType"];
			this.IsRecurring = (bool)data["IsRecurring"];
			this.IsOneTimeTimer = (bool)data["IsOneTimeTimer"];
			this.Interval = (TimeSpan)data["Interval"];
			if (this.IsRecurring || this.IsOneTimeTimer)
			{
				if (this.IsRecurring || !this.IsOneTimeTimer)
				{
					this.RemainingTime = this.Interval;
				}
				else
				{
					DateTime item = (DateTime)data["StartedAtForFirstTime"];
					DateTime utcNow = DateTime.UtcNow;
					TimeSpan interval = this.Interval - utcNow.Subtract(item);
					if (interval > TimeSpan.FromSeconds(0))
					{
						if (interval >= TimeSpan.FromSeconds(2))
						{
							this.RemainingTime = interval;
						}
						else
						{
							this.RemainingTime = TimeSpan.FromSeconds(2);
						}
					}
					else
					{
						this.TimerReachedAlready = true;
						this.RemainingTime = TimeSpan.FromSeconds(2);
					}
				}
			}
			else
			{
				this.RemainingTime = (TimeSpan)data["RemainingTime"];
			}
			this.StartedAtForFirstTime = null;
			this.StartedTime = null;
			this.IsRunning = false;
			this.Handler = handler;
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (this.disposed || !disposing)
			{
				return;
			}
			else
			{
				lock (this.syncLock)
				{
					if (!this.disposed)
					{
						this.Stop();
						if (this.timer != null)
						{
							this.timer.Elapsed -= new ElapsedEventHandler(this.timer_Elapsed);
							this.Handler = null;
							this.timer.Dispose();
						}
						this.disposed = true;
					}
				}
				return;
			}
		}

		internal Dictionary<string, object> GetSerializedData()
		{
			Dictionary<string, object> strs = new Dictionary<string, object>();
			strs.Add("TimerType", this.TimerType);
			strs.Add("IsRecurring", this.IsRecurring);
			strs.Add("IsOneTimeTimer", this.IsOneTimeTimer);
			strs.Add("Interval", this.Interval);
			if (this.IsRecurring || this.IsOneTimeTimer)
			{
				if (!this.IsRecurring && this.IsOneTimeTimer)
				{
					if (!this.StartedAtForFirstTime.HasValue)
					{
						strs.Add("StartedAtForFirstTime", DateTime.UtcNow);
					}
					else
					{
						strs.Add("StartedAtForFirstTime", this.StartedAtForFirstTime);
					}
				}
			}
			else
			{
				if (!this.IsRunning)
				{
					strs.Add("RemainingTime", this.RemainingTime);
				}
				else
				{
					DateTime utcNow = DateTime.UtcNow;
					TimeSpan remainingTime = this.RemainingTime - utcNow.Subtract(this.StartedTime.Value);
					if (remainingTime < TimeSpan.FromMilliseconds(0))
					{
						remainingTime = TimeSpan.FromMilliseconds(0);
					}
					strs.Add("RemainingTime", remainingTime);
				}
			}
			return strs;
		}

		internal void Start()
		{
			if (!this.disposed)
			{
				if (!this.IsRunning)
				{
					if (this.Interval > TimeSpan.FromMilliseconds(0))
					{
						lock (this.syncLock)
						{
							if (!this.IsRunning)
							{
								if (this.timer != null)
								{
									this.timer.Interval = this.RemainingTime.TotalMilliseconds;
								}
								else
								{
									this.timer = new Timer(this.RemainingTime.TotalMilliseconds);
									this.timer.AutoReset = this.IsRecurring;
									this.timer.Elapsed += new ElapsedEventHandler(this.timer_Elapsed);
									this.StartedAtForFirstTime = new DateTime?(DateTime.UtcNow);
								}
								this.timer.Start();
								this.IsRunning = true;
								this.StartedTime = new DateTime?(DateTime.UtcNow);
							}
						}
						return;
					}
					else
					{
						return;
					}
				}
				else
				{
					return;
				}
			}
			else
			{
				return;
			}
		}

		internal void Stop()
		{
			if (!this.disposed)
			{
				if (this.IsRunning)
				{
					lock (this.syncLock)
					{
						if (this.IsRunning)
						{
							if (this.timer != null)
							{
								this.timer.Stop();
								this.IsRunning = false;
								if (!this.IsRecurring)
								{
									PSTimer remainingTime = this;
									DateTime utcNow = DateTime.UtcNow;
									remainingTime.RemainingTime = remainingTime.RemainingTime - utcNow.Subtract(this.StartedTime.Value);
									if (this.RemainingTime < TimeSpan.FromMilliseconds(0))
									{
										this.RemainingTime = TimeSpan.FromMilliseconds(0);
									}
								}
							}
						}
					}
					return;
				}
				else
				{
					return;
				}
			}
			else
			{
				return;
			}
		}

		private void timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (!this.disposed)
			{
				if (this.IsRunning)
				{
					lock (this.syncLock)
					{
						if (this.IsRunning)
						{
							if (this.Handler != null)
							{
								this.Handler(this, e);
							}
							if (!this.IsRecurring)
							{
								this.IsRunning = false;
							}
						}
					}
					return;
				}
				else
				{
					return;
				}
			}
			else
			{
				return;
			}
		}

		private event WorkflowTimerElapsedHandler Handler;
	}
}