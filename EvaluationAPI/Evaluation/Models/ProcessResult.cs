using EvaluationAPI.Services;
using EvaluationAPI.Evaluation;
using EvaluationAPI.Models;

namespace EvaluationAPI.Evaluation.Models
{
    /// <summary>
    /// Storesresult of processing <see cref="ServiceActivityReport"/> against a list of rules.
    /// </summary>
    public class ProcessResult
    {
        #region Public - Properties
        /// <summary>
        /// Result Type.
        /// </summary>
        public Constants.ResultType ResultType { get; set; }

        /// <summary>
        /// Evaluator name or template key.
        /// </summary>
        public string ResultKey { get; set; }
        #endregion
    }
}
