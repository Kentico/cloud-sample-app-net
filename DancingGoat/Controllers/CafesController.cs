﻿using DancingGoat.Models;
using KenticoCloud.Delivery;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DancingGoat.Controllers
{
    [RoutePrefix("cafes")]
    public class CafesController : AsyncController
    {
        private readonly DeliveryClient client = new DeliveryClient(ConfigurationManager.AppSettings["ProjectId"]);

        public CafesController()
        {
            client.CodeFirstModelProvider.TypeProvider = new CustomTypeProvider();
        }

        [Route]
        public async Task<ActionResult> Index()
        {
            var response = await client.GetItemsAsync<Cafe>(
                new EqualsFilter("system.type", "cafe"),
                new OrderParameter("system.name")
            );
            var cafes = response.Items;

            var viewModel = new CafesViewModel
            {
                CompanyCafes = cafes.Where(c => c.Country == "USA").ToList(),
                PartnerCafes = cafes.Where(c => c.Country != "USA").ToList()
            };

            return View(viewModel);
        }
    }
}