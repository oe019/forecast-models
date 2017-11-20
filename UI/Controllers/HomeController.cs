using System;
using System.IO;
using System.Web.Mvc;
using Abstraction;

namespace FortellMe.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home//
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult UpLoadAndPrepareRespond()
        {
            if (Request.Files.Count > 0)
            {
                var file = Request.Files[0];
                if (file != null && file.ContentLength > 0)
                {
                    string sourcePath;
                    string targetPath;
                    sourcePath = string.Format("{0}Uploaded\\{1}", AppDomain.CurrentDomain.BaseDirectory, Request.Files[0].FileName);
                    file.SaveAs(sourcePath);
                    targetPath = Path.Combine(Server.MapPath("~/Download/"), "ResultFile.csv");
                    IService service = Forcasting.Services.ServiceAccess.AccessForcastingService();
                    service.ResponseCSVFile(sourcePath, targetPath); // Services Framework is Invoked Here
                    return View();
                }
            }
                return PartialView("_ErrorView");
            throw new Exception("There is no file found to Save and Process");
        }
        [HttpPost]
        public ActionResult Downloads()
        {
            string Targetpath = Path.Combine(Server.MapPath("~/Download/"), "ResultFile.csv");
            FileInfo file = new FileInfo(Targetpath);
            if (file.Exists)
            {
                Response.Clear();
                Response.ClearHeaders();
                Response.ClearContent();
                Response.AddHeader("Content-Disposition", "attachment; filename=" + file.Name);
                Response.AddHeader("Content-Length", file.Length.ToString());
                Response.ContentType = "text/plain";
                Response.Flush();
                Response.TransmitFile(file.FullName);
                Response.End();
            }
            else
            {
                throw new Exception("File Not Found");
            }
            return View();
        }
        public void PathPrepare()
        {
            string CSVpath = string.Format("{0}Uploaded\\{1}", AppDomain.CurrentDomain.BaseDirectory, Request.Files[0].FileName);
        }
    }
}