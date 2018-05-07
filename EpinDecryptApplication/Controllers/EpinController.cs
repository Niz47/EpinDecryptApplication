using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Extensions.FileProviders;
using System.Diagnostics;
using EpinDecryptApplication.Models;
using System.Security.Cryptography;
using System.Text;
using System;

namespace EpinDecryptApplication.Controllers
{
    public class EpinController : Controller
    {
        private readonly IFileProvider fileProvider;

        public EpinController(IFileProvider fileProvider)
        {
            this.fileProvider = fileProvider;
        }
      
        [HttpPost]
        public async Task<IActionResult> UploadFileViaModel(FileInputModel model)
        {
            if (model == null ||
                model.FileToUpload == null || model.FileToUpload.Length == 0)
                return Content("file not selected");

            string secretKey = model.SecretKey;

            var path = Path.Combine(
                        Directory.GetCurrentDirectory(), "wwwroot/epin",
                        model.FileToUpload.GetFilename());

            string fileName = Path.GetFileNameWithoutExtension(model.FileToUpload.GetFilename());
            string sFileExtension = Path.GetExtension(fileName).ToLower();
            string inputFile = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/epin", model.FileToUpload.GetFilename());
            string outputFile = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/epin", "decry_" + fileName);

            // save encrypted file
            using (var stream = new FileStream(path, FileMode.Create))
            {
                await model.FileToUpload.CopyToAsync(stream);                
            }

            // decrypt file
            try
            {
                byte[] key = ASCIIEncoding.UTF8.GetBytes(secretKey);
                byte[] IV = ASCIIEncoding.UTF8.GetBytes(secretKey);

                RijndaelManaged aes = new RijndaelManaged();
                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.PKCS7;

                ICryptoTransform decryptor = aes.CreateDecryptor(key, IV);

                CryptoStream cs = new CryptoStream(model.FileToUpload.OpenReadStream(), decryptor, CryptoStreamMode.Read);
                FileStream fsOut = new FileStream(outputFile, FileMode.Create);
                int data;
                while ((data = cs.ReadByte()) != -1)
                {
                    fsOut.WriteByte((byte)data);
                }
                model.FileToUpload.OpenReadStream().Close();
                fsOut.Close();
                cs.Close();
            }

            catch (Exception ex)
            {
                // failed to decrypt file
                Console.WriteLine("Errors : " + ex.Message);
            }

            // File Download
            var memory = new MemoryStream();
            using (var stream = new FileStream(outputFile, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            return File(memory, GetContentType(outputFile), Path.GetFileName(outputFile));
            
        }

        private string GetContentType(string path)
        {
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types[ext];
        }

        private Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
            {
                {".pdf", "application/pdf"},
                {".xls", "application/vnd.ms-excel"},
                {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
                {".csv", "text/csv"}
            };
        }
    }
}
