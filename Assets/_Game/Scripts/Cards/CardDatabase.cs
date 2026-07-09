using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Lionrise
{
    public sealed class CardDatabase
    {
        private readonly List<CardDef> cards = new List<CardDef>();

        public IReadOnlyList<CardDef> Cards => cards;
        public int Count => cards.Count;

        public IEnumerator Load(Action onLoaded, Action<string> onError)
        {
            var path = Path.Combine(Application.streamingAssetsPath, "Cards", "cards.json");
            string json;

            if (path.Contains("://") || path.Contains(":///"))
            {
                using (var request = UnityWebRequest.Get(path))
                {
                    yield return request.SendWebRequest();
                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        onError?.Invoke($"Could not load card data: {request.error}");
                        yield break;
                    }
                    json = request.downloadHandler.text;
                }
            }
            else
            {
                try { json = File.ReadAllText(path); }
                catch (Exception exception)
                {
                    onError?.Invoke($"Could not load card data at {path}: {exception.Message}");
                    yield break;
                }
            }

            CardCollection collection;
            try { collection = JsonUtility.FromJson<CardCollection>(json); }
            catch (Exception exception)
            {
                onError?.Invoke($"Card JSON is invalid: {exception.Message}");
                yield break;
            }

            cards.Clear();
            if (collection?.cards != null) cards.AddRange(collection.cards.Where(card => card != null));
            if (cards.Count == 0)
            {
                onError?.Invoke("Card database is empty.");
                yield break;
            }

            onLoaded?.Invoke();
        }

        public CardDef Find(string id) => cards.FirstOrDefault(card => card.id == id);
    }
}

