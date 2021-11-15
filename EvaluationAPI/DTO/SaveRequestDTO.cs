using EvaluationAPI.Models;
using EvaluationAPI.Controllers;
using System.Collections.Generic;

namespace EvaluationAPI.DTO
{
    /// <summary>
    /// Data transfer object used in rest api controller <see cref="EvaluationRuleController.Save(SaveRequestDTO)"/>.
    /// </summary>
    public class SaveRequestDTO
    {
        #region Public Properties
        /// <summary>
        /// Evaluation rules to add or update.
        /// </summary>
        public IEnumerable<EvaluationRule> Rules { get; set; }

        /// <summary>
        /// Evaluation rules to delete.
        /// </summary>
        public IEnumerable<EvaluationRule> RulesToDelete { get; set; }
        #endregion
    }
}
