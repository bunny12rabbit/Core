using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UniRx;
using UnityEngine;

namespace Common.Core
{
    [Serializable]
    public sealed class SavableValue<T>
    {
        private readonly string _playerPrefsPath;

        private T _value;

        private readonly ReactiveCommand<(T, T)> _changed = new ReactiveCommand<(T, T)>();

        public IObservable<(T, T)> Changed => _changed;

        public T Value
        {
            get => _value;

            set
            {
                PrevValue = _value;
                _value = value;
                SaveToPrefs();
                _changed.Execute((PrevValue, value));
            }
        }

        public T PrevValue { get; private set; }

        public SavableValue(string playerPrefsPath, T defaultValue = default)
        {
            if (string.IsNullOrEmpty(playerPrefsPath))
                throw new Exception("Empty playerPrefsPath in savableValue!");

            _playerPrefsPath = playerPrefsPath;
            _value = defaultValue;
            PrevValue = defaultValue;

            LoadFromPrefs();
        }

        private void LoadFromPrefs()
        {
            if (!PlayerPrefs.HasKey(_playerPrefsPath))
            {
                SaveToPrefs();
                return;
            }

            var stringToDeserialize = PlayerPrefs.GetString(_playerPrefsPath, string.Empty);
            var bytes = Convert.FromBase64String(stringToDeserialize);
            var binaryFormatter = new BinaryFormatter();

            using var memoryStream = new MemoryStream(bytes);
            _value = (T)binaryFormatter.Deserialize(memoryStream);
        }

        private void SaveToPrefs()
        {
            var binaryFormatter = new BinaryFormatter();

            using var memoryStream = new MemoryStream();

            binaryFormatter.Serialize(memoryStream, _value);

            var stringToSave = Convert.ToBase64String(memoryStream.ToArray());
            PlayerPrefs.SetString(_playerPrefsPath, stringToSave);
        }
    }
}