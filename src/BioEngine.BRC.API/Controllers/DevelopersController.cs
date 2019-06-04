﻿using BioEngine.BRC.Api.Entities.Request;
using BioEngine.BRC.Domain.Entities;
using BioEngine.BRC.Domain.Repository;
using BioEngine.Core.API;
using BioEngine.Core.DB;
using BioEngine.Core.Repository;
using BioEngine.Core.Web;

namespace BioEngine.BRC.Api.Controllers
{
    public class
        DevelopersController : SectionController<Developer, DeveloperData, DevelopersRepository,
            Entities.Response.Developer,
            DeveloperRequestItem>
    {
        protected override string GetUploadPath()
        {
            return "developers";
        }


        public DevelopersController(
            BaseControllerContext<Developer, ContentEntityQueryContext<Developer>, DevelopersRepository> context,
            BioEntityMetadataManager metadataManager, ContentBlocksRepository blocksRepository) : base(context,
            metadataManager, blocksRepository)
        {
        }
    }
}
