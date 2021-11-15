using EvaluationAPI.Controllers;
using EvaluationAPI.Services;
namespace EvaluationAPI.Evaluation.Models
{
    /// <summary>
    /// Represents the result of an evaluation.
    /// </summary>
    public class EvaluationResult
    {
        #region Private Properties
        private string actionPlanKey_;
        #endregion

        #region Public Properties
        /// <summary>
        /// The action plan that was selected for the Service Activity Report, or null if
        /// no action plan is needed.
        /// </summary>
        public string ActionPlanKey
        {
            get { return actionPlanKey_; }
            set
            {
                actionPlanKey_ = value;
                EvaluationComplete = true;
            }
        }

        /// <summary>
        /// Whether or not evaluation has been completed.
        /// </summary>
        public bool EvaluationComplete { get; set; }
        #endregion

    }
}
