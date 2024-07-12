using System;

namespace Nop.Plugin.NopStation.WebApi.Models.Common
{
    public class GenericResponseModel<TResult> : BaseResponseModel
    {
        public GenericResponseModel()
        {
            Type t = typeof(TResult);
            if (t.GetConstructor(Type.EmptyTypes) != null)
                Data = Activator.CreateInstance<TResult>();
        }

        public TResult Data { get; set; }
    }
}
