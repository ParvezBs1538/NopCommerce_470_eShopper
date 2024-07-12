using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Nop.Services.Localization;
using Nop.Web.Framework.Models;
using NopStation.Plugin.Misc.Core.Extensions;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Misc.Core.Helpers;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.Core.Controllers;

[NopStationApiLicense]
public class NopStationApiController : Controller
{
    /// <summary>
    /// Returns Ok response (200 status code)
    /// </summary>
    /// <typeparam name="T">Type of 'BaseNopModel'</typeparam>
    /// <param name="baseModel">The main model</param>
    /// <param name="message">The success message</param>
    /// <param name="errors">The error list</param>
    /// <param name="defaultMessage">'true' value indicates it will send a default success message with the response.</param>
    /// <returns></returns>
    public IActionResult OkWrap<T>(T baseModel, string message = null, IList<string> errors = null, bool defaultMessage = false) where T : BaseNopModel
    {
        var model = new GenericResponseModel<T>();
        model.Data = baseModel;
        if (defaultMessage && string.IsNullOrWhiteSpace(message))
            message = NopInstance.Load<ILocalizationService>().GetResourceAsync("NopStation.Core.Request.Common.Ok").Result;
        model.Message = message;
        if (errors != null && errors.Any())
            model.ErrorList.AddRange(errors);
        return base.Ok(model);
    }

    /// <summary>
    /// Returns Ok response (200 status code)
    /// </summary>
    /// <param name="message">The success message</param>
    /// <param name="errors">The error list</param>
    /// <param name="defaultMessage">'true' value indicates it will send a default success message with the response.</param>
    /// <returns></returns>
    public IActionResult Ok(string message = null, IList<string> errors = null, bool defaultMessage = false)
    {
        var model = new BaseResponseModel();
        if (defaultMessage && string.IsNullOrWhiteSpace(message))
            message = NopInstance.Load<ILocalizationService>().GetResourceAsync("NopStation.Core.Request.Common.Ok").Result;
        model.Message = message;
        if (errors != null && errors.Any())
            model.ErrorList.AddRange(errors);
        return base.Ok(model);
    }

    /// <summary>
    /// Returns Created response (201 status code)
    /// </summary>
    /// <param name="id">The entity id</param>
    /// <param name="message">The success message</param>
    /// <param name="errors">The error list</param>
    /// <param name="defaultMessage">'true' value indicates it will send a default success message with the response.</param>
    /// <returns></returns>
    public IActionResult Created(int id, string message = null, IList<string> errors = null, bool defaultMessage = true)
    {
        var model = new GenericResponseModel<int>();
        model.Data = id;
        if (defaultMessage && string.IsNullOrWhiteSpace(message))
            message = NopInstance.Load<ILocalizationService>().GetResourceAsync("NopStation.Core.Request.Common.Ok").Result;
        model.Message = message;
        if (errors != null && errors.Any())
            model.ErrorList.AddRange(errors);
        return base.StatusCode(StatusCodes.Status201Created, model);
    }

    /// <summary>
    /// Returns BadRequest response (400 status code)
    /// </summary>
    /// <typeparam name="T">Type of 'BaseNopModel'</typeparam>
    /// <param name="baseModel">The main model</param>
    /// <param name="modelState">The model state of the request (ModelState)</param>
    /// <param name="errors">The error list</param>
    /// <param name="showDefaultMessageIfEmpty">'true' value indicates it will send a default success message with the response.</param>
    /// <returns></returns>
    public IActionResult BadRequestWrap<T>(T baseModel, ModelStateDictionary modelState = null, IList<string> errors = null, bool showDefaultMessageIfEmpty = true) where T : BaseNopModel
    {
        var model = new GenericResponseModel<T>();
        model.Data = baseModel;
        if (modelState != null && !modelState.IsValid)
            model.ErrorList.AddRange(modelState.GetErrors());
        if (errors != null && errors.Any())
            model.ErrorList.AddRange(errors);
        if (!model.ErrorList.Any() && showDefaultMessageIfEmpty)
            model.ErrorList.Add(NopInstance.Load<ILocalizationService>().GetResourceAsync("NopStation.Core.Request.Common.BadRequest").Result);
        return base.BadRequest(model);
    }

    /// <summary>
    /// Returns BadRequest response (400 status code)
    /// </summary>
    /// <param name="error">Single error message</param>
    /// <param name="errors">The error list</param>
    /// <param name="defaultMessage">'true' value indicates it will send a default success message with the response.</param>
    /// <returns></returns>
    public IActionResult BadRequest(string error = null, IList<string> errors = null, bool defaultMessage = true)
    {
        var model = new BaseResponseModel();
        if (!string.IsNullOrWhiteSpace(error))
            model.ErrorList.Add(error);
        if (errors != null && errors.Any())
            model.ErrorList.AddRange(errors);
        if (!model.ErrorList.Any() && defaultMessage)
            model.ErrorList.Add(NopInstance.Load<ILocalizationService>().GetResourceAsync("NopStation.Core.Request.Common.BadRequest").Result);
        return base.BadRequest(model);
    }

    /// <summary>
    /// Returns Unauthorized response (401 status code)
    /// </summary>
    /// <typeparam name="T">Type of 'BaseNopModel'</typeparam>
    /// <param name="baseModel">The main model</param>
    /// <param name="error">Single error message</param>
    /// <param name="errors">The error list</param>
    /// <param name="defaultMessage">'true' value indicates it will send a default success message with the response.</param>
    /// <returns></returns>
    public IActionResult UnauthorizedWrap<T>(T baseModel, string error = null, IList<string> errors = null, bool defaultMessage = true) where T : BaseNopModel
    {
        var model = new GenericResponseModel<T>();
        model.Data = baseModel;
        if (!string.IsNullOrWhiteSpace(error))
            model.ErrorList.Add(error);
        if (errors != null && errors.Any())
            model.ErrorList.AddRange(errors);
        if (!model.ErrorList.Any() && defaultMessage)
            model.ErrorList.Add(NopInstance.Load<ILocalizationService>().GetResourceAsync("NopStation.Core.Request.Common.Unauthorized").Result);
        return base.Unauthorized(model);
    }

    /// <summary>
    /// Returns Unauthorized response (401 status code)
    /// </summary>
    /// <param name="error">Single error message</param>
    /// <param name="errors">The error list</param>
    /// <param name="defaultMessage">'true' value indicates it will send a default success message with the response.</param>
    /// <returns></returns>
    public IActionResult Unauthorized(string error = null, IList<string> errors = null, bool defaultMessage = true)
    {
        var model = new BaseResponseModel();
        if (!string.IsNullOrWhiteSpace(error))
            model.ErrorList.Add(error);
        if (errors != null && errors.Any())
            model.ErrorList.AddRange(errors);
        if (!model.ErrorList.Any() && defaultMessage)
            model.ErrorList.Add(NopInstance.Load<ILocalizationService>().GetResourceAsync("NopStation.Core.Request.Common.Unauthorized").Result);
        return base.Unauthorized(model);
    }

    /// <summary>
    /// Returns NotFound response (404 status code)
    /// </summary>
    /// <typeparam name="T">Type of 'BaseNopModel'</typeparam>
    /// <param name="baseModel">The main model</param>
    /// <param name="error">Single error message</param>
    /// <param name="errors">The error list</param>
    /// <param name="defaultMessage">'true' value indicates it will send a default success message with the response.</param>
    /// <returns></returns>
    public IActionResult NotFoundWrap<T>(T baseModel, string error = null, IList<string> errors = null, bool defaultMessage = true) where T : BaseNopModel
    {
        var model = new GenericResponseModel<T>();
        model.Data = baseModel;
        if (!string.IsNullOrWhiteSpace(error))
            model.ErrorList.Add(error);
        if (errors != null && errors.Any())
            model.ErrorList.AddRange(errors);
        if (!model.ErrorList.Any() && defaultMessage)
            model.ErrorList.Add(NopInstance.Load<ILocalizationService>().GetResourceAsync("NopStation.Core.Request.Common.NotFound").Result);
        return base.NotFound(model);
    }

    /// <summary>
    /// Returns NotFound response (404 status code)
    /// </summary>
    /// <param name="error">Single error message</param>
    /// <param name="errors">The error list</param>
    /// <param name="defaultMessage">'true' value indicates it will send a default success message with the response.</param>
    /// <returns></returns>
    public IActionResult NotFound(string error = null, IList<string> errors = null, bool defaultMessage = true)
    {
        var model = new BaseResponseModel();
        if (!string.IsNullOrWhiteSpace(error))
            model.ErrorList.Add(error);
        if (errors != null && errors.Any())
            model.ErrorList.AddRange(errors);
        if (!model.ErrorList.Any() && defaultMessage)
            model.ErrorList.Add(NopInstance.Load<ILocalizationService>().GetResourceAsync("NopStation.Core.Request.Common.NotFound").Result);
        return base.NotFound(model);
    }

    /// <summary>
    /// Returns InternalServerError response (500 status code)
    /// </summary>
    /// <typeparam name="T">Type of 'BaseNopModel'</typeparam>
    /// <param name="baseModel">The main model</param>
    /// <param name="error">Single error message</param>
    /// <param name="errors">The error list</param>
    /// <param name="defaultMessage">'true' value indicates it will send a default success message with the response.</param>
    /// <returns></returns>
    public IActionResult InternalServerErrorWrap<T>(T baseModel, string error = null, IList<string> errors = null, bool defaultMessage = true) where T : BaseNopModel
    {
        var model = new GenericResponseModel<T>();
        model.Data = baseModel;
        if (!string.IsNullOrWhiteSpace(error))
            model.ErrorList.Add(error);
        if (errors != null && errors.Any())
            model.ErrorList.AddRange(errors);
        if (!model.ErrorList.Any() && defaultMessage)
            model.ErrorList.Add(NopInstance.Load<ILocalizationService>().GetResourceAsync("NopStation.Core.Request.Common.InternalServerError").Result);
        return base.StatusCode(StatusCodes.Status500InternalServerError, model);
    }

    /// <summary>
    /// Returns InternalServerError response (500 status code)
    /// </summary>
    /// <param name="error">Single error message</param>
    /// <param name="errors">The error list</param>
    /// <param name="defaultMessage">'true' value indicates it will send a default success message with the response.</param>
    /// <returns></returns>
    public IActionResult InternalServerError(string error = null, IList<string> errors = null, bool defaultMessage = true)
    {
        var model = new BaseResponseModel();
        if (!string.IsNullOrWhiteSpace(error))
            model.ErrorList.Add(error);
        if (errors != null && errors.Any())
            model.ErrorList.AddRange(errors);
        if (!model.ErrorList.Any() && defaultMessage)
            model.ErrorList.Add(NopInstance.Load<ILocalizationService>().GetResourceAsync("NopStation.Core.Request.Common.InternalServerError").Result);
        return base.StatusCode(StatusCodes.Status500InternalServerError, model);
    }

    /// <summary>
    /// Returns MethodNotAllowed response (405 status code)
    /// </summary>
    /// <returns></returns>
    public IActionResult MethodNotAllowed()
    {
        return base.StatusCode(StatusCodes.Status405MethodNotAllowed);
    }

    /// <summary>
    /// Returns LengthRequired response (411 status code)
    /// </summary>
    /// <returns></returns>
    public IActionResult LengthRequired()
    {
        return base.StatusCode(StatusCodes.Status411LengthRequired);
    }
}
