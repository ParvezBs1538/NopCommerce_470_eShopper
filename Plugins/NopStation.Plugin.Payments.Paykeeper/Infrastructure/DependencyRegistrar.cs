﻿//using Microsoft.Extensions.DependencyInjection;
//using Nop.Core.Configuration;
//using Nop.Core.Infrastructure;
//using Nop.Core.Infrastructure.DependencyManagement;
//using NopStation.Plugin.Payments.Paykeeper.Services;

//namespace NopStation.Plugin.Payments.Paykeeper.Infrastructure
//{
//    /// <summary>
//    /// Represents a plugin dependency registrar
//    /// </summary>
//    public class DependencyRegistrar : IDependencyRegistrar
//    {
//        /// <summary>
//        /// Register services and interfaces
//        /// </summary>
//        /// <param name="services">Collection of service descriptors</param>
//        /// <param name="typeFinder">Type finder</param>
//        /// <param name="appSettings">App settings</param>
//        public virtual void Register(IServiceCollection services, ITypeFinder typeFinder, AppSettings appSettings)
//        {
//        }

//        /// <summary>
//        /// Order of this dependency registrar implementation
//        /// </summary>
//        public int Order => 1;
//    }
//}