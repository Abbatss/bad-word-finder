namespace DocumentGeneration.Api.Host.Lambda
{
    using Microsoft.AspNetCore.Hosting;

    /// <summary>
    /// This class extends from APIGatewayProxyFunction which contains the method FunctionHandlerAsync which is the
    /// actual Lambda function entry point. The Lambda handler field should be set to
    /// DocumentGeneration.Api.Host::DocumentGeneration.Api.Host.LambdaEntryPoint::FunctionHandlerAsync.
    /// </summary>
    public class LambdaEntryPoint :
         Amazon.Lambda.AspNetCoreServer.APIGatewayProxyFunction
    {
        /// <summary>
        /// The builder has configuration, logging and Amazon API Gateway already configured. The startup class
        /// needs to be configured in this method using the UseStartu() method.
        /// </summary>
        /// <param name="builder">Web Host builder.</param>
        protected override void Init(IWebHostBuilder builder)
        {
            builder
                .UseStartup<BWF.Api.Host.Startup>();
        }
    }
}
