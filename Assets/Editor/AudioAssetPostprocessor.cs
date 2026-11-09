//  AudioAssetPostprocessor.cs
//  VoiceParty
//
//  Created by henryy on 12/01/2015 10:57:05.

using UnityEngine;
using System.Collections;
using UnityEditor;

namespace ProGamer
{
    public class AudioAssetPostprocessor : AssetPostprocessor
    {
        public void OnPreprocessAudio()
        {
            (assetImporter as AudioImporter).loadInBackground = true;
            AudioImporterSampleSettings settings = new AudioImporterSampleSettings();
            settings.loadType = AudioClipLoadType.CompressedInMemory;
            settings.quality = 0.01f;
            settings.compressionFormat = AudioCompressionFormat.Vorbis;
            settings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
            settings.sampleRateOverride = 0;
            (assetImporter as AudioImporter).defaultSampleSettings = settings;
        }
    }
}