using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BombaProMaxWPF.Enums
{
    /// <summary>
    /// Represents the state of the guided "Journée" workflow.
    /// </summary>
    public enum JourneeState
    {
        /// <summary>
        /// No journée is active - normal navigation allowed.
        /// </summary>
        None,

        /// <summary>
        /// Journée is active - user must follow guided workflow.
        /// </summary>
        Active,

        /// <summary>
        /// Journée has been completed.
        /// </summary>
        Finished
    }
}
