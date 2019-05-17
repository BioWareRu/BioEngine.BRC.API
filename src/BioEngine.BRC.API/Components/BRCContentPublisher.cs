using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.Entities;
using BioEngine.Core.Properties;
using BioEngine.Core.Repository;
using BioEngine.Extra.Facebook;
using BioEngine.Extra.IPB.Properties;
using BioEngine.Extra.IPB.Publishing;
using BioEngine.Extra.Twitter;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;

namespace BioEngine.BRC.Api.Components
{
    [UsedImplicitly]
    public class BRCContentPublisher
    {
        private readonly IPBContentPublisher _ipbContentPublisher;
        private readonly TwitterContentPublisher _twitterContentPublisher;
        private readonly FacebookContentPublisher _facebookContentPublisher;
        private readonly SitesRepository _sitesRepository;
        private readonly SectionsRepository _sectionsRepository;
        private readonly PropertiesProvider _propertiesProvider;
        private readonly BrcApiOptions _options;

        public BRCContentPublisher(IPBContentPublisher ipbContentPublisher,
            TwitterContentPublisher twitterContentPublisher,
            FacebookContentPublisher facebookContentPublisher, SitesRepository sitesRepository,
            SectionsRepository sectionsRepository,
            PropertiesProvider propertiesProvider, IOptions<BrcApiOptions> options)
        {
            _ipbContentPublisher = ipbContentPublisher;
            _twitterContentPublisher = twitterContentPublisher;
            _facebookContentPublisher = facebookContentPublisher;
            _sitesRepository = sitesRepository;
            _sectionsRepository = sectionsRepository;
            _propertiesProvider = propertiesProvider;
            _options = options.Value;
        }

        private Guid GetMainSiteId(Post post)
        {
            return post.SiteIds.Contains(_options.DefaultMainSiteId)
                ? _options.DefaultMainSiteId
                : post.SiteIds.First();
        }


        public async Task PublishOrDeleteAsync(string currentUserToken, Post post,
            PropertyChange[] changes)
        {
            var sites = await _sitesRepository.GetByIdsAsync(post.SiteIds);
            foreach (var site in sites)
            {
                if (site.Id == GetMainSiteId(post))
                {
                    var ipbSettings = await _propertiesProvider.GetAsync<IPBSitePropertiesSet>(site);
                    if (ipbSettings != null && ipbSettings.ForumId > 0)
                    {
                        await _ipbContentPublisher.PublishAsync(post, site,
                            new IPBPublishConfig(ipbSettings.ForumId, currentUserToken));
                    }
                }

                var twitterSettings = await _propertiesProvider.GetAsync<TwitterSitePropertiesSet>(site);
                if (twitterSettings != null && twitterSettings.IsEnabled)
                {
                    var hasChanges = changes != null && changes.Any(c =>
                                         c.Name == nameof(post.Title) || c.Name == nameof(post.Url));

                    var sections = await _sectionsRepository.GetByIdsAsync(post.SectionIds);
                    var tags =
                        sections
                            .Where(s => !string.IsNullOrEmpty(s.Hashtag))
                            .Select(s => $"#{s.Hashtag.Replace("#", "")}").ToList();

                    var twitterConfig = new TwitterPublishConfig(
                        new TwitterConfig(twitterSettings.ConsumerKey, twitterSettings.ConsumerSecret,
                            twitterSettings.AccessToken, twitterSettings.AccessTokenSecret), tags);

                    if (hasChanges || !post.IsPublished)
                    {
                        await _twitterContentPublisher.DeleteAsync(post, twitterConfig, site);
                    }

                    if (post.IsPublished)
                    {
                        await _twitterContentPublisher.PublishAsync(post, site, twitterConfig);
                    }
                }

                var facebookSettings = await _propertiesProvider.GetAsync<FacebookSitePropertiesSet>(site);
                if (facebookSettings != null && facebookSettings.IsEnabled)
                {
                    var hasChanges = changes != null && changes.Any(c => c.Name == nameof(post.Url));

                    var facebookConfig = new FacebookConfig(new Uri(facebookSettings.ApiUrl), facebookSettings.PageId,
                        facebookSettings.AccessToken);

                    if (hasChanges || !post.IsPublished)
                    {
                        await _facebookContentPublisher.DeleteAsync(post, facebookConfig, site);
                    }

                    if (post.IsPublished)
                    {
                        await _facebookContentPublisher.PublishAsync(post, site, facebookConfig);
                    }
                }
            }
        }

        public async Task DeleteAsync(string currentUserToken, Post post)
        {
            var sites = await _sitesRepository.GetByIdsAsync(post.SiteIds);
            foreach (var site in sites)
            {
                if (site.Id == GetMainSiteId(post))
                {
                    var ipbSettings = await _propertiesProvider.GetAsync<IPBSitePropertiesSet>(site);
                    if (ipbSettings != null && ipbSettings.ForumId > 0)
                    {
                        await _ipbContentPublisher.DeleteAsync(post,
                            new IPBPublishConfig(ipbSettings.ForumId, currentUserToken), site);
                    }
                }

                var twitterSettings = await _propertiesProvider.GetAsync<TwitterSitePropertiesSet>(site);
                if (twitterSettings != null && twitterSettings.IsEnabled)
                {
                    await _twitterContentPublisher.DeleteAsync(post,
                        new TwitterPublishConfig(
                            new TwitterConfig(twitterSettings.ConsumerKey, twitterSettings.ConsumerSecret,
                                twitterSettings.AccessToken, twitterSettings.AccessTokenSecret), new List<string>()),
                        site);
                }

                var facebookSettings = await _propertiesProvider.GetAsync<FacebookSitePropertiesSet>(site);
                if (facebookSettings != null && facebookSettings.IsEnabled)
                {
                    await _facebookContentPublisher.DeleteAsync(post,
                        new FacebookConfig(new Uri(facebookSettings.ApiUrl), facebookSettings.PageId,
                            facebookSettings.AccessToken), site);
                }
            }
        }
    }
}
