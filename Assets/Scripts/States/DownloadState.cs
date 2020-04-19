using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

public class DownloadState : IState
{
    private readonly DownloadPresenter downloadPresenter;
    private ServerConfiguration serverConfiguration;
    private const int maxConcurrentDownloads = 4;
    private int concurrentDownloadCounter = 0;
    private int numberOfFilesDownloaded = 0;
    private int numberOfFilesToDownload = 0;

    private Coroutine downloadCoroutine;
    
    public DownloadState(DownloadPresenter downloadPresenter)
    {
        this.downloadPresenter = downloadPresenter;
        downloadPresenter.backButtonPressed += OnBackButtonPressed;
    }

    private void OnBackButtonPressed()
    {
        StateManager.GoToState<ServerConfigurationState>();
    }

    public void Enter()
    {
        serverConfiguration = ServerConfigurationModel.ActiveConfiguration;
        if (serverConfiguration.AllFilesDownloaded || Application.isEditor)
        {
            StateManager.GoToState<GameState>();
        }
        else
        {
            downloadPresenter.gameObject.SetActive(true);
            //Get list of files to download from server
            /*var uriBuilder = new UriBuilder("http",serverConfiguration.FileDownloadServerUrl,8080);
            var request = UnityWebRequest.Get(uriBuilder.Uri);
            request.SendWebRequest().completed += operation =>
            {
                if (request.isHttpError || request.isNetworkError)
                {
                    var error = $"Error while getting list of files from server: {request.error}";
                    Debug.LogError(error);
                    downloadPresenter.ShowError(error);
                    return;
                }
                string hRefPattern = @"href\s*=\s*(?:[""'](?<1>[^""']*)[""']|(?<1>\S+))";
                var filesList = new List<string>(Regex.Matches(request.downloadHandler.text, hRefPattern, RegexOptions.IgnoreCase).
                    Cast<Match>().Select(match => match.Groups[1].Value)
                    .Where(text => text.Contains("."))
                    .Where(text => text.Contains(".exe") == false));
                numberOfFilesToDownload = filesList.Count;
                downloadCoroutine = downloadPresenter.StartCoroutine(DownloadFiles(filesList));
            };*/
            numberOfFilesToDownload = Files.Length;
            downloadCoroutine = downloadPresenter.StartCoroutine(DownloadFiles(Files.ToList()));

        }
    }
        private static string[] Files = new[] {"Anim1.def","Anim2.def", "mobtypes.txt", "unifont1.mul","unifont2.mul","unifont3.mul","unifont4.mul","unifont5.mul","unifont6.mul","unifont7.mul","unifont8.mul","unifont9.mul","unifont10.mul","unifont11.mul","unifont12.mul","anim.idx","map0.mul","mapdif0.mul", "mapdifl0.mul","cliloc.enu","statics0.mul","staidx0.mul", "anim.mul",/*"sound.mul","soundidx.mul", */"light.mul","lightidx.mul", "speech.mul", "unifont.mul", "texidx.mul", "texmaps.mul", "multi.mul","multi.idx","tiledata.mul","radarcol.mul","hues.mul","fonts.mul", "stadifl0.mul", "stadifi0.mul", "stadif0.mul","gumpart.mul", "gumpidx.mul", "art.mul","artidx.mul"};

    private IEnumerator DownloadFiles(List<string> filesList)
    {
        int index = 0;

        var pathToSaveFiles = serverConfiguration.GetPathToSaveFiles();
        var directoryInfo = new DirectoryInfo(pathToSaveFiles);
        if (directoryInfo.Exists == false)
        {
            directoryInfo.Create();
        }

        while (index < filesList.Count)
        {
            while (concurrentDownloadCounter < maxConcurrentDownloads && index < filesList.Count)
            {
                var fileName = filesList[index++];
                var uriBuilder = new UriBuilder("http",serverConfiguration.FileDownloadServerUrl,8080, fileName);
                var request = UnityWebRequest.Get(uriBuilder.Uri);
                var filePath = Path.Combine(pathToSaveFiles, fileName);
                var fileDownloadHandler = new DownloadHandlerFile(filePath);
                fileDownloadHandler.removeFileOnAbort = true;
                request.downloadHandler = fileDownloadHandler;
                request.SendWebRequest().completed += operation => SingleFileDownloadFinished(request, fileName);
                ++concurrentDownloadCounter;
            }

            yield return new WaitUntil(() => concurrentDownloadCounter < maxConcurrentDownloads);
        }

        //Wait until final downloads finish
        yield return new WaitUntil(() => concurrentDownloadCounter == 0);

        serverConfiguration.AllFilesDownloaded = true;
        ServerConfigurationModel.SaveServerConfigurations();
        
        SyncFiles();
        StateManager.GoToState<GameState>();
    }

    private void SingleFileDownloadFinished(UnityWebRequest request, string fileName)
    {
        if (request.isHttpError || request.isNetworkError)
        {
            var error = $"Error while downloading {fileName}: {request.error}";
            Debug.LogError(error);
            downloadPresenter.ShowError(error);
            //Stop downloads
            downloadPresenter.StopCoroutine(downloadCoroutine);
            return;
        }
        Debug.Log($"Download finished - {fileName}");
        --concurrentDownloadCounter;
        ++numberOfFilesDownloaded;
        downloadPresenter.UpdateCounter(numberOfFilesDownloaded, numberOfFilesToDownload);
    }
    
    [DllImport("__Internal")]
    private static extern void SyncFiles();
    
    public void Exit()
    {
        downloadPresenter.gameObject.SetActive(false);
    }
}