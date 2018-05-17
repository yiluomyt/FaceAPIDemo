using FaceAPIDemo.Web.Domain;
using ImageMagick;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ProjectOxford.Face;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FaceAPIDemo.Web.Controllers
{
    [Produces("application/json")]
    [Route("api/[Controller]")]
    public class FaceController : Controller
    {
        private readonly FaceServiceClient _faceClient;
        private const string groupId = "Your Group Id";

        public FaceController(FaceServiceClient faceClient)
        {
            _faceClient = faceClient;
        }

        /// <summary>
        /// 上次图片获取Face Id
        /// </summary>
        /// <param name="image">待识别的图片</param>
        /// <returns></returns>
        [Route("[Action]")]
        [HttpPost]
        public async Task<JsonResult> Upload(IFormFile image)
        {
            // 启用图片压缩，提高传输速度
            var magickImage = new MagickImage(image.OpenReadStream())
            {
                Quality = 50
            };
            var faces = await _faceClient.DetectAsync(new MemoryStream(magickImage.ToByteArray()));
            // 返回Face Id以及人脸位置信息
            return Json(faces.Select(face => new
            {
                Id = face.FaceId,
                Rect = face.FaceRectangle
            }));
        }

        /// <summary>
        /// 根据Face Id识别人脸
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route("[Action]")]
        [HttpPost]
        public async Task<JsonResult> Identify([FromBody]IdentifyModel model)
        {
            // 识别人脸
            var identifyResults = await _faceClient.IdentifyAsync(groupId, model.Faces, 0.6f);
            List<IdentifyResult> result = new List<IdentifyResult>();
            foreach (var item in identifyResults)
            {
                // 跳过无识别结果的人脸
                if (item.Candidates.Length == 0)
                {
                    continue;
                }
                // 获取第一个识别结果的对应实体
                var person = await _faceClient.GetPersonAsync(groupId, item.Candidates.First().PersonId);
                result.Add(new IdentifyResult
                {
                    Name = person.Name,
                    StudentId = person.UserData
                });
            }
            return Json(result);
        }
    }
}