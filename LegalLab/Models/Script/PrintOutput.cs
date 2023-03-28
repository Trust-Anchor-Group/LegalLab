using System.IO;
using System.Text;

namespace LegalLab.Models.Script
{
	/// <summary>
	/// Redirects script print output to the script tab output.
	/// From the IoTGateway project, with permission.
	/// </summary>
	public class PrintOutput : TextWriter
	{
		private readonly ScriptModel scriptModel;

		/// <summary>
		/// Redirects script print output to the script tab output.
		/// From the IoTGateway project, with permission.
		/// </summary>
		/// <param name="ScriptModel">Script model</param>
		public PrintOutput(ScriptModel ScriptModel)
		{
			this.scriptModel = ScriptModel;
		}

		/// <inheritdoc/>
		public override Encoding Encoding => Encoding.UTF8;

		/// <inheritdoc/>
		public override void Write(string value)
		{
			this.scriptModel.Print(value);
		}
	}
}
