using Amazon.CDK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cdk
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            string appName = System.Environment.GetEnvironmentVariable("APP_NAME") ?? throw new ArgumentNullException("APP_NAME");
            string accountAws = System.Environment.GetEnvironmentVariable("ACCOUNT_AWS") ?? throw new ArgumentNullException("ACCOUNT_AWS");
            string regionAws = System.Environment.GetEnvironmentVariable("REGION_AWS") ?? throw new ArgumentNullException("REGION_AWS");
            string certificateRegionAws = System.Environment.GetEnvironmentVariable("CERTIFICATE_REGION_AWS") ?? throw new ArgumentNullException("CERTIFICATE_REGION_AWS");


            var app = new App();

            var certificateStack = new CdkStackCertificate(app, $"Cdk{appName}Certificate", new StackProps {
                Env = new Amazon.CDK.Environment {
                    Account = accountAws,
                    Region = certificateRegionAws,
                }
            });

            new CdkStack(app, $"Cdk{appName}LandingPage", new CdkStackProps() {
                Env = new Amazon.CDK.Environment {
                  Account = accountAws,
                  Region = regionAws,
                },
                HostedZone = certificateStack.HostedZone,
                Certificate = certificateStack.Certificate,
            });

            app.Synth();
        }
    }
}
