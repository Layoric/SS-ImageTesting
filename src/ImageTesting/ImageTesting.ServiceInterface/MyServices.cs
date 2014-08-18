using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using ServiceStack;
using ImageTesting.ServiceModel;
using ServiceStack.Web;

namespace ImageTesting.ServiceInterface
{
    public class MyServices : Service
    {
        public object Any(Hello request)
        {
            return new HelloResponse { Result = "Hello, {0}!".Fmt(request.Name) };
        }
    }

    [Route("/HelloImage")]
    public class HelloImage
    {
        public string ContentTypeOverride { get; set; }
    }

    public class HelloImageService : Service
    {
        public object Any(HelloImage request)
        {
            string contentType = request.ContentTypeOverride ?? "image/png";
            using (Bitmap image = new Bitmap(10, 10))
            {
                using (Graphics g = Graphics.FromImage(image))
                {
                    g.Clear(Color.Red);
                }
                var ms = new MemoryStream();
                image.Save(ms, ImageFormat.Png);
                return new HttpResult(ms, contentType); //writes stream directly to response
            }
        }
    }

    [Route("/HelloImage2")]
    public class HelloImage2
    {
        public string ContentTypeOverride { get; set; }
    }

    public class HelloImage2Service : Service
    {
        public object Any(HelloImage2 request)
        {
            string contentType = request.ContentTypeOverride ?? "image/png";
            using (Bitmap image = new Bitmap(10, 10))
            {
                using (Graphics g = Graphics.FromImage(image))
                {
                    g.Clear(Color.Blue);
                }
                using (MemoryStream m = new MemoryStream())
                {
                    image.Save(m, ImageFormat.Png);
                    var imageData = m.ToArray(); //buffers
                    return new HttpResult(imageData, contentType);
                }
            }
        }
    }

    [Route("/HelloImage3")]
    public class HelloImage3
    {
        public string ContentTypeOverride { get; set; }
    }

    //Your own re-usable Custom ImageResult, writes directly to response stream
    public class ImageResult : IDisposable, IStreamWriter, IHasOptions
    {
        private readonly Image image;
        private readonly ImageFormat imgFormat;

        public ImageResult(Image image, ImageFormat imgFormat = null)
        {
            this.image = image;
            this.imgFormat = imgFormat ?? ImageFormat.Png;
            this.Options = new Dictionary<string, string> {
                { HttpHeaders.ContentType, "image/" + this.imgFormat.ToString().ToLower() }
            };
        }

        public void WriteTo(Stream responseStream)
        {
            image.Save(responseStream, imgFormat); //Writes to response stream here
        }

        public void Dispose()
        {
            this.image.Dispose();
        }

        public IDictionary<string, string> Options { get; set; }
    }

    public class HelloImage3Service : Service
    {
        public object Any(HelloImage3 request)
        {
            var image = new Bitmap(10, 10);
            using (var g = Graphics.FromImage(image))
                g.Clear(Color.Green);

            return new ImageResult(image); //terse + explicit is good :)
        }
    }
}