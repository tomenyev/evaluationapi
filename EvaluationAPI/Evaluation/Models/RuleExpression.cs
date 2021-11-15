using EvaluationAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using LinqExp = System.Linq.Expressions;

namespace EvaluationAPI.Evaluation.Models
{
    /// <summary>
    /// Represents a Expression Node in Rule Expression Tree.
    /// </summary>
    public class RuleExpression
    {
        #region Public Properties
        /// <summary>
        /// Children nodes.
        /// </summary>
        public List<RuleExpression> Childrens { get; set; } = new List<RuleExpression>();

        /// <summary>
        /// Rule Expression (Rule ID).
        /// </summary>
        public string Expression { get; set; }

        /// <summary>
        /// Suffix operation.
        /// </summary>
        public string Suffix { get; set; }

        /// <summary>
        /// Associated rule info.
        /// </summary>
        public EvaluationRule Rule { get; set; }
        #endregion

        #region Public Methods
        /// <summary>
        /// Adds a child expression.
        /// </summary>
        /// <param name="c">Child Expression to be added.</param>
        public void Add(RuleExpression c) => this.Childrens.Add(c);

        /// <summary>
        /// Remove a child expression.
        /// </summary>
        /// <param name="c">Child expression to be removed.</param>
        public void Remove(RuleExpression c)
        {
            if (this.Childrens.Contains(c)) this.Childrens.Remove(c);
        }

        /// <summary>
        /// Evaluates the expression tree.
        /// </summary>
        /// <returns>Returns Evaluation Result.</returns>
        public bool Evaluate()
        {
            if (this.Childrens.Count > 0)
            {
                var e0 = LinqExp.Expression.Constant(true, typeof(bool));
                var e1 = LinqExp.Expression.Constant(Childrens[0].Evaluate(), typeof(bool));
                var result = LinqExp.Expression.And(e0, e1);

                for (int i = 1; i < Childrens.Count; i++)
                {
                    if (string.IsNullOrEmpty(Childrens[i - 1].Suffix))
                        throw new Exception(string.Format("Suffix Not available for Expression {0}", Childrens[i - 1].Expression));

                    var eNext = LinqExp.Expression.Constant(Childrens[i].Evaluate(), typeof(bool));

                    if (Childrens[i - 1].Suffix == Constants.RULE_SEPARATOR_AND)
                        result = LinqExp.Expression.And(e1, eNext);
                    else if (Childrens[i - 1].Suffix == Constants.RULE_SEPARATOR_OR)
                        result = LinqExp.Expression.Or(e1, eNext);
                }
                return LinqExp.Expression.Lambda<Func<bool>>(result).Compile()();
            }

            if (this.Rule == null)
                throw new Exception(string.Format("Rule Info Not available for Expression {0}", this.Expression));

            return this.Rule.Evaluate();
        }

        /// <summary>
        /// Get lists of rules associated with Expression Node.
        /// </summary>
        /// <returns>Returns lists of Rules associated with Expression Node.</returns>
        public IEnumerable<EvaluationRule> GetEvaluationRules()
        {
            List<EvaluationRule> rules = new List<EvaluationRule>();

            if (Childrens.Count > 0)
                foreach (RuleExpression re in Childrens)
                    foreach (EvaluationRule cr in re.GetEvaluationRules())
                        rules.Add(cr);
            else if (this.Rule != null)
                rules.Add(this.Rule);

            return rules;
        }

        /// <summary>
        /// Gets Highest priority value(Minimum Value) of all associated rules(Including Decendents).
        /// </summary>
        /// <returns>
        /// Returns highest priority.
        /// </returns>
        public int GetHighestPriority()
        {
            int min = int.MaxValue;
            int newMin;
            if (Childrens.Count > 0 && (newMin = this.Childrens.Min(c => c.GetHighestPriority())) < min)
                min = newMin;
            else if (this.Rule != null && this.Rule.Priority < min)
                min = (int)this.Rule.Priority;

            return min;
        }
        #endregion
    }
}
