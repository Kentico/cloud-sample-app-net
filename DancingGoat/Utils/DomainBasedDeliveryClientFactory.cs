﻿using Kentico.Kontent.Delivery;
using Kentico.Kontent.Delivery.Abstractions;

namespace DancingGoat.Utils
{
    public class DomainBasedDeliveryClientFactory : IDeliveryClientFactory
    {
        private readonly IDeliveryClientFactory _baseClientFactory;
        private readonly ProjectContext _projectContext;
        private readonly ITypeProvider _typeProvider;
        private readonly IContentLinkUrlResolver _linkResolver;

        public DomainBasedDeliveryClientFactory(
            IDeliveryClientFactory baseClientFactory,
            ProjectContext projectContext,
            ITypeProvider typeProvider,
            IContentLinkUrlResolver linkResolver
        )
        {
            _baseClientFactory = baseClientFactory;
            _projectContext = projectContext;
            _typeProvider = typeProvider;
            _linkResolver = linkResolver;
        }

        public IDeliveryClient Get()
        {
            var clientBuilder = _projectContext.IsInPreviewMode
                ? DeliveryClientBuilder.WithOptions(builder => builder
                    .WithProjectId(_projectContext.ProjectGuid)
                    .UsePreviewApi(_projectContext.PreviewApiKey)
                    .Build()
                )
                : DeliveryClientBuilder.WithOptions(builder => builder
                    .WithProjectId(_projectContext.ProjectGuid)
                    .UseProductionApi()
                    .Build()
                );

            return clientBuilder
                .WithTypeProvider(_typeProvider)
                .WithContentLinkUrlResolver(_linkResolver)
                .Build();
        }

        public IDeliveryClient Get(string name)
        {
            return _baseClientFactory.Get(name);
        }
    }
}
