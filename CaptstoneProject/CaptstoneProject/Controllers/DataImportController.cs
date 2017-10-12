using DataService.Model;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using OfficeOpenXml;
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

            FileInfo[] files =
                di.GetFiles(searchPattern, SearchOption.TopDirectoryOnly);

            try
            {
                //Microsoft.Office.Interop.Excel.Application app = new Microsoft.Office.Interop.Excel.Application();
                using (var context = new DB_Finance_AcademicEntities())
                {
                    foreach (var excel in files)
                    {
                        HSSFWorkbook hssfwb;
                        using (FileStream file = excel.OpenRead())
                        {
                            hssfwb = new HSSFWorkbook(file);
                        }

                        ISheet component = hssfwb.GetSheetAt(0);

                        var markCompRow = component.GetRow(8);
                        var titleRow = 7;

                        //Microsoft.Office.Interop.Excel.Workbook xls;
                        //xls = app.Workbooks.Open(excel.FullName);
                        //xls.SaveAs(path + "Khao thi/New files/" + excel.Name, Microsoft.Office.Interop.Excel.XlFileFormat.xlExcel8);

                        for (int i = 9; i <= component.LastRowNum; i++)
                        {
                            var row = component.GetRow(i);
                            if (row != null) //null is when the row only contains empty cells
                            {
                                var studentCode = row.Cells[1].StringCellValue.Trim().ToLower();
                                var studentInCourse = context.StudentInCourses.Where(q => q.Student.StudentCode.ToUpper().Equals(studentCode)).FirstOrDefault();

                                var average = 0.0;
                                for (int j = 4; j <= row.Cells.Count; j++)
                                {
                                    if (row.GetCell(j, MissingCellPolicy.RETURN_NULL_AND_BLANK) != null)
                                    {
                                        average += row.GetCell(j, MissingCellPolicy.RETURN_NULL_AND_BLANK).NumericCellValue * markCompRow.Cells[j].NumericCellValue;
                                    }
                                }
                            }
                        }
                    }
                    //app.Workbooks.Close();
                    //app.Quit();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }
    }
}