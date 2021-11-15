using System.Globalization;

namespace EvaluationAPI.Evaluation
{
    /// <summary>
    /// String Rule Expression Reader class.
    /// Contains all methods for performing Rule Expression parse logic.
    /// </summary>
    public class RuleExpressionReader
    {
        #region Private Propertis 
        //Rule Expression to be parsed.
        private string ruleExpression_;

        //Pointer to the next character to be read.
        int currentIndex_;

        //Keeps track of Expression Groups Brackets.
        int bracketCount_;
        #endregion

        #region Public Constructor
        public RuleExpressionReader(string ruleExpression)
        {
            this.ruleExpression_ = ruleExpression;
        }
        #endregion

        #region Public Methods 
        /// <summary>
        /// Gets next Expression.
        /// </summary>
        /// <param name="expression">Next expression.</param>
        /// <param name="suffix">Suffix for next Rule Expression.</param>
        /// <returns>
        /// Returns True if next expression is read. 
        /// Returns False if no more expression to read.
        /// </returns>
        public bool GetNextExpression(out string expression, out string suffix)
        {
            expression = suffix = string.Empty;

            while (currentIndex_ < ruleExpression_.Length && !ExitRead(ruleExpression_[currentIndex_].ToString(CultureInfo.InvariantCulture)))
            {
                expression += ruleExpression_[currentIndex_];
                currentIndex_++;
            }

            while (currentIndex_ < ruleExpression_.Length && ruleExpression_[currentIndex_].ToString(CultureInfo.InvariantCulture) == Constants.GROUP_END_CHAR)
            {
                expression += ruleExpression_[currentIndex_];
                currentIndex_++;
            }

            if (currentIndex_ < ruleExpression_.Length)
                suffix = ruleExpression_.Substring(currentIndex_, 1);

            currentIndex_++;

            return expression.Length > 0;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Checks whether next character signals a break in reading.
        /// </summary>
        /// <param name="c">Next character to check.</param>
        /// <returns>Returns true if next character signals a break in expression.</returns>
        private bool ExitRead(string c)
        {
            if (bracketCount_ == 0 && (c == Constants.RULE_SEPARATOR_AND || c == Constants.RULE_SEPARATOR_OR))
                return true;

            if (c == Constants.GROUP_START_CHAR)
                bracketCount_++;
            else if (c == Constants.GROUP_END_CHAR)
                bracketCount_--;

            return c == Constants.GROUP_END_CHAR && bracketCount_ == 0;
        }
        #endregion
    }
}
