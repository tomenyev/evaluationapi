using EvaluationAPI.DTO;
using System;
using System.Collections.Generic;

namespace EvaluationAPI.Exceptions
{
    /// <summary>
    /// Base exception.
    /// </summary>
    public abstract class IException : Exception
    {
        #region Private Properties
        /// <summary>
        /// List of <see cref="ErrorDTO"/>.
        /// </summary>
        private List<ErrorDTO> errors_;
        #endregion

        #region Public Constructor
        /// <summary>
        /// Base contructor.
        /// </summary>
        /// <param name="errors">List of <see cref="ErrorDTO"/>.</param>
        public IException(List<ErrorDTO> errors) : base(null)
        {
            errors_ = errors;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets list of <see cref="ErrorDTO"/>.
        /// </summary>
        public List<ErrorDTO> Errors
        {
            get { return this.errors_; }
        }

        /// <summary>
        /// Base converts object to dictionary.
        /// </summary>
        /// <returns>Returns dictionary.</returns>
        public abstract Dictionary<string, object> ToDictionary();
        #endregion
    }
}
