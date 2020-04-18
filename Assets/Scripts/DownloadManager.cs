
using System;
using System.IO;
using System.Threading;
using ClassicUO.IO;
using ClassicUO.Utility.Logging;
using UnityEngine;
using UnityEngine.Networking;

namespace ClassicUO
{
    public  class DownloadManager
    {
        private static string[] Files = new[] {/*"anim.idx","map0.mul","mapdif0.mul","cliloc.enu","statics0.mul","staidx0.mul", "anim.mul","sound.mul","soundidx.mul", */"light.mul","lightidx.mul", "speech.mul", "unifont.mul", "texidx.mul", "texmaps.mul", "multi.mul","multi.idx","tiledata.mul","radarcol.mul","hues.mul","fonts.mul",  "gumpart.mul", "gumpidx.mul", "art.mul","artidx.mul"};
        private static int _downLoading = 0;
        public static void DownloadFiles()
        {
            return;
            foreach (var fileName in Files)
            {
                if (File.Exists(Path.Combine(Application.persistentDataPath, fileName)))
                    continue;
                Log.Error( $"{fileName} Downloading...");
                    var uriBuilder = new UriBuilder("http",ServerConfigurationModel.ActiveConfiguration.FileDownloadServerUrl,2595, fileName);
                    var request = UnityWebRequest.Get(uriBuilder.Uri);
                    var fileDownloadHandler = new DownloadHandlerFile(Path.Combine(Application.persistentDataPath,fileName));
                    fileDownloadHandler.removeFileOnAbort = true;
                    request.downloadHandler = fileDownloadHandler;
                    request.SendWebRequest().completed += operation => SingleFileDownloadFinished(request, fileName);
                    _downLoading++;

            }
            
        }
        
        
        private static void SingleFileDownloadFinished(UnityWebRequest request, string fileName)
        {
            if (request.isHttpError || request.isNetworkError)
            {
                var error = $"Error while downloading {fileName}: {request.error}";
                Debug.LogError(error);
                _downLoading--;
                return;
            }
            Debug.Log($"Download finished - {fileName}");
            _downLoading--;
        }
    }
}