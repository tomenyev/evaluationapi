using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EvaluationAPI.Models
{
    /// <summary>
    /// Service Activity Report (SAR) Model for Evaluation.
    /// </summary>
    public class ServiceActivityReport
    {
        #region Public Properties
        /// <summary>
        /// Report ID.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Report origin info.
        /// </summary>
        [Required(ErrorMessage = "Origin is null.")]
        public Origin Origin { get; set; }

        /// <summary>
        /// Vehicle info.
        /// </summary>
        public Vehicle Vehicle { get; set; }

        /// <summary>
        /// List or faults
        /// </summary>
        public List<Fault> Faults { get; set; } = new List<Fault>();

        /// <summary>
        /// Location info.
        /// </summary>
        public Location Location { get; set; }
        #endregion
    }
}
