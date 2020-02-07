﻿using System;
using System.Threading.Tasks;
using DancingGoat.Areas.Admin.Models;
using DancingGoat.Configuration;
using DancingGoat.Helpers;
using Kentico.Kontent.Delivery;
using Kentico.Kontent.Delivery.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace DancingGoat.Areas.Admin.Controllers
{
    public class SelfConfigController : Controller
    {
        protected const string CAPTION_CONFIGURATION_WRITE_ERROR = "Configuration Save Error";
        protected const string CAPTION_DESERIALIZATION_ERROR = "API Response Deserialization Error";

        protected const string MESSAGE_SELECTED_PROJECT =
                "You've configured your app with a project ID \"{0}\". You can edit its contents at https://app.kontent.ai/."
            ;

        protected const string MESSAGE_SHARED_PROJECT =
            "You've configured your app to with a project ID of a shared Kentico Kontent project.";

        protected const string MESSAGE_INVALID_PROJECT_GUID = "ProjectId is not valid GUID.";

        public const int PROJECT_EXISTENCE_VERIFICATION_REQUIRED_ITEMS = 30;

        protected IDeliveryClient client;

        public IWritableOptions<DeliveryOptions> Options { get; }
        public IWritableOptions<AppConfiguration> Configuration { get; }

        public SelfConfigController(IDeliveryClient deliveryClient, IWritableOptions<DeliveryOptions> options, IWritableOptions<AppConfiguration> configuration)
        {
            Options = options;
            Configuration = configuration;
            client = deliveryClient;
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View("~/Areas/Admin/Views/SelfConfig/Index.cshtml", new IndexViewModel());
        }

        [HttpGet]
        public ActionResult Done()
        {
            return RedirectHelpers.GetHomeRedirectResult(new MessageModel { Caption = null, Message = string.Format(MESSAGE_SELECTED_PROJECT, Options.Value.ProjectId), MessageType = MessageType.Info });
        }

        [HttpGet]
        public async Task<ActionResult> SampleProjectReady()
        {
            var items = (await client.GetItemsAsync()).Items;
            return Json(items.Count >= PROJECT_EXISTENCE_VERIFICATION_REQUIRED_ITEMS);
        }

        [HttpPost]
        public ActionResult UseShared()
        {
            return SetConfiguration(MESSAGE_SHARED_PROJECT, Configuration.Value.DefaultProjectId, null, false);
        }

        [HttpPost]
        public ActionResult UseSelected(IndexViewModel model)
        {
            if (!model.ProjectGuid.HasValue)
            {
                return View("Error", new MessageModel { Caption = CAPTION_CONFIGURATION_WRITE_ERROR, Message = MESSAGE_INVALID_PROJECT_GUID, MessageType = MessageType.Error });
            }

            return SetConfiguration(string.Format(MESSAGE_SELECTED_PROJECT, model.ProjectGuid.Value), model.ProjectGuid.Value, model.EndAt, model.NewlyGeneratedProject);
        }

        private ActionResult SetConfiguration(string message, Guid projectGuid, DateTime? endAt, bool isNew)
        {
            try
            {
                if (endAt.HasValue)
                {
                    Options.Update(opt => { opt.ProjectId = projectGuid.ToString(); });
                    Configuration.Update(opt => { opt.SubscriptionExpiresAt = endAt.Value.ToUniversalTime(); });
                }

                if (isNew)
                {
                    return View("Wait");
                }

                return RedirectHelpers.GetHomeRedirectResult(new MessageModel { Caption = null, Message = message, MessageType = MessageType.Info });
            }
            catch (JsonSerializationException ex)
            {
                return View("Error", new MessageModel { Caption = CAPTION_DESERIALIZATION_ERROR, Message = ex.Message, MessageType = MessageType.Error });
            }
            catch (Exception ex)
            {
                return View("Error", new MessageModel { Caption = CAPTION_CONFIGURATION_WRITE_ERROR, Message = ex.Message, MessageType = MessageType.Error });
            }

        }
    }
}