using EvaluationAPI.Evaluation;
using EvaluationAPI.Evaluation.Models;
using EvaluationAPI.Models;
using EvaluationAPI.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EvaluationAPI.Controllers;

namespace EvaluationAPI.Services
{
    /// <summary>
    /// Evaluation service layer for <see cref="EvaluationController"/>.
    /// </summary>
    public class EvaluationService
    {
        #region Private Properties
        private EvaluationRulesRepository repository_;

        private EvaluationRuleService service_;

        private RuleExpressionProcessor processor_;
        #endregion

        #region Constructor
        public EvaluationService(EvaluationRulesRepository repository, EvaluationRuleService service)
        {
            repository_ = repository;
            service_ = service;
            processor_ = new RuleExpressionProcessor();

        }
        #endregion

        #region Public Methods - Evaluate
        /// <summary>
        /// Evaluates Service Activity Report.
        /// </summary>
        /// <exception cref="ArgumentException">Throws if <paramref name="key"/> is null or empty.</exception>
        /// <exception cref="Exception">Throws if unhandled error has occurred.</exception>
        /// <param name="sar">Service Activity Report to evaluate.</param>
        /// <param name="key">Evaluation key.</param>
        /// <param name="result">Evaluation result.</param>
        public void Evaluate(ServiceActivityReport sar, string key, ref EvaluationResult result)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException(Constants.EVALUATION_KEY_IS_NULL_OR_EMPTY);

            try
            {
                IEnumerable<EvaluationRule> rules = repository_.GetEvaluationRulesByRuleGroup(key).Result;

                IEnumerable<RuleExpression> expressions = processor_
                    .GetRuleExpressions(rules, sar)
                    .Where(re => re != null)
                    .OrderBy(re => re.GetHighestPriority());

                foreach (RuleExpression expression in expressions)
                {
                    if (!expression.Evaluate())
                        continue;

                    ProcessResult processResult = processor_.GetEvaluationResult(expression);

                    if (processResult?.ResultType == Constants.ResultType.EVALUATE)
                    {
                        Evaluate(sar, processResult.ResultKey, ref result);

                        if (result.EvaluationComplete)
                            break;
                    }
                    else if (processResult?.ResultType == Constants.ResultType.ACTION_PLAN)
                    {
                        result.ActionPlanKey = processResult.ResultKey;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while Processing Rules for '{key}'. {ex.Message}", ex);
            }

            if (!result.EvaluationComplete && key == Constants.ROOT_EVALUATION)
            {
                result.ActionPlanKey = Constants.EVALUATION_FAILED;
                result.EvaluationComplete = false;
            }
        }
        #endregion
    }
}
