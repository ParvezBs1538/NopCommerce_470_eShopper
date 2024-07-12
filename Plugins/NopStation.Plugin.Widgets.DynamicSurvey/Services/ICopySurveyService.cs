using System.Threading.Tasks;
using NopStation.Plugin.Widgets.DynamicSurvey.Domain;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Services
{
    public interface ICopySurveyService
    {
        Task<Survey> CopySurveyAsync(Survey survey, string newName, bool isPublished = true);
    }
}