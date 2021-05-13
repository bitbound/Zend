﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Zend.Models;
using Zend.Services;

namespace Zend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly IFileService _fileService;
        private readonly IWebHostEnvironment _hostEnv;
        private readonly string _appData;

        public FileController(
            IFileService fileService,
            IWebHostEnvironment hostEnv)
        {
            _fileService = fileService;
            _hostEnv = hostEnv;
            _appData = Directory.CreateDirectory(Path.Combine(_hostEnv.ContentRootPath, "App_Data")).FullName;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest();
            }

            var result = await _fileService.Load(id);

            var mimeType = "application/octet-stream";

            var contentProvider = new FileExtensionContentTypeProvider();
            if (contentProvider.TryGetContentType(result.Item1.FileName, out var resolvedType))
            {
                mimeType = resolvedType;
            }

            return File(result.Item2, mimeType, result.Item1.FileName);
        }

        [IgnoreAntiforgeryToken]
        [RequestSizeLimit(100_000_000)]
        [HttpPost]
        public async Task<ActionResult<SavedFile>> Post(IFormFile file)
        {
            if (file == null)
            {
                return BadRequest();
            }

            return await _fileService.Save(file);
        }

    }
}
