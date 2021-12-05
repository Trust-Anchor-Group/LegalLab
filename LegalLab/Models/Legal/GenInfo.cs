using System;

namespace LegalLab.Models.Legal
{
    /// <summary>
    /// Contains some general information
    /// </summary>
    public class GenInfo
    {
        /// <summary>
        /// Contains some general information
        /// </summary>
        /// <param name="Name">Name</param>
        /// <param name="Value">Value</param>
        public GenInfo(string Name, string Value)
		{
            this.Name = Name;
            this.Value = Value;
        }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Value
        /// </summary>
        public string Value { get; internal set; }
    }
}
