using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Triggered
{
	internal class LowLevelKeyboardHook
	{
		#region Fields

		private const int WH_KEYBOARD_LL = 13;
		private const int WM_KEYDOWN = 0x0100;
		private const int WM_SYSKEYDOWN = 0x0104;

		private readonly LowLevelKeyboardProc _proc;
		private IntPtr _hookId = IntPtr.Zero;

		public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

		public event EventHandler<Keys> OnKeyPressed;

		#endregion

		#region .ctors

		public LowLevelKeyboardHook()
		{
			_proc = HookCallback;
		}

		#endregion

		#region Private methods

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool UnhookWindowsHookEx(IntPtr hhk);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern IntPtr GetModuleHandle(string lpModuleName);

		private static IntPtr SetHook(LowLevelKeyboardProc proc)
		{
			using var curProcess = Process.GetCurrentProcess();
			using var curModule = curProcess.MainModule;
			{
				return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
			}
		}

		private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
		{
			if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN)
			{
				var vkCode = Marshal.ReadInt32(lParam);
				OnKeyPressed.Invoke(this, ((Keys)vkCode));
			}

			return CallNextHookEx(_hookId, nCode, wParam, lParam);
		}

		#endregion

		public void HookKeyboard() => _hookId = SetHook(_proc);

		public void UnHookKeyboard() => UnhookWindowsHookEx(_hookId);
	}
}