using FastDFS.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Steven.FastDFS.Demo.Provider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Web;

namespace Steven.FastDFS.Demo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private FastDFSProvider _fastDFSProvider;

        public UploadController(FastDFSProvider fastDFSProvider)
        {
            _fastDFSProvider = fastDFSProvider;
        }
        /// <summary>
        /// 文件上传的API
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost]
        [Consumes("multipart/form-data")]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> UploadFileAsync(IFormFile file)
        {
            string filename = await _fastDFSProvider.UploadObjectByteAsync(file.OpenReadStream(), file.FileName);
            return Ok(filename);
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="filename">文件名称 eg: M00/00/00/wKgD-GC4Ua-AKV3QAAIQRwlsiXY728.jpg</param>
        /// <returns></returns>
        [HttpGet("{**filename}")]
        public async Task<IActionResult> DownloadFileAsync(string filename)
        {
            string urlName = _fastDFSProvider.HtmlDecode(filename);
            byte[] fileContent = await _fastDFSProvider.DownloadObjectByteAsync(urlName);
            return File(fileContent, "application/octet-stream");
        }

        /// <summary>
        /// 查看文件信息
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        [HttpGet("detail/{**filename}")]
        public async Task<IActionResult> GetFileInfoAsync(string filename)
        {
            string urlName = _fastDFSProvider.HtmlDecode(filename);
            FDFSFileInfo fileInfo = await _fastDFSProvider.ViewObjectByteAsync(urlName);
            return Ok(fileInfo);
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        [HttpDelete("{**filename}")]
        public async Task<IActionResult> DeleteFileInfoAsync(string filename)
        {
            string msg = "delete fail";
            string urlName = _fastDFSProvider.HtmlDecode(filename);
            bool flag = await _fastDFSProvider.DeleteObjectByteAsync(urlName);
            if (flag)
            {
                msg = "delete success";
            }
            return Ok(msg);
        }
    }
}
