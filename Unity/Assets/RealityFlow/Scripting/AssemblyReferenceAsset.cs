using System;
using UnityEngine;
using Microsoft.CodeAnalysis;
using System.IO;

namespace RealityFlow.Scripting
{
    [CreateAssetMenu(fileName = "Assembly Reference Asset", menuName = "Assembly Reference Asset")]
    public class AssemblyReferenceAsset : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField, HideInInspector]
        private string assemblyName = "";

        [SerializeField, HideInInspector]
        private string assemblyPath = "";

        [SerializeField, HideInInspector]
        private byte[] assemblyImage = null;

        [SerializeField, HideInInspector]
        private long lastWriteTimeTicks = 0;

        private DateTime lastWriteTime = DateTime.Now;

        public MetadataReference CompilerReference
        {
            get { return GetReferences(); }
        }

        public string AssemblyName
        {
            get { return assemblyName; }
        }

        public string AssemblyPath
        {
            get { return assemblyPath; }
        }

        public byte[] AssemblyImage
        {
            get { return assemblyImage; }
        }

        public DateTime LastWriteTime
        {
            get { return lastWriteTime; }
        }

        public bool IsValid
        {
            get { return assemblyImage != null && assemblyImage.Length > 0; }
        }

        public void UpdateAssemblyReference(string referencePath, string assemblyName)
        {
            if (referencePath == null) throw new ArgumentNullException(nameof(referencePath));
            if (referencePath == string.Empty) throw new ArgumentException("Path cannot be empty");
            if (assemblyName == null) throw new ArgumentNullException(nameof(assemblyName));

            this.assemblyName = "";
            this.assemblyPath = "";
            this.assemblyImage = new byte[0];

            if (File.Exists(referencePath) == true)
            {
                this.assemblyName = assemblyName;
                this.assemblyPath = referencePath;
                this.assemblyImage = File.ReadAllBytes(referencePath);
                this.lastWriteTime = File.GetLastWriteTime(referencePath);
            }
        }

        void UpdateIfOutdated()
        {
            if (File.Exists(assemblyPath) == true)
            {
                DateTime lastTime = File.GetLastWriteTime(assemblyPath);

                if (lastTime > lastWriteTime)
                {
                    UpdateAssemblyReference(assemblyPath, assemblyName);
                }
            }
        }
        
        public override string ToString()
        {
            string asmName = assemblyName;

            if (string.IsNullOrEmpty(asmName) == true)
                asmName = "<Invalid Assembly>";

            return string.Format("{0}({1})", nameof(AssemblyReferenceAsset), asmName);
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            UpdateIfOutdated();

            lastWriteTimeTicks = lastWriteTime.Ticks;
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            lastWriteTime = new DateTime(lastWriteTimeTicks);

            UpdateIfOutdated();
        }

        private MetadataReference GetReferences()
        {
            return MetadataReference.CreateFromImage(assemblyImage);
        }
    }
}