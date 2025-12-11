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
            string ownershipTxtRecord = System.Environment.GetEnvironmentVariable("OWNERSHIP_TXT_RECORD") ?? throw new ArgumentNullException("OWNERSHIP_TXT_RECORD");

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

            // Para integración con WorkMail se crean registros en DNS...
            _ = new TxtRecord(this, $"{appName}SPF1TXTRecord", new TxtRecordProps {
                Zone = HostedZone,
                RecordName = HostedZone.ZoneName,
                Values = ["v=spf1 include:amazonses.com ~all"]
            });

            _ = new MxRecord(this, $"{appName}MXRecord", new MxRecordProps {
                Zone = HostedZone,
                RecordName = HostedZone.ZoneName,
                Values = [new MxRecordValue {
                    HostName = $"inbound-smtp.us-east-1.amazonaws.com.",
                    Priority = 10
                }]
            });

            _ = new CnameRecord(this, $"{appName}CNAMEAutodiscoverRecord", new CnameRecordProps {
                Zone = HostedZone,
                RecordName = $" autodiscover.{HostedZone.ZoneName}",
                DomainName = "autodiscover.mail.us-east-1.awsapps.com."
            });

            _ = new TxtRecord(this, $"{appName}OwnershipTXTRecord", new TxtRecordProps {
                Zone = HostedZone,
                RecordName = $"_amazonses.{HostedZone.ZoneName}",
                Values = [ownershipTxtRecord]
            });
        }
    }
}
