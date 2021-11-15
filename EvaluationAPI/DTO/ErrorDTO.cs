namespace EvaluationAPI.DTO
{
    /// <summary>
    /// Evaluation rules <see cref="Validator"/> error data transfer object.
    /// </summary>
    public class ErrorDTO
    {
        #region Public Properties
        /// <summary>
        /// Error Id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Error message.
        /// </summary>
        public string Message { get; set; }
        #endregion

        #region Public Constructors
        public ErrorDTO(string id, string message)
        {
            this.Id = id;
            this.Message = message;
        }
        #endregion
    }
}
