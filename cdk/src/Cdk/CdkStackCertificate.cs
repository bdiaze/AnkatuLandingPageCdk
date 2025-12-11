using Amazon.CDK;
using Amazon.CDK.AWS.CertificateManager;
using Amazon.CDK.AWS.Route53;
using Amazon.CDK.AWS.SES;
using Constructs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cdk {
    public class CdkStackCertificate : Stack {

        public HostedZone HostedZone { get; set; }

        public Certificate Certificate { get; set; }

        internal CdkStackCertificate(Construct scope, string id, IStackProps props = null) : base(scope, id, props) {
            string appName = System.Environment.GetEnvironmentVariable("APP_NAME") ?? throw new ArgumentNullException("APP_NAME");
            string domainName = System.Environment.GetEnvironmentVariable("DOMAIN_NAME") ?? throw new ArgumentNullException("DOMAIN_NAME");
            string alternativeNames = System.Environment.GetEnvironmentVariable("ALTERNATIVE_NAMES") ?? throw new ArgumentNullException("ALTERNATIVE_NAMES");
            string workmailFromDomain = System.Environment.GetEnvironmentVariable("WORKMAIL_FROM_DOMAIN") ?? throw new ArgumentNullException("WORKMAIL_FROM_DOMAIN");

            // Se crea hosted zone...
            HostedZone = new(this, $"{appName}HostedZone", new HostedZoneProps {
                Comment = $"{appName} Hosted Zone",
                ZoneName = domainName
            });

            // Se crea certificado para custom domain...
            Certificate = new(this, $"{appName}Certificate", new CertificateProps {
                CertificateName = $"{appName}Certificate",
                DomainName = domainName,
                SubjectAlternativeNames = alternativeNames.Split(","),
                Validation = CertificateValidation.FromDns(HostedZone),
            });

            IPublicHostedZone publicHostedZone = PublicHostedZone.FromPublicHostedZoneAttributes(this, $"{appName}PublicHostedZone", new PublicHostedZoneAttributes {
                ZoneName = HostedZone.ZoneName,
                HostedZoneId = HostedZone.HostedZoneId,
            });

            // Se crea email identity para envío de correos...
            EmailIdentity emailIdentity = new(this, $"{appName}EmailIdentity", new EmailIdentityProps {
                Identity = Identity.PublicHostedZone(publicHostedZone),
                MailFromDomain = workmailFromDomain,
                MailFromBehaviorOnMxFailure = MailFromBehaviorOnMxFailure.USE_DEFAULT_VALUE,
            });
        }
    }
}
