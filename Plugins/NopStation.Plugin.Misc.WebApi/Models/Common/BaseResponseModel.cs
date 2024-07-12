using System.Collections.Generic;

namespace Nop.Plugin.NopStation.WebApi.Models.Common
{
    public class BaseResponseModel
    {
        public BaseResponseModel()
        {
            ErrorList = new List<string>();
        }

        public string Message { get; set; }

        public List<string> ErrorList { get; set; }
    }
}
