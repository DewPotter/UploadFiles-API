using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using UploadFiles.Models;

namespace UploadFiles.Data
{
    public partial class TransactionDataContext : DbContext
    {
        public TransactionDataContext(DbContextOptions<TransactionDataContext> options)
           : base(options)
        {

        }

        public virtual DbSet<TransactionData> TransactionData { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TransactionData>(entity =>
            {
                entity.HasKey(k => k.ID);
                entity.ToTable("TRANSACTION_DATA");
            });

            OnModelCreatingPartial(modelBuilder);
        }
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
