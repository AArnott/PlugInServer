using System;
using System.IO;
using System.Windows.Forms;

namespace Byu.IT347.PluginServer.ServerGui
{
	/// <summary>
	/// Summary description for TextWriterBox.
	/// </summary>
	public class TextWriterBox : TextWriter
	{
		public TextWriterBox(TextBox textBox)
		{
			this.textBox = textBox;
		}

		private TextBox textBox;

		public override void Write(char value)
		{
			Write(value.ToString());
		}

		public override void Write(string value)
		{
			textBox.AppendText(value);
		}

		public override System.Text.Encoding Encoding
		{
			get
			{
				return System.Text.Encoding.ASCII;
			}
		}

	}
}
