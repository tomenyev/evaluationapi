using EvaluationAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EvaluationAPI.Repository
{
    /// <summary>
    /// Evaluation Rules repository - Database context.
    /// </summary>
    public class EvaluationRulesRepository : DbContext
    {
        #region Public Properties - DbSet
        public DbSet<EvaluationRule> EvaluationRules { get; set; }
        #endregion

        #region Constructor
        public EvaluationRulesRepository(DbContextOptions<EvaluationRulesRepository> options) : base(options)
        {
        }
        #endregion

        #region Protected Methods
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<EvaluationRule>(entity =>
            {
                entity.ToTable("EvaluationRuleAPI");

                entity.HasIndex(e => new { e.RuleGroup, e.Priority }, "IX_EvaluationRuleT")
                    .IsUnique();

                entity.Property(e => e.Id).HasComment("Unique ID for Rule");

                entity.Property(e => e.ComponentSourceAddress).HasComment("Component Address");

                entity.Property(e => e.FaultCode).HasComment("The Eaton fault code that the SPN/FMI has been interpreted as.");

                entity.Property(e => e.FaultSourceAddress).HasComment("The Source Address of the component with the fault.");

                entity.Property(e => e.Fmi)
                    .HasColumnName("FMI")
                    .HasComment("The FMI of the fault code");

                entity.Property(e => e.IsActive).HasComment("1 if the fault is active, 0 otherwise");

                entity.Property(e => e.IsEaton).HasComment("1 if the component is an Eaton product, 0 otherwise");

                entity.Property(e => e.IsPrimaryFault).HasComment("If true, this rule is also used as a filter to select which faults in a SAR should be marked as the primary root cause for the evaluation");

                entity.Property(e => e.OriginType)
                    .HasMaxLength(3)
                    .HasComment("SAR Origin Type - SR4, OTS, etc.");

                entity.Property(e => e.Prefix)
                    .HasMaxLength(30)
                    .HasComment("Used for writing Complex Rules for doing logical operation with other Rule.");

                entity.Property(e => e.Priority).HasComment(" Result Priority of Rule for Evaluation");

                entity.Property(e => e.ProductCode).HasComment("The Product Code for the component");

                entity.Property(e => e.ProductFamilyId).HasComment("The Id of the Product Family this Component belongs to, if this is an Eaton product.");

                entity.Property(e => e.ResultKey)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasComment("Evalution Result Key - Template Key or Evaluation Sheet Name");

                entity.Property(e => e.ResultType).HasComment("Evaluation Result Type - 1 - Evaluation, 2 - EvaluationRoot, 3 - ActionPlan");

                entity.Property(e => e.RuleGroup)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasComment("Name of the RuleGroup (Name of the sheet from the excel spreadsheet)");

                entity.Property(e => e.Spn)
                    .HasMaxLength(6)
                    .HasColumnName("SPN")
                    .HasComment("The PID/SID/SPN (Protocol Specific Parameter)");

                entity.Property(e => e.Suffix)
                    .HasMaxLength(30)
                    .HasComment("Used for writing Complex Rules for doing logical operation with other Rule.");

                entity.Property(e => e.RuleGroupChecksum)
                    .IsRequired();
            });

            base.OnModelCreating(modelBuilder);
        }
        #endregion

        #region Public Methods - Get Content
        /// <summary>
        /// Gets all evaluation rules.
        /// </summary>
        /// <returns>Returns list of evaluation rules.</returns>
        public async Task<IEnumerable<EvaluationRule>> GetEvaluationRules()
        {
            return await EvaluationRules
                .AsNoTrackingWithIdentityResolution()
                .OrderBy(r => r.Priority)
                .ToListAsync();
        }

        /// <summary>
        /// Gets list of evaluation rules by Rule Group.
        /// </summary>
        /// <param name="ruleGroup">Rule Group to filter.</param>
        /// <returns>Returns a list of evaluation rules.</returns>
        public async Task<IEnumerable<EvaluationRule>> GetEvaluationRulesByRuleGroup(string ruleGroup)
        {
            return await EvaluationRules
                .AsNoTrackingWithIdentityResolution()
                .Where(r => r.RuleGroup.Equals(ruleGroup))
                .OrderBy(r => r.Priority)
                .ToListAsync();
        }

        /// <summary>
        /// Check if Rule Group exists.
        /// </summary>
        /// <param name="ruleGroup">RuleGroup to be checked.</param>
        /// <returns>
        /// Returns true if <paramref name="ruleGroup"/> exists.
        /// Returns false if <paramref name="ruleGroup"/> dosn't exists.
        /// </returns>
        public async Task<bool> IsRuleGroupExists(string ruleGroup)
        {
            return await EvaluationRules
               .AsNoTrackingWithIdentityResolution()
               .Where(r => r.RuleGroup.Equals(ruleGroup))
               .OrderBy(r => r.Priority)
               .FirstOrDefaultAsync() != null;
        }

        /// <summary>
        /// Gets list of Rule Groups which are present in the database.
        /// </summary>
        /// <returns>Returns a list of Rule Groups.</returns>
        public async Task<string[]> GetRuleGroups()
        {
            return await EvaluationRules
                .AsNoTrackingWithIdentityResolution()
                .Select(r => r.RuleGroup)
                .Distinct()
                .ToArrayAsync();
        }
        #endregion

        #region Public Methods - Add Content
        /// <summary>
        /// Add new rules to the database.
        /// </summary>
        /// <param name="rules">Rules to be added.</param>
        /// <param name="ruleGroup">Rule Group to be returned.</param>
        /// <returns>Returns a list of rules.</returns>
        public async Task<IEnumerable<EvaluationRule>> Add(IEnumerable<EvaluationRule> rules, string ruleGroup)
        {
            await EvaluationRules.AddRangeAsync(rules);

            await SaveChangesAsync();

            return await GetEvaluationRulesByRuleGroup(ruleGroup);
        }
        #endregion

        #region Public Methods - Update Content
        /// <summary>
        /// Updates rules in the database.
        /// </summary>
        /// <param name="rules">Rules to be updated.</param>
        /// <returns>Returnes updated list of rules.</returns>
        public async Task<IEnumerable<EvaluationRule>> Update(IEnumerable<EvaluationRule> rules)
        {
            EvaluationRules.UpdateRange(rules);

            await SaveChangesAsync();

            return await GetEvaluationRulesByRuleGroup(rules.FirstOrDefault()?.RuleGroup);
        }
        #endregion

        #region Public Methods - Delete Content
        /// <summary>
        /// Deletes all rules with RuleGroup equals <paramref name="ruleGroup"/>.
        /// </summary>
        /// <param name="ruleGroup">RuleGroup to delete.</param>
        /// <returns>Returns a list of rule groups.</returns>
        public async Task<string[]> DeleteAllByRuleGroup(string ruleGroup)
        {
            IEnumerable<EvaluationRule> rules = await GetEvaluationRulesByRuleGroup(ruleGroup);

            EvaluationRules.RemoveRange(rules);

            int? result = await SaveChangesAsync();

            return await this.GetRuleGroups();
        }

        /// <summary>
        /// Deletes all rules.
        /// </summary>
        /// <returns>Returns count of deleted rules.</returns>
        public async Task<int?> DeleteAll()
        {
            EvaluationRules.RemoveRange(EvaluationRules);

            int? result = await SaveChangesAsync();

            return result;
        }
        #endregion
    }
}
