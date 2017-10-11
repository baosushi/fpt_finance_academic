using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace CaptstoneProject.Controllers
{
    public class DataImportController : MyBaseController
    {
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/drive-dotnet-quickstart.json
        static string[] Scopes = { DriveService.Scope.DriveReadonly };
        static string ApplicationName = "Drive API .NET Quickstart";

        public void Sgbjfxdkfbhk()
        {
            UserCredential credential;
            var path = Server.MapPath("~");
            using (var stream = new FileStream(path + "client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, ".credentials/drive-dotnet-quickstart.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Debug.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Drive API service.
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            // Define parameters of request.
            FilesResource.ListRequest listRequest = service.Files.List();
            listRequest.PageSize = 10;
            listRequest.Fields = "nextPageToken, files(id, name)";

            // List files.
            IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute().Files;
            Debug.WriteLine("Files:");
            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    Debug.WriteLine("{0} ({1})", file.Name, file.Id);
                }
            }
            else
            {
                Debug.WriteLine("No files found.");
            }


        }

        public void Toast()
        {
            string searchPattern = "*";
            var path = Server.MapPath("~");

            DirectoryInfo di = new DirectoryInfo(path + "Khao thi/");
            DirectoryInfo[] directories =
                di.GetDirectories(searchPattern, SearchOption.TopDirectoryOnly);

            FileInfo[] files =
                di.GetFiles(searchPattern, SearchOption.TopDirectoryOnly);

            try
            {
                foreach (DirectoryInfo dir in directories)
                {
                    var subFiles = dir.GetFiles("*Ketquahoctap*.xls?", SearchOption.AllDirectories);
                    subFiles.Concat(dir.GetFiles("*Ket qua hoc tap*.xls?", SearchOption.AllDirectories));
                    subFiles = subFiles.Where(q => !q.Name.StartsWith("_")).ToArray();

                    foreach (var excel in subFiles)
                    {
                        HSSFWorkbook hssfwb;
                        using (FileStream file = excel.OpenRead())
                        {
                            hssfwb = new HSSFWorkbook(file);
                        }

                        ISheet result = hssfwb.GetSheetAt(0);
                        ISheet component = hssfwb.GetSheetAt(1);

                        var markCompRow = component.GetRow(1);
                        for (int i = 3; i <= component.LastRowNum; i++)
                        {
                            var row = component.GetRow(i);
                            if (row != null) //null is when the row only contains empty cells 
                            {
                                var loginName = row.Cells[0].StringCellValue.Trim().ToLower();
                                var average = 0.0;
                                for (int j = 5; j <= row.Cells.Count; j++)
                                {
                                    if(row.GetCell(j, MissingCellPolicy.RETURN_NULL_AND_BLANK) != null)
                                    {
                                        average += row.GetCell(j, MissingCellPolicy.RETURN_NULL_AND_BLANK).NumericCellValue * markCompRow.Cells[j].NumericCellValue;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }
    }
}