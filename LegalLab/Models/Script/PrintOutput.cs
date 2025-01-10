using System.IO;
using System.Text;

namespace LegalLab.Models.Script
{
	/// <summary>
	/// Redirects script print output to the script tab output.
	/// From the IoTGateway project, with permission.
	/// </summary>
	/// <param name="ScriptModel">Script model</param>
	public class PrintOutput(ScriptModel ScriptModel) 
		: TextWriter
	{
		private readonly ScriptModel scriptModel = ScriptModel;

		/// <inheritdoc/>
		public override Encoding Encoding => Encoding.UTF8;

		/// <inheritdoc/>
		public override void Write(string value)
		{
			this.scriptModel.Print(value);
		}
	}
}
