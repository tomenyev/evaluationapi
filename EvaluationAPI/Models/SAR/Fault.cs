using System;

namespace EvaluationAPI.Models
{
    /// <summary>
    /// A fault within a <see cref="ServiceActivityReport"/>.
    /// </summary>
    public class Fault
    {
        #region Public Properties
        /// <summary>
        /// Fault ID.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Source protocol.
        /// </summary>
        /// <remarks>
        /// (1 = J1587; 2 = J1939).
        /// </remarks>
        public int Protocol { get; set; }

        /// <summary>
        /// The Source Address of the component with the fault.
        /// </summary>
        public int Source { get; set; }

        /// <summary>
        /// The Eaton-translated fault code.
        /// </summary>
        public string FaultCode { get; set; }

        /// <summary>
        /// The PID/SID/SPN (Protocol Specific Parameter).
        /// </summary>
        public string Spn { get; set; }

        /// <summary>
        /// The FMI of the fault code.
        /// </summary>
        public int Fmi { get; set; }

        /// <summary>
        /// Timestamp of when the fault reading session started.
        /// </summary>
        public DateTime? SessionDate { get; set; }

        /// <summary>
        /// The position of the vehicle when the fault was detected.
        /// </summary>
        public decimal? Latitude { get; set; }

        /// <summary>
        /// The position of the vehicle when the fault was detected.
        /// </summary>
        public decimal? Longitude { get; set; }

        /// <summary>
        /// True if the fault is active, False otherwise
        /// </summary>
        public bool IsActive { get; set; }
        public byte LampStatus { get; set; }
        #endregion
    }
}
