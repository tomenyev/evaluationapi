using EvaluationAPI.Evaluation.Models;
using EvaluationAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EvaluationAPI.Evaluation
{
    /// <summary>
    /// Rule Expression Builder class.
    /// Contains all methods for building Expression Tree from a list of rules.
    /// </summary>
    public class RuleExpressionBuilder
    {
        #region Public Methods
        /// <summary>
        /// Builds Expression Tree from a list of rules.
        /// </summary>
        /// <param name="rules">List of rules to be processed.</param>
        /// <returns>
        /// Returns a list of <see cref="RuleExpression"/> in the form of a tree.
        /// </returns>
        public IEnumerable<RuleExpression> Build(IEnumerable<EvaluationRule> rules)
        {
            EvaluationRule[] rulesArray = rules != null ?
                rules.ToArray() :
                new EvaluationRule[0];

            if (rules == null || !rulesArray.Any())
                return new List<RuleExpression>();

            if (rulesArray.Any(r => string.IsNullOrEmpty(r.EId)))
                throw new Exception("ID missing for one or more Rule.");

            string expressionString = BuildExpressionString(rulesArray);

            RuleExpression rootExpression = GetExpression(expressionString, rulesArray);

            return string.IsNullOrEmpty(rootExpression.Expression) ?
                rootExpression.Childrens :
                new List<RuleExpression> { rootExpression };
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Builds a logic expression string from specified rules.
        /// </summary>
        /// <example>
        /// Return exmaple: R1&R2&R3
        /// </example>
        /// <param name="rules">List of rules for processing.</param>
        /// <returns>Returns a logic expression string.</returns>
        private string BuildExpressionString(EvaluationRule[] rules)
        {
            StringBuilder result = new StringBuilder();

            for (int i = 0; i < rules.Count(); i++)
            {
                EvaluationRule rule = rules[i];
                string prefix = FormatRuleSeparators(rule.Prefix);

                if (result.ToString().Length > 0 && !StartsWithOperator(prefix) && !EndsWithOperator(result.ToString()))
                    prefix = Constants.RULE_SEPARATOR_AND + prefix;

                result.Append(prefix);
                result.Append(rule.EId);
                result.Append(FormatRuleSeparators(rule.Suffix));
            }

            return result.ToString();
        }

        /// <summary>
        /// Build a expression node from given expression string.
        /// </summary>
        /// <param name="expressionString">Specified expression string.</param>
        /// <param name="rules">Evaluation rules.</param>
        /// <returns>Returns a new expression node containing zero or more Child Nodes.</returns>
        private RuleExpression GetExpression(string expressionString, EvaluationRule[] rules)
        {
            RuleExpressionReader reader = new RuleExpressionReader(expressionString);

            RuleExpression rootExpression = new RuleExpression { Expression = string.Empty };

            string expressionStr, suffix;

            while (reader.GetNextExpression(out expressionStr, out suffix))
            {
                RuleExpression nextExpression;
                if (expressionStr.StartsWith(Constants.GROUP_START_CHAR) && expressionStr.EndsWith(Constants.GROUP_END_CHAR) && expressionStr.Length > 2)
                    nextExpression = GetExpression(expressionStr.Substring(1, expressionStr.Length - 2), rules);
                else
                {
                    nextExpression = new RuleExpression
                    {
                        Expression = expressionStr,
                        Rule = rules.FirstOrDefault(r => r.EId == expressionStr)
                    };
                    if (nextExpression.Rule == null)
                        throw new Exception("Invalid Rule Prefix/Suffix Format.");

                }
                nextExpression.Suffix = suffix;
                rootExpression.Add(nextExpression);
            }

            return rootExpression;
        }

        /// <summary>
        /// Formats rule separator as single character representation.
        /// </summary>
        /// <remarks>
        /// To simplify parsing.
        /// </remarks>
        /// <param name="opr">Template logic operator.</param>
        /// <returns>Returns formatted logic operator.</returns>
        private string FormatRuleSeparators(string opr) =>
            (opr ?? string.Empty)
                .ToUpper()
                .Replace(Constants.TEMPLATE_RULE_SEPARATOR_OR, Constants.RULE_SEPARATOR_OR)
                .Replace(Constants.TEMPLATE_RULE_SEPARATOR_AND, Constants.RULE_SEPARATOR_AND)
                .Replace(" ", string.Empty);

        /// <summary>
        /// Checks whether expression string ends with rule separator.
        /// </summary>
        /// <param name="str">Rule expression to check.</param>
        /// <returns>Returns true if expression string ends with rule separator.</returns>
        private bool EndsWithOperator(string str) =>
            str.EndsWith(Constants.RULE_SEPARATOR_AND) || str.EndsWith(Constants.RULE_SEPARATOR_OR);

        /// <summary>
        /// Checks whether expression string starts with rule separator.
        /// </summary>
        /// <param name="str">Rule expression to check.</param>
        /// <returns>Returns true if expression string starts with rule separator.</returns>
        private bool StartsWithOperator(string str) =>
            str.StartsWith(Constants.RULE_SEPARATOR_AND) || str.StartsWith(Constants.RULE_SEPARATOR_OR);
        #endregion
    }
}
