using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;

namespace GetMentality
{
    /// <summary>Plays the opening movies before revealing the room and its HUD.</summary>
    public sealed class IntroSequenceController : MonoBehaviour
    {
        [SerializeField] private GameObject roomUI;
        [SerializeField] private string[] videoFiles = { "Intro/Building.mp4", "Intro/WakingUp.mp4" };
        [SerializeField, Min(1)] private float prepareTimeout = 30f;

        private VideoPlayer player;
        private UIDocument document;
        private PanelSettings panelSettings;
        private RenderTexture videoTexture;
        private Image videoImage;
        private bool finished, failed;

        private void Awake()
        {
            if (roomUI != null) roomUI.SetActive(false);
        }

        private IEnumerator Start()
        {
            if (roomUI == null)
            {
                Debug.LogError("Intro sequence needs a Room UI object.", this);
                yield break;
            }

            panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
            panelSettings.themeStyleSheet = Resources.Load<ThemeStyleSheet>("RoomTheme");
            panelSettings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            panelSettings.referenceResolution = new Vector2Int(1280, 720);
            panelSettings.sortingOrder = 100;
            document = gameObject.AddComponent<UIDocument>();
            document.panelSettings = panelSettings;
            var root = document.rootVisualElement;
            root.style.flexGrow = 1;
            root.style.backgroundColor = Color.black;
            videoImage = new Image { scaleMode = ScaleMode.ScaleToFit };
            videoImage.style.position = Position.Absolute;
            videoImage.style.left = videoImage.style.right = 0;
            videoImage.style.top = videoImage.style.bottom = 0;
            root.Add(videoImage);

            player = gameObject.AddComponent<VideoPlayer>();
            player.playOnAwake = false;
            player.isLooping = false;
            player.source = VideoSource.Url;
            player.renderMode = VideoRenderMode.RenderTexture;
            player.audioOutputMode = VideoAudioOutputMode.Direct;
            player.loopPointReached += OnFinished;
            player.errorReceived += OnError;

            foreach (var file in videoFiles)
            {
                finished = failed = false;
                // StreamingAssets keeps the original movie and its embedded audio in builds.
                player.url = Application.streamingAssetsPath.Replace('\\', '/') + "/" + file;
                player.Prepare();
                var deadline = Time.realtimeSinceStartup + prepareTimeout;
                while (!player.isPrepared && !failed && Time.realtimeSinceStartup < deadline)
                    yield return null;

                if (!player.isPrepared || failed)
                {
                    if (!failed) Debug.LogError($"Intro video preparation timed out: {file}", this);
                    player.Stop();
                    continue;
                }

                ReleaseTexture();
                videoTexture = new RenderTexture(Math.Max(1, (int)player.width),
                    Math.Max(1, (int)player.height), 0, RenderTextureFormat.ARGB32);
                videoTexture.Create();
                player.targetTexture = videoTexture;
                videoImage.image = videoTexture;
                player.Play();
                while (!finished && !failed) yield return null;
                // Freeze the final frame while preparing the next movie.
                player.Stop();
            }

            roomUI.SetActive(true);
            document.enabled = false;
            ReleaseTexture();
        }

        private void OnFinished(VideoPlayer source) => finished = true;

        private void OnError(VideoPlayer source, string message)
        {
            failed = true;
            Debug.LogError($"Intro video failed ({source.url}): {message}", this);
        }

        private void ReleaseTexture()
        {
            if (player != null) player.targetTexture = null;
            if (videoImage != null) videoImage.image = null;
            if (videoTexture == null) return;
            videoTexture.Release();
            Destroy(videoTexture);
            videoTexture = null;
        }

        private void OnDestroy()
        {
            if (player != null)
            {
                player.loopPointReached -= OnFinished;
                player.errorReceived -= OnError;
                player.Stop();
            }
            if (document != null) document.enabled = false;
            ReleaseTexture();
            if (panelSettings != null) Destroy(panelSettings);
        }
    }
}
