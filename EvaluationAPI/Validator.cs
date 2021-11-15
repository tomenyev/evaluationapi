using EvaluationAPI.DTO;
using EvaluationAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EvaluationAPI
{
    /// <summary>
    /// Evaluation rules validator.
    /// </summary>
    public class Validator
    {
        #region Public Methods
        /// <summary>
        /// Validate rules.
        /// </summary>
        /// <param name="rules">Rules to validate.</param>
        /// <returns>
        /// Returns not empty list of <see cref="ErrorDTO"/> if invalid.
        /// Returns empty list of <see cref="ErrorDTO"/> if valid.
        /// </returns>
        public static List<ErrorDTO> ValidateRules(IEnumerable<EvaluationRule> rules)
        {
            EvaluationRule rule = rules.FirstOrDefault();
            if (rule == null)
                return null;

            bool multiple = IsMultiple(rule);

            List<ErrorDTO> errors = new List<ErrorDTO>();

            if (multiple)
            {
                errors.AddRange(ValidateMultipleRules(rules));
            }
            else
            {
                errors.AddRange(ValidateSingleRules(rules));
            }

            return errors;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Validate not complex rules.
        /// </summary>
        /// <param name="rules">Rules to validate.</param>
        /// <returns>
        /// Returns not empty list of <see cref="ErrorDTO"/> if invalid.
        /// Returns empty list of <see cref="ErrorDTO"/> if valid.
        /// </returns>
        private static List<ErrorDTO> ValidateSingleRules(IEnumerable<EvaluationRule> rules)
        {
            List<ErrorDTO> errors = new List<ErrorDTO>();
            EvaluationRule firstRule = rules.FirstOrDefault();
            if (firstRule == null)
                return errors;

            var map = new Dictionary<int, KeyValuePair<string, string>>();

            bool isRoot = firstRule.RuleGroup?.Equals(Constants.ROOT_EVALUATION) ?? false;

            foreach (EvaluationRule rule in rules)
            {
                if (isRoot)
                {
                    int key = GenerateComponentHash(rule);
                    
                    if (map.ContainsKey(key))
                    {
                        if (map[key].Value == rule.OriginType)
                            errors.Add(new ErrorDTO(map[key].Key + ", " + rule.Priority, Constants.DUPLICATE_ERROR));
                    }
                    else
                        map.Add(key, new KeyValuePair<string, string>(rule.Priority?.ToString() ?? string.Empty, rule.OriginType));
                } 
                else
                {
                    int key =  GenerateFaultHash(rule);
                    if (map.ContainsKey(key))
                        errors.Add(new ErrorDTO(map[key].Key + ", " + rule.Priority, Constants.DUPLICATE_ERROR));
                    else
                        map.Add(key, new KeyValuePair<string, string>(rule.Priority?.ToString() ?? string.Empty, rule.OriginType));
                }
                errors.AddRange(ValidateSingleRule(rule));
            }
            return errors;
        }

        /// <summary>
        /// Validate complex rules.
        /// </summary>
        /// <param name="rules">Rules to validate.</param>
        /// <returns>
        /// Returns not empty list of <see cref="ErrorDTO"/> if invalid.
        /// Returns empty list of <see cref="ErrorDTO"/> if valid.
        /// </returns>
        private static List<ErrorDTO> ValidateMultipleRules(IEnumerable<EvaluationRule> rules)
        {
            List<ErrorDTO> errors = new List<ErrorDTO>();

            int i = -1;
            int count = 0;
            string strExpression = string.Empty;
            bool validExpression = true;

            List<KeyValuePair<int, List<List<int>>>> expressions = new List<KeyValuePair<int, List<List<int>>>>();
            KeyValuePair<int, List<List<int>>> expression = new KeyValuePair<int, List<List<int>>>();
            List<List<int>> expr = new List<List<int>>();
            List<int> e = new List<int>();


            foreach (EvaluationRule rule in rules)
            {
                bool start = IsStartRule(rule);
                if (start)
                {
                    start = false;
                    if (count != 0 || !validExpression)
                    {
                        strExpression = Regex.Replace(
                                strExpression,
                                Constants.TEMPLATE_RULE_SEPARATOR_AND,
                                Constants.RULE_SEPARATOR_AND,
                                RegexOptions.IgnoreCase
                            );
                        strExpression = Regex.Replace(
                                strExpression,
                                Constants.TEMPLATE_RULE_SEPARATOR_OR,
                                Constants.RULE_SEPARATOR_OR,
                                RegexOptions.IgnoreCase
                            );
                        errors.Add(new ErrorDTO(i.ToString(), Constants.MULTIPLE_RULE_ILLEGAL_EXPRESSION_ERROR(strExpression)));
                    }

                    if (e.Count() != 0)
                    {
                        expr.Add(e);
                        expression = new KeyValuePair<int, List<List<int>>>(i == -1 ? 0 : i, expr);
                        var error = ValidateDuplicates(expression);
                        if (error == null)
                        {
                            errors.AddRange(ValidateExpressions(expressions, expression));
                            expressions.Add(expression);
                        } else
                        {
                            errors.Add(error);
                        }
                        expr = new List<List<int>>();
                        e = new List<int>();
                    }
                    e.Add(GenerateFaultHash(rule));

                    i++;
                    count = 0;
                    validExpression = true;
                    strExpression = string.Empty;
                }

                int x = 0;
                int y = 0;

                if (rule.Prefix?.Contains(Constants.GROUP_START_CHAR) ?? false)
                    x = rule.Prefix.Length;
                if (x > 0 && !string.IsNullOrEmpty(rule.Suffix))
                    validExpression = false;
                if (rule.Suffix?.Contains(Constants.GROUP_END_CHAR) ?? false)
                    y = rule.Suffix.Length;
                if (y > 0 && !Constants.isAndOrOr.IsMatch(rule.Prefix))
                    validExpression = false;

                if (x > 0)
                {
                    count += x;
                    strExpression += rule.Prefix;
                    strExpression += "R" + rule.Priority;
                }
                else if (y > 0)
                {
                    count -= y;
                    strExpression += rule.Prefix;
                    strExpression += "R" + rule.Priority;
                    strExpression += rule.Suffix;
                }
                else
                {
                    strExpression += rule.Prefix;
                    strExpression += "R" + rule.Priority;
                    strExpression += rule.Suffix;
                }

                errors.AddRange(ValidateMultipleRule(rule, GetErrorId(i, rule.Priority ?? 0)));

                if (Constants.isAnd.IsMatch(rule.Prefix))
                {
                    e.Add(GenerateFaultHash(rule));
                }
                else if (Constants.isOr.IsMatch(rule.Prefix))
                {
                    expr.Add(e);
                    e = new List<int>();
                    e.Add(GenerateFaultHash(rule));

                }
            }
            if (count != 0 || !validExpression)
            {
                strExpression = Regex.Replace(
                        strExpression,
                        Constants.TEMPLATE_RULE_SEPARATOR_AND,
                        Constants.RULE_SEPARATOR_AND,
                        RegexOptions.IgnoreCase
                    );
                strExpression = Regex.Replace(
                        strExpression,
                        Constants.TEMPLATE_RULE_SEPARATOR_OR,
                        Constants.RULE_SEPARATOR_OR,
                        RegexOptions.IgnoreCase
                    );
                errors.Add(new ErrorDTO((i == -1 ? 0 : i).ToString(), Constants.MULTIPLE_RULE_ILLEGAL_EXPRESSION_ERROR(strExpression)));
            }
            if (e.Count() != 0)
            {
                expr.Add(e);
                expression = new KeyValuePair<int, List<List<int>>>(i, expr);
                var error = ValidateDuplicates(expression);
                if (error == null)
                    errors.AddRange(ValidateExpressions(expressions, expression));
                else
                    errors.Insert(0, error);
            }

            return errors;
        }

        /// <summary>
        /// Validate not complex evaluation rule.
        /// </summary>
        /// <param name="rule">Evaluation rule to validate.</param>
        /// <returns>
        /// Returns not empty list of <see cref="ErrorDTO"/> if invalid.
        /// Returns empty list of <see cref="ErrorDTO"/> if valid.
        /// </returns>
        private static List<ErrorDTO> ValidateSingleRule(EvaluationRule rule)
        {
            if (rule == null)
                return new List<ErrorDTO>();

            string id = string.Empty + rule.Priority;
            List<ErrorDTO> errors = new List<ErrorDTO>();

            errors.AddRange(ValidateRule(rule, id));

            errors.AddRange(ValidateRuleGroupAndResultKey(rule, id));

            if (rule.RuleGroup?.Equals(Constants.ROOT_EVALUATION) ?? false)
            {
                if (!rule.ComponentSourceAddress.HasValue && !rule.ProductCode.HasValue && !rule.ProductFamilyId.HasValue
                    && rule.ResultType != (byte) Constants.ResultType.ACTION_PLAN)
                    errors.Add(new ErrorDTO(id, Constants.EMPTY_ERROR("Component")));
            }
            else
            {
                if (!rule.FaultSourceAddress.HasValue && !rule.FaultCode.HasValue && !rule.Fmi.HasValue &&
                    string.IsNullOrEmpty(rule.Spn) && rule.ResultType != (byte)Constants.ResultType.EVALUATE)
                    errors.Add(new ErrorDTO(id, Constants.EMPTY_ERROR("Fault")));
            }


            if (!(string.IsNullOrEmpty(rule.Prefix) && string.IsNullOrEmpty(rule.Suffix)))
                errors.Add(new ErrorDTO(id, Constants.SINGLE_RULE_NOT_EMPTY_PREFIX_AND_SUFFIX_ERROR));

            return errors;
        }

        /// <summary>
        /// Validate complex evaluation rule.
        /// </summary>
        /// <param name="rule">Evaluation rule to validate.</param>
        /// <param name="id">Error Id.</param>
        /// <returns>
        /// Returns not empty list of <see cref="ErrorDTO"/> if invalid.
        /// Returns empty list of <see cref="ErrorDTO"/> if valid.
        /// </returns>
        private static List<ErrorDTO> ValidateMultipleRule(EvaluationRule rule, string id)
        {
            List<ErrorDTO> errors = new List<ErrorDTO>();

            if (rule == null)
                return errors;

            errors.AddRange(ValidateRule(rule, id));

            errors.AddRange(ValidatePrefixAndSuffix(rule, id));
            errors.AddRange(ValidateFault(rule, id));
            errors.AddRange(ValidateRuleGroupAndResultKey(rule, id));

            if (rule.ResultType != (byte)Constants.ResultType.ACTION_PLAN)
                errors.Add(new ErrorDTO(id, Constants.MULTIPLE_RULE_INVALID_RESULT_TYPE));

            if (!(!rule.ComponentSourceAddress.HasValue && !rule.ProductCode.HasValue && !rule.ProductFamilyId.HasValue))
                errors.Add(new ErrorDTO(id, Constants.MULTIPLE_RULE_NOT_EMPTY_COMPONENT_ERROR));

            return errors;
        }

        /// <summary>
        /// Validate evaluation rule.
        /// </summary>
        /// <param name="rule">Rule to validate.</param>
        /// <param name="id">Error Id.</param>
        /// <returns>
        /// Returns not empty list of <see cref="ErrorDTO"/> if invalid.
        /// Returns empty list of <see cref="ErrorDTO"/> if valid.
        /// </returns>
        private static List<ErrorDTO> ValidateRule(EvaluationRule rule, string id)
        {
            List<ErrorDTO> errors = new List<ErrorDTO>();

            if (rule == null)
                return errors;

            errors.AddRange(ValidatePrefix(rule.Prefix, id));
            errors.AddRange(ValidateSuffix(rule.Suffix, id));
            errors.AddRange(ValidateOriginType(rule.OriginType, id));
            errors.AddRange(ValidateComponentSourceAddress(rule.ComponentSourceAddress, id));
            errors.AddRange(ValidateSpn(rule.Spn, id));
            errors.AddRange(ValidateFmi(rule.Fmi, id));
            errors.AddRange(ValidateRuleGroup(rule.RuleGroup, id));
            errors.AddRange(ValidateResultKey(rule.ResultKey, id));

            return errors;
        }

        #region Complex Validation
        /// <summary>
        /// Validate prefix and suffix possible cases.
        /// </summary>
        /// <param name="rule">Rule to validate.</param>
        /// <param name="id">Error Id.</param>
        /// <returns>
        /// Returns not empty list of <see cref="ErrorDTO"/> if invalid.
        /// Returns empty list of <see cref="ErrorDTO"/> if valid.
        /// </returns>
        private static List<ErrorDTO> ValidatePrefixAndSuffix(EvaluationRule rule, string id)
        {
            List<ErrorDTO> errors = new List<ErrorDTO>();

            if (rule == null)
                return errors;

            string prefix = rule.Prefix ?? string.Empty;
            string suffix = rule.Suffix ?? string.Empty;

            if (string.IsNullOrEmpty(suffix) && string.IsNullOrEmpty(prefix))
                errors.Add(new ErrorDTO(id, Constants.MULTIPLE_RULE_PREFIX_AND_SUFFIX_ARE_EMPTY_ERROR));
            else if (Constants.isGroupStartOrEndChar.IsMatch(prefix) && Constants.isGroupStartOrEndChar.IsMatch(suffix))
                errors.Add(new ErrorDTO(id, Constants.ILLEGAL_COMPLEX_RULE_FORMAT_ERROR));
            else if (Constants.isAndOrOr.IsMatch(prefix) && Constants.isAndOrOr.IsMatch(suffix))
                errors.Add(new ErrorDTO(id, Constants.ILLEGAL_COMPLEX_RULE_FORMAT_ERROR));
            else if (string.IsNullOrEmpty(prefix) && !string.IsNullOrEmpty(suffix))
                errors.Add(new ErrorDTO(id, Constants.MULTIPLE_RULE_EMPTY_PREFIX_ERROR));

            return errors;
        }

        /// <summary>
        /// Validate fault info.
        /// </summary>
        /// <param name="rule">Rule to validate.</param>
        /// <param name="id">Error Id.</param>
        /// <returns>
        /// Returns not empty list of <see cref="ErrorDTO"/> if invalid.
        /// Returns empty list of <see cref="ErrorDTO"/> if valid.
        /// </returns>
        private static List<ErrorDTO> ValidateFault(EvaluationRule rule, string id)
        {
            List<ErrorDTO> errors = new List<ErrorDTO>();

            if (rule == null)
                return errors;

            if (!rule.FaultSourceAddress.HasValue && !rule.FaultCode.HasValue && !rule.Fmi.HasValue && string.IsNullOrEmpty(rule.Spn))
                errors.Add(new ErrorDTO(id, Constants.EMPTY_ERROR("Fault")));

            return errors;
        }

        /// <summary>
        /// Validate component info.
        /// </summary>
        /// <param name="rule">Rule to validate.</param>
        /// <param name="id">Error Id.</param>
        /// <returns>
        /// Returns not empty list of <see cref="ErrorDTO"/> if invalid.
        /// Returns empty list of <see cref="ErrorDTO"/> if valid.
        /// </returns>
        private static List<ErrorDTO> ValidateComponent(EvaluationRule rule, string id)
        {
            List<ErrorDTO> errors = new List<ErrorDTO>();

            if (rule == null)
                return errors;

            if (!rule.ComponentSourceAddress.HasValue && !rule.ProductCode.HasValue && !rule.ProductFamilyId.HasValue)
                errors.Add(new ErrorDTO(id, Constants.EMPTY_ERROR("Component")));

            return errors;
        }

        /// <summary>
        /// Validate RuleGroup and ResultKey possible cases.
        /// </summary>
        /// <param name="rule">Rule to validate.</param>
        /// <param name="id">Error Id.</param>
        /// <returns>
        /// Returns not empty list of <see cref="ErrorDTO"/> if invalid.
        /// Returns empty list of <see cref="ErrorDTO"/> if valid.
        /// </returns>
        private static List<ErrorDTO> ValidateRuleGroupAndResultKey(EvaluationRule rule, string id)
        {
            List<ErrorDTO> errors = new List<ErrorDTO>();

            if (rule == null)
                return errors;

            if (rule.ResultKey == rule.RuleGroup && rule.ResultType == (byte)Constants.ResultType.EVALUATE)
                errors.Add(new ErrorDTO(id, Constants.RULE_GROUP_AND_RESULT_KEY_ERROR));

            return errors;
        }
        #endregion

        #region Simple Validation
        /// <summary>
        /// Validate Prefix value.
        /// </summary>
        /// <param name="value">Prefix value to be validated.</param>
        /// <param name="id">Error Id.</param>
        /// <returns>
        /// Returns not empty list of <see cref="ErrorDTO"/> if invalid.
        /// Returns empty list of <see cref="ErrorDTO"/> if valid.
        /// </returns>
        private static List<ErrorDTO> ValidatePrefix(string value, string id)
        {
            List<ErrorDTO> errors = new List<ErrorDTO>();

            if (string.IsNullOrEmpty(value))
                return errors;

            if (!Constants.isValidPrefix.IsMatch(value))
                errors.Add(new ErrorDTO(id, Constants.INVALID_PREFIX_ERROR));
            else if (value.Length > Constants.MAX_PREFIX)
                errors.Add(new ErrorDTO(id, Constants.MAX_CHARACRERS_ERROR("Prefix", Constants.MAX_PREFIX)));

            return errors;
        }

        /// <summary>
        /// Validate Suffix value.
        /// </summary>
        /// <param name="value">Suffix value to be validated.</param>
        /// <param name="id">Error Id.</param>
        /// <returns>
        /// Returns not empty list of <see cref="ErrorDTO"/> if invalid.
        /// Returns empty list of <see cref="ErrorDTO"/> if valid.
        /// </returns>
        private static List<ErrorDTO> ValidateSuffix(string value, string id)
        {
            List<ErrorDTO> errors = new List<ErrorDTO>();

            if (string.IsNullOrEmpty(value))
                return errors;

            if (!Constants.isValidSuffix.IsMatch(value))
                errors.Add(new ErrorDTO(id, Constants.INVALID_SUFFIX_ERROR));
            else if (value.Length > Constants.MAX_SUFFIX)
                errors.Add(new ErrorDTO(id, Constants.MAX_CHARACRERS_ERROR("Suffix", Constants.MAX_SUFFIX)));

            return errors;
        }

        /// <summary>
        /// Validate Origin Type value.
        /// </summary>
        /// <param name="value">Origin Type value.</param>
        /// <param name="id">Error Id.</param>
        /// <returns>
        /// Returns not empty list of <see cref="ErrorDTO"/> if invalid.
        /// Returns empty list of <see cref="ErrorDTO"/> if valid.
        /// </returns>
        private static List<ErrorDTO> ValidateOriginType(string value, string id)
        {
            List<ErrorDTO> errors = new List<ErrorDTO>();

            if (string.IsNullOrEmpty(value))
                return errors;

            if (!Constants.isValidOriginType.IsMatch(value))
                errors.Add(new ErrorDTO(id, Constants.INVALID_CHARACTERS_ERROR));
            else if (value.Length > Constants.MAX_ORIGIN_TYPE)
                errors.Add(new ErrorDTO(id, Constants.MAX_CHARACRERS_ERROR("OriginType", Constants.MAX_ORIGIN_TYPE)));

            return errors;
        }

        /// <summary>
        /// Validate ComponentSourceAddress value.
        /// </summary>
        /// <param name="value">ComponentSourceAddress value to be validated.</param>
        /// <param name="id">Error Id.</param>
        /// <returns>
        /// Returns not empty list of <see cref="ErrorDTO"/> if invalid.
        /// Returns empty list of <see cref="ErrorDTO"/> if valid.
        /// </returns>
        private static List<ErrorDTO> ValidateComponentSourceAddress(byte? value, string id)
        {
            List<ErrorDTO> errors = new List<ErrorDTO>();

            if (value == null)
                return errors;

            if (value < Constants.MIN_COMPONENT_SOURCE_ADDRESS || value > Constants.MAX_COMPONENT_SOURCE_ADDRESS)
                errors.Add(new ErrorDTO(id,
                    Constants.OUTSIDE_BOUNDS_ERROR(
                        "ComponentSourceAddress",
                        Constants.MAX_COMPONENT_SOURCE_ADDRESS,
                        Constants.MIN_COMPONENT_SOURCE_ADDRESS
                    )));

            return errors;
        }

        /// <summary>
        /// Validate Spn value.
        /// </summary>
        /// <param name="value">Spn value to be validated.</param>
        /// <param name="id">Error Id.</param>
        /// <returns>
        /// Returns not empty list of <see cref="ErrorDTO"/> if invalid.
        /// Returns empty list of <see cref="ErrorDTO"/> if valid.
        /// </returns>
        private static List<ErrorDTO> ValidateSpn(string value, string id)
        {
            List<ErrorDTO> errors = new List<ErrorDTO>();

            if (string.IsNullOrEmpty(value))
                return errors;

            if (value.Length > Constants.MAX_SPN)
                errors.Add(new ErrorDTO(id, Constants.MAX_CHARACRERS_ERROR("SPN", Constants.MAX_SPN)));

            return errors;
        }

        /// <summary>
        /// Validate Fmi value.
        /// </summary>
        /// <param name="value">Fmi value to be validated.</param>
        /// <param name="id">Error Id.</param>
        /// <returns>
        /// Returns not empty list of <see cref="ErrorDTO"/> if invalid.
        /// Returns empty list of <see cref="ErrorDTO"/> if valid.
        /// </returns>
        private static List<ErrorDTO> ValidateFmi(byte? value, string id)
        {
            List<ErrorDTO> errors = new List<ErrorDTO>();

            if (value == null)
                return errors;

            if (value < Constants.MIN_FMI || value > Constants.MAX_FMI)
                errors.Add(new ErrorDTO(id, Constants.OUTSIDE_BOUNDS_ERROR("FMI", Constants.MAX_FMI, Constants.MIN_FMI)));

            return errors;
        }

        /// <summary>
        /// Validate RuleGroup value.
        /// </summary>
        /// <param name="value">RuleGroup value to be validated.</param>
        /// <param name="id">Error Id.</param>
        /// <returns>
        /// Returns not empty list of <see cref="ErrorDTO"/> if invalid.
        /// Returns empty list of <see cref="ErrorDTO"/> if valid.
        /// </returns>
        private static List<ErrorDTO> ValidateRuleGroup(string value, string id)
        {
            List<ErrorDTO> errors = new List<ErrorDTO>();

            if (string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value))
                errors.Add(new ErrorDTO(id, Constants.EMPTY_ERROR("RuleGroup")));
            else if (value.Length > Constants.MAX_RULE_GROUP)
                errors.Add(new ErrorDTO(id, Constants.MAX_CHARACRERS_ERROR("RuleGroup", Constants.MAX_RULE_GROUP)));

            return errors;
        }

        /// <summary>
        /// Validate ResultKey value.
        /// </summary>
        /// <param name="value">ResultKey value to be validated.</param>
        /// <param name="id">Error Id.</param>
        /// <returns>
        /// Returns not empty list of <see cref="ErrorDTO"/> if invalid.
        /// Returns empty list of <see cref="ErrorDTO"/> if valid.
        /// </returns>
        private static List<ErrorDTO> ValidateResultKey(string value, string id)
        {
            List<ErrorDTO> errors = new List<ErrorDTO>();

            if (string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value))
                errors.Add(new ErrorDTO(id, Constants.EMPTY_ERROR("ResultKey")));
            else if (value.Length > Constants.MAX_RESULT_KEY)
                errors.Add(new ErrorDTO(id, Constants.MAX_CHARACRERS_ERROR("ResultKey", Constants.MAX_RESULT_KEY)));

            return errors;
        }
        #endregion

        #region Multiple Rules Duplicate Error Validator
        private static IEnumerable<ErrorDTO> ValidateExpressions(List<KeyValuePair<int, List<List<int>>>> expressions, KeyValuePair<int, List<List<int>>> expression)
        {
            List<ErrorDTO> errors = new List<ErrorDTO>();
            foreach (KeyValuePair<int, List<List<int>>> expr in expressions)
            {
                if (CompareExpressions(expr.Value, expression.Value))
                {
                    errors.Add(new ErrorDTO(expr.Key + ", " + expression.Key, Constants.DUPLICATE_ERROR));
                    return errors;
                }

            }
            return errors;
        }

        private static bool CompareExpressions(List<List<int>> expr1, List<List<int>> expr2)
        {
            foreach (List<int> e1 in expr1)
            {
                foreach (List<int> e2 in expr2)
                {
                    if (CompareArrays(e1.ToArray(), e2.ToArray()))
                        return true;
                }
            }
            return false;
        }

        private static bool CompareArrays(int[] e1, int[] e2)
        {
            if (e1.Length != e2.Length)
                return false;

            Array.Sort<int>(e1);
            Array.Sort<int>(e2);

            for (int i = 0; i < e1.Length; i++)
            {
                if (e1[i] != e2[i])
                    return false;
            }

            return true;
        }

        private static int GenerateFaultHash(EvaluationRule rule)
        {
            return new StringBuilder()
                .Append(rule.FaultSourceAddress?.ToString() ?? "0").Append(".")
                .Append(rule.FaultCode?.ToString() ?? "0").Append(".")
                .Append(rule.Spn).Append(".")
                .Append(rule.Fmi?.ToString() ?? "0").Append(".")
                .Append(rule.IsActive?.ToString() ?? "False").ToString().GetHashCode();
        }

        private static int GenerateComponentHash(EvaluationRule rule)
        {
            return new StringBuilder()
                .Append(rule.ComponentSourceAddress?.ToString() ?? "0").Append(".")
                .Append(rule.IsEaton?.ToString() ?? "0").Append(".")
                .Append(rule.ProductFamilyId).Append(".")
                .Append(rule.ProductCode?.ToString() ?? "0").ToString().GetHashCode();
        }
        #endregion

        #region Multiple Rule Duplicate Error Check
        private static ErrorDTO ValidateDuplicates(KeyValuePair<int, List<List<int>>> expression)
        {
            for (int i = 0; i < expression.Value.Count; i++)
            {
                var e1 = ValidateExpressionDuplicate(expression.Value[i], expression.Key);
                if (e1 != null)
                    return e1;
                for (int j = i + 1; j < expression.Value.Count; j++)
                {
                    var e2 = CompareSubArrays(expression.Value[i].ToArray(), expression.Value[j].ToArray());
                    if (e2)
                        return new ErrorDTO(expression.Key.ToString(), Constants.ILLEGAL_COMPLEX_RULE_FORMAT_ERROR);
                }
            }
            return null;
        }

        private static ErrorDTO ValidateExpressionDuplicate(List<int> expression, int priority)
        {
            var map = new Dictionary<int, int>();
            for (int i = 0; i < expression.Count; i++)
            {
                if (map.ContainsKey(expression[i]))
                {
                    return new ErrorDTO(priority.ToString(), message: Constants.DUPLICATE_ERROR);
                }
                else
                {
                    map.Add(expression[i], expression[i]);
                }
            }
            return null;
        }

        private static bool CompareSubArrays(int[] e1, int[] e2)
        {
            for (int i = 0; i < e1.Length; i++)
            {
                for (int j = 0; j < e2.Length; j++)
                {
                    if (e1[i] == e2[j])
                        return true;
                }
            }
            return false;
        }
        #endregion

        #region Utils
        /// <summary>
        /// Check if rule is start rule of complex expression.
        /// </summary>
        /// <param name="rule">Rule to be checked.</param>
        /// <returns>
        /// Returns true if rule is start rule of complex expression.
        /// Returns false if rule is not start rule of complex expression.
        /// </returns>
        private static bool IsStartRule(EvaluationRule rule)
        {
            return Constants.isGroupStartChar.IsMatch(rule.Prefix) && string.IsNullOrEmpty(rule.Suffix);
        }

        /// <summary>
        /// Check if rule is end rule of complex expression.
        /// </summary>
        /// <param name="rule">Rule to be checked.</param>
        /// <returns>
        /// Returns true if rule is end rule of complex expression.
        /// Returns false if rule is not end rule of complex expression.
        /// </returns>
        private static bool IsEndRule(EvaluationRule rule)
        {
            return Constants.isAndOrOr.IsMatch(rule.Prefix) && Constants.isGroupEndChar.IsMatch(rule.Suffix);
        }

        /// <summary>
        /// Generate Error Id.
        /// </summary>
        /// <param name="x">Complex rule priority.</param>
        /// <param name="y">Sub rule priority.</param>
        /// <returns>Returns error id.</returns>
        private static string GetErrorId(int x, int y)
        {
            return string.Empty + x + "." + y;
        }

        /// <summary>
        /// Check if rule is complex.
        /// </summary>
        /// <param name="rule">Rule to be checked.</param>
        /// <returns>
        /// Returns true if rule is complex.
        /// Returns false if rule is not complex.
        /// </returns>
        public static bool IsMultiple(EvaluationRule rule)
        {
            return !string.IsNullOrEmpty(rule.Prefix) || !string.IsNullOrEmpty(rule.Suffix);
        }
        #endregion

        #endregion
    }
}
