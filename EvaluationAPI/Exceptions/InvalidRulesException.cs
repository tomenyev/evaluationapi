using EvaluationAPI.DTO;
using System.Collections.Generic;

namespace EvaluationAPI.Exceptions
{
    /// <summary>
    /// Invalid rules exception used in <see cref="Validator"/>.
    /// </summary>
    public class InvalidRulesException : IException
    {

        #region Public Constructor
        public InvalidRulesException(List<ErrorDTO> errors) : base(errors)
        {

        }
        #endregion

        #region Public Methods
        public override Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>() { { "errors", Errors } };
        }
        #endregion 
    }
}
