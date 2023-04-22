namespace Triggered
{
	partial class Fxrm
	{
		#region Fields

		private LowLevelKeyboardHook _kbh;

		#endregion

		#region .ctors

		public Fxrm()
		{
			InitializeComponent();
			InitializeHook();
		}

		#endregion

		#region Private methods

		private void InitializeComponent()
		{
			FormBorderStyle = FormBorderStyle.FixedDialog;
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(239, 34);
			BackColor = Color.Black;
			MaximizeBox = false;
			Text = "Triggered";
			Name = "Fxrm";
		}

		private void InitializeHook()
		{
			_kbh = new LowLevelKeyboardHook();
			_kbh.OnKeyPressed += DarkOrbit.OnKeyPressed;
			_kbh.HookKeyboard();
		}

		#endregion

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				_kbh.UnHookKeyboard();

			base.Dispose(disposing);
		}
	}
}