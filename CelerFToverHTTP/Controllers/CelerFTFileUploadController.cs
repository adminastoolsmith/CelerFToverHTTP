using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.IO;
using System.Collections.Specialized;
using System.Web;
using System.Configuration;
using System.Text;
using System.Security.Cryptography;



namespace CelerFToverHTTP.Controllers
{
    public class CelerFTFileUploadController : ApiController
    {
      

        private string getFileFolder(string directoryname)
        {
            
            var folder = ConfigurationManager.AppSettings["uploadpath"] + "\\" + directoryname;

            if (!System.IO.Directory.Exists(folder))
            {
                System.IO.Directory.CreateDirectory(folder);
            }

            return folder;
        }

        private static string GetHashFromFile(string fileName, HashAlgorithm algorithm)
        {
            using (var stream = new BufferedStream(File.OpenRead(fileName), (1024*1024)))
            {
                return BitConverter.ToString(algorithm.ComputeHash(stream)).Replace("-", string.Empty);
            }
        }

        private async Task<HttpResponseMessage> ProcessChunk(string filename, string directoryname, int chunknumber, int numberofChunks)
        {
            // Check if the request contains multipart/form-data.            
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            // Check that we are not trying to upload a file greater than 50MB
            Int32 maxinputlength = 51 * 1024 * 1024;

            if (Convert.ToInt32(HttpContext.Current.Request.InputStream.Length) > maxinputlength)
            {
                return Request.CreateErrorResponse(HttpStatusCode.RequestEntityTooLarge, "Maximum upload chunk size exceeded");
            }

            try
            {

                byte[] filedata = null;

                // If we have the custome header then we are processing hand made multipart-form-data
                if (HttpContext.Current.Request.Headers["CelerFT-Encoded"] != null)
                {

                    // Read in the request
                    HttpPostedFileBase base64file = new HttpPostedFileWrapper(HttpContext.Current.Request.Files["Slice"]);

                    if (base64file == null)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No file chunk uploaded");
                    }

                    // Convert the base64 string into a byte array
                    byte[] base64filedata = new byte[base64file.InputStream.Length];
                    await base64file.InputStream.ReadAsync(base64filedata, 0, Convert.ToInt32(HttpContext.Current.Request.InputStream.Length));

                    var base64string = System.Text.UTF8Encoding.UTF8.GetString(base64filedata);

                    filedata = Convert.FromBase64String(base64string);

                }
                else
                {

                    HttpPostedFileBase file = new HttpPostedFileWrapper(HttpContext.Current.Request.Files["Slice"]);

                    if (file == null)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No file chunk uploaded");
                    }

                    filedata = new byte[file.InputStream.Length];
                    await file.InputStream.ReadAsync(filedata, 0, Convert.ToInt32(HttpContext.Current.Request.InputStream.Length));

                }

                if (filedata == null)
                {

                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No file chunk uploaded");
                }

                // Write the byte array to a file
                var newfilename = filename.Split('.');
                string baseFilename = Path.GetFileNameWithoutExtension(filename);
                string extension = Path.GetExtension(filename);

                string tempdirectoryname = Path.GetFileNameWithoutExtension(filename);
                var localFilePath = getFileFolder(directoryname + "\\" + tempdirectoryname) + "\\" + baseFilename + "." + chunknumber.ToString().PadLeft(16, Convert.ToChar("0")) + "." + extension + ".tmp";


                var input = new MemoryStream(filedata);
                var outputFile = File.Open(localFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);

                await input.CopyToAsync(outputFile);
                input.Close();
                outputFile.Close();


                filedata = null;

                return new HttpResponseMessage()
                {
                    Content = new StringContent(localFilePath),
                    StatusCode = HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [System.Web.Http.HttpPost]
        public async Task<HttpResponseMessage> UploadChunk(string filename, string directoryname, int chunknumber, int numberofChunks)
        {

            HttpResponseMessage returnmessage = await ProcessChunk(filename, directoryname, chunknumber, numberofChunks);
            return returnmessage;
        }

        [System.Web.Http.HttpPost]
        public async Task<HttpResponseMessage> UploadChunk1(string filename, string directoryname, int chunknumber, int numberofChunks)
        {

            HttpResponseMessage returnmessage = await ProcessChunk(filename, directoryname, chunknumber, numberofChunks);
            return returnmessage;
        }

        [System.Web.Http.HttpPost]
        public async Task<HttpResponseMessage> UploadChunk2(string filename, string directoryname, int chunknumber, int numberofChunks)
        {

            HttpResponseMessage returnmessage = await ProcessChunk(filename, directoryname, chunknumber, numberofChunks);
            return returnmessage;
        }

        [System.Web.Http.HttpPost]
        public async Task<HttpResponseMessage> UploadChunk3(string filename, string directoryname, int chunknumber, int numberofChunks)
        {

            HttpResponseMessage returnmessage = await ProcessChunk(filename, directoryname, chunknumber, numberofChunks);
            return returnmessage;
        }

        [System.Web.Http.HttpPost]
        public async Task<HttpResponseMessage> UploadChunk4(string filename, string directoryname, int chunknumber, int numberofChunks)
        {

            HttpResponseMessage returnmessage = await ProcessChunk(filename, directoryname, chunknumber, numberofChunks);
            return returnmessage;
        }

        [System.Web.Http.HttpPost]
        public async Task<HttpResponseMessage> UploadChunk5(string filename, string directoryname, int chunknumber, int numberofChunks)
        {

            HttpResponseMessage returnmessage = await ProcessChunk(filename, directoryname, chunknumber, numberofChunks);
            return returnmessage;
        }
      
        [System.Web.Http.HttpGet]
        public HttpResponseMessage MergeAll(string filename, string directoryname, int numberofChunks)
        {
           
            string tempdirectoryname = Path.GetFileNameWithoutExtension(filename);
            var localFilePath = getFileFolder(directoryname + "\\" + tempdirectoryname) + "\\";
            DirectoryInfo diSource = new DirectoryInfo(localFilePath);
            string baseFilename = Path.GetFileNameWithoutExtension(filename);
            string extension = Path.GetExtension(filename);

            // If the number of uploaded files is less than the total number of files then             
            // return an error. This will happen in asynchronous file uploads where the final             
            // chunk arrives before other chunks 
            if (diSource.GetFiles("*.tmp").Length != numberofChunks)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Number of file chunks less than total count");
            }

            FileStream outputFile = new FileStream(localFilePath + baseFilename + extension, FileMode.OpenOrCreate, FileAccess.Write);

            try
            {
                // Get all of the file chunks in the directory and use them to create the file.
                // All of the file chunks are named in sequential order.
                foreach (FileInfo fiPart in diSource.GetFiles("*.tmp")) {

                    byte[] filedata = System.IO.File.ReadAllBytes(fiPart.FullName);
                    outputFile.Write(filedata, 0, filedata.Length);
                    File.Delete(fiPart.FullName);

                }

                outputFile.Flush();
                outputFile.Close();

                // Move the file to the top level directory
                string oldfilelocation = localFilePath + baseFilename + extension;
                string newfilelocation = getFileFolder(directoryname + "\\") + baseFilename + extension;

                // Check if the file exists. If it does delete it then move the file
                if(System.IO.File.Exists(newfilelocation)) {
                    System.IO.File.Delete(newfilelocation);
                }
                System.IO.File.Move(oldfilelocation, newfilelocation);

                // Delete the temporary directory
                System.IO.Directory.Delete(localFilePath);

                // Get the MD5 hash for the file and send it back to the client
                HashAlgorithm MD5 = new MD5CryptoServiceProvider();
                //string checksumMd5 = GetHashFromFile(localFilePath + baseFilename + extension, MD5);
                string checksumMd5 = GetHashFromFile(newfilelocation, MD5);
                
                
                return new HttpResponseMessage()
                {
                    Content = new StringContent("Sucessfully merged file " + filename + "," + checksumMd5),
                    StatusCode = HttpStatusCode.OK
                };


            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }

        }

        
    }

  
}
