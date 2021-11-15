using EvaluationAPI.DTO;
using EvaluationAPI.Excel;
using EvaluationAPI.Exceptions;
using EvaluationAPI.Models;
using EvaluationAPI.Repository;
using EvaluationAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EvaluationAPI.Controllers
{
    /// <summary>
    /// <c>EvaluationRule</c> rest api controller class.
    /// Contains all methods for performing <see cref="EvaluationRule"/> CRUD operations, import and export.
    /// </summary>
    [Authorize]
    [EnableCors("EvaluationRulePolicy")]
    [Route("api/[controller]")]
    [ApiController]
    public class EvaluationRuleController : ControllerBase
    {
        #region Private Properties
        private EvaluationRulesRepository repository_;

        private EvaluationRuleService service_;

        private ExcelEvaluationRulesUtils excelUtils_;
        #endregion

        #region Public Constructors
        public EvaluationRuleController(EvaluationRulesRepository repository, EvaluationRuleService service)
        {
            repository_ = repository;
            service_ = service;
            excelUtils_ = new ExcelEvaluationRulesUtils();
        }
        #endregion

        #region Public Methods
        #region GET 
        /// <summary>
        /// Gets Evaluation Rules by RuleGroup rest api controller.
        /// </summary>
        /// <param name="ruleGroup">Used to filter Evaluation Rules by RuleGroup.</param>
        /// <exception cref="Exception">Throws if an unhandled error has occurred.</exception>
        /// <returns>
        /// Returns <see cref="OkResult"/> with the list of <see cref="EvaluationRule"/> if no error has occurred.
        /// Returns <see cref="BadRequestResult"/> if <paramref name="ruleGroup"/> is empty, null or contains white spaces.
        /// Returns <see cref="StatusCodeResult"/> with <see cref="StatusCodes.Status500InternalServerError"/> status code if an error has occurred.
        /// </returns>
        [HttpGet("{ruleGroup}")]
        public async Task<IActionResult> GetByRuleGroup(string ruleGroup)
        {
            if (string.IsNullOrEmpty(ruleGroup) || string.IsNullOrWhiteSpace(ruleGroup))
                return BadRequest(Constants.DATA_ARE_EMPTY_ERROR);

            IEnumerable<EvaluationRule> result = null;

            try
            {
                result = await repository_.GetEvaluationRulesByRuleGroup(ruleGroup);
            }
            catch (Exception e)
            {
                return StatusCode(statusCode: StatusCodes.Status500InternalServerError, value: e.Message);
            }

            return Ok(result);
        }

        /// <summary>
        /// Exports EvaluationRules by RuleGroup as an EXCEL file rest api controller.
        /// </summary>
        /// <param name="ruleGroup">Used to filter Evaluation Rules by RuleGroup.</param>
        /// <exception cref="Exception">Throws if unhandled error has occurred.</exception>
        /// <returns>
        /// Returns <see cref="FileResult"/> with EXCEL file if no error has occurred.
        /// Returns <see cref="BadRequestResult"/> if <paramref name="ruleGroup"/> is empty, null or contains white spaces.
        /// Returns <see cref="StatusCodeResult"/> with <see cref="StatusCodes.Status500InternalServerError"/> status code if an unhandled error has occurred.
        /// </returns>
        [HttpGet("export")]
        public async Task<IActionResult> Export(string ruleGroup)
        {
            if (string.IsNullOrEmpty(ruleGroup) || string.IsNullOrWhiteSpace(ruleGroup))
                return BadRequest(Constants.DATA_ARE_EMPTY_ERROR);

            const string contentType = "application/vnd.ms-excel";
            string fileName = $"{ruleGroup}.xlsx";

            try
            {
                IEnumerable<EvaluationRule> rules = await repository_.GetEvaluationRulesByRuleGroup(ruleGroup);

                byte[] content = excelUtils_.EvaluationRulesToExcel(rules, ruleGroup);

                return File(content, contentType, fileName);
            }
            catch (Exception e)
            {
                return StatusCode(statusCode: StatusCodes.Status500InternalServerError, value: e.Message);
            }
        }

        /// <summary>
        /// Gets all Evaluation Rules rest api controller.
        /// </summary>
        /// <exception cref="Exception">Throws if unhandled error has occurred.</exception>
        /// <returns>
        /// Returns <see cref="OkResult"/> with Evaluation Rules if no error has occurred.
        /// Returns <see cref="StatusCodeResult"/> with <see cref="StatusCodes.Status500InternalServerError"/> status code if an unhandled error has occurred.
        /// </returns>
        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            IEnumerable<EvaluationRule> result = null;

            try
            {
                result = await repository_.GetEvaluationRules();
            }
            catch (Exception e)
            {
                return StatusCode(statusCode: StatusCodes.Status500InternalServerError, value: e.Message);
            }

            return Ok(result);
        }

        /// <summary>
        /// Gets all Evaluation Rule RuleGroups rest api controller.
        /// </summary>
        /// <exception cref="Exception">Throws if an unhandled error has occurred.</exception>
        /// <returns>
        /// Returns <see cref="OkResult"/> with Evaluation Rules if no error has occurred.
        /// Returns <see cref="StatusCodeResult"/> with <see cref="StatusCodes.Status500InternalServerError"/> status code if an unhandled error has occurred.
        /// </returns>
        [HttpGet("ruleGroup")]
        public async Task<IActionResult> GetRuleGroups()
        {
            string[] result = null;

            try
            {
                result = await repository_.GetRuleGroups();
            }
            catch (Exception e)
            {
                return StatusCode(statusCode: StatusCodes.Status500InternalServerError, value: e.Message);
            }

            return Ok(result);
        }
        #endregion

        #region POST
        /// <summary>
        /// Creates/Updates/Deletes Evaluation Rules rest api controller.
        /// </summary>
        /// <param name="request">Used to transfer <see cref="SaveRequestDTO.Rules"/> and <see cref="SaveRequestDTO.RulesToDelete"/> to change database state.</param>
        /// <exception cref="Exception">Throws if an unhandled error has occurred.</exception>
        /// <exception cref="IException">Throws if <paramref name="request"/> is invalid or a database concurrency error has occurred.</exception>
        /// <returns>
        /// Returns <see cref="OkResult"/> with updated or added Evaluation Rules if no error has occurred.
        /// Returns <see cref="StatusCodeResult"/> with <see cref="StatusCodes.Status500InternalServerError"/> status code if an error has occurred.
        /// </returns>
        [HttpPost("save")]
        public async Task<IActionResult> Save(SaveRequestDTO request)
        {
            IEnumerable<EvaluationRule> result = null;

            try
            {
                result = await service_.SaveEvaluationRules(request.Rules, request.RulesToDelete);
            }
            catch (IException e)
            {
                return StatusCode(statusCode: StatusCodes.Status500InternalServerError, value: e.ToDictionary());
            }
            catch (Exception e)
            {
                return StatusCode(statusCode: StatusCodes.Status500InternalServerError, value: e.Message);
            }
            return Ok(result);
        }

        /// <summary>
        /// Adds Evaluation Rules rest api controller.
        /// </summary>
        /// <param name="rules">Evaluation Rules to be added.</param>
        /// <exception cref="Exception">Throws if an unhandled error has occurred.</exception>
        /// <exception cref="IException">Throws if <paramref name="rules"/> is invalid or a database concurrency error has occurred.</exception>
        /// <returns>
        /// Returns <see cref="OkResult"/> with Evaluation Rules if no error has occurred.
        /// Returns <see cref="StatusCodeResult"/> with <see cref="StatusCodes.Status500InternalServerError"/> status code if an error has occurred.
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> Add(IEnumerable<EvaluationRule> rules)
        {
            IEnumerable<EvaluationRule> result = null;

            try
            {
                result = await service_.SaveEvaluationRules(rules, Array.Empty<EvaluationRule>());
            }
            catch (IException e)
            {
                return StatusCode(statusCode: StatusCodes.Status500InternalServerError, value: e.ToDictionary());
            }
            catch (Exception e)
            {
                return StatusCode(statusCode: StatusCodes.Status500InternalServerError, value: e.Message);
            }

            return Ok(result);
        }

        /// <summary>
        /// Imports Evaluation Rules from EXCEL file rest api controller.
        /// </summary>
        /// <param name="file">EXCEL file to be imported.</param>
        /// <param name="sheetName">EXCEL sheetname to be imported.</param>
        /// <param name="rule">Evaluation Rule in JSON format used to link the EXCEL file Rules.</param>
        /// <exception cref="Exception">Throws if an unhandled error has occurred.</exception>
        /// <exception cref="IException">Throws if Rules in EXCEL file are invalid or a database concurrency error has occurred.</exception>
        /// <returns>
        /// Returns <see cref="OkResult"/> with Evaluation Rules if no error has occurred.
        /// Returns <see cref="BadRequestResult"/> if <paramref name="sheetName"/>/<paramref name="rule"/>/<paramref name="file"/> is null or empty.
        /// Returns <see cref="StatusCodeResult"/> with <see cref="StatusCodes.Status500InternalServerError"/> status code if an error has occurred.
        /// </returns>
        [HttpPost("import")]
        [Consumes("application/vnd.ms-excel", "multipart/form-data")]
        public async Task<IActionResult> Import([FromForm] IFormFile file, [FromForm] string sheetName, [FromForm] string rule)
        {
            if (string.IsNullOrEmpty(sheetName) || string.IsNullOrEmpty(rule) || file == null || file.Length == 0)
                return BadRequest(Constants.DATA_ARE_EMPTY_ERROR);

            IEnumerable<EvaluationRule> result = null;

            EvaluationRule objRule = JsonConvert.DeserializeObject<EvaluationRule>(rule);

            try
            {
                result = excelUtils_.ExcelToEvaluationRules(file, sheetName);

                result = await service_.AddEvaluationRules((List<EvaluationRule>)result, objRule);
            }
            catch (IException e)
            {
                return StatusCode(statusCode: StatusCodes.Status500InternalServerError, value: e.ToDictionary());
            }
            catch (Exception e)
            {
                return StatusCode(statusCode: StatusCodes.Status500InternalServerError, value: e.Message);
            }

            return Ok(result);
        }

        /// <summary>
        /// Initializes Root Evaluation rest api controller.
        /// </summary>
        /// <exception cref="Exception">Throws if an unhandled error has occurred.</exception>
        /// <exception cref="IException">Throws if a database concurrency error has occurred.</exception>
        /// <returns>
        /// Returns <see cref="OkResult"/> with Root Evaluation Rules if no error has occurred.
        /// Returns <see cref="StatusCodeResult"/> with <see cref="StatusCodes.Status500InternalServerError"/> status code if an error has occurred.
        /// </returns>
        [HttpPost("Init")]
        public async Task<IActionResult> InitRoot()
        {
            IEnumerable<EvaluationRule> result = null;

            try
            {
                result = await service_.InitRoot();
            }
            catch (IException e)
            {
                return StatusCode(statusCode: StatusCodes.Status500InternalServerError, value: e.ToDictionary());
            }
            catch (Exception e)
            {
                return StatusCode(statusCode: StatusCodes.Status500InternalServerError, value: e.Message);
            }

            return Ok(result);
        }

        /// <summary>
        /// Initializes new Rule Group rest api controller.
        /// </summary>
        /// <exception cref="Exception">Throws if an unhandled error has occurred.</exception>
        /// <exception cref="IException">Throws if a database concurrency error has occurred.</exception>
        /// <param name="request">Transfers <see cref="AddRuleGroupDTO.Rule"/> to which new RuleGroup must be linked and <see cref="AddRuleGroupDTO.Multiple"/> to specify new RuleGroup type.</param>
        /// <returns>
        /// Returns <see cref="OkResult"/> with updated EvaluationRules if success.
        /// Returns <see cref="StatusCodeResult"/> with <see cref="StatusCodes.Status500InternalServerError"/> status code if an error has occurred.
        /// </returns>
        [HttpPost("RuleGroup")]
        public async Task<IActionResult> AddRuleGroup(AddRuleGroupDTO request)
        {
            IEnumerable<EvaluationRule> result = null;

            try
            {
                result = await service_.AddRuleGroup(request.Rule, request.Multiple);
            }
            catch (IException e)
            {
                return StatusCode(statusCode: StatusCodes.Status500InternalServerError, value: e.ToDictionary());
            }
            catch (Exception e)
            {
                return StatusCode(statusCode: StatusCodes.Status500InternalServerError, value: e.Message);
            }

            return Ok(result);
        }
        #endregion

        #region PUT
        /// <summary>
        /// Updates Evaluation Rules rest api controller.
        /// </summary>
        /// <exception cref="Exception">Throws if an unhandled error has occurred.</exception>
        /// <exception cref="IException">Throws if a database concurrency error has occurred.</exception>
        /// <param name="rules">Evaluation Rules to be updated.</param>
        /// <returns>
        /// Returns <see cref="OkResult"/> with updated Evaluation Rules if no error has occurred.
        /// Returns <see cref="StatusCodeResult"/> with <see cref="StatusCodes.Status500InternalServerError"/> status code if an error has occurred.
        /// </returns>
        [HttpPut]
        public async Task<IActionResult> Update(IEnumerable<EvaluationRule> rules)
        {
            IEnumerable<EvaluationRule> result = null;

            try
            {
                result = await service_.UpdateEvaluationRules(rules);
            }
            catch (IException e)
            {
                return StatusCode(statusCode: StatusCodes.Status500InternalServerError, value: e.ToDictionary());
            }
            catch (Exception e)
            {
                return StatusCode(statusCode: StatusCodes.Status500InternalServerError, value: e.Message);
            }

            return Ok(result);
        }
        #endregion

        #region DELETE
        /// <summary>
        /// Deletes Evaluation Rules rest api controller.
        /// </summary>
        /// <exception cref="Exception">Throws if an unhandled error has occurred.</exception>
        /// <exception cref="IException">Throws if a database concurrency error has occurred.</exception>
        /// <param name="rules">Evaluation rules to be deleted.</param>
        /// <returns>
        /// Returns <see cref="OkResult"/> with Evaluation Rules if no error has occurred.
        /// Returns <see cref="StatusCodeResult"/> with <see cref="StatusCodes.Status500InternalServerError"/> status code if an error has occurred.
        /// </returns>
        [HttpDelete]
        public async Task<IActionResult> Delete(IEnumerable<EvaluationRule> rules)
        {
            IEnumerable<EvaluationRule> result = null;

            try
            {
                result = await service_.DeleteEvaluationRules(rules);
            }
            catch (IException e)
            {
                return StatusCode(statusCode: StatusCodes.Status500InternalServerError, value: e.ToDictionary());
            }
            catch (Exception e)
            {
                return StatusCode(statusCode: StatusCodes.Status500InternalServerError, value: e.Message);
            }

            return Ok(result);
        }

        /// <summary>
        /// Deletes Evaluation Rules by RuleGroup.
        /// </summary>
        /// <exception cref="Exception">Throws if an unhandled error has occurred.</exception>
        /// <param name="ruleGroup">RuleGroup to be deleted.</param>
        /// <returns>
        /// Returns <see cref="OkResult"/> with list of RuleGroups if no error has occurred.
        /// Returns <see cref="BadRequestResult"/> if <paramref name="ruleGroup"/> is invalid.
        /// Returns <see cref="StatusCodeResult"/> with <see cref="StatusCodes.Status500InternalServerError"/> status code if an error has occurred.
        /// </returns>
        [HttpDelete("{ruleGroup}")]
        public async Task<IActionResult> DeleteAllByRuleGroup(string ruleGroup)
        {
            if (string.IsNullOrEmpty(ruleGroup) || string.IsNullOrWhiteSpace(ruleGroup))
                return BadRequest(Constants.DATA_ARE_EMPTY_ERROR);

            string[] result = Array.Empty<string>();

            try
            {
                result = await repository_.DeleteAllByRuleGroup(ruleGroup);
            }
            catch (Exception e)
            {
                return StatusCode(statusCode: StatusCodes.Status500InternalServerError, value: e.Message);
            }

            return Ok(result);
        }

        /// <summary>
        /// Deletes all Evaluation Rules.
        /// </summary>
        /// <exception cref="Exception">Throws if unhandled error has occurred.</exception>
        /// <returns>
        /// Returns <see cref="OkResult"/> with the number of deleted Rules if no error has occurred.
        /// Returns <see cref="StatusCodeResult"/> with <see cref="StatusCodes.Status500InternalServerError"/> status code if an error has occurred.
        /// </returns>
        [Authorize(Roles = "Admin")]
        [HttpDelete("all")]
        public async Task<IActionResult> DeleteAll()
        {
            int? result = null;

            try
            {
                result = await repository_.DeleteAll();
            }
            catch (Exception e)
            {
                return StatusCode(statusCode: StatusCodes.Status500InternalServerError, value: e.Message);
            }

            return Ok(result);
        }
        #endregion
        #endregion

    }
}
