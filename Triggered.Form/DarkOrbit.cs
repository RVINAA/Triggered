using System.ComponentModel;

namespace Triggered
{
	using Callback = Action<object, DoWorkEventArgs>;

	internal static class DarkOrbit
	{
		#region Fields

		private static readonly IDictionary<Keys, BackgroundWorker> _workers = new Dictionary<Keys, BackgroundWorker>(capacity: 5);
		private static readonly object _lock = new();

#if LASER
		private const Keys STOP_CODE = Keys.LControlKey;
		private const Keys LASER_CODE = Keys.NumPad1;

		private const string X4_KEY = "{1}";
		private const string RSB_75_KEY = "{2}";
		private const string PIB_100_KEY = "{F1}";

		private const int RSB_75_CD = 3000;
		private static DateTime? _nextRsb75;
#endif

		private const Keys MISILE_CODE = Keys.NumPad2;
		private const Keys ISH_BOMB_CODE = Keys.NumPad3;
		private const Keys DMG_MINE_CODE = Keys.NumPad4;
		private const Keys SLW_MINE_CODE = Keys.NumPad5;

		// Not naming them rigth now.
		private const string MISILE_0 = "{F2}";
		private const string MISILE_1 = "{F3}";
		private const string MISILE_2 = "{F4}";

		private const string IBOMB_KEY = "{F5}";
		private const string ISH01_KEY = "{F6}";

		private const string WHATEVER = "{F7}";
		private const string SL_M01 = "{F8}";

		private const string PET_DEFENSE = "{R}";
		private const string DD_M01_KEY = "{F9}";
		private const string MEGA_MINE_KEY = "{F10}";

		private const int KEY_PRESSED_DELAY = 250;

		#endregion

		#region Private methods

		private static void SendKeysWithDelay(params string[] keys)
		{
			foreach (var key in keys)
			{
				SendKeys.SendWait(key);
				Thread.Sleep(KEY_PRESSED_DELAY);
			}
		}

#if LASER
		private static BackgroundWorker RunWorkerAsync(Callback callback)
		{
			var worker = new BackgroundWorker();
			worker.DoWork += new DoWorkEventHandler(callback);
			worker.WorkerSupportsCancellation = true;
			worker.RunWorkerAsync();
			return worker;
		}

		private static void MaybeAdd(Keys key, Callback callback, bool force = false)
		{
			if (!_workers.ContainsKey(key))
			{
				lock (_lock)
				{
					if (!_workers.ContainsKey(key))
						_workers.Add(key, RunWorkerAsync(callback));
				}
			}
			else if (force)
			{
				lock (_lock)
				{
					_workers[key].CancelAsync();
					_workers[key] = RunWorkerAsync(callback);
				}
			}
		}

		private static void LaserCallback(object sender, DoWorkEventArgs e)
		{
			var worker = (BackgroundWorker)sender;
			if (worker.CancellationPending) goto CANCEL;

			SendKeys.SendWait(PIB_100_KEY);
			Thread.Sleep(KEY_PRESSED_DELAY);
			SendKeys.SendWait(X4_KEY);
			Thread.Sleep(KEY_PRESSED_DELAY);

			while (!worker.CancellationPending)
			{
				if (_nextRsb75 != null && DateTime.UtcNow <= _nextRsb75)
					continue;

				SendKeys.SendWait(RSB_75_KEY);
				Thread.Sleep(KEY_PRESSED_DELAY); //< Wait some ms before changing again to X4..
				_nextRsb75 = DateTime.UtcNow.AddMilliseconds(RSB_75_CD);

				// NOTE: Sometimes the game is weido and RSB-75
				//		 key is not available (idkw), so we are pressing
				//		 X4 again and stopping the ammo shooting :)
				SendKeys.SendWait(X4_KEY);
				Thread.Sleep(RSB_75_CD);
			}

		CANCEL:
			e.Cancel = true;
		}

		private static void Clear()
		{
			lock (_lock)
			{
				foreach (var worker in _workers)
				{
					worker.Value.CancelAsync();
					worker.Value.Dispose();
				}

				_workers.Clear();
			}
		}
#endif

		#endregion

		public static void OnKeyPressed(object sender, Keys key)
		{
			try
			{
				switch (key)
				{
#if LASER
					case LASER_CODE:
						if (!Monitor.TryEnter(_lock)) return;
						MaybeAdd(LASER_CODE, LaserCallback, force: true);
						Monitor.Exit(_lock);
						return;

					case STOP_CODE:
						if (!Monitor.TryEnter(_lock)) return;
						Clear();
						Monitor.Exit(_lock);
						return;
#endif
					case MISILE_CODE:
						if (!Monitor.TryEnter(_lock)) return;
						SendKeysWithDelay(MISILE_1, MISILE_2, MISILE_0);
						Monitor.Exit(_lock);
						return;

					case ISH_BOMB_CODE:
						if (!Monitor.TryEnter(_lock)) return;
						SendKeysWithDelay(IBOMB_KEY, ISH01_KEY);
						Monitor.Exit(_lock);
						return;

					case DMG_MINE_CODE:
						if (!Monitor.TryEnter(_lock)) return;
						SendKeysWithDelay(DD_M01_KEY, MEGA_MINE_KEY, PET_DEFENSE);
						Monitor.Exit(_lock);
						return;

					case SLW_MINE_CODE:
						if (!Monitor.TryEnter(_lock)) return;
						SendKeysWithDelay(SL_M01, WHATEVER);
						Monitor.Exit(_lock);
						return;
				}
			}
			catch
			{
				if (Monitor.IsEntered(_lock))
					Monitor.Exit(_lock);
			}
		}
	}
}