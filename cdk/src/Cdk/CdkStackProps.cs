using Amazon.CDK;
using Amazon.CDK.AWS.CertificateManager;
using Amazon.CDK.AWS.Route53;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cdk {
    internal class CdkStackProps : StackProps {
        public required HostedZone HostedZone { get; set; }
        public required Certificate Certificate { get; set; }
    }
}
