using EvaluationAPI.Controllers;

namespace EvaluationAPI.Models
{
    /// <summary>
    /// Data transfer object for rest api. Used in <see cref="EvaluationRuleController.AddRuleGroup(AddRuleGroupDTO)"/>.
    /// </summary>
    public class AddRuleGroupDTO
    {
        #region Public Properties
        /// <summary>
        /// Rule to link new Rule Group.
        /// </summary>
        public EvaluationRule Rule { get; set; }

        /// <summary>
        /// Specify linked Rule Group.
        /// </summary>
        public bool Multiple { get; set; }
        #endregion
    }
}
