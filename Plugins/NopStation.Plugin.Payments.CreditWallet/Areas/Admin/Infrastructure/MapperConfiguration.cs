using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Payments.CreditWallet.Areas.Admin.Models;
using NopStation.Plugin.Payments.CreditWallet.Domain;

namespace NopStation.Plugin.Payments.CreditWallet.Areas.Admin.Infrastructure
{
    public class MapperConfiguration : Profile, IOrderedMapperProfile
    {
        public MapperConfiguration()
        {
            CreateMap<Wallet, WalletModel>()
                .ForMember(model => model.AvailableCredit, options => options.Ignore())
                .ForMember(model => model.WalletCustomerEmail, options => options.Ignore())
                .ForMember(model => model.WalletCustomerName, options => options.Ignore());
            CreateMap<WalletModel, Wallet>();

            CreateMap<ActivityHistory, ActivityHistoryModel>()
                .ForMember(model => model.CreatedByCustomer, options => options.Ignore())
                .ForMember(model => model.CreatedOn, options => options.Ignore())
                .ForMember(model => model.ActivityTypeStr, options => options.Ignore())
                .ForMember(model => model.WalletCustomerEmail, options => options.Ignore())
                .ForMember(model => model.WalletCustomerName, options => options.Ignore())
                .ForMember(model => model.CreditUsed, options => options.Ignore());
            CreateMap<ActivityHistoryModel, ActivityHistory>()
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore())
                .ForMember(entity => entity.CreatedByCustomerId, options => options.Ignore());

            CreateMap<InvoicePayment, InvoicePaymentModel>()
                .ForMember(model => model.CreatedByCustomerId, options => options.Ignore())
                .ForMember(model => model.UpdatedByCustomerId, options => options.Ignore())
                .ForMember(model => model.WalletCustomerEmail, options => options.Ignore())
                .ForMember(model => model.WalletCustomerName, options => options.Ignore())
                .ForMember(model => model.UpdatedByCustomer, options => options.Ignore())
                .ForMember(model => model.CreatedByCustomer, options => options.Ignore())
                .ForMember(model => model.PaymentDate, options => options.Ignore());
            CreateMap<InvoicePaymentModel, InvoicePayment>()
                .ForMember(entity => entity.PaymentDateUtc, options => options.Ignore())
                .ForMember(entity => entity.CreatedByCustomerId, options => options.Ignore())
                .ForMember(entity => entity.UpdatedByCustomerId, options => options.Ignore());
        }

        public int Order => 1;
    }
}
