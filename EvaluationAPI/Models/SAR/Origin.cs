using System.ComponentModel.DataAnnotations;

namespace EvaluationAPI.Models
{
    /// <summary>
    /// Allows tracing of data back to the origin of that data.
    /// </summary>
    public class Origin
    {
        #region Public Properties
        /// <summary>
        /// Origin Type.
        /// </summary>
        /// <remarks>
        /// SR4 = ServiceRanger, OTS = Omnitracs, PPN = PeopleNet, etc.
        /// </remarks>
        [Required(ErrorMessage = "OriginType is null or empty.")]
        public string OriginType { get; set; }
        #endregion
    }
}
