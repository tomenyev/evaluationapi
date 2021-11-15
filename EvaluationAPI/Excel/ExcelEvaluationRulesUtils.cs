using ClosedXML.Excel;
using EvaluationAPI.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EvaluationAPI.Excel
{
    /// <summary>
    /// EXEL reader/wirter class.
    /// Contains all methods for converting an EXCEL file into the list of Evaluation Rules and vice versa.
    /// </summary>
    public class ExcelEvaluationRulesUtils
    {
        #region Constructor
        public ExcelEvaluationRulesUtils()
        {

        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Converts an EXCEL file into the list of Evaluation Rules.
        /// </summary>
        /// <exception cref="Exception">
        /// Throws if <paramref name="file"/> is null or empty 
        /// or <paramref name="sheetName"/> is null or empty 
        /// or the worksheet is null or empty or has an invalid format.
        /// </exception>
        /// <param name="file">EXCEL file to be converted.</param>
        /// <param name="sheetName">EXCEL file sheetname to be converted.</param>
        /// <returns>
        /// Returns the list of Evaluation Rules.
        /// </returns>
        public IEnumerable<EvaluationRule> ExcelToEvaluationRules(IFormFile file, string sheetName)
        {
            if (file == null || file.Length == 0 || string.IsNullOrEmpty(sheetName))
                throw new Exception(Constants.DATA_ARE_EMPTY_ERROR);

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            using (var stream = new MemoryStream())
            {
                file.CopyTo(stream);

                using (var workbook = new XLWorkbook(stream))
                {
                    var workSheet = workbook.Worksheet(sheetName);

                    if (workSheet == null || workSheet.IsEmpty())
                        throw new Exception(Constants.WORKSHEET_WAS_NOT_FOUND_ERROR(sheetName));

                    IXLRow[] rowsArray = workSheet.Rows()?.ToArray();
                    int j = 0;
                    IXLRow[] rows = rowsArray?
                        .Where(r =>
                        {
                            if (j++ < 4)
                                return false;
                            return !r.FirstCell().IsEmpty();
                        })
                        .OrderBy(r => r.FirstCell().GetValue<decimal>())
                        .ToArray();

                    IXLRow columns = rowsArray[3];

                    if (rows == null || rowsArray.Length <= 4)
                        throw new Exception(Constants.EMPTY_ERROR("Worksheet"));

                    if (ValidateColumnsNames(columns?.Cells().ToArray()))
                        throw new Exception(Constants.INVALID_WORKSHEET_FORMAT_ERROR);

                    List<EvaluationRule> rules = new List<EvaluationRule>();
                    int checksum = DateTime.Now.GetHashCode();
                    for (int i = 0; i < rows.Length; i++)
                    {
                        EvaluationRule rule = RowToEvaluationRule(rows[i]);
                        rule.RuleGroup = sheetName;
                        rule.RuleGroupChecksum = checksum;
                        rule.Priority = i;
                        rules.Add(rule);
                    }
                    return rules;
                }
            }
        }
        
        /// <summary>
        /// Converts the list of Evaluation Rules to an EXCEL file.
        /// </summary>
        /// <exception cref="Exception">
        /// Throws if <paramref name="rules"/> are empty 
        /// or <paramref name="ruleGroup"/> is empty or null.
        /// </exception>
        /// <param name="rules">Evaluation Rules to be converted.</param>
        /// <param name="ruleGroup">EXEL file sheetname to be created.</param>
        /// <returns>
        /// Returns an EXCEL file as a byte array.
        /// </returns>
        public byte[] EvaluationRulesToExcel(IEnumerable<EvaluationRule> rules, string ruleGroup)
        {
            if (string.IsNullOrEmpty(ruleGroup))
                throw new Exception(Constants.DATA_ARE_EMPTY_ERROR);

            XLWorkbook workbook = new XLWorkbook();

            GetWorksheetTemplate().CopyTo(workbook, ruleGroup);

            IXLWorksheet worksheet = workbook.Worksheet(ruleGroup);

            if (rules == null)
                throw new Exception(Constants.DATA_ARE_EMPTY_ERROR);

            EvaluationRule[] rulesArray = rules.ToArray();

            for (int i = 5; i < rules.Count() + 5; i++)
            {
                worksheet.Cell(i, 1).Value = rulesArray[i - 5].Priority;
                worksheet.Cell(i, 2).Value = rulesArray[i - 5].Prefix;
                worksheet.Cell(i, 3).Value = rulesArray[i - 5].Suffix;
                worksheet.Cell(i, 4).Value = rulesArray[i - 5].OriginType;
                worksheet.Cell(i, 5).Value = rulesArray[i - 5].ComponentSourceAddress;
                worksheet.Cell(i, 6).Value = BoolToCellValue(rulesArray[i - 5].IsEaton);
                worksheet.Cell(i, 7).Value = rulesArray[i - 5].ProductFamilyId;
                worksheet.Cell(i, 8).Value = rulesArray[i - 5].ProductCode;
                worksheet.Cell(i, 9).Value = rulesArray[i - 5].FaultSourceAddress;
                worksheet.Cell(i, 10).Value = rulesArray[i - 5].FaultCode;
                worksheet.Cell(i, 11).Value = rulesArray[i - 5].Spn;
                worksheet.Cell(i, 12).Value = rulesArray[i - 5].Fmi;
                worksheet.Cell(i, 13).Value = BoolToCellValue(rulesArray[i - 5].IsActive);
                worksheet.Cell(i, 14).Value = BoolToCellValue(rulesArray[i - 5].IsPrimaryFault);
                worksheet.Cell(i, 15).Value = ResultTypeToCellValue(rulesArray[i - 5].ResultType);
                worksheet.Cell(i, 16).Value = rulesArray[i - 5].ResultKey;
            }

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                var content = stream.ToArray();
                return content;
            }
        }
        #endregion

        #region Private Methods 
        /// <summary>
        /// Converts the row of the EXCEL file into the Evaluation Rule.
        /// </summary>
        /// <exception cref="Exception">Throws if row cells are empty or null.</exception>
        /// <param name="row">Row to be converted.</param>
        /// <returns>
        /// Returns the Evaluation Rule.
        /// </returns>
        private EvaluationRule RowToEvaluationRule(IXLRow row)
        {
            IXLCell[] cells = row?.Cells(1, Constants.COLUMNS_NAMES.Length)?.ToArray();

            if (cells == null)
                throw new Exception(Constants.INVALID_WORKSHEET_FORMAT_ERROR);

            EvaluationRule rule = new EvaluationRule();

            bool? value = null;

            rule.Prefix = cells[1].GetValue<string>();
            rule.Suffix = cells[2].GetValue<string>();
            rule.OriginType = cells[3].GetValue<string>();
            rule.ComponentSourceAddress = cells[4].GetValue<byte?>();

            value = CellToBool(cells[5]);
            if (value != null)
                rule.IsEaton = value;

            rule.ProductFamilyId = cells[6].GetValue<short?>();
            rule.ProductCode = cells[7].GetValue<int?>();
            rule.FaultSourceAddress = cells[8].GetValue<byte?>();
            rule.FaultCode = cells[9].GetValue<int?>();
            rule.Spn = cells[10].GetValue<string>();
            rule.Fmi = cells[11].GetValue<byte?>();

            value = CellToBool(cells[12]);
            if (value != null)
                rule.IsActive = value;

            value = CellToBool(cells[13]);
            if (value != null)
                rule.IsPrimaryFault = (bool)value;

            rule.ResultType = CellToResultType(cells[14]);
            rule.ResultKey = cells[15].GetValue<string>();
            rule.RuleGroupChecksum = null;

            return rule;
        }

        /// <summary>
        /// Stringifies the Result Type of the Evaluation Rule.
        /// </summary>
        /// <param name="value">Result Type to be stringified.</param>
        /// <returns>
        /// Returns the cell value as a string.
        /// </returns>
        private string ResultTypeToCellValue(byte value)
        {
            switch (value)
            {
                case (byte)Constants.ResultType.EVALUATE:
                    return "Evaluate";
                case (byte)Constants.ResultType.ACTION_PLAN:
                    return "Action Plan";
                default:
                    return "Unknown";
            }
        }

        /// <summary>
        /// Checks if the clumn names of the EXCEL file are valid..
        /// </summary>
        /// <param name="cells">Cells to be validated.</param>
        /// <returns>
        /// Returns true if names are valid.
        /// Returns false if names are invalid.
        /// </returns>
        private bool ValidateColumnsNames(IXLCell[] cells)
        {
            if (cells == null || cells.Length < Constants.COLUMNS_NAMES.Length)
                return false;

            for (int i = 0; i < Constants.COLUMNS_NAMES.Length; i++)
            {
                if (!Constants.COLUMNS_NAMES[i].Equals(cells[i].GetValue<string>()))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Converts the cell value to a boolean type.
        /// </summary>
        /// <param name="cell">Cell to be converted.</param>
        /// <returns>
        /// Returns true if <paramref name="cell"/> is "TRUE".
        /// Returns false if <paramref name="cell"/> is "FALSE".
        /// Returns null if <paramref name="cell"/> is null or empty.
        /// </returns>
        private bool? CellToBool(IXLCell cell)
        {
            string value = cell?.GetValue<string>()?.Trim().ToUpper();
            if (!string.IsNullOrEmpty(value))
            {
                if (value.Equals("TRUE"))
                    return true;
                if (value.Equals("FALSE"))
                    return false;
            }
            return null;
        }

        /// <summary>
        /// Stringifies the boolean type value.
        /// </summary>
        /// <param name="value">Value to be converted.</param>
        /// <returns>
        /// Returns "TRUE" if <paramref name="value"/> is true.
        /// Returns "FALSE" if <paramref name="value"/> is false.
        /// Returns null if <paramref name="value"/> is null.
        /// </returns>
        private string BoolToCellValue(bool? value)
        {
            if (value != null)
                return (bool)value ? "TRUE" : "FALSE";
            return null;
        }

        /// <summary>
        /// Converts the cell to the Result Type of the Evaluation Rule.
        /// </summary>
        /// <param name="cell">Cell to be converted.</param>
        /// <returns>
        /// Returns Result Type of the Evaluation Rule.
        /// </returns>
        private byte CellToResultType(IXLCell cell)
        {
            string key = cell?.GetValue<string>();

            switch (key)
            {
                case "Evaluate":
                    return (byte)Constants.ResultType.EVALUATE;
                case "Action Plan":
                    return (byte)Constants.ResultType.ACTION_PLAN;
                default:
                    return (byte)Constants.ResultType.UNKNOWN;
            }
        }

        /// <summary>
        /// Gets the worksheet tamplete.
        /// </summary>
        /// <returns>Returns the worksheet template to be filled.</returns>
        private IXLWorksheet GetWorksheetTemplate()
        {
            IXLWorkbook workbookTemplate = new XLWorkbook(Constants.TEMPLATE_PATH);

            IXLWorksheet worksheetTemplate = workbookTemplate.Worksheet(Constants.TEMPLATE_WORKSHEET_NAME);

            return worksheetTemplate;

        }
        #endregion
    }
}
