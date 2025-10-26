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

namespace Cdk
{
    public class CdkStack : Stack
    {
        internal CdkStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            string appName = System.Environment.GetEnvironmentVariable("APP_NAME") ?? throw new ArgumentNullException("APP_NAME");
            string arnCertificate = System.Environment.GetEnvironmentVariable("CERTIFICATE_ARN")!;
            string domainName = System.Environment.GetEnvironmentVariable("DOMAIN_NAME") ?? throw new ArgumentNullException("DOMAIN_NAME");
            string buildDirectory = System.Environment.GetEnvironmentVariable("BUILD_DIR")!;
            string rootObject = System.Environment.GetEnvironmentVariable("ROOT_OBJECT")!;
            string subdomainName = System.Environment.GetEnvironmentVariable("SUBDOMAIN_NAME") ?? throw new ArgumentNullException("SUBDOMAIN_NAME");

            // Se obtiene el certificado existente...
            ICertificate certificate = Certificate.FromCertificateArn(this, $"{appName}Certificate", arnCertificate);

            // Se obtiene el hosted zone existente...
            IHostedZone hostedZone = HostedZone.FromLookup(this, $"{appName}HostedZone", new HostedZoneProviderProps {
              DomainName = domainName,
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
                DefaultRootObject = rootObject,
                DomainNames = [subdomainName],
                DefaultBehavior = new BehaviorOptions {
                  Origin = S3BucketOrigin.WithOriginAccessControl(bucket),
                  Compress = true,
                  AllowedMethods = AllowedMethods.ALLOW_GET_HEAD_OPTIONS,
                  ViewerProtocolPolicy = ViewerProtocolPolicy.REDIRECT_TO_HTTPS,
                },
                Certificate = certificate,
                ErrorResponses = [
                    new ErrorResponse {
                        HttpStatus = 403,
                        ResponseHttpStatus = 200,
                        ResponsePagePath = $"/{rootObject}",
                    },
                    new ErrorResponse {
                        HttpStatus = 404,
                        ResponseHttpStatus = 200,
                        ResponsePagePath = $"/{rootObject}",
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
            _ = new ARecord(this, $"{appName}LandingPageARecord", new ARecordProps {
                Zone = hostedZone,
                RecordName = subdomainName,
                Target = RecordTarget.FromAlias(new CloudFrontTarget(distribution)),
            });
        }
    }
}
