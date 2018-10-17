﻿using System.Threading.Tasks;
using BioEngine.Core.API;
using BioEngine.Core.Web;
using Page = BioEngine.Core.API.Entities.Page;

namespace BioEngine.BRC.Api.Controllers
{
    public class PagesController : RestController<Page, Core.Entities.Page, int>
    {
        public PagesController(BaseControllerContext<Core.Entities.Page, int> context) : base(context)
        {
        }

        protected override async Task<Core.Entities.Page> MapDomainModelAsync(Page restModel,
            Core.Entities.Page domainModel = null)
        {
            domainModel = await base.MapDomainModelAsync(restModel, domainModel);
            domainModel.Title = restModel.Title;
            domainModel.Url = restModel.Url;
            domainModel.SiteIds = restModel.SiteIds;
            domainModel.Text = restModel.Text;
            return domainModel;
        }

        protected override async Task<Page> MapRestModelAsync(Core.Entities.Page domainModel)
        {
            var restModel = await base.MapRestModelAsync(domainModel);
            restModel.Title = domainModel.Title;
            restModel.Url = domainModel.Url;
            restModel.SiteIds = domainModel.SiteIds;
            restModel.Text = domainModel.Text;
            return restModel;
        }
    }
}