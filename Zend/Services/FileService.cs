using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Zend.Models;

namespace Zend.Services
{
    public interface IFileService
    {
        Task<Tuple<SavedFile, Stream>> Load(Guid fileId);

        Task<SavedFile> Save(IFormFile uploadedFile);
    }
    public class FileService : IFileService
    {
        private readonly string _appData;
        private readonly AppDb _appDb;
        private readonly IWebHostEnvironment _hostEnv;
        public FileService(
            AppDb appDb,
            IWebHostEnvironment hostEnv)
        {
            _appDb = appDb;
            _hostEnv = hostEnv;
            _appData = Directory.CreateDirectory(Path.Combine(_hostEnv.ContentRootPath, "App_Data")).FullName;
        }

        public async Task<Tuple<SavedFile, Stream>> Load(Guid fileId)
        {
            var savedFile = await _appDb.SavedFiles.FindAsync(fileId);
            var filePath = Path.Combine(_appData, $"{savedFile.Id}{Path.GetExtension(savedFile.FileName)}");
            var fs = new FileStream(filePath, FileMode.Create);

            return new Tuple<SavedFile, Stream>(savedFile, fs);
        }

        public async Task<SavedFile> Save(IFormFile uploadedFile)
        {
            var savedFile = new SavedFile()
            {
                FileName = uploadedFile.FileName,
                UploadedAt = DateTimeOffset.Now,
                ContentDisposition = uploadedFile.ContentDisposition,
                ContentType = uploadedFile.ContentType
            };

            _appDb.SavedFiles.Add(savedFile);
            await _appDb.SaveChangesAsync();

            var filePath = Path.Combine(_appData, $"{savedFile.Id}{Path.GetExtension(savedFile.FileName)}");
            using var fs = new FileStream(filePath, FileMode.Create);
            await uploadedFile.CopyToAsync(fs);

            return savedFile;

        }
    }
}
