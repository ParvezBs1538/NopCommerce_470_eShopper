﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     //
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DBBL_WebService_Live
{
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("dotnet-svcutil", "1.0.0.1")]
    [System.ServiceModel.ServiceContractAttribute(Namespace="http://ecom.dbbl/", ConfigurationName="DBBL_WebService_Live.dbblecomtxn")]
    public interface dbblecomtxn
    {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://ecom.dbbl/dbblecomtxn/getsubmertransidRequest", ReplyAction="http://ecom.dbbl/dbblecomtxn/getsubmertransidResponse")]
        System.Threading.Tasks.Task<DBBL_WebService_Live.getsubmertransidResponse> getsubmertransidAsync(DBBL_WebService_Live.getsubmertransidRequest request);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://ecom.dbbl/dbblecomtxn/getresultfieldRequest", ReplyAction="http://ecom.dbbl/dbblecomtxn/getresultfieldResponse")]
        System.Threading.Tasks.Task<DBBL_WebService_Live.getresultfieldResponse> getresultfieldAsync(DBBL_WebService_Live.getresultfieldRequest request);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://ecom.dbbl/dbblecomtxn/getransidRequest", ReplyAction="http://ecom.dbbl/dbblecomtxn/getransidResponse")]
        System.Threading.Tasks.Task<DBBL_WebService_Live.getransidResponse> getransidAsync(DBBL_WebService_Live.getransidRequest request);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://ecom.dbbl/dbblecomtxn/getresultRequest", ReplyAction="http://ecom.dbbl/dbblecomtxn/getresultResponse")]
        System.Threading.Tasks.Task<DBBL_WebService_Live.getresultResponse> getresultAsync(DBBL_WebService_Live.getresultRequest request);
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("dotnet-svcutil", "1.0.0.1")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(IsWrapped=false)]
    public partial class getsubmertransidRequest
    {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Name="getsubmertransid", Namespace="http://ecom.dbbl/", Order=0)]
        public DBBL_WebService_Live.getsubmertransidRequestBody Body;
        
        public getsubmertransidRequest()
        {
        }
        
        public getsubmertransidRequest(DBBL_WebService_Live.getsubmertransidRequestBody Body)
        {
            this.Body = Body;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("dotnet-svcutil", "1.0.0.1")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.Runtime.Serialization.DataContractAttribute(Namespace="")]
    public partial class getsubmertransidRequestBody
    {
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=0)]
        public string userid;
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=1)]
        public string pwd;
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=2)]
        public string submername;
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=3)]
        public string submerid;
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=4)]
        public string terminalid;
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=5)]
        public string amount;
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=6)]
        public string cardtype;
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=7)]
        public string txnrefnum;
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=8)]
        public string clientip;
        
        public getsubmertransidRequestBody()
        {
        }
        
        public getsubmertransidRequestBody(string userid, string pwd, string submername, string submerid, string terminalid, string amount, string cardtype, string txnrefnum, string clientip)
        {
            this.userid = userid;
            this.pwd = pwd;
            this.submername = submername;
            this.submerid = submerid;
            this.terminalid = terminalid;
            this.amount = amount;
            this.cardtype = cardtype;
            this.txnrefnum = txnrefnum;
            this.clientip = clientip;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("dotnet-svcutil", "1.0.0.1")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(IsWrapped=false)]
    public partial class getsubmertransidResponse
    {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Name="getsubmertransidResponse", Namespace="http://ecom.dbbl/", Order=0)]
        public DBBL_WebService_Live.getsubmertransidResponseBody Body;
        
        public getsubmertransidResponse()
        {
        }
        
        public getsubmertransidResponse(DBBL_WebService_Live.getsubmertransidResponseBody Body)
        {
            this.Body = Body;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("dotnet-svcutil", "1.0.0.1")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.Runtime.Serialization.DataContractAttribute(Namespace="")]
    public partial class getsubmertransidResponseBody
    {
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=0)]
        public string @return;
        
        public getsubmertransidResponseBody()
        {
        }
        
        public getsubmertransidResponseBody(string @return)
        {
            this.@return = @return;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("dotnet-svcutil", "1.0.0.1")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(IsWrapped=false)]
    public partial class getresultfieldRequest
    {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Name="getresultfield", Namespace="http://ecom.dbbl/", Order=0)]
        public DBBL_WebService_Live.getresultfieldRequestBody Body;
        
        public getresultfieldRequest()
        {
        }
        
        public getresultfieldRequest(DBBL_WebService_Live.getresultfieldRequestBody Body)
        {
            this.Body = Body;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("dotnet-svcutil", "1.0.0.1")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.Runtime.Serialization.DataContractAttribute(Namespace="")]
    public partial class getresultfieldRequestBody
    {
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=0)]
        public string userid;
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=1)]
        public string pwd;
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=2)]
        public string transid;
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=3)]
        public string clientip;
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=4)]
        public string billinginfo;
        
        public getresultfieldRequestBody()
        {
        }
        
        public getresultfieldRequestBody(string userid, string pwd, string transid, string clientip, string billinginfo)
        {
            this.userid = userid;
            this.pwd = pwd;
            this.transid = transid;
            this.clientip = clientip;
            this.billinginfo = billinginfo;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("dotnet-svcutil", "1.0.0.1")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(IsWrapped=false)]
    public partial class getresultfieldResponse
    {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Name="getresultfieldResponse", Namespace="http://ecom.dbbl/", Order=0)]
        public DBBL_WebService_Live.getresultfieldResponseBody Body;
        
        public getresultfieldResponse()
        {
        }
        
        public getresultfieldResponse(DBBL_WebService_Live.getresultfieldResponseBody Body)
        {
            this.Body = Body;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("dotnet-svcutil", "1.0.0.1")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.Runtime.Serialization.DataContractAttribute(Namespace="")]
    public partial class getresultfieldResponseBody
    {
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=0)]
        public string @return;
        
        public getresultfieldResponseBody()
        {
        }
        
        public getresultfieldResponseBody(string @return)
        {
            this.@return = @return;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("dotnet-svcutil", "1.0.0.1")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(IsWrapped=false)]
    public partial class getransidRequest
    {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Name="getransid", Namespace="http://ecom.dbbl/", Order=0)]
        public DBBL_WebService_Live.getransidRequestBody Body;
        
        public getransidRequest()
        {
        }
        
        public getransidRequest(DBBL_WebService_Live.getransidRequestBody Body)
        {
            this.Body = Body;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("dotnet-svcutil", "1.0.0.1")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.Runtime.Serialization.DataContractAttribute(Namespace="")]
    public partial class getransidRequestBody
    {
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=0)]
        public string userid;
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=1)]
        public string pwd;
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=2)]
        public string amount;
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=3)]
        public string cardtype;
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=4)]
        public string txnrefnum;
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=5)]
        public string clientip;
        
        public getransidRequestBody()
        {
        }
        
        public getransidRequestBody(string userid, string pwd, string amount, string cardtype, string txnrefnum, string clientip)
        {
            this.userid = userid;
            this.pwd = pwd;
            this.amount = amount;
            this.cardtype = cardtype;
            this.txnrefnum = txnrefnum;
            this.clientip = clientip;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("dotnet-svcutil", "1.0.0.1")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(IsWrapped=false)]
    public partial class getransidResponse
    {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Name="getransidResponse", Namespace="http://ecom.dbbl/", Order=0)]
        public DBBL_WebService_Live.getransidResponseBody Body;
        
        public getransidResponse()
        {
        }
        
        public getransidResponse(DBBL_WebService_Live.getransidResponseBody Body)
        {
            this.Body = Body;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("dotnet-svcutil", "1.0.0.1")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.Runtime.Serialization.DataContractAttribute(Namespace="")]
    public partial class getransidResponseBody
    {
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=0)]
        public string @return;
        
        public getransidResponseBody()
        {
        }
        
        public getransidResponseBody(string @return)
        {
            this.@return = @return;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("dotnet-svcutil", "1.0.0.1")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(IsWrapped=false)]
    public partial class getresultRequest
    {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Name="getresult", Namespace="http://ecom.dbbl/", Order=0)]
        public DBBL_WebService_Live.getresultRequestBody Body;
        
        public getresultRequest()
        {
        }
        
        public getresultRequest(DBBL_WebService_Live.getresultRequestBody Body)
        {
            this.Body = Body;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("dotnet-svcutil", "1.0.0.1")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.Runtime.Serialization.DataContractAttribute(Namespace="")]
    public partial class getresultRequestBody
    {
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=0)]
        public string userid;
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=1)]
        public string pwd;
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=2)]
        public string transid;
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=3)]
        public string clientip;
        
        public getresultRequestBody()
        {
        }
        
        public getresultRequestBody(string userid, string pwd, string transid, string clientip)
        {
            this.userid = userid;
            this.pwd = pwd;
            this.transid = transid;
            this.clientip = clientip;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("dotnet-svcutil", "1.0.0.1")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(IsWrapped=false)]
    public partial class getresultResponse
    {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Name="getresultResponse", Namespace="http://ecom.dbbl/", Order=0)]
        public DBBL_WebService_Live.getresultResponseBody Body;
        
        public getresultResponse()
        {
        }
        
        public getresultResponse(DBBL_WebService_Live.getresultResponseBody Body)
        {
            this.Body = Body;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("dotnet-svcutil", "1.0.0.1")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.Runtime.Serialization.DataContractAttribute(Namespace="")]
    public partial class getresultResponseBody
    {
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=0)]
        public string @return;
        
        public getresultResponseBody()
        {
        }
        
        public getresultResponseBody(string @return)
        {
            this.@return = @return;
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("dotnet-svcutil", "1.0.0.1")]
    public interface dbblecomtxnChannel : DBBL_WebService_Live.dbblecomtxn, System.ServiceModel.IClientChannel
    {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("dotnet-svcutil", "1.0.0.1")]
    public partial class dbblecomtxnClient : System.ServiceModel.ClientBase<DBBL_WebService_Live.dbblecomtxn>, DBBL_WebService_Live.dbblecomtxn
    {
        
    /// <summary>
    /// Implement this partial method to configure the service endpoint.
    /// </summary>
    /// <param name="serviceEndpoint">The endpoint to configure</param>
    /// <param name="clientCredentials">The client credentials</param>
    static partial void ConfigureEndpoint(System.ServiceModel.Description.ServiceEndpoint serviceEndpoint, System.ServiceModel.Description.ClientCredentials clientCredentials);
        
        public dbblecomtxnClient() : 
                base(dbblecomtxnClient.GetDefaultBinding(), dbblecomtxnClient.GetDefaultEndpointAddress())
        {
            this.Endpoint.Name = EndpointConfiguration.dbblecomtxnPort.ToString();
            ConfigureEndpoint(this.Endpoint, this.ClientCredentials);
        }
        
        public dbblecomtxnClient(EndpointConfiguration endpointConfiguration) : 
                base(dbblecomtxnClient.GetBindingForEndpoint(endpointConfiguration), dbblecomtxnClient.GetEndpointAddress(endpointConfiguration))
        {
            this.Endpoint.Name = endpointConfiguration.ToString();
            ConfigureEndpoint(this.Endpoint, this.ClientCredentials);
        }
        
        public dbblecomtxnClient(EndpointConfiguration endpointConfiguration, string remoteAddress) : 
                base(dbblecomtxnClient.GetBindingForEndpoint(endpointConfiguration), new System.ServiceModel.EndpointAddress(remoteAddress))
        {
            this.Endpoint.Name = endpointConfiguration.ToString();
            ConfigureEndpoint(this.Endpoint, this.ClientCredentials);
        }
        
        public dbblecomtxnClient(EndpointConfiguration endpointConfiguration, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(dbblecomtxnClient.GetBindingForEndpoint(endpointConfiguration), remoteAddress)
        {
            this.Endpoint.Name = endpointConfiguration.ToString();
            ConfigureEndpoint(this.Endpoint, this.ClientCredentials);
        }
        
        public dbblecomtxnClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress)
        {
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        System.Threading.Tasks.Task<DBBL_WebService_Live.getsubmertransidResponse> DBBL_WebService_Live.dbblecomtxn.getsubmertransidAsync(DBBL_WebService_Live.getsubmertransidRequest request)
        {
            return base.Channel.getsubmertransidAsync(request);
        }
        
        public System.Threading.Tasks.Task<DBBL_WebService_Live.getsubmertransidResponse> getsubmertransidAsync(string userid, string pwd, string submername, string submerid, string terminalid, string amount, string cardtype, string txnrefnum, string clientip)
        {
            DBBL_WebService_Live.getsubmertransidRequest inValue = new DBBL_WebService_Live.getsubmertransidRequest();
            inValue.Body = new DBBL_WebService_Live.getsubmertransidRequestBody();
            inValue.Body.userid = userid;
            inValue.Body.pwd = pwd;
            inValue.Body.submername = submername;
            inValue.Body.submerid = submerid;
            inValue.Body.terminalid = terminalid;
            inValue.Body.amount = amount;
            inValue.Body.cardtype = cardtype;
            inValue.Body.txnrefnum = txnrefnum;
            inValue.Body.clientip = clientip;
            return ((DBBL_WebService_Live.dbblecomtxn)(this)).getsubmertransidAsync(inValue);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        System.Threading.Tasks.Task<DBBL_WebService_Live.getresultfieldResponse> DBBL_WebService_Live.dbblecomtxn.getresultfieldAsync(DBBL_WebService_Live.getresultfieldRequest request)
        {
            return base.Channel.getresultfieldAsync(request);
        }
        
        public System.Threading.Tasks.Task<DBBL_WebService_Live.getresultfieldResponse> getresultfieldAsync(string userid, string pwd, string transid, string clientip, string billinginfo)
        {
            DBBL_WebService_Live.getresultfieldRequest inValue = new DBBL_WebService_Live.getresultfieldRequest();
            inValue.Body = new DBBL_WebService_Live.getresultfieldRequestBody();
            inValue.Body.userid = userid;
            inValue.Body.pwd = pwd;
            inValue.Body.transid = transid;
            inValue.Body.clientip = clientip;
            inValue.Body.billinginfo = billinginfo;
            return ((DBBL_WebService_Live.dbblecomtxn)(this)).getresultfieldAsync(inValue);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        System.Threading.Tasks.Task<DBBL_WebService_Live.getransidResponse> DBBL_WebService_Live.dbblecomtxn.getransidAsync(DBBL_WebService_Live.getransidRequest request)
        {
            return base.Channel.getransidAsync(request);
        }
        
        public System.Threading.Tasks.Task<DBBL_WebService_Live.getransidResponse> getransidAsync(string userid, string pwd, string amount, string cardtype, string txnrefnum, string clientip)
        {
            DBBL_WebService_Live.getransidRequest inValue = new DBBL_WebService_Live.getransidRequest();
            inValue.Body = new DBBL_WebService_Live.getransidRequestBody();
            inValue.Body.userid = userid;
            inValue.Body.pwd = pwd;
            inValue.Body.amount = amount;
            inValue.Body.cardtype = cardtype;
            inValue.Body.txnrefnum = txnrefnum;
            inValue.Body.clientip = clientip;
            return ((DBBL_WebService_Live.dbblecomtxn)(this)).getransidAsync(inValue);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        System.Threading.Tasks.Task<DBBL_WebService_Live.getresultResponse> DBBL_WebService_Live.dbblecomtxn.getresultAsync(DBBL_WebService_Live.getresultRequest request)
        {
            return base.Channel.getresultAsync(request);
        }
        
        public System.Threading.Tasks.Task<DBBL_WebService_Live.getresultResponse> getresultAsync(string userid, string pwd, string transid, string clientip)
        {
            DBBL_WebService_Live.getresultRequest inValue = new DBBL_WebService_Live.getresultRequest();
            inValue.Body = new DBBL_WebService_Live.getresultRequestBody();
            inValue.Body.userid = userid;
            inValue.Body.pwd = pwd;
            inValue.Body.transid = transid;
            inValue.Body.clientip = clientip;
            return ((DBBL_WebService_Live.dbblecomtxn)(this)).getresultAsync(inValue);
        }
        
        public virtual System.Threading.Tasks.Task OpenAsync()
        {
            return System.Threading.Tasks.Task.Factory.FromAsync(((System.ServiceModel.ICommunicationObject)(this)).BeginOpen(null, null), new System.Action<System.IAsyncResult>(((System.ServiceModel.ICommunicationObject)(this)).EndOpen));
        }
        
        public virtual System.Threading.Tasks.Task CloseAsync()
        {
            return System.Threading.Tasks.Task.Factory.FromAsync(((System.ServiceModel.ICommunicationObject)(this)).BeginClose(null, null), new System.Action<System.IAsyncResult>(((System.ServiceModel.ICommunicationObject)(this)).EndClose));
        }
        
        private static System.ServiceModel.Channels.Binding GetBindingForEndpoint(EndpointConfiguration endpointConfiguration)
        {
            if ((endpointConfiguration == EndpointConfiguration.dbblecomtxnPort))
            {
                System.ServiceModel.BasicHttpBinding result = new System.ServiceModel.BasicHttpBinding();
                result.MaxBufferSize = int.MaxValue;
                result.ReaderQuotas = System.Xml.XmlDictionaryReaderQuotas.Max;
                result.MaxReceivedMessageSize = int.MaxValue;
                result.AllowCookies = true;
                result.Security.Mode = System.ServiceModel.BasicHttpSecurityMode.Transport;
                return result;
            }
            throw new System.InvalidOperationException(string.Format("Could not find endpoint with name \'{0}\'.", endpointConfiguration));
        }
        
        private static System.ServiceModel.EndpointAddress GetEndpointAddress(EndpointConfiguration endpointConfiguration)
        {
            if ((endpointConfiguration == EndpointConfiguration.dbblecomtxnPort))
            {
                return new System.ServiceModel.EndpointAddress("https://ecom.dutchbanglabank.com/ecomws/dbblecomtxn");
            }
            throw new System.InvalidOperationException(string.Format("Could not find endpoint with name \'{0}\'.", endpointConfiguration));
        }
        
        private static System.ServiceModel.Channels.Binding GetDefaultBinding()
        {
            return dbblecomtxnClient.GetBindingForEndpoint(EndpointConfiguration.dbblecomtxnPort);
        }
        
        private static System.ServiceModel.EndpointAddress GetDefaultEndpointAddress()
        {
            return dbblecomtxnClient.GetEndpointAddress(EndpointConfiguration.dbblecomtxnPort);
        }
        
        public enum EndpointConfiguration
        {
            
            dbblecomtxnPort,
        }
    }
}
