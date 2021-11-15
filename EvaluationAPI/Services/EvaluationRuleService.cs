using EvaluationAPI.DTO;
using EvaluationAPI.Exceptions;
using EvaluationAPI.Models;
using EvaluationAPI.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EvaluationAPI.Services
{
    public class EvaluationRuleService
    {

        #region Private Properties
        private EvaluationRulesRepository repository_;
        #endregion

        #region Constructor
        public EvaluationRuleService(EvaluationRulesRepository repository)
        {
            repository_ = repository;
        }
        #endregion

        #region Public Methods - Change Db Context
        /// <summary>
        /// Adds new RuleGroup.
        /// </summary>
        /// <exception cref="Exception">
        /// Throws if <paramref name="rule"/> is null or <paramref name="rule"/> RuleGroup, RuleGroupChecksum or ResultKey is null or empty
        /// or database doesn't contains rules with <paramref name="rule"/> RuleGroup.
        /// </exception>
        /// <exception cref="ConcurrencyException">
        /// Throws if database RuleGroupChecksum doesn't equals <paramref name="rule"/> RuleGroupChecksum.
        /// </exception>
        /// <param name="rule">Rule to link new rule group to.</param>
        /// <param name="multiple">Complex rules indicator.</param>
        /// <returns>
        /// Returns list of rules by <paramref name="rule"/> RuleGroup.
        /// </returns>
        public async Task<IEnumerable<EvaluationRule>> AddRuleGroup(EvaluationRule rule, bool multiple)
        {
            if (rule == null || string.IsNullOrEmpty(rule.RuleGroup) || rule.RuleGroupChecksum == null || string.IsNullOrEmpty(rule.ResultKey))
                throw new Exception(Constants.DATA_ARE_EMPTY_ERROR);

            List<EvaluationRule> rules = (List<EvaluationRule>)await repository_.GetEvaluationRulesByRuleGroup(rule.RuleGroup);

            EvaluationRule dbRule = rules.FirstOrDefault();

            if (dbRule == null)
                throw new Exception(Constants.OUT_OF_DATE_ERROR);

            try
            {
                List<EvaluationRule> rulesToUpdate = new List<EvaluationRule>();
                List<EvaluationRule> rulesToAdd = new List<EvaluationRule>();

                int? dbRuleGroupChecksum = dbRule.RuleGroupChecksum;

                bool ruleGroupChecksumException = (rule.RuleGroupChecksum == null || !rule.RuleGroupChecksum.Equals(dbRuleGroupChecksum));

                if (ruleGroupChecksumException)
                    throw new DbUpdateConcurrencyException();

                int checksum = DateTime.Now.GetHashCode();

                if (rule.Id == null)
                    rules.Insert(0, rule);

                int i = 0;
                foreach (EvaluationRule r in rules)
                {
                    r.RuleGroupChecksum = checksum;
                    r.Priority = i;
                    if (r.Id == null)
                    {
                        rulesToAdd.Add(r);
                    }
                    else
                    {
                        rulesToUpdate.Add(r);
                    }
                    i++;
                }

                if (multiple)
                {
                    EvaluationRule r1 = new EvaluationRule();
                    EvaluationRule r2 = new EvaluationRule();
                    r1.Priority = 0;
                    r2.Priority = 1;
                    r1.RuleGroupChecksum = checksum;
                    r2.RuleGroupChecksum = checksum;
                    r1.Prefix = Constants.GROUP_START_CHAR;
                    r2.Prefix = Constants.TEMPLATE_RULE_SEPARATOR_AND;
                    r2.Suffix = Constants.GROUP_END_CHAR;
                    r1.RuleGroup = rule.ResultKey;
                    r2.RuleGroup = rule.ResultKey;
                    r1.ResultKey = Constants.DEFAULT_RESULT_KEY;
                    r2.ResultKey = Constants.DEFAULT_RESULT_KEY;
                    r1.ResultType = (byte)Constants.ResultType.ACTION_PLAN;
                    r2.ResultType = (byte)Constants.ResultType.ACTION_PLAN;
                    rulesToAdd.Add(r1);
                    rulesToAdd.Add(r2);
                }
                else
                {
                    EvaluationRule r1 = new EvaluationRule();
                    r1.Priority = 0;
                    r1.RuleGroupChecksum = checksum;
                    r1.RuleGroup = rule.ResultKey;
                    r1.ResultType = (byte)Constants.ResultType.ACTION_PLAN;
                    r1.ResultKey = Constants.DEFAULT_RESULT_KEY;
                    rulesToAdd.Add(r1);
                }

                repository_.UpdateRange(rulesToUpdate);
                return await repository_.Add(rulesToAdd, rule.RuleGroup);
            }
            catch (DbUpdateConcurrencyException)
            {
                List<ErrorDTO> errors = new List<ErrorDTO>();
                errors.Add(new ErrorDTO(null, Constants.OUT_OF_DATE_ERROR));
                throw new ConcurrencyException(rules, errors);
            }
        }

        /// <summary>
        /// Initialize Root Evaluation.
        /// </summary>
        /// <exception cref="ConcurrencyException">
        /// Throws if Root Evaluation already exists.
        /// </exception>
        /// <returns>
        /// Returns list of rules in Root Evaluation.
        /// </returns>
        public async Task<IEnumerable<EvaluationRule>> InitRoot()
        {
            List<EvaluationRule> rules = (List<EvaluationRule>)await repository_.GetEvaluationRulesByRuleGroup(Constants.ROOT_EVALUATION);

            try
            {
                EvaluationRule rule = rules.FirstOrDefault();

                if (rule != null)
                    throw new DbUpdateConcurrencyException();

                int checksum = DateTime.Now.GetHashCode();

                rule = new EvaluationRule();
                rule.Priority = 0;
                rule.RuleGroup = Constants.ROOT_EVALUATION;
                rule.ResultKey = Constants.DEFAULT_RESULT_KEY;
                rule.ResultType = (byte)Constants.ResultType.ACTION_PLAN;
                rule.RuleGroupChecksum = checksum;
                rules.Add(rule);

                return await repository_.Add(rules, Constants.ROOT_EVALUATION);
            }
            catch (DbUpdateConcurrencyException)
            {
                List<ErrorDTO> errors = new List<ErrorDTO>();
                errors.Add(new ErrorDTO(null, Constants.OUT_OF_DATE_ERROR));
                throw new ConcurrencyException(rules, errors);
            }
        }

        /// <summary>
        /// Saves changes.
        /// </summary>
        /// <exception cref="Exception">
        /// Throws if <paramref name="rules"/> RuleGroup is null or empty or <paramref name="rulesToDelete"/> RuleGroup is null or empty.
        /// </exception>
        /// <exception cref="ConcurrencyException">
        /// Throws if <paramref name="rules"/> or <paramref name="rulesToDelete"/> RuleGroupChecksum is null or empty or doesn't equals database RuleGroupChecksum.
        /// </exception>
        /// <exception cref="InvalidRulesException">
        /// Throws if evaluation rules are invalid.
        /// </exception>
        /// <param name="rules">Rules to add or update.</param>
        /// <param name="rulesToDelete">Rules to delete.</param>
        /// <returns>
        /// Returns list of rules with RuleGroup equals <paramref name="rules"/> or <paramref name="rulesToDelete"/> RuleGroup.
        /// </returns>
        public async Task<IEnumerable<EvaluationRule>> SaveEvaluationRules(IEnumerable<EvaluationRule> rules, IEnumerable<EvaluationRule> rulesToDelete)
        {
            EvaluationRule rule = rules.FirstOrDefault();

            string ruleGroup = rule?.RuleGroup;

            if (string.IsNullOrEmpty(ruleGroup))
            {
                rule = rulesToDelete.FirstOrDefault();
                ruleGroup = rule?.RuleGroup;
            }
            if (string.IsNullOrEmpty(ruleGroup))
                throw new Exception(Constants.DATA_ARE_EMPTY_ERROR);

            rules = rules.OrderBy(r => r.Priority);

            IEnumerable<EvaluationRule> dbRules = await repository_.GetEvaluationRulesByRuleGroup(ruleGroup);

            try
            {
                List<EvaluationRule> rulesToUpdate = new List<EvaluationRule>();
                List<EvaluationRule> rulesToAdd = new List<EvaluationRule>();

                int? ruleGroupChecksum = rule.RuleGroupChecksum;

                int? dbRuleGroupChecksum = dbRules.FirstOrDefault()?.RuleGroupChecksum;

                bool ruleGroupChecksumException = (dbRuleGroupChecksum != null && !dbRuleGroupChecksum.Equals(ruleGroupChecksum));

                if (ruleGroupChecksumException)
                    throw new DbUpdateConcurrencyException();

                List<ErrorDTO> errors = Validator.ValidateRules(rules);
                if ((errors?.Count() ?? 0) != 0)
                    throw new InvalidRulesException(errors);

                int checksum = DateTime.Now.GetHashCode();
                foreach (EvaluationRule r in rules)
                {
                    r.RuleGroupChecksum = checksum;
                    if (r.Id == null)
                    {
                        rulesToAdd.Add(r);
                    }
                    else
                    {
                        rulesToUpdate.Add(r);
                    }
                }

                List<int> idsToDelete = rulesToDelete.Select(r => r.Id.Value).ToList();
                repository_.RemoveRange(repository_.EvaluationRules.Where(r => idsToDelete.Contains(r.Id.Value)));
                repository_.UpdateRange(rulesToUpdate);
                return await repository_.Add(rulesToAdd, ruleGroup);
            }
            catch (DbUpdateConcurrencyException)
            {
                List<ErrorDTO> errors = new List<ErrorDTO>();
                errors.Add(new ErrorDTO(null, Constants.OUT_OF_DATE_ERROR));
                throw new ConcurrencyException(dbRules, errors);
            }
        }

        /// <summary>
        /// Adds <paramref name="rules"/> and links to <paramref name="rule"/> 
        /// or just adds if <paramref name="rules"/> RuleGroup equals Root Evaluation.
        /// </summary>
        /// <exception cref="Exception">
        /// Throws if <paramref name="rule"/> is null or empty or <paramref name="rules"/> RuleGroup already exists.
        /// Throws if <paramref name="rule"/> RuleGroup is null or empty.
        /// Throws if <paramref name="rule"/> RuleGroup is Root Evaluation and <paramref name="rules"/> are complex.
        /// Throws if database doesn't contains rules with <paramref name="rule"/> RuleGroup.
        /// Throws if <paramref name="rule"/> is complex.
        /// Throws if <paramref name="rule"/> is not Root Evaluation and <paramref name="rules"/> are not complex.
        /// Trhows if <paramref name="rule"/> Id is not null and <paramref name="rule"/> ResultKey doesn't equals <paramref name="rules"/> RuleGroup.
        /// </exception>
        /// <exception cref="ConcurrencyException">
        /// Throws if <paramref name="rule"/> RuleGroupChecksum doesn't equal database RuleGroupChecksum.
        /// </exception>
        /// <exception cref="InvalidRulesException">
        /// Throws if evaluation rules are invalid.
        /// </exception>
        /// <param name="rules">Evaluation rules to be added.</param>
        /// <param name="rule">Rule to be linked to.</param>
        /// <returns>
        /// Returns <paramref name="rules"/> if database was empty.
        /// Returns <paramref name="rule"/> RuleGroup rules.
        /// </returns>
        public async Task<IEnumerable<EvaluationRule>> AddEvaluationRules(List<EvaluationRule> rules, EvaluationRule rule)
        {
            EvaluationRule ruleToAdd = rules.FirstOrDefault();
            if (ruleToAdd == null)
                throw new Exception(Constants.DATA_ARE_EMPTY_ERROR);

            if (await repository_.IsRuleGroupExists(ruleToAdd.RuleGroup))
                throw new Exception(Constants.RULE_GROUP_ALREADY_EXISTS_ERROR);

            if (ruleToAdd.RuleGroup == Constants.ROOT_EVALUATION)
            {
                try
                {
                    List<ErrorDTO> errors = Validator.ValidateRules(rules);
                    if ((errors?.Count() ?? 0) != 0)
                        throw new InvalidRulesException(errors);

                    return await repository_.Add(rules, ruleToAdd.RuleGroup);
                }
                catch (DbUpdateConcurrencyException)
                {
                    List<ErrorDTO> errors = new List<ErrorDTO>();
                    errors.Add(new ErrorDTO(null, Constants.OUT_OF_DATE_ERROR));
                    throw new ConcurrencyException(await repository_.GetEvaluationRulesByRuleGroup(Constants.ROOT_EVALUATION), errors);
                }
            }

            if (string.IsNullOrEmpty(rule.RuleGroup))
                throw new Exception(Constants.DATA_ARE_EMPTY_ERROR);

            if (rule.RuleGroup == Constants.ROOT_EVALUATION && Validator.IsMultiple(ruleToAdd))
                throw new Exception(Constants.ROOT_TO_MULTIPLE_EVALUATION_ERROR);

            List<EvaluationRule> dbRules = (List<EvaluationRule>) await repository_.GetEvaluationRulesByRuleGroup(rule.RuleGroup);

            EvaluationRule dbRule = dbRules.FirstOrDefault();

            if (dbRule == null)
                throw new Exception(Constants.OUT_OF_DATE_ERROR);

            if (Validator.IsMultiple(dbRule))
                throw new Exception(Constants.MULTIPLE_EVALUATION_ERROR);

            if (rule.RuleGroup != Constants.ROOT_EVALUATION && !Validator.IsMultiple(ruleToAdd))
                throw new Exception(Constants.SINGLE_EVALUATION_TO_SINGLE_EVALUATION_ERROR);

            if (rule.Id != null && rule.ResultKey != ruleToAdd.RuleGroup)
                throw new Exception(Constants.RESULT_KEY_SAME_AS_RULE_GROUP_ERROR);

            try
            {
                List<EvaluationRule> rulesToUpdate = new List<EvaluationRule>();

                int? dbRuleGroupChecksum = dbRule.RuleGroupChecksum;

                bool ruleGroupChecksumException = (!rule.RuleGroupChecksum.Equals(dbRuleGroupChecksum));

                if (ruleGroupChecksumException)
                    throw new DbUpdateConcurrencyException();

                List<ErrorDTO> errors = Validator.ValidateRules(rules);
                if ((errors?.Count() ?? 0) != 0)
                    throw new InvalidRulesException(errors);

                if (rule.Id != null)
                {
                    rulesToUpdate.Add(rule);
                }
                else
                {
                    int newChecksum = DateTime.Now.GetHashCode();

                    rule.RuleGroupChecksum = newChecksum;
                    rule.ResultType = (byte)Constants.ResultType.EVALUATE;
                    rule.ResultKey = ruleToAdd.RuleGroup;
                    int size = dbRules.Count();
                    if (rule.RuleGroup == Constants.ROOT_EVALUATION && size > 2)
                    {
                        rule.Priority = size - 1;
                        rules.Insert(size - 1, rule);
                        int i = 0;
                        foreach (EvaluationRule r in dbRules)
                        {
                            if (i == rule.Priority)
                                i++;
                            r.RuleGroupChecksum = newChecksum;
                            r.Priority = i;
                            rulesToUpdate.Add(r);
                            i++;
                        }
                    }
                    else
                    {
                        rule.Priority = 0;
                        rules.Insert(0, rule);
                        int i = 1;
                        foreach (EvaluationRule r in dbRules)
                        {
                            r.RuleGroupChecksum = newChecksum;
                            r.Priority = i;
                            rulesToUpdate.Add(r);
                            i++;
                        }
                    }
                }

                repository_.UpdateRange(rulesToUpdate);
                return await repository_.Add(rules, rule.RuleGroup);
            }
            catch (DbUpdateConcurrencyException)
            {
                List<ErrorDTO> errors = new List<ErrorDTO>();
                errors.Add(new ErrorDTO(null, Constants.OUT_OF_DATE_ERROR));
                throw new ConcurrencyException(dbRules, errors);
            }
        }

        /// <summary>
        /// Update Evaluation Rules.
        /// </summary>
        /// <exception cref="Exception">
        /// Throws if <paramref name="rulesToUpdate"/> RuleGroup is null or empty.
        /// </exception>
        /// <exception cref="ConcurrencyException">
        /// Throws if <paramref name="rulesToUpdate"/> RuleGroupChecksum doesn't equals database RuleGroupChecksum.
        /// </exception>
        /// <exception cref="InvalidRulesException">
        /// Throws if evaluation rules are invalid.
        /// </exception>
        /// <param name="rulesToUpdate">Rules to update.</param>
        /// <returns>
        /// Returns list of evaluation rules.
        /// </returns>
        public async Task<IEnumerable<EvaluationRule>> UpdateEvaluationRules(IEnumerable<EvaluationRule> rulesToUpdate)
        {
            EvaluationRule ruleToUpdate = rulesToUpdate.FirstOrDefault();

            string ruleGroup = ruleToUpdate?.RuleGroup;

            if (string.IsNullOrEmpty(ruleGroup))
                throw new Exception(Constants.DATA_ARE_EMPTY_ERROR);

            int? rulesToUpdateRuleGroupChecksum = ruleToUpdate.RuleGroupChecksum;

            IEnumerable<EvaluationRule> dbRules = await repository_.GetEvaluationRulesByRuleGroup(ruleGroup);

            try
            {
                int? RuleGroupChecksum = dbRules.FirstOrDefault()?.RuleGroupChecksum;

                if (RuleGroupChecksum == null || !RuleGroupChecksum.Equals(rulesToUpdateRuleGroupChecksum))
                    throw new DbUpdateConcurrencyException();

                EvaluationRule[] rulesToUpdateX =
                    dbRules
                    .Where(rX => !rulesToUpdate.Any(rY => rX.Id == rY.Id))
                    .ToArray();

                EvaluationRule[] rulesToUpdateY = rulesToUpdate.ToArray();

                int sizeX = rulesToUpdateX.Length;

                int sizeY = rulesToUpdateY.Length;

                int checksum = DateTime.Now.GetHashCode();

                for (int i = 0; i < (sizeX >= sizeY ? sizeX : sizeY); i++)
                {
                    if (i < sizeX)
                        rulesToUpdateX[i].RuleGroupChecksum = checksum;
                    if (i < sizeY)
                        rulesToUpdateY[i].RuleGroupChecksum = checksum;
                }

                rulesToUpdate = rulesToUpdateX.Concat(rulesToUpdateY);

                List<ErrorDTO> errors = Validator.ValidateRules(rulesToUpdate);
                if (errors.Count() != 0)
                    throw new InvalidRulesException(errors);

                return await repository_.Update(rulesToUpdate);
            }
            catch (DbUpdateConcurrencyException)
            {
                List<ErrorDTO> errors = new List<ErrorDTO>();
                errors.Add(new ErrorDTO(null, Constants.OUT_OF_DATE_ERROR));
                throw new ConcurrencyException(dbRules, errors);
            }
        }

        /// <summary>
        /// Delete Evaluation Rules.
        /// </summary>
        /// <exception cref="Exception">
        /// Throws if <paramref name="rulesToDelete"/> RuleGroup is null or empty.
        /// </exception>
        /// <exception cref="ConcurrencyException">
        /// Throws if <paramref name="rulesToDelete"/> RuleGroupChecksum doesn't equals database RuleGroupChecksum.
        /// </exception>
        /// <exception cref="InvalidRulesException">
        /// Throws if evaluation rules are invalid.
        /// </exception>
        /// <param name="rulesToDelete">Rules to delete.</param>
        /// <returns>
        /// Returns list of evaluation rules.
        /// </returns>
        public async Task<IEnumerable<EvaluationRule>> DeleteEvaluationRules(IEnumerable<EvaluationRule> rulesToDelete)
        {
            EvaluationRule ruleToDelete = rulesToDelete.FirstOrDefault();

            string ruleGroup = ruleToDelete?.RuleGroup;

            if (string.IsNullOrEmpty(ruleGroup))
                throw new Exception(Constants.DATA_ARE_EMPTY_ERROR);

            IEnumerable<EvaluationRule> dbRules = await repository_.GetEvaluationRulesByRuleGroup(ruleGroup);

            try
            {
                int? rulesToDeleteRuleGroupChecksum = ruleToDelete.RuleGroupChecksum;

                int? ruleGroupChecksum = dbRules.FirstOrDefault()?.RuleGroupChecksum;

                if (ruleGroupChecksum == null || !ruleGroupChecksum.Equals(rulesToDeleteRuleGroupChecksum))
                    throw new DbUpdateConcurrencyException();

                IEnumerable<EvaluationRule> rulesToUpdate =
                    dbRules
                    .Where(r1 => !rulesToDelete.Any(r2 => r1.Id == r2.Id));

                List<int> idsToDelete = rulesToDelete.Select(r => r.Id.Value).ToList();
                repository_.RemoveRange(repository_.EvaluationRules.Where(r => idsToDelete.Contains(r.Id.Value)));

                int checksum = DateTime.Now.GetHashCode();

                int i = 0;

                foreach (EvaluationRule r in rulesToUpdate)
                {
                    r.RuleGroupChecksum = checksum;
                    r.Priority = i;
                    i++;
                }

                List<ErrorDTO> errors = Validator.ValidateRules(rulesToUpdate);
                if (errors.Count() != 0)
                    throw new InvalidRulesException(errors);

                return await repository_.Update(rulesToUpdate);
            }
            catch (DbUpdateConcurrencyException)
            {
                List<ErrorDTO> errors = new List<ErrorDTO>();
                errors.Add(new ErrorDTO(null, Constants.OUT_OF_DATE_ERROR));
                throw new ConcurrencyException(dbRules, errors);
            }
        }
        #endregion
    }
}
