using EvaluationAPI.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace EvaluationAPI.Repository
{
    /// <summary>
    /// Auth repository - Database context.
    /// </summary>
    public partial class AuthRepository : IdentityDbContext<EvaluationUser>
    {
        #region Public Constructors
        public AuthRepository()
        {
        }

        public AuthRepository(DbContextOptions<AuthRepository> options) : base(options)
        {
        }
        #endregion

        #region Methods
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
        #endregion
    }
}
