using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Widgets.SEOExpert.Domains;

namespace NopStation.Plugin.Widgets.SEOExpert.Services
{
    public interface ISEOExpertService
    {
        Task<SEOContent> GenerateSEOAsync<TEntity>(TEntity entity) where TEntity : BaseEntity;
        Task GenerateAndUpdateSEOAsync<TEntity>(IEnumerable<TEntity> entities) where TEntity : BaseEntity;
    }
}