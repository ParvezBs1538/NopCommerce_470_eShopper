using System.Threading.Tasks;
using Amazon.Personalize;
using NopStation.Plugin.Misc.AmazonPersonalize.Areas.Admin.Models;

namespace NopStation.Plugin.Misc.AmazonPersonalize.Factories
{
    public interface IAmazonPersonalizeHelperFactory
    {
        Task UploadInteractionsAsync(UploadInteractionsDataModel model);

        AmazonPersonalizeClient GetAmazonPersonalizeClient();
    }
}