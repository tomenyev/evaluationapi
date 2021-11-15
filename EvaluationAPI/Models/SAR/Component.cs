using System.ComponentModel.DataAnnotations;

namespace EvaluationAPI.Models
{
    /// <summary>
    /// Component detected while writing a <see cref="ServiceActivityReport"/>.
    /// </summary>
    public class Component
    {
        #region Public Properties
        /// <summary>
        /// Source protocol. 
        /// </summary>
        /// <remarks>
        /// (1 = J1587; 2 = J1939).
        /// </remarks>
        public int Protocol { get; set; }

        /// <summary>
        /// The source address of the component.
        /// </summary>
        public int SourceAddress { get; set; }

        /// <summary>
        /// Component make if ProductFamilyId is unknown.
        /// </summary>
        /// <remarks>
        /// Nullable.
        /// </remarks>
        [Required(ErrorMessage = "Make is null or empty.")]
        public string Make { get; set; }

        /// <summary>
        /// Component model if ProductFamilyId is unknown.
        /// </summary>
        /// <remarks>
        /// Nullable.
        /// </remarks>
        [Required(ErrorMessage = "Model is null or empty.")]
        public string Model { get; set; }

        /// <summary>
        /// Component serial number if ProductFamilyId is unknown.
        /// </summary>
        /// <remarks>
        /// Nullable.
        /// </remarks>
        public string SerialNumber { get; set; }

        /// <summary>
        /// Flag to indicate if it is an Eaton Component.
        /// </summary>
        /// <remarks>
        /// When this is true then ProductFamilyId and ProductCode should also be present.
        /// </remarks>
        public bool? IsEaton { get; set; }

        /// <summary>
        /// Eaton Product Family ID.
        /// </summary>
        public int? ProductFamilyId { get; set; }

        /// <summary>
        /// Eaton Product Code.
        /// </summary>
        public int? ProductCode { get; set; }
        #endregion
    }
}
