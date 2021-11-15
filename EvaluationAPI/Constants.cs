using System;
using System.Text.RegularExpressions;

namespace EvaluationAPI
{
    public static class Constants
    {
        #region Evaluation Types
        public static readonly string ROOT_EVALUATION = "Root Evaluation";
        public static readonly string EVALUATION_FAILED = "EvaluationFailed";
        #endregion

        #region Result Types
        public enum ResultType
        {
            UNKNOWN = 0,
            /// <summary>Evaluate further using result key</summary>
            EVALUATE = 1,
            /// <summary>Result contains an action plan key</summary>
            ACTION_PLAN = 3
        }
        public static readonly string DEFAULT_RESULT_KEY = "Null";
        #endregion

        #region EvaluationRules Constants
        public static readonly string GROUP_START_CHAR = "(";
        public static readonly string GROUP_END_CHAR = ")";
        public static readonly string RULE_SEPARATOR_AND = "&";
        public static readonly string RULE_SEPARATOR_OR = "|";
        public static readonly string TEMPLATE_RULE_SEPARATOR_AND = "AND";
        public static readonly string TEMPLATE_RULE_SEPARATOR_OR = "OR";
        #endregion

        #region Auth Controller Allowed Origins
        public static readonly string[] AUTH_ALLOWED_ORIGINS = new string[] { "https://localhost:3000", "http://localhost:3000" };
        #endregion

        #region Evaluation Rule Controller Allowed Origins
        public static readonly string[] EVALUATION_RULE_ALLOWED_ORIGINS = new string[] { "https://localhost:3000", "http://localhost:3000" };
        #endregion

        #region Evaluation Controller Allowed Origins
        public static readonly string[] EVALUATION_ALLOWED_ORIGINS = new string[] { "https://localhost:3000", "http://localhost:3000" };
        #endregion

        #region EXCEL Constants
        public static readonly string[] COLUMNS_NAMES = new string[]
        {
            "Priority", "Prefix", "Suffix", "OriginType",
            "Source Address", "IsEaton", "ProductFamilyId", "ProductCode Source",
            "Source", "Fault Code", "SPN", "FMI", "IsActive",
            "Primary Fault", "Result Type", "Result Key"
        };

        public static readonly string TEMPLATE_PATH = "..\\EvaluationAPI\\Excel\\Spreadsheets\\Empty Import Spreadsheet.xlsm";
        public static readonly string TEMPLATE_WORKSHEET_NAME = "Empty Evaluation";
        #endregion

        #region AuthController Errors
        public static readonly string USER_ALREADY_EXISTS_ERROR = "User already exists!";
        public static readonly string USER_CREATION_FAILED = "User creation failed! Please check user details and try again.";
        #endregion

        #region Evaluation Errors
        public static readonly string EVALUATION_KEY_IS_NULL_OR_EMPTY = "Evaluation Key is null or empty";
        public static readonly string EATON_COMPONENT_NAME = "eaton";
        public static readonly string INVALID_SAR_ERROR = "Null or invalid SAR";
        public static readonly string NO_ORIGIN_INFO_ERROR = "Null or no Origin information in SAR";
        public static readonly string VEHICLE_IS_NULL_ERROR = "Vehicle is null.";
        public static readonly string VEHICLE_HAS_NO_COMPONENTS_ERROR = "Vehicle has no component defined.";
        public static readonly string INVALID_COMPONENT_DATA_ERROR = "Invalid component data.";
        public static readonly string NO_FAULTS_TO_EVALUATE_ERROR = "Request contains no faults to evaluate.";
        public static readonly string INVALID_FAULT_DATA_ERROR = "Invalid fault data.";
        public static readonly string NO_EATON_COMPONENTS_ERROR = "Request contains no Eaton components.";
        #endregion

        #region EvaluationRuleService Errors
        public static readonly string OUT_OF_DATE_ERROR = "Unable to save changes. Your data are out of date.";
        public static readonly string DATA_ARE_EMPTY_ERROR = "Data are empty.";
        #endregion

        #region Import/Export Errors
        public static readonly Func<string, string> WORKSHEET_WAS_NOT_FOUND_ERROR = sheetName => $"Worksheet {sheetName} hasn't been found.";
        public static readonly string INVALID_WORKSHEET_FORMAT_ERROR = "Invalid worksheet format.";
        public static readonly string RULE_GROUP_ALREADY_EXISTS_ERROR = "Unable to import. The RuleGroup already exists.";
        public static readonly string SHEET_NAME_AND_RESULT_KEY_ERROR = "Sheetname has to be the same as Result key.";
        public static readonly string RESULT_KEY_SAME_AS_RULE_GROUP_ERROR = "Rules RuleGroup has to be the same as ResultKey.";
        public static readonly string SINGLE_EVALUATION_TO_SINGLE_EVALUATION_ERROR = "Unable to link single evaluation rules to single evaluation rules.";
        public static readonly string MULTIPLE_EVALUATION_ERROR = "Unable to link evaluation rules to multiple evaluation rules.";
        public static readonly string ROOT_TO_MULTIPLE_EVALUATION_ERROR = "Unable to link multiple evaluation rules to Root Evaluation.";
        #endregion

        #region Rules Validation Errors
        public static readonly string DUPLICATE_ERROR = "Rule duplication error has occurred.";
        public static readonly string RULE_GROUP_AND_RESULT_KEY_ERROR = "ResultKey must not be the same as RuleGroup.";
        public static readonly string ILLEGAL_COMPLEX_RULE_FORMAT_ERROR = "Illegal complex rule format.";

        #region Rule Errors
        public static readonly string INVALID_PREFIX_ERROR = "Prefix has an unsupported format.";
        public static readonly string INVALID_SUFFIX_ERROR = "Suffix has an unsupported format.";
        public static readonly Func<string, int, int, string> OUTSIDE_BOUNDS_ERROR = (name, max, min) => $"{name} number must be between {max} and {min}.";
        public static readonly Func<string, int, string> MAX_CHARACRERS_ERROR = (name, max) => $"{name} must have no more than {max} characters.";
        public static readonly Func<string, string> EMPTY_ERROR = name => $"{name} is empty.";
        public static readonly string INVALID_CHARACTERS_ERROR = "Illegal characters have been detected.";
        #endregion

        #region Single Rule Errors
        public static readonly string SINGLE_RULE_NOT_EMPTY_PREFIX_AND_SUFFIX_ERROR = "Single rule Prefix and Suffix must be empty or null.";
        #endregion

        #region Multiple Rule Errors
        public static readonly string MULTIPLE_RULE_PREFIX_AND_SUFFIX_ARE_EMPTY_ERROR = "Multiple rule Suffix or Prefix must not be empty or null.";
        public static readonly Func<string, string> MULTIPLE_RULE_ILLEGAL_EXPRESSION_ERROR = expr => $"Illegal multiple rule fromat: <{expr}>.";
        public static readonly string MULTIPLE_RULE_INVALID_RESULT_TYPE = "Multiple rule ResultType must be the Action Plan.";
        public static readonly string MULTIPLE_RULE_EMPTY_PREFIX_ERROR = "Prefix mustn't be empty or null while Suffix is not.";
        public static readonly string MULTIPLE_RULE_NOT_EMPTY_COMPONENT_ERROR = "Multiple rule must not have any Component data.";
        public static readonly string MULTIPLE_RULE_EMPTY_FAULT_ERROR = "Multiple rule must have some Fault data.";
        #endregion
        #endregion

        #region Regex
        public static readonly Regex isGroupStartChar = new Regex(@"^\(+$");
        public static readonly Regex isGroupEndChar = new Regex(@"^\)+$");
        public static readonly Regex isAnd = new Regex(@"^and$", RegexOptions.IgnoreCase);
        public static readonly Regex isOr = new Regex(@"^or$", RegexOptions.IgnoreCase);
        public static readonly Regex isAndOrOr = new Regex(@"^or$|^and$", RegexOptions.IgnoreCase);
        public static readonly Regex isAndOrGroupStartChar = new Regex(@"^\(+$|^and$", RegexOptions.IgnoreCase);
        public static readonly Regex isGroupStartOrEndChar = new Regex(@"^\(+$|^\)+$");
        public static readonly Regex isValidPrefix = new Regex(@"^\(+$|^and$|^or$|^()$", RegexOptions.IgnoreCase);
        public static readonly Regex isValidSuffix = new Regex(@"^\)+$|^()$");
        public static readonly Regex isValidOriginType = new Regex(@"^[a-z0-9]+$|^()$", RegexOptions.IgnoreCase);
        #endregion

        #region MAX, MIN
        public static readonly int MAX_RESULT_KEY = 50;
        public static readonly int MAX_RULE_GROUP = 50;
        public static readonly int MAX_SPN = 6;
        public static readonly int MAX_FMI = 255;
        public static readonly int MIN_FMI = 0;
        public static readonly int MAX_FAULT_CODE = 255;
        public static readonly int MIN_FAULT_CODE = 0;
        public static readonly int MAX_COMPONENT_SOURCE_ADDRESS = 255;
        public static readonly int MIN_COMPONENT_SOURCE_ADDRESS = 0;
        public static readonly int MAX_ORIGIN_TYPE = 3;
        public static readonly int MAX_PREFIX = 30;
        public static readonly int MAX_SUFFIX = 30;
        #endregion
    }
}
