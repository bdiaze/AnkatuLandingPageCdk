using Amazon.CDK;
using Amazon.CDK.AWS.CertificateManager;
using Amazon.CDK.AWS.CloudFront;
using Amazon.CDK.AWS.CloudFront.Origins;
using Amazon.CDK.AWS.Route53;
using Amazon.CDK.AWS.Route53.Targets;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.S3.Deployment;
using Constructs;
using System;
using System.Linq;

namespace Cdk
{
    public class CdkStack : Stack
    {
        internal CdkStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            string appName = System.Environment.GetEnvironmentVariable("APP_NAME") ?? throw new ArgumentNullException("APP_NAME");
            string domainName = System.Environment.GetEnvironmentVariable("DOMAIN_NAME") ?? throw new ArgumentNullException("DOMAIN_NAME");
            string alternativeNames = System.Environment.GetEnvironmentVariable("ALTERNATIVE_NAMES") ?? throw new ArgumentNullException("ALTERNATIVE_NAMES");
            string distributionDomainNames = System.Environment.GetEnvironmentVariable("DISTRIBUTION_DOMAIN_NAMES") ?? throw new ArgumentNullException("DISTRIBUTION_DOMAIN_NAMES");
            string rootObject = System.Environment.GetEnvironmentVariable("ROOT_OBJECT") ?? throw new ArgumentNullException("ROOT_OBJECT");
            string buildDirectory = System.Environment.GetEnvironmentVariable("BUILD_DIR") ?? throw new ArgumentNullException("BUILD_DIR");

            HostedZone hostedZone = new (this, $"{appName}HostedZone", new HostedZoneProps {
                Comment = $"{appName} Hosted Zone",
                ZoneName = domainName
            });

            // Se crea certificado para custom domain...
            Certificate certificate = new(this, $"{appName}Certificate", new CertificateProps {
                CertificateName = $"{appName}Certificate",
                DomainName = domainName,
                SubjectAlternativeNames = alternativeNames.Split(","),
                Validation = CertificateValidation.FromDns(hostedZone),
            });

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
                Certificate = certificate,
                DefaultRootObject = rootObject,
                DefaultBehavior = new BehaviorOptions {
                    Origin = S3BucketOrigin.WithOriginAccessControl(bucket),
                    Compress = true,
                    AllowedMethods = AllowedMethods.ALLOW_GET_HEAD_OPTIONS,
                    ViewerProtocolPolicy = ViewerProtocolPolicy.REDIRECT_TO_HTTPS,
                },
            });

            // Se despliegan piezas del frontend en el bucket...
            _ = new BucketDeployment(this, $"{appName}LandingPageDeployment", new BucketDeploymentProps {
                Sources = [Source.Asset(buildDirectory)],
                DestinationBucket = bucket,
                Distribution = distribution,
            });

            // Se crea record en hosted zone...
            foreach (string distrDomainName in distributionDomainNames.Split(",")) {
                _ = new ARecord(this, $"{appName}LandingPageARecord", new ARecordProps {
                    Zone = hostedZone,
                    RecordName = distrDomainName,
                    Target = RecordTarget.FromAlias(new CloudFrontTarget(distribution)),
                });
            }
        }
    }
}
