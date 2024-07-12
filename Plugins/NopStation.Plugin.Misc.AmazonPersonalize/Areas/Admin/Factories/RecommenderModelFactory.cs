using System;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Personalize.Model;
using Nop.Services.Helpers;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Misc.AmazonPersonalize.Areas.Admin.Models;
using NopStation.Plugin.Misc.AmazonPersonalize.Services;

namespace NopStation.Plugin.Misc.AmazonPersonalize.Areas.Admin.Factories
{
    public class RecommenderModelFactory : IRecommenderModelFactory
    {
        #region Fields

        private readonly IRecommenderService _recommenderService;
        private readonly IDateTimeHelper _dateTimeHelper;

        #endregion Fields

        #region Ctor

        public RecommenderModelFactory(IRecommenderService recommenderService,
            IDateTimeHelper dateTimeHelper)
        {
            _recommenderService = recommenderService;
            _dateTimeHelper = dateTimeHelper;
        }

        #endregion Ctor

        #region Methods

        public async Task<RecommenderListModel> PrepareRecommenderListModelAsync(RecommenderSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));
            var recommenders = (await _recommenderService.GetRecommendersAsync(searchModel)).ToPagedList(searchModel);

            //prepare list model
            var model = new RecommenderListModel().PrepareToGridAsync(searchModel, recommenders, () =>
            {
                return recommenders.SelectAwait(async recommender =>
                {
                    return await PrepareRecommenderModelAsync(null, recommender);
                });
            });
            return await model;
        }

        public async Task<RecommenderModel> PrepareRecommenderModelAsync(RecommenderModel model, RecommenderSummary recommenderSummary)
        {
            if (recommenderSummary != null)
            {
                if (model == null)
                {
                    model = new RecommenderModel();
                    model.Name = recommenderSummary.Name;
                    model.RecommenderArn = recommenderSummary.RecommenderArn;
                    model.CreationDateTime = await _dateTimeHelper.ConvertToUserTimeAsync(recommenderSummary.CreationDateTime, DateTimeKind.Utc);
                    model.DatasetGroupArn = recommenderSummary.DatasetGroupArn;
                    model.RecipeArn = recommenderSummary.RecipeArn;
                    model.Status = recommenderSummary.Status;
                    model.LastUpdatedDateTime = recommenderSummary.LastUpdatedDateTime;
                }
            }
            return model;
        }

        #endregion Methods
    }
}