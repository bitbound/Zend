using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        Task Delete(string id);
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

        public async Task Delete(string id)
        {
            if (!Guid.TryParse(id, out var idResult))
            {
                return;
            }

            var savedFile = await _appDb.SavedFiles.FindAsync(idResult);

            if (savedFile is not null)
            {
                _appDb.SavedFiles.Remove(savedFile);
                await _appDb.SaveChangesAsync();
            }

            try
            {
                if (Directory.Exists(_appData))
                {
                    var fsFile = Directory.EnumerateFiles(_appData).FirstOrDefault(x => x.Contains(id));
                    if (fsFile is not null)
                    {
                        File.Delete(fsFile);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while deleting file: {ex.Message}");
            }

        }

        public async Task<Tuple<SavedFile, Stream>> Load(Guid fileId)
        {
            var savedFile = await _appDb.SavedFiles.FindAsync(fileId);
            var filePath = Path.Combine(_appData, $"{savedFile.Id}{Path.GetExtension(savedFile.FileName)}");

            if (!File.Exists(filePath))
            {
                return new Tuple<SavedFile, Stream>(null, null);
            }

            var fs = new FileStream(filePath, FileMode.Open);

            return new Tuple<SavedFile, Stream>(savedFile, fs);
        }

        public async Task<SavedFile> Save(IFormFile uploadedFile)
        {
            var savedFile = new SavedFile()
            {
                FileName = uploadedFile.FileName,
                UploadedAt = DateTimeOffset.Now,
                ContentDisposition = uploadedFile.ContentDisposition,
                ContentType = uploadedFile.ContentType,
                FileSize = uploadedFile.Length
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
