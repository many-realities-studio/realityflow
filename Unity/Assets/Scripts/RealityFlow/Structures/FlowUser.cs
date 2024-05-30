using Newtonsoft.Json;
using System;
using UnityEngine;

namespace RealityFlow.Plugin.Scripts
{
    [Serializable]
    public class FlowUser
    {
        [SerializeField]
        private string _username;

        [SerializeField]
        private string _password;

        [JsonProperty("Username")]
        public string Username { get => _username; set => _username = value; }

        [JsonProperty("Password")]
        public string Password { get => _password; set => _password = value; }

        public FlowUser(string username, string password)
        {
            this.Username = username;
            this.Password = password;
        }
    }
}