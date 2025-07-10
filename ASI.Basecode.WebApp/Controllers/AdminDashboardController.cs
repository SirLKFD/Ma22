using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using ASI.Basecode.WebApp.Mvc;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Controllers
{
   
    [Route("admin/[action]")]
    [Authorize(Roles = "0,2")]
    public class AdminDashboardController : ControllerBase<AdminDashboardController>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="configuration"></param>
        /// <param name="localizer"></param>
        /// <param name="mapper"></param>
        /// 
        private readonly IAdminDashboardService _adminDashboardService;
        public AdminDashboardController(IHttpContextAccessor httpContextAccessor,
                              ILoggerFactory loggerFactory,
                              IConfiguration configuration,
                              IAdminDashboardService adminDashboardService = null,
                              IMapper mapper = null) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _adminDashboardService = adminDashboardService;
        }

 
        public IActionResult AdminDashboard()
        {
            var Dashboard = _adminDashboardService.GetAdminDashboardInformation();
            return View("~/Views/Admin/AdminDashboard.cshtml",Dashboard);
        }
    }
}