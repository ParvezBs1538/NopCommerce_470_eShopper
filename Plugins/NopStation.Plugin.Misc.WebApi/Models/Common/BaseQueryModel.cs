using System;
using System.Collections.Generic;

namespace Nop.Plugin.NopStation.WebApi.Models.Common
{
    public class BaseQueryModel<TModel>
    {
        public BaseQueryModel()
        {
            Type t = typeof(TModel);
            if (t.GetConstructor(Type.EmptyTypes) != null)
                Data = Activator.CreateInstance<TModel>();

            FormValues = new List<KeyValueApi>();
            UploadPicture = new PictureQueryModel();
        }

        public TModel Data { get; set; }
        public List<KeyValueApi> FormValues { get; set; }
        public PictureQueryModel UploadPicture { get; set; }
    }
}
