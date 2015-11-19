using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Newtonsoft.Json;
using System.Drawing;

namespace CameraService.Controllers
{
    [Route("api/[controller]")]
    public class CameraController : Controller
    {
        private readonly Microsoft.AspNet.Hosting.IHostingEnvironment _hostEnvironment;
        public CameraController(Microsoft.AspNet.Hosting.IHostingEnvironment env)
        {
            _hostEnvironment = env;
        }

        // GET: api/[controller]
        [HttpGet]
        public ActionResult GetDevices()
        {

            CameraService.DPP.TemplateMessage result = new DPP.TemplateMessage("result");
            result.id = CameraService.DPP.TemplateMessage.NextId();
            result.result = Camera.AForgeStillWrapper.ListDevices();

            return Content(result.stringify(), "application/json");
        }


        [HttpGet("{id}")]
        public ActionResult GetFresh(int id)
        {


            Bitmap frame = Camera.AForgeStillWrapper.GetFrame(id).Result;

            using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
            {
                frame.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                frame.Dispose();
                return File(stream.ToArray(), "image/jpeg");
            }
        }

        [HttpGet("{id}/GetCameraControlProperties")]
        public ActionResult GetCameraControlProperties(int id)
        {

            CameraService.DPP.TemplateMessage result = new DPP.TemplateMessage("result");
            result.id = CameraService.DPP.TemplateMessage.NextId();
            result.result = Camera.AForgeStillWrapper.GetCameraControlProperties(id);
            
            return Content(result.stringify(), "application/json");
        }

        [HttpGet("{id}/SetCameraControlProperties")]
        public ActionResult SetCameraControlProperties(int id, String Pan, String Tilt, String Roll, String Zoom, String Exposure, String Iris, String Focus)
        {
            List<Camera.AForgeStillWrapper.ContainerForCameraControlPropery> output = new List<Camera.AForgeStillWrapper.ContainerForCameraControlPropery>();
            int pan = 0;
            if(!String.IsNullOrEmpty(Pan) && Int32.TryParse(Pan, out pan))
            {
                // Got Pan
                output.Add(Camera.AForgeStillWrapper.SetCameraControlPropertyManual(id, AForge.Video.DirectShow.CameraControlProperty.Pan, pan));
            }
            else if(!String.IsNullOrEmpty(Pan) && Pan.ToLower().Equals("auto"))
            {
                output.Add(Camera.AForgeStillWrapper.SetCameraControlPropertyAuto(id, AForge.Video.DirectShow.CameraControlProperty.Pan));
            }         

            int tilt = 0;
            if (!String.IsNullOrEmpty(Tilt) && Int32.TryParse(Tilt, out tilt))
            {
                // Got Tilt
                output.Add(Camera.AForgeStillWrapper.SetCameraControlPropertyManual(id, AForge.Video.DirectShow.CameraControlProperty.Tilt, tilt));
            }
            else if (!String.IsNullOrEmpty(Tilt) && Tilt.ToLower().Equals("auto"))
            {
                output.Add(Camera.AForgeStillWrapper.SetCameraControlPropertyAuto(id, AForge.Video.DirectShow.CameraControlProperty.Tilt));
            }

            int roll = 0;
            if (!String.IsNullOrEmpty(Roll) && Int32.TryParse(Roll, out roll))
            {
                // Got Roll
                output.Add(Camera.AForgeStillWrapper.SetCameraControlPropertyManual(id, AForge.Video.DirectShow.CameraControlProperty.Roll, roll));
            }
            else if (!String.IsNullOrEmpty(Roll) && Roll.ToLower().Equals("auto"))
            {
                output.Add(Camera.AForgeStillWrapper.SetCameraControlPropertyAuto(id, AForge.Video.DirectShow.CameraControlProperty.Roll));
            }

            int zoom = 0;
            if (!String.IsNullOrEmpty(Zoom) && Int32.TryParse(Zoom, out zoom))
            {
                // Got Zoom
                output.Add(Camera.AForgeStillWrapper.SetCameraControlPropertyManual(id, AForge.Video.DirectShow.CameraControlProperty.Zoom, zoom));
                
            }
            else if (!String.IsNullOrEmpty(Zoom) && Zoom.ToLower().Equals("auto"))
            {
                output.Add(Camera.AForgeStillWrapper.SetCameraControlPropertyAuto(id, AForge.Video.DirectShow.CameraControlProperty.Zoom));
            }

            int exposure = 0;
            if (!String.IsNullOrEmpty(Exposure) && Int32.TryParse(Exposure, out exposure))
            {
                // Got Exposure
                output.Add(Camera.AForgeStillWrapper.SetCameraControlPropertyManual(id, AForge.Video.DirectShow.CameraControlProperty.Exposure, exposure));
            }
            else if (!String.IsNullOrEmpty(Exposure) && Exposure.ToLower().Equals("auto"))
            {
                output.Add(Camera.AForgeStillWrapper.SetCameraControlPropertyAuto(id, AForge.Video.DirectShow.CameraControlProperty.Exposure));
            }

            int iris = 0;
            if (!String.IsNullOrEmpty(Iris) && Int32.TryParse(Iris, out iris))
            {
                // Got Iris
                output.Add(Camera.AForgeStillWrapper.SetCameraControlPropertyManual(id, AForge.Video.DirectShow.CameraControlProperty.Iris, iris));
            }
            else if (!String.IsNullOrEmpty(Iris) && Iris.ToLower().Equals("auto"))
            {
                output.Add(Camera.AForgeStillWrapper.SetCameraControlPropertyAuto(id, AForge.Video.DirectShow.CameraControlProperty.Iris));
            }

            int focus = 0;
            if (!String.IsNullOrEmpty(Focus) && Int32.TryParse(Focus, out focus))
            {
                // Got Focus
                output.Add(Camera.AForgeStillWrapper.SetCameraControlPropertyManual(id, AForge.Video.DirectShow.CameraControlProperty.Focus, focus));
            }
            else if (!String.IsNullOrEmpty(Focus) && Focus.ToLower().Equals("auto"))
            {
                output.Add(Camera.AForgeStillWrapper.SetCameraControlPropertyAuto(id, AForge.Video.DirectShow.CameraControlProperty.Focus));
            }
            CameraService.DPP.TemplateMessage result = new DPP.TemplateMessage("result");
            result.id = CameraService.DPP.TemplateMessage.NextId();
            result.result = output;

            return Content(result.stringify(), "application/json");
        }

        [HttpGet("{id}/GetVideoResolutionProfiles")]
        public ActionResult GetVideoResolutionProfiles(int id)
        {

            CameraService.DPP.TemplateMessage result = new DPP.TemplateMessage("result");
            result.id = CameraService.DPP.TemplateMessage.NextId();
            result.result = Camera.AForgeStillWrapper.GetVideoResolutionProfiles(id);

            return Content(result.stringify(), "application/json");
        }

        [HttpGet("{id}/GetVideoResolutionProfile")]
        public ActionResult GetVideoResolutionProfile(int id)
        {

            CameraService.DPP.TemplateMessage result = new DPP.TemplateMessage("result");
            result.id = CameraService.DPP.TemplateMessage.NextId();
            result.result = Camera.AForgeStillWrapper.GetVideoResolutionProfile(id);

            return Content(result.stringify(), "application/json");
        }

        [HttpGet("{id}/SetVideoResolutionProfile/{profile}")]
        public ActionResult SetVideoResolutionProfile(int id, int profile)
        {

            CameraService.DPP.TemplateMessage result = new DPP.TemplateMessage("result");
            result.id = CameraService.DPP.TemplateMessage.NextId();
            result.result = Camera.AForgeStillWrapper.SetVideoResolutionProfile(id, profile);

            return Content(result.stringify(), "application/json");
        }

        

        /// <summary>
        /// This is the main beef
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="d"></param>
        /// <param name="th"></param>
        /// <returns></returns>
        // GET api/[controller]/0  optional ?d=NNNN&h
        [HttpGet("{id}/compare/{name}")]
        public ActionResult Compare(int id, string name, string d, int th = 60)
        {
            Camera.AForgeStillWrapper.PingDeviceWasAlive(id);
            String filePath = _hostEnvironment.WebRootPath + "\\static\\Camera\\" + id;
            String filePathDiff = _hostEnvironment.WebRootPath + "\\static\\Camera\\" + id + "\\diff";
            String filePathLeft = _hostEnvironment.WebRootPath + "\\static\\Camera\\" + id + "\\left";
            String filePathRight = _hostEnvironment.WebRootPath + "\\static\\Camera\\" + id + "\\right";
            String filePathLast = _hostEnvironment.WebRootPath + "\\static\\Camera\\" + id + "\\last";

            List<String> files = new List<String>();
            files.Add(filePath);
            files.Add(filePathDiff);
            files.Add(filePathLeft);
            files.Add(filePathRight);
            files.Add(filePathLast);

            foreach(String f in files)
            {
                if (!System.IO.Directory.Exists(f))
                {
                    System.IO.Directory.CreateDirectory(f);
                }
            }

            String fileName = name + ".png";
            String fullPath = System.IO.Path.Combine(filePathLast, fileName);

            Bitmap right = null;
            try
            {
                right = Camera.AForgeStillWrapper.GetFrame(id).Result;
                right.Save(fullPath);
            }
            catch (Exception e)
            {
                CameraService.DPP.TemplateMessage errmsg = new DPP.TemplateMessage("result");
                errmsg.id = CameraService.DPP.TemplateMessage.NextId();
                errmsg.error = new DPP.TemplateMessage.Error("Internal");
                errmsg.error.reason = "Could not get frame from camera";
                errmsg.error.offendingMessage = e.Message;

                return Content(errmsg.stringify().ToString(), "application/json");
            }

            Bitmap left = null;
            //  Check for comparison picture
            fullPath = System.IO.Path.Combine(filePath, fileName);
            if (System.IO.File.Exists(fullPath))
            {
                left = (Bitmap)Image.FromFile(fullPath);
            }
            else
            {
                // If File does not Exists make this the reference.
                fullPath = System.IO.Path.Combine(filePath, fileName);
                right.Save(fullPath);
                left = (Bitmap)right.Clone();
            }

            // Possible crop.
            if (!String.IsNullOrEmpty(d))
            {
                try
                {
                    int x = 0;
                    int y = 0;
                    int widht = 640;
                    int height = 480;
                    //   widht x height + x + y
                    // d=165x280+85+90
                    // [165x280][85][90]
                    d = d.Replace(' ', '+');
                    String[] tmp = d.Split('+');

                    x = Int32.Parse(tmp[1]);
                    y = Int32.Parse(tmp[2]);

                    // [165][280]
                    String[] dimemsions = tmp[0].Split('x');
                    widht = Int32.Parse(dimemsions[0]);
                    height = Int32.Parse(dimemsions[1]);


                    System.Drawing.Rectangle cropArea = new Rectangle(x, y, widht, height);

                    // Crop left
                    Bitmap nleft = new Bitmap(cropArea.Width, cropArea.Height);
                    Graphics gr = Graphics.FromImage(nleft);
                    gr.DrawImage(left, -cropArea.X, -cropArea.Y);
                    gr.Dispose();
                    left.Dispose();
                    left = nleft;

                    // Crop right
                    Bitmap nright = new Bitmap(cropArea.Width, cropArea.Height);
                    gr = Graphics.FromImage(nright);
                    gr.DrawImage(right, -cropArea.X, -cropArea.Y);
                    gr.Dispose();
                    right.Dispose();
                    right = nright;
                }
                catch (Exception e)
                {
                    CameraService.DPP.TemplateMessage errmsg = new DPP.TemplateMessage("result");
                    errmsg.id = CameraService.DPP.TemplateMessage.NextId();
                    errmsg.error = new DPP.TemplateMessage.Error("Internal");
                    errmsg.error.reason = "Cropping failed with the parameter d = " + d;
                    errmsg.error.offendingMessage = e.Message;

                    return Content(errmsg.stringify().ToString(), "application/json");

                }
            }

            fullPath = System.IO.Path.Combine(filePathLeft, fileName);
            left.Save(fullPath);

            fullPath = System.IO.Path.Combine(filePathRight, fileName);
            right.Save(fullPath);

            // Compare
            AForge.Imaging.Filters.ThresholdedDifference filter = new AForge.Imaging.Filters.ThresholdedDifference(th);
            filter.OverlayImage = left;
            Bitmap resultImage = filter.Apply(right);

            // Safe resultImage to diff folder
            fullPath = System.IO.Path.Combine(filePathDiff, fileName);
            resultImage.Save(fullPath);



            // Dispose the images
            left.Dispose();
            right.Dispose();
            resultImage.Dispose();

            int diff = filter.WhitePixelsCount;

            CameraService.DPP.TemplateMessage result = new DPP.TemplateMessage("result");
            result.id = CameraService.DPP.TemplateMessage.NextId();
            result.result = diff;

            return Content(result.stringify().ToString(), "application/json");
        }

        // Legacy
        // GET api/[controller]/0 
        [HttpGet("{id}/lastToNewReference/{name}")]
        public ActionResult SaveLatestAs(int id, string name)
        {
            CameraService.Camera.AForgeStillWrapper.PingDeviceWasAlive(id);
            CameraService.DPP.TemplateMessage result = new DPP.TemplateMessage("result");
            result.id = CameraService.DPP.TemplateMessage.NextId();

            String last = _hostEnvironment.WebRootPath + "\\static\\Camera\\" + id + "\\right\\" + name + ".png";
            String target = _hostEnvironment.WebRootPath + "\\static\\Camera\\" + id + "\\" + name + ".png";

            try
            {
                System.IO.File.Copy(last, target, true);
            }
            catch (Exception e)
            {
                result.error = new DPP.TemplateMessage.Error("fs");
                result.error.reason = "Failed to copy last as latest";
                result.error.offendingMessage = e.Message;
            }
            result.result = "OK";
            return Content(result.stringify().ToString(), "application/json");

        }

        // Legacy
        // GET api/[controller]/0 
        [HttpGet("{id}/get/{name}")]
        public ActionResult GetImageRoot(int id, string name)
        {
            String filePath = _hostEnvironment.WebRootPath + "\\static\\Camera\\" + id;

            String fileName = name + ".png";
            String fullPath = System.IO.Path.Combine(filePath, fileName);

            Bitmap frame = null;

            if (System.IO.File.Exists(fullPath))
            {
                frame = (Bitmap)Image.FromFile(fullPath);
            }

            if (frame == null)
            {
                frame = new Bitmap(640, 480);
            }
            using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
            {
                frame.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                frame.Dispose();
                return File(stream.ToArray(), "image/jpeg");
            }

        }

        // GET api/[controller]/0 
        [HttpGet("{id}/get/{folder}/{name}")]
        public ActionResult GetImage(int id, string folder, string name)
        {
            String filePath = _hostEnvironment.WebRootPath + "\\static\\Camera\\" + id + "\\" + folder;

            String fileName = name + ".png";
            String fullPath = System.IO.Path.Combine(filePath, fileName);

            Bitmap frame = null;

            if (System.IO.File.Exists(fullPath))
            {
                frame = (Bitmap)Image.FromFile(fullPath);
            }

            if(frame == null)
            {
                frame = new Bitmap(640, 480);
            }
            using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
            {
                frame.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                frame.Dispose();
                return File(stream.ToArray(), "image/jpeg");
            }

        }



        // POST api/[controller]
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/[controller]/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // Modern
        // DELETE api/[controller]/5
        [HttpDelete("{id}/compare/{name}")]
        public void Delete(int id, string name)
        {
            CameraService.Camera.AForgeStillWrapper.PingDeviceWasAlive(id);

            List<String> files = new List<String>();
            files.Add(_hostEnvironment.WebRootPath + "\\static\\Camera\\" + id + "\\" + name + ".png");
            files.Add(_hostEnvironment.WebRootPath + "\\static\\Camera\\" + id + "\\diff\\" + name + ".png");
            files.Add(_hostEnvironment.WebRootPath + "\\static\\Camera\\" + id + "\\left\\" + name + ".png");
            files.Add(_hostEnvironment.WebRootPath + "\\static\\Camera\\" + id + "\\right\\" + name + ".png");
            files.Add(_hostEnvironment.WebRootPath + "\\static\\Camera\\" + id + "\\last\\" + name + ".png");

            foreach (String file in files)
            {
                try
                {
                    System.IO.File.Delete(file);
                }
                catch
                {
                    // void
                }
            }
        }


        // Legacy
        // GET api/[controller]/0 
        [HttpGet("{id}/clear/{name}")]
        public ActionResult Clear(int id, string name)
        {
            CameraService.Camera.AForgeStillWrapper.PingDeviceWasAlive(id);
            List<String> files = new List<String>();
            files.Add(_hostEnvironment.WebRootPath + "\\static\\Camera\\" + id + "\\" + name + ".png");
            files.Add(_hostEnvironment.WebRootPath + "\\static\\Camera\\" + id + "\\diff\\" + name + ".png");
            files.Add(_hostEnvironment.WebRootPath + "\\static\\Camera\\" + id + "\\left\\" + name + ".png");
            files.Add(_hostEnvironment.WebRootPath + "\\static\\Camera\\" + id + "\\right\\" + name + ".png");
            files.Add(_hostEnvironment.WebRootPath + "\\static\\Camera\\" + id + "\\last\\" + name + ".png");

            foreach (String file in files)
            {
                try
                {
                    System.IO.File.Delete(file);
                }
                catch
                {
                    // void
                }
            }

            CameraService.DPP.TemplateMessage result = new DPP.TemplateMessage("result");
            result.id = CameraService.DPP.TemplateMessage.NextId();
            result.result = "OK";
            return Content(result.stringify().ToString(), "application/json");
        }
    }
}
