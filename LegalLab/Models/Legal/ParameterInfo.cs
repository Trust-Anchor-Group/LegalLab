using System;
using System.Windows.Controls;
using Waher.Networking.XMPP.Contracts;

namespace LegalLab.Models.Legal
{
    /// <summary>
    /// Contains information about a parameter.
    /// </summary>
    public class ParameterInfo
    {
        /// <summary>
        /// Contains information about a parameter.
        /// </summary>
        /// <param name="Parameter">Contract parameter.</param>
        /// <param name="Control">Generated control.</param>
        public ParameterInfo(Parameter Parameter, Control Control)
		{
            this.Parameter = Parameter;
            this.Control = Control;
		}

        /// <summary>
        /// Contract parameter.
        /// </summary>
        public Parameter Parameter { get; internal set; }

        /// <summary>
        /// Generated control.
        /// </summary>
        public Control Control { get; internal set; }
    }
}
