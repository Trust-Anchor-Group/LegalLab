﻿using LegalLab.Models.Design;
using System;
using System.Windows.Controls;
using Waher.Networking.XMPP.Contracts;

namespace LegalLab.Models.Legal.Items.Parameters
{
	/// <summary>
	/// Contains information about a string parameter
	/// </summary>
	public class StringParameterInfo : ParameterInfo
	{
		/// <summary>
		/// Contains information about a string parameter
		/// </summary>
		/// <param name="Contract">Contract hosting the parameter</param>
		/// <param name="Parameter">Parameter</param>
		/// <param name="Control">Edit control</param>
		/// <param name="DesignModel">Design model</param>
		public StringParameterInfo(Contract Contract, StringParameter Parameter, Control Control, DesignModel DesignModel) 
			: base(Contract, Parameter, Control, DesignModel)
		{
		}
	}
}
