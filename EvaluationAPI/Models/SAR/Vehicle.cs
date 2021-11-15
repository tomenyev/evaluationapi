using System.Collections.Generic;

namespace EvaluationAPI.Models
{
    /// <summary>
    /// Vehicle Information of <see cref="ServiceActivityReport"/>.
    /// </summary>
    public class Vehicle
    {
        #region Public Properties
        /// <summary>
        /// Vehicle Id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Components of a vehicle.
        /// </summary>
        public List<Component> Components { get; set; } = new List<Component>();
        #endregion
    }
}
