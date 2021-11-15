using EvaluationAPI.Evaluation.Models;
using EvaluationAPI.Models;
using EvaluationAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace EvaluationAPI.Controllers
{
    /// <summary>
    /// Evaluation rest api controller class.
    /// Contains all methods for performing <see cref="ServiceActivityReport"/> evaluation.
    /// </summary>
    [Authorize]
    [EnableCors("EvaluationPolicy")]
    [Route("api/[controller]")]
    [ApiController]
    public class EvaluationController : ControllerBase
    {
        #region Private Properties
        private readonly EvaluationService service_;
        #endregion

        #region Public Constructors
        public EvaluationController(EvaluationService service)
        {
            service_ = service;
        }
        #endregion

        #region Public Methods
        #region POST
        /// <summary>
        /// <see cref="ServiceActivityReport"/> evaluation rest api controller.
        /// </summary>
        /// <param name="sar">Transfers <see cref="Origin"/>, <see cref="Location"/>, <see cref="Vehicle"/> data and <see cref="Fault"/> to be evaluated.</param>
        /// <exception cref="Exception">Thrown when error in evaluation process has occurred.</exception>
        /// <returns>
        /// Returns <see cref="OkResult"/> with Action Plan Key if no error has occurred.
        /// Returns <see cref="BadRequestResult"/> if request data are invalid.
        /// Returns <see cref="StatusCodeResult"/> with <see cref="StatusCodes.Status500InternalServerError"/> status code if any error has occurred.
        /// </returns>
        [HttpPost]
        public IActionResult Evaluate(ServiceActivityReport sar)
        {
            string badRequest = ValidateSar(sar);

            if (!string.IsNullOrEmpty(badRequest))
                return BadRequest(badRequest);

            EvaluationResult result = new EvaluationResult();
            string actionPlanKey = null;

            try
            {
                service_.Evaluate(sar, Constants.ROOT_EVALUATION, ref result);

                actionPlanKey = result?.ActionPlanKey;
            }
            catch (Exception e)
            {
                return StatusCode(statusCode: StatusCodes.Status500InternalServerError, value: e.Message);
            }

            return Ok(actionPlanKey);
        }
        #endregion
        #endregion

        #region Private Methods
        #region Service Activity Report Validator
        /// <summary>
        /// <see cref="ServiceActivityReport"/> validator.
        /// </summary>
        /// <param name="sar">Transfers <see cref="Origin"/>, <see cref="Location"/>, <see cref="Vehicle"/>, <see cref="Component"/> and <see cref="Fault"/> to be validated.</param>
        /// <returns>
        /// Returns validation result message if <paramref name="sar"/> is invalid.
        /// Returns null value if <paramref name="sar"/> is valid.
        /// </returns>
        private string ValidateSar(ServiceActivityReport sar)
        {
            string result = null;

            if (sar == null || sar.Id <= 0)
                result = Constants.INVALID_SAR_ERROR;
            if (string.IsNullOrEmpty(sar.Origin?.OriginType))
                result = Constants.NO_ORIGIN_INFO_ERROR;
            if (sar.Vehicle == null)
                result = Constants.VEHICLE_IS_NULL_ERROR;
            if (sar.Vehicle.Components == null || !sar.Vehicle.Components.Any())
                result = Constants.VEHICLE_HAS_NO_COMPONENTS_ERROR;
            if (!sar.Vehicle.Components.TrueForAll(c => IsValidComponent(c)))
                result = Constants.INVALID_COMPONENT_DATA_ERROR;
            if (sar.Faults == null || !sar.Faults.Any())
                result = Constants.NO_FAULTS_TO_EVALUATE_ERROR;
            if (!sar.Faults.TrueForAll(f => IsValidFault(f)))
                result = Constants.INVALID_FAULT_DATA_ERROR;
            if (!sar.Vehicle.Components.Any(p => IsEatonComponent(p)))
                result = Constants.NO_EATON_COMPONENTS_ERROR;

            return result;
        }

        /// <summary>
        /// <see cref="Component"/> validator.
        /// </summary>
        /// <remarks>
        /// <paramref name="component"/> is valid if both <see cref="Component.Make"/> and <see cref="Component.Model"/> are not null or empty.
        /// </remarks>
        /// <param name="component">Transfers component data to be validated.</param>
        /// <returns>
        /// Retruns true if <paramref name="component"/> is valid. 
        /// Returns false if <paramref name="component"/> is invalid.
        /// </returns>
        private bool IsValidComponent(Component component) => !string.IsNullOrEmpty(component.Make) && !string.IsNullOrEmpty(component.Model);

        /// <summary>
        /// Check if <see cref="Component"/> belongs to Eaton.
        /// </summary>
        /// <remarks>
        /// <paramref name="component"/> is Eaton's if <see cref="Component.Make"/> equals <see cref="Constants.EATON_COMPONENT_NAME"/>.
        /// </remarks>
        /// <param name="component">Transfers component data to be checked.</param>
        /// <returns>
        /// Returns true if <paramref name="component"/> is Eaton's.
        /// Returns false if <paramref name="component"/> is not Eaton's.
        /// </returns>
        private bool IsEatonComponent(Component component) => component.Make.ToLower() == Constants.EATON_COMPONENT_NAME;

        /// <summary>
        /// <see cref="Fault"/> validator.
        /// </summary>
        /// <remarks>
        /// <paramref name="fault"/> is valid if <see cref="Fault.Spn"/> is not null or empty and is a digit.
        /// </remarks>
        /// <param name="fault">Transfer fault data to be validated.</param>
        /// <returns>
        /// Returns true if <paramref name="fault"/> is valid.
        /// Returns false if <paramref name="fault"/> is invalid.
        /// </returns>
        private bool IsValidFault(Fault fault) => !string.IsNullOrEmpty(fault.Spn) && fault.Spn.All(Char.IsDigit);
        #endregion
        #endregion
    }
}
