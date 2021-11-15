using EvaluationAPI.DTO;
using EvaluationAPI.Models;
using System.Collections.Generic;

namespace EvaluationAPI.Exceptions
{
    /// <summary>
    /// Database concurrency exception.
    /// </summary>
    public class ConcurrencyException : IException
    {
        #region Private Properties
        /// <summary>
        /// Lates database rules.
        /// </summary>
        private readonly IEnumerable<EvaluationRule> rules_;
        #endregion

        #region Public Constructors
        public ConcurrencyException(IEnumerable<EvaluationRule> rules, List<ErrorDTO> errors) : base(errors)
        {
            this.rules_ = rules;
        }
        #endregion

        #region Public Methods
        public IEnumerable<EvaluationRule> Rules
        {
            get { return this.rules_; }
        }

        public override Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>() { { "rules", Rules }, { "errors", Errors } };
        }
        #endregion
    }
}
