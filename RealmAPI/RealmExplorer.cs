﻿using System;
using System.Linq;
using System.Text;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace RealmAPI
{
    public class RealmExplorer : IRealmExplorer<Realm>
    {
        private const string baseRealmAPIurl = "http://{0}.battle.net/api/wow/realm/status{1}";

        public Realm GetSingleRealm(string name)
        {
            var realmList = GetMultipleRealms(name);

            return realmList == null ? null : realmList.FirstOrDefault();
        }

        public IEnumerable<Realm> GetAllRealms()
        {
            return GetRealmData(string.Format(baseRealmAPIurl, Region, string.Empty));
        }

        public IEnumerable<Realm> GetRealmsByType(string type)
        {
            var realmList = GetRealmData(string.Format(baseRealmAPIurl, Region, string.Empty));

            return realmList
                .Where(r => r.type.Equals(type, StringComparison.InvariantCultureIgnoreCase));
        }

        public IEnumerable<Realm> GetRealmsByPopulation(string population)
        {
            var realmList = GetRealmData(string.Format(baseRealmAPIurl, Region, string.Empty));
            return realmList
                .Where(r => r.population.Equals(population, StringComparison.InvariantCultureIgnoreCase));
        }

        public IEnumerable<Realm> GetRealmsByStatus(bool status)
        {
            var realmList = GetRealmData(string.Format(baseRealmAPIurl, Region, string.Empty));
            return realmList
                .Where(r => r.status == status);
        }

        public IEnumerable<Realm> GetRealmsByQueue(bool queue)
        {
            var realmList = GetRealmData(string.Format(baseRealmAPIurl, Region, string.Empty));
            return realmList
                .Where(r => r.queue == queue);
        }

        public IEnumerable<Realm> GetMultipleRealms(params string[] names)
        {
            var realmList = Enumerable.Empty<Realm>();
            if (names == null 
                || names.Length == 0
                || names.Any(r => r == null)) return realmList;

            var query = "?realm=" + names[0];
            for (int i = 1; i < names.Length; i++)
            {
                query += "&realm=" + names[i];
            }

            realmList = GetMultipleRealmsViaQuery(query);
            return realmList;
        }

        public IEnumerable<Realm> GetMultipleRealmsViaQuery(string query)
        {
            if (string.IsNullOrEmpty(query)) return null;

            try
            {
                return GetRealmData(string.Format(baseRealmAPIurl, Region, query));
            }
            catch
            {
                return null;
            }
        }

        public string GetRealmsByTypeAsJson(string type)
        {
            return ConvertRealmListToJson(GetRealmsByType(type));
        }

        public string GetRealmsByPopulationAsJson(string population)
        {
            return ConvertRealmListToJson(GetRealmsByPopulation(population));
        }

        public string GetRealmsByStatusAsJson(bool status)
        {
            return ConvertRealmListToJson(GetRealmsByStatus(status));
        }

        public string GetRealmsByQueueAsJson(bool queue)
        {
            return ConvertRealmListToJson(GetRealmsByQueue(queue));
        }

        public string GetAllRealmsAsJson()
        {
            return GetJson(string.Format(baseRealmAPIurl, Region, string.Empty));
        }

        public string GetSingleRealmAsJson(string name)
        {
            return ConvertRealmListToJson(GetMultipleRealms(name));
        }

        public string GetMultipleRealmsAsJson(params string[] mames)
        {
            return ConvertRealmListToJson(GetMultipleRealms(mames));
        }
            
        public string GetRealmsViaQueryAsJson(string query)
        {
            return GetJson(string.Format(baseRealmAPIurl, Region, query));
        }

        public string Region { get; set; }

        public RealmExplorer() : this("us") { }

        public RealmExplorer(string region)
        {
            this.Region = region;
        }

        private string ConvertRealmListToJson(IEnumerable<Realm> realmList)
        {
            return JsonConvert.SerializeObject(
                new Dictionary<string, IEnumerable<Realm>> { { "realms", realmList } });
        }

        private IEnumerable<Realm> GetRealmData(string url)
        {
            var jsonObject = JObject.Parse(GetJson(url));
            var realms = jsonObject["realms"];

            return JsonConvert.DeserializeObject<IEnumerable<Realm>>(realms.ToString());
        }

        private string GetJson(string url)
        {
            using (var wc = new WebClient())
            {
                var jsonString = wc.DownloadString(SanitizeUrl(url));
                return jsonString;
            }
        }

        //Todo: Improve URL sanitizer
        //http://stackoverflow.com/questions/25259/how-do-you-include-a-webpage-title-as-part-of-a-webpage-url/25486#25486
        private string SanitizeUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) return "";

            url = Regex.Replace(url.Trim(), @"\s+", "-");
            url = Regex.Replace(url, "[#']", "");

            return url;
        }
    }
}
