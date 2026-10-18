using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

// ReSharper disable InconsistentNaming

namespace LD60.Web
{
    public static class ScoresWeb
    {
        private const string url = "https://nathanistace.be/gamejams/ld60/scores.php";

        public static async UniTask<Dictionary<string, (string name, int time)>> Get(CancellationToken cancellationToken = default)
        {
            try
            {
                var request = UnityWebRequest.Get(url);

                await request.SendWebRequest().ToUniTask(cancellationToken: cancellationToken);

                if (request.result == UnityWebRequest.Result.Success)
                {
                    var responseData = JsonUtility.FromJson<GetScoresResponse>(request.downloadHandler.text.TrimStart('\uFEFF'));

                    return responseData.scores.ToDictionary(t => t.level, t => (t.user_name, t.time));
                }

                Debug.LogError(request.error);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return new Dictionary<string, (string name, int time)>();
        }

        public static async UniTask<(bool success, bool newBest)> Post(ScoreEntry entry, CancellationToken cancellationToken = default)
        {
            try
            {
                using var request = new UnityWebRequest(url, "POST");

                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonUtility.ToJson(entry)));
                request.downloadHandler = new DownloadHandlerBuffer();

                request.SetRequestHeader("Content-Type", "application/json");

                await request.SendWebRequest().ToUniTask(cancellationToken: cancellationToken);

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Failed to submit score: {request.error}");
                    return (false, false);
                }

                var responseData = JsonUtility.FromJson<PostResult>(request.downloadHandler.text.TrimStart('\uFEFF'));
                return (true, responseData.best);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return (false, false);
            }
        }

        [Serializable]
        public class PostResult
        {
            public bool best;
        }

        [Serializable]
        public class ScoreEntry
        {
            public string user_name;
            public string level;
            public int time;
        }

        [Serializable]
        public class GetScoresResponse
        {
            public ScoreEntry[] scores;
        }
    }
}