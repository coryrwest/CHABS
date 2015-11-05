using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;

namespace CRWestropp.Utilities.ImageFinder {
    public class ImageFinder {
        public List<ImageWithDimensions> FindImages(string HTML) {
            // Get img tags from HTML
            List<string> imgTags = GetImgTagsFromHTML(HTML);

            // Get img src from img tags
            List<string> imgURLS = GetImgURLSFromTag(imgTags);

            // Set up ImageWD Object list
            List<ImageWithDimensions> ImageWDs = new List<ImageWithDimensions>();
            foreach (var imgURL in imgURLS) {
                ImageWithDimensions image = new ImageWithDimensions();
                image = GetDimensionsFromImage(DownloadImageFromURL(imgURL));
                ImageWDs.Add(image);
            }

            return ImageWDs;
        }

        #region Private Methods
        /// <summary>
        /// Gets the img tags from a block of HTML
        /// </summary>
        /// <param name="HTML"></param>
        /// <returns></returns>
        private List<string> GetImgTagsFromHTML(string HTML) {
            List<string> imgs = new List<string>();

            while (HTML.IndexOf("<img") != -1) {
                int startIndex = HTML.IndexOf("<img");
                int endIndex = IndexAfter(HTML, startIndex, ">");
                string img = HTML.Substring(startIndex, endIndex - startIndex + 1);
                imgs.Add(img);
                HTML = HTML.Replace(img, "");
            }

            return imgs;
        }

        /// <summary>
        /// Gets the urls from a list of HTML img tags. Tags must contain src.
        /// </summary>
        /// <param name="imgs"></param>
        /// <returns></returns>
        private List<string> GetImgURLSFromTag(List<string> imgs) {
            List<string> imgURLS = new List<string>();

            foreach (var img in imgs) {
                int startIndex = img.IndexOf("src=\"") + 5;
                int endIndex = IndexAfter(img, startIndex, "\"") - 1;
                string imgURL = img.Substring(startIndex, endIndex - startIndex + 1);
                // Check for HTTP
                if (imgURL.Contains("http")) {
                    if (!imgURL.Contains("?")) {
                        imgURLS.Add(imgURL);
                    }
                }
            }

            return imgURLS;
        }

        /// <summary>
        /// Get the dimesions of an Image object. This is SLOW.
        /// </summary>
        /// <param name="imageURL"></param>
        /// <returns>An image with dimensions object</returns>
        private ImageWithDimensions GetDimensionsFromImage(Image image) {
            ImageWithDimensions imageWD = new ImageWithDimensions();
            var stream = new System.IO.MemoryStream();
            image.Save(stream, ImageFormat.Jpeg);

            using (Image jpg = Image.FromStream(stream: stream, useEmbeddedColorManagement: false, validateImageData: false)) {
                imageWD.Width = Convert.ToInt32(jpg.PhysicalDimension.Width);
                imageWD.Height = Convert.ToInt32(jpg.PhysicalDimension.Height);
            }

            imageWD.Image = image;

            return imageWD;
        }

        /// <summary>
        /// Downloads image from url and returns it
        /// </summary>
        /// <param name="imageURL"></param>
        /// <returns></returns>
        private Image DownloadImageFromURL(string imageURL) {
            HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(imageURL);
            HttpWebResponse httpWebReponse = (HttpWebResponse)httpWebRequest.GetResponse();
            Stream stream = httpWebReponse.GetResponseStream();
            return Image.FromStream(stream);
        }

        private int IndexAfter(string text, int startIndex, string match) {
            text = text.Substring(startIndex);
            return text.IndexOf(match) + startIndex;
        }
        #endregion
    }

    public class ImageHelpers {
        public void SaveJpegToDisk(Image image, string fileName, string path) {
            image.Save(path + fileName, ImageFormat.Jpeg);
        }
    }

    public class ImageWithDimensions {
        public string URL { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public Image Image { get; set; }
    }

    public class ImageSizer {
        public List<ImageWithDimensions> GetLargestImages(List<ImageWithDimensions> images) {
            int largestDimensions = 0;
            List<ImageWithDimensions> largestImages = new List<ImageWithDimensions>();
            List<ImageWithDimensions> finalLargestImages = new List<ImageWithDimensions>();

            // Filter once to find largest dimesions
            foreach (ImageWithDimensions image in images) {
                int imageDimensions = image.Width * image.Height;
                if (imageDimensions >= largestDimensions) {
                    largestDimensions = imageDimensions;
                    largestImages.Add(image);
                }
            }

            // Filter again against the largest dimension
            foreach (ImageWithDimensions image in largestImages) {
                int imageDimensions = image.Width * image.Height;
                if (imageDimensions >= largestDimensions) {
                    finalLargestImages.Add(image);
                }
            }

            return finalLargestImages;
        }

        public ImageWithDimensions GetFirstLargestImage(List<ImageWithDimensions> images) {
            List<ImageWithDimensions> imageList = GetLargestImages(images);
            return imageList.First();
        }
    }
}
