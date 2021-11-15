using System.ComponentModel.DataAnnotations;
using EvaluationAPI.Controllers;

namespace EvaluationAPI.DTO
{
    /// <summary>
    /// Data transfer object used in <see cref="AuthController"/>.
    /// </summary>
    public class UserDTO
    {
        #region Public Methods
        /// <summary>
        /// User username.
        /// </summary>
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; }

        /// <summary>
        /// User password.
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }

        /// <summary>
        /// User role.
        /// </summary>
        public string Role { get; set; }
        #endregion

    }
}
