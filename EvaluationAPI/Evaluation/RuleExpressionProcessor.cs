using EvaluationAPI.Evaluation.Models;
using EvaluationAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EvaluationAPI.Evaluation
{
    /// <summary>
    /// Rules Processor class.
    /// Contains all methods for processing Evaluation Rules.
    /// </summary>
    public class RuleExpressionProcessor
    {
        #region Private Properties
        private RuleExpressionBuilder ruleExpressionBuilder_;
        #endregion

        #region Public Constructor
        public RuleExpressionProcessor()
        {
            this.ruleExpressionBuilder_ = new RuleExpressionBuilder();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets Evaluation Result information from the Rule Expression.
        /// </summary>
        /// <param name="ruleExpression">Rule Expression that contains evaluation result information.</param>
        /// <returns>Returns <see cref="ProcessResult"/> which stores Evaluation Result.</returns>
        public ProcessResult GetEvaluationResult(RuleExpression ruleExpression)
        {
            ProcessResult result = new ProcessResult();

            IEnumerable<EvaluationRule> expressionRules = ruleExpression
                .GetEvaluationRules();

            EvaluationRule ruleMatch = expressionRules
                .FirstOrDefault(r => (r.ResultType != 1 || r.ResultType != 3) && !string.IsNullOrEmpty(r.ResultKey));

            if (ruleMatch == null)
                throw new Exception($"No Result Type/Key Found for Rule [ {string.Join(",", expressionRules)} ]");

            result.ResultType = ruleMatch.EResultType;
            result.ResultKey = ruleMatch.ResultKey;

            return result;
        }

        /// <summary>
        /// Process Rules and Build Rule Expression that can be evaluated individually.
        /// </summary>
        /// <param name="rules">List of Evaluation Rules.</param>
        /// <param name="sar"><see cref="ServiceActivityReport"/> to be evaluated.</param>
        /// <returns>Returns list of <see cref="RuleExpression"/> which can be evaluated.</returns>
        public IEnumerable<RuleExpression> GetRuleExpressions(IEnumerable<EvaluationRule> rules, ServiceActivityReport sar)
        {
            EvaluationRule[] rulesArray = rules?
                .OrderBy(r => r.Priority)
                .ToArray() ?? new EvaluationRule[0];

            int i = 1;
            foreach (EvaluationRule rule in rulesArray)
            {
                rule.EId = "R" + i;
                rule.Evaluate = () => Evaluate(rule, sar);
                i++;
            }

            return ruleExpressionBuilder_.Build(rulesArray);
        }
        #endregion

        #region Private - Methods
        /// <summary>
        /// Evaluates a <see cref="ServiceActivityReport"/> against a specified rule.
        /// </summary>
        /// <remarks>
        /// Any rule value is only matched if it has a value.
        /// </remarks>
        /// <param name="rule">Specified rule.</param>
        /// <param name="sar"><see cref="ServiceActivityReport"/> to be evaluated.</param>
        /// <returns>
        /// Returns True if match found.
        /// Returns False if match not found.
        /// </returns>
        private static bool Evaluate(EvaluationRule rule, ServiceActivityReport sar)
        {
            bool result = string.IsNullOrEmpty(rule.OriginType) || rule.OriginType == sar.Origin.OriginType;

            if (!result)
                return false;

            bool matchComponent =
                rule.ComponentSourceAddress.HasValue || rule.IsEaton.HasValue ||
                rule.ProductFamilyId.HasValue || rule.ProductCode.HasValue;

            if (matchComponent && sar.Vehicle == null)
                return false;

            if (matchComponent)
            {
                IEnumerable<Component> components = sar.Vehicle.Components.AsEnumerable();

                if (rule.ComponentSourceAddress.HasValue)
                    components = components.Where(c => c.SourceAddress == rule.ComponentSourceAddress);
                if (rule.IsEaton.HasValue)
                    components = components.Where(c => c.IsEaton == rule.IsEaton);
                if (rule.ProductFamilyId.HasValue)
                    components = components.Where(c => c.ProductFamilyId == rule.ProductFamilyId);
                if (rule.ProductCode.HasValue)
                    components = components.Where(c => c.ProductCode == rule.ProductCode);

                result = components.Any();
            }

            if (!result)
                return false;

            bool matchFaults =
                rule.FaultSourceAddress.HasValue || rule.FaultCode.HasValue ||
                !string.IsNullOrEmpty(rule.Spn) || rule.Fmi.HasValue || rule.IsActive.HasValue;

            if (matchFaults)
            {
                IEnumerable<Fault> faults = sar.Faults.AsEnumerable();

                if (rule.FaultSourceAddress.HasValue)
                    faults = faults.Where(f => f.Source == rule.FaultSourceAddress);
                if (rule.FaultCode.HasValue)
                    faults = faults.Where(f => f.FaultCode == rule.FaultCode.ToString());
                if (!string.IsNullOrEmpty(rule.Spn))
                    faults = faults.Where(f => string.Compare(f.Spn, rule.Spn, StringComparison.InvariantCultureIgnoreCase) == 0);
                if (rule.Fmi.HasValue)
                    faults = faults.Where(f => f.Fmi == rule.Fmi);
                if (rule.IsActive.HasValue)
                    faults = faults.Where(f => f.IsActive == rule.IsActive);

                result = faults.Any();
            }

            return result;
        }
        #endregion
    }
}
