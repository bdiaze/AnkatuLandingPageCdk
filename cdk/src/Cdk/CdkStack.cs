using Amazon.CDK;
using Amazon.CDK.AWS.CertificateManager;
using Amazon.CDK.AWS.CloudFront;
using Amazon.CDK.AWS.CloudFront.Origins;
using Amazon.CDK.AWS.Route53;
using Amazon.CDK.AWS.Route53.Targets;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.S3.Deployment;
using Amazon.CDK.AWS.SES;
using Constructs;
using System;
using System.Linq;

namespace Cdk
{
    public class CdkStack : Stack
    {
        internal CdkStack(Construct scope, string id, CdkStackProps props = null) : base(scope, id, props)
        {
            string appName = System.Environment.GetEnvironmentVariable("APP_NAME") ?? throw new ArgumentNullException("APP_NAME");
            string distributionDomainNames = System.Environment.GetEnvironmentVariable("DISTRIBUTION_DOMAIN_NAMES") ?? throw new ArgumentNullException("DISTRIBUTION_DOMAIN_NAMES");
            string rootObject = System.Environment.GetEnvironmentVariable("ROOT_OBJECT") ?? throw new ArgumentNullException("ROOT_OBJECT");
            string buildDirectory = System.Environment.GetEnvironmentVariable("BUILD_DIR") ?? throw new ArgumentNullException("BUILD_DIR");
            string mailFromDomain = System.Environment.GetEnvironmentVariable("MAIL_FROM_DOMAIN") ?? throw new ArgumentNullException("MAIL_FROM_DOMAIN");


            // Se crea bucket donde se almacenará aplicación frontend...  
            Bucket bucket = new(this, $"{appName}LandingPageS3Bucket", new BucketProps {
                BucketName = $"{appName.ToLower()}-landing-page",
                Versioned = false,
                RemovalPolicy = RemovalPolicy.DESTROY,
                BlockPublicAccess = BlockPublicAccess.BLOCK_ALL,
            });
            
            // Se crea distribución de cloudfront...
            Distribution distribution = new(this, $"{appName}LandingPageDistribution", new DistributionProps {
                Comment = $"{appName} Landing Page Distribution",
                DomainNames = distributionDomainNames.Split(","),
                Certificate = props.Certificate,
                DefaultRootObject = rootObject,
                DefaultBehavior = new BehaviorOptions {
                    Origin = S3BucketOrigin.WithOriginAccessControl(bucket),
                    Compress = true,
                    AllowedMethods = AllowedMethods.ALLOW_GET_HEAD_OPTIONS,
                    ViewerProtocolPolicy = ViewerProtocolPolicy.REDIRECT_TO_HTTPS,
                },
                ErrorResponses = [
                    new ErrorResponse {
                        HttpStatus = 403,
                        ResponseHttpStatus = 200,
                        ResponsePagePath = $"/{rootObject}",
                        Ttl = Duration.Days(1),
                    },
                    new ErrorResponse {
                        HttpStatus = 404,
                        ResponseHttpStatus = 200,
                        ResponsePagePath = $"/{rootObject}",
                        Ttl = Duration.Days(1),
                    },
                ]
            });

            // Se despliegan piezas del frontend en el bucket...
            _ = new BucketDeployment(this, $"{appName}LandingPageDeployment", new BucketDeploymentProps {
                Sources = [Source.Asset(buildDirectory)],
                DestinationBucket = bucket,
                Distribution = distribution,
            });

            // Se crea record en hosted zone...
            string[] distrDomainNames = distributionDomainNames.Split(",");
            for (int i = 0; i < distrDomainNames.Length; i++) {
                _ = new ARecord(this, $"{appName}LandingPageARecord{i + 1}", new ARecordProps {
                    Zone = props.HostedZone,
                    RecordName = distrDomainNames[i],
                    Target = RecordTarget.FromAlias(new CloudFrontTarget(distribution)),
                });
            }

            IPublicHostedZone publicHostedZone = PublicHostedZone.FromPublicHostedZoneAttributes(this, $"{appName}PublicHostedZone", new PublicHostedZoneAttributes {
                ZoneName = props.HostedZone.ZoneName,
                HostedZoneId = props.HostedZone.HostedZoneId,
            });

            // Se crea email identity para envío de correos...
            EmailIdentity emailIdentity = new(this, $"{appName}EmailIdentity", new EmailIdentityProps {
                Identity = Identity.PublicHostedZone(publicHostedZone),
                MailFromDomain = mailFromDomain,
                MailFromBehaviorOnMxFailure = MailFromBehaviorOnMxFailure.USE_DEFAULT_VALUE,
            });
        }
    }
}
