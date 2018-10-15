using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace ShiningHill
{
    public enum SHGame
    {
        SH1,
        SH2PC,
        SH3PC,
        SH3PCdemo,
        SH4PC
    }

    public struct TexAssetPaths
    {
        string filename;
        string genericPath;
        SHGame game;
        bool forMap;

        public TexAssetPaths(string hardAssetPath, SHGame forgame)
        {
            forMap = hardAssetPath.Contains("TR.tex") || hardAssetPath.Contains("GB.tex");
            filename = Path.GetFileNameWithoutExtension(hardAssetPath);
            genericPath = Path.GetDirectoryName(hardAssetPath).Substring(hardAssetPath.LastIndexOf("/data/data/") + 1).Replace("\\", "/") + "/";
            game = forgame;
        }

        public string GetTextureName()
        {
            return filename + "_tex";
        }

        public string GetHardAssetPath()
        {
            string path = CustomPostprocessor.GetHardDataPathFor(game);
            return path + genericPath + filename + ".tex";
        }

        public string GetExtractAssetPath()
        {
            string path = CustomPostprocessor.GetExtractDataPathFor(game);
            return path + genericPath + filename + ".asset";
        }
    }

    public class CustomPostprocessor : AssetPostprocessor
    {
        public const string SH1DataPath = "Assets/sh1/";
        public const string SH2PCDataPath = "Assets/sh2pc/";
        public const string SH3PCDataPath = "Assets/sh3pc/";
        public const string SH3PCdemoDataPath = "Assets/sh3pc_demo/";
        public const string SH4PCDataPath = "Assets/sh4pc/";

        public const string SH1ExtractPath = "Assets/Resources/sh1/";
        public const string SH2PCExtractPath = "Assets/Resources/sh2pc/";
        public const string SH3PCExtractPath = "Assets/Resources/sh3pc/";
        public const string SH3PCdemoExtractPath = "Assets/Resources/sh3pc_demo/";
        public const string SH4PCExtractPath = "Assets/Resources/sh4pc/";

        public static string GetHardDataPathFor(SHGame game)
        {
            if (game == SHGame.SH1) return CustomPostprocessor.SH1DataPath;
            if (game == SHGame.SH2PC) return CustomPostprocessor.SH2PCDataPath;
            if (game == SHGame.SH3PC) return CustomPostprocessor.SH3PCDataPath;
            if (game == SHGame.SH3PCdemo) return CustomPostprocessor.SH3PCdemoDataPath;
            if (game == SHGame.SH4PC) return CustomPostprocessor.SH4PCDataPath;
            return null;
        }

        public static string GetExtractDataPathFor(SHGame game)
        {
            if (game == SHGame.SH1) return CustomPostprocessor.SH1ExtractPath;
            if (game == SHGame.SH2PC) return CustomPostprocessor.SH2PCExtractPath;
            if (game == SHGame.SH3PC) return CustomPostprocessor.SH3PCExtractPath;
            if (game == SHGame.SH3PCdemo) return CustomPostprocessor.SH3PCdemoExtractPath;
            if (game == SHGame.SH4PC) return CustomPostprocessor.SH4PCExtractPath;
            return null;
        }

        public static bool DoImports = true;

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromPath)
        {
            if (DoImports)
            {
                foreach (string asset in importedAssets)
                {
                    try
                    {
                        string extension = Path.GetExtension(asset);
                        SHGame forGame = SHGame.SH3PC;
                        if (asset.Contains("sh1")) forGame = SHGame.SH1;
                        else if(asset.Contains("sh2pc")) forGame = SHGame.SH2PC;
                        else if (asset.Contains("sh3pc_demo")) forGame = SHGame.SH3PCdemo;
                        else if (asset.Contains("sh3pc")) forGame = SHGame.SH3PC;
                        else if (asset.Contains("sh4pc")) forGame = SHGame.SH4PC;
                        else continue;

                        //Debug.Log("Loading " + asset);

                        if (extension == ".tex") { ProcessTEX(new TexAssetPaths(asset, forGame)); continue; }
                        if (extension == ".map") { Map.ReadMap(new MapAssetPaths(asset, forGame)); continue; }
                        if (extension == ".cld") { MapCollisions.ReadCollisions(new MapCollisionsAssetPaths(asset, forGame)); continue; }
                        if (extension == ".kg2") { MapShadows.ReadShadowCasters(new MapShadowsAssetPaths(asset, forGame)); continue; }
                        if (extension == ".ded") { MapLights.ReadLights(new MapLightsAssetPaths(asset, forGame)); continue; }
                        if (extension == ".cam") { MapCameras.ReadCameras(new MapCamerasAssetPaths(asset, forGame)); continue; }
                        if (extension == ".afs") { AFSReader.ReadAFSFiles(new AFSAssetPaths(asset, forGame)); continue; }
                        if (extension == ".mdl") { Model.LoadModel(new ModelAssetPaths(asset, forGame)); continue; }
                    }
                    catch(Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
                AssetDatabase.SaveAssets();
            }
        }

        public static MaterialRolodex ProcessTEX(TexAssetPaths paths)
        {
            if (File.Exists(paths.GetHardAssetPath()))
            {
                Texture2D[] textures = TextureUtils.ReadTex32(paths.GetTextureName(), new BinaryReader(new FileStream(paths.GetHardAssetPath(), FileMode.Open, FileAccess.Read, FileShare.Read)));
                
                MaterialRolodex rolodex = MaterialRolodex.GetOrCreateAt(paths.GetExtractAssetPath());
                rolodex.AddTextures(textures);
                return rolodex;
            }
            return null;
        }
    }
}