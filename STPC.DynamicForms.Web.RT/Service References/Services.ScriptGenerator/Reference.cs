﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34014
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace STPC.DynamicForms.Web.RT.Services.ScriptGenerator {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="Services.ScriptGenerator.IScriptGeneratorService")]
    public interface IScriptGeneratorService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IScriptGeneratorService/GenerateScriptString", ReplyAction="http://tempuri.org/IScriptGeneratorService/GenerateScriptStringResponse")]
        string GenerateScriptString(System.Guid formId);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IScriptGeneratorServiceChannel : STPC.DynamicForms.Web.RT.Services.ScriptGenerator.IScriptGeneratorService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class ScriptGeneratorServiceClient : System.ServiceModel.ClientBase<STPC.DynamicForms.Web.RT.Services.ScriptGenerator.IScriptGeneratorService>, STPC.DynamicForms.Web.RT.Services.ScriptGenerator.IScriptGeneratorService {
        
        public ScriptGeneratorServiceClient() {
        }
        
        public ScriptGeneratorServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public ScriptGeneratorServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public ScriptGeneratorServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public ScriptGeneratorServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public string GenerateScriptString(System.Guid formId) {
            return base.Channel.GenerateScriptString(formId);
        }
    }
}