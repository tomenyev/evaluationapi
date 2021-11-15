using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

#nullable disable

namespace EvaluationAPI.Models
{
    /// <summary>
    /// Evaluation Rule.
    /// </summary>
    public partial class EvaluationRule
    {
        #region Public Constructor
        public EvaluationRule()
        {

        }
        #endregion

        #region Public Properties - Logic, Origin
        /// <summary>
        /// Unique ID for rule.
        /// </summary>
        public int? Id { get; set; }
        /// <summary>
        /// Rule Group.
        /// </summary>
        [Required(ErrorMessage = "RuleGroup is null or empty.")]
        public string RuleGroup { get; set; }
        /// <summary>
        /// Priority.
        /// </summary>
        public int? Priority { get; set; }
        /// <summary>
        /// Used for writing complex rules for doing logical operation with other rule.
        /// </summary>
        public string Prefix { get; set; }
        /// <summary>
        /// Used for writing complex rules for doing logical operation with other Rule.
        /// </summary>
        public string Suffix { get; set; }
        /// <summary>
        /// <see cref="ServiceActivityReport"/> (SAR) Origin Type.
        /// </summary>
        /// <remarks>
        /// SR4, OTS, etc.
        /// </remarks>
        public string OriginType { get; set; }
        #endregion

        #region Public Properties - Evaluation Logic
        /// <summary>
        /// Evaluation Id.
        /// </summary>
        [JsonIgnore]
        [NotMapped]
        public string EId { get; set; }
        /// <summary>
        /// Evaluation Result Type.
        /// </summary>
        [JsonIgnore]
        [NotMapped]
        public Constants.ResultType EResultType { get { return ToEnum(this.ResultType); } }
        /// <summary>
        /// Evaluation evaluate function.
        /// </summary>
        [JsonIgnore]
        [NotMapped]
        public Func<bool> Evaluate { get; set; }
        #endregion

        #region Public Properties - Component
        /// <summary>
        /// Component Address.
        /// </summary>
        public byte? ComponentSourceAddress { get; set; }
        /// <summary>
        /// Is Eaton Component.
        /// </summary>
        /// <remarks>
        /// Null value means it will not be checked.
        /// </remarks>
        public bool? IsEaton { get; set; }
        /// <summary>
        /// Product Family ID.
        /// </summary>
        public short? ProductFamilyId { get; set; }
        /// <summary>
        /// Product Code.
        /// </summary>
        public int? ProductCode { get; set; }
        #endregion

        #region Public Properties - Fault
        /// <summary>
        /// Fault Source.
        /// </summary>
        public byte? FaultSourceAddress { get; set; }
        /// <summary>
        /// FaultCode.
        /// </summary>
        public int? FaultCode { get; set; }
        /// <summary>
        /// Fault SPN.
        /// </summary>
        public string Spn { get; set; }
        /// <summary>
        /// Fault FMI.
        /// </summary>
        public byte? Fmi { get; set; }
        /// <summary>
        ///  Is Active Fault.
        /// </summary>
        /// <remarks>
        ///  Null value means it will not be checked.
        /// </remarks>
        public bool? IsActive { get; set; }
        /// <summary>
        /// Flag to set fault as primary in Action Plan.
        /// </summary>
        public bool IsPrimaryFault { get; set; }
        #endregion

        #region Public Properties - Results
        /// <summary>
        /// Evaluation Result Type.
        /// </summary>
        [Required(ErrorMessage = "ResultType is null or empty.")]
        public byte ResultType { get; set; }
        /// <summary>
        /// Evalution Result Key.
        /// </summary>
        /// <remarks>
        /// Template Key or Evaluation sheetname.
        /// </remarks>
        public string ResultKey { get; set; }
        #endregion

        #region Public Properties - Db Concurrency
        /// <summary>
        /// ConcurrencyException idicator.
        /// </summary>
        public int? RuleGroupChecksum { get; set; }
        #endregion

        #region Private Method
        /// <summary>
        /// Converts Result Type to enum.
        /// </summary>
        /// <param name="resultType"></param>
        /// <returns>Returns enum <see cref="Constants.ResultType"/>.</returns>
        private Constants.ResultType ToEnum(byte resultType)
        {
            switch (resultType)
            {
                case 1:
                    return Constants.ResultType.EVALUATE;
                case 3:
                    return Constants.ResultType.ACTION_PLAN;
                default:
                    return Constants.ResultType.UNKNOWN;
            }
        }
        #endregion
    }
}
