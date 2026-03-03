using Payments.Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace Payments.Application.Common.Interfaces;

public interface IPaymentsDbContext
{
    DbSet<Payment> Payments { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}