namespace Steven.FastDFS.Demo.Provider
{
    using global::FastDFS.Client;
    using System.IO;
    using System.Threading.Tasks;
    using System.Web;

    /// <summary>
    /// FastDFS操作提供类
    /// </summary>
    public class FastDFSProvider
    {
        /// <summary>
        /// 文件上传
        /// </summary>
        /// <param name="objectByte">上传对象对应的字节数组</param>
        /// <param name="objectName">对象名</param>
        /// <param name="groupName">分组</param>
        /// <returns></returns>
        public async Task<string> UploadObjectByteAsync(Stream objectStream, string fileName)
        {
            // 获取要上传的storage-server节点
            var storageNode = await FastDFSClient.GetStorageNodeAsync();
            var filePath = await FastDFSClient.UploadFileAsync(storageNode, objectStream, Path.GetExtension(fileName));
            return storageNode.GroupName + "/" + filePath;
        }

        /// <summary>
        /// 文件下载
        /// </summary>
        /// <param name="fileName">下载文件的名称</param>
        /// <returns>返回下载文件对应的字节数组</returns>
        public async Task<byte[]> DownloadObjectByteAsync(string fileName)
        {
            var storageNode = await FastDFSClient.GetStorageNodeAsync();
            byte[] fileContent = await FastDFSClient.DownloadFileAsync(storageNode, fileName);

            return fileContent;
        }

        /// <summary>
        /// 查看文件属性
        /// </summary>
        /// <param name="fileName">查看的文件的名称</param>
        /// <returns>返回查询文件的文件信息</returns>
        public async Task<FDFSFileInfo> ViewObjectByteAsync(string fileName)
        {
            var storageNode = await FastDFSClient.GetStorageNodeAsync();
            var fileInfo = await FastDFSClient.GetFileInfoAsync(storageNode, fileName);

            return fileInfo;
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="fileName">删除的文件的名称</param>
        /// <returns>返回查询文件的文件信息</returns>
        public async Task<bool> DeleteObjectByteAsync(string fileName)
        {
            var storageNode = await FastDFSClient.GetStorageNodeAsync();
            try
            {
                await FastDFSClient.RemoveFileAsync(storageNode.GroupName, fileName);
                return true;
            }
            catch (System.Exception)
            {
                return false;
            }

        }

        /// <summary>
        /// 解码前端进行Html加码的方法
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public string HtmlDecode(string filename)
        {
            return HttpUtility.UrlDecode(filename);
        }
    }
}
