using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Shipping;
using Nop.Services;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Shipping;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Widgets.DMS.Areas.Admin.Models;
using NopStation.Plugin.Widgets.DMS.Domain;
using NopStation.Plugin.Widgets.DMS.Services;

namespace NopStation.Plugin.Widgets.DMS.Areas.Admin.Factories
{
    public class CourierShipmentModelFactory : ICourierShipmentModelFactory
    {
        #region Fields

        private readonly ICourierShipmentService _courierShipmentService;
        private readonly IShipmentPickupPointService _shipmentPickupPointService;
        private readonly IShipperService _shipperService;
        private readonly IShipmentService _shipmentService;
        private readonly ICustomerService _customerService;
        private readonly IPictureService _pictureService;
        private readonly ILocalizationService _localizationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IProofOfDeliveryDataService _proofOfDeliveryDataService;

        #endregion

        #region Ctor

        public CourierShipmentModelFactory(ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService,
            IPictureService pictureService,
            ICourierShipmentService courierShipmentService,
            IShipmentPickupPointService shipmentPickupPointService,
            IShipperService shipperService,
            IShipmentService shipmentService,
            IProofOfDeliveryDataService proofOfDeliveryDataService)
        {
            _pictureService = pictureService;
            _dateTimeHelper = dateTimeHelper;
            _localizationService = localizationService;
            _customerService = customerService;
            _courierShipmentService = courierShipmentService;
            _shipmentPickupPointService = shipmentPickupPointService;
            _shipperService = shipperService;
            _shipmentService = shipmentService;
            _proofOfDeliveryDataService = proofOfDeliveryDataService;
        }

        #endregion

        #region Methods

        public async Task<CourierShipmentModel> PrepareCourierShipmentModelAsync(CourierShipmentModel model,
            CourierShipment courierShipment, Shipment shipment, bool excludeProperties = false)
        {
            if (courierShipment != null)
            {
                if (model == null)
                {
                    var shipper = await _shipperService.GetShipperByIdAsync(courierShipment.ShipperId);
                    var sc = await _customerService.GetCustomerByIdAsync(shipper.CustomerId);

                    model = new CourierShipmentModel
                    {
                        Id = courierShipment.Id,
                        ShipperName = $"{await _customerService.GetCustomerFullNameAsync(sc)} ({sc.Email})",
                        ShipperId = courierShipment.ShipperId,
                        ShipperNopCustomerId = sc.Id,
                        //SignaturePictureId = courierShipment.SignaturePictureId,
                        CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(courierShipment.CreatedOnUtc, DateTimeKind.Utc),
                        UpdatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(courierShipment.UpdatedOnUtc, DateTimeKind.Utc),
                        //SignaturePictureUrl = "",
                        ShipmentStatusId = courierShipment.ShipmentStatusId,
                        ShipmentStatus = await _localizationService.GetResourceAsync($"Admin.NopStation.DMS.CourierShipments.Fields.ShipmentStatus.ShipmentStatusTypes.{courierShipment.ShipmentStatusId}"),
                        ShipmentPickupPointId = courierShipment.ShipmentPickupPointId,
                        TrackingNumber = shipment?.TrackingNumber ?? ""
                    };
                }

                model.Delivered = shipment.DeliveryDateUtc.HasValue;
            }

            model ??= new CourierShipmentModel();
            model.ShipmentId = shipment.Id;
            if (model.Delivered && courierShipment != null)
            {
                var shipmentPoD = await _proofOfDeliveryDataService.GetProofOfDeliveryDataByCourierShipmentIdAsync(courierShipment.Id);
                if (shipmentPoD != null)
                {
                    model.ProofOfDeliveryType = shipmentPoD.ProofOfDeliveryType.ToString();
                    if (shipmentPoD.ProofOfDeliveryType == ProofOfDeliveryTypes.PhotoOfCourier
                        || shipmentPoD.ProofOfDeliveryType == ProofOfDeliveryTypes.CustomersSignature)
                    {
                        var podPhoto = await _pictureService.GetPictureByIdAsync(shipmentPoD.ProofOfDeliveryReferenceId);
                        if (podPhoto != null)
                        {
                            model.PODContainPhoto = true;
                            model.PODPhotoUrl = await _pictureService.GetPictureUrlAsync(podPhoto.Id, 300);
                        }
                    }
                }
            }
            if (!excludeProperties)
            {
                var shippers = await _shipperService.GetAllShippersAsync(active: true);
                var customers = await _customerService.GetCustomersByIdsAsync(shippers.Select(x => x.CustomerId).ToArray());
                foreach (var customer in customers)
                {
                    model.AvailableShippers.Add(new SelectListItem
                    {
                        Text = $"{await _customerService.GetCustomerFullNameAsync(customer)} ({customer.Email})",
                        Value = shippers.First(x => x.CustomerId == customer.Id).Id.ToString()
                    });
                }

                model.AvailableShippers = model.AvailableShippers.OrderBy(x => x.Text).ToList();

                model.AvailableShipmentStatusTypes = (await ShipmentStatusTypes.AssignedToShipper.ToSelectListAsync(false))
                    .Select(item => new SelectListItem(item.Text, item.Value))
                    .ToList();

                //model.AvailablePickupPoints.Add(new SelectListItem(await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.SelectPickupPoint"), "0"));

                var pickupPoints = await _shipmentPickupPointService.GetAllShipmentPickupPointsAsync();
                if (pickupPoints != null && pickupPoints.Count > 0)
                {
                    model.AvailablePickupPoints = pickupPoints.Select(pickupPoint =>
                    {
                        return (new SelectListItem
                        {
                            Text = pickupPoint.Name,
                            Value = pickupPoint.Id.ToString(),
                            //Selected = pickupPoint.Id == courierShipment.ShipmentPickupPointId
                        });
                    }).ToList();
                }
                model.AvailablePickupPoints.Insert(0, new SelectListItem()
                {
                    Value = "",
                    Text = await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.SelectPickupPoint")
                });
            }

            return model;
        }

        public async Task<CourierShipmentSearchModel> PrepareCourierShipmentSearchModelAsync(CourierShipmentSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var shippers = await _shipperService.GetAllShippersAsync(active: true);
            var customers = await _customerService.GetCustomersByIdsAsync(shippers.Select(x => x.CustomerId).ToArray());
            foreach (var customer in customers)
            {
                searchModel.AvailableShippers.Add(new SelectListItem
                {
                    Text = $"{await _customerService.GetCustomerFullNameAsync(customer)} ({customer.Email})",
                    Value = shippers.First(x => x.CustomerId == customer.Id).Id.ToString()
                });
            }

            searchModel.AvailableShippers = searchModel.AvailableShippers.OrderBy(x => x.Text).ToList();
            searchModel.AvailableShippers.Insert(0, new SelectListItem()
            {
                Text = await _localizationService.GetResourceAsync("Admin.NopStation.DMS.CourierShipments.List.SearchShipperId.All"),
                Value = "0"
            });

            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual async Task<CourierShipmentListModel> PrepareCourierShipmentListModelAsync(CourierShipmentSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var courierShipments = await _courierShipmentService.GetAllCourierShipmentsAsync(
                                   shipperId: searchModel.SearchShipperId,
                                   trackingNumber: searchModel.SearchShipmentTrackingNumber,
                                   customOrderNumber: searchModel.SearchCustomOrderNumber,
                                   orderId: searchModel.SearchOrderId,
                                   shipmentId: searchModel.SearchShipmentId,
                                   pageIndex: searchModel.Page - 1,
                                   pageSize: searchModel.PageSize);

            var model = await new CourierShipmentListModel().PrepareToGridAsync(searchModel, courierShipments, () =>
            {
                return courierShipments.SelectAwait(async courierShipment =>
                {
                    var shipment = await _shipmentService.GetShipmentByIdAsync(courierShipment.ShipmentId);
                    return await PrepareCourierShipmentModelAsync(null, courierShipment, shipment, true);
                });
            });

            return model;
        }

        #endregion
    }
}
