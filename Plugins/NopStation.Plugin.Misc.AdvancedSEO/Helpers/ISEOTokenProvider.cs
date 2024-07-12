using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Services.Messages;

namespace NopStation.Plugin.Misc.AdvancedSEO.Helpers
{
    public interface ISEOTokenProvider
    {
        Task<IList<string>> GetListOfAllowedTokensAsync(string tokenGroup = null);

        IEnumerable<string> GetTokenGroups(string templateProperty);

        #region Category

        //Task AddCategoryTitleTokensAsync(IList<Token> tokens, Category category, int languageId);
        
        Task AddCategoryMetaTitleTokensAsync(IList<Token> tokens, Category category, int languageId);

        Task AddCategoryKeywordsTokensAsync(IList<Token> tokens, Category category, int languageId);

        //Task AddCategoryDescriptionTokensAsync(IList<Token> tokens, Category category, int languageId);

        Task AddCategoryMetaDescriptionTokensAsync(IList<Token> tokens, Category category, int languageId);

        #endregion

        #region Manufacturer

        Task AddManufacturerMetaTitleTokensAsync(IList<Token> tokens, Manufacturer manufacturer, int languageId);

        Task AddManufacturerKeywordsTokensAsync(IList<Token> tokens, Manufacturer manufacturer, int languageId);

        Task AddManufacturerMetaDescriptionTokensAsync(IList<Token> tokens, Manufacturer manufacturer, int languageId);

        #endregion

        #region Product

        Task AddProductMetaTitleTokensAsync(IList<Token> tokens, Product product, int languageId);

        Task AddProductKeywordsTokensAsync(IList<Token> tokens, Product product, int languageId);

        Task AddProductMetaDescriptionTokensAsync(IList<Token> tokens, Product product, int languageId);

        #endregion


    }
}
